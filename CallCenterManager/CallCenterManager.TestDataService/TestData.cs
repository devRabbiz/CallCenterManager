using LightSwitchApplication.Implementation;
using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Linq;
using System.ServiceModel.DomainServices.Server;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CallCenterManager.TestDataService
{
    public class TestData : DomainService {
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

        [Invoke]
        public void AddLlamada(string numero, DateTime inicio, DateTime fin, string status, string numeroExtension, string colaDeServicio) {
            var extension = Context.Extensiones.Where(i => i.Numero == numeroExtension).FirstOrDefault();
            var cola = Context.ColaDeServicios.Where(i => i.Nombre == colaDeServicio).FirstOrDefault();
            if (extension == null || cola == null) {
                return;
            }
            Llamada llamada = new Llamada {
                Numero = numero,
                Inicio = inicio,
                Fin = fin,
                Status = status,
                Extension = extension,
                ColaDeServicio = cola
            };
            Context.Llamadas.AddObject(llamada);
            Context.SaveChanges();
        }

        [Invoke]
        public void AddCambioStatus(string numeroExtension, string status, string motivo) {
            var extension = Context.Extensiones.Where(i => i.Numero == numeroExtension).FirstOrDefault();
            var statusDeExtension = Context.StatusDeExtesiones.Where(i => i.Descripcion == status).FirstOrDefault();
            var motivoDeStatus = Context.MotivosDeStatus.Where(i => i.Descripcion == motivo).FirstOrDefault();

            BitacoraStatusDeExtension bitacora = new BitacoraStatusDeExtension {
                Inicio = DateTime.Now,
                Extension = extension,
                StatusDeExtesionInicial = statusDeExtension,
                MotivoDeStatusInicial = motivoDeStatus,
            };
            Context.BitacoraStatusDeExtensions.AddObject(bitacora);
            Context.SaveChanges();
        }
    }
}
