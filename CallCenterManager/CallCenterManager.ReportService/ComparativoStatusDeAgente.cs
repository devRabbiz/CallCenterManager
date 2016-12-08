using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenterManager.ReportService {
    public class ComparativoStatusDeAgente {
        [Key]
        public int ID { get; set; }
        public string Servicio { get; set; }
        public string Status { get; set; }

        public double Tiempo { get; set; }
        public string Agente { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}
