using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Docker.DotNet.X509;
using Maila.Cocoa.Beans.Models;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework;
using Maila.Cocoa.Framework.Models;
using Maila.Cocoa.Framework.Models.Processing;
using Maila.Cocoa.Framework.Support;
using Timer = System.Timers.Timer;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace OpenBot.CsOrg
{
    [BotModule(nameof(CodeRunner))]
    public class CodeRunner : BotModuleBase
    {
        [RegexRoute("test docker")]
        public static void DockerTestSync(MessageSource src)
        {
            Console.WriteLine("hello docker sync test!");

            DockerTest(src);
            
            Task.Run(async () =>
            {
                var sb = new StringBuilder();
                var credential =
                    new CertificateCredentials(new X509Certificate2("key.pfx",
                        await File.ReadAllTextAsync("key-passwd.txt")));
                var client = new DockerClientConfiguration(new Uri("https://bj01.ryuujo.com:2376"), credential)
                    .CreateClient(); //todo: do this with configuration

                await client.Containers.StartContainerAsync("hello-world", new ContainerStartParameters(),
                    CancellationToken.None);
                await client.Containers.GetContainerLogsAsync("hello-world", new ContainerLogsParameters(),
                    CancellationToken.None, new Progress<string>(@string => sb.AppendLine(@string)));
                await src.SendAsync(sb.ToString());
                await BotAPI.SendFriendMessage(1014469764, new PlainMessage(sb.ToString()));
            });
            Console.WriteLine("goodbye docker sync test!");
        }
        
        // [ThreadSafe]
        [RegexRoute("atest docker")]
        public static async Task DockerTest(MessageSource src)
        {
            throw new NotImplementedException("真的");
            Console.WriteLine("hello docker!");
            var sb = new StringBuilder();
            var credential =
                new CertificateCredentials(new X509Certificate2("key.pfx",
                    await File.ReadAllTextAsync("key-passwd.txt")));
            var client = new DockerClientConfiguration(new Uri("https://bj01.ryuujo.com:2376"), credential)
                .CreateClient(); //todo: do this with configuration

            await client.Containers.StartContainerAsync("hello-world", new ContainerStartParameters(),
                CancellationToken.None);
            await client.Containers.GetContainerLogsAsync("hello-world", new ContainerLogsParameters(),
                CancellationToken.None, new Progress<string>(@string => sb.AppendLine(@string)));
            await src.SendAsync(sb.ToString());
        }


        [RegexRoute(@"^/?run (?<lang>\w+)\s(?<code>[\s\S]*)")]
        public static int RunCode(string lang, string code)
        {
            // TODO
            switch (lang.ToLower())
            {
                case "python":
                case "py":
                    // RunPython();
                    break;
                case "csharp":
                case "c#":
                case "c井":

                    break;
            }

            return 0;
        }

        // private static int RunPython()
        // {
        //     var proc = new Process()
        //     {
        //         StartInfo = new ProcessStartInfo
        //         {
        //             CreateNoWindow = true,
        //             FileName = "python"
        //         }
        //     };
        //     
        // }

        [RegexRoute(@"^(@(?<wd>[\S]+))?>\s?(?<line>[\s\S]*)")]
        [IdentityRequirements(UserIdentity.Admin)]
        public static IEnumerable RunBash(string line, string wd, MessageSource src, QMessage msg)
        {
            Console.WriteLine($"({src.MemberCard}) {wd ?? "~"}> {line}");
            var receiver = new MessageReceiver();
            yield return receiver;
            yield return TimeSpan.Zero;
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments =
                        $@"-wd {(wd == "#" ? Environment.CurrentDirectory : wd ?? "~")} -MTA -Command ""{line}""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                }
            };

            // await
            Task.Run(() =>
            {
                Task.Delay(1000);
                Console.WriteLine("Replied!");
                // src.SendReplyEx(msg, false, "Processing...");
                src.Send("Processing...");
            });

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // handle the stderr

            // errorReader.ReadToEndAsync()
            //     .ContinueWith(async s => await src.SendPrivateAsync("[stderr]\n" + await s));

            // handle the stdout
            // Read and output with every 8 lines, and output as time elapsed by 3 seconds with no output.
            StringBuilder outBuilder = new();
            // var count = 1;
            var timer = new Timer(3000) {AutoReset = true}; // dispose ?

            void Output()
            {
                if (outBuilder.Length == 0) return;
                var @out = outBuilder.ToString();
                outBuilder.Clear();
                Console.WriteLine("msg: " + @out);
                // Task.Delay(1000);
                src.Send($"[{DateTime.Now:T}] {@out}");
            }

            timer.Start();
            timer.Elapsed += (_, _) => { Output(); };

            proc.OutputDataReceived += async (sender, args) =>
            {
                if (args.Data is null) return;
                Console.WriteLine("[output] " + args.Data);
                try
                {
                    outBuilder.AppendLine(args.Data);
                    // if (++count > 8)
                    // {
                    //     count = 1;
                    //     Output();
                    //     timer.Interval = 3000; // reset the timer
                    // }

                    if (proc.HasExited)
                    {
                        Output();
                    }
                }
                catch (Exception e)
                {
                    await Console.Error.WriteLineAsync(e.Message);
                }
            };

            var writer = proc.StandardInput;
            while (!proc.HasExited)
            {
                yield return null;
                if (receiver.Message!.ToString().ToLower().StartsWith("^c"))
                {
                    proc.Kill();
                    yield break;
                }

                var match = Regex.Match(receiver.Message!, @">>([\s\S]*)");
                if (match.Success)
                    writer.WriteLine(match.Groups[1]);
                else
                {
                    yield return NotFit.Continue;
                }
            }
        }
    }
}