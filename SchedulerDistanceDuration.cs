using System;
using System.Runtime.Serialization;

namespace LibScheduler
{
    [DataContract]
    public class SchedulerDistanceDuration
    {
        [DataMember]
        public int LocationId { get; set; }

        [DataMember]
        public double Distance { get; set; }

        [DataMember]
        public double Duration { get; set; }
    }
}
