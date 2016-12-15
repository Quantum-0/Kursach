using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUI
{
    static class Downloader
    {
        //static WebClient webClient;
        static Dictionary<Guid, string> Results = new Dictionary<Guid, string>();
        static Dictionary<Guid, string> Urls = new Dictionary<Guid, string>();
        static Dictionary<Guid, bool> Finished = new Dictionary<Guid, bool>();
        static List<Guid> DownloadingList = new List<Guid>();
        static bool Processing = false;
        static Guid CurrentTask;
        
        public static Guid StartDownloadString(string Url)
        {
            Guid guid = Guid.NewGuid();
            DownloadingList.Add(guid);
            Urls.Add(guid, Url);
            Finished.Add(guid, false);
            if (!Processing)
                Task.Run((Action)DownloadFile);
            //Task.Run((Action)StartProcessing);

            Processing = true;
            return guid;
        }
        public static string WaitForDownloaded(Guid guid)
        {
            if (!Processing)
                Task.Run((Action)DownloadFile);
            //                Task.Run((Action)StartProcessing);
            Processing = true;
            while (!Finished[guid])
                Thread.Sleep(10);

            var result = Results[guid];
            Results.Remove(guid);
            Urls.Remove(guid);
            Finished.Remove(guid);
            return result;
        }

        /*private static void StartProcessing()
        {
            Processing = true;


            var Tasks = DownloadingList.ToArray();
            DownloadingList.Clear();
            foreach (var task in Tasks)
            {
                webClient = new WebClient();
                webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
                CurrentTask = task;
                webClient.DownloadStringAsync(new Uri(Urls[task]));
            }

            Processing = false;
        }*/

        private static void DownloadFile()
        {
            if (DownloadingList.Any())
            {
                CurrentTask = DownloadingList.First();
                DownloadingList.Remove(CurrentTask);

                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
                webClient.DownloadStringAsync(new Uri(Urls[CurrentTask]));
            }
            else
            {
                Processing = false;
            }
        }

        private static void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Results[CurrentTask] = e.Result;
            Finished[CurrentTask] = true;
            ((WebClient)sender).DownloadStringCompleted -= WebClient_DownloadStringCompleted;
            ((WebClient)sender).Dispose();
            DownloadFile();
        }
    }
}
