using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;


public class PlayerController : CreatureController
{
    protected Coroutine _coSkill;
    protected bool _rangedSkill = false;

    protected override void Init()
    {
        base.Init();
        AddHpBar();
    }

    protected override void UpdateAnimation()
    {
        //if(_animaotr == null || _sprite == null)
        //      return;     // 맨처음 실행시, ani, sprite null이어서... 예외 처리
        if (State == CreatureState.Idle)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    //_animator.Play("IDLE_BACK");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    //_animator.Play("IDLE_FRONT");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    //_animator.Play("IDLE_RIGHT");
                    //_sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    //_animator.Play("IDLE_RIGHT");
                    //_sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    //_animator.Play("WALK_BACK");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    //_animator.Play("WALK_FRONT");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    //_animator.Play("WALK_RIGHT");
                    //_sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    //_animator.Play("WALK_RIGHT");
                    //_sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    //_animator.Play(_rangedSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    //_animator.Play(_rangedSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
                    //_sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    //_animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    //_sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    //_animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    //_sprite.flipX = false;
                    break;
            }
        }
        else
        {

        }
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    //protected override void UpdateIdle()
    //{
     //   // 이동 상태로 갈지 확인
    //    if (Dir != MoveDir.Down)
    //    {
    //        State = CreatureState.Moving;
   //        return;
    //    }
   // }

    protected virtual void CheckUpdatedFlag()
    {

    }

    IEnumerator CoStartPunch()
    {
        // 대기 시간
        _rangedSkill = false; 
        State = CreatureState.Skill; // 크리쳐 상태 스킬    
        yield return new WaitForSeconds(0.5f);  // 스킬 사용여부는 서버에서 체크해주기도 하지만,
                                                // 클라쪽에도 걸어 무한정 스킬 사용 요청 방지
        State = CreatureState.Idle; // 크리쳐 상태 기본    
        _coSkill = null;
        CheckUpdatedFlag();
    }

    IEnumerator CoStartShootBomb()
    {       
        // 스킬 쿨타임 2초
        _rangedSkill = true;
        State = CreatureState.Skill; // 크리쳐 상태 스킬    
        yield return new WaitForSeconds(2f);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag();
    }

    public override void OnDamaged()
    {
        Debug.Log("Player HIT !");
    }

    public void UseSkill(int skillId)
    {
        if(skillId == 1)
        {
            _coSkill = StartCoroutine("CoStartPunch");
        }
        else if (skillId == 2)
        {
            _coSkill = StartCoroutine("CoStartShootBomb");
        }
    }
}
