using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Item : GameObject
    {
        public ItemInfo iteminfo { get; private set; } = new ItemInfo();

        public Item()
        {
            ObjectType = GameObjectType.Item;
        }

        public void itemEffect()
        {
            // 아이템 사용 결과
        }

    }
}


