using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Presentation;
using Xbim.Presentation.XplorerPluginSystem;

namespace XbimTRexPlugin
{
    public partial class AppStandaloneWindow : Window, IXbimXplorerPluginMasterWindow
    {
        public AppStandaloneWindow()
        {
            InitializeComponent();
            Store = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
        }

        public ObjectDataProvider StoreProvider => MainGrid.DataContext as ObjectDataProvider;

        public IfcStore Model => Store;

        public IfcStore Store
        {
            get => StoreProvider.ObjectInstance as IfcStore;
            set {
                if (null != StoreProvider.ObjectInstance)
                {
                    StoreProvider.ObjectInstance = null;
                    StoreProvider.Refresh();
                }
                StoreProvider.ObjectInstance = value;
                StoreProvider.Refresh();
            }
        }

        public DrawingControl3D DrawingControl => throw new NotImplementedException();

        public IPersistEntity SelectedItem
        {
            get { return (IPersistEntity)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem", 
                typeof(IPersistEntity), 
                typeof(AppStandaloneWindow),
                new UIPropertyMetadata(null, OnSelectedItemChanged));


        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as AppStandaloneWindow;
            if (window != null && e.NewValue is IPersistEntity p)
            {
                // TODO
            }            
        }

        #region Default implementations

        public void BroadCastMessage(object sender, string messageTypeString, object messageData)
        {
        }

        public string GetAssemblyLocation(Assembly requestingAssembly)
        {
            return null;
        }

        public string GetOpenedModelFileName()
        {
            return Store.FileName;
        }

        public void RefreshModel()
        {
        }

        public void RefreshPlugins()
        {
        }

        #endregion

        #region Event handling

        private void CommandBinding_LoadIfcFile(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO
        }

        private void CommandBinding_SaveIfcFile(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO
        }

        #endregion
    }
}
