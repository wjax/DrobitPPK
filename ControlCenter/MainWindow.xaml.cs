using ControlCenter.Extras;
using ControlCenter.Models;
using ControlCenter.ModelViews;
using ControlCenter.UserControls.Dialogs;
using DrobitExtras;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WExtraControlLibrary.Extras;
using WExtraControlLibrary.UserControls.LogControl;
using AutoUpdaterDotNET;
using System.Windows.Forms;
using System.Reflection;
using System.Net;

namespace ControlCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        System.Timers.Timer timer;
        private bool projectsAlreadyLoaded = false;

        private void CreateFoldersCallback(object obj, string folder, bool hasToExit)
        {
            bool bResponse = (bool)obj;

            if (bResponse)
            {
                // Persist Setting
                Properties.Settings.Default["DrobitMainPath"] = folder;
                Properties.Settings.Default.Save();

                try
                {
                    DrobitTools.CreateFolderIfNotExists(DrobitTools.GetDOWNLOADSDrobitFolder());
                    DrobitTools.CreateFolderIfNotExists(DrobitTools.GetPROJECTSDrobitFolder());
                } catch(Exception ee)
                {
                    Environment.Exit(1);
                }

                // AutoUpdate Dialog
                AutoUpdater.RunUpdateAsAdmin = false;
                AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
                Dispatcher.Invoke(new Action(() => AutoUpdater.Start("http://drobit.es/Updater/DrobitPPKAutoUpdater.xml")));
            }
            else if (hasToExit)
                 Environment.Exit(1);
        }

        public MainWindow()
        {
            InitializeComponent();            
        }



        private async void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (args.IsUpdateAvailable)
                {
                    //let's set up a little MVVM, cos that's what the cool kids are doing:
                    AutoUpdaterDialog view = new AutoUpdaterDialog();
                    view.DataContext = this;
                    view.TB_Version.Text = args.CurrentVersion.ToString();
                    view.TB_Changelog.Text = (new WebClient()).DownloadString(args.ChangelogURL);

                    var result = await DialogHost.Show(view, "RootDialog");

                    bool bResponse = (bool)result;


                    if (bResponse)
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate())
                            {
                                Environment.Exit(0);
                            }
                        }
                        catch (Exception exception)
                        {
                            //MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                            //    MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private async void ExecuteRunDialog(bool o)
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            MainSettingsDialog view = new MainSettingsDialog();
            view.DataContext = this;

            var result = await DialogHost.Show(view, "RootDialog");

            CreateFoldersCallback(result, view.TB_Folder.Text, o);

            //ProcessingViewModel procViewModel = (ProcessingViewModel)JobContainer.DataContext;
            //procViewModel.LoadFromDiskProjectCallback(null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void InitDialogs()
        {
            // Init projects
            if (!projectsAlreadyLoaded)
            {
                ProcessingViewModel procViewModel = (ProcessingViewModel)JobContainer.DataContext;
                procViewModel.LoadFromDiskProjectCallback(null);
                projectsAlreadyLoaded = true;
            }


            if (DrobitTools.CheckFolders() == false)
            {
                ExecuteRunDialog(true);
            }
            else
            {
                // AutoUpdate Dialog
                AutoUpdater.RunUpdateAsAdmin = false;
                AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
                Dispatcher.Invoke(new Action(() => AutoUpdater.Start("http://drive.google.com/uc?export=download&id=13NSdu7FFZEZ-Eqc7rSEmY-epnJEKYd65")));
            }

            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        private void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {
            InitDialogs();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExecuteRunDialog(false);
        }
    }
}
