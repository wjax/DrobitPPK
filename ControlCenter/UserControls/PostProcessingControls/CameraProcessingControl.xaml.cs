using ControlCenter.Models;
using ControlCenter.Models.JOBS;
using ControlCenter.Models.JOBS.Parameters;
using ControlCenter.Models.JOBS.Parameters.GNSS;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlCenter.UserControls.PostProcessingControls
{
    /// <summary>
    /// Interaction logic for PositionInterpolatorControl.xaml
    /// </summary>
    public partial class CameraProcessingControl : UserControl
    {
        string[] picturesURIs = new string[0];

        private const string JPEG_FILE_REGEX = @"\.jpg|\.jpeg";
        private const string PRECISE_EXTERNAL_ORIENTATION = @"^([\w\-. ]+);([-+]?[0-9]*\.?[0-9]+);([-+]?[0-9]*\.?[0-9]+);([-+]?[0-9]*\.?[0-9]+)$";

        public CameraProcessingControl()
        {
            InitializeComponent();
            InputControlPOS.NewParamEvent += InputControl_NewRINEXEvent;
            InputControlCAM.NewParamEvent += InputControl_NewRINEXEvent;

            this.DataContextChanged += CAMProcessingControl_DataContextChanged;

        }

        private void CAMProcessingControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                if (e.NewValue is CAMProcessingJob)
                {
                    CAMProcessingJob job = (CAMProcessingJob)DataContext;
                    job.CAMControl = InputControlCAM;
                    job.POSControl = InputControlPOS;
                    job.RecalculateObsMaxMinTimes();
                    job.ApplyObsMaxMinTimes();

                }
        }

        private void InputControl_NewRINEXEvent(JobParameter unit)
        {
            CAMProcessingJob job = (CAMProcessingJob)DataContext;
            ////job.RoverGNSS = rinex;
            job.RecalculateObsMaxMinTimes();
            job.ApplyObsMaxMinTimes();
            //InputControlPOS.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
            //InputControlCAM.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);

            job.PicsInCam = job.CamUnit.CameraShots.Count;
            if (job.InputParameter is CAMUnit)
                if ((job.InputParameter as CAMUnit).IsDJI)
                    job.ShutterDelay = false;
            // Add if needed;
            PopulatePictureURIInformation(job.CamUnit.CameraShots, picturesURIs);
        }

        private void GNSSInputControlBASE_NewRINEXEvent(JobParameter unit)
        {
            //GNSSPostProcessingJob job = (GNSSPostProcessingJob)DataContext;
            ////job.BaseGNSS = rinex;
            //job.RecalculateObsMaxMinTimes();
            //GNSSInputControlROVER.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
            //GNSSInputControlBASE.SetMinMaxTimeLine(job.MinOBSTime, job.MaxOBSTime);
        }

        private void Button_Drop(object sender, DragEventArgs e)
        {
            Button b = (Button)sender;
            b.Effect = null;

            CAMProcessingJob job = (CAMProcessingJob)DataContext;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                picturesURIs = (string[])e.Data.GetData(DataFormats.FileDrop);
                Array.Sort(picturesURIs);

                job.PicsToGeotag = picturesURIs.Length;

                PopulatePictureURIInformation(job.CamUnit.CameraShots, picturesURIs);
            }
        }

        private void Button_Drop_External(object sender, DragEventArgs e)
        {
            Button b = (Button)sender;
            b.Effect = null;

            CAMProcessingJob job = (CAMProcessingJob)DataContext;
            int numValidExternalOrientations = 0;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] preciseExternalFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (preciseExternalFiles.Length > 0)
                {
                    Dictionary<string, float[]> preciseOrientationDict = new Dictionary<string, float[]>();
                    string[] lines = File.ReadAllLines(preciseExternalFiles[0]);
                    // Read external file
                    foreach (string line in lines)
                    {
                        Match mLine = Regex.Match(line, PRECISE_EXTERNAL_ORIENTATION);
                        if (mLine.Groups.Count > 1)
                        {
                            float roll, pitch, yaw;
                            string name;
                            name = mLine.Groups[1].Value;
                            float.TryParse(mLine.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out roll);
                            float.TryParse(mLine.Groups[3].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out pitch);
                            float.TryParse(mLine.Groups[4].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out yaw);

                            preciseOrientationDict.Add(name.ToLowerInvariant(), new float[] { roll, pitch, yaw });

                            numValidExternalOrientations++;
                        }
                    }
                    // Update number
                    job.PicsInExternal = numValidExternalOrientations;

                    // Now include the info from externalfile into CamUnits
                    foreach (CameraShot cam in job.CamUnit.CameraShots.Values)
                    {
                        string name = cam.Name.ToLowerInvariant();
                        if (preciseOrientationDict.ContainsKey(name))
                        {
                            float[] preciseAttitude = preciseOrientationDict[name];
                            cam.ManualRoll = preciseAttitude[0];
                            cam.ManualPitch = preciseAttitude[1];
                            cam.ManualYaw = preciseAttitude[2];
                        }
                    }
                }
            }
        }

        private void PopulatePictureURIInformation(SortedDictionary<long, CameraShot> cameras, string[] fileURIs)
        {
            CAMProcessingJob job = (CAMProcessingJob)DataContext;

            if (fileURIs != null)
            { 
                for (int i = 0; i < fileURIs.Length; i++)
                {
                    if (!Regex.IsMatch(fileURIs[i].ToLowerInvariant(), JPEG_FILE_REGEX))
                        job.IsJPEG = false;
                    else
                        job.IsJPEG = true;

                    try
                    {
                        CameraShot cam = cameras.Values.ElementAt(i);
                        cam.fileOriginalUri = fileURIs[i];
                        cam.Name = System.IO.Path.GetFileName(fileURIs[i]);
                    }
                    catch (ArgumentOutOfRangeException ee)
                    {
                    }
                }
            }
        }

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            Button b = (Button)sender;
            DropShadowEffect effect = new DropShadowEffect();

            var palette = new PaletteHelper().QueryPalette();
            var hue = palette.AccentSwatch.AccentHues.ToArray()[palette.AccentHueIndex];
            effect.Color = hue.Color;


            effect.Direction = 0;
            effect.ShadowDepth = 0;
            effect.Opacity = 0.5;
            effect.BlurRadius = 15;

            b.Effect = effect;

            TimeSpan duration = TimeSpan.FromMilliseconds(500);
            DoubleAnimation animateOpacity = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = duration,
                //AutoReverse = true
            };

            effect.BeginAnimation(DropShadowEffect.OpacityProperty,
                                            animateOpacity);
        }

        private void Button_DragLeave(object sender, DragEventArgs e)
        {
            Button b = (Button)sender;
            b.Effect = null;

            //TimeSpan duration = TimeSpan.FromMilliseconds(500);
            //DoubleAnimation animateOpacity = new DoubleAnimation()
            //{
            //    From = 0,
            //    To = 1,
            //    Duration = duration,
            //    //AutoReverse = true
            //};

            //baseButtomEffect.BeginAnimation(DropShadowEffect.OpacityProperty,
            //                                animateOpacity);
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == RB_CAM_External)
            {
                RB_Fixed.IsChecked = true;
                RB_Gimbal.IsEnabled = false;
            }
            else
            {
                RB_Gimbal.IsEnabled = true;
            }
            
        }
    }
}
