/*
    Introduction to the C# SDK for Model Context Protocol (MCP)
    https://www.youtube.com/watch?v=krB1aA9xpts
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder .Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes a message back to the client.")]
    public static string Echo(string message) => $"Hello from C#, {message}!";

    [McpServerTool, Description("Echoes in reverse the message sent")]
    public static string ReverseEcho(string message) => message == null ? string.Empty : new string(message.Reverse().ToArray());
}
