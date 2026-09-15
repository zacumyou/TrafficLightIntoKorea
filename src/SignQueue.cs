using System.Collections.Generic;
namespace TrafficLightIntoKorea {
// Unity's reference assemblies expose Queue in two assemblies. An amortized
// list queue also avoids RemoveAt(0) during city initialization.
internal sealed class SignQueue<T> {
 private readonly List<T> _items=new List<T>();private int _head;
 internal int Count=>_items.Count-_head;
 internal void Enqueue(T item)=>_items.Add(item);
 internal T Dequeue(){var value=_items[_head];_items[_head++]=default(T);if(_head==_items.Count)Clear();else if(_head>=1024&&_head>=_items.Count/2){_items.RemoveRange(0,_head);_head=0;}return value;}
 internal void Clear(){_items.Clear();_head=0;}
}
}
