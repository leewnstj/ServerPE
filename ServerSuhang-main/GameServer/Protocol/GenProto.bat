
protoc.exe -I=./ --csharp_out=./ ./Protocol.proto

IF ERRORLEVEL 1 PAUSE	


START ../PacketGenerator/bin/Debug/net8.0/PacketGenerator.exe ../Protocol/Protocol.proto
XCOPY /Y Protocol.cs "../UnityClient/Assets/Scripts/Protocol/Generated"
XCOPY /Y Protocol.cs "../GameServer/Protocol/Generated"
XCOPY /Y ClientPacketManager.cs "../UnityClient/Assets/Scripts/Protocol"
XCOPY /Y ServerPacketManager.cs "../GameServer/Protocol"


