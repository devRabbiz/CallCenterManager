using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenterManager.ReportService {
    public class StatusPorAgente {
        [Key]
        public int ID { get; set; }

        public int IdAgente { get; set; }
        public string Agente { get; set; }
        public string Status { get; set; }
        public string HoraInicio { get; set; }
        public string Tiempo { get; set; }
        public int TiempoAsInt { get; set; }
        public string Motivo { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

    public class StatusPorAgenteGrouped {
        [Key]
        public int ID { get; set; }

        public string Agente { get; set; }
        public string Status { get; set; }
        public int TiempoAsInt { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}
