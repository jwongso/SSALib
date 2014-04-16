using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon;

namespace LibScheduler.Algo
{
    public static class Algo1Utils
    {
        public static void FineTuneP2(List<SchedulerJob> result, List<SchedulerJob> p2, 
            List<SchedulerJob> rest, SchedulerConfig config)
        {
            if (p2.Count == 0)
            {
                return;
            }

            foreach (SchedulerJob j in p2)
            {
                result.Add(j);
            }

            int i = 0;

            while (i + 1 < result.Count)
            {
                SchedulerJob j1 = result[i];
                SchedulerJob j2 = result[i + 1];

                if (j1.EndTime + TimeSpan.FromMinutes(j1.DurationTo(j2) + config.MinutesBetweenJobs) >= j2.ScheduleTime)
                {
                    result.Remove(j2);
                }
                else
                {
                    ++i;
                }
            }

            foreach (SchedulerJob j in result)
            {
                if (rest.Contains(j))
                {
                    rest.Remove(j);
                }
            }
        }

        public static void MergeAndFineTuneP2AndP1(List<SchedulerJob> results, List<SchedulerJob> p1, 
            List<SchedulerJob> rest, SchedulerConfig config)
        {
            int i = 0;

            DateTime startHour = config.StartHour;
            if (DateTime.Now.Date == config.StartHour.Date)
            {
                startHour = DateTime.Now;
                startHour = startHour.AddMinutes(30);
            }

            while (i < p1.Count)
            {
                SchedulerJob target = p1[i];

                if (results.Count == 0)
                {
                    results.Add(target);
                }

                for (int j = 0; j < results.Count; ++j)
                {
                    SchedulerJob job = results[j];

                    if (j == 0)
                    {
                        TimeSpan diff = job.ScheduleTime - startHour;

                        if (target.ScheduleTime < job.ScheduleTime)
                        {
                            TimeSpan diff2 = job.ScheduleTime - target.EndTime;

                            if (diff.TotalMinutes >= 60 &&
                                diff2.TotalMinutes >= config.MinutesBetweenJobs)
                            {
                                results.Insert(j, target);
                                break;
                            }
                            else if (diff.TotalMinutes >= config.MinutesBetweenJobs &&
                                diff2.TotalMinutes >= config.MinutesBetweenJobs)
                            {
                                double d1 = GeoUtils.CalculateBearing(
                                    config.StartLocation, target.Position);

                                double d2 = GeoUtils.CalculateBearing(
                                    config.StartLocation, job.Position);

                                if (Math.Abs(d2 - d1) <= config.DegreeWithinDirection)
                                {
                                    results.Insert(j, target);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Check index 0 and 1 if there's enough time slot
                            if (j + 1 < results.Count)
                            {
                                SchedulerJob next = results[j + 1];

                                if (job.EndTime + TimeSpan.FromMinutes(job.DurationTo(next)) < target.ScheduleTime &&
                                target.EndTime + TimeSpan.FromMinutes(target.DurationTo(next)) < next.ScheduleTime)
                                {
                                    results.Insert(j + 1, target);
                                    break;
                                }
                            }
                            else
                            {
                                // Last item in p2
                                if (target.ScheduleTime >= job.EndTime +
                                    TimeSpan.FromMinutes(job.DurationTo(target)) &&
                                    target.ScheduleTime <= config.LastHour)
                                {
                                    results.Add(target);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (target.ScheduleTime > job.EndTime + 
                            TimeSpan.FromMinutes(job.DurationTo(target)))
                        {
                            if (j + 1 < results.Count)
                            {
                                SchedulerJob next = results[j + 1];

                                if (job.EndTime + TimeSpan.FromMinutes(job.DurationTo(next)) < target.ScheduleTime &&
                                    target.EndTime + TimeSpan.FromMinutes(target.DurationTo(next)) < next.ScheduleTime)
                                {
                                    results.Insert(j + 1, target);
                                    break;
                                }
                            }
                            else
                            {
                                // Last item in p2

                                if (target.ScheduleTime >= job.EndTime +
                                    TimeSpan.FromMinutes(job.DurationTo(target)) &&
                                    target.ScheduleTime <= config.LastHour)
                                {
                                    results.Add(target);
                                    break;
                                }
                                else
                                {
                                    // p1 is okay to have late job delivery
                                    if (target.ScheduleTime <= config.LastHour)
                                    {
                                        results.Add(target);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                ++i;
            }

            foreach (SchedulerJob j in results)
            {
                if (rest.Contains(j))
                {
                    rest.Remove(j);
                }
            }
        }

        public static void MergeAndFineTunePAndNoPriority(List<SchedulerJob> results, List<SchedulerJob> n, 
            List<SchedulerJob> rest, SchedulerConfig config)
        {
            if (n.Count == 0)
            {
                return;
            }

            DateTime startHour = config.StartHour;
            if (DateTime.Now.Date == config.StartHour.Date)
            {
                startHour = DateTime.Now;
                startHour = startHour.AddMinutes(30);
            }

            if (results.Count == 0)
            {
                SchedulerJob target = n[0];
                results.Add(target);
            }

            int i = 0;

            while (i < results.Count)
            {
                SchedulerJob job = results[i];

                // Find the head
                if (i == 0)
                {
                    TimeSpan diff = job.ScheduleTime - startHour;

                    n = SchedulerGeoUtils.SortJobsByDistanceTo(
                        new GeoCoordinate(config.StartLocation), n);

                    double d = GeoUtils.CalculateBearing(
                        config.StartLocation, job.Position);

                    double distance = GeoUtils.CalculateAirDistanceInKm(
                        config.StartLocation, job.Position);

                    for (int j = 0; j < n.Count; ++j)
                    {
                        SchedulerJob target = n[j];

                        if (diff.TotalMinutes >= 60 ||
                            target._Distance <= 0.5)
                        {
                            target.StartTs = (job.ScheduleTime -
                                (TimeSpan.FromMinutes(job.WaitingTimeMin) +
                                TimeSpan.FromMinutes(target.DurationTo(job)))).Ticks;

                            results.Insert(i, target);
                            break;
                        }
                        else if (diff.TotalMinutes >= config.MinutesBetweenJobs)
                        {
                            double diffD = Math.Abs(d - target._Bearing);

                            if (diffD <= config.DegreeWithinDirection &&
                                target._Distance <= distance)
                            {
                                target.StartTs = (job.ScheduleTime -
                                    (TimeSpan.FromMinutes(job.WaitingTimeMin) +
                                    TimeSpan.FromMinutes(target.DurationTo(job)))).Ticks;

                                results.Insert(i, target);
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // Find a enough gap between jobs and add a non prio one
                    if (i + 1 < results.Count)
                    {
                        // Not the last job in p2
                        SchedulerJob next = results[i + 1];

                        TimeSpan diff = next.ScheduleTime - (job.EndTime +
                            TimeSpan.FromMinutes(job.DurationTo(next)));

                        if (diff.TotalMinutes >= config.MinutesBetweenJobs)
                        {
                            n = SchedulerGeoUtils.SortJobsByDistanceTo(job, n);

                            double d = GeoUtils.CalculateBearing(
                                job.Position, next.Position);

                            double distance = GeoUtils.CalculateAirDistanceInKm(
                                job.Position, next.Position);

                            for (int j = 0; j < n.Count; ++j)
                            {
                                SchedulerJob target = n[j];

                                if ((Math.Abs(d - target._Bearing) <= config.DegreeWithinDirection &&
                                    target._Distance <= distance) ||
                                    (target._Distance < 0.5))
                                {
                                    target.StartTs = (job.EndTime + TimeSpan.FromMinutes(target.DurationTo(job))).Ticks;
                                    results.Insert(i + 1, target);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Last job in p2
                        TimeSpan diff = config.LastHour - job.EndTime;

                        if (diff.TotalMinutes >= 0)
                        {
                            n = SchedulerGeoUtils.SortJobsByDistanceTo(job.Position, n);

                            SchedulerJob target = n[0];

                            target.StartTs = (job.EndTime +
                                TimeSpan.FromMinutes(job.DurationTo(target))).Ticks;

                            results.Add(target);
                        }
                    }
                }

                ++i;
            }

            foreach (SchedulerJob j in results)
            {
                if (rest.Contains(j))
                {
                    rest.Remove(j);
                }
            }
        }
    }
}
