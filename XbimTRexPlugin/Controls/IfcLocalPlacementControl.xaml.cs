using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XbimTRexPlugin.Controls
{
    /// <summary>
    /// Interaktionslogik für IfcLocalPlacementControl.xaml
    /// </summary>
    public partial class IfcLocalPlacementControl : UserControl
    {
        public IfcLocalPlacementControl()
        {
            InitializeComponent();
            Placement = new IfcLocalPlacementDescriptor();
        }

        public ObjectDataProvider PlacementProvider => PlacementControl.DataContext as ObjectDataProvider;

        public IfcLocalPlacementDescriptor Placement
        {
            get => PlacementProvider.ObjectInstance as IfcLocalPlacementDescriptor;
            set {
                PlacementProvider.ObjectInstance = value;
                PlacementProvider.Refresh();
            }
        }
    }
}
