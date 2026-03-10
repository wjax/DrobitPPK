using ControlCenter.Models.JOBS;
using ControlCenter.Models.JOBS.Parameters;
using ControlCenter.Models.JOBS.Parameters.GNSS;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlCenter.UserControls.PostProcessingControls
{
    /// <summary>
    /// Interaction logic for GNSSPostProcessingControl.xaml
    /// </summary>
    public partial class GNSSPostProcessingControl : UserControl
    {
        Point _startPoint;
        bool IsDragging;
        Cursor _allOpsCursor;

        public GNSSPostProcessingControl()
        {
            InitializeComponent();
            GNSSInputControlBASE.NewParamEvent += GNSSInputControlBASE_NewRINEXEvent;
            GNSSInputControlROVER.NewParamEvent += GNSSInputControlROVER_NewRINEXEvent;

            this.DataContextChanged += GNSSPostProcessingControl_DataContextChanged;

        }

        private void GNSSPostProcessingControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           if (e.NewValue != null)
                if (e.NewValue is GNSSPostProcessingJob)
                {
                    GNSSPostProcessingJob job = (GNSSPostProcessingJob)DataContext;
                    job.RecalculateObsMaxMinTimes();
                    GNSSInputControlROVER.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
                    GNSSInputControlBASE.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
                }
        }

        private void GNSSInputControlROVER_NewRINEXEvent(JobParameter rinex)
        {
            GNSSPostProcessingJob job = (GNSSPostProcessingJob)DataContext;
            //job.RoverGNSS = rinex;
            job.RecalculateObsMaxMinTimes();
            GNSSInputControlROVER.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
            GNSSInputControlBASE.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);

            // Check whether there is trigger info in it and add new one
            
        }

        private void GNSSInputControlBASE_NewRINEXEvent(JobParameter rinex)
        {
            GNSSPostProcessingJob job = (GNSSPostProcessingJob)DataContext;
            //job.BaseGNSS = rinex;
            job.RecalculateObsMaxMinTimes();
            GNSSInputControlROVER.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
            GNSSInputControlBASE.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
        }

        private void StartDrag(MouseEventArgs e)
        {
            IsDragging = true;
            DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
            DragDropEffects de = DragDrop.DoDragDrop(B_DragMe, data, DragDropEffects.Move);
            IsDragging = false;
        }

        private void StartDragCustomCursor(MouseEventArgs e)
        {
            GiveFeedbackEventHandler handler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
            B_DragMe.GiveFeedback += handler;
            IsDragging = true;
            DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), B_DragMe.Tag.ToString());
            DragDropEffects de = DragDrop.DoDragDrop(B_DragMe, data, DragDropEffects.Move);
            B_DragMe.GiveFeedback -= handler;
            IsDragging = false;

        }

        void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                //This loads the cursor from a stream ..
                if (_allOpsCursor == null)
                {
                    //_allOpsCursor = Cursors.Arrow;
                    using (Stream cursorStream = new System.IO.MemoryStream(Properties.Resources.data))
                    {
                        _allOpsCursor = new Cursor(cursorStream);
                    }
                }
                Mouse.SetCursor(_allOpsCursor);
                e.UseDefaultCursors = false;
                e.Handled = true;
            }
            finally { }
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDragCustomCursor(e);
                }

            }
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }
    }
}
