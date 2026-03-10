using ControlCenter.Models.JOBS.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter.Models.JOBS.Parameters.GNSS
{
    public class RTKPOSUnit : JobParameter
    {
        public SortedDictionary<long, GPSPosition> GPSPositions { get; set; } = new SortedDictionary<long, GPSPosition>();

        // Type for Badge presentation
        private string type;
        public string Type
        {
            get { return type; }
            set { Set(ref type, value); }
        }

        // Name
        private string name;
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        // Path
        private string path;
        public string Path
        {
            get { return path; }
            set { Set(ref path, value); }
        }

        // Num Positions
        private long numPositions;
        public long NumPositions
        {
            get { return numPositions; }
            set { Set(ref numPositions, value); }
        }

        // Q1
        private long q1;
        public long Q1
        {
            get { return q1; }
            set { Set(ref q1, value); }
        }

        // Q2
        private long q2;
        public long Q2
        {
            get { return q2; }
            set { Set(ref q2, value); }
        }

        // Q5
        private long q5;
        public long Q5
        {
            get { return q5; }
            set { Set(ref q5, value); }
        }

        // Q1 Pe3rcent
        private int q1per100;
        public int Q1per100
        {
            get { return q1per100; }
            set { Set(ref q1per100, value); }
        }

        // MeanLat
        private double meanLat;
        public double MeanLat
        {
            get { return meanLat; }
            set { Set(ref meanLat, value); }
        }

        // MeanLon
        private double meanLon;
        public double MeanLon
        {
            get { return meanLon; }
            set { Set(ref meanLon, value); }
        }

        //MeanAlt
        private double meanAlt;
        public double MeanAlt
        {
            get { return meanAlt; }
            set { Set(ref meanAlt, value); }
        }

        // RefPosLat
        private double refPosLat;
        public double RefPosLat
        {
            get { return refPosLat; }
            set { Set(ref refPosLat, value); }
        }

        // RefPosLon
        private double refPosLon;
        public double RefPosLon
        {
            get { return refPosLon; }
            set { Set(ref refPosLon, value); }
        }

        // RefPosAlt
        private double refPosAlt;
        public double RefPosAlt
        {
            get { return refPosAlt; }
            set { Set(ref refPosAlt, value); }
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

        // Time resolution
        private long millisBTSamples;
        public long MillisBTSamples
        {
            get { return millisBTSamples; }
            set
            {
                Set(ref millisBTSamples, value);
            }
        }


        public RTKPOSUnit()
        {
            this.reset();
        }

        public void reset()
        {
            Name = "Drop File";
            StartTime = DateTime.Parse("2000-01-01T12:00:00");
            StopTime = DateTime.Parse("2000-01-01T12:00:00");
            Path = "";
            Type = "POS";
            JobParamType = JobParameterType.RTKPOSUNIT;
            GPSPositions.Clear();
        }
    }
}
