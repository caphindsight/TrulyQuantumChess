using System;
using System.Threading;

using Nancy;
using Nancy.Hosting.Self;

namespace TrulyQuantumChess.WebApp {
    public static class Program {
        public static void Main(string[] args) {
            var uri = new Uri("http://127.0.0.1:9000");
            using (var host = new NancyHost(uri)) {
                host.Start();
                Console.WriteLine($"Listening on {uri}..");

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
                Console.WriteLine("Bye.");
            }
        }
    }
}
