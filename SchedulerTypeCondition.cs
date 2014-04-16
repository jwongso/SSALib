using System;
using System.Runtime.Serialization;

namespace LibScheduler
{
    [DataContract]
    public class SchedulerTypeCondition
    {
        public SchedulerTypeCondition(int jobType, int beforeState, int afterState)
        {
            JobType = jobType;
            BeforeState = beforeState;
            AfterState = afterState;
        }

        public SchedulerTypeCondition()
        {
        }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public int BeforeState { get; set; }

        [DataMember]
        public int AfterState { get; set; }
    }
}
