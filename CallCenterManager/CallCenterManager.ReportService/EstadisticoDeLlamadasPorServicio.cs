using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenterManager.ReportService {
    public class EstadisticoDeLlamadasPorServicio {
        [Key]
        public int ID { get; set; }
        public string Servicio { get; set; }

        public double Value { get; set; }
        public string Descripcion { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}
