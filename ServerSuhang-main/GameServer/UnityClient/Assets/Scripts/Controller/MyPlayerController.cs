using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        // 쿨타임, 키입력 체크
        if (_coSkillCooltime == null && Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Punch");

            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            skill.Info.SkillId = 1;
            Managers.Network.Send(skill);   // 스킬 사용 서버로 요청

            _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);   // 쿨타임 0.2s
        }
        else if (_coSkillCooltime == null && Input.GetKey(KeyCode.R))
        {
            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            skill.Info.SkillId = 2;
            Managers.Network.Send(skill);   // 스킬 사용 서버로 요청

            _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);   // 쿨타임 0.2s
        }
    }

    Coroutine _coSkillCooltime; // 쿨타임 체크 코루틴
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCooltime = null;
    }

    void LateUpdate()   // 내 위치로 카메라 따라가도록
    {
        //Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    // 키보드 입력
    void GetDirInput()
    {
        _moveKeyPressed = true;
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag(); // 아무동작 없어도 기본적으로 상태 체크
            return;
        }

        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.forward;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.back;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        if (Managers.Object.Find(destPos) == null)
        {
            CellPos = destPos;
            State = CreatureState.Moving;
        }

        CheckUpdatedFlag();  // 이동 후에도 상태 체크

    }

    protected override void CheckUpdatedFlag()
    {  
        if(_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;   // 상태정보 보냈으니 다시 false로 초기화
        }
    }
}
