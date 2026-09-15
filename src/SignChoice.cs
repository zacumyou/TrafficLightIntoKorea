namespace TrafficLightIntoKorea {
// Preserve a confirmed sign across a short lane/prefab rebuild, but never retain
// an obsolete sign indefinitely or override a confirmed unsupported result.
internal sealed class SignChoice {
 internal string Selected;private float _unknownSince=-1;
 internal string Update(string candidate,bool known,float now){if(known){Selected=candidate;_unknownSince=-1;}else{if(_unknownSince<0)_unknownSince=now;if(now-_unknownSince>=10)Selected=null;}return Selected;}
}
}
