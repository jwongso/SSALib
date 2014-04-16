using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LibCommon;

namespace LibScheduler
{
    [DataContract]
    public class SchedulerConfig
    {
        public SchedulerConfig()
        {
            StartLocation = new GeoCoordinate();
            TypeConditionRules = new List<SchedulerTypeCondition>();
        }

        [DataMember]
        public GeoCoordinate StartLocation { get; set; }

        [DataMember]
        public int StartLocationId { get; set; }

        [DataMember]
        public int StartHour24 { get; set; }

        [DataMember]
        public int StartMinute { get; set; }

        [DataMember]
        public int EndHour24 { get; set; }

        [DataMember]
        public int EndMinute { get; set; }

        [DataMember]
        public long TimeStamp { get; set; }

        [DataMember]
        public int MinutesBetweenJobs { get; set; }

        [DataMember]
        public double DegreeWithinDirection { get; set; }

        [DataMember]
        public double TolerantDistanceToOvercomeDegreeInM { get; set; }

        [DataMember]
        public double PreferredMaxDistanceInKm { get; set; }

        [DataMember]
        public bool ConsiderLastJobPosition { get; set; }

        [DataMember]
        public List<SchedulerTypeCondition> TypeConditionRules { get; private set; }

        [DataMember]
        public int AfterStateException { get; set; }

        [DataMember]
        public double ResourceRadiusSearch { get; set; }

        [IgnoreDataMember]
        public DateTime Date
        {
            get
            {
                return new DateTime(TimeStamp);
            }
        }

        [IgnoreDataMember]
        public DateTime StartHour
        {
            get
            {
                return new DateTime(Date.Year, Date.Month, Date.Day, StartHour24, StartMinute, 0);
            }
        }

        [IgnoreDataMember]
        public DateTime LastHour
        {
            get
            {
                return new DateTime(Date.Year, Date.Month, Date.Day, EndHour24, EndMinute, 0);
            }
        }
    }

    [Flags]
    public enum ComfortFactor
    {
        NoneComfort         = 0x0,
        TimeComfort         = 0x1,
        DistanceComfort     = 0x2,
        DirectionComfort    = 0x4,
        TypeComfort         = 0x8
    }
}
