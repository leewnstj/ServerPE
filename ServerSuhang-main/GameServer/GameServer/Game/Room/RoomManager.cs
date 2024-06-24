using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public GameRoom Add()               // 게임룸 새로 생성
        {
            GameRoom gameRoom = new GameRoom();

            lock (_lock)
            {
                gameRoom.RoomId = _roomId;
                _rooms.Add(_roomId, gameRoom);
                _roomId++;  // ID 증가
            }

            return gameRoom;
        }

        public bool Remove(int roomId)      // 게임룸 제거
        {
            lock (_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)    // 게임룸 찾기
        {
            lock (_lock)
            {
                GameRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;

                return null;
            }
        }
    }
}
