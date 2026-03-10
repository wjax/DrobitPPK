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
    public class PostProcessManager
    {
        private const string RNX2RTKP_EXE = @".\Utils\rnx2rtkp.exe";

        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__RINEX = @" -k .\Utils\opt_rtk_ellip_rinex.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__AVERAGE = @" -k .\Utils\opt_rtk_ellip_average.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__RINEX = @" -k .\Utils\opt_rtk_geoid_rinex.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__AVERAGE = @" -k .\Utils\opt_rtk_geoid_average.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__RINEX = @" -k .\Utils\opt_stc_ellip_rinex.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__RINEX = @" -k .\Utils\opt_stc_geoid_rinex.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__AVERAGE = @" -k .\Utils\opt_stc_ellip_average.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__AVERAGE = @" -k .\Utils\opt_stc_geoid_average.conf";

        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__RINEX_ONLYGPS = @" -k .\Utils\opt_rtk_ellip_rinex_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__AVERAGE_ONLYGPS = @" -k .\Utils\opt_rtk_ellip_average_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__RINEX_ONLYGPS = @" -k .\Utils\opt_rtk_geoid_rinex_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__AVERAGE_ONLYGPS = @" -k .\Utils\opt_rtk_geoid_average_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__RINEX_ONLYGPS = @" -k .\Utils\opt_stc_ellip_rinex_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__AVERAGE_ONLYGPS = @" -k .\Utils\opt_stc_ellip_average_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__RINEX_ONLYGPS = @" -k .\Utils\opt_stc_geoid_rinex_onlygps.conf";
        private const string RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__AVERAGE_ONLYGPS = @" -k .\Utils\opt_stc_geoid_average_onlygps.conf";

        private const string RNX2RTKP_ARGS_BASEPOS = " -l ";
        private const string RNX2RTKP_ARGS_OUT = " -o ";

        private const string RNX2RTKP_ARGS_START = " -ts ";
        private const string RNX2RTKP_ARGS_END = " -te ";
        private const string RNX2RTKP_ARGS_INTERVAL = " -ti ";
        private const string RNX2RTKP_ARGS_ELEVMASK = " -m ";
        private const string RNX2RTKP_ARGS_MODE = " -p ";

        private string cmdLineArgs = " ";

        private CMDExecutor executor;
        private Process process;

        public PostProcessManager()
        {
        }

        public void Initialize(bool normal_alternative, float interval, int elevationMask, bool rtk_static, bool rinex_average, bool ellip_geoid, RINEXUnit baseRinex, RINEXUnit roverRinex, string outputPath, DateTime sTime, DateTime eTime, double[] basePosition = null)
        {
            // Conf file
            if (rtk_static && ellip_geoid && rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__RINEX;
            else if (rtk_static && ellip_geoid && rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__RINEX_ONLYGPS;
            else if (rtk_static && ellip_geoid && !rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__AVERAGE;
            else if (rtk_static && ellip_geoid && !rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__ELLIP__AVERAGE_ONLYGPS;
            else if (rtk_static && !ellip_geoid && rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__RINEX;
            else if (rtk_static && !ellip_geoid && rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__RINEX_ONLYGPS;
            else if (rtk_static && !ellip_geoid && !rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__AVERAGE;
            else if (rtk_static && !ellip_geoid && !rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_RTK__GEOID__AVERAGE_ONLYGPS;
            else if (!rtk_static && ellip_geoid && rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__RINEX;
            else if (!rtk_static && ellip_geoid && rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__RINEX_ONLYGPS;
            else if (!rtk_static && ellip_geoid && !rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__AVERAGE;
            else if (!rtk_static && ellip_geoid && !rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__ELLIP__AVERAGE_ONLYGPS;
            else if (!rtk_static && !ellip_geoid && rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__RINEX;
            else if (!rtk_static && !ellip_geoid && rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__RINEX_ONLYGPS;
            else if (!rtk_static && !ellip_geoid && !rinex_average && normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__AVERAGE;
            else if (!rtk_static && !ellip_geoid && !rinex_average && !normal_alternative)
                cmdLineArgs += RNX2RTKP_ARGS_OPTIONFILE_STATIC__GEOID__AVERAGE_ONLYGPS;

            // output file
            cmdLineArgs += RNX2RTKP_ARGS_OUT + "\"" + outputPath + "\"";

            // mode rtk or single depeding is there is base
            if (string.IsNullOrEmpty(baseRinex.ObsFileUri))
                cmdLineArgs += RNX2RTKP_ARGS_MODE + "0";

            // Base with custom pos
            if (basePosition != null)
                cmdLineArgs += RNX2RTKP_ARGS_BASEPOS + basePosition[0].ToString(CultureInfo.InvariantCulture) + " " + basePosition[1].ToString(CultureInfo.InvariantCulture) + " " + basePosition[2].ToString(CultureInfo.InvariantCulture) + " ";

            // Time Start
            cmdLineArgs += RNX2RTKP_ARGS_START + dateRTKLIBFormat(sTime) + " " + timeRTKLIBFormat(sTime);

            // Time End
            cmdLineArgs += RNX2RTKP_ARGS_END + dateRTKLIBFormat(eTime) + " " + timeRTKLIBFormat(eTime) + " ";

            // Interval
            cmdLineArgs += RNX2RTKP_ARGS_INTERVAL + interval.ToString(CultureInfo.InvariantCulture) + " ";

            // Elevation Mask
            cmdLineArgs += RNX2RTKP_ARGS_ELEVMASK + elevationMask + " ";

            // Files
            // Rover
            cmdLineArgs += " \"" + roverRinex.ObsFileUri + " \"";
            // Base
            if (!string.IsNullOrEmpty(baseRinex.ObsFileUri))
            cmdLineArgs += " \"" + baseRinex.ObsFileUri + " \"";
            // Navigation
            foreach (string s in roverRinex.NavFileUris)
                cmdLineArgs += " \"" + s + " \" ";
            foreach (string s in baseRinex.NavFileUris)
                cmdLineArgs += " \"" + s + " \" ";
        }

        private string timeRTKLIBFormat(DateTime time)
        {
            if (time == null)
                return "";

            return time.ToUniversalTime().Hour + ":" + time.ToUniversalTime().Minute + ":" + time.ToUniversalTime().Second;
        }

        private string dateRTKLIBFormat(DateTime date)
        {
            if (date == null)
                return "";

            return date.ToUniversalTime().Year + "/" + date.ToUniversalTime().Month + "/" + date.ToUniversalTime().Day;
        }

        public Process getProcess()
        {
            return process;
        }

        public void Start()
        {
            executor = new CMDExecutor();

            System.Diagnostics.Debug.WriteLine($"Exe command: {RNX2RTKP_EXE} {cmdLineArgs}");

            process = executor.Start(RNX2RTKP_EXE, cmdLineArgs);

            //proc.OutputDataReceived += (sender, args) => Display(args.Data);
        }

        public void Stop()
        {
            executor.Kill();
        }
    }
}
