using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DumpSoundcloud
{
    class Program
    {
        public static List<string> contentHolder = new List<string>();
        static void Main(string[] args)
        {

            //HttpListener listener;
            //listener = new HttpListener();
            //listener.Prefixes.Add("http://localhost:9999/");

            //listener.Start();
            //while (true)
            //{
            //    var context = listener.GetContext();
            //    var request = context.Request;
            //    string text;
            //    if (context == null)
            //    {
            //        Thread.Sleep(2000);

            //    }
            //    else
            //    {
            //        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            //        {
            //            text = reader.ReadToEnd();
            //        }
            //            Console.WriteLine(text);

            //    }

            //}
            var token = CancellationToken.None;
            Listen("http://localhost:9999/", 28, token);
            Console.ReadKey();
        }
        public static async void Listen(string prefix, int maxConcurrentRequests, CancellationToken token)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            var requests = new HashSet<Task>();
            for (int i = 0; i < maxConcurrentRequests; i++)
                requests.Add(listener.GetContextAsync());

            while (!token.IsCancellationRequested)
            {
                Task t = await Task.WhenAny(requests);
                requests.Remove(t);

                if (t is Task<HttpListenerContext>)
                {
                    var context = (t as Task<HttpListenerContext>).Result;
                    requests.Add(ProcessRequestAsync(context));
                    requests.Add(listener.GetContextAsync());
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }
            }
        }

        public static async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string path = "C:\\NowPlayingString";
            string text = String.Empty;
            var request = context.Request;

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {

                text = reader.ReadToEnd();
                var cleanText =await FormattedText(text);
                contentHolder.Add(cleanText);
                if (contentHolder.Count > 1)
                {

                    contentHolder.RemoveAt(0);
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine("Writing to directory...");
                    }
                    else
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                        Console.WriteLine("Directory Created at: " + path);
                    }
                    using (var writer = new StreamWriter(path + "\\nowPlaying.txt"))
                    {
                        foreach (var i in contentHolder)
                        {
                            writer.WriteLine(i);
                        }
                    }

                }
            }
            Console.WriteLine(text);
            await Task.Delay(100);
        }
        public static async Task<string> FormattedText(string textToFomat)
        {
            string formattedText = "";
            var firstRound = textToFomat.Split('&');
            var cleanPlayer = firstRound[0].Replace('=',':');
            var cleanSong = firstRound[1].Replace('+', ' ').Replace('=',':');
            formattedText = cleanPlayer + " " + cleanSong;
            return formattedText;
        }
    }
   
}
