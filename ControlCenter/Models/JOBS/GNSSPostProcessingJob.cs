using ControlCenter.Extras;
using ControlCenter.GNSSProcessingEngine;
using ControlCenter.Models.JOBS.Parameters;
using ControlCenter.Models.JOBS.Parameters.GNSS;
using DrobitExtras;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCenter.Models.JOBS
{


    #region ENUMS
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ProcessingElevationMASK
    {
        [Description("10")]
        D10,
        [Description("15")]
        D15,
        [Description("20")]
        D20,
        [Description("25")]
        D25,
        [Description("30")]
        D30,
        [Description("35")]
        D35,
        [Description("40")]
        D40
    }
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ProcessingINTERVAL
    {
        [Description("0.05")]
        HZ20,
        [Description("0.10")]
        HZ10,
        [Description("0.20")]
        HZ5,
        [Description("1")]
        HZ1
    }
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ProcessingOptions
    {
        [Description("GPS+GLONASS")]
        STANDARD,
        [Description("Only GPS")]
        ALTERNATE
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AltitudeModeTYPE
    {
        [Description("ELLIPSOIDAL")]
        ELLIPSOIDAL,
        [Description("GEOID EGM96")]
        GEOID
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum GNSSProcessingTYPE
    {
        [Description("GNSS POST STATIC")]
        STATIC,
        [Description("GNSS POST KINEMATIC")]
        KINEMATIC
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum BASEPositionTYPE
    {
        [Description("From Previous Task")]
        FROM_PREVIOUS,
        [Description("From RINEX")]
        FROM_RINEX,
        [Description("From Average Single")]
        FROM_SINGLE,
        [Description("From User Input")]
        FROM_USERINPUT
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ResultValidity
    {
        [Description("Solution is valid")]
        VALID,
        [Description("Solution is NOT valid")]
        NOT_VALID,
        [Description("")]
        UNDEF
    }
    #endregion


    #region CLASS
    public class GNSSPostProcessingJob : IWorkJob
    {
        private const double EXTRATIME = 10;

        private const string INPUT_FOLDER = "INPUT";
        private const string OUTPUT_FOLDER = "OUTPUT";
        private const string OUTPUT_FILENAME = "computed_positions.pos";
        private const string REGEX_PROCESS = "^processing : (.*) (.*) Q=(.*)$";

        string outputFilePath;

        private PostProcessManager ppk;

        public GNSSPostProcessingJob(string parentFolder)
        {
            BaseGNSS = new RINEXUnit();
            BaseGNSS.Type = "BASE";
            //BaseGNSS.PropertyChanged += GNSS_PropertyChanged;
            RoverGNSS = new RINEXUnit();
            RoverGNSS.Type = "ROVER";
            //RoverGNSS.PropertyChanged += GNSS_PropertyChanged;

            GNSSProcessingType = GNSSProcessingTYPE.KINEMATIC;
            ProcessingOption = ProcessingOptions.STANDARD;
            AltitudeMode = AltitudeModeTYPE.ELLIPSOIDAL;
            Worktype = WorkType.GNSSKINEMATIC;

            ProcessingInterval = ProcessingINTERVAL.HZ20;

            ProcessingMask = ProcessingElevationMASK.D15;

            this.parentFolder = parentFolder;

            ResultValidStatus = ResultValidity.UNDEF;

        }

        public void Initialize()
        {
            // Create and associate working folder
            Name = Worktype + "_" + Guid.NewGuid().ToString();
            WorkingFolder = DrobitTools.ConcatenatePath(new string[] { parentFolder, Name });
            DrobitTools.CreateFolderIfNotExists(WorkingFolder);

            InputsFolder = DrobitTools.ConcatenatePath(new string[] { WorkingFolder, INPUT_FOLDER });
            DrobitTools.CreateFolderIfNotExists(InputsFolder);
            OutputsFolder = DrobitTools.ConcatenatePath(new string[] { WorkingFolder, OUTPUT_FOLDER });
            DrobitTools.CreateFolderIfNotExists(OutputsFolder);

            Percentage = 0;

        }

        private void GNSS_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (Regex.IsMatch(e.PropertyName, "MeasPeriod"))
            //{
            //    RecalculateObsMaxMinTimes();
            //    System.Diagnostics.Debug.WriteLine("Min: " + MinOBSTime.ToString() + " -- Max: " + MaxOBSTime.ToString());
                    
            //}
        }

        private RINEXUnit baseGNSS;
        [JsonProperty("BaseGNSS")]
        public RINEXUnit BaseGNSS
        {
            get { return baseGNSS; }
            set {
                Set(ref baseGNSS, value);
            }
        }


        private RINEXUnit roverGNSS;
        [JsonProperty("RoverGNSS")]
        public RINEXUnit RoverGNSS
        {
            get { return roverGNSS; }
            set {
                Set(ref roverGNSS, value);
            }
        }

        public void RecalculateObsMaxMinTimes()
        {
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;

            DateTime innerMin = DateTime.MinValue;
            DateTime innerMax = DateTime.MaxValue;

            if (!BaseGNSS.StartTime.Equals(DateTime.Parse("2000-01-01T12:00:00")) && !BaseGNSS.StopTime.Equals(DateTime.Parse("2000-01-01T12:00:00")))
            {
                // Base is set
                innerMin = min = BaseGNSS.StartTime;
                innerMax = max = BaseGNSS.StopTime;

            }

            if (!RoverGNSS.StartTime.Equals(DateTime.Parse("2000-01-01T12:00:00")) && !RoverGNSS.StopTime.Equals(DateTime.Parse("2000-01-01T12:00:00")))
            {
                if (RoverGNSS.StartTime.CompareTo(min) < 0)
                    min = RoverGNSS.StartTime;
                //else
                //    innerMin = RoverGNSS.StartTime;
                if (RoverGNSS.StopTime.CompareTo(max) > 0)
                    max = RoverGNSS.StopTime;
                //else
                //    innerMax = RoverGNSS.StopTime;

                innerMin = RoverGNSS.StartTime;
                innerMax = RoverGNSS.StopTime;

                // Set times to rover
                //RequiredProcessStartTime = RoverGNSS.StartTime;
                //RequiredProcessStopTime = RoverGNSS.StopTime;
            }

            RequiredProcessStartTime = innerMin;
            RequiredProcessStopTime = innerMax;

            MinOBSTime = min;
            MaxOBSTime = max;

            //UpdateProposedProcessingTimes();


        }

        private void UpdateProposedProcessingTimes()
        {
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;

            if (!BaseGNSS.StartTime.Equals(DateTime.Parse("2000-01-01T12:00:00")) && !BaseGNSS.StopTime.Equals(DateTime.Parse("2000-01-01T12:00:00")))
            {
                // Base is set
                min = BaseGNSS.StartTime;
                max = BaseGNSS.StopTime;
            }
            if (!RoverGNSS.StartTime.Equals(DateTime.Parse("2000-01-01T12:00:00")) && !RoverGNSS.StopTime.Equals(DateTime.Parse("2000-01-01T12:00:00")))
            {
                if (RoverGNSS.StartTime.CompareTo(min) > 0)
                {
                    min = RoverGNSS.StartTime;
                }
                if (RoverGNSS.StopTime.CompareTo(max) < 0)
                {
                    max = RoverGNSS.StopTime;
                }
            }

            // Update Times for processing
            if (!min.Equals(DateTime.MaxValue))
                RequiredProcessStartTime = min.AddSeconds(EXTRATIME);
            if (!max.Equals(DateTime.MinValue))
                RequiredProcessStopTime = max.AddSeconds(-1*EXTRATIME);

        }


        private GNSSProcessingTYPE gNSSProcessingType;
        [JsonProperty("GNSSProcessingType")]
        public GNSSProcessingTYPE GNSSProcessingType {
            get { return gNSSProcessingType; }
            set
            {
                Set(ref gNSSProcessingType, value);
                switch (gNSSProcessingType)
                {
                    case GNSSProcessingTYPE.KINEMATIC:
                        Worktype = WorkType.GNSSKINEMATIC;
                        ProcessingInterval = ProcessingINTERVAL.HZ20;
                        break;
                    case GNSSProcessingTYPE.STATIC:
                        Worktype = WorkType.GNSSSTATIC;
                        ProcessingInterval = ProcessingINTERVAL.HZ1;
                        break;
                }
            }
        }

        private BASEPositionTYPE bASEPositionType;
        [JsonProperty("BASEPositionType")]
        public BASEPositionTYPE BASEPositionType {
            get { return bASEPositionType; }
            set
            {
                Set(ref bASEPositionType, value);
                NeedInputParameterFromPrevious = value == BASEPositionTYPE.FROM_PREVIOUS ? true : false;
            }
        }

        private ResultValidity resultValidStatus;
        [JsonProperty("ResultValidStatus")]
        public ResultValidity ResultValidStatus
        {
            get { return resultValidStatus; }
            set
            {
                Set(ref resultValidStatus, value);
            }
        }

        private double customBaseLat;
        [JsonProperty("CustomBaseLat")]
        public double CustomBaseLat
        {
            get { return customBaseLat; }
            set
            {
                Set(ref customBaseLat, value);
            }
        }
        private double customBaseLon;
        [JsonProperty("CustomBaseLon")]
        public double CustomBaseLon
        {
            get { return customBaseLon; }
            set
            {
                Set(ref customBaseLon, value);
            }
        }
        private double customBaseAlt;
        [JsonProperty("CustomBaseAlt")]
        public double CustomBaseAlt
        {
            get { return customBaseAlt; }
            set
            {
                Set(ref customBaseAlt, value);
            }
        }

        private ProcessingINTERVAL processingInterval;
        [JsonProperty("ProcessingInterval")]
        public ProcessingINTERVAL ProcessingInterval
        {
            get { return processingInterval; }
            set
            {
                Set(ref processingInterval, value);
            }
        }

        private ProcessingElevationMASK processingMask;
        [JsonProperty("ProcessingMask")]
        public ProcessingElevationMASK ProcessingMask
        {
            get { return processingMask; }
            set
            {
                Set(ref processingMask, value);
            }
        }

        private ProcessingOptions processingOption;
        [JsonProperty("ProcessingOption")]
        public ProcessingOptions ProcessingOption {
            get { return processingOption; }
            set
            {
                Set(ref processingOption, value);
            }
        }

        private AltitudeModeTYPE altitudeMode;
        [JsonProperty("AltitudeMode")]
        public AltitudeModeTYPE AltitudeMode {
            get { return altitudeMode; }
            set
            {
                Set(ref altitudeMode, value);
            }
        }

        private DateTime requiredProcessStartTime;
        [JsonProperty("RequiredProcessStartTime")]
        public DateTime RequiredProcessStartTime {
            get { return requiredProcessStartTime; }
            set
            {
                Set(ref requiredProcessStartTime, value);
            }
        }

        private DateTime requiredProcessStopTime;
        [JsonProperty("RequiredProcessStopTime")]
        public DateTime RequiredProcessStopTime {
            get { return requiredProcessStopTime; }
            set
            {
                Set(ref requiredProcessStopTime, value);
            }
        }

        private DateTime minOBSTime;
        [JsonProperty("MinOBSTime")]
        public DateTime MinOBSTime {
            get { return minOBSTime; }
            set
            {
                Set(ref minOBSTime, value);
            }
        }

        private DateTime maxOBSTime;
        [JsonProperty("MaxOBSTime")]
        public DateTime MaxOBSTime {
            get { return maxOBSTime; }
            set
            {
                Set(ref maxOBSTime, value);
            }
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

        [JsonProperty("parentFolder")]
        public string parentFolder;


        private long numPoints;
        [JsonProperty("NumPoints")]
        public long NumPoints
        {
            get { return numPoints; }
            set { Set(ref numPoints, value); }
        }

        private int q1Percent;
        [JsonProperty("Q1Percent")]
        public int Q1Percent
        {
            get { return q1Percent; }
            set { Set(ref q1Percent, value); }
        }

        #endregion

        #region WORK_OVERRIDES

        public override void StopJob(object obj = null)
        {
            base.StopJob();
           
            
        }

        public override void StartJob(object obj = null)
        {
            System.Diagnostics.Debug.WriteLine("Started GNSSJob: " + Name);
            Phase = WorkPhase.RUNNING;

            // Empty Output dir
            DrobitTools.EmptyDirectory(OutputsFolder);
            
            // Postprocessing
            ppk = new PostProcessManager();

            // Prepare parameters
            bool rtk_static = GNSSProcessingType == GNSSProcessingTYPE.KINEMATIC ? true : false;
            bool ellip_geoid = AltitudeMode == AltitudeModeTYPE.ELLIPSOIDAL ? true : false;
            bool rinex_average = BASEPositionType == BASEPositionTYPE.FROM_RINEX ? true : false;
            bool normal_alternative = ProcessingOption == ProcessingOptions.STANDARD ? true : false;

            float interval = float.Parse(DrobitTools.GetDescription(ProcessingInterval), CultureInfo.InvariantCulture);
            int elevMask = int.Parse(DrobitTools.GetDescription(ProcessingMask), CultureInfo.InvariantCulture);

            double[] userBasePosition = null;
            if (BASEPositionType == BASEPositionTYPE.FROM_USERINPUT)
                userBasePosition = new double[] { CustomBaseLat, CustomBaseLon, CustomBaseAlt };
            if (BASEPositionType == BASEPositionTYPE.FROM_PREVIOUS)
            {
                if (InputParameter != null)
                    userBasePosition = new double[] { (InputParameter as RTKPOSUnit).MeanLat, (InputParameter as RTKPOSUnit).MeanLon, (InputParameter as RTKPOSUnit).MeanAlt };
                else
                    userBasePosition = new double[] { 0d, 0d, 0d};
            }
                
            //userBasePosition = new double[] { (InputParameter as RTKPOSUnit).MeanLat, (InputParameter as RTKPOSUnit).MeanLon, (InputParameter as RTKPOSUnit).MeanAlt };

            outputFilePath = DrobitTools.ConcatenatePath(new string[] { OutputsFolder, OUTPUT_FILENAME });
            // Initialize PPK
            ppk.Initialize(normal_alternative, interval, elevMask, rtk_static, rinex_average, ellip_geoid, BaseGNSS, RoverGNSS, outputFilePath, RequiredProcessStartTime, RequiredProcessStopTime, userBasePosition);

            //ThreadPool.QueueUserWorkItem(new WaitCallback(DoProcessJob), new object[] { ppk, this });

            DoProcessJob( ppk);

        }

        private void DoProcessJob(PostProcessManager ppk)
        {
            //PostProcessManager ppk = param[0] as PostProcessManager;
            ppk.Start();

            // Progress time estimation
            TimeSpan tSpan = RequiredProcessStopTime - RequiredProcessStartTime;
            int seconds = (int)tSpan.TotalSeconds*2;

            // Seconds that have been processed by rtklib
            int processedSeconds = 0;
            string lastProcessedTime = "";

            Process proc = ppk.getProcess();

            // Read standardoutput
            StreamReader outReader = proc.StandardError;
            string str;
            int nQ1 = 0;
            int rawPercent = 0;
            long rawPoints = 0;
            NumPoints = 0;


            while (!outReader.EndOfStream)
            {
                if (needsExit)
                {
                    ppk?.Stop();
                    StoppedJob();
                    return;
                }
                // Read line
                str = outReader.ReadLine();
                System.Diagnostics.Debug.WriteLine(str);
                // Percent calc
                rawPercent = (processedSeconds * 100) / seconds;
                // Top saturate
                Percentage = rawPercent <= 100 ? rawPercent : 100;

                System.Diagnostics.Debug.WriteLine("Processing Drobit: " + str);
                Match mQ = Regex.Match(str, REGEX_PROCESS);
                if (mQ.Groups.Count > 2)
                {
                    // Num lines read increase
                    rawPoints++;
                    // Number of Q1
                    if (int.Parse(mQ.Groups[3].Value) == 1)
                        nQ1++;
                    // Time elapsed 
                    if (!mQ.Groups[2].Value.Equals(lastProcessedTime))
                    {
                        lastProcessedTime = mQ.Groups[2].Value;
                        processedSeconds++;
                    }
                }

                if ((rawPoints % 50) == 0)
                    NumPoints = rawPoints/2;
            }

            // Unfold from forward and backwards
            NumPoints = rawPoints/2;
            nQ1 /= 2;
            System.Diagnostics.Debug.WriteLine("Finished proccessing - awaiting for exit ");
            // Wait for Process to Exit
            proc.WaitForExit();

            System.Diagnostics.Debug.WriteLine("Finished proccessing - 100% ");

            // Analyze Pos File and Statistics
            OutputParameter = RTKOutputFileParser.ParseFile(outputFilePath);

            if (OutputParameter != null && (OutputParameter as RTKPOSUnit).Q1per100 > 50)
                ResultValidStatus = ResultValidity.VALID;
            else
                ResultValidStatus = ResultValidity.NOT_VALID;

            Percentage = 100;
            Phase = WorkPhase.COMPLETED;

        }

        #endregion
    }



}

