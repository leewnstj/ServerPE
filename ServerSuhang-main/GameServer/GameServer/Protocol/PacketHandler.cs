using Google.Protobuf.Protocol;
using Google.Protobuf;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Session;
using System.ComponentModel;
using GameServer;


class PacketHandler
{
    // S_TestHandler 패킷 수신 후 동작 정의
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
        Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}" +
            $", {movePacket.PosInfo.PosZ})");

        // 플레이어가 방에 없을 경우 종료
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        // 어쨋든, room은 먼저 사라지지 않기때문에 
        GameRoom room = player.Room;
        if(room == null) 
            return;

        room.HandleMove(player, movePacket);
    }
    // 스킬 핸들러
    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer;
        if(player == null) return;

        GameRoom room = player.Room;
        if(room == null) return;

        room.HandleSkill(player, skillPacket);  // GameRoom에서 호출
    }

    public static void C_ItemGetHandler(PacketSession session, IMessage packet)
    {
        C_ItemGet iGetPacket = packet as C_ItemGet;
        ClientSession clientSession = session as ClientSession;
        Player player = clientSession.MyPlayer; // 아이템을 취득한 클라이언트세션의 플레이어 정보

        GameRoom room = player.Room;
        if (room == null) return;

        room.JudgeGetItem(iGetPacket, player );     // 아이템 취득 판정
    }

    





}
