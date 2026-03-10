using ControlCenter.Base;
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

namespace ControlCenter.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for WaitingDialog.xaml
    /// </summary>
    public partial class WaitingDialog : UserControl
    {
        public WaitingDialog()
        {
            InitializeComponent();
        }
    }

    public class WaitingDialogData : BindableModelBase
    {
        public WaitingDialogData()
        {
            Message = "";
        }


        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                Set(ref message, value);
            }
        }

    }
}
