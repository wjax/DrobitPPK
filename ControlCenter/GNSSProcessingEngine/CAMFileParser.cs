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
    public class CAMFileParser
    {
        private const string REGEX_CAM_OFFSET_V2_0 = @"^(.*);(.*);(.*);(.*);(.*);(.*);(.*);(.*);(.*);(.*);(.*);(.*)$";
        private const string REGEX_CAM_OFFSET_V1_2 = @"^(.*);(.*);(.*)$";
        private const string REGEX_CAM_Version = @"^Drobit CAM File (.*)$";

        private const string VERSION_2_0 = "v2.0";
        private const string VERSION_1_2 = "v1.2";

        private const int I_TIME_MS_1 = 0;
        private const int I_TIME_MS_2 = 1;
        private const int I_TIME_MS_3 = 2;
        private const int I_DATETIME = 3;
        private const int I_PICNAME = 4;
        private const int I_TIME_WEEK = 5;
        private const int I_TIME_SECONDS = 6;
        private const int I_ROLL_B = 7;
        private const int I_PITCH_B = 8;
        private const int I_YAW_B = 9;
        private const int I_COARSE_LAT = 10;
        private const int I_COARSE_LON = 11;
        private const int I_COARSE_ALT = 12;
        private const int I_ROLL_H = 13;
        private const int I_PITCH_H = 14;
        private const int I_YAW_H = 15;

        public static bool ParseFile(string filePath, ref CAMUnit unit)
        {
            string line;
            int numLines = 0;
            int numCams = 0;

            int currWeek;
            double currSecond;

            string camVersion = "v1.2";

            DateTime minTime = DateTime.MaxValue;
            DateTime maxTime = DateTime.MinValue;

            if (File.Exists(filePath))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(filePath);
                    while ((line = file.ReadLine()) != null)
                    {
                        if (numLines == 0)
                        {
                            // Drobit Version
                            Match mVersion = Regex.Match(line, REGEX_CAM_Version);
                            if (mVersion.Groups.Count == 2)
                            {
                                camVersion = mVersion.Groups[1].Value;
                            }
                            else
                                return false;
                        }
                        // First line is Header with ref position of base
                        else if (numLines == 1)
                        {
                            Match mHead = Regex.Match(line, camVersion == VERSION_2_0 ? REGEX_CAM_OFFSET_V2_0 : REGEX_CAM_OFFSET_V1_2);

                            if (camVersion == VERSION_2_0)
                            {
                                if (mHead.Groups.Count > 2)
                                {
                                    unit.XOffset1 = double.Parse(mHead.Groups[1].Value, CultureInfo.InvariantCulture);
                                    unit.YOffset1 = double.Parse(mHead.Groups[2].Value, CultureInfo.InvariantCulture);
                                    unit.ZOffset1 = double.Parse(mHead.Groups[3].Value, CultureInfo.InvariantCulture);
                                    unit.XOffset2 = double.Parse(mHead.Groups[4].Value, CultureInfo.InvariantCulture);
                                    unit.YOffset2 = double.Parse(mHead.Groups[5].Value, CultureInfo.InvariantCulture);
                                    unit.ZOffset2 = double.Parse(mHead.Groups[6].Value, CultureInfo.InvariantCulture);
                                    unit.XOffset3 = double.Parse(mHead.Groups[7].Value, CultureInfo.InvariantCulture);
                                    unit.YOffset3 = double.Parse(mHead.Groups[8].Value, CultureInfo.InvariantCulture);
                                    unit.ZOffset3 = double.Parse(mHead.Groups[9].Value, CultureInfo.InvariantCulture);
                                    unit.XOffset4 = double.Parse(mHead.Groups[10].Value, CultureInfo.InvariantCulture);
                                    unit.YOffset4 = double.Parse(mHead.Groups[11].Value, CultureInfo.InvariantCulture);
                                    unit.ZOffset4 = double.Parse(mHead.Groups[12].Value, CultureInfo.InvariantCulture);
                                }
                            }
                            else if (camVersion == VERSION_1_2)
                            {
                                if (mHead.Groups.Count > 2)
                                {
                                    unit.XOffset1 = double.Parse(mHead.Groups[1].Value, CultureInfo.InvariantCulture);
                                    unit.YOffset1 = double.Parse(mHead.Groups[2].Value, CultureInfo.InvariantCulture);
                                    unit.ZOffset1 = double.Parse(mHead.Groups[3].Value, CultureInfo.InvariantCulture);
                                    unit.XOffset2 = 0;
                                    unit.YOffset2 = 0;
                                    unit.ZOffset2 = 0;
                                    unit.XOffset3 = 0;
                                    unit.YOffset3 = 0;
                                    unit.ZOffset3 = 0;
                                    unit.XOffset4 = 0;
                                    unit.YOffset4 = 0;
                                    unit.ZOffset4 = 0;
                                }
                            }
                        }
                        else
                        {
                            // Go for the data portion
                            if (numLines >= 2 && !line.Equals(""))
                            {
                                // Split as it is faster than regex
                                string[] fields = line.Split(';');

                                if (fields.Length >= 15)
                                {
                                    numCams++;

                                // Time -----------------------------------------------------------------------
                                currWeek = int.Parse(fields[I_TIME_WEEK], CultureInfo.InvariantCulture);
                                currSecond = double.Parse(fields[I_TIME_SECONDS], CultureInfo.InvariantCulture);

                                DateTime currDate = DrobitTools.GetTimeFromGps(currWeek, currSecond, true);

                                if (currDate.CompareTo(minTime) < 0)
                                    minTime = currDate;

                                if (currDate.CompareTo(maxTime) > 0)
                                    maxTime = currDate;

                                // Fill CameraShot -----------------------------------------------------------------
                                CameraShot camShot = new CameraShot();

                                camShot.GPSWeek = currWeek;
                                camShot.GPSSecond = currSecond;
                                camShot.MicrosTime = DrobitTools.microsFromEpoch(currDate);

                                camShot.CoarseLat = double.Parse(fields[I_COARSE_LAT], CultureInfo.InvariantCulture);
                                camShot.CoarseLon = double.Parse(fields[I_COARSE_LON], CultureInfo.InvariantCulture);
                                camShot.CoarseAlt = double.Parse(fields[I_COARSE_ALT], CultureInfo.InvariantCulture);

                                camShot.RollB = float.Parse(fields[I_ROLL_B], CultureInfo.InvariantCulture);
                                camShot.PitchB = float.Parse(fields[I_PITCH_B], CultureInfo.InvariantCulture);
                                camShot.YawB = float.Parse(fields[I_YAW_B], CultureInfo.InvariantCulture);

                                if (camVersion == VERSION_2_0)
                                {
                                    camShot.RollH = float.Parse(fields[I_ROLL_H], CultureInfo.InvariantCulture);
                                    camShot.PitchH = float.Parse(fields[I_PITCH_H], CultureInfo.InvariantCulture);
                                    camShot.YawH = float.Parse(fields[I_YAW_H], CultureInfo.InvariantCulture);
                                }

                                camShot.Name = fields[I_PICNAME];
                                //-------------------------------------------------------------------------------------
                                
                                // Add to List
                                //unit.CameraShots.Add(camShot.MicrosTime, camShot);
                                unit.CameraShots.Add(numLines, camShot);
                                }

                            }
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

            // Last
            unit.NumCameras = numCams;
            unit.StartTime = minTime;
            unit.StopTime = maxTime;
            return true;
        }

        public static CAMUnit ParseFile(string filePath)
        {
            CAMUnit unit = new CAMUnit();
            bool ok = ParseFile(filePath, ref unit);
            return ok ? unit : null;
        }
    }
}
