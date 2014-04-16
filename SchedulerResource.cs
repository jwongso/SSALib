using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using LibCommon;

namespace LibScheduler
{
    [DataContract]
    public class SchedulerResource
    {
        private List<SchedulerJob> mJobList;

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public int State { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public GeoCoordinate Position { get; set; }

        [DataMember]
        public List<SchedulerJob> JobList
        {
            get
            {
                if (mJobList == null)
                {
                    mJobList = new List<SchedulerJob>();
                }

                return mJobList;
            }

            set
            {
                foreach (SchedulerJob j in value)
                {
                    AddJob(j);
                }
            }
        }

        public void AddJob(SchedulerJob job)
        {
            job.ResourceId = this.Id;
            JobList.Add(job);
        }

        public bool HasJob(SchedulerJob job)
        {
            return JobList.Contains(job);
        }

        public bool RemoveJob(SchedulerJob job)
        {
            if (JobList.Count == 0)
            {
                return false;
            }

            bool success = JobList.Remove(job);

            if (success)
            {
                job.ResourceId = string.Empty;
            }
            else
            {
                SchedulerJob j = JobList.First(x => x.Id == job.Id);

                if (j != null)
                {
                    success = JobList.Remove(j);
                }
            }

            return success;
        }
    }
}
