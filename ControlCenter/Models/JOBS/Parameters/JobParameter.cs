using ControlCenter.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCenter.Models.JOBS.Parameters
{
    public enum JobParameterType
    {
        RINEXUNIT,
        RTKPOSUNIT,
        DROBITCAMUNIT,
        POSITIONUNIT
    }

    public abstract class JobParameter : BindableModelBase
    {
        private JobParameterType jobparamType;
        public JobParameterType JobParamType
        {
            get { return jobparamType; }
            set
            {
                Set(ref jobparamType, value);
            }
        }
    }
}
