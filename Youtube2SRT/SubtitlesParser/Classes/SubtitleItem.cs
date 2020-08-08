using System;
using System.Collections.Generic;

namespace SubtitlesParser.Classes
{
    public class SubtitleItem
    {

        //Properties------------------------------------------------------------------
        
        //StartTime and EndTime times are in milliseconds
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public List<string> Lines { get; set; }
        

        //Constructors-----------------------------------------------------------------

        /// <summary>
        /// The empty constructor
        /// </summary>
        public SubtitleItem()
        {
            this.Lines = new List<string>();
        }


        // Methods --------------------------------------------------------------------------

        public override string ToString()
        {
            int startTimeSec = Convert.ToInt32(Math.Floor(Convert.ToDouble(StartTime) / 1000.0));
            int startTimeMilli = Convert.ToInt32(StartTime - (Convert.ToInt64(startTimeSec) * 1000));
            int endTimeSec = Convert.ToInt32(Math.Floor(Convert.ToDouble(EndTime) / 1000.0));
            int endTimeMilli = Convert.ToInt32(StartTime - (Convert.ToInt64(endTimeSec) * 1000));

            TimeSpan startTs = new TimeSpan(0, 0, 0, startTimeSec, startTimeMilli);
            TimeSpan endTs = new TimeSpan(0, 0, 0, endTimeSec, endTimeMilli);

            string res = string.Format("{0} --> {1}: {2}", startTs.ToString("G"), endTs.ToString("G"), string.Join(Environment.NewLine, Lines));
            return res;
        }

    }
}