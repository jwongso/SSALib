using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibCommon;

namespace LibScheduler.Algo
{
    public static class Algo4Utils
    {
        public static List<SchedulerResource> GetResourceInRadius(
            List<SchedulerResource> list, 
            List<SchedulerResource> exceptions,
            SchedulerJob job, 
            SchedulerConfig config)
        {
            double radius = config.ResourceRadiusSearch;
            List<SchedulerResource> results = new List<SchedulerResource>();

            foreach (SchedulerResource res in list)
            {
                if (exceptions.Count > 0)
                {
                    SchedulerResource exc = exceptions.FirstOrDefault(x => x.Id == res.Id && x.Type == res.Type);

                    if (exc != null)
                    {
                        continue;
                    }
                }

                if (res.Position.IsValid() && job.Position.IsValid())
                {
                    double distance = res.Position.CalculateAirDistanceInKm(job.Position);

                    if (distance <= radius)
                    {
                        results.Add(res);
                    }
                }
            }

            return results;
        }

        public static SchedulerResource GetNearestResource(
            List<SchedulerResource> list, 
            SchedulerJob job)
        {
            double minDistance = 99999.99;
            SchedulerResource result = null;

            foreach (SchedulerResource res in list)
            {
                if (res.Position.IsValid() && job.Position.IsValid())
                {
                    double distance = res.Position.CalculateAirDistanceInKm(job.Position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result = res;
                    }
                }
            }

            return result;
        }

        private static void CheckIsEnum<T>(bool withFlags)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
            if (withFlags && !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
                throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
        }

        public static bool IsFlagSet<T>(this T value, T flag) where T : struct
        {
            CheckIsEnum<T>(true);
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public static void AssignP2(SchedulerResourceList results, List<SchedulerJob> p2,
            List<SchedulerJob> restJobs, ComfortFactor factor)
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
                        ComfortFactor f = Analyze(job, vec.JobList, results.Config);

                        if (f.IsFlagSet(factor))
                        {
                            vec.AddJob(job);
                            p2.RemoveAt(i);
                        }
                        else
                        {
                            ++i;
                        }
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

        public static void AssignP1(SchedulerResourceList results, List<SchedulerJob> p1,
            List<SchedulerJob> restJobs, SchedulerConfig config, ComfortFactor factor)
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

                if (factor.IsFlagSet(ComfortFactor.DirectionComfort))
                {
                    if (vec.Position != null && vec.Position.IsValid())
                    {
                        p1 = SchedulerGeoUtils.SortJobsByDistanceTo(vec.Position, p1);
                    }
                    else if (config.StartLocation != null && config.StartLocation.IsValid())
                    {
                        p1 = SchedulerGeoUtils.SortJobsByDistanceTo(config.StartLocation, p1);
                    }
                }

                while (i < p1.Count)
                {
                    SchedulerJob job = p1[i];

                    TimeSpan diff = job.EndTime - job.ScheduleTime;
                    DateTime suggestedTime = new DateTime();

                    if (vec.HasFreeSlot((int)diff.TotalMinutes, results.Config, out suggestedTime))
                    {
                        long startTmp = job.StartTs;
                        long endTmp = job.EndTs;
                        job.StartTs = suggestedTime.Ticks;
                        job.EndTs = suggestedTime.AddMinutes(diff.TotalMinutes).Ticks;

                        ComfortFactor f = Analyze(job, vec.JobList, results.Config);

                        if (f.IsFlagSet(factor))
                        {
                            vec.AddJob(job);
                            p1.RemoveAt(i);
                        }
                        else
                        {
                            job.StartTs = startTmp;
                            job.EndTs = endTmp;

                            ++i;
                        }
                    }
                    else
                    {
                        ++i;
                    }
                }

                ++idx;
            }

            foreach (SchedulerJob j in p1)
            {
                restJobs.Add(j);
            }
        }

        public static void AssignN(SchedulerResourceList results, List<SchedulerJob> n,
            List<SchedulerJob> restJobs, SchedulerConfig config, ComfortFactor factor)
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

