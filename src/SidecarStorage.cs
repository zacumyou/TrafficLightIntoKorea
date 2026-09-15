using System.IO;using System.Text;
namespace TrafficLightIntoKorea {
internal static class SidecarStorage {
 internal static void Write(string file,string json){
  Directory.CreateDirectory(Path.GetDirectoryName(file));
  var temp=file+".tmp";
  using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){
   var bytes=new UTF8Encoding(false).GetBytes(json);stream.Write(bytes,0,bytes.Length);stream.Flush(true);
  }
  if(File.Exists(file))File.Replace(temp,file,file+".bak");else File.Move(temp,file);
 }
}
}
