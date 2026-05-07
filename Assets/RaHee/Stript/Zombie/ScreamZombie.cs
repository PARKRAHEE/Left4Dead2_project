using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScreamZombie : ZombieBase
{
    #region 설정 값
    [Header("Scream")]
    [SerializeField] private float screamRange = 5f;
    [SerializeField] private float screamDuration = 2.5f;
    [SerializeField] private ParticleSystem screamParticle;

    [Header("Attack")]
    [SerializeField] private AnimationClip attackClip;
    #endregion

    #region 상태 변수
    private bool isScreaming = false;
    private bool wasInRange = false;
    #endregion

    #region Update
    protected override void Update()
    {
        if (currentState == ZombieState.Dead || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 비명 중이면 모든 행동 차단
        if (isScreaming)
        {
            agent.isStopped = true;
            return;
        }

        // 5m 진입 시 1회 비명
        bool isInRange = distance <= screamRange;

        if (isInRange && !wasInRange)
        {
            StartCoroutine(ScreamRoutine());
            wasInRange = true;
            return;
        }

        if (!isInRange)
        {
            wasInRange = false;
        }

        base.Update();
    }
    #endregion

    #region 비명
    private IEnumerator ScreamRoutine()
    {
        isScreaming = true;

        agent.isStopped = true;
        agent.ResetPath();

        if (anim != null)
            anim.SetTrigger("Scream");

        if (screamParticle != null)
            screamParticle.Play();

        yield return new WaitForSeconds(screamDuration);

        if (screamParticle != null)
            screamParticle.Stop();

        isScreaming = false;

        ChangeState(ZombieState.Chase);
    }
    #endregion

}
