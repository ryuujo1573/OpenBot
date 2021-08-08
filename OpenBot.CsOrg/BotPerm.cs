using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework;
using Maila.Cocoa.Framework.Core;
using Maila.Cocoa.Framework.Support;

// ReSharper disable UnusedType.Global

namespace OpenBot.CsOrg
{
    [BotModule(nameof(BotPerm))]
    public class BotPerm : BotModuleBase
    {
        // add/remove
        [IdentityRequirements(UserIdentity.Admin)]
        [RegexRoute(
            @"^/perm\s?(?<subCommand>((?:re)?set)|(add)|(remove)|(rm))(\s+(?<identity>[a-zA-Z]\w+))?(\s+(?<accounts>\d{6,}))*")]
        public static string SetPerm(string identity, string subCommand, string[] accounts, QMessage msg)
        {
            // var modules = ModuleCore.Modules
            //     .Where(m => m.Name?.Contains(module) ?? false)
            //     .ToArray();
            //
            // if (modules.Length is 1 && modules[0].Enabled)
            // {
            // }

            var identityName = typeof(UserIdentity).GetEnumNames()
                .FirstOrDefault(m => m.ToLower().Contains(identity.ToLower()));
            if (identityName is null)
                return "[Error] Identity not found.";

            var arrayId =
                msg.GetSubMessages<AtMessage>()
                    .Select(at => at.Target)
                    //.Concat(accounts.Select(long.Parse))
                    .ToArray();

            Console.WriteLine(arrayId.Select(a => a.ToString()).Aggregate((a, b) => string.Concat(a, ' ', b)));
            switch (subCommand.ToLower())
            {
                case "set":
                    foreach (var id in arrayId)
                    {
                        BotAuth.SetIdentity(id, Enum.Parse<UserIdentity>(identityName));
                    }

                    break;
                case "add":
                    foreach (var id in arrayId)
                    {
                        BotAuth.AddIdentity(id, Enum.Parse<UserIdentity>(identityName));
                    }

                    break;

                case "remove":
                case "rm":
                    foreach (var id in arrayId)
                    {
                        BotAuth.RemoveIdentity(id, Enum.Parse<UserIdentity>(identityName));
                    }

                    break;
                case "reset":
                    foreach (var id in arrayId)
                    {
                        BotAuth.ClearIdentity(id);
                    }

                    break;
            }

            return "Done! ";
        }

        [RegexRoute("^/mod(?:ule)? (list|ls)")]
        public static string ListModules()
            => "Loaded Modules: \n" +
               $@"{ModuleCore.Modules
                   .Select(mod => mod.Name)
                   .Aggregate((a, b) => $"{a}, {b}")}";

        [RegexRoute(@"^/ban(\s+(?<idAll>\d{6,10}))*")]
        public static string Ban(long[] idAll, QMessage msg)
        {
            var before = Blacklist.Forever.Count;
            Blacklist.Forever.UnionWith(
                msg.GetSubMessages<AtMessage>()
                    .Select(at => at.Target)
                    .Concat(idAll));
            var diff = Blacklist.Forever.Count - before;
            
            return $"Done! ({diff} record{(diff > 1 ? "s" : string.Empty)} added.)";
        }
    }
}