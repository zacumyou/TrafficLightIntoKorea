using System;using System.IO;using System.Text;using Newtonsoft.Json;using Newtonsoft.Json.Linq;
namespace TrafficLightIntoKorea {
internal static class SidecarStorage {
 internal static SignalSidecar Read(string file,out bool backup){
  backup=false;if(!File.Exists(file)&&!File.Exists(file+".bak"))return null;
  Exception primary=null;
  try {if(File.Exists(file))return Parse(File.ReadAllText(file));}catch(NotSupportedException){throw;}catch(Exception e){primary=e;}
  try {if(File.Exists(file+".bak")){var data=Parse(File.ReadAllText(file+".bak"));backup=true;return data;}}catch(Exception e){throw new InvalidDataException("Settings and backup could not be read",new AggregateException(primary??new FileNotFoundException(file),e));}
  throw new InvalidDataException("Settings could not be read and no usable backup exists",primary);
 }
 internal static SignalSidecar Parse(string json){var obj=JObject.Parse(json);if(obj["schema"]!=null&&(int)obj["schema"]!=1)throw new NotSupportedException("Unsupported sidecar schema; preserve the original settings");var data=JsonConvert.DeserializeObject<SignalSidecar>(json);if(data==null||data.schema!=1||data.nodes==null)throw new InvalidDataException("Unsupported or invalid sidecar");return data;}
 internal static void Write(string file,string json){
  Parse(json);Directory.CreateDirectory(Path.GetDirectoryName(file));var temp=file+".tmp";
  using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){var bytes=new UTF8Encoding(false).GetBytes(json);stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
  if(File.Exists(file)){
   // Do not replace a healthy backup with the corrupt primary we recovered from.
   bool healthy;try{Parse(File.ReadAllText(file));healthy=true;}catch{healthy=false;}
   File.Replace(temp,file,healthy?file+".bak":null);
  }else File.Move(temp,file);
 }
}
}
