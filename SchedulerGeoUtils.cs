using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon;

namespace LibScheduler
{
    public class SchedulerGeoUtils
    {
        public static int FindNearestJobByIndex(GeoCoordinate start, List<SchedulerJob> jobList)
        {
            double nearest = 9999.99D;
            int i = 0;
            int index = 0;

            foreach (SchedulerJob j in jobList)
            {
                double dist = GeoUtils.CalculateAirDistanceInKm(start, j.Position);

                if (dist < nearest)
                {
                    nearest = dist;
                    index = i;
                }

                i++;
            }

            return index;
        }

        public static List<SchedulerJob> SortJobsByDistanceTo(SchedulerJob start, List<SchedulerJob> jobList)
        {
            int id = start.LocationId;

            foreach (SchedulerJob j in jobList)
            {
                if (id < j.Matrix.Count)
                {
                    j._Distance = j.Matrix[id].Distance;
                }
                else
                {
                    j._Distance = GeoUtils.CalculateAirDistanceInKm(start.Position, j.Position);
                }

                j._Bearing = start.Position.CalculateBearing(j.Position);
            }

            return jobList.OrderBy(job => job._Distance).ToList();
        }

        public static List<SchedulerJob> SortJobsByDistanceTo(GeoCoordinate start, List<SchedulerJob> jobList)
        {
            foreach (SchedulerJob j in jobList)
            {
                j._Distance = GeoUtils.CalculateAirDistanceInKm(start, j.Position);

                j._Bearing = GeoUtils.CalculateBearing(start, j.Position);
            }

            return jobList.OrderBy(job => job._Distance).ToList();
        }
    }
}
