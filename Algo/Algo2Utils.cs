using System;
using System.Collections.Generic;
using System.Linq;

namespace LibScheduler.Algo
{
    public static class Algo2Utils
    {
        public static void AssignP2(SchedulerResourceList results, List<SchedulerJob> p2, List<SchedulerJob> restJobs)
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

        public static void AssignP1(SchedulerResourceList results, List<SchedulerJob> p1, List<SchedulerJob> restJobs)
        {
            if (p1.Count == 0)
            {
                return;
            }

            int idx = 0;

            while (idx < results.Count && p1.Count > 0)
            {
                SchedulerResource vec = results.CounterResource(idx);
                int i = 0;

                while (i < p1.Count)
                {
                    SchedulerJob job = p1[i];

                    if (vec.HasFreeSlot(job.ScheduleTime, job.EndTime, results.Config))
                    {
                        vec.AddJob(job);
                        p1.RemoveAt(i);
                    }
                    else
                    {
                        TimeSpan diff = job.EndTime - job.ScheduleTime;
                        DateTime suggestedTime = new DateTime();

                        if (vec.HasFreeSlot((int)diff.TotalMinutes, results.Config, out suggestedTime))
                        {
                            job.StartTs = suggestedTime.Ticks;
                            job.EndTs = suggestedTime.AddMinutes(diff.TotalMinutes).Ticks;
                            vec.AddJob(job);
                            p1.RemoveAt(i);
                        }
                        else
                        {
                            ++i;
                        }
                    }
                }

                ++idx;
            }

            foreach (SchedulerJob j in p1)
            {
                restJobs.Add(j);
            }
        }

        public static void AssignN(SchedulerResourceList results, List<SchedulerJob> n, List<SchedulerJob> restJobs)
        {
            if (n.Count == 0)
            {
                return;
            }

            int idx = 0;

            while (idx < results.Count && n.Count > 0)
            {
                SchedulerResource vec = results.CounterResource(idx);
                int i = 0;

                while (i < n.Count)
                {
                    SchedulerJob job = n[i];

                    TimeSpan diff = job.EndTime - job.ScheduleTime;
                    DateTime suggestedTime = new DateTime();

                    if (vec.HasFreeSlot((int)diff.TotalMinutes, results.Config, out suggestedTime))
                    {
                        job.StartTs = suggestedTime.Ticks;
                        job.EndTs = suggestedTime.AddMinutes(diff.TotalMinutes).Ticks;
                        vec.AddJob(job);
                        n.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }

                ++idx;
            }

            foreach (SchedulerJob j in n)
            {
                restJobs.Add(j);
            }
        }

        public static bool HasFreeSlot(this SchedulerResource resource, DateTime start, DateTime end, SchedulerConfig config)
        {
            if (resource.JobList.Count == 0)
            {
                return true;
            }
            else
            {
                // If the given start time is before or after config's start and end, 
                // then there's no free slot!
                if (start < config.StartHour || start > config.LastHour)
                {
                    return false;
                }

                List<SchedulerJob> jobList = resource.JobList.Where(
                    s => s.ScheduleTime.Date == config.StartHour.Date).OrderBy(s => s.ScheduleTime).ToList();

                foreach (SchedulerJob j in jobList)
                {
                    if ((start >= j.ScheduleTime && start <= j.EndTime) ||
                        (end >= j.ScheduleTime && end <= j.EndTime) ||
                        (j.ScheduleTime >= start && j.ScheduleTime <= end) ||
                        (j.EndTime >= start && j.EndTime <= end))
                    {
                        return false;
                    }

                    if (j.EndTime < start)
                    {
                        // Prev
                        if ((start - j.EndTime).TotalMinutes < config.MinutesBetweenJobs)
                        {
                            return false;
                        }
                    }
                    else if (end < j.ScheduleTime)
                    {
                        // Next
                        if ((j.ScheduleTime - end).TotalMinutes < config.MinutesBetweenJobs)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool HasFreeSlot(this SchedulerResource resource, int minutes, SchedulerConfig config, out DateTime suggestedTime)
        {
            DateTime startHour = config.StartHour;
            if (DateTime.Now.Date == config.StartHour.Date)
            {
                startHour = DateTime.Now;
                startHour = startHour.AddMinutes(30);
            }

            if (resource.JobList.Count == 0)
            {
                suggestedTime = startHour;
                return true;
            }
            else
            {
                List<SchedulerJob> jobList = resource.JobList.Where(
                    s => s.ScheduleTime.Date == config.StartHour.Date).OrderBy(s => s.ScheduleTime).ToList();

                int i = 0;

                while (i < jobList.Count)
                {
                    SchedulerJob job = jobList[i];

                    if (i == 0)
                    {
                        TimeSpan diff = job.ScheduleTime - startHour;

                        if (diff.TotalMinutes >= minutes + config.MinutesBetweenJobs)
                        {
                            suggestedTime = startHour;
                            return true;
                        }
                    }

                    if (i + 1 < jobList.Count)
                    {
                        SchedulerJob next = jobList[i + 1];

                        TimeSpan diff = next.ScheduleTime - job.EndTime;

                        if (diff.TotalMinutes >= minutes + (config.MinutesBetweenJobs * 2))
                        {
                            suggestedTime = job.EndTime + TimeSpan.FromMinutes(config.MinutesBetweenJobs);
                            return true;
                        }
                    }
                    else
                    {
                        TimeSpan diff = config.LastHour - job.EndTime;

                        if (diff.TotalMinutes >= minutes + config.MinutesBetweenJobs)
                        {
                            suggestedTime = job.EndTime + TimeSpan.FromMinutes(config.MinutesBetweenJobs);
                            return true;
                        }
                    }

                    ++i;
                }
            }

            suggestedTime = config.LastHour;
            return false;
        }
    }
}