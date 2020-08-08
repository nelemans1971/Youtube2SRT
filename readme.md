## Youtube2SRT

Simple .NET console application which downloads subtitles from youtube using
a youtube VideoID or a youtube playlist url.


### What does it do

The program downloads a subtitle from youtube. Converts the timedtext xml to Subrip srt
format and writes this to disk. The filename is this title of the video.
Is also does it for a playlist, conveniently creating a list of subtitles.

Example usage:
Youtube2SRT -l:en "https://www.youtube.com/watch?v=yLL6Tc02NHo&list=PLEXBGg5OB0B_VVQXo5IAKXGIxqsHIpBcq&index=2&t=0s"

Watch the quotes around the url they are needed most of the time!

### Warning

This program is not well tested so use it at your own risk.
It works for me.

### Reason for writing this program

I use 4k Video Download to occasinaly download chinese dramas. Up to a couple 
of weeks (time of writing this is 8th of august 2020) it also downloaded the corresponding 
subtitles. Something changed on youtube and it no longer works.
the people at 4k Video Download are aware of this, but up to now there is still no fix from them.
Another program Google2SRT which I sometimes use to download subtitles still works, so
I looked at it and googled for some info how youtube worked. It seemd simple so I wrote
this program to automated the download of subtitles from playlists to saved me some time :-)


### Used packages

[Newtonsoft.Json](https://www.newtonsoft.com/json)<br/>
[RestSharp](https://restsharp.dev/)<br/>
[SubtitlesParser](https://github.com/AlexPoint/SubtitlesParser)<br/>

### License

The code is copyrighted 2019 by Yvo Nelemans, and licensed under the 
[MIT license](https://opensource.org/licenses/MIT).
Specific parts of the code are written by others and are copyrighted by their
respective holders.

### Questions?
You can contact me (Yvo Nelemans) at yvo @ nelemans.net
