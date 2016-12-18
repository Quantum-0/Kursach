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
        static bool Aborting;
        static object Sync = new object();
        
        public static void AbortTasks()
        {
            Aborting = true;
        }
        public static Guid StartDownloadString(string Url)
        {
            lock (Sync)
            {
                Aborting = false;
                Guid guid = Guid.NewGuid();
                Urls[guid] = Url;
                Finished[guid] = false;
                lock (DownloadingList)
                {
                    DownloadingList.Add(guid);
                }
                if (!Processing)
                    Task.Run((Action)DownloadFile);

                Processing = true;
                return guid;
            }
        }
        public static ResultWithTime WaitForDownloadedWithTime(Guid guid)
        {
            if (!Processing)
            {
                Processing = true;
                Task.Run((Action)DownloadFile);
            }

            var TimeoutSW = new Stopwatch();
            TimeoutSW.Start();
            while (!Aborting && !Finished[guid] && TimeoutSW.ElapsedMilliseconds < 30*1000)
                Thread.Sleep(1);

            if (!Aborting && TimeoutSW.ElapsedMilliseconds < 30*1000)
                return GetResultAndRemoveFromLists(guid);
            else
                return new ResultWithTime("", 0);
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
            lock (DownloadingList)
            {
                if (DownloadingList.Any())
                {
                    CurrentTask = DownloadingList.First();
                    DownloadingList.Remove(CurrentTask);

                    WebClient webClient = new WebClient();
                    webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
                    webClient.DownloadStringAsync(new Uri(Urls[CurrentTask]));
                    SWs[CurrentTask] = new Stopwatch();
                    SWs[CurrentTask].Start();
                }
                else
                {
                Processing = false;
                }
            }
        }

        private static void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //if (e.Error != null)
               //;
            //SWs[CurrentTask].Stop();
            Results[CurrentTask] = e.Result;
            Finished[CurrentTask] = true;
            ((WebClient)sender).DownloadStringCompleted -= WebClient_DownloadStringCompleted;
            ((WebClient)sender).Dispose();
            Task.Run((Action)DownloadFile);
            //DownloadFile();
        }
    }
}
