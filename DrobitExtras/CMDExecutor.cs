using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrobitExtras
{
    public class CMDExecutor
    {
        private Process proc;
        private ProcessStartInfo procInfo;

        public CMDExecutor()
        {
           
        }

        public Process Start(string cmdUri, string args)
        {
            proc = new Process();
            procInfo = new ProcessStartInfo();

            procInfo.FileName = cmdUri;
            procInfo.Arguments = args;
            procInfo.WindowStyle = ProcessWindowStyle.Hidden;
            procInfo.CreateNoWindow = true;
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardInput = false;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            proc.StartInfo = procInfo;

            proc.Start();

            return proc;
        }

        public void Kill()
        {
            try
            {
                if (proc != null)
                    proc.Kill();
            }
            catch (Exception)
            { }
            finally
            {
                proc = null;
            }
        }

        public void Close()
        {
            if (proc != null)
            {
                proc.WaitForExit();
                proc.Close();
            }
        }
    }
}
