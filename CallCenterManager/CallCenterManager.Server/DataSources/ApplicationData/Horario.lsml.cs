using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication {
    public partial class Horario {
        partial void Horario_Created() {
            this.HoraInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
            this.HoraFin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
        }

        partial void Summary_Compute(ref string result) {
            result = string.Format("De {0} a {1}", HoraInicio.ToShortTimeString(), HoraFin.ToShortTimeString());
        }   
    }
}
