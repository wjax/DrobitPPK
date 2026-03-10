using ControlCenter.Models.JOBS.Parameters;
using DrobitExtras;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter.Models.JOBS.Parameters.GNSS
{
    public class CAMUnit : JobParameter
    {
        public const string CAMFILE_DROBIT_REGEX_EXT = @"\.cam";
        public const string CAMFILE_DJI_REGEX_EXT = @"\.mrk";

        // List of Camera Shots. Key is TimeMillis
        public  SortedDictionary<long, CameraShot> CameraShots { get; set; } = new SortedDictionary<long, CameraShot>();

        // DJI
        private bool isDJI;
        public bool IsDJI
        {
            get => isDJI;
            set => Set(ref isDJI, value);
        }

        // Start time
        private DateTime startTime;
        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                Set(ref startTime, value);
            }
        }

        // Stop time
        private DateTime stopTime;
        public DateTime StopTime
        {
            get { return stopTime; }
            set
            {
                Set(ref stopTime, value);
            }
        }

        // Name
        private string name;
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        // Type for Badge presentation
        private string type;
        public string Type
        {
            get { return type; }
            set { Set(ref type, value); }
        }

        // Path
        private string path;
        public string Path
        {
            get { return path; }
            set { Set(ref path, value); }
        }

        // Num Cameras
        private long numCameras;
        public long NumCameras
        {
            get { return numCameras; }
            set { Set(ref numCameras, value); }
        }

        // X
        private double xOffset1;
        public double XOffset1
        {
            get { return xOffset1; }
            set { Set(ref xOffset1, value); }
        }

        // Y
        private double yOffset1;
        public double YOffset1
        {
            get { return yOffset1; }
            set { Set(ref yOffset1, value); }
        }

        // Z
        private double zOffset1;
        public double ZOffset1
        {
            get { return zOffset1; }
            set { Set(ref zOffset1, value); }
        }

        // X
        private double xOffset2;
        public double XOffset2
        {
            get { return xOffset2; }
            set { Set(ref xOffset2, value); }
        }

        // Y
        private double yOffset2;
        public double YOffset2
        {
            get { return yOffset2; }
            set { Set(ref yOffset2, value); }
        }

        // Z
        private double zOffset2;
        public double ZOffset2
        {
            get { return zOffset2; }
            set { Set(ref zOffset2, value); }
        }

        // X
        private double xOffset3;
        public double XOffset3
        {
            get { return xOffset3; }
            set { Set(ref xOffset3, value); }
        }

        // Y
        private double yOffset3;
        public double YOffset3
        {
            get { return yOffset3; }
            set { Set(ref yOffset3, value); }
        }

        // Z
        private double zOffset3;
        public double ZOffset3
        {
            get { return zOffset3; }
            set { Set(ref zOffset3, value); }
        }

        // X
        private double xOffset4;
        public double XOffset4
        {
            get { return xOffset4; }
            set { Set(ref xOffset4, value); }
        }

        // Y
        private double yOffset4;
        public double YOffset4
        {
            get { return yOffset4; }
            set { Set(ref yOffset4, value); }
        }

        // Z
        private double zOffset4;
        public double ZOffset4
        {
            get { return zOffset4; }
            set { Set(ref zOffset4, value); }
        }

        // Camera fixing type
        //private CAMFixingType camFixingType;
        //public CAMFixingType CamFixingType
        //{
        //    get { return camFixingType; }
        //    set { Set(ref camFixingType, value); }
        //}


        public CAMUnit()
        {
            this.reset();
        }

        public void reset()
        {
            Name = "Drop File";
            StartTime = DateTime.Parse("2000-01-01T12:00:00");
            StopTime = DateTime.Parse("2000-01-01T12:00:00");
            Path = "";
            Type = "CAM";
            JobParamType = JobParameterType.DROBITCAMUNIT;
            CameraShots.Clear();
        }
    }
}
