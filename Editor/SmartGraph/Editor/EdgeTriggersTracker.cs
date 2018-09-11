using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Events;

namespace SmartData.Graph
{
    public static class EdgeTriggersTracker
    {
        public class EdgeTrigger
        {
            public Edge edge;
			public bool direction;
            public float triggeredTime;
        }

        private readonly static float TimeToLive = 1f;
        private static List<EdgeTrigger> triggers = new List<EdgeTrigger>();
		private static List<float> timings = new List<float>();
        public static void RegisterTrigger(Edge edge, bool dir)
        {
            triggers.Add(new EdgeTrigger() { edge = edge, triggeredTime = Time.unscaledTime , direction = dir});
        }


        public static List<float> GetTimings(Edge edge)
        {
            float now = Time.unscaledTime;
            List<EdgeTrigger> acceptedTriggers = triggers.FindAll(t => t.edge == edge);

			timings.Clear();
            foreach (EdgeTrigger t in acceptedTriggers)
            {
				float delay = !t.direction ? TimeToLive : 0;

				float time = Mathf.Abs((t.triggeredTime - now)) / TimeToLive;
				time = time - delay;
                if (time <= 1f && time >= 0f)
                {
					timings.Add(t.direction? time : 1-time);
                }
                if(time > 1f)
                {
                    triggers.Remove(t);
                }
            }
            return timings;
        }

        public static void CleanObsolete()
        {
            float now = Time.unscaledTime;
            triggers.RemoveAll(trigger => Mathf.Abs(now - trigger.triggeredTime) > TimeToLive);
        }

        public static bool HasData()
        {
            return triggers.Count > 0;
        }
    }
}