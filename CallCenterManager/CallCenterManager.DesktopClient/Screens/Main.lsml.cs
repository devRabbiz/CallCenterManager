using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using Microsoft.LightSwitch.Threading;

namespace LightSwitchApplication {
    public partial class Main {
        private DispatcherTimer _timer;

        partial void Main_InitializeDataWorkspace(List<IDataService> saveChangesTo) {
            ImageLogo = GetImageByName("LightSwitchApplication.Images.logo.png");
            Titulo = "Call Voice Management System";
            Subtitulo = "Plataforma VoIP";
            Descripcion = "Ésta aplicación permite monitorear en tiempo real las colas de llamadas y las actividades de los agentes de la Cooperativa COSIT, generando informes estadísticos de las llamadas y las actividades de los agentes basados en la información de la plataforma VoIP.";

            GestionarAgentes_Image = GetImageByName("LightSwitchApplication.Images.ic_agentes.png");
            GestionarAgentes_Titulo = "Administre la lista de agentes del sistema.";

            GestionaExtensiones_Image = GetImageByName("LightSwitchApplication.Images.ic_extensiones.png");
            GestionaExtensiones_Titulo = "Administre la lista de extensiones telefónicas.";

            GestionaStatusExtensiones_Image = GetImageByName("LightSwitchApplication.Images.ic_extension_status.png");
            GestionaStatusExtensiones_Titulo = "Administre los status de los agentes.";

            GestionarHorarios_Image = GetImageByName("LightSwitchApplication.Images.ic_horarios.png");
            GestionarHorarios_Titulo = "Administre los horarios de trabajo.";

            GestionarColaDeServicio_Image = GetImageByName("LightSwitchApplication.Images.ic_servicios.png");
            GestionarColaDeServicio_Titulo = "Administre las colas de servicios.";

            GestionarMotivos_Image = GetImageByName("LightSwitchApplication.Images.ic_motivo.png");
            GestionarMotivos_Titulo = "Administre los motivos de status.";


            AgentesConectados_Titulo = "Agentes Conectados";
            AgentesConectados_Image = GetImageByName("LightSwitchApplication.Images.ic_agentes_conectados.png");
            AgentesConectados_Cantidad = "0";

            AgentesDisponibles_Titulo = "Agentes Disponibles";
            AgentesDisponibles_Image = GetImageByName("LightSwitchApplication.Images.ic_agentes_disponibles.png");
            AgentesDisponibles_Cantidad = "0";

            LlamadasMayoreas5Min_Titulo = "Llamadas > 5 min.";
            LlamadasMayoreas5Min_Image = GetImageByName("LightSwitchApplication.Images.ic_llamadas_alerta.png");
            LlamadasMayoreas5Min_Cantidad = "0";

            LlamadasEnEspera_Titulo = "Llamadas en Espera.";
            LlamadasEnEspera_Image = GetImageByName("LightSwitchApplication.Images.ic_llamadas_espera.png");
            LlamadasEnEspera_Cantidad = "0";

            TotalLlamadasDia_Titulo = "Total de Llamadas.";
            TotalLlamadasDia_Image = GetImageByName("LightSwitchApplication.Images.ic_llamadas_total.png");
            TotalLlamadasDia_Cantidad = "0";

            PromedioDuracionLlamadas_Titulo = "Promedio de Duración.";
            PromedioDuracionLlamadas_Image = GetImageByName("LightSwitchApplication.Images.ic_llamadas_promedio.png");
            PromedioDuracionLlamadas_Cantidad = "0";

            InformeStatusPorAnalista = "Status por agente.";
            InformeStatusPorAnalista_Image = GetImageByName("LightSwitchApplication.Images.ic_informe_status_analista.png");

            InformeLlamadasPorAnalista = "Llamadas por agente.";
            InformeLlamadasPorAnalista_Image = GetImageByName("LightSwitchApplication.Images.ic_informe_llamadas_analista.png");

            InformeEstadisticoLlamadasPorAgente = "Estadístico de llamadas por agente.";
            InformeEstadisticoLlamadasPorAgente_Image = GetImageByName("LightSwitchApplication.Images.ic_informe_estadistico_llamadas_analista.png");

            InformeEstadisticoColasDeServicio = "Estadístico de colas de servicio.";
            InformeEstadisticoColasDeServicio_Image = GetImageByName("LightSwitchApplication.Images.ic_informe_estadistico_llamadas_analista.png");

            InformeComparativoDeStatus = "Comparativo de status.";
            InformeComparativoDeStatus_Image = GetImageByName("LightSwitchApplication.Images.ic_informe_status_analista.png");
            

        }

