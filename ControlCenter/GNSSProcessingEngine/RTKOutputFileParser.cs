using ControlCenter.Extras;
using ControlCenter.Models;
using ControlCenter.Models.JOBS.Parameters.GNSS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ControlCenter.GNSSProcessingEngine
{
    public class RTKOutputFileParser
    {
        private const string REGEX_HEADER = @"^\% ref pos.*:(.*);(.*);(.*)$";
        private const string HEADER_END_LINE_START = "%  GPST";

        private const int NUMLINES_HEADER_MAX = 20;

        private const int I_TIME_WEEK = 0;
        private const int I_TIME_SECOND = 1;
        private const int I_LAT = 2;
        private const int I_LON = 3;
        private const int I_ALT = 4;
        private const int I_Q = 5;
        private const int I_NS = 6;
        private const int I_SDN = 7;
        private const int I_SDE = 8;
        private const int I_SDU = 9;
        private const int I_SDNE = 10;
        private const int I_SDEU = 11;
        private const int I_SDUN = 12;
        private const int I_AGE = 13;
        private const int I_RATIO = 14;
        private const int I_VN = 15;
        private const int I_VE = 16;
        private const int I_VU = 17;



        public static bool ParseFile(string filePath, ref RTKPOSUnit posUnit)
        {
            string line;

            int numLines = 0;
            int numPositions = 0;
            int numQ1 = 0;
            int numQ2 = 0;
            int numQ5 = 0;
            double MeanLat = 0;
            double MeanLon = 0;
            double MeanAlt = 0;

            int currQ = 0;
            double currLat = 0;
            double currLon = 0;
            double currAlt = 0;
            double currVe = 0;
            double currVn = 0;
            double currVu = 0;

            long millisBTSample = 10000;
            long currmillisBTSample = 0;
            long lastSamplemillis = 0;
            long currMillis = 0;

            DateTime minTime = DateTime.MaxValue;
            DateTime maxTime = DateTime.MinValue;

            int currWeek;
            double currSecond;

            bool still_in_header = true;

            if (File.Exists(filePath))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(filePath);
                    while ((line = file.ReadLine()) != null)
                    {
                        Match mHead;
                        if (still_in_header)
                        {
                            if ((mHead = Regex.Match(line, REGEX_HEADER)).Success)
                        {
                            if (mHead.Groups.Count > 2)
                            {
                                posUnit.RefPosLat = double.Parse(mHead.Groups[1].Value, CultureInfo.InvariantCulture);
                                posUnit.RefPosLon = double.Parse(mHead.Groups[2].Value, CultureInfo.InvariantCulture);
                                posUnit.RefPosAlt = double.Parse(mHead.Groups[3].Value, CultureInfo.InvariantCulture);
                            }
                        }
                            else if (line.StartsWith(HEADER_END_LINE_START))
                            {
                                still_in_header = false;
                            }
                        }
                        else
                        {
                            //// Go for the data portion
                            //if (numLines >= 2)
                            //{
                                numPositions++;
                                // Split as it is faster than regex
                                string[] fields = line.Split(';');

                                //Time -------------------------------------------------------------------------
                                currWeek = int.Parse(fields[I_TIME_WEEK], CultureInfo.InvariantCulture);
                                currSecond = double.Parse(fields[I_TIME_SECOND], CultureInfo.InvariantCulture);
                                DateTime currDate = DrobitTools.GetTimeFromGps(currWeek, currSecond, true);

                                if (currDate.CompareTo(minTime) < 0)
                                    minTime = currDate;

                                if (currDate.CompareTo(maxTime) > 0)
                                    maxTime = currDate;

                                currMillis = DrobitTools.millisFromEpoch(currDate);
                                currmillisBTSample = currMillis - lastSamplemillis;
                                millisBTSample = (currmillisBTSample < millisBTSample) ? currmillisBTSample : millisBTSample;
                                lastSamplemillis = currMillis;

                                // Quality Fix
                                currQ = int.Parse(fields[I_Q]);
                                // GPS Position
                                currLat = double.Parse(fields[I_LAT], CultureInfo.InvariantCulture);
                                currLon = double.Parse(fields[I_LON], CultureInfo.InvariantCulture);
                                currAlt = double.Parse(fields[I_ALT], CultureInfo.InvariantCulture);

                                // GPS Position
                                currVe = double.Parse(fields[I_VE], CultureInfo.InvariantCulture);
                                currVn = double.Parse(fields[I_VN], CultureInfo.InvariantCulture);
                                currVu = double.Parse(fields[I_VU], CultureInfo.InvariantCulture);



                                // Gps Position to list
                                GPSPosition gpsPos = new GPSPosition();
                                gpsPos.GPSWeek = currWeek;
                                gpsPos.GPSSecond = currSecond;
                                gpsPos.MicrosTime = DrobitTools.microsFromEpoch(currDate);
                                gpsPos.Lat = currLat;
                                gpsPos.Lon = currLon;
                                gpsPos.Alt = currAlt;
                                gpsPos.FIXType = currQ;
                                gpsPos.Ve = currVe;
                                gpsPos.Vn = currVn;
                                gpsPos.Vu = currVu;

                                posUnit.GPSPositions.Add(gpsPos.MicrosTime, gpsPos);

                                switch (currQ)
                                {
                                    case 1:
                                        numQ1++;

                                        if (MeanLon != 0)
                                        {
                                            //MeanLat = (MeanLat += currLat) / 2;
                                            //MeanLon = (MeanLon += currLon) / 2;
                                            //MeanAlt = (MeanAlt += currAlt) / 2;
                                            MeanLat += currLat;
                                            MeanLon += currLon;
                                            MeanAlt += currAlt;
                                        }
                                        else
                                        {
                                            MeanLat = currLat;
                                            MeanLon = currLon;
                                            MeanAlt = currAlt;
                                        }
                                        break;
                                    case 2:
                                        numQ2++;
                                        break;
                                    case 5:
                                        numQ5++;
                                        break;
                                }
                            //}
                        }
                        numLines++;
                    }
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }
            else
                return false;

            // Copy 
            posUnit.MeanLat = MeanLat/ numQ1;
            posUnit.MeanLon = MeanLon/ numQ1;
            posUnit.MeanAlt = MeanAlt/ numQ1;

            posUnit.NumPositions = numPositions;
            posUnit.Path = filePath;
            posUnit.Q1 = numQ1;
            posUnit.Q2 = numQ2;
            posUnit.Q5 = numQ5;

            if (numPositions != 0)
                posUnit.Q1per100 = (100 * numQ1) / numPositions;
            else
                posUnit.Q1per100 = 0;

            posUnit.StartTime = minTime;
            posUnit.StopTime = maxTime;

            posUnit.MillisBTSamples = millisBTSample;

            return true;
        }

        public static RTKPOSUnit ParseFile(string filePath)
        {
            RTKPOSUnit posUnit = new RTKPOSUnit();
            bool ok = ParseFile(filePath, ref posUnit);
            return ok ? posUnit:null;
        }

    }
}
