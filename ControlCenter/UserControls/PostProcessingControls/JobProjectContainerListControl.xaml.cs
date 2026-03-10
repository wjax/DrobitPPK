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

namespace ControlCenter.UserControls.PostProcessingControls
{
    /// <summary>
    /// Interaction logic for JobProjectContainerListControl.xaml
    /// </summary>
    public partial class JobProjectContainerListControl : UserControl
    {
        public JobProjectContainerListControl()
        {
            InitializeComponent();
        }

        private void TB_NewProjectName_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Text="";
        }

    }
}