        partial void Main_Created() {
            Dispatchers.Main.Invoke(() => {
                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 5);
                _timer.Tick += ActualizaEstadoActual;
                _timer.Start();
            });
        }

        private void ActualizaEstadoActual(object sender, EventArgs e) {
            this.Details.Dispatcher.BeginInvoke(() => {
                var context = Application.Current.CreateDataWorkspace().ApplicationData;
                AgentesConectados_Cantidad = GetCantidadAgentesConectados(context);
                AgentesDisponibles_Cantidad = GetCantidadAgentesDisponibles(context);
                LlamadasMayoreas5Min_Cantidad = GetCantidadLlamadasMayoreas5Min(context);
                LlamadasEnEspera_Cantidad = GetCantidadLlamadasEnEspera(context);
                TotalLlamadasDia_Cantidad = GetTotalLlamadasDia(context);
                PromedioDuracionLlamadas_Cantidad = GetPromedioDuracionLlamadas(context);
            });
        }
        
        private string GetCantidadAgentesConectados(ApplicationData context) {
            int count = CantidadDeAgentesPorStatus(context, "Conectado (Logged-in)");
            return count.ToString("N0");
        }

        private string GetCantidadAgentesDisponibles(ApplicationData context) {
            int count = CantidadDeAgentesPorStatus(context, "Preparado (Ready)"); 
            return count.ToString("N0");
        }

        private int CantidadDeAgentesPorStatus(ApplicationData context, string status) {
            var extensiones = context.Extensiones.Select(i => i).Execute();
            var statusDeExtension = context.StatusDeExtesiones.Select(i => i).Execute();
            var count = (from ext in extensiones
                         join st in statusDeExtension on
                            ext.StatusDeExtesion.Id equals st.Id
                         where st.Descripcion == status
                         select ext).ToList().Count;
            return count;
        }

        private string GetCantidadLlamadasMayoreas5Min(ApplicationData context) {
            var extensiones = context.Llamadas.Select(e => e);

            int count = extensiones.Execute().Count(e=> e.Duracion > 5);
            return count.ToString("N0");
        }

        private string GetCantidadLlamadasEnEspera(ApplicationData context) {
            var extensiones = context.Llamadas.Where(e => 
                e.Status.Contains("En Espera"));

            int count = extensiones.Execute().Count();
            return count.ToString("N0");
        }

        private string GetTotalLlamadasDia(ApplicationData context) {
            var llamadas = context.Llamadas.Select(e => e);

            int count = llamadas.Execute().Count(e => e.DeHoy);
            return count.ToString("N0");
        }

        private string GetPromedioDuracionLlamadas(ApplicationData context) {
            var llamadas = context.Llamadas.Select(e => e);
            var list = llamadas.Execute();
            int count = list.Count(e => e.DeHoy);
            var avg = 0;
            if (count > 0) {
                avg = list.Sum(e => e.Duracion) / count;
            }
            return avg.ToString("N0") + " min.";
        }

        private byte[] GetImageByName(string fileName) {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(fileName);
            return GetStreamAsByteArray(stream);
        }

        private byte[] GetStreamAsByteArray(Stream stream) {
            int streamLength = Convert.ToInt32(stream.Length);
            byte[] fileData = new byte[streamLength];
            stream.Read(fileData, 0, streamLength);
            stream.Close();
            return fileData;
        }

        partial void GestionarAgentes_Execute() {
            Application.ShowEditableAgentesGrid();
        }

        partial void GestionarColaDeServicios_Execute() {
            Application.ShowEditableColaDeServiciosGrid();
        }

        partial void GestionarHorarios_Execute() {
            Application.ShowEditableHorariosGrid();
        }

        partial void GestionarExtensiones_Execute() {
            Application.ShowEditableExtensionesGrid();
        }

        partial void GestionarStatus_Execute() {
            Application.ShowEditableStatusDeExtesionesGrid();
        }

        partial void GestionarMotivos_Execute() {
            Application.ShowEditableMotivosDeStatusGrid();
        }

        partial void MostrarInformeStatusPorAnalista_Execute() {
            Application.ShowInformeStatusPorAgentes();
        }

        partial void MostrarInformeLlamadasPorAnalista_Execute() {
            Application.ShowInformeLlamadaPorAgentes();
        }

        partial void MostrarInformeEstadisticoLlamadasPorAgente_Execute() {
            Application.ShowInformeEstadisticoDeLlamadasPorAgentes();
        }

        partial void MostrarInformeEstadisticoColasDeServicio_Execute() {
            Application.ShowInformeEstadisticoDeLlamadasPorServicios();
        }

        partial void MostrarInformeComparativoDeStatus_Execute() {
            Application.ShowInformeComparativoStatusDeAgentes();
        }
    }
}
