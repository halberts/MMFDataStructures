MMDataStructures
==================

Adapted and improved from https://mmf.codeplex.com/. Replaced all serializers 
by BinaryFormatter with easy option to replace it by setting a static `Config.Serializer`
 property. Structs deserialization must support zero bytes array - wrap any existing 
serializer as shown [here](https://github.com/halberts/MMFDataStructures/blob/master/MMDataStructures/Serializer.cs).


Use this only for IPC with in-memory or temporary files (files will be deleted when last process 
accessing them exits) or if you are on Mono and need quick and simple solution. For Windows, use
[ESENT](http://managedesent.codeplex.com/). 
<!--See [details in a comment.](https://github.com/buybackoff/MMDataStructures/issues/6#issuecomment-65485369).-->



[Licensed as Apache 2.0.](https://github.com/buybackoff/MMDataStructures/blob/master/LICENSE.md)
