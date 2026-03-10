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
    /// Interaction logic for JobProjectContainer.xaml
    /// </summary>
    public partial class JobProjectContainer : UserControl
    {
        public JobProjectContainer()
        {
            InitializeComponent();

            this.DragEnter += JobProjectContainer_DragEnter;
        }

        private void JobProjectContainer_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }
    }
}
