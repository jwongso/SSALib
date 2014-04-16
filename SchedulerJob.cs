using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using LibCommon;

namespace LibScheduler
{
    [DataContract]
    public class SchedulerJob
    {
        private List<SchedulerDistanceDuration> mSchedulerMatrix;

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int Priority { get; set; }

        [DataMember]
        public string Descriptions { get; set; }

        [DataMember]
        public int LocationId { get; set; }

        [DataMember]
        public GeoCoordinate Position { get; set; }

        [DataMember]
        public string Address { get; set; }
        
        [DataMember]
        public long StartTs { get; set; }

        [DataMember]
        public long EndTs { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public string ResourceId { get; set; }

        [DataMember]
        public List<SchedulerDistanceDuration> Matrix
        {
            get
            {
                if (mSchedulerMatrix == null)
                {
                    mSchedulerMatrix = new List<SchedulerDistanceDuration>();
                }

                return mSchedulerMatrix;
            }

            set
            {
                if (value != null)
                {
                    foreach (SchedulerDistanceDuration d in value)
                    {
                        Matrix.Add(d);
                    }
                }
            }
        }

        [IgnoreDataMember]
        public DateTime ScheduleTime 
        {
            get
            {
                return new DateTime(StartTs);
            }
        }

        [IgnoreDataMember]
        public DateTime EndTime
        {
            get
            {
                return new DateTime(EndTs);
            }
        }

        public SchedulerJob(string id)
        {
            Id = id;
        }

        public SchedulerJob()
        {
        }

        public SchedulerJob(SchedulerJob other)
        {
            Id = other.Id;
            Priority = other.Priority;
            Descriptions = other.Descriptions;
            LocationId = other.LocationId;
            Position = new GeoCoordinate(other.Position);
            Address = other.Address;
            StartTs = other.StartTs;
            EndTs = other.EndTs;
            JobType = other.JobType;
            ResourceId = other.ResourceId;
            Matrix = other.Matrix;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to SSAJob return false:
            SchedulerJob p = obj as SchedulerJob;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) &&
                string.Compare(this.Id, p.Id) == 0 &&
                this.Priority == p.Priority &&
                this.ScheduleTime == p.ScheduleTime &&
                this.EndTime == p.EndTime &&
                this.JobType == p.JobType &&
                this.Position == p.Position;
        }

        public bool Equals(SchedulerJob p)
        {
            // Return true if the fields match:
            return base.Equals(p) &&
                string.Compare(this.Id, p.Id) == 0 &&
                this.Priority == p.Priority &&
                this.ScheduleTime == p.ScheduleTime &&
                this.EndTime == p.EndTime &&
                this.JobType == p.JobType &&
                this.Position == p.Position;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ WaitingTimeMin;
        }

        public static bool operator ==(SchedulerJob a, SchedulerJob b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return string.Compare(a.Id, b.Id) == 0 &&
                a.Priority == b.Priority &&
                a.ScheduleTime == b.ScheduleTime &&
                a.EndTime == b.EndTime &&
                a.JobType == b.JobType &&
                a.Position == b.Position;
        }

        public static bool operator !=(SchedulerJob a, SchedulerJob b)
        {
            return !(a == b);
        }

        [IgnoreDataMember]
        public int WaitingTimeMin
        {
            get
            {
                if (EndTime != null && ScheduleTime != null)
                {
                    return (int)(EndTime - ScheduleTime).TotalMinutes;
                }
                else
                    return 0;
            }
        }

        [IgnoreDataMember]
        public string ScheduleTimeStr
        {
            get
            {
                return ScheduleTime.ToString();
            }
        }

        [IgnoreDataMember]
        public string EndTimeStr
        {
            get
            {
                return EndTime.ToString();
            }
        }

        [IgnoreDataMember]
        public double _Distance { get; set; }

        [IgnoreDataMember]
        public double _Bearing { get; set; }

        public void AddDistanceMatrix(int id, double distance)
        {
            if (!UpdateDistance(id, distance))
            {
                SchedulerDistanceDuration dd = new SchedulerDistanceDuration();
                dd.LocationId = id;
                dd.Distance = distance;

                Matrix.Add(dd);
            }
        }

        public void AddDurationMatrix(int id, double duration)
        {
            if (!UpdateDuration(id, duration))
            {
                SchedulerDistanceDuration dd = new SchedulerDistanceDuration();
                dd.LocationId = id;
                dd.Duration = duration;

                Matrix.Add(dd);
            }
        }

        public double DistanceTo(SchedulerJob other)
        {
            SchedulerDistanceDuration d = GetDistanceDurationById(other.LocationId);

            if (d != null)
            {
                return d.Distance;
            }

            return 0;
        }

        public double DistanceTo(int locationId)
        {
            SchedulerDistanceDuration d = GetDistanceDurationById(locationId);

            if (d != null)
            {
                return d.Distance;
            }

            return 0;
        }

        public double DurationTo(SchedulerJob other)
        {
            SchedulerDistanceDuration d = GetDistanceDurationById(other.LocationId);

            if (d != null)
            {
                return d.Duration;
            }

            return 0;
        }

        public double DurationTo(int locationId)
        {
            SchedulerDistanceDuration d = GetDistanceDurationById(locationId);

            if (d != null)
            {
                return d.Duration;
            }

            return 0;
        }

        bool UpdateDistance(int id, double distance)
        {
            SchedulerDistanceDuration d = GetDistanceDurationById(id);

            if (d != null)
            {
                d.Distance = distance;

                return true;
            }

            return false;
        }

        bool UpdateDuration(int id, double duration)
        {
            SchedulerDistanceDuration d = GetDistanceDurationById(id);

            if (d != null)
            {
                d.Duration = duration;

                return true;
            }

            return false;
        }

        public SchedulerDistanceDuration GetDistanceDurationById(int id)
        {
            try
            {
                SchedulerDistanceDuration d = Matrix.First(j => j.LocationId == id);

                if (d != null)
                {
                    return d;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
