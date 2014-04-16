using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon;

namespace LibScheduler
{
    public class SchedulerResourceList
    {
        private readonly Random mRand = new Random();
        private readonly SchedulerConfig mConfig = null;

        public SchedulerResourceList(IEnumerable<SchedulerResource> enumerable, SchedulerConfig config)
        {
            List = new List<SchedulerResource>(enumerable);
            mConfig = config;
        }

        public SchedulerResourceList(SchedulerConfig config)
        {
            List = new List<SchedulerResource>();
            mConfig = config;
        }

        private List<SchedulerResource> List
        {
            get;
            set;
        }

        public SchedulerResource GetResourceById(string id)
        {
            return List.FirstOrDefault(r => r.Id == id);
        }

        public void Add(SchedulerResource resource)
        {
            List.Add(resource);
        }

        public void Clear()
        {
            List.Clear();
        }

        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        public SchedulerConfig Config
        {
            get
            {
                return mConfig;
            }
        }

        public SchedulerResource this[int index]
        {
            get
            {
                return List[index];
            }
        }

        public SchedulerResource RandomResource()
        {
            int r = 0;
            SchedulerResource v = null;

            if (List.Count > 0)
            {
                while (v == null)
                {
                    r = mRand.Next(1, List.Count);
                    v = List[r];
                }

                return v;
            }

            return null;
        }

        public SchedulerResource CounterResource(int offset)
        {
            int r = 0;
            SchedulerResource v = null;

            if (List.Count > 0)
            {
                r = (mConfig.StartHour.Day + offset) % List.Count;

                while (v == null)
                {
                    v = List[r];
                    r++;
                }

                return v;
            }

            return null;
        }
    }
}
