using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication {
    public partial class Llamada {
        partial void Duracion_Compute(ref int result) {
            result = Fin.Subtract(Inicio).Minutes;
        }

        partial void DeHoy_Compute(ref bool result) {
            result = 
                Inicio.Year == DateTime.Now.Year && 
                Inicio.Month == DateTime.Now.Month && 
                Inicio.Day == DateTime.Now.Day;
        }
    }
}
