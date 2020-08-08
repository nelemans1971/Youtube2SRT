using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml;

namespace Youtube2SRT
{
    public class PlaylistItem
    {
        public string VideoId = "";
        public string Title = "";
    }

    public class YoutubeLanguage
    {
        public string Language = "";
        public string ISO639_1 = ""; // 2 lettercode
        public bool DefaultLanguage = false;
    }

    public class Youtube
    {
        private const string youtubeUrl = "https://www.youtube.com/";
        private static int RandomMinTime = 100;
        private static int RandomMaxTime = 500;


        private DateTime dtLastMatch = DateTime.MinValue;
        private Random random = new Random();

        /*
         // https://www.youtube.com/playlist?list=PLEXBGg5OB0B_VVQXo5IAKXGIxqsHIpBcq
            Youtube y = new Youtube();
            List<PlaylistItem> playlistItems;
            y.GetYoutubePlaylist("PLEXBGg5OB0B_VVQXo5IAKXGIxqsHIpBcq", out playlistItems);
            foreach (PlaylistItem item in playlistItems)
            {
                Console.WriteLine($"VideoID: {item.VideoId} Title: {item.Title}");
            } //foreach
            return;
        */
        public bool GetYoutubePlaylist(string playlist, out List<PlaylistItem> playlistItems)
        {
            playlistItems = null;

            if ((DateTime.Now - dtLastMatch).TotalSeconds < 5)
            {
                int milliSeconds = random.Next(RandomMinTime, RandomMaxTime);
                Thread.Sleep(milliSeconds);
            }

            StringBuilder sb = new StringBuilder();
            bool canceled = false;
            int retry = 2;
            do
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Were we already canceled?
                        cancelTokenSource.Token.ThrowIfCancellationRequested();

                        RestClient client = new RestClient(youtubeUrl);
                        client.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; Trident/7.0; rv:11.0) like Gecko";
                        client.Timeout = 5000; // 5 seconden

                        // Build search request
                        RestRequest request = new RestRequest("/playlist", Method.GET);
                        request.AddHeader("Accept-Encoding", "gzip,deflate");
                        request.AddParameter("list", playlist);

