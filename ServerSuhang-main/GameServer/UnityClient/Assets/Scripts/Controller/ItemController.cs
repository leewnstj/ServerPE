using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ItemController : CreatureController
{
    protected GameObject item;
    protected override void Init()
    {
        State = CreatureState.Moving;
        base.Init();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActiveItem(other.gameObject);
        }
    }

    protected override void UpdateAnimation()
    {
        // 추후 기능 추가
    }

    protected void ActiveItem(GameObject player)
    {
        MyPlayerController mc = player.GetComponent<MyPlayerController>();

        // 추후 기능 추가
        C_ItemGet cItemPacket = new C_ItemGet() { Iteminfo = new ItemInfo() };
        Debug.Log($"Creature {Id} Item Get");
        cItemPacket.Iteminfo.ItemId = Id;
        cItemPacket.Iteminfo.Name = "Coin";
        cItemPacket.Iteminfo.PosInfo = PosInfo;
        cItemPacket.PlayerObjectId = mc.Id;
        Managers.Network.Send(cItemPacket);
        Debug.Log($"Creature {mc.Id} Item Get");
    }

}



