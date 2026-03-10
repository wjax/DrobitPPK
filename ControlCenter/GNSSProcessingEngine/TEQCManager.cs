using ControlCenter.Models.JOBS.Parameters.GNSS;
using DrobitExtras;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCenter.GNSSProcessingEngine
{
    static class TEQCManager
    {
        private const string TEQC_EXE = @".\Utils\teqc.exe";
        private const string TEQC_ARGS = "+qc -l -s -n_GLONASS 32 ";

        private const string REGEX_STARTTIME = "Time of start of window : (.*)";
        private const string REGEX_STOPTIME = "Time of  end  of window : (.*)";
        private const string REGEX_PERIOD = "Observation interval    : (.*) seconds";
        private const string REGEX_OBSCOUNT = "Possible obs >  10.0 deg: (.*)";
        private const string REGEX_SATSCOUNT = "Total satellites w/ obs : (.*)";


        public static bool AnalyzeRINEX(ref RINEXUnit unit)
        {
            CMDExecutor executor = new CMDExecutor();

            string fileArgs = "\"" + unit.ObsFileUri + "\"";
            /*foreach (string nav in unit.NavFileUris)
            {
                fileArgs += " \"" + nav + "\"";
            }*/

            
            Process proc = executor.Start(TEQC_EXE, TEQC_ARGS + fileArgs);

            var outTask = proc.StandardOutput.ReadToEndAsync();
            var errTask = proc.StandardError.ReadToEndAsync();
            proc.WaitForExit();
            string processOutput = outTask.Result;
            string processError = errTask.Result;

            System.Diagnostics.Debug.WriteLine(processError);
            System.Diagnostics.Debug.WriteLine(processOutput);

            if (processOutput != "")
            {
                ParseTEQCQC(processOutput, unit);
                return true;
            }
            else
                return false;

        }

        private static bool ParseTEQCQC(string output, RINEXUnit unit)
        {
            if (output.Equals(""))
                return false;

            // Parse
            Match mStartTime = Regex.Match(output, REGEX_STARTTIME);
            Match mStopTime = Regex.Match(output, REGEX_STOPTIME);
            Match mPeriod = Regex.Match(output, REGEX_PERIOD);
            Match mObsCount = Regex.Match(output, REGEX_OBSCOUNT);
            Match mSatCount = Regex.Match(output, REGEX_SATSCOUNT);

            char[] chars = new char[2];
            chars[0] = '\r';
            chars[1] = ' ';


            string startTime = "";
            string stopTime = "";
            string period = "";
            string obsCount = "";
            string satCount = "";

            if (mStartTime.Groups.Count > 0)
                startTime = mStartTime.Groups[1].Value.Trim(chars);
            if (mStopTime.Groups.Count > 0)
                stopTime = mStopTime.Groups[1].Value.Trim(chars);
            if (mPeriod.Groups.Count > 0)
                period = mPeriod.Groups[1].Value.Trim(chars);
            if (mObsCount.Groups.Count > 0)
                obsCount = mObsCount.Groups[1].Value.Trim(chars);
            if (mSatCount.Groups.Count > 0)
                satCount = mSatCount.Groups[1].Value.Trim(chars);

            string[] dateFormats = { @"yyyy MMM dd  HH:mm:ss.fff", @"yyyy MMM  d  HH:mm:ss.fff" };

            DateTime dstartTime;
            if (DateTime.TryParseExact(startTime, dateFormats, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dstartTime))
                unit.StartTime = dstartTime.ToUniversalTime();
                //System.Diagnostics.Debug.WriteLine("");

            DateTime dstopTime;
            if (DateTime.TryParseExact(stopTime, dateFormats, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dstopTime))
                unit.StopTime = dstopTime.ToUniversalTime();
            //System.Diagnostics.Debug.WriteLine("");

            int iSatCount = 0;
            long lObsNumber = 0;
            float fMeasPeriod = 0.0f;

            int.TryParse(satCount, out iSatCount);
            long.TryParse(obsCount, out lObsNumber);
            float.TryParse(period, out fMeasPeriod);

            unit.SatCount = iSatCount;
            unit.ObsNumber = lObsNumber;
            unit.MeasPeriod = fMeasPeriod;

            System.Diagnostics.Debug.WriteLine("Start: " + dstartTime.ToString() + " -- StopTime: " + dstopTime.ToString());

            return true;
        }
    }


    //public class TEQCWotkItem
    //{
    //    public StreamReader stream;
    //    public RINEXUnit unit;
    //    public CMDExecutor executor;

    //    public TEQCWotkItem(CMDExecutor executor_, StreamReader stream_, RINEXUnit unit_)
    //    {
    //        executor = executor_;
    //        stream = stream_;
    //        unit = unit_;
    //    }
    //}
}
