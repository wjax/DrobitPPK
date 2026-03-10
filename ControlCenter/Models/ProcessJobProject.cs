using ControlCenter.Extras;
using ControlCenter.Models.JOBS;
using ControlCenter.Models.JOBS.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WExtraControlLibrary.Extras;

namespace ControlCenter.Models
{
    public class ProcessJobProject : IWorkJob
    {
        #region FIELDS

        private string path;
        [JsonProperty("Path")]
        public string Path
        {
            get { return path; }
            set { Set(ref path, value); }
        }

        [JsonProperty("IndividualJobs")]
        public ObservableCollection<IWorkJob> IndividualJobs { get; set; } = new ObservableCollection<IWorkJob>();

        #endregion

        #region COMMANDS
        // Comand from UI REC
        private ICommand _addTaskCommand;
        [JsonIgnore]
        public ICommand AddTaskCommand
        {
            get
            {
                _addTaskCommand = new CommandHandler(AddTaskCallback, true);
                return _addTaskCommand;
            }
        }

        // Comand from UI SAVE
        private ICommand _save2DiskCommand;
        [JsonIgnore]
        public ICommand Save2DiskCommand
        {
            get
            {
                _save2DiskCommand = new CommandHandler(Save2DiskCallback, true);
                return _save2DiskCommand;
            }
        }



        #endregion

        #region COMMANDCALLBACK

        private void Save2DiskCallback(object obj)
        {
            // Serielize
            String serialized = serialize();
            // Save to Disk
            File.WriteAllText(DrobitTools.ConcatenatePath(new string[] {Path, Name+".prj" }), serialized);
        }

        public String serialize()
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            
            return JsonConvert.SerializeObject(this, Formatting.Indented, settings);
        }

        private void AddTaskCallback(object obj)
        {
            if (obj is string)
            {
                string type = (string)obj;

                IWorkJob job = null;

                switch (type)
                {
                    case "GNSSSTATIC":
                        job = new GNSSPostProcessingJob(Path);
                        (job as GNSSPostProcessingJob).GNSSProcessingType = GNSSProcessingTYPE.STATIC;
                        //(job as GNSSPostProcessingJob).Worktype = WorkType.GNSSSTATIC;
                        (job as GNSSPostProcessingJob).BASEPositionType = BASEPositionTYPE.FROM_RINEX;
                        (job as GNSSPostProcessingJob).Initialize();
                        break;
                    case "GNSSKINEMATIC":
                        job = new GNSSPostProcessingJob(Path);
                        (job as GNSSPostProcessingJob).GNSSProcessingType = GNSSProcessingTYPE.KINEMATIC;
                        //(job as GNSSPostProcessingJob).Worktype = WorkType.GNSSKINEMATIC;
                        (job as GNSSPostProcessingJob).BASEPositionType = BASEPositionTYPE.FROM_RINEX;
                        (job as GNSSPostProcessingJob).Initialize();
                        break;
                    case "CAMERA":
                        job = new CAMProcessingJob(Path);
                        (job as CAMProcessingJob).Initialize();
                        break;
                }

                // Subscribe to events
                job.IWorkJobPhaseChangeEvent += Job_IWorkJobPhaseChangeEvent;
                job.IWorkJobPercentageChangeEvent += Job_IWorkJobPercentageChangeEvent;
                job.IWorkJobGenericInformationEvent += Job_IWorkJobGenericInformationEvent;
                IndividualJobs.Add(job);
            }
        }

        private void Job_IWorkJobGenericInformationEvent(GeneriInformationType type, JobParameter param)
        {
            switch (type)
            {
                case GeneriInformationType.TRIGGER_UBX:
                    CAMProcessingJob cJob = new CAMProcessingJob(Path);
                    IndividualJobs.Add(cJob);
                    break;
            }
        }

        private void Job_IWorkJobPercentageChangeEvent(string Name_ID, int percentage, IWorkJob job)
        {
            CalculateGlobalPercent();
        }

        private void CalculateGlobalPercent()
        {
            int numJobs = IndividualJobs.Count;

            int sum = 0;
            foreach (IWorkJob job in IndividualJobs)
                sum += job.Percentage;

            sum /= numJobs;

            if (100 - sum < 1)
                sum = 100;

            Percentage = sum;
        }

