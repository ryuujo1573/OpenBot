using System;
using System.Threading.Tasks;
using Maila.Cocoa.Framework;
// ReSharper disable UnusedMember.Global

namespace OpenBot.CsOrg
{
    [BotModule("Hello ryuubot")]
    public class BotBot : BotModuleBase
    {
        public new string Name => "Ah♂";

        
        [TextRoute("hello ryuubot", IgnoreCase = false)]
        public static void Hello(MessageSource src)
        {
            Console.WriteLine($"hello hello {src.MemberCard} can you hear me");
            Task.Delay(1000);
            src.Send($"Hi! {DateTime.Now:g}");
        }
    }
}