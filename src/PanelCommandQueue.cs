using System.Collections.Generic;
namespace TrafficLightIntoKorea {
// UI callbacks enqueue; the normal UI update consumes one command at a time.
internal sealed class PanelCommandQueue {
 private readonly List<string> items=new List<string>();
 internal int Count=>items.Count;
 internal void Enqueue(string value){items.Add(value);}
 internal string Dequeue(){var value=items[0];items.RemoveAt(0);return value;}
 internal void Clear(){items.Clear();}
}
}
