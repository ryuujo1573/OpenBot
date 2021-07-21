using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Reflection.Metadata;

namespace OpenBot.CUI
{
    public static class Commands
    {
        static Commands()
        {
            var exit = new Command("exit");
            exit.Handler = CommandHandler.Create(() => -1);
                
            var rootCommand = new RootCommand
            {
                exit
            };
            
            Parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();
        }
        public static Parser Parser { get; }
    }
    
}