using LightSwitchApplication.Implementation;
using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Linq;
using System.ServiceModel.DomainServices.Server;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CallCenterManager.ReportService {
    public class ReportData : DomainService {
        private ApplicationData _context;
        public ApplicationData Context {
            get {
                if (_context == null) {
                    string connString = WebConfigurationManager.ConnectionStrings["_IntrinsicData"].ConnectionString;
                    EntityConnectionStringBuilder builder = new EntityConnectionStringBuilder();
                    builder.Metadata = "res://*/ApplicationData.csdl|res://*/ApplicationData.ssdl|res://*/ApplicationData.msl";
                    builder.Provider = "System.Data.SqlClient";
                    builder.ProviderConnectionString = connString;
                    _context = new ApplicationData(builder.ConnectionString);
                }
                return _context;
            }
        }

        [Query(IsDefault = true)]
        public IQueryable<StatusPorAgente> GetStatusPorAgente() {
            List<StatusPorAgente> result = new List<StatusPorAgente>();
            var agenteServicio = this.Context.AgentesServicios.Include("Extension").Include("Agente").ToList();
            if (agenteServicio != null) {
                foreach (var item in agenteServicio) {
                    var bitacoraDeExtension = this.Context.BitacoraStatusDeExtensions
                        .Include("Extension").Include("Extension.StatusDeExtesion").Include("MotivoDeStatusInicial")
                        .Where(i => i.Extension.Id == item.Extension.Id).ToList();
                    if (bitacoraDeExtension != null) {
                        result.AddRange(bitacoraDeExtension.Select(bitacora => new StatusPorAgente {
                            ID = bitacora.Id,
                            IdAgente = item.Agente.Id,
                            Agente = string.Format("{0} {1}", item.Agente.Nombre, item.Agente.Apellido),
                            Status = bitacora.Extension.StatusDeExtesion.Descripcion,
                            HoraInicio = bitacora.Inicio.ToShortTimeString(),
                            Tiempo = bitacora.Fin == null ? "" : ((TimeSpan)(bitacora.Fin - bitacora.Inicio)).ToString(@"hh\:mm\:ss"),
                            TiempoAsInt = GetTiempoSegundos(bitacora), //bitacora.Inicio == null ? 0 : ((TimeSpan)(bitacora.Fin - bitacora.Inicio)).Seconds,
                            Motivo = bitacora.MotivoDeStatusInicial.Descripcion,
                            FechaDesde = bitacora.Inicio,
                            FechaHasta = bitacora.Fin,
                        }));
                    }
                }
            }
            return result.AsQueryable<StatusPorAgente>();
        }

        [Query(IsDefault = true)]
        public IQueryable<StatusPorAgenteGrouped> GetStatusPorAgenteGrouped() {
            List<StatusPorAgenteGrouped> result = new List<StatusPorAgenteGrouped>();
            var agenteServicio = this.Context.AgentesServicios.Include("Extension").Include("Extension.StatusDeExtesion").Include("Agente").ToList();
            if (agenteServicio != null) {
                foreach (var item in agenteServicio) {
                    foreach (var status in this.Context.StatusDeExtesiones) {
                        var bitacoras = this.Context.BitacoraStatusDeExtensions
                                .Include("Extension").Include("Extension.StatusDeExtesion").Include("MotivoDeStatusInicial")
                                .Where(i => i.StatusDeExtesionInicial.Id == status.Id &&
                                    i.Extension.AgenteServicios.Where(s => s.Agente.Id == item.Agente.Id).Count() > 0).ToList();
                        int tiempoStatus = 0;
                        DateTime inicio = new DateTime(1900, 1, 1);
                        DateTime fin = new DateTime(1900, 1, 1);
                        if (bitacoras != null && bitacoras.Count > 0) {
                            tiempoStatus = bitacoras.Select(GetTiempo).ToList().Sum();
                            inicio = bitacoras.Select(i => i.Inicio).Min();
                            fin = bitacoras.Select(i => i.Inicio).Max();
                        }
                        result.Add(new StatusPorAgenteGrouped {
                            ID = status.Id,
                            Agente = string.Format("{0} {1}", item.Agente.Nombre, item.Agente.Apellido),
                            Status = status.Descripcion,
                            TiempoAsInt = tiempoStatus,
                            FechaDesde = inicio,
                            FechaHasta = fin
                        }); 
                    }
                }
            }
            return result.AsQueryable<StatusPorAgenteGrouped>();
        }

        [Query(IsDefault = true)]
        public IQueryable<LlamadaPorAgente> GetLlamadasPorAgente() {
            List<LlamadaPorAgente> result = new List<LlamadaPorAgente>();
            var llamadas = this.Context.Llamadas.Include("Extension").Include("Extension.StatusDeExtesion").Include("ColaDeServicio").ToList();
            if (llamadas != null) {
                foreach (var llamada in llamadas) {
                    Agente agente = GetAgenteByExtensionId(llamada.Extension.Id);
                    if (agente != null) {
                        result.Add(new LlamadaPorAgente {
                            ID = llamada.Id,
                            IdAgente = agente.Id,
                            Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                            Status = llamada.Status,
                            Inicio = llamada.Inicio.ToShortTimeString(),
                            Fin = llamada.Fin.ToShortTimeString(),
                            Tiempo = llamada.Fin == null ? "" : ((TimeSpan)(llamada.Fin - llamada.Inicio)).ToString(@"hh\:mm\:ss"),
                            TiempoAsInt = llamada.Fin == null ? 0 : ((TimeSpan)(llamada.Fin - llamada.Inicio)).Seconds,
                            Numero = llamada.Numero,
                            Extension = llamada.Extension.Numero,
                            FechaDesde = llamada.Inicio,
                            FechaHasta = llamada.Fin,
                        }); 
                    }
                }
            }
            return result.AsQueryable<LlamadaPorAgente>();
        }

        [Query(IsDefault = true)]
        public IQueryable<LlamadaPorAgenteGrouped> GetLlamadasPorAgenteGrouped() {
            List<LlamadaPorAgenteGrouped> result = new List<LlamadaPorAgenteGrouped>();

            int id = 1;
            foreach (var status in new string[] { "Atendida", "Abandonada" }) {
                foreach (var agente in this.Context.Agentes) {
                    var llamadas = (from l in this.Context.Llamadas.Include("Extension").Include("Extension.Agente")
                                    join agenteServicio in this.Context.AgentesServicios on l.Extension.Id equals agenteServicio.Extension.Id
                                    where agenteServicio.Agente.Id == agente.Id && l.Status == status
                                    select l).ToList();

                    int totalLlamadas = 0;
                    DateTime inicio = new DateTime(1900, 1, 1);
                    DateTime fin = new DateTime(1900, 1, 1);
                    if (llamadas != null && llamadas.Count > 0) {
                        totalLlamadas = llamadas.Count;
                        inicio = llamadas.Select(i => i.Inicio).Min();
                        fin = llamadas.Select(i => i.Inicio).Max();
                    }
                    if (totalLlamadas > 0) {


                        result.Add(new LlamadaPorAgenteGrouped {
                            ID = id++,
                            Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                            Status = status,
                            TotalLlamadasAsInt = totalLlamadas,
                            FechaDesde = inicio,
                            FechaHasta = fin
                        }); 
                    }
                }
            }
            return result.AsQueryable<LlamadaPorAgenteGrouped>();
        }

        [Query(IsDefault = true)]
        public IQueryable<EstadisticoDeLlamadasPorAgente> GetEstadisticoDeLlamadasPorAgente() {
            List<EstadisticoDeLlamadasPorAgente> result = new List<EstadisticoDeLlamadasPorAgente>();
            int id = 1;
            foreach (var agente in this.Context.Agentes) {
                List<Llamada> llamadas = GetLlamadasDeAgente(agente.Id, "");
                if (llamadas != null && llamadas.Count > 0) {
                    DateTime inicio = llamadas.Select(i => i.Inicio).Min();
                    DateTime fin = llamadas.Select(i => i.Inicio).Max();
                    result.Add(new EstadisticoDeLlamadasPorAgente {
                        ID = id++,
                        Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                        Descripcion = "Total Llamadas Presentadas",
                        Value = llamadas.Count,
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorAgente {
                        ID = id++,
                        Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                        Descripcion = "Total Llamadas Atendida",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorAgente {
                        ID = id++,
                        Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                        Descripcion = "Total Llamadas Abandonada",
                        Value = llamadas.Where(i => i.Status == "Abandonada").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    }); 
                    result.Add(new EstadisticoDeLlamadasPorAgente {
                        ID = id++,
                        Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                        Descripcion = "Total Tiempo en Conversación",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorAgente {
                        ID = id++,
                        Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                        Descripcion = "Tiempo de Conversación Más Largo",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorAgente {
                        ID = id++,
                        Agente = string.Format("{0} {1}", agente.Nombre, agente.Apellido),
                        Descripcion = "Promedio Tiempo de Conversación",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                }
            }
            return result.AsQueryable<EstadisticoDeLlamadasPorAgente>();
        }

        [Query(IsDefault = true)]
        public IQueryable<EstadisticoDeLlamadasPorServicio> GetEstadisticoDeLlamadasPorServicio() {
            List<EstadisticoDeLlamadasPorServicio> result = new List<EstadisticoDeLlamadasPorServicio>();
            int id = 1;
            foreach (var cola in this.Context.ColaDeServicios) {
                List<Llamada> llamadas = GetLlamadasDeServicio(cola.Id, "");
                if (llamadas != null && llamadas.Count > 0) {
                    DateTime inicio = llamadas.Select(i => i.Inicio).Min();
                    DateTime fin = llamadas.Select(i => i.Inicio).Max();
                    result.Add(new EstadisticoDeLlamadasPorServicio {
                        ID = id++,
                        Servicio = cola.Nombre,
                        Descripcion = "Total Llamadas Presentadas",
                        Value = llamadas.Count,
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorServicio {
                        ID = id++,
                        Servicio = cola.Nombre,
                        Descripcion = "Total Llamadas Atendida",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorServicio {
                        ID = id++,
                        Servicio = cola.Nombre,
                        Descripcion = "Total Llamadas Abandonada",
                        Value = llamadas.Where(i => i.Status == "Abandonada").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorServicio {
                        ID = id++,
                        Servicio = cola.Nombre,
                        Descripcion = "Total Tiempo en Conversación",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorServicio {
                        ID = id++,
                        Servicio = cola.Nombre,
                        Descripcion = "Tiempo de Conversación Más Largo",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                    result.Add(new EstadisticoDeLlamadasPorServicio {
                        ID = id++,
                        Servicio = cola.Nombre,
                        Descripcion = "Promedio Tiempo de Conversación",
                        Value = llamadas.Where(i => i.Status == "Atendida").Count(),
                        FechaDesde = inicio,
                        FechaHasta = fin
                    });
                }
            }
            return result.AsQueryable<EstadisticoDeLlamadasPorServicio>();
        }

        [Query(IsDefault = true)]
        public IQueryable<ComparativoStatusDeAgente> GetComparativoStatusDeAgente() {
            List<ComparativoStatusDeAgente> result = new List<ComparativoStatusDeAgente>();
            
            var bitacoras = this.Context.BitacoraStatusDeExtensions
                .Include("Extension").Include("Extension.StatusDeExtesion").Include("MotivoDeStatusInicial");

            var agenteServicios = this.Context.AgentesServicios
                .Include("Extension").Include("Extension.StatusDeExtesion").Include("Agente").ToList();

            DateTime inicio = bitacoras.Select(i => i.Inicio).Min();
            DateTime fin = bitacoras.Select(i => i.Inicio).Max();
            int id = 1;
            foreach (var cola in this.Context.ColaDeServicios) {
                foreach (var agente in agenteServicios.Where(i => i.ColaDeServicio != null && i.ColaDeServicio.Id == cola.Id)) {
                    foreach (var status in this.Context.StatusDeExtesiones) {

                        int tiempo = TiemoDeAgenteEnStatus(bitacoras, agente.Agente.Id, status.Id);
                        result.Add(new ComparativoStatusDeAgente {
                            ID = id++,
                            Servicio = cola.Nombre,
                            Status = status.Descripcion,
                            Agente = string.Format("{0} {1}", agente.Agente.Nombre, agente.Agente.Apellido),
                            Tiempo = tiempo,
                            FechaDesde = inicio,
                            FechaHasta = fin
                        });
                    }
                }
            }
            return result.AsQueryable<ComparativoStatusDeAgente>();
        }


        private int TiemoDeAgenteEnStatus(System.Data.Objects.ObjectQuery<BitacoraStatusDeExtension> query, int idAgente, int idStatus) {
            var tiempoStatus = query.Where(i => 
                    i.StatusDeExtesionInicial.Id == idStatus && 
                    i.Extension.AgenteServicios.Where(s => s.Agente.Id == idAgente).Count() > 0)
                .Select(GetTiempo).ToList().Sum();
            return tiempoStatus;
        }

        private List<Llamada> GetLlamadasDeAgente(int idAgente, string statusLlamadas) {
            if (string.IsNullOrWhiteSpace(statusLlamadas)) {
                return (from l in this.Context.Llamadas.Include("Extension").Include("Extension.Agente")
                        join agenteServicio in this.Context.AgentesServicios on l.Extension.Id equals agenteServicio.Extension.Id
                        where agenteServicio.Agente.Id == idAgente
                        select l).ToList();
            } else {
                return (from l in this.Context.Llamadas.Include("Extension").Include("Extension.Agente")
                        join agenteServicio in this.Context.AgentesServicios on l.Extension.Id equals agenteServicio.Extension.Id
                        where agenteServicio.Agente.Id == idAgente && l.Status == statusLlamadas
                        select l).ToList();
            }
        }

        private List<Llamada> GetLlamadasDeServicio(int idServicio, string statusLlamadas) {
            if (string.IsNullOrWhiteSpace(statusLlamadas)) {
                return (from l in this.Context.Llamadas.Include("Extension").Include("Extension.Agente").Include("ColaDeServicio")
                        join agenteServicio in this.Context.AgentesServicios on l.Extension.Id equals agenteServicio.Extension.Id
                        where agenteServicio.ColaDeServicio.Id == idServicio
                        select l).ToList();
            } else {
                return (from l in this.Context.Llamadas.Include("Extension").Include("Extension.Agente").Include("ColaDeServicio")
                        join agenteServicio in this.Context.AgentesServicios on l.Extension.Id equals agenteServicio.Extension.Id
                        where agenteServicio.ColaDeServicio.Id == idServicio && l.Status == statusLlamadas
                        select l).ToList();
            }
        }

        private Agente GetAgenteByExtensionId(int idExtension) {
            var agenteServicio = this.Context.AgentesServicios.Include("Agente")
                .Where(i => i.Extension.Id == idExtension).FirstOrDefault();
            if (agenteServicio != null) {
                return agenteServicio.Agente;
            }
            return null;
        }

        private static int GetTiempo(BitacoraStatusDeExtension bitacora) {
            return bitacora.Fin == null ? 0 : ((TimeSpan)(bitacora.Fin.Value - bitacora.Inicio)).Minutes;
        }

        private static int GetTiempoLlamada(Llamada llamada) {
            return llamada.Fin == null ? 0 : ((TimeSpan)(llamada.Fin - llamada.Inicio)).Minutes;
        }
        
        private static int GetTiempoSegundos(BitacoraStatusDeExtension bitacora) {
            return bitacora.Fin == null ? 0 : ((TimeSpan)(bitacora.Fin.Value - bitacora.Inicio)).Seconds;
        }
        
        protected override int Count<T>(IQueryable<T> query) {
            return query.Count();
        }
    }
}
