using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitlesParser.Classes;

namespace Youtube2SRT
{
    public class SubRip
    {
        public bool SubToSubRipSRT(List<SubtitleItem> subTitles, string subRipFilename)
        {
            MemoryStream ms = new MemoryStream();
            using (StreamWriter srtFile = new StreamWriter(ms, Encoding.UTF8, 1024))
            {
                int srtCount = 1;
                foreach (SubtitleItem item in subTitles)
                {
                    if (item.Lines.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(TimeSpanToSRTTimeSTring(TimeSpan.FromMilliseconds(item.StartTime)));
                        sb.Append(" --> ");
                        sb.Append(TimeSpanToSRTTimeSTring(TimeSpan.FromMilliseconds(item.EndTime)));
                        sb.Append("\r\n");

                        foreach (string line in item.Lines)
                        {
                            sb.Append(line.Trim());
                            sb.Append("\r\n");
                        }

                        if (srtCount > 1)
                        {
                            srtFile.WriteLine();
                        }
                        srtFile.WriteLine(srtCount);
                        srtFile.Write(sb.ToString());

                        srtCount++;
                    }
                } //foreach
                srtFile.Flush();

                using (StreamWriter writer = new StreamWriter(subRipFilename, false, Encoding.UTF8, 1024))
                {
                    ms.Position = 0;
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            writer.WriteLine(line);
                        } //while
                    } //using
                } //using
            } //using
            GC.WaitForPendingFinalizers(); // make sure file handlersd are closed

            return true;
        }

        private static string TimeSpanToSRTTimeSTring(TimeSpan ts)
        {
            return string.Format("{0:00}:{1:00}:{2:00},{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

    }
}
