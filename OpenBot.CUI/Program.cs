using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using Maila.Cocoa.Framework;
using Newtonsoft.Json;
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

var succeed = await BotStartup.Connect(config);
if (succeed) // 如果连接成功
{
    Console.WriteLine("Startup OK"); // 提示连接成功
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