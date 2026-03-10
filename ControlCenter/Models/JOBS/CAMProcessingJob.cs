using ControlCenter.Extras;
using ControlCenter.Extras.KML;
using ControlCenter.GNSSProcessingEngine;
using ControlCenter.Models.JOBS.Parameters.GNSS;
using ControlCenter.UserControls.PostProcessingControls;
using DrobitExtras;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ControlCenter.Models.JOBS
{
    public class CAMProcessingJob : IWorkJob
    {
        private const int SPLINE_MAX_SAMPLES = 10;
        private const int MAX_SAMPLES_DISTANCE = 4;

        private const string SEPARATOR = ";";

        private const string CAM_FILE_OUTPUT_HEADER = "PicName; Longitude; Latitude; Altitude; Yaw; Pitch; Roll; Accuracy; ShutterDelay; GPS Week; GPS Second; Roll Hotshoe; Pitch Hotshoe; Yaw Hotshoe";
        private const string GPS_FILE_OUTPUT_HEADER = "PicName; Longitude; Latitude; Altitude; Yaw; Pitch; Roll; Accuracy; ShutterDelay; GPS Week; GPS Second";

        private const string TIME_MARK_EVENT_REGEX = @"^\s*([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+\.+[0-9]+)\s+5+\s+0$";

        [JsonProperty("GeotagImagesOutputFolder")]
        public string GeotagImagesOutputFolder;

        [JsonProperty("KMLIconsOutputFolder")]
        public string KMLIconsOutputFolder;

        [JsonIgnore]
        public DrobitInputFileControl CAMControl;
        [JsonIgnore]
        public DrobitInputFileControl POSControl;

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CAMFixingType
        {
            [Description("Mount fixed 2 Body")]
            FIXED2BODY,
            [Description("Gimbal mount")]
            GIMBALED
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum OrientationType
        {
            [Description("Calculate From IMUs")]
            FROM_CAM,
            [Description("Manual/Fixed")]
            MANUAL_FIXED,
            [Description("From external file")]
            FROM_EXTERNAL
        }

        private const string OUTPUT_FILENAME_CAM = "cameras.txt";
        private const string OUTPUT_FILENAME_ANTENNA = "antennas.txt";
        private const string OUTPUT_FOLDER = "OUTPUT";
        private const string INPUT_FOLDER = "INPUT";

        [JsonProperty("parentFolder")]
        public string parentFolder;


        private CAMFixingType camFixingType;
        [JsonProperty("CamFixingType")]
        public CAMFixingType CamFixingType
        {
            get { return camFixingType; }
            set {
                Set(ref camFixingType, value);
                if (CamFixingType == CAMFixingType.FIXED2BODY)
                {
                    KMLOrientationSRCBody = true;
                    KMLOrientationSRCHotshoe = false;
                }
                else
                {
                    KMLOrientationSRCBody = false;
                    KMLOrientationSRCHotshoe = true;
                }
                    
            }
        }

        private OrientationType orientationMode;
        [JsonProperty("OrientationMode")]
        public OrientationType OrientationMode
        {
            get { return orientationMode; }
            set {
                Set(ref orientationMode, value);
                //if (value == OrientationType.FROM_EXTERNAL && CamFixingType != CAMFixingType.FIXED2BODY)
                //    CamFixingType = CAMFixingType.FIXED2BODY;
            }
        }

        private bool isJPEG;
        [JsonProperty("IsJPEG")]
        public bool IsJPEG
        {
            get { return isJPEG; }
            set {
                Set(ref isJPEG, value);
                //HasToGeotag = value;
            }
        }

        private bool forceGPSHeading;
        [JsonProperty("ForceGPSHeading")]
        public bool ForceGPSHeading
        {
            get { return forceGPSHeading; }
            set
            {
                Set(ref forceGPSHeading, value);
            }
        }

        private string altitudeOffset;
        [JsonProperty("AltitudeOffset")]
        public string AltitudeOffset
        {
            get { return altitudeOffset; }
            set
            {
                Set(ref altitudeOffset, value);
            }
        }

        private string pitchOffset;
        [JsonProperty("PitchOffset")]
        public string PitchOffset
        {
            get { return pitchOffset; }
            set
            {
                Set(ref pitchOffset, value);
            }
        }

        private string modelScale;
        [JsonProperty("ModelScale")]
        public string ModelScale
        {
            get { return modelScale; }
            set
            {
                Set(ref modelScale, value);
            }
        }

        private float manualRoll;
        public float ManualRoll
        {
            get { return manualRoll;}
            set
            {
                Set(ref manualRoll, value);
            }
        }

        private float manualPitch;
        public float ManualPitch
        {
            get { return manualPitch; }
            set
            {
                Set(ref manualPitch, value);
            }
        }

        private float manualYaw;
        public float ManualYaw
        {
            get { return manualYaw; }
            set
            {
                Set(ref manualYaw, value);
            }
        }

        public void ApplyObsMaxMinTimes()
        {
            POSControl?.SetMinMaxTimeLine(MinOBSTime, MaxOBSTime);
            CAMControl?.SetMinMaxTimeLine(MinOBSTime, MaxOBSTime);
        }

        private bool hasToGeotag;
        [JsonProperty("HasToGeotag")]
        public bool HasToGeotag
        {
            get { return hasToGeotag; }
            set { Set(ref hasToGeotag, value); }
        }

        private bool kmlOrientationSRCBody;
        [JsonProperty("KMLOrientationSRCBody")]
        public bool KMLOrientationSRCBody
        {
            get { return kmlOrientationSRCBody; }
            set {
                Set(ref kmlOrientationSRCBody, value);
                if (kmlOrientationSRCBody)
                    PitchOffset = "-90";
            }
        }

        private bool kmlOrientationSRCHotshoe;
        [JsonProperty("KMLOrientationSRCHotshoe")]
        public bool KMLOrientationSRCHotshoe
        {
            get { return kmlOrientationSRCHotshoe; }
            set {
                Set(ref kmlOrientationSRCHotshoe, value);
                if (kmlOrientationSRCHotshoe)
                    PitchOffset = "0";
            }
        }

        private bool shutterDelay;
        [JsonProperty("ShutterDelay")]
        public bool ShutterDelay
        {
            get { return shutterDelay; }
            set { Set(ref shutterDelay, value); }
        }

        private string shutterDelayMS;
        [JsonProperty("ShutterDelayMS")]
        public string ShutterDelayMS
        {
            get { return shutterDelayMS; }
            set { Set(ref shutterDelayMS, value); }
        }

        private string inputsFolder;
        [JsonProperty("InputsFolder")]
        public string InputsFolder
        {
            get { return inputsFolder; }
            set { Set(ref inputsFolder, value); }
        }

        private string ouputsFolder;
        [JsonProperty("OutputsFolder")]
        public string OutputsFolder
        {
            get { return ouputsFolder; }
            set { Set(ref ouputsFolder, value); }
        }

        // Input Parameter
        private CAMUnit camUnit;
        [JsonProperty("CamUnit")]
        public CAMUnit CamUnit
        {
            get { return camUnit; }
            set
            {
                Set(ref camUnit, value);
            }
        }

        private int picsInCam;
        [JsonProperty("PicsInCam")]
        public int PicsInCam
        {
            get { return picsInCam; }
            set
            {
                Set(ref picsInCam, value);
            }
        }

        private int picsInExternal;
        [JsonProperty("PicsInExternal")]
        public int PicsInExternal
        {
            get { return picsInExternal; }
            set
            {
                Set(ref picsInExternal, value);
            }
        }


        private int picsToGeotag;
        [JsonProperty("PicsToGeotag")]
        public int PicsToGeotag
        {
            get { return picsToGeotag; }
            set
            {
                Set(ref picsToGeotag, value);
            }
        }

        private int highQCameras;
        [JsonProperty("HighQCameras")]
        public int HighQCameras
        {
            get { return highQCameras; }
            set
            {
                Set(ref highQCameras, value);
            }
        }

        private int medQCameras;
        [JsonProperty("MedQCameras")]
        public int MedQCameras
        {
            get { return medQCameras; }
            set
            {
                Set(ref medQCameras, value);
            }
        }

        private int lowQCameras;
        [JsonProperty("LowQCameras")]
        public int LowQCameras
        {
            get { return lowQCameras; }
            set
            {
                Set(ref lowQCameras, value);
            }
        }

        private int lowestQCameras;
        [JsonProperty("LowestQCameras")]
        public int LowestQCameras
        {
            get { return lowestQCameras; }
            set
            {
                Set(ref lowestQCameras, value);
            }
        }

        // Input Parameter
        //private RTKPOSUnit posUnit;
        //public RTKPOSUnit PosUnit
        //{
        //    get { return posUnit; }
        //    set
        //    {
        //        Set(ref posUnit, value);
        //    }
        //}

        private DateTime minOBSTime;
        [JsonProperty("MinOBSTime")]
        public DateTime MinOBSTime
        {
            get { return minOBSTime; }
            set
            {
                Set(ref minOBSTime, value);
            }
        }

        private DateTime maxOBSTime;
        [JsonProperty("MaxOBSTime")]
        public DateTime MaxOBSTime
        {
            get { return maxOBSTime; }
            set
            {
                Set(ref maxOBSTime, value);
            }

        }
        public CAMProcessingJob(string parentFolder)
        {
            Worktype = WorkType.CAMPROC;
            this.parentFolder = parentFolder;

            CamUnit = new CAMUnit();
            CamUnit.Type = "CAM";
            //CamUnit.PropertyChanged += Param_PropertyChanged;
            InputParameter = new RTKPOSUnit();
            (InputParameter as RTKPOSUnit).Type = "POS";
            //PosUnit.PropertyChanged += Param_PropertyChanged;

            CamFixingType = CAMFixingType.FIXED2BODY;

            NeedInputParameterFromPrevious = true;

            IsJPEG = true;
            //HasToGeotag = true;

            ModelScale = "1";
            AltitudeOffset = "0";
        }

        private void Param_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        public void RecalculateObsMaxMinTimes()
        {
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;

            RTKPOSUnit PosUnit = (RTKPOSUnit)InputParameter;

            if (!CamUnit.StartTime.Equals(DateTime.Parse("2000-01-01T12:00:00")) && !CamUnit.StopTime.Equals(DateTime.Parse("2000-01-01T12:00:00")))
            {
                // Base is set
                min = CamUnit.StartTime;
                max = CamUnit.StopTime;
            }

            if (PosUnit != null)
            { 
                if (!PosUnit.StartTime.Equals(DateTime.Parse("2000-01-01T12:00:00")) && !PosUnit.StopTime.Equals(DateTime.Parse("2000-01-01T12:00:00")))
                {
                    if (PosUnit.StartTime.CompareTo(min) < 0)
                    {
                        min = PosUnit.StartTime;
                    }
                    if (PosUnit.StopTime.CompareTo(max) > 0)
                    {
                        max = PosUnit.StopTime;
                    }
                }
            }

            MinOBSTime = min;
            MaxOBSTime = max;


        }

        public void Initialize()
        {
            // Create and associate working folder
            Name = Worktype + "_" + Guid.NewGuid().ToString();
            WorkingFolder = DrobitTools.ConcatenatePath(new string[] { parentFolder, Name });
            DrobitTools.CreateFolderIfNotExists(WorkingFolder);

            OutputsFolder = DrobitTools.ConcatenatePath(new string[] { WorkingFolder, OUTPUT_FOLDER });
            DrobitTools.CreateFolderIfNotExists(OutputsFolder);
            InputsFolder = DrobitTools.ConcatenatePath(new string[] { WorkingFolder, INPUT_FOLDER });
            DrobitTools.CreateFolderIfNotExists(InputsFolder);

            GeotagImagesOutputFolder = DrobitTools.ConcatenatePath(new string[] { OutputsFolder, "GEO_IMAGES" });
            KMLIconsOutputFolder = DrobitTools.ConcatenatePath(new string[] { OutputsFolder, "kmlresources" });

            Percentage = 0;

            HasToGeotag = false;

            ForceGPSHeading = false;
            ManualPitch = 0;
            ManualRoll = 0;
            ManualYaw = 0;

            OrientationMode = OrientationType.FROM_CAM;

        }

        #region WORK_OVERIDES

        public override void StopJob(object obj = null)
        {
            base.StopJob();
        }

        public override void StartJob(object obj = null)
        {
            System.Diagnostics.Debug.WriteLine("Started CAMJob: " + Name);

            Phase = WorkPhase.RUNNING;

            Notes = "";
            HighQCameras = 0;
            MedQCameras = 0;
            LowQCameras = 0;
            LowestQCameras = 0;

            // Empty Output dir
            DrobitTools.EmptyDirectory(OutputsFolder);

            // Create Images folder
            DrobitTools.CreateFolderIfNotExists(GeotagImagesOutputFolder);
            DrobitTools.CreateFolderIfNotExists(KMLIconsOutputFolder);

            // Process Job
            //ThreadPool.QueueUserWorkItem(new WaitCallback(DoProcessJob), null);
            DoProcessJob();

        }

        private void DoProcessJob()
        {
            if (CamUnit.CameraShots.Count <= 0)
                return;

            int msShutterDelay = 0;
            int.TryParse(ShutterDelayMS, out msShutterDelay);

            int numCamFound = 0;
            int maxPercentPhase1 = 95;
            float cam2Percent = (100f / CamUnit.CameraShots.Count) * (maxPercentPhase1 / 100f);

            RTKPOSUnit PosUnit = (RTKPOSUnit)InputParameter;

            if (PosUnit == null)
            {
                Percentage = 100;
                Phase = WorkPhase.COMPLETED;
                Notes = "GPS Pos file missing or corrupted";
                return;
            }

            long millisBTSamples = PosUnit.MillisBTSamples;
            long millisMAXDistance = MAX_SAMPLES_DISTANCE * millisBTSamples;

            // Interpolation --------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------
            SplineInterpolatorB LatInterpolator = new SplineInterpolatorB(SPLINE_MAX_SAMPLES - 2);
            SplineInterpolatorB LonInterpolator = new SplineInterpolatorB(SPLINE_MAX_SAMPLES - 2);
            SplineInterpolatorB AltInterpolator = new SplineInterpolatorB(SPLINE_MAX_SAMPLES - 2);

            // Apply shutter delay
            foreach (CameraShot shot in CamUnit.CameraShots.Values)
                shot.shutterDelay = msShutterDelay;

            //Copy Camera positions to aux structure
            SortedList<long, CameraShot> camerasAux = new SortedList<long, CameraShot>();
            foreach (long key in CamUnit.CameraShots.Keys)
                camerasAux.Add(key, CamUnit.CameraShots[key]);

            for (int i = 0; i < PosUnit.GPSPositions.Count; i++)
            {
                if (needsExit)
                {
                    StoppedJob();
                    return;
                }
                    
                long millisGPS = PosUnit.GPSPositions.ElementAt(i).Key / 1000;

                foreach (long key in camerasAux.Keys)
                {
                    CameraShot currCamShot = camerasAux[key];
                    long cam = currCamShot.MicrosTime;
                    long millisCamera = cam / 1000;

                    millisCamera += currCamShot.shutterDelay;
            
                    long millisDiff = millisCamera - millisGPS;
                    //System.Diagnostics.Debug.WriteLine("Diff: " + millisDiff);
                    if (Math.Abs(millisDiff) <= millisMAXDistance)
                    {
                        // Found camera position in GPS file
                        // Have to copy SPLINE_MAX_SAMPLES/2 before and after gps samples
                        numCamFound++;

                        Notes = "Found Shot " + numCamFound;

                        Percentage = (int)(numCamFound * cam2Percent);
                        //System.Diagnostics.Debug.WriteLine("Percentage : " +  Percentage);
                        
                        LatInterpolator.clear();
                        LonInterpolator.clear();
                        AltInterpolator.clear();
                        // We have stoped millisMAXDistance before so we have to advance as close as possible to the CAM real position
                        int sweetPoint = i + MAX_SAMPLES_DISTANCE - 1;

                        int lowLimitLoop = sweetPoint - (SPLINE_MAX_SAMPLES / 2);
                        int highLimitLoop = sweetPoint + (SPLINE_MAX_SAMPLES / 2);

                        Notes = "Found Shot " + numCamFound + ". Interpolating...";

                        // By default HIGH. We will modify accordingly
                        currCamShot.Accuracy = CameraShot.HIGH_ACCURACY;

                        if (lowLimitLoop >= 0 && highLimitLoop < PosUnit.GPSPositions.Count)
                        {
                            for (int u = lowLimitLoop; u < highLimitLoop; u++)
                            {
                                try
                                {
                                    KeyValuePair<long, GPSPosition> pairGPSPos;
                                    pairGPSPos = PosUnit.GPSPositions.ElementAt(u);

                                    LatInterpolator.push(pairGPSPos.Value.Lat, pairGPSPos.Key);
                                    LonInterpolator.push(pairGPSPos.Value.Lon, pairGPSPos.Key);
                                    AltInterpolator.push(pairGPSPos.Value.Alt, pairGPSPos.Key);

                                    // Calculate Accuracy
                                    switch (pairGPSPos.Value.FIXType)
                                    {
                                        case 2:
                                            currCamShot.Accuracy = CameraShot.MEDIUM_ACCURACY;
                                            break;
                                        case 5:
                                            currCamShot.Accuracy = CameraShot.LOW_ACCURACY;
                                            break;
                                    }

                                } catch(ArgumentOutOfRangeException eee)
                                {
                                    // Missing Sample
                                    currCamShot.Accuracy = CameraShot.MEDIUM_ACCURACY;
                                }
                                
                            }
                            // All magic happens here
                            if (LatInterpolator.ready() && LonInterpolator.ready() && AltInterpolator.ready())
                            {
                                currCamShot.PreciseLatGPSAntenna = LatInterpolator.value_estimate_spline(cam);
                                currCamShot.PreciseLonGPSAntenna = LonInterpolator.value_estimate_spline(cam);
                                currCamShot.PreciseAltGPSAntenna = AltInterpolator.value_estimate_spline(cam);
                            }
                            else
                            {
                                // We have to use Coarse Position if possible
                                currCamShot.PreciseLatGPSAntenna = currCamShot.CoarseLat;
                                currCamShot.PreciseLonGPSAntenna = currCamShot.CoarseLon;
                                currCamShot.PreciseAltGPSAntenna = currCamShot.CoarseAlt;
                                currCamShot.Accuracy = CameraShot.LOWEST_ACCURACY;
                            }

                            // Calculate Heading GPS
                            GPSPosition currPos = PosUnit.GPSPositions.ElementAt(i).Value;
                            currCamShot.GPSTrackYaw = HeadingCalculator.HeadingFromVelocities(currPos.Vn, currPos.Ve);

                            // Remove Camera already processed from list so we speed up nexts ones
                            camerasAux.Remove(key);

                            if (!CamUnit.IsDJI)
                            {
                                // Do AXIS CHANGE ---------------------------------------------------------------------
                                double[] coord1 = new double[3];

                                Notes = "Found Shot " + numCamFound + ". Applying rotations";

                                switch (OrientationMode)
                                {
                                    case OrientationType.FROM_CAM:
                                        coord1 = CoordinateChanger.TrasladarPunto(new double[] { currCamShot.PreciseLatGPSAntenna, currCamShot.PreciseLonGPSAntenna, currCamShot.PreciseAltGPSAntenna },
                                                                new double[] { CamUnit.XOffset1, CamUnit.YOffset1, CamUnit.ZOffset1 },
                                                                new double[] { currCamShot.RollB, currCamShot.PitchB, ForceGPSHeading ? currCamShot.GPSTrackYaw : currCamShot.YawB });
                                        break;
                                    case OrientationType.MANUAL_FIXED:
                                        coord1 = CoordinateChanger.TrasladarPunto(new double[] { currCamShot.PreciseLatGPSAntenna, currCamShot.PreciseLonGPSAntenna, currCamShot.PreciseAltGPSAntenna },
                                                                new double[] { CamUnit.XOffset1, CamUnit.YOffset1, CamUnit.ZOffset1 },
                                                                new double[] { ManualRoll, ManualPitch, ForceGPSHeading ? currCamShot.GPSTrackYaw : ManualYaw });
                                        break;
                                    case OrientationType.FROM_EXTERNAL:
                                        coord1 = CoordinateChanger.TrasladarPunto(new double[] { currCamShot.PreciseLatGPSAntenna, currCamShot.PreciseLonGPSAntenna, currCamShot.PreciseAltGPSAntenna },
                                                                new double[] { CamUnit.XOffset1, CamUnit.YOffset1, CamUnit.ZOffset1 },
                                                                new double[] { currCamShot.ManualRoll, currCamShot.ManualPitch, ForceGPSHeading ? currCamShot.GPSTrackYaw : currCamShot.ManualYaw });
                                        break;
                                }

                                if (CamFixingType == CAMFixingType.FIXED2BODY)
                                {
                                    currCamShot.PreciseLatNodalPoint = coord1[0];
                                    currCamShot.PreciseLonNodalPoint = coord1[1];
                                    currCamShot.PreciseAltNodalPoint = coord1[2];

                                    // Calculate distance between Camera and Antenna. Sanity Check
                                    //currCamShot.distanceCameraAntenna = CoordinateChanger.DistanceStraight3D(
                                    //                                                     currCamShot.PreciseLatNodalPoint,
                                    //                                                     currCamShot.PreciseLonNodalPoint,
                                    //                                                     currCamShot.PreciseAltNodalPoint,
                                    //                                                     currCamShot.PreciseLatGPSAntenna,
                                    //                                                     currCamShot.PreciseLonGPSAntenna,
                                    //                                                     currCamShot.PreciseAltGPSAntenna);

                                }
                                else
                                {
                                    double virtualRollatYAW;
                                    double virtualPitchatYAW;

                                    RollPitchRotation.Rotate(currCamShot.RollB, currCamShot.PitchB, currCamShot.YawH, out virtualRollatYAW, out virtualPitchatYAW);

                                    double[] coordRollMotor = CoordinateChanger.TrasladarPunto(new double[] { coord1[0], coord1[1], coord1[2] },
                                                                new double[] { CamUnit.XOffset2, CamUnit.YOffset2, CamUnit.ZOffset2 },
                                                                new double[] { virtualRollatYAW, virtualPitchatYAW, currCamShot.YawH });

                                    double[] coordPitchMotor = CoordinateChanger.TrasladarPunto(new double[] { coordRollMotor[0], coordRollMotor[1], coordRollMotor[2] },
                                                                new double[] { CamUnit.XOffset3, CamUnit.YOffset3, CamUnit.ZOffset3 },
                                                                new double[] { currCamShot.RollH, virtualPitchatYAW, currCamShot.YawH });

                                    double[] coordCameraNodalPoint = CoordinateChanger.TrasladarPunto(new double[] { coordPitchMotor[0], coordPitchMotor[1], coordPitchMotor[2] },
                                                                new double[] { CamUnit.XOffset4, CamUnit.YOffset4, CamUnit.ZOffset4 },
                                                                new double[] { currCamShot.RollH, currCamShot.PitchH, currCamShot.YawH });

                                    currCamShot.PreciseLatNodalPoint = coordCameraNodalPoint[0];
                                    currCamShot.PreciseLonNodalPoint = coordCameraNodalPoint[1];
                                    currCamShot.PreciseAltNodalPoint = coordCameraNodalPoint[2];
                                }

                                // ---------------------------------------------------------------------------------------
                            }
                            else
                            {
                                // DJI. Apply offset directly
                                double[] coordDJICam = CoordinateChanger.TrasladarPunto(new double[] { currCamShot.PreciseLatGPSAntenna, currCamShot.PreciseLonGPSAntenna, currCamShot.PreciseAltGPSAntenna },
                                                                new double[] { currCamShot.OffsetX, currCamShot.OffsetY, currCamShot.OffsetZ },
                                                                new double[] { 0, 0, 0 });

                                currCamShot.PreciseLatNodalPoint = coordDJICam[0];
                                currCamShot.PreciseLonNodalPoint = coordDJICam[1];
                                currCamShot.PreciseAltNodalPoint = coordDJICam[2];
                            }

                            // GEOTAG.....................................
                            if (HasToGeotag && currCamShot.fileOriginalUri != null && currCamShot.fileOriginalUri != "")
                            {
                                Notes = "Found Shot " + numCamFound + ". Geotagging ...";
                                string image_Name = Path.GetFileName(currCamShot.fileOriginalUri);
                                string dstImageUri = DrobitTools.ConcatenatePath(new string[] { GeotagImagesOutputFolder, image_Name });
                                ExifManipulationLibrary.ExifManipulator.SaveGPS2Image(currCamShot.fileOriginalUri, dstImageUri, currCamShot.PreciseLatNodalPoint, currCamShot.PreciseLonNodalPoint, currCamShot.PreciseAltNodalPoint);
                            }
                            // ...........................................
                            break;
                        }
                    }


                }
                
            }
            foreach (CameraShot cam in camerasAux.Values)
            {
                // Copy Coarse to Precise and Accuracy to lowest
                // We have to use Coarse Position if possible
                cam.PreciseLatGPSAntenna = cam.CoarseLat;
                cam.PreciseLonGPSAntenna = cam.CoarseLon;
                cam.PreciseAltGPSAntenna = cam.CoarseAlt;
                cam.PreciseLatNodalPoint = cam.CoarseLat;
                cam.PreciseLonNodalPoint = cam.CoarseLon;
                cam.PreciseAltNodalPoint = cam.CoarseAlt;

                cam.Accuracy = CameraShot.LOWEST_ACCURACY;
            }

            // Save GPSInterP to file
            SaveCameras2File(CamUnit.CameraShots, OutputsFolder, "gpsAntennaPositions.txt",  "NodalCameraPositions.txt", (InputParameter as RTKPOSUnit));

            // Save KML
            KMLCreator.CreateKML3D(CamUnit.CameraShots, OutputsFolder, "CameraPositions.kml", AltitudeOffset, ModelScale, KMLOrientationSRCBody, PitchOffset);

            Percentage = 100;
            Phase = WorkPhase.COMPLETED;

            Notes = "Generating Statistics";

            // Analyze Pos File and Statistics
            foreach (CameraShot cam in CamUnit.CameraShots.Values)
            {
                if (cam.Accuracy == CameraShot.HIGH_ACCURACY)
                    HighQCameras++;
                else if (cam.Accuracy == CameraShot.MEDIUM_ACCURACY)
                    MedQCameras++;
                else if (cam.Accuracy == CameraShot.LOW_ACCURACY)
                    LowQCameras++;
                else if (cam.Accuracy == CameraShot.LOWEST_ACCURACY)
                    LowestQCameras++;
            }

            Notes = "Finished";
        }

        private string GetCameraOutputFileHeader(string header, RTKPOSUnit posUnit)
        {
            return header + " || (Base Position: " + posUnit.RefPosLat.ToString(CultureInfo.InvariantCulture) + " , " + posUnit.RefPosLon.ToString(CultureInfo.InvariantCulture) + " , " + posUnit.RefPosAlt.ToString(CultureInfo.InvariantCulture) + " )";
        }

        private void SaveCameras2File(SortedDictionary<long, CameraShot> camGpsInterP, string outputsFolder, string filenameGPS, string filenameCamera, RTKPOSUnit posUnit)
        {

            string fullPathGPS = DrobitTools.ConcatenatePath(new string[] { outputsFolder, filenameGPS });
            string fullPathCamera = DrobitTools.ConcatenatePath(new string[] { outputsFolder, filenameCamera });
            StreamWriter fileGPS = null;
            StreamWriter fileCamera = null;
            try
            {
                fileGPS = new StreamWriter(fullPathGPS);
                fileCamera = new StreamWriter(fullPathCamera);

                fileGPS.WriteLine(GetCameraOutputFileHeader(GPS_FILE_OUTPUT_HEADER, posUnit));
                fileCamera.WriteLine(GetCameraOutputFileHeader(CAM_FILE_OUTPUT_HEADER, posUnit));


                foreach (CameraShot cam in camGpsInterP.Values)
                {
                    fileGPS.WriteLine(CameraShot2Line(cam, false ));
                    fileCamera.WriteLine(CameraShot2Line(cam, true));
                }

                fileGPS.Close();
                fileCamera.Close();
            }
            catch (Exception) { }
        }

        private string GetCameraName(CameraShot cam)
        {
            string fileUri = "";

            if (cam.fileOriginalUri != null && cam.fileOriginalUri != "")
                fileUri = cam.fileOriginalUri;

            if (cam.Name != "N/A")
                return cam.Name;
            else
            {
                return Path.GetFileName(fileUri);
            }
        }

        private string CameraShot2Line(CameraShot cam, bool cameraNodalPointPosition)
        {
            StringBuilder lineB = new StringBuilder();
            lineB.Append(GetCameraName(cam)).Append(";");

            if (cameraNodalPointPosition)
            {
                lineB.Append(cam.PreciseLonNodalPoint.ToString(CultureInfo.InvariantCulture)).Append(";");
                lineB.Append(cam.PreciseLatNodalPoint.ToString(CultureInfo.InvariantCulture)).Append(";");
                lineB.Append(cam.PreciseAltNodalPoint.ToString(CultureInfo.InvariantCulture)).Append(";");
            }
            else
            {
                lineB.Append(cam.PreciseLonGPSAntenna.ToString(CultureInfo.InvariantCulture)).Append(";");
                lineB.Append(cam.PreciseLatGPSAntenna.ToString(CultureInfo.InvariantCulture)).Append(";");
                lineB.Append(cam.PreciseAltGPSAntenna.ToString(CultureInfo.InvariantCulture)).Append(";");
            }

            double actualRoll;
            double actualPitch;
            double actualYaw;

            switch (OrientationMode)
            {
                case OrientationType.FROM_CAM:
                    actualRoll = cam.RollB;
                    actualPitch = cam.PitchB;
                    actualYaw = ForceGPSHeading ? cam.GPSTrackYaw : cam.YawB;
                    break;
                case OrientationType.MANUAL_FIXED:
                    actualRoll = ManualRoll;
                    actualPitch = ManualPitch;
                    actualYaw = ForceGPSHeading? cam.GPSTrackYaw: ManualYaw;
                    break;
                case OrientationType.FROM_EXTERNAL:
                    actualRoll = cam.ManualRoll;
                    actualPitch = cam.ManualPitch;
                    actualYaw = ForceGPSHeading ? cam.GPSTrackYaw : cam.ManualYaw;
                    break;
                default:
                    actualRoll = 0d;
                    actualPitch = 0d;
                    actualYaw = 0d;
                    break;
            }

            lineB.Append(actualYaw.ToString(CultureInfo.InvariantCulture)).Append(";");
            lineB.Append(actualPitch.ToString(CultureInfo.InvariantCulture)).Append(";");
            lineB.Append(actualRoll.ToString(CultureInfo.InvariantCulture)).Append(";");

            lineB.Append(cam.Accuracy.ToString(CultureInfo.InvariantCulture)).Append(";");

            lineB.Append(cam.shutterDelay.ToString(CultureInfo.InvariantCulture)).Append(";");

            lineB.Append(cam.GPSWeek).Append(";");
            lineB.Append(cam.GPSSecond.ToString(CultureInfo.InvariantCulture));

            if (cameraNodalPointPosition)
            {
                lineB.Append(";");
                // Write Camera hotshoe angles. Just in case they are usefull
                lineB.Append(cam.RollH.ToString(CultureInfo.InvariantCulture)).Append(";");
                lineB.Append(cam.PitchH.ToString(CultureInfo.InvariantCulture)).Append(";");
                lineB.Append(cam.YawH.ToString(CultureInfo.InvariantCulture));
            }

            return lineB.ToString();
        }

        public static string GenerateCAMLine(long timeUTCFromEpoch, string formatedDateTime, string namePic, int gpsWeek, double gpsSeconds, float rollB, float pitchB, float yawB, float rollH, float pitchH, float yawH)
        {
            StringBuilder line = new StringBuilder();
            line.Append(timeUTCFromEpoch).Append(SEPARATOR);
            line.Append(timeUTCFromEpoch).Append(SEPARATOR);
            line.Append(timeUTCFromEpoch).Append(SEPARATOR);
            line.Append(formatedDateTime).Append(SEPARATOR);
            line.Append(namePic).Append(SEPARATOR);
            line.Append(gpsWeek).Append(SEPARATOR);
            line.Append(gpsSeconds.ToString(CultureInfo.InvariantCulture)).Append(SEPARATOR);
            line.Append(rollB.ToString(CultureInfo.InvariantCulture)).Append(SEPARATOR);
            line.Append(pitchB.ToString(CultureInfo.InvariantCulture)).Append(SEPARATOR);
            line.Append(yawB.ToString(CultureInfo.InvariantCulture)).Append(SEPARATOR);
            line.Append("0.0").Append(SEPARATOR);
            line.Append("0.0").Append(SEPARATOR);
            line.Append("0.0").Append(SEPARATOR);
            line.Append(rollH.ToString(CultureInfo.InvariantCulture)).Append(SEPARATOR);
            line.Append(pitchH.ToString(CultureInfo.InvariantCulture)).Append(SEPARATOR);
            line.Append(yawH.ToString(CultureInfo.InvariantCulture));

            return line.ToString();
        }

        public static bool ProcessRINEXTriggers(RINEXUnit rUnit, string outputFilePath)
        {
            StreamWriter fileCam = null ;
            bool foundTrigers = false;

            // Only do it if exists and UBX
            if (rUnit != null && rUnit.SourceFileType == GNSS_FILE_TYPE.UBLOX && File.Exists(rUnit.ObsFileUri))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(rUnit.ObsFileUri);
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        Match mTrigger = Regex.Match(line, TIME_MARK_EVENT_REGEX);
                        if (mTrigger.Groups.Count > 6)
                        {
                            if (!foundTrigers)
                            {
                                fileCam = new StreamWriter(outputFilePath);
                                fileCam.WriteLine("Drobit CAM File v2.0");
                                fileCam.WriteLine(@"0.1;0.1;0.1;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0;0.0");
                                foundTrigers = true;
                            }
                            else
                            {
                                int year = int.Parse(mTrigger.Groups[1].Value, CultureInfo.InvariantCulture);
                                int month = int.Parse(mTrigger.Groups[2].Value, CultureInfo.InvariantCulture);
                                int day = int.Parse(mTrigger.Groups[3].Value, CultureInfo.InvariantCulture);
                                int hour = int.Parse(mTrigger.Groups[4].Value, CultureInfo.InvariantCulture);
                                int minute = int.Parse(mTrigger.Groups[5].Value, CultureInfo.InvariantCulture);
                                double second = double.Parse(mTrigger.Groups[6].Value, CultureInfo.InvariantCulture);

                                int gpsWeek;
                                double gpsSeconds;

                                DrobitTools.GetGPSFromRINEX(year, month, day, hour, minute, second, out gpsWeek, out gpsSeconds);
                                DateTime triggerTimeUTC = DrobitTools.GetTimeFromGps(gpsWeek, gpsSeconds, true);


                                fileCam.WriteLine(GenerateCAMLine(DrobitTools.GetMillisFromEpoch(triggerTimeUTC), triggerTimeUTC.ToString(), "N/A", gpsWeek, gpsSeconds, 0, 0, 0, 0, 0, 0));
                            }

                        }
                    }
                }
                catch (Exception)
                {
                    foundTrigers = false;
                }
                finally
                {
                    try
                    {
                        fileCam.Close();
                        file.Close();
                    }
                    catch { }
                }
            }
            return foundTrigers;
        }

        //private bool IsCameraOK(CameraShot cam, SortedDictionary<long, GPSPosition> gPSPositions, ref List<GPSPosition> position4Camera, int NumPositions, long millisBTSamples)
        //{
        //    long millisCamera = cam.MicrosTime / 1000;
        //    long millisMAXDistance = MAX_SAMPLES_DISTANCE * millisBTSamples;

        //    position4Camera.Clear();

        //    // Look for positions
        //    for (int i = 0; i < gPSPositions.Count; i++)
        //    {
        //        long microsGPS = gPSPositions.ElementAt(i).Key;
        //        long millisGPS = microsGPS / 1000;
        //        if (Math.Abs(millisCamera - millisGPS) <= millisMAXDistance)
        //        {
        //            // There we are
        //            for (int u = i - (NumPositions / 2); u < i + (NumPositions / 2); u++)
        //            {
        //                position4Camera.Add(gPSPositions.ElementAt(u).Value);
        //            }
        //            return true;
        //        }
        //        else
        //        {
        //            if (millisGPS - millisCamera > millisMAXDistance)
        //            {
        //                // We are far. Not found. Break;
        //                return false;
        //            }
        //        }
        //    }

        //    return false;

        //}

        #endregion
    }
}
