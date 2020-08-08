using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SubtitlesParser.Classes;

namespace Youtube2SRT
{
    class Program
    {
        public static bool ShowHelp = false;
        public static string YoutubeURL = "";
        public static string YoutubePlaylist = "";
        public static string YoutubeVideoID = "";
        public static string SubtitleLanguage = "";

        /// <summary>
        /// https://www.youtube.com/watch?v=yLL6Tc02NHo&list=PLEXBGg5OB0B_VVQXo5IAKXGIxqsHIpBcq&index=2&t=0s
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string debugStr = "";
#if DEBUG
            debugStr = " [DEBUG]";
#endif
            string appName = Path.GetFileNameWithoutExtension(Path.GetFileName(ExeFilename));

            string consoleTitle = String.Format("{0} v{1:0}.{2:00}{3}", appName, version.Major, version.Minor, debugStr);

            Console.Title = consoleTitle;
            Console.WriteLine(consoleTitle);
            Console.WriteLine();

            
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (arg.Length >= 2 && arg == "-?")
                    {
                        ShowHelp = true;
                    }
                    else if(arg.Length == 5 && arg.Substring(0, 3).ToLower() == "-l:")
                    {
                        SubtitleLanguage = arg.Substring(3).ToLower();
                    }
                }
                else if (args.Length > 0)
                {
                    YoutubeURL = arg.Trim();
                    if (!YoutubeURL.ToLower().Contains("youtube."))
                    {
                        YoutubeURL = "";
                        continue;
                    }

                    Uri url = new Uri(YoutubeURL);
                    YoutubePlaylist = HttpUtility.ParseQueryString(url.Query).Get("list");
                    YoutubeVideoID = HttpUtility.ParseQueryString(url.Query).Get("v");
                }
            } //foreach

            ShowHelp = (YoutubeURL.Length == 0);
            if (ShowHelp)
            {
                Console.WriteLine("Youtube2SRT [options] <youtube-url>");
                Console.WriteLine();
                Console.WriteLine("-?        this help text");
                Console.WriteLine("-l:xx     subtitle language to extract (if available) eg -l:en");
                Console.WriteLine("-ld       extract default subtitle language (default when no options ar given)");
                Console.WriteLine();
                Console.WriteLine("When a url contains a playlist reference the subtitles from the entire playlist will be extracted.");
                Console.WriteLine("A playlist reference can be recognized by the parameter 'list='.");
                Console.WriteLine();
                Console.WriteLine("When a url only contains the 'v=' parameter of the video, only the subtitle from this video will be extracted.");

                Environment.Exit(0);
            }
            

            Youtube y = new Youtube();

            List<PlaylistItem> playlistItems = null;
            if (YoutubePlaylist.Length > 0)
            {
                if (!y.GetYoutubePlaylist(YoutubePlaylist, out playlistItems))
                {
                    Console.WriteLine("Playlist not found/youtube didn't give the right response.");
                    Environment.Exit(1);
                }
            }
            else
            {
                PlaylistItem item;
                if (y.GetYoutubeVideoID(YoutubeVideoID, out item))
                {
                    playlistItems = new List<PlaylistItem>();
                    playlistItems.Add(item);
                }
                else
                {
                    Console.WriteLine("Video not found/youtube didn't give the right response.");
                    Environment.Exit(1);
                }
            }
            if (playlistItems == null)
            {
                Console.WriteLine("No video to process.");
                Environment.Exit(1);
            }

            foreach (PlaylistItem item in playlistItems)
            {
                Console.WriteLine($"VideoID: {item.VideoId} Title: {item.Title}");
                List<YoutubeLanguage> languages;
                if (y.GetYoutubeTranslations(item.VideoId, out languages))
                {
                    YoutubeLanguage subLanguage = null;
                    YoutubeLanguage defaultSubLanguage = null;
                    Console.Write("Language(s) found: ");
                    int count = 0;
                    foreach (YoutubeLanguage language in languages)
                    {
                        if (count > 0)
                        {
                            Console.Write(", ");
                        }
                        Console.Write($"{language.ISO639_1}");
                        count++;

                        if (language.ISO639_1.ToLower() == SubtitleLanguage.ToLower())
                        {
                            subLanguage = language;
                        }
                        if (language.DefaultLanguage)
                        {
                            defaultSubLanguage = language;
                        }
                    } //foreach
                    if (count == 0)
                    {
                        Console.Write("No subs");
                    }
                    Console.WriteLine(".");

                    if (subLanguage == null)
                    {
                        subLanguage = defaultSubLanguage;
                    }

                    if (subLanguage != null)
                    {
                        Stream transscript;
                        if (y.GetYoutubeTimedText(subLanguage.ISO639_1, item.VideoId, out transscript))
                        {
                            TimedText tt = new TimedText();
                            List<SubtitleItem> subTitles;
                            if (tt.ConvertTimedText(transscript, out subTitles))
                            {
                                SubRip subRip = new SubRip();
                                if (subRip.SubToSubRipSRT(subTitles, Youtube.ReplaceInvalidChars(item.Title) + $".{subLanguage.ISO639_1}.srt"))
                                {
                                    Console.WriteLine("Subtitle written to disk)");
                                }
                            }
                        }
                    }
                }
                Console.WriteLine();
            } //foreach

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Press enter to close.");
            Console.ReadLine();
#endif
        }

        public static string ExeFilename
        {
            get
            {
                return Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }


    }
}
