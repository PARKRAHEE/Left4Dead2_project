using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScreamZombie : ZombieBase
{
    [SerializeField] private float screamRange = 5f;
    [SerializeField] private ParticleSystem screamParticle;

    private bool hasScreamed = false;
    private bool isScreaming = false;

    protected override void HandleBehavior()
    {
        if (agent == null || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 1. 비명 중이면 끝날 때까지 유지
        if (isScreaming)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Zombie_Scream") && state.normalizedTime >= 1f)
            {
                EndScream();
            }

            return;
        }

        // 2. 거리 벗어나면 다시 비명 가능
        if (distance > screamRange)
        {
            hasScreamed = false;
        }

        // 3. 처음 들어왔을 때만 비명
        if (distance <= screamRange && !hasScreamed)
        {
            StartScream();
            return;
        }

        // 4. 기본 행동
        base.HandleBehavior();
    }

    private void StartScream()
    {
        hasScreamed = true;
        isScreaming = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        if (anim != null)
            anim.SetTrigger("Scream");

        if (screamParticle != null)
            screamParticle.Play();
    }

    private void EndScream()
    {
        isScreaming = false;

        agent.isStopped = false;

        agent.SetDestination(player.position);

        if (screamParticle != null)
            screamParticle.Stop();
    }

}
