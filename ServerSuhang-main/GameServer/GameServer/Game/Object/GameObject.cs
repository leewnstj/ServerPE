
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;

        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }

        public GameRoom Room { get; set; }

        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; }  = new StatInfo();

        public int Level
        {
            get { return Stat.Level; }
            set { Stat.Level = value; }
        }
        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = value; }
        }
        public int MaxHp
        {
            get { return Stat.MaxHp; }
            set { Stat.MaxHp = value; }
        }
        public int Atk
        {
            get { return Stat.Atk; }
            set { Stat.Atk = value; }
        }
        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }

        public Vector3 CellPos
        {
            get
            {
                return new Vector3(Info.PosInfo.PosX, Info.PosInfo.PosY, Info.PosInfo.PosZ);
            }
            set
            {
                Info.PosInfo.PosX = (int)value.X;
                Info.PosInfo.PosY = (int)value.Y;
                Info.PosInfo.PosZ = (int)value.Z;
            }
        }
        // 인자 안받는 앞체크, 추후 사용 예정
        public Vector3 GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }
        // 인자 받는 앞체크, Player에서 가져옴
        public Vector3 GetFrontCellPos(MoveDir dir)
        {
            Vector3 cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector3.UnitZ;
                    break;
                case MoveDir.Down:
                    cellPos -= Vector3.UnitZ;
                    break;
                case MoveDir.Left:
                    cellPos -= Vector3.UnitX;
                    break;
                case MoveDir.Right:
                    cellPos += Vector3.UnitX;
                    break;
            }

            return cellPos;
        }

        public List<Vector3> GetSplashCellPos()
        {
            List<Vector3> splashCells = new List<Vector3>();
            Vector3 cellPos = CellPos;

            // 현재 위치 포함
            splashCells.Add(cellPos);

            // 주변 1칸 이내의 모든 셀 위치 추가
            splashCells.Add(cellPos + new Vector3(1, 0, 0)); // Right
            splashCells.Add(cellPos + new Vector3(-1, 0, 0)); // Left
            splashCells.Add(cellPos + new Vector3(0, 0, 1)); // Up
            splashCells.Add(cellPos + new Vector3(0, 0, -1)); // Down

            // 대각선 위치 추가
            splashCells.Add(cellPos + new Vector3(1, 0, 1)); // Up-Right
            splashCells.Add(cellPos + new Vector3(1, 0, -1)); // Down-Right
            splashCells.Add(cellPos + new Vector3(-1, 0, 1)); // Up-Left
            splashCells.Add(cellPos + new Vector3(-1, 0, -1)); // Down-Left

            return splashCells;
        }
        // 피격 메서드 - Bomb에서 사용
        public virtual void OnDamaged(GameObject attacker, GameObject fired, int damage)
        {
            
            fired.Info.StatInfo.Hp = Math.Max(fired.Hp - damage, 0);    // HP 감소
            Console.WriteLine($"Player HP : {fired.Hp}");
            // 체력 깎았다는 정보 모두에게 브로드캐스트
            S_ChangeHp changeHpPacket = new S_ChangeHp();
            changeHpPacket.ObjectId = fired.Id;
            changeHpPacket.Hp = fired.Hp;
            Room.BroadCast(changeHpPacket);

            if (fired.Hp <= 0)   // HP 0 이하
            {
                OnDead(attacker);
                Console.WriteLine($"{fired}가 죽었습니다");
            }
        }
        // 죽었을 때
        public virtual void OnDead(GameObject attacker)
        {
            //S_Die diePacket = new S_Die();
            //diePacket.ObjectId = Id;
            //diePacket.AttackerId = attacker.Id;
            //Room.Broadcast(diePacket);

            //GameRoom room = Room;
            //room.LeaveGame(Id);
        }
    }
}
