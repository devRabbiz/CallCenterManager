using System;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.LightSwitch;
using Microsoft.LightSwitch.Framework.Client;
using Microsoft.LightSwitch.Presentation;
using Microsoft.LightSwitch.Presentation.Extensions;

namespace LightSwitchApplication
{
    public partial class AddEditAgente
    {
        partial void AddEditAgente_InitializeDataWorkspace(List<IDataService> saveChangesTo)
        {
            if (AgenteId.HasValue && AgenteId != 0) {
                this.AgenteProperty = this.DataWorkspace.ApplicationData.Agentes
                    .Where(i => i.Id == AgenteId).FirstOrDefault();
            } else {
                this.AgenteProperty = new Agente();
                this.AgenteProperty.AgenteServicios.Add(new AgenteServicio());
            }
        }

        partial void AddEditAgente_Saved()
        {
            // Write your code here.
            this.Close(false);
            Application.Current.ShowEditableAgentesGrid();
        }
    }
}