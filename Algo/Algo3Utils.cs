using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon;

namespace LibScheduler.Algo
{
    public static class Algo3Utils
    {
        public static void AssignP2(SchedulerResourceList results, List<SchedulerJob> p2,
            List<SchedulerJob> restJobs)
        {
            if (p2.Count == 0)
            {
                return;
            }

            int idx = 0;

            while (idx < results.Count && p2.Count > 0)
            {
                SchedulerResource vec = results.CounterResource(idx);
                int i = 0;

                while (i < p2.Count)
                {
                    SchedulerJob job = p2[i];

                    if (vec.HasFreeSlot(job.ScheduleTime, job.EndTime, results.Config))
                    {
                        vec.AddJob(job);
                        p2.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }

                ++idx;
            }

            foreach (SchedulerJob j in p2)
            {
                restJobs.Add(j);
            }
        }

        public static void AssignP1AndN(SchedulerResourceList results, List<SchedulerJob> p1n,
            List<SchedulerJob> restJobs, SchedulerConfig config)
        {
            if (p1n.Count == 0)
            {
                return;
            }

            int idx = 0;
            bool useLastJob = config.ConsiderLastJobPosition;

            while (idx < results.Count && p1n.Count > 0)
            {
                SchedulerResource vec = results.CounterResource(idx);
                int i = 0;

                if (vec.Position != null && vec.Position.IsValid())
                {
                    p1n = SchedulerGeoUtils.SortJobsByDistanceTo(vec.Position, p1n);
                }
                else if (config.StartLocation != null && config.StartLocation.IsValid())
                {
                    p1n = SchedulerGeoUtils.SortJobsByDistanceTo(config.StartLocation, p1n);
                }

                while (i < p1n.Count)
                {
                    if (useLastJob && vec.JobList.Count > 0)
                    {
                        if (vec.JobList.Last<SchedulerJob>().Position != null && 
                            vec.JobList.Last<SchedulerJob>().Position.IsValid())
                        {
                            p1n = SchedulerGeoUtils.SortJobsByDistanceTo(vec.JobList.Last<SchedulerJob>().Position, p1n);
                        }
                    }

                    SchedulerJob job = p1n[i];
                    
                    TimeSpan diff = job.EndTime - job.ScheduleTime;
                    DateTime suggestedTime = new DateTime();

                    if (vec.HasFreeSlot((int)diff.TotalMinutes, results.Config, out suggestedTime))
                    {
                        long startTmp = job.StartTs;
                        long endTmp = job.EndTs;
                        job.StartTs = suggestedTime.Ticks;
                        job.EndTs = suggestedTime.AddMinutes(diff.TotalMinutes).Ticks;

                        vec.AddJob(job);
                        p1n.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }

                ++idx;
            }

            foreach (SchedulerJob j in p1n)
            {
                restJobs.Add(j);
            }
        }
    }
}