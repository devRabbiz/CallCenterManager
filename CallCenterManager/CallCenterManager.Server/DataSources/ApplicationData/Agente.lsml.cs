using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LightSwitch;
namespace LightSwitchApplication {
    public partial class Agente {
        partial void Horario_Compute(ref string result) {
            var entity = AgenteServicios.FirstOrDefault();
            if (entity != null) {
                result = entity.Horario.Summary;
            }
        }

        partial void ColaDeServicio_Compute(ref string result) {
            var entity = AgenteServicios.FirstOrDefault();
            if (entity != null) {
                result = entity.ColaDeServicio.Nombre;
            }
        }

        partial void Extension_Compute(ref string result) {
            var entity = AgenteServicios.FirstOrDefault();
            if (entity != null) {
                result = entity.Extension.Numero;
            }
        }

        partial void FullName_Compute(ref string result) {
            result = string.Format("{0} {1}", Nombre, Apellido);
        }
    }
}
