using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

public class NetworkManager
{
	ServerSession _session = new ServerSession();

	public void Send(IMessage packet)
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

		Send(new ArraySegment<byte>(sendBuffer));
	}

    private void Send(ArraySegment<byte> arraySegment)
    {
		_session.Send(arraySegment);
    }

	public void Init()
	{
		// DNS (Domain Name System)
		string host = Dns.GetHostName();
		IPHostEntry ipHost = Dns.GetHostEntry(host);
		IPAddress ipAddr = ipHost.AddressList[0];
		IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

		Connector connector = new Connector();

		connector.Connect(endPoint,
			() => { return _session; },
			1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}	
	}

}
