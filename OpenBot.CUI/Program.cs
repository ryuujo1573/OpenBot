using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Maila.Cocoa.Framework;
using Maila.Cocoa.Framework.Core;
using Newtonsoft.Json;
using OpenBot.CsOrg;
using OpenBot.CUI;

Directory.CreateDirectory("config");
StreamReader reader = new(File.OpenRead("config/mirai-http.json"));
var json = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
    reader.ReadToEnd());
if (json is null)
{
    Console.Error.WriteLine("Json error.");
    return;
}
BotStartupConfig config = new(json["AuthKey"],
    (long)json["Id"],
    host: json.TryGetValue("Host",
        out var host)
        ? host
        : "127.0.0.1",
    port: json.TryGetValue("Port",
        out var port)
        ? (int)port
        : 8080);

config.AddAssembly(typeof(BotBot).Assembly);
config.AddMiddleware<Blacklist>();

var succeed = await BotStartup.Connect(config);
if (succeed)
{
    Console.WriteLine("Startup OK"); // 提示连接成功
    Console.WriteLine($@"Loaded Modules: 
{ModuleCore.Modules
        .Select(mod => mod.Name)
        .Aggregate((a, b) => $"{a}, {b}")}");
    while (true)
    {
        var retCode = await Commands.Parser.InvokeAsync(Console.ReadLine() ?? string.Empty);
        if (retCode < 0)
            break;
    } // 在用户往控制台输入“exit”前持续运行

    await BotStartup.Disconnect(); // 断开连接
}
else
{
    Console.Error.WriteLine("Failed"); // 提示连接失败
}