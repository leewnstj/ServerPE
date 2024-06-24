using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerSession : PacketSession
	{
		public override void OnConnected(EndPoint endPoint)
		{
			UnityEngine.Debug.Log($"OnConnected : {endPoint}");
			PacketManager.Instance.CustomHandler = (s, m, i) =>
			{ 
				PacketQueue.Instance.Push(i, m); 
			};	// id와 패킷을 큐에 입력
        }

		public override void OnDisconnected(EndPoint endPoint)
		{

		}

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

    public override void OnSend(int numOfBytes)
		{
		}
	}

