
using GameServer.Session;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; } // Room 고유ID

        //List<Player> _players = new List<Player>();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();   // List -> Dictionary 
        Dictionary<int, Bomb> _bombs = new Dictionary<int, Bomb>();
        Dictionary<int, Item> _items = new Dictionary<int, Item>();

        // 방에 들어가기
        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)  // 새로운 플레이어 유/무 확인
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock (_lock)    // 무언가 새로 추가하는 건 Lock이 꼭 필요
            {
                if (type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    player.Room = this;  // 새플레이어에 현재 방 등록

                    // 새플레이어 본인한테 정보 전송
                    {
                        S_Enter enterPacket = new S_Enter();    // 방 입장 패킷
                        enterPacket.Player = player.Info;
                        player.Session.ProtoSend(enterPacket);

                        S_Spawn spawnPacket = new S_Spawn();    // 스폰 패킷
                        foreach (Player p in _players.Values)
                        {
                            if (player != p) // 내 정보는 제외
                            {
                                spawnPacket.Objects.Add(p.Info);    // 방안에 다른 플레이어 정보 추가
                            }

                        }
                        player.Session.ProtoSend(spawnPacket);   // 방안 플레이어 정보 -> 새플레이어에게 전송
                        Console.WriteLine($"spawnPacket 전송 :{spawnPacket.Objects}");
                    }

                }
                else if (type == GameObjectType.Bomb)
                {
                    Bomb bomb = gameObject as Bomb;
                    _bombs.Add(gameObject.Id, bomb);
                    bomb.Room = this;   // 게임룸 입장
                }
                else if (type == GameObjectType.Item)
                {
                    Item item = gameObject as Item;
                    _items.Add(gameObject.Id, item);
                    item.Room = this;   // 게임룸 정보 부여
                }

                //  새플레이어 정보 -> 방안 기존 플레이어한테 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id) // 새 플레이어 제외
                            p.Session.ProtoSend(spawnPacket);  // 방안 기존 플레이어한테 전송 
                    }
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.Remove(objectId, out player) == false)
                        return;
                    player.Room = null;

                    // 본인한테 정보 전송
                    {
                        S_Leave leavePacket = new S_Leave();
                        player.Session.ProtoSend(leavePacket);
                    }
                }
                else if (type == GameObjectType.Bomb)
                {
                    Bomb bomb = null;
                    if (_bombs.Remove(objectId, out bomb) == false)
                        return;

                    bomb.Room = null;
                }
                else if (type == GameObjectType.Item)
                {
                    Item item = null;
                    if (_items.Remove(objectId, out item) == false)
                        return;

                    item.Room = null;
                }



                // 타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectIds.Add(objectId);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != objectId)
                            p.Session.ProtoSend(despawnPacket);
                    }
                }
            }
        }

        public void BroadCast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.ProtoSend(packet);
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                // 서버에서 플레이어 좌표 이동
                ObjectInfo info = player.Info;
                info.PosInfo = movePacket.PosInfo;

                // 다른 플레이어에게 이동정보 브로드캐스트
                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.Info.ObjectId;
                resMovePacket.PosInfo = movePacket.PosInfo;

                BroadCast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null) return;

            lock (_lock)
            {
                ObjectInfo info = player.Info;
                // 상태 Idle일 때 스킬 사용 가능
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                // 통과되어 스킬 상태 전환
                info.PosInfo.State = CreatureState.Skill;

                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = info.ObjectId; // 누가 스킬쓰는지?
                skill.Info.SkillId = skillPacket.Info.SkillId; // 어떤 스킬?
                BroadCast(skill);   // 스킬 사용 패킷 전송

                if (skillPacket.Info.SkillId == 1)
                {
                    // TODO : 데미지 판정
                    Vector3 skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                    Console.WriteLine($"skillPos : {skillPos}");

                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            if (p.Info.PosInfo.PosX == skillPos.X &&
                                p.Info.PosInfo.PosY == skillPos.Y &&
                                p.Info.PosInfo.PosY == skillPos.Y)
                                Console.WriteLine($"{p.Info.ObjectId} Hit Player");
                    }
                }
                else if (skillPacket.Info.SkillId == 2)
                {
                    Bomb bomb = ObjectManager.Instance.Add<Bomb>();
                    if (bomb == null)
                        return;
                    bomb.Owner = player;
                    bomb.PosInfo.State = CreatureState.Moving;
                    bomb.PosInfo.PosX = (int)player.CellPos.X;
                    bomb.PosInfo.PosY = (int)player.CellPos.Y;
                    bomb.PosInfo.PosZ = (int)player.CellPos.Z;
                    Console.WriteLine($"Bomb Located {bomb.PosInfo.PosX},{bomb.PosInfo.PosZ}");
                    EnterGame(bomb);
                    Task.Run(async () =>
                    {

                        await Task.Delay(2000);
                        //ActiveBomb(bomb);
                        bomb.ActiveBomb(_players);
                        // 어디에 아이템을 생성해야할까???
                        GenerateItem(player);   // 폭탄 터질때 플레이어 근처 아이템 생성


                    });
                }

            }
        }
        public void GenerateItem(Player player)
        {
            if (player == null)
            {
                Console.WriteLine("GenItem null");
                return;
            }

            lock (_lock)
            {
                ObjectInfo info = player.Info;
                // 상태 Idle일 때 스킬 사용 가능
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                Item item = ObjectManager.Instance.Add<Item>();
                if (item == null)
                    return;
                item.PosInfo.State = CreatureState.Moving;
                Random rand = new Random(); // 아이템 랜덤위치 생성 
                item.PosInfo.PosX = info.PosInfo.PosX + rand.Next(10);
                item.PosInfo.PosY = info.PosInfo.PosY;
                item.PosInfo.PosZ = info.PosInfo.PosZ + rand.Next(-10, 10); ;
                Console.WriteLine($"item Located {item.PosInfo.PosX},{item.PosInfo.PosZ}");
                EnterGame(item);    // 게임룸 입장
            }
        }
        public void JudgeGetItem(C_ItemGet iGetPacket, Player player)
        {
            // iGetPacket 아이템 취득 판정
            // player 에게 아이템 효과 부여
            lock (_lock)
            {
                foreach (Item i in _items.Values)
                {
                    if (i.Id == iGetPacket.Iteminfo.ItemId)
                    {
                        Console.WriteLine($"{player} item Get! {iGetPacket}");
                        S_ItemGet sGetPacket = new S_ItemGet() { Iteminfo = new ItemInfo() };
                        sGetPacket.Iteminfo.ItemId = i.Id;
                        player.Session.ProtoSend(sGetPacket);
                        S_ChangeHp changeHpPacket = new S_ChangeHp();
                        changeHpPacket.ObjectId = player.Id;
                        player.Hp = player.Hp + 10;
                        changeHpPacket.Hp = player.Hp;
                        BroadCast(changeHpPacket);
                        LeaveGame(i.Id);
                    }
                }
            }
        }
        public void Update()
        {
            lock (_lock)
            {
                foreach (Bomb bomb in _bombs.Values)
                {
                    bomb.Update();
                }
            }
        }
    }
}
