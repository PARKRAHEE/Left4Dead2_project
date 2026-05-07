using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZBookHead : ZombieBase
{
    #region 업데이트
    protected override void Update()
    {
        base.Update();

        if (anim == null)
            return;

        // 공격 애니 중 이동 방지
        AnimatorStateInfo stateInfo =
            anim.GetCurrentAnimatorStateInfo(0);

        bool isAttackAnim =
            stateInfo.IsName("Attack_1") ||
            stateInfo.IsName("Attack_2");

        if (isAttackAnim)
        {
            agent.isStopped = true;
        }
    }
    #endregion

    #region 공격 처리
    protected override void HandleAttack()
    {
        // Animator에서 공격 루프 처리
    }
    #endregion
}