        private void Job_IWorkJobPhaseChangeEvent(string Name_ID, WorkPhase phase, IWorkJob job)
        {
            Phase = phase;

            //Notes = "x:" + job.Name;
            Notes = "x:" + job.Name.Substring(0, job.Name.IndexOf("_"));

            if (Phase == WorkPhase.RUNNING)
            {
                // Started work
                // Look for NeedInputParameters
                int currjobIndex = -1;
                int previousIndex = -1;
                for (int i = 0; i < IndividualJobs.Count; i++)
                    if (IndividualJobs[i].Equals(job))
                        currjobIndex = i;

                previousIndex = currjobIndex -1;

                if (previousIndex >= 0 && previousIndex < IndividualJobs.Count)
                {
                    if (IndividualJobs[currjobIndex].NeedInputParameterFromPrevious)
                    {
                        IndividualJobs[currjobIndex].InputParameter = IndividualJobs[previousIndex].OutputParameter;
                        if (IndividualJobs[currjobIndex] is CAMProcessingJob)
                        {
                            (IndividualJobs[currjobIndex] as CAMProcessingJob).RecalculateObsMaxMinTimes();
                            (IndividualJobs[currjobIndex] as CAMProcessingJob).ApplyObsMaxMinTimes();
                        }
                    }
                }

            }

            if (Phase == WorkPhase.COMPLETED)
            {
                // Look for NeedInputParameters
                int jobIndex = -1;
                for (int i = 0; i < IndividualJobs.Count; i++)
                    if (IndividualJobs[i].Equals(job))
                        jobIndex = i;

                jobIndex++;

                if (jobIndex > 0 && jobIndex < IndividualJobs.Count)
                    if (IndividualJobs[jobIndex].NeedInputParameterFromPrevious)
                    {
                        IndividualJobs[jobIndex].InputParameter = job.OutputParameter;
                        if (IndividualJobs[jobIndex] is CAMProcessingJob)
                        {
                            (IndividualJobs[jobIndex] as CAMProcessingJob).RecalculateObsMaxMinTimes();
                            (IndividualJobs[jobIndex] as CAMProcessingJob).ApplyObsMaxMinTimes();
                        }
                    }

            }
        }
        #endregion

        #region IWORKJOB_OVERRIDES
        // Start Job from ProcessJobProject Main Control
        public override void StartJob(object obj)
        {
            // Create Task and pass the list of jobs. It will start with the first task that is not completed and wait for it to finish. Then pass to the next one
            System.Diagnostics.Debug.WriteLine("Start ProcessJobProject: " + Name);
            //Setting WorkPhase to Running. Disallow Adding new tasks
            Phase = WorkPhase.RUNNING;
            Task task = new Task(() => StartJobProjectCallBack(IndividualJobs));
            task.Start();
        }

        private void StartJobProjectCallBack(ObservableCollection<IWorkJob> individualJobs)
        {
            //JobParameter Parameter = null;

            foreach(IWorkJob job in individualJobs)
            {
                // Parameter input
                //if (job.NeedInputParameterFromPrevious)
                //    job.InputParameter = Parameter;

                // Start
                job.StartJob();

                // Once Finished Store output in case it is needed
                //Parameter = job.OutputParameter;
            }

            Phase = WorkPhase.COMPLETED;
        }

        public override void StopJob(object obj)
        {
            foreach (IWorkJob job in IndividualJobs)
                job.StopJob();

        }
        #endregion

        public void PostDeserializeAction()
        {

            foreach (IWorkJob job in IndividualJobs)
            {
                // Subscribe to events
                job.IWorkJobPhaseChangeEvent += Job_IWorkJobPhaseChangeEvent;
                job.IWorkJobPercentageChangeEvent += Job_IWorkJobPercentageChangeEvent;
                job.IWorkJobGenericInformationEvent += Job_IWorkJobGenericInformationEvent;
            }
            
        }

        public static ProcessJobProject CreateNewProject(string name, string path)
        {
            ProcessJobProject jobProject = new ProcessJobProject();
            jobProject.Name = name;
            jobProject.Path = path;

            return jobProject;
        }
    }
}
