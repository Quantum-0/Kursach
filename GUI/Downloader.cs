using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUI
{
    static class Downloader
    {
        public class ResultWithTime
        {
            public string Result { get; private set; }
            public long Time { get; private set; }

            public ResultWithTime(string Result, long ElapsedMilliseconds)
            {
                this.Time = ElapsedMilliseconds;
                this.Result = Result;
            }
        }
        static Dictionary<Guid, string> Results = new Dictionary<Guid, string>();
        static Dictionary<Guid, string> Urls = new Dictionary<Guid, string>();
        static Dictionary<Guid, bool> Finished = new Dictionary<Guid, bool>();
        static Dictionary<Guid, Stopwatch> SWs = new Dictionary<Guid, Stopwatch>();
        static List<Guid> DownloadingList = new List<Guid>();
        static bool Processing = false;
        static Guid CurrentTask;
        
        public static Guid StartDownloadString(string Url)
        {
            Guid guid = Guid.NewGuid();
            DownloadingList.Add(guid);
            Urls.Add(guid, Url);
            Finished.Add(guid, false);
            SWs[guid] = new Stopwatch();
            if (!Processing)
                Task.Run((Action)DownloadFile);
            //Task.Run((Action)StartProcessing);

            Processing = true;
            return guid;
        }
        public static ResultWithTime WaitForDownloadedWithTime(Guid guid)
        {
            if (!Processing)
            {
                Processing = true;
                Task.Run((Action)DownloadFile);
            }

            while (!Finished[guid])
                Thread.Sleep(10);

            return GetResultAndRemoveFromLists(guid);
        }
        public static string WaitForDownloaded(Guid guid)
        {
            return WaitForDownloadedWithTime(guid).Result;
        }
        private static ResultWithTime GetResultAndRemoveFromLists(Guid guid)
        {
            var result = new ResultWithTime(Results[guid], SWs[guid].ElapsedMilliseconds);
            Results.Remove(guid);
            Urls.Remove(guid);
            Finished.Remove(guid);
            SWs.Remove(guid);
            return result;
        }
        
        private static void DownloadFile()
        {
            if (DownloadingList.Any())
            {
                CurrentTask = DownloadingList.First();
                DownloadingList.Remove(CurrentTask);

                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
                webClient.DownloadStringAsync(new Uri(Urls[CurrentTask]));
                SWs[CurrentTask].Start();
            }
            else
            {
                Processing = false;
            }
        }

        private static void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SWs[CurrentTask].Stop();
            Results[CurrentTask] = e.Result;
            Finished[CurrentTask] = true;
            ((WebClient)sender).DownloadStringCompleted -= WebClient_DownloadStringCompleted;
            ((WebClient)sender).Dispose();
            DownloadFile();
        }
    }
}
