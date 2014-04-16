using System;
using System.Collections.Generic;
using System.Linq;

namespace LibScheduler.Algo
{
    public class Algorithm
    {
        [Obsolete("Don't use A1, use A2 or A3 instead")]
        public static List<List<SchedulerJob>> DoAlgo1(List<SchedulerJob> jobList, SchedulerConfig config)
        {
            List<SchedulerJob> restJobs = new List<SchedulerJob>(
                jobList.Where(job => job.ScheduleTime.Date == config.Date).OrderBy(job => job.ScheduleTime));

            List<List<SchedulerJob>> final = new List<List<SchedulerJob>>();

            while (restJobs.Count > 0)
            {
                List<SchedulerJob> p2 = new List<SchedulerJob>(
                    restJobs.Where(job => job.Priority == 2).OrderBy(job => job.ScheduleTime));

                List<SchedulerJob> p1 = new List<SchedulerJob>(
                    restJobs.Where(job => job.Priority == 1).OrderBy(job => job.ScheduleTime));

                List<SchedulerJob> n = new List<SchedulerJob>(
                    restJobs.Where(job => job.Priority == 0));

                List<SchedulerJob> results = new List<SchedulerJob>();

                Algo1Utils.FineTuneP2(results, p2, restJobs, config);

                Algo1Utils.MergeAndFineTuneP2AndP1(results, p1, restJobs, config);

                Algo1Utils.MergeAndFineTunePAndNoPriority(results, n, restJobs, config);

                final.Add(results);
            }

            return final;
        }

        public static SchedulerResourceList DoAlgo2(List<SchedulerJob> jobList, List<SchedulerJob> restJobs, 
            List<SchedulerResource> vecList, SchedulerConfig config)
        {
            jobList = jobList.Where(
                job => job.ScheduleTime.Date == config.StartHour.Date).OrderBy(job => job.ScheduleTime).ToList();

            List<SchedulerJob> p2 = new List<SchedulerJob>(
                jobList.Where(job => job.Priority == 2).OrderBy(job => job.ScheduleTime));

            List<SchedulerJob> p1 = new List<SchedulerJob>(
                jobList.Where(job => job.Priority == 1).OrderBy(job => job.ScheduleTime));
            
            List<SchedulerJob> n = new List<SchedulerJob>(
                jobList.Where(job => job.Priority == 0));

            SchedulerResourceList results = new SchedulerResourceList(config);

            foreach (SchedulerResource vec in vecList)
            {
                results.Add(vec);
            }

            Algo2Utils.AssignP2(results, p2, restJobs);

            Algo2Utils.AssignP1(results, p1, restJobs);

            Algo2Utils.AssignN(results, n, restJobs);

            return results;
        }

        public static SchedulerResourceList DoAlgo3(List<SchedulerJob> jobList, List<SchedulerJob> restJobs,
            List<SchedulerResource> vecList, SchedulerConfig config)
        {
            jobList = jobList.Where(
                job => job.ScheduleTime.Date == config.StartHour.Date).OrderBy(job => job.ScheduleTime).ToList();

            List<SchedulerJob> p2 = new List<SchedulerJob>(
                jobList.Where(job => job.Priority == 2).OrderBy(job => job.ScheduleTime));

            List<SchedulerJob> p1n = new List<SchedulerJob>(
                jobList.Where(job => job.Priority != 2).OrderBy(job => job.ScheduleTime));

            SchedulerResourceList results = new SchedulerResourceList(config);

            foreach (SchedulerResource vec in vecList)
            {
                results.Add(vec);
            }

            Algo3Utils.AssignP2(results, p2, restJobs);

            Algo3Utils.AssignP1AndN(results, p1n, restJobs, config);

            return results;
        }
    }
}
