using System;
using System.Threading;

using Nancy;
using Nancy.Hosting.Self;

namespace TrulyQuantumChess.WebApp {
    public static class Program {
        public static void Main(string[] args) {
            var cleaner = new Thread(() => {
                try {
                    for (;;) {
                        try {
                            Games.Clean();
                        } catch (ThreadAbortException) {
                            throw;
                        } catch (Exception e) {
                            Console.WriteLine($"Error occured while cleaning: {e}");
                        }
                        Thread.Sleep(TimeSpan.FromMinutes(5));
                    }
                } catch (ThreadAbortException) {}
            });

            StaticConfiguration.DisableErrorTraces = WebAppConfig.Instance.Mode != "debug";
            var uri = new Uri(WebAppConfig.Instance.ListenUrl);
            using (var host = new NancyHost(uri)) {
                host.Start();
                Console.WriteLine($"Listening on {uri}..");

                cleaner.Start();

                long host_alive = 1;

                Console.CancelKeyPress += (sender, e) => {
                    Interlocked.Exchange(ref host_alive, 0L);
                };

                for (;;) {
                    if (Interlocked.Read(ref host_alive) == 0)
                        goto abort;

                    while (Console.KeyAvailable) {
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                            goto abort;
                    }

                    Thread.Sleep(100);
                }

                abort:
                host.Stop();
                cleaner.Abort();
                cleaner.Join();
                Console.WriteLine("Bye.");
            }
        }
    }
}
