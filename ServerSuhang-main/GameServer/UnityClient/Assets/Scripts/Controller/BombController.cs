using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : CreatureController
{
    // Start is called before the first frame update

    protected override void Init()
    {
        State = CreatureState.Moving;
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        // 추후 기능 추가
    }

    protected void ActiveBomb()
    {
        // 추후 기능 추가
    }
}

