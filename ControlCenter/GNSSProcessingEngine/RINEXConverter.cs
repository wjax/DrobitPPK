using ControlCenter.Extras;
using DrobitExtras;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCenter.GNSSProcessingEngine
{
    public enum GNSS_FILE_TYPE
    {
        DROBIT,
        UBLOX,
        TOPCON,
        RINEX3x,
        RINEX2x,
        RINEX3x_COMPACT,
        RINEX2x_COMPACT
    }

    static class RINEXConverter
    {
        private const string CONVBIN_EXE = @".\Utils\convbin.exe";
        private const string CONVBIN_ARGS = " -od -os -oi -ot -scan";
        private const string CONVBIN_ARGS_RINEX = " -r rinex -d";
        private const string CONVBIN_ARGS_DROBIT = " -r nov -d";
        private const string CONVBIN_ARGS_UBLOX = " -r ubx -d";

        private const string TPS2TINEX_EXE = @".\Utils\tps2rin.exe";
        private const string TPS2RINEX_ARGS = " -o ";

        private const string CRX2RNX_EXE = @".\Utils\crx2rnx.exe";

        

        public static string DeCompact(string fileUri)
        {
            // Start
            CMDExecutor executor = new CMDExecutor();
            Process proc = executor.Start(CRX2RNX_EXE, "\"" + fileUri + "\"");

            var outTask = proc.StandardOutput.ReadToEndAsync();
            var errTask = proc.StandardError.ReadToEndAsync();
            proc.WaitForExit();
            string processOutput = outTask.Result;
            string processError = errTask.Result;

            return fileUri.Remove(fileUri.Length - 1, 1) + "o";

        }

        public static void Convert(string fileUri, GNSS_FILE_TYPE type, string outFolderPath)
        {
            // Temp folder
            string tempFolder = DrobitTools.ConcatenatePath(new string[] { outFolderPath, "temp" });
            // Make new temp dir
            DrobitTools.EmptyDirectory(tempFolder);
            // Create Temp
            DrobitTools.CreateFolderIfNotExists(tempFolder);

            // Start
            CMDExecutor executor = new CMDExecutor();

            string args;
            string exe;

            switch (type)
            {
                case GNSS_FILE_TYPE.DROBIT:
                    exe = CONVBIN_EXE;
                    args = CONVBIN_ARGS;
                    args += CONVBIN_ARGS_DROBIT + " \"" + tempFolder + "\" " + "\"" + fileUri + "\"";
                    break;
                case GNSS_FILE_TYPE.UBLOX:
                    exe = CONVBIN_EXE;
                    args = CONVBIN_ARGS;
                    args += CONVBIN_ARGS_UBLOX + " \"" + tempFolder + "\" " + "\"" + fileUri + "\"";
                    break;
                case GNSS_FILE_TYPE.RINEX3x:
                case GNSS_FILE_TYPE.RINEX3x_COMPACT:
                    exe = CONVBIN_EXE;
                    args = CONVBIN_ARGS;
                    args += CONVBIN_ARGS_RINEX + " \"" + tempFolder + "\" " + "\"" + fileUri + "\"";
                    break;
                case GNSS_FILE_TYPE.TOPCON:
                    exe = TPS2TINEX_EXE;
                    args = "\"" + fileUri + "\" " + TPS2RINEX_ARGS;
                    args += " \"" + tempFolder + "\"";
                    break;
                default:
                    return;
            }

            Process proc = executor.Start(exe, args);

            string processError = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            //-------------

            System.Diagnostics.Debug.WriteLine(processError);

            // Move files to output Folder
            DrobitTools.CopyFolderContents(tempFolder, outFolderPath, true);

        }
    }
}
