using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CallCenterManager.Charts {
    public partial class StatusPorAgenteChart : UserControl {
        public StatusPorAgenteChart() {
            InitializeComponent();
        }

        private void Chart_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            
        }
    }

    public class StatusDef : List<string> {
    }
}
