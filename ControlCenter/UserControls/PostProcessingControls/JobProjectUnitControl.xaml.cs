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
    /// Interaction logic for JobProjectUnitControl.xaml
    /// </summary>
    public partial class JobProjectUnitControl : UserControl
    {
        public JobProjectUnitControl()
        {
            InitializeComponent();
        }

        private void dialogHostJobProject_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            // We get here after dialog confirmation for drobit shutdown is closed.
            if (eventArgs.Parameter != null)
            {
                if (eventArgs.Parameter is ICommand)
                {
                    // Have to launch the command
                    ICommand cmd = eventArgs.Parameter as ICommand;
                    cmd.Execute(DataContext);
                }
            }
        }

    }

    
}
