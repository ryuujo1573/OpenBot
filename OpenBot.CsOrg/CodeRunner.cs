using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Maila.Cocoa.Beans.Models;
using Maila.Cocoa.Framework;
using Maila.Cocoa.Framework.Models;
using Maila.Cocoa.Framework.Models.Processing;
// ReSharper disable UnusedType.Global

namespace OpenBot.CsOrg
{
    [BotModule(nameof(CodeRunner))]
    public class CodeRunner : BotModuleBase
    {
        
        [RegexRoute(@"^/?run (?<lang>\w+)\s(?<code>[\s\S]*)")]
        public static int RunCode(string lang, string code)
        {
            // TODO
            switch (lang.ToLower())
            {
                case "python":
                case "py":

                    break;
                case "csharp":
                case "c#":
                case "c井":

                    break;
            }

            return 0;
        }

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