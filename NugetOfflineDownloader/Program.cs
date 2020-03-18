﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;

namespace NugetOfflineDownloader
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Task.Run(() => Run(args[0])).Wait();

            return 0;
        }

        public static async Task Run(string manifestPath)
        {
            ILogger logger = new ConsoleLogger();
            Application nuget = new Application(logger, "packages");

            await nuget.Initialize(new CancellationTokenSource(4000).Token);

            foreach (var item in Parse(manifestPath))
            {
                await nuget.DownloadPackageAsync(item.Item1, item.Item2, new CancellationTokenSource(10 * 60 * 1000).Token);
            }
        }

        private static IEnumerable<Tuple<string, string>> Parse(string manifestPath)
        {
            using (FileStream fs = new FileStream(manifestPath, FileMode.Open))
            {
                using (TextReader reader = new StreamReader(fs))
                {
                    string id = null;
                    string version = null;

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            yield break;
                        }
                        else if (line.StartsWith("Id"))
                        {
                            id = line.Split(':').Last().Trim();
                        }
                        else if (line.StartsWith("Version"))
                        {
                            version = line.Split(':').Last().Trim();

                            yield return new Tuple<string, string>(id, version);
                        }
                    }
                }
            }
        }
    }
}
