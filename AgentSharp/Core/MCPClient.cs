//using ModelContextProtocol.Client;
//using System.Collections.Generic;
//using System.Threading;
//using System;
//using System.Threading.Tasks;
//using System.Linq;

//namespace AgentSharp.Core
//{
//  public class MCPClient
//  {
//    public async Task Teste()
//    {
//      var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
//      {
//        Name = "Everything",
//        Command = "npx",
//        Arguments = new List<string>()
//        {
//          "-y",
//          "@modelcontextprotocol/server-everything"
//        },
//      });

//      var client = await McpClientFactory.CreateAsync(clientTransport);

//      // Print the list of tools available from the server.
//      foreach (var tool in await client.ListToolsAsync())
//      {
//        Console.WriteLine($"{tool.Name} ({tool.Description})");
//      }

//      // Execute a tool (this would normally be driven by LLM tool invocations).
//      var result = await client.CallToolAsync(
//          "echo",
//          new Dictionary<string, object>() { ["message"] = "Hello MCP!" },
//          cancellationToken: CancellationToken.None);

//      // echo always returns one and only one text content object
//      Console.WriteLine(result.Content.First(c => c.Type == "text").Text);
//    }
//  }
//}
