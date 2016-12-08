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
    public partial class EditableAgentesGrid {
        partial void CreateNew_Execute() {
            Application.ShowAddEditAgente(null);
        }

        partial void Modificar_Execute() {
            Application.ShowAddEditAgente(Agentes.SelectedItem.Id);
        }

        partial void Eliminar_Execute() {
            var result = this.ShowMessageBox("Ésta acción eliminara el registro. ¿Desea continuar?", "Eliminar Agente", MessageBoxOption.YesNo);
            if (result == System.Windows.MessageBoxResult.Yes) {
                this.Agentes.DeleteSelected();
                this.Save();
            }
        }
    }
}
