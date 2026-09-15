using System;
using System.IO;
using System.Reflection;
using System.Text;
namespace TrafficLightIntoKorea {
internal static class IndividualMenuScript {
 internal const string ResourceName="TrafficLightIntoKorea.IndividualSignals.js";
 internal static string Read(Assembly assembly){
  if(assembly==null)throw new ArgumentNullException(nameof(assembly));
  using(var stream=assembly.GetManifestResourceStream(ResourceName)){
   if(stream==null)throw new InvalidOperationException("Embedded individual signal menu is missing.");
   using(var reader=new StreamReader(stream,Encoding.UTF8,true)){
    var script=reader.ReadToEnd();
    if(string.IsNullOrWhiteSpace(script))throw new InvalidOperationException("Embedded individual signal menu is empty.");
    return script;
   }
  }
 }
}
}