                        // Run and wait for result
                        IRestResponse response = client.Execute(request);
                        cancelTokenSource.Token.ThrowIfCancellationRequested();
                        if (response.ResponseStatus != ResponseStatus.Completed)
                        {
                            return;
                        }
                        sb.Append(response.Content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        waitEvent.Set();
                    }
                }, cancelTokenSource.Token);

                if (!waitEvent.WaitOne(5000))
                {
                    /// Timeout!
                    ///
                    cancelTokenSource.Cancel();
                    canceled = true;
                }
                retry--;
            } while (canceled && retry > 0);
            if (canceled)
            {
                return false;
            }

            dtLastMatch = DateTime.Now;

            // Process data
            if (sb.Length <= 10)
            {
                return false;
            }
            try
            {
                string text = sb.ToString();
                int p1 = text.IndexOf("window[\"ytInitialData\"]");
                int p2 = text.IndexOf("window[\"ytInitialPlayerResponse\"]");
                if (p1 > 0 && p2 > 0)
                {
                    text = text.Substring(p1);
                    text = text.Substring(0, p2 - p1);
                    while (text.Contains("\r\n"))
                    {
                        text = text.Replace("\r\n", "").Trim();
                    }
                    text = text.Replace("window[\"ytInitialData\"] =", "").Trim();
                    text = text.Substring(0, text.Length - 1);

                    /*
                     * Dump json data to file inspection (debug code)
                    using (StreamWriter srtFile = new StreamWriter("youtubeList.txt", false, Encoding.UTF8))
                    {
                        srtFile.WriteLine(text);
                    }
                    */

                    Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(text);
                    if (jObject["contents"] != null)
                    {
                        var videos = jObject["contents"]["twoColumnBrowseResultsRenderer"]["tabs"][0]["tabRenderer"]["content"]["sectionListRenderer"]["contents"][0]["itemSectionRenderer"]["contents"][0]["playlistVideoListRenderer"]["contents"];

                        playlistItems = new List<PlaylistItem>();
                        foreach (var video in videos)
                        {
                            // Zou ook een "playlistRenderer" kunnen zijn.
                            if (video["playlistVideoRenderer"] != null)
                            {
                                PlaylistItem item = new PlaylistItem();
                                item.VideoId = (string)video["playlistVideoRenderer"]["videoId"];
                                item.Title = (string)video["playlistVideoRenderer"]["title"]["simpleText"];
                                playlistItems.Add(item);
                            }
                        } //foreach

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                playlistItems = null;
                Console.WriteLine(e.ToString());
            }

            return false;
        }

        /// <summary>
        /// https://www.youtube.com/watch?v=yLL6Tc02NHo
        /// </summary>
        public bool GetYoutubeVideoID(string videoID, out PlaylistItem item)
        {
            item = null;

            if ((DateTime.Now - dtLastMatch).TotalSeconds < 5)
            {
                int milliSeconds = random.Next(RandomMinTime, RandomMaxTime);
                Thread.Sleep(milliSeconds);
            }

            StringBuilder sb = new StringBuilder();
            bool canceled = false;
            int retry = 2;
            do
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Were we already canceled?
                        cancelTokenSource.Token.ThrowIfCancellationRequested();

                        RestClient client = new RestClient(youtubeUrl);
                        client.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; Trident/7.0; rv:11.0) like Gecko";
                        client.Timeout = 5000; // 5 seconden

                        // Build search request
                        RestRequest request = new RestRequest("/watch", Method.GET);
                        request.AddHeader("Accept-Encoding", "gzip,deflate");
                        request.AddParameter("v", videoID);

                        // Run and wait for result
                        IRestResponse response = client.Execute(request);
                        cancelTokenSource.Token.ThrowIfCancellationRequested();
                        if (response.ResponseStatus != ResponseStatus.Completed)
                        {
                            return;
                        }
                        sb.Append(response.Content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        waitEvent.Set();
                    }
                }, cancelTokenSource.Token);

                if (!waitEvent.WaitOne(5000))
                {
                    /// Timeout!
                    ///
                    cancelTokenSource.Cancel();
                    canceled = true;
                }
                retry--;
            } while (canceled && retry > 0);
            if (canceled)
            {
                return false;
            }

            dtLastMatch = DateTime.Now;

            // Process data
            if (sb.Length <= 10)
            {
                return false;
            }
            try
            {
                string text = sb.ToString();
                int p1 = text.IndexOf("window[\"ytInitialData\"]");
                int p2 = text.IndexOf("window[\"ytInitialPlayerResponse\"]");
                if (p1 > 0 && p2 > 0)
                {
                    text = text.Substring(p1);
                    text = text.Substring(0, p2 - p1);
                    while (text.Contains("\r\n"))
                    {
                        text = text.Replace("\r\n", "").Trim();
                    }
                    text = text.Replace("window[\"ytInitialData\"] =", "").Trim();
                    text = text.Substring(0, text.Length - 1);

                    /*
                     * Dump json data to file inspection (debug code)
                    using (StreamWriter videoIDFile = new StreamWriter("youtubeList.json", false, Encoding.UTF8))
                    {
                        videoIDFile.WriteLine(text);
                    }
                    */

                    Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(text);
                    if (jObject["contents"] != null)
                    {
                        item = new PlaylistItem();
                        item.VideoId = videoID;
                        item.Title = jObject["contents"]["twoColumnWatchNextResults"]["results"]["results"]["contents"][0]["videoPrimaryInfoRenderer"]["title"]["runs"][0]["text"].ToString();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                item = null;
                Console.WriteLine(e.ToString());
            }

            return false;
        }



        /*
            Youtube y = new Youtube();
            List<YoutubeLanguage> langs;
            y.GetYoutubeTranslations("DrrLsuS1IP4", out langs);
            Console.WriteLine("Language: ");
            int tmpCount = 0;
            foreach (YoutubeLanguage language in langs)
            {
                if (tmpCount > 0)
                {
                    Console.Write(", ");
                }
                Console.Write($"{language.ISO639_1}");
                tmpCount++;
            } //foreach
            Console.WriteLine(".");          
        */
        /// <summary>
        /// http://video.google.com/timedtext?type=list&v=DrrLsuS1IP4
        /// https://www.youtube.com/api/timedtext?type=list&v=DrrLsuS1IP4
        /// </summary>
        public bool GetYoutubeTranslations(string videoID, out List<YoutubeLanguage> languages)
        {
            languages = null;

            if ((DateTime.Now - dtLastMatch).TotalSeconds < 5)
            {
                int milliSeconds = random.Next(RandomMinTime, RandomMaxTime);
                Thread.Sleep(milliSeconds);
            }

            StringBuilder sb = new StringBuilder();
            bool canceled = false;
            int retry = 2;
            do
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Were we already canceled?
                        cancelTokenSource.Token.ThrowIfCancellationRequested();

                        RestClient client = new RestClient(youtubeUrl);
                        client.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; Trident/7.0; rv:11.0) like Gecko";
                        client.Timeout = 5000; // 5 seconden

                        // Build search request
                        RestRequest request = new RestRequest("/api/timedtext", Method.GET);
                        request.AddHeader("Accept-Encoding", "gzip,deflate");
                        request.AddParameter("type", "list");
                        request.AddParameter("v", videoID);

                        // Run and wait for result
                        IRestResponse response = client.Execute(request);
                        cancelTokenSource.Token.ThrowIfCancellationRequested();
                        if (response.ResponseStatus != ResponseStatus.Completed)
                        {
                            return;
                        }
                        sb.Append(response.Content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        waitEvent.Set();
                    }
                }, cancelTokenSource.Token);

                if (!waitEvent.WaitOne(5000))
                {
                    /// Timeout!
                    ///
                    cancelTokenSource.Cancel();
                    canceled = true;
                }
                retry--;
            } while (canceled && retry > 0);
            if (canceled)
            {
                return false;
            }

            dtLastMatch = DateTime.Now;

            // Process data
            if (sb.Length <= 10)
            {
                return false;
            }
            try
            {
                string text = sb.ToString();// 1, sb.Length - 2);
                text = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><transcript_list docid=\"1061384631400866046\"><track id=\"0\" name=\"\" lang_code=\"en\" lang_original=\"English\" lang_translated=\"English\" lang_default=\"true\"/></transcript_list>";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(text);
                XmlNodeList xTranscriptList = xmlDoc.GetElementsByTagName("transcript_list");
                if (xTranscriptList == null)
                {
                    return false;
                }

                languages = new List<YoutubeLanguage>();
                foreach (XmlNode xn in xTranscriptList[0].ChildNodes)
                {
                    YoutubeLanguage item = new YoutubeLanguage();
                    item.Language = xn.Attributes["lang_original"].Value.ToString();
                    item.ISO639_1 = xn.Attributes["lang_code"].Value.ToString();
                    item.DefaultLanguage = (xn.Attributes["lang_default"].Value.ToString().ToLower() == "true");

                    languages.Add(item);
                } //foreach

                return true;
            }
            catch (Exception e)
            {
                languages = null;
                Console.WriteLine(e.ToString());
            }

            return false;
        }

        /// <summary>
        /// http://video.google.com/timedtext?lang=en&v=DrrLsuS1IP4
        /// https://www.youtube.com/api/timedtext?lang=en&v=DrrLsuS1IP4
        /// </summary>
        public bool GetYoutubeTimedText(string languageCode, string videoID, out Stream transscript)
        {
            transscript = null;

            if ((DateTime.Now - dtLastMatch).TotalSeconds < 5)
            {
                int milliSeconds = random.Next(RandomMinTime, RandomMaxTime);
                Thread.Sleep(milliSeconds);
            }

            StringBuilder sb = new StringBuilder();
            bool canceled = false;
            int retry = 2;
            do
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                EventWaitHandle waitEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Were we already canceled?
                        cancelTokenSource.Token.ThrowIfCancellationRequested();

                        RestClient client = new RestClient(youtubeUrl);
                        client.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; Trident/7.0; rv:11.0) like Gecko";
                        client.Timeout = 5000; // 5 seconden

                        // Build search request
                        RestRequest request = new RestRequest("/api/timedtext", Method.GET);
                        request.AddHeader("Accept-Encoding", "gzip,deflate");
                        request.AddParameter("lang", languageCode);
                        request.AddParameter("v", videoID);

                        // Run and wait for result
                        IRestResponse response = client.Execute(request);
                        cancelTokenSource.Token.ThrowIfCancellationRequested();
                        if (response.ResponseStatus != ResponseStatus.Completed)
                        {
                            return;
                        }
                        sb.Append(response.Content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    finally
                    {
                        waitEvent.Set();
                    }
                }, cancelTokenSource.Token);

                if (!waitEvent.WaitOne(5000))
                {
                    /// Timeout!
                    ///
                    cancelTokenSource.Cancel();
                    canceled = true;
                }
                retry--;
            } while (canceled && retry > 0);
            if (canceled)
            {
                return false;
            }

            dtLastMatch = DateTime.Now;

            // Process data
            if (sb.Length <= 10)
            {
                return false;
            }
            try
            {
                transscript = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(sb.ToString()));
                transscript.Position = 0;

                return true;
            }
            catch (Exception e)
            {
                transscript = null;
                Console.WriteLine(e.ToString());
            }

            return false;
        }


        private string YouTubeJSONRunsText(JToken jRuns)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var jText in jRuns)
            {
                sb.Append((string)jText["text"]);
            } //foreach

            return sb.ToString();
        }

        public static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

    }
}
