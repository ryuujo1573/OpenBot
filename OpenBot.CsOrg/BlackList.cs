namespace OpenBot.CsOrg
{
    using System;
    using System.Collections.Generic;
    using Maila.Cocoa.Framework;

    public class Blacklist : BotMiddlewareBase
    {
        [Hosting] public static ISet<long> Forever = new HashSet<long>();
        
        protected override void OnMessage(MessageSource src, QMessage msg, Action<MessageSource, QMessage> next)
        {
            if (!Forever.Contains(src.User.Id))
            {
                next(src, msg);
            }
        }
    }
}