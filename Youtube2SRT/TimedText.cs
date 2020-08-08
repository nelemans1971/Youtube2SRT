using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SubtitlesParser.Classes;

namespace Youtube2SRT
{
    public class TimedText
    {
        public bool ConvertTimedText(Stream streamTimedText, out List<SubtitleItem> subTitles)
        {
            subTitles = null;
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            try
            {
                streamTimedText.Seek(0, SeekOrigin.Begin);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(streamTimedText);
                XmlNodeList xTranscript = xmlDoc.GetElementsByTagName("transcript");
                if (xTranscript == null)
                {
                    return false;
                }

                subTitles = new List<SubtitleItem>();
                foreach (XmlNode xn in xTranscript[0].ChildNodes)
                {
                    double start = 0.0;
                    double dur = 0.0;
                    double.TryParse(xn.Attributes["start"].Value, System.Globalization.NumberStyles.Float, culture, out start);
                    double.TryParse(xn.Attributes["dur"].Value, System.Globalization.NumberStyles.Float, culture, out dur);
                    List<string> lines = new List<string>();
                    foreach (string line in xn.InnerText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        lines.Add(WebUtility.HtmlDecode(line));
                    }//foreach

                    SubtitleItem item = new SubtitleItem();
                    item.Lines.AddRange(lines);
                    item.StartTime = Convert.ToInt64(start * 1000.0);
                    item.EndTime = Convert.ToInt64((start + dur) * 1000.0);
                    subTitles.Add(item);
                } //foreach

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return false;
        }


    }
}