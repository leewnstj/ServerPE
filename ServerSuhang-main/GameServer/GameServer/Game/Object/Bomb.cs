using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Bomb : GameObject
    {
        #region
        public GameObject Owner { get; set; }
        public int BombDamage { get; set; }
        public Bomb()
        {
            ObjectType = GameObjectType.Bomb;
            BombDamage = 10;    // 기본 폭탄 데미지
        }
        #endregion
        public async void Update()
        {
            //  쏜 주인도 없고, 방도 없으면 나가기
            if (Owner == null || Room == null) return;
        }
        public void ActiveBomb(Dictionary<int, Player> _players)
        {
            if (this == null) return;

            List<Vector3> splashCells = this.GetSplashCellPos();

            // 폭발 범위 내의 플레이어들에게 데미지 판정
            foreach (Player player in _players.Values)
            {
                Vector3 playerPos
                    = new Vector3(player.Info.PosInfo.PosX,
                    player.Info.PosInfo.PosY, player.Info.PosInfo.PosZ);
                if (splashCells.Contains(playerPos))
                {
                    Console.WriteLine($"{player.Info.ObjectId} Hit by Bomb Damage : {BombDamage * Owner.Atk}");
                    // TODO: 데미지 처리 로직 추가
                    this.OnDamaged(this, player, BombDamage * Owner.Atk);  // 폭탄을 맞은 player에게 데미지입히기
                }
            }
            // 폭탄 제거
            //RoomManager.Instance.Find(1).LeaveGame(this.Id);
            Room.LeaveGame(this.Id);
        }

    }
}
