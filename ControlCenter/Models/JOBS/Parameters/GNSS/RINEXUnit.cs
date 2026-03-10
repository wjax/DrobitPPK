using ControlCenter.GNSSProcessingEngine;
using ControlCenter.Models.JOBS.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ControlCenter.GNSSProcessingEngine.RINEXConverter;

namespace ControlCenter.Models.JOBS.Parameters.GNSS
{
    public class RINEXUnit : JobParameter
    {
        public const string OBSFILE_REGEX_EXT = @"(?i:\.obs|\.[0-9]{2}[o,d]+)"; //@"\.obs|\.OBS|\.15O|\.17D|\.15o|\.17d|\.17o|\.16d|\.16D|\.16o|\.16O";
        public const string NAVFILE_REGEX_EXT = @"(?i:\.nav|\.gnav|\.[0-9]{2}[n,g]+)";//@"\.nav|\.NAV|\.15N|\.gnav|\.GNAV|\.17N|\.17G|\.17n|\.17g|\.15n|\.15G|\.15g|\.16N|\.16G|\.16n|\.16g";
        public const string SBASFILE_REGEX_EXT = @"\.sbs|\.SBS";

        public const string DROBIT_FILE_REGEX = @"(?i:\.gnss)";
        public const string UBLOX_FILE_REGEX = @"(?i:\.ubx)";
        public const string TOPCON_FILE_REGEX = @"(?i:\.tps)";

        public RINEXUnit()
        {
            reset();
            Type = "";
            ContainsTrigger = false;
        }

        // Name
        private string name;
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        // Start time
        private DateTime startTime;
        public DateTime StartTime
        {
            get { return startTime; }
            set {
                Set(ref startTime, value);
            }
        }

        // Stop time
        private DateTime stopTime;
        public DateTime StopTime
        {
            get { return stopTime; }
            set {
                Set(ref stopTime, value);
            }
        }

        // OBS File
        private string obsFileUri;
        public string ObsFileUri
        {
            get { return obsFileUri; }
            set { Set(ref obsFileUri, value); }
        }

        //NAV Files
        public ObservableCollection<string> NavFileUris { get; set; } = new ObservableCollection<string>();

        // Obs Number
        private long obsNumber;
        public long ObsNumber
        {
            get { return obsNumber; }
            set { Set(ref obsNumber, value); }
        }

        // MeasurePeriod
        private float measPeriod;
        public float MeasPeriod
        {
            get { return measPeriod; }
            set { Set(ref measPeriod, value); }
        }

        // SatCount
        private int satCount;
        public int SatCount
        {
            get { return satCount; }
            set { Set(ref satCount, value); }
        }

        // Type, BASE or ROVER
        private string type;
        public string Type
        {
            get { return type; }
            set { Set(ref type, value); }
        }

        // UBX with trigger info
        private bool containsTrigger;
        public bool ContainsTrigger
        {
            get { return containsTrigger; }
            set { Set(ref containsTrigger, value); }
        }

        // UBX with trigger info
        private string camFromRinexFilePath;
        public string CAMFromRinexFilePath
        {
            get { return camFromRinexFilePath; }
            set { Set(ref camFromRinexFilePath, value); }
        }

        private GNSS_FILE_TYPE sourceFileType;
        public GNSS_FILE_TYPE SourceFileType
        {
            get { return sourceFileType; }
            set { Set(ref sourceFileType, value); }
        }


        public void reset()
        {
            Name = "Drop File";
            StartTime = DateTime.Parse("2000-01-01T12:00:00");
            StopTime = DateTime.Parse("2000-01-01T12:00:00");
            ObsFileUri = "";
            NavFileUris.Clear();
            MeasPeriod = 0;
            SatCount = 0;
            //Type = "BASE";
            JobParamType = JobParameterType.RINEXUNIT;
            ContainsTrigger = false;
            SourceFileType = GNSS_FILE_TYPE.DROBIT;
            CAMFromRinexFilePath = "";
        }

    }
}
