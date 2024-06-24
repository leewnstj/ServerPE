
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Session
{
    public class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }    // 세션에 속한 플레이어 구분
        public int SessionId { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            MyPlayer = ObjectManager.Instance.Add<Player>();    // 플레이어 추가
            {
                MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;
                MyPlayer.Info.PosInfo.PosZ = 0;
                MyPlayer.Session = this;
            }
            RoomManager.Instance.Find(1).EnterGame(MyPlayer);   // 게임 룸 입장!
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);   // 1번방 아웃


            //Console.WriteLine($"OnDisconnected : {endPoint}");

        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // 패킷매니저 실행하여 패킷 수신
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }


        // 직렬화 및 전송을 위한 래핑 함수
        public void ProtoSend(IMessage packet)  // IMessage타입 -> ArraySegment타입 변환 후 전송
        {
            Send(new ArraySegment<byte>(MakeSendBuffer(packet)));
        }

        // 직렬화 함수
        // NC소프트 방식
        public static byte[] MakeSendBuffer(IMessage packet)    // IMessage타입 : 프로토버프에서 상속되어있음
        {

            // 패킷 헤더 정의
            // [2:Size][2:ID][?:Payload]
            // 2byte : packet Size
            // 2byte : packet ID
            // ?byte : Payload : 패킷 내용물, 길이는 가변적일 수 있음.

            // (1) ID 헤더 만들기   : Enum에서 동일한 이름 찾아서 변환
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);

            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            // (2) Size 헤더 만들기 : 패킷에서 사이즈 체크
            ushort size = (ushort)packet.CalculateSize();

            // (3) 패킷 객체를 바이트배열로 변환
            byte[] sendBuffer = new byte[size + 4]; // 헤더사이즈 4 추가한 임시 패킷 버퍼 생성
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));  // ID,Size,Payload 전체 크기 sendbuffer 0바이트 위치에 할당
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));  // 2바이트 위치에, ID값 할당
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);  // 4바이트 위치에 페이로드 할당

            return sendBuffer;
        }




    }
}
