using System;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Data;
using Microsoft.Win32;

namespace XbimTRexPlugin.Controls
{
    /// <summary>
    /// Axis alignment control wrapping source and target reference axis into an aligment provider.
    /// </summary>
    public partial class AxisAlignmentControl : UserControl
    {
        public AxisAlignmentControl()
        {
            InitializeComponent();
            AxisAlignment = new AxisAlignment();
        }

        /// <summary>
        /// Alignment data provider.
        /// </summary>
        public ObjectDataProvider AxisAlignmentProvider => AxisReferenceControl.DataContext as ObjectDataProvider;

        /// <summary>
        /// Currently bound axis alignment data instance.
        /// </summary>
        public AxisAlignment AxisAlignment
        {
            get {
                return AxisAlignmentProvider.ObjectInstance as AxisAlignment;
            }
            set {
                AxisAlignmentProvider.ObjectInstance = value;
                AxisAlignmentProvider.Refresh();
            }
        }

        #region Event handling

        private void CommandBinding_SaveSourceAxis(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Save selected source X-axis";
            dlg.Filter = "XML files | *.xml";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    AxisAlignment.SourceReferenceAxis.SaveTo(dlg.FileName);
                }
                catch(Exception)
                {
                    MessageBox.Show($"Failed to save to file {dlg.FileName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CommandBinding_LoadTargetAxis(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Load reference X-axis";
            dlg.Filter = "XML files | *.xml";

            if (dlg.ShowDialog() == true)
            {
                var sourceAxis = AxisAlignment.SourceReferenceAxis;
                try
                {
                    AxisAlignment = new AxisAlignment(sourceAxis, ReferenceAxis.LoadFrom(dlg.FileName));
                }
                catch(Exception)
                {
                    MessageBox.Show($"Failed to load from file {dlg.FileName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CommandBinding_SaveAxisAlignment(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Save current alignment";
            dlg.Filter = "XML files | *.xml";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    AxisAlignment.SaveToFile(dlg.FileName);
                }
                catch(Exception)
                {
                    MessageBox.Show($"Failed to save to file {dlg.FileName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CommandBinding_LoadAxisAlignment(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Load complete alignment";
            dlg.Filter = "XML files | *.xml";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    AxisAlignment = AxisAlignment.LoadFromFile(dlg.FileName);
                }
                catch(Exception)
                {
                    MessageBox.Show($"Failed to load from file {dlg.FileName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}
