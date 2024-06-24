using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	
	public static GameObjectType GetObjectTypeById(int id)
	{
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
	}

	public void Add(ObjectInfo info, bool myPlayer = false)
	{
        Debug.Log($"Object Add {info.ObjectId}");

        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
		if (objectType == GameObjectType.Player)
		{
			if (myPlayer)
			{
				GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
				go.name = info.Name;
				_objects.Add(info.ObjectId, go);
				Debug.Log($"Object Add {info.ObjectId}");

				MyPlayer = go.GetComponent<MyPlayerController>();
				MyPlayer.Id = info.ObjectId;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.Stat = info.StatInfo;
				Vector3Int vec = new Vector3Int(MyPlayer.PosInfo.PosX, MyPlayer.PosInfo.PosY, MyPlayer.PosInfo.PosZ);
				MyPlayer.SyncPos(vec);
				Debug.Log("Myplayer Add");
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate("Creature/Player");
				go.name = info.Name;
				_objects.Add(info.ObjectId, go);

				PlayerController pc = go.GetComponent<PlayerController>();
				pc.Id = info.ObjectId;
				pc.PosInfo = info.PosInfo;
				pc.Stat = info.StatInfo;
				Vector3Int vec = new Vector3Int(pc.PosInfo.PosX, pc.PosInfo.PosY, pc.PosInfo.PosZ);
				pc.SyncPos(vec);
				Debug.Log("player Add");

			}
		}
		else if (objectType == GameObjectType.Bomb)
		{
			GameObject go = Managers.Resource.Instantiate("Creature/Bomb");
			go.name = "Bomb";
			_objects.Add(info.ObjectId, go);


			BombController bc = go.GetComponent<BombController>();
			bc.PosInfo = info.PosInfo;
			bc.Stat = info.StatInfo;
			Vector3Int vec = new Vector3Int(bc.PosInfo.PosX, bc.PosInfo.PosY, bc.PosInfo.PosZ);
			bc.SyncPos(vec);
		}
		else if (objectType == GameObjectType.Item)	
		{
			GameObject io = Managers.Resource.Instantiate("Creature/Coin");
			io.name = "Item";	// 프리팹 Coin 생성
			_objects.Add(info.ObjectId, io);
            Debug.Log($"item posinfo : {info.PosInfo}");

            ItemController ic = io.GetComponent<ItemController>();
			ic.Id = info.ObjectId;	// 클라이언트의 아이템 오브젝트에 패킷정보 입력
			ic.PosInfo = info.PosInfo;
			ic.Stat = info.StatInfo;
            Vector3Int vec = new Vector3Int(ic.PosInfo.PosX, ic.PosInfo.PosY, ic.PosInfo.PosZ);
            ic.SyncPos(vec);

        }
    }

        public void Add(int id, GameObject go)
	{
		_objects.Add(id, go);
	}

	public void Remove(int id)
	{
		GameObject go = FindById(id);	// id로 오브젝트 찾기
		if (go == null)
			return;

		_objects.Remove(id);
		Managers.Resource.Destroy(go);	// 해당 오브젝 제거
	}

	public void RemoveMyPlayer()
	{
		if (MyPlayer == null)
			return;

		Remove(MyPlayer.Id);
		MyPlayer = null;
	}

	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public GameObject FindById(int id)
    {
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
    }

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
	}
}
