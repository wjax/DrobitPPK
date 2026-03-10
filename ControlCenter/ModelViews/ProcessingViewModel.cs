using ControlCenter.Extras;
using ControlCenter.Models;
using ControlCenter.Models.JOBS;
using ControlCenter.Models.JOBS.Parameters;
using ControlCenter.Models.JOBS.Parameters.GNSS;
using DrobitExtras;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WExtraControlLibrary.Extras;
using System.Windows;
using ControlCenter.Base;

namespace ControlCenter.ModelViews
{
    public class ProcessingViewModel : BindableModelBase
    {
        public ProcessingViewModel()
        {
            //LoadFromDiskProjectCallback(null);
        }

        // GUI Plot Exec
        private const string RTKPLOT_EXE = @".\Utils\rtkplot.exe";
        // ProjectJobs List
        public ObservableCollection<ProcessJobProject> JobProjectList { get; set; } = new ObservableCollection<ProcessJobProject>();

        private string newProjectName;
        public string NewProjectName
        {
            get { return newProjectName; }
            set { Set(ref newProjectName, value); }
        }


        // Comand from JobProcessListCOntrol
        private ICommand _addProcessingProjectCommand;
        public ICommand AddProcessingProjectCommand
        {
            get
            {
                _addProcessingProjectCommand = new CommandHandler(AddProcessingProjectCallback, true);
                return _addProcessingProjectCommand;
            }
        }

        // Comand from JobProcessListCOntrol
        private ICommand _loadProjectsCommand;
        public ICommand LoadProjectsCommand
        {
            get
            {
                _loadProjectsCommand = new CommandHandler(LoadFromDiskProjectCallback, true);
                return _loadProjectsCommand;
            }
        }

        // Comand from JobProcessListCOntrol
        private ICommand _openProjectsFolderCommand;
        public ICommand OpenProjectsFolderCommand
        {
            get
            {
                _openProjectsFolderCommand = new CommandHandler(OpenProjectsFolderCallback, true);
                return _openProjectsFolderCommand;
            }
        }

        // Comand from JobProcessListCOntrol
        private ICommand _removeProcessingProjectAndDiskFilesCommand;
        public ICommand RemoveProcessingProjectAndDiskFilesCommand
        {
            get
            {
                _removeProcessingProjectAndDiskFilesCommand = new CommandHandler(RemoveProcessingProjectAndDiskFilesCallback, true);
                return _removeProcessingProjectAndDiskFilesCommand;
            }
        }

        // Comand from JobProcessListCOntrol
        private ICommand _removeProcessingProjectCommand;
        public ICommand RemoveProcessingProjectCommand
        {
            get
            {
                _removeProcessingProjectCommand = new CommandHandler(RemoveProcessingProjectCallback, true);
                return _removeProcessingProjectCommand;
            }
        }



        private ICommand _openPlotCommand;
        public ICommand OpenPlotCommand
        {
            get
            {
                _openPlotCommand = new CommandHandler(OpenPlotCallback, true);
                return _openPlotCommand;
            }
        }

        private ICommand _openFolderCommand;
        public ICommand OpenFolderCommand
        {
            get
            {
                _openFolderCommand = new CommandHandler(OpenFolderCallback, true);
                return _openFolderCommand;
            }
        }

        public void LoadFromDiskProjectCallback(object obj)
        {
            bool msgShown = false;
            string pathProjects = DrobitTools.GetPROJECTSDrobitFolder();
            if (Directory.Exists(pathProjects))
            {
                string[] dirs = Directory.GetDirectories(pathProjects);
                foreach (string dir in dirs)
                {
                    string[] filesPrj = Directory.GetFiles(dir, "*.prj");
                    if (filesPrj.Length == 1)
                    {
                        try
                        {
                            string dataPrj = File.ReadAllText(filesPrj[0]);
                            var settings = new JsonSerializerSettings();
                            settings.TypeNameHandling = TypeNameHandling.Objects;

                            ProcessJobProject jobProject = JsonConvert.DeserializeObject<ProcessJobProject>(dataPrj, settings);
                            jobProject.PostDeserializeAction();
                            JobProjectList.Add(jobProject);
                        }
                        catch (Exception e) {
                            if (!msgShown)
                            {
                                MessageBox.Show("Some projects could not be loaded. They were created with previous version. Please remove them from PROJECTS folder");
                                msgShown = true;
                            }  
                        }
                        
                    }
                }
            }
            
            
        }

        private void OpenProjectsFolderCallback(object obj)
        {
            string folder = obj as string;
            if (folder.Equals("downloads"))
                Process.Start(DrobitTools.GetDOWNLOADSDrobitFolder());
            else
                Process.Start(DrobitTools.GetPROJECTSDrobitFolder());
        }

        private void RemoveProcessingProjectCallback(object obj)
        {
            ProcessJobProject jobProject = (obj as ProcessJobProject);

            foreach (IWorkJob job in jobProject.IndividualJobs)
            {
                if (job.Phase == WorkPhase.RUNNING)
                    job.StopJob();
            }

            JobProjectList.Remove(jobProject);
        }
        private void RemoveProcessingProjectAndDiskFilesCallback(object obj)
        {
            ProcessJobProject jobProject = (obj as ProcessJobProject);

            foreach (IWorkJob job in jobProject.IndividualJobs)
            {
                if (job.Phase == WorkPhase.RUNNING)
                    job.StopJob();
            }

            JobProjectList.Remove(jobProject);

            // Delete
            try
            {
                Directory.Delete(jobProject.Path, true);
            } catch (Exception) { }

            
        }


        private void OpenPlotCallback(object obj)
        {
            JobParameter outputParameter = obj as JobParameter;
            if (outputParameter.JobParamType == JobParameterType.RTKPOSUNIT)
            {
                CMDExecutor exec = new CMDExecutor();
                RTKPOSUnit posUnit = outputParameter as RTKPOSUnit;
                exec.Start(RTKPLOT_EXE, "\"" + posUnit.Path + "\"");
            }
           
        }

        private void OpenFolderCallback(object obj)
        {
            IWorkJob job = obj as IWorkJob;

            string outputURI = DrobitTools.ConcatenatePath(new string[] {job.WorkingFolder, "OUTPUT" });
            
            Process.Start(outputURI);


        }

        private void AddProcessingProjectCallback(object obj)
        {
            object[] objList = obj as object[];

            // Check there is no problem with folder
            string unfiltProjectName = (string)objList[0];
            string projectName = DrobitTools.NormalizeString4FS(unfiltProjectName);

            if (projectName == null)
            {
                NewProjectName = "Bad Project Name. Please enter valid one";
                return;
            }

            ListBox listBox = (ListBox)objList[1];
            string projectPath = DrobitTools.ConcatenatePath(new string[] { DrobitTools.GetPROJECTSDrobitFolder(), projectName });
            if (!DrobitTools.CheckFolderExists(projectPath))
            {
                DrobitTools.CreateFolderIfNotExists(projectPath);
                ProcessJobProject jobProject = ProcessJobProject.CreateNewProject(projectName, projectPath);
                JobProjectList.Add(jobProject);
                listBox.SelectedItem = jobProject;
                NewProjectName = "";
            }
            else
            {
                NewProjectName = "Project allready exists in disk";
            }

        }


    }
}
