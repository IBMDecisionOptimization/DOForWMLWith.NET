using Newtonsoft.Json.Linq;
using System;

namespace ILOG.CP
{
    /**
     *
     */
    public class Segment
    {
        int start, end, value;

        public Segment(JObject segment)
        {
            if (segment.ContainsKey("start"))
            {
                String val = segment.Value<String>("start");
                if (val.ToString().Equals("intervalmin"))
                {
                    start = CP.IntervalMin;
                }
                else
                {
                    start = int.Parse(val);
                }
            }
            else
            {
                start = CP.IntervalMin;
            }

            if (segment.ContainsKey("end"))
            {
                String val = segment.Value<String>("end");
                if (val.ToString().Equals("intervalmax"))
                {
                    end = CP.IntervalMax;
                }
                else
                {
                    end = int.Parse(val);
                }
            }
            else
            {
                end = CP.IntervalMax;
            }

            if (segment.ContainsKey("value"))
            {
                String val = segment.Value< String>("value");
                value = int.Parse(val);
            }
            else
            {
                value = CP.NoState;
            }
        }
    }
}
