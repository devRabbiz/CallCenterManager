using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;
namespace LightSwitchApplication {
    public partial class InformeComparativoStatusDeAgentes {
        partial void ColaDeServicio_Changed() {
            if (ColaDeServicio != null && StatusDeExtension != null) {
                this.ComparativoStatusDeAgentes.Load();
            }
        }

        partial void StatusDeExtension_Changed() {
            if (ColaDeServicio != null && StatusDeExtension != null) {
                this.ComparativoStatusDeAgentes.Load();
            }
        }

        partial void InformeComparativoStatusDeAgentes_InitializeDataWorkspace(List<IDataService> saveChangesTo) {
            FechaDesde = DateTime.Now.AddDays(-30);
            FechaHasta = DateTime.Now;
        }
    }
}
