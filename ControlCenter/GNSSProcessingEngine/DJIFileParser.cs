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
    public class DJIFileParser
    {
        private const string REGEX_DJI_LINE = @"^([-+]?[0-9]*\.?[0-9]+)\s+([-+]?[0-9]*\.?[0-9]+)\s+\[([-+]?[0-9]*\.?[0-9]+)\]\s+([-+]?[0-9]*\.?[0-9]+),N\s+([-+]?[0-9]*\.?[0-9]+),E\s+([-+]?[0-9]*\.?[0-9]+),V\s+([-+]?[0-9]*\.?[0-9]+),Lat\s+([-+]?[0-9]*\.?[0-9]+),Lon\s+([-+]?[0-9]*\.?[0-9]+),Ellh\s+([-+]?[0-9]*\.?[0-9]+),\s+([-+]?[0-9]*\.?[0-9]+),\s+([-+]?[0-9]*\.?[0-9]+)\s+([-+]?[0-9]*\.?[0-9]+),Q$";

        private const int I_PICNAME = 1;
        private const int I_TIME_WEEK = 3;
        private const int I_TIME_SECONDS = 2;
        private const int I_COARSE_LAT = 7;
        private const int I_COARSE_LON = 8;
        private const int I_COARSE_ALT = 9;
        private const int I_OFFSET_X = 4;
        private const int I_OFFSET_Y = 5;
        private const int I_OFFSET_Z = 6;


        public static bool ParseFile(string filePath, ref CAMUnit unit)
        {
            string line;
            int numLines = 0;
            int numCams = 0;

            int currWeek;
            double currSecond;

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
                        Match mLine = Regex.Match(line, REGEX_DJI_LINE);
                        if (mLine.Success)
                        {
                            numCams++;

                            // Time -----------------------------------------------------------------------
                            currWeek = int.Parse(mLine.Groups[I_TIME_WEEK].Value, CultureInfo.InvariantCulture);
                            currSecond = double.Parse(mLine.Groups[I_TIME_SECONDS].Value, CultureInfo.InvariantCulture);

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

                            camShot.skipRotationsAndApplyDirectOffset = true;

                            camShot.OffsetX = float.Parse(mLine.Groups[I_OFFSET_X].Value, CultureInfo.InvariantCulture)/1000f;
                            camShot.OffsetY = float.Parse(mLine.Groups[I_OFFSET_Y].Value, CultureInfo.InvariantCulture)/1000f;
                            camShot.OffsetZ = float.Parse(mLine.Groups[I_OFFSET_Z].Value, CultureInfo.InvariantCulture)/1000f;

                            camShot.CoarseLat = double.Parse(mLine.Groups[I_COARSE_LAT].Value, CultureInfo.InvariantCulture);
                            camShot.CoarseLon = double.Parse(mLine.Groups[I_COARSE_LON].Value, CultureInfo.InvariantCulture);
                            camShot.CoarseAlt = double.Parse(mLine.Groups[I_COARSE_ALT].Value, CultureInfo.InvariantCulture);

                            camShot.Name = mLine.Groups[I_PICNAME].Value;
                            //-------------------------------------------------------------------------------------

                            // Add to List
                            //unit.CameraShots.Add(camShot.MicrosTime, camShot);
                            unit.CameraShots.Add(numLines, camShot);
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
