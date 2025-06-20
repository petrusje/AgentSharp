using System;
using System.Collections.Generic;

namespace Arcana.AgentsNet.Core.Memory
{
  /// <summary>
  /// Interface base para qualquer item armazenável em memória
  /// </summary>
  public interface IMemoryItem
  {
    string Id { get; }
    DateTime Timestamp { get; }
    double Relevance { get; set; }
    string Content { get; }
    string Type { get; }
    Dictionary<string, object> Metadata { get; }
  }

  /// <summary>
  /// Representa um item básico na memória
  /// </summary>
  public class MemoryItem : IMemoryItem
  {
    public string Id { get; }
    public DateTime Timestamp { get; }
    public double Relevance { get; set; }
    public string Content { get; }
    public string Type { get; }
    public Dictionary<string, object> Metadata { get; }

    public MemoryItem(string content, string type = "text", double relevance = 1.0, Dictionary<string, object> metadata = null)
    {
      Id = Guid.NewGuid().ToString();
      Timestamp = DateTime.UtcNow;
      Content = content;
      Type = type;
      Relevance = relevance;
      Metadata = metadata ?? new Dictionary<string, object>();
    }
  }


}
