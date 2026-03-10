using ControlCenter.Base;
using ControlCenter.Models.JOBS.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WExtraControlLibrary.Extras;

namespace ControlCenter.Models.JOBS
{
    public enum WorkPhase
    {
        UNDEF,
        [Description("STOP")]
        STOP,
        [Description("RUNNING")]
        RUNNING,
        [Description("COMPLETED")]
        COMPLETED,
        [Description("FAILED")]
        FAILED,
        [Description("CANCELLING")]
        CANCELLING
    }

    public enum WorkType
    {
        UNDEF,
        GNSSSTATIC,
        GNSSKINEMATIC,
        SFTPTRANSFER,
        CAMPROC
    }

    public enum GeneriInformationType
    {
        UNDEF,
        TRIGGER_UBX
    }

    public abstract class IWorkJob : BindableModelBase
    {
        public delegate void IWorkJobPhaseChangeDelegate(string Name_ID, WorkPhase phase, IWorkJob job);
        public event IWorkJobPhaseChangeDelegate IWorkJobPhaseChangeEvent;
        public delegate void IWorkJobPercentageChangeDelegate(string Name_ID, int percentage, IWorkJob job);
        public event IWorkJobPercentageChangeDelegate IWorkJobPercentageChangeEvent;

        public delegate void IWorkJobGenericInformationDelegate(GeneriInformationType type, JobParameter param);
        public event IWorkJobGenericInformationDelegate IWorkJobGenericInformationEvent;

        protected bool needsExit = false;

        private bool needInputParameterFromPrevious;
        public bool NeedInputParameterFromPrevious
        {
            get { return needInputParameterFromPrevious; }
            set { Set(ref needInputParameterFromPrevious, value); }
        }

        private int percentage;
        public int Percentage
        {
            get { return percentage; }
            set { Set(ref percentage, value);
                FireChangePercentageEvent();
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        private string workingFolder;
        public string WorkingFolder
        {
            get { return workingFolder; }
            set { Set(ref workingFolder, value); }
        }

        private WorkPhase phase;
        public WorkPhase Phase
        {
            get { return phase; }
            set {
                Set(ref phase, value);
                FireChangePhaseEvent();
            }
        }

        private WorkType worktype;
        public WorkType Worktype
        {
            get { return worktype; }
            set { Set(ref worktype, value); }
        }

        private string notes;
        public string Notes
        {
            get { return notes; }
            set { Set(ref notes, value); }
        }

        private JobParameter inputParameter;
        public JobParameter InputParameter
        {
            get { return inputParameter; }
            set { Set(ref inputParameter, value); }
        }

        private JobParameter outputParameter;
        public JobParameter OutputParameter
        {
            get { return outputParameter; }
            set { Set(ref outputParameter, value); }
        }

        // Comand from
        private ICommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                _startCommand = new CommandHandler(StartJobAsync, true);
                return _startCommand;
            }
        }

        // Comand from
        private ICommand _stopCommand;
        public ICommand StopCommand
        {
            get
            {
                _stopCommand = new CommandHandler(StopJob, true);
                return _stopCommand;
            }
        }

        private void StartJobAsync(object obj = null)
        {
            needsExit = false;
            if (Phase != WorkPhase.RUNNING)
                ThreadPool.QueueUserWorkItem(new WaitCallback(StartJob), obj);
            else
                StopJob();
        }

        public virtual void StopJob(object obj = null)
        {
            needsExit = true;
            Phase = WorkPhase.CANCELLING;
        }

        protected void StoppedJob()
        {
            Percentage = 0;
            Phase = WorkPhase.FAILED;
        }

        public virtual void StartJob(object obj = null)
        {
            System.Diagnostics.Debug.WriteLine("Started Job: " + Name);
        }

        protected void FireChangePhaseEvent()
        {
            // Launch Event
            IWorkJobPhaseChangeEvent?.Invoke(Name, Phase, this);
        }

        protected void FireChangePercentageEvent()
        {
            // Launch Event
            IWorkJobPercentageChangeEvent?.Invoke(Name, Percentage, this);
        }

        public IWorkJob()
        {
            Phase = WorkPhase.STOP;
        }
    }
}
