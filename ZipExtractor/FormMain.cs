using Ionic.Zip;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor
{
    public partial class FormMain : MaterialForm
    {
        private BackgroundWorker _backgroundWorker;

        public FormMain()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.Red200, TextShade.WHITE);
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length >= 3)
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.MainModule.FileName.Equals(args[2]))
                        {
                            labelInformation.Text = @"Waiting for application to Exit...";
                            process.WaitForExit();
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
                    var path = Path.GetDirectoryName(args[2]);
                    var pathZip = args[1];
                    var password = "drobit4work";

                    using (var zip = ZipFile.Read(pathZip))
                    {
                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                        zip.Password = password;

                        int i = 0;
                        float total = zip.Count;

                        foreach (var ee in zip)
                        {
                            ee.Extract(path, ExtractExistingFileAction.OverwriteSilently);
                            float p = i / total;
                            _backgroundWorker.ReportProgress((int)(p * 100), "Extracting files...");

                            i++;
                        }
                    }


                    // Use DotnetZip
                    //using (ZipFile archive = ZipFile.Read(pathZip))
                    //{
                    //    archive.Password = "drobit4work";
                    //    archive.Encryption = EncryptionAlgorithm.WinZipAes256; // the default: you might need to select the proper value here
                    //    //archive.StatusMessageTextWriter = Console.Out;

                    //    archive.ExtractAll(path, ExtractExistingFileAction.OverwriteSilently);
                    //}


                };

                _backgroundWorker.ProgressChanged += (o, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    labelInformation.Text = eventArgs.UserState.ToString();
                };

                _backgroundWorker.RunWorkerCompleted += (o, eventArgs) =>
                {
                    if (!eventArgs.Cancelled)
                    {
                        labelInformation.Text = @"Finished";
                        try
                        {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo(args[2]);
                            if (args.Length > 3)
                            {
                                processStartInfo.Arguments = args[3];
                            }
                            Process.Start(processStartInfo);
                        }
                        catch (Win32Exception exception)
                        {
                            if (exception.NativeErrorCode != 1223)
                                throw;
                        }
                        Application.Exit();
                    }
                };

                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();
        }
    }
}
