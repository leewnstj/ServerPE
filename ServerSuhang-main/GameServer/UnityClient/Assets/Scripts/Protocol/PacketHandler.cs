using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

class PacketHandler
{
    // S_ 패킷 수신 후 동작 정의
    public static void S_EnterHandler(PacketSession session, IMessage packet)
    {
        S_Enter enterGamePacket = packet as S_Enter;
        Debug.Log("S_EnterHandler");
        Debug.Log(enterGamePacket.Player);
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }

    public static void S_LeaveHandler(PacketSession session, IMessage packet)
    {
        S_Leave leaveHandler = packet as S_Leave;
        Managers.Object.RemoveMyPlayer();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
            Debug.Log($"S_SpawnHandler : {spawnPacket.Objects}");
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;

        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;
        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
            return;
        cc.PosInfo = movePacket.PosInfo;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;
        // 스킬 쓴 오브젝트 찾기
        GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
        if (go == null)
            return;
        // 스킬 쓴 오브젝트의 조작 가져오기
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            pc.UseSkill(skillPacket.Info.SkillId);  // 스킬 발동!
        Debug.Log($"System : {go.name}님이 {skillPacket.Info.SkillId} 스킬 사용");
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changeHpPacket = packet as S_ChangeHp;

        GameObject go = Managers.Object.FindById(changeHpPacket.ObjectId);
        if (go == null)
            return;
        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        { 
            cc.Hp = changeHpPacket.Hp;
            Debug.Log($"ChangeHP : {cc.Hp}");
        }
    }
    public static void S_ItemGetHandler(PacketSession session, IMessage packet)
    {
        // 서버로 부터 아이템 생성 통지
        Debug.Log($"Creature : {packet}");
    }
}