                if (factor.IsFlagSet(ComfortFactor.DirectionComfort))
                {
                    if (vec.Position != null && vec.Position.IsValid())
                    {
                        n = SchedulerGeoUtils.SortJobsByDistanceTo(vec.Position, n);
                    }
                    else if (config.StartLocation != null && config.StartLocation.IsValid())
                    {
                        n = SchedulerGeoUtils.SortJobsByDistanceTo(config.StartLocation, n);
                    }
                }

                while (i < n.Count)
                {
                    SchedulerJob job = n[i];

                    TimeSpan diff = job.EndTime - job.ScheduleTime;
                    DateTime suggestedTime = new DateTime();

                    if (vec.HasFreeSlot((int)diff.TotalMinutes, results.Config, out suggestedTime))
                    {
                        long startTmp = job.StartTs;
                        long endTmp = job.EndTs;
                        job.StartTs = suggestedTime.Ticks;
                        job.EndTs = suggestedTime.AddMinutes(diff.TotalMinutes).Ticks;

                        ComfortFactor f = Analyze(job, vec.JobList, results.Config);

                        if (f.IsFlagSet(factor))
                        {
                            job.StartTs = suggestedTime.Ticks;
                            vec.AddJob(job);
                            n.RemoveAt(i);
                        }
                        else
                        {
                            job.StartTs = startTmp;
                            job.EndTs = endTmp;

                            ++i;
                        }
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

        public static ComfortFactor Analyze(SchedulerJob job, List<SchedulerJob> list, SchedulerConfig config)
        {
            ComfortFactor factor = ComfortFactor.NoneComfort;

            if (list.Count == 0)
            {
                factor |= ComfortFactor.TimeComfort;
                factor |= ComfortFactor.DirectionComfort;
                factor |= ComfortFactor.DistanceComfort;
                factor |= ComfortFactor.TypeComfort;
            }
            else
            {
                SchedulerJob p = GetPreviousJob(job, list, config);
                SchedulerJob n = GetNextJob(job, list, config);

                if (p != null && n != null)
                {
                    factor = Analyze(p, job, n, config);
                }
                else if (p != null)
                {
                    factor = Analyze(p, job, config);
                }
                else if (n != null)
                {
                    factor = Analyze(n, job, config);
                }
            }

            return factor;
        }

        public static SchedulerJob GetPreviousJob(SchedulerJob job, List<SchedulerJob> list, SchedulerConfig config)
        {
            List<SchedulerJob> l = list.Where(
                    s => s.ScheduleTime.Date == config.StartHour.Date).OrderBy(s => s.ScheduleTime).ToList();

            int i = 0;
            int index = -1;

            while (i < l.Count)
            {
                SchedulerJob p = l[i];

                if (p.EndTime < job.ScheduleTime)
                {
                    index = i;
                }

                i++;
            }

            return index >= 0 ? l[index] : null;
        }

        public static SchedulerJob GetNextJob(SchedulerJob job, List<SchedulerJob> list, SchedulerConfig config)
        {
            List<SchedulerJob> l = list.Where(
                    s => s.ScheduleTime.Date == config.StartHour.Date).OrderBy(s => s.ScheduleTime).ToList();

            int i = 0;
            int index = -1;

            while (i < l.Count)
            {
                SchedulerJob p = l[i];

                if (job.EndTime < p.ScheduleTime)
                {
                    index = i;
                }

                i++;
            }

            return index >= 0 ? l[index] : null;
        }

        public static ComfortFactor Analyze(SchedulerJob prev, SchedulerJob current,
            SchedulerJob next, SchedulerConfig config)
        {
            ComfortFactor factor = ComfortFactor.NoneComfort;

            // Check time
            TimeSpan pDiff = current.ScheduleTime - prev.EndTime;
            TimeSpan nDiff = next.ScheduleTime - current.EndTime;

            if (pDiff.TotalMinutes >= config.MinutesBetweenJobs &&
                nDiff.TotalMinutes >= config.MinutesBetweenJobs)
            {
                factor |= ComfortFactor.TimeComfort;
            }

            // Check distance
            double pDistance = prev.DistanceTo(current);
            if (pDistance == 0)
            {
                pDistance = prev.Position.CalculateAirDistanceInKm(current.Position);
            }

            double nDistance = next.DistanceTo(current);
            if (nDistance == 0)
            {
                nDistance = next.Position.CalculateAirDistanceInKm(current.Position);
            }

            if (pDistance <= config.PreferredMaxDistanceInKm &&
                nDistance <= config.PreferredMaxDistanceInKm)
            {
                factor |= ComfortFactor.DistanceComfort;
            }

            // Check direction
            double prevToNext = prev.Position.CalculateBearing(next.Position);
            double prevToCurrent = prev.Position.CalculateBearing(current.Position);
            double currentToNext = current.Position.CalculateBearing(next.Position);

            if (Math.Abs(prevToNext - prevToCurrent) <= config.DegreeWithinDirection &&
                Math.Abs(prevToNext - currentToNext) <= config.DegreeWithinDirection)
            {
                factor |= ComfortFactor.DirectionComfort;
            }
            else if (pDistance * 1000 <= config.TolerantDistanceToOvercomeDegreeInM)
            {
                factor |= ComfortFactor.DirectionComfort;
            }

            // Check Type
            SchedulerTypeCondition prevCond = ConditionOf(prev, config);
            SchedulerTypeCondition currentCond = ConditionOf(current, config);
            SchedulerTypeCondition nextCond = ConditionOf(next, config);

            if (prevCond != null && currentCond != null && nextCond != null)
            {
                if (prevCond.AfterState == currentCond.BeforeState &&
                    currentCond.AfterState == nextCond.BeforeState)
                {
                    factor |= ComfortFactor.TypeComfort;
                }
                else if (prevCond.AfterState == config.AfterStateException &&
                    currentCond.AfterState == config.AfterStateException)
                {
                    factor |= ComfortFactor.TypeComfort;
                }
            }

            return factor;
        }

        public static ComfortFactor Analyze(SchedulerJob other, SchedulerJob current,
            SchedulerConfig config)
        {
            ComfortFactor factor = ComfortFactor.DirectionComfort;
            bool isPrev = false;

            // Check time
            TimeSpan diff;

            if (other.ScheduleTime > current.EndTime)
            {
                // Next
                diff = other.ScheduleTime - current.EndTime;
            }
            else
            {
                // Prev
                diff = current.EndTime - other.ScheduleTime;
                isPrev = true;
            }

            if (diff.TotalMinutes >= config.MinutesBetweenJobs)
            {
                factor |= ComfortFactor.TimeComfort;
            }

            // Distance
            double distance = other.DistanceTo(current);
            if (distance == 0)
            {
                distance = other.Position.CalculateAirDistanceInKm(current.Position);
            }

            if (distance <= config.PreferredMaxDistanceInKm)
            {
                factor |= ComfortFactor.DistanceComfort;
            }

            // Check Type
            SchedulerTypeCondition otherCond = ConditionOf(other, config);
            SchedulerTypeCondition currentCond = ConditionOf(current, config);

            if (otherCond != null && currentCond != null)
            {
                if (isPrev)
                {
                    if (otherCond.AfterState == currentCond.BeforeState)
                    {
                        factor |= ComfortFactor.TypeComfort;
                    }
                    else if (otherCond.AfterState == config.AfterStateException)
                    {
                        factor |= ComfortFactor.TypeComfort;
                    }
                }
                else
                {
                    if (currentCond.AfterState == otherCond.BeforeState)
                    {
                        factor |= ComfortFactor.TypeComfort;
                    }
                    else if (currentCond.AfterState == config.AfterStateException)
                    {
                        factor |= ComfortFactor.TypeComfort;
                    }
                }
            }

            return factor;
        }

        public static SchedulerTypeCondition ConditionOf(SchedulerJob job, SchedulerConfig config)
        {
            int type = job.JobType;

            foreach (SchedulerTypeCondition t in config.TypeConditionRules)
            {
                if (t.JobType == type)
                {
                    return t;
                }
            }

            return null;
        }
    }
}
