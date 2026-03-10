using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlCenter.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for .xaml
    /// </summary>
    public partial class MainSettingsDialog : System.Windows.Controls.UserControl
    {
        public MainSettingsDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            DialogResult result = openFileDialog.ShowDialog();



            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.SelectedPath))
            {
                if (Directory.Exists(openFileDialog.SelectedPath))
                {
                    TB_Folder.Text = openFileDialog.SelectedPath;
                    B_SAVE.IsEnabled = true;
                }
            }
        }
    }
}
