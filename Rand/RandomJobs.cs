using System;
using System.Collections.Generic;
using System.Linq;
using LibScheduler;
using LibCommon;

namespace LibScheduler.Rand
{
    public class RandomJobs
    {
        public static List<SchedulerJob> Random1(string number, string start, string end, string priority)
        {
            uint n = 50;
            DateTime s = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
            DateTime e = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 0, 0);
            uint p = 3;
            Random rand = new Random();

            Parse(number, ref n);

            if (n == 0)
            {
                n = 50;
            }

            Parse(start, ref s);
            Parse(end, ref e);

            if (e < s)
            {
                System.Diagnostics.Debug.WriteLine("Endtime earlier than Starttime");
                e = s.AddHours(8);
            }

            Parse(priority, ref p);

            if (p > 3)
            {
                p = 3;
            }

            if (s.Date == DateTime.Now.Date)
            {
                if (DateTime.Now.Hour > s.Hour)
                {
                    TimeSpan diff = DateTime.Now - s;
                    s = s.AddMinutes(diff.TotalMinutes + 30);
                }
            }

            List<SchedulerJob> list = new List<SchedulerJob>();

            GeoRect rect = new GeoRect(new GeoCoordinate(1.424986, 103.674145), 
                new GeoCoordinate(1.304771, 103.929462));

            for (int i = 0; i < n; ++i)
            {
                SchedulerJob j = new SchedulerJob("Job #" + (i + 1).ToString());
                int hourStart = rand.Next(s.Hour, e.Hour);
                int minuteStart = rand.Next(0, 60);
                j.StartTs = new DateTime(s.Year, s.Month, s.Day, hourStart, minuteStart, 0).Ticks;
                int durationMin = rand.Next(30, 121);
                j.EndTs = new DateTime(j.StartTs).AddMinutes(durationMin).Ticks;
                j.Descriptions = "Randomized job";
                j.LocationId = i;
                j.ResourceId = string.Empty;
                j.Address = string.Empty;
                j.JobType = rand.Next(1, 5);

                if (p == 3)
                {
                    j.Priority = rand.Next(0, 3);
                }
                else
                {
                    j.Priority = (int)p;
                }
                double rndLat = rand.NextDouble() * 
                    (rect.TopLeft.Latitude - rect.BottomRight.Latitude) + rect.BottomRight.Latitude;
                double rndLng = rand.NextDouble() *
                    (rect.BottomRight.Longitude - rect.TopLeft.Longitude) + rect.TopLeft.Longitude;
                j.Position = new GeoCoordinate(rndLat, rndLng);

                list.Add(j);
            }

            return list;
        }

        public static bool Parse(string d, ref DateTime date)
        {
            try
            {
                long startNumber = long.Parse(d);
                date = new DateTime(startNumber);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Parsing d: " + d + ", result:" + ex.Message);
            }

            return false;
        }

        public static bool Parse(string n, ref uint number)
        {
            try
            {
                number = uint.Parse(n);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Parsing n: " + n + ", result:" + ex.Message);
            }

            return false;
        }

        public static bool Parse(string n, ref int number)
        {
            try
            {
                number = int.Parse(n);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Parsing n: " + n + ", result:" + ex.Message);
            }

            return false;
        }
    }
}
