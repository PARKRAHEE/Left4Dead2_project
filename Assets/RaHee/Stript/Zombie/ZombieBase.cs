using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBase : MonoBehaviour
{
    #region 설정 값
    [Header("감지 범위")]
    [SerializeField] protected float detectRange = 10f;

    [Header("체력")]
    [SerializeField] protected float health = 50f;

    [Header("공격 거리")]
    [SerializeField] protected float attackRange = 2f;

    [Header("이동 속도")]
    [SerializeField] protected float moveSpeed = 3.5f;

    [Header("사망 파티클")]
    [SerializeField] private ParticleSystem deathParticle;

    [Header("파티클 지연")]
    [SerializeField] private float deathParticleDelay = 2f;
    #endregion

    #region 컴포넌트
    protected Transform player; // 플레이어 참조
    protected NavMeshAgent agent; // 이동 제어
    protected Animator anim; // 애니메이션 제어
    #endregion

    #region FSM
    protected enum ZombieState
    {
        Idle, // 대기 상태
        Chase, // 추적 상태
        Attack, // 공격 상태
        Dead // 사망 상태
    }

    protected ZombieState currentState; // 현재 상태
    #endregion

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // 플레이어 탐색
        if (playerObj != null)
            player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent 가져오기
        anim = GetComponent<Animator>(); // Animator 가져오기

        agent.speed = moveSpeed; // 이동 속도 설정
        agent.stoppingDistance = attackRange * 0.7f; // 정지 거리 설정

        //ChangeState(ZombieState.Chase); // 초기 상태 설정
        currentState = ZombieState.Idle;
    }

    protected virtual void Update()
    {
        if (currentState == ZombieState.Dead || player == null) // 사망 또는 타겟 없음
            return;

        float distance = Vector3.Distance(transform.position, player.position); // 좀비와 플레이어 거리 계산

        switch (currentState)
        {
            case ZombieState.Idle:

                agent.isStopped = true;

                if (distance <= detectRange)
                {
                    ChangeState(ZombieState.Chase);
                }

                break;

            case ZombieState.Chase:
                if (distance <= attackRange + 0.2f) // 공격 범위 + 여유 거리(오차 보정)
                {
                    ChangeState(ZombieState.Attack); // 공격 상태 전환
                    return;
                }

                agent.isStopped = false; // 이동 활성화
                agent.SetDestination(player.position); // 목표 위치 설정
                break;

            case ZombieState.Attack:
                if (distance > attackRange + 0.2f) // 범위 이탈
                {
                    ChangeState(ZombieState.Chase); // 추적 상태 전환
                    return;
                }

                agent.isStopped = true; // 이동 정지
                agent.ResetPath(); // 경로 초기화

                Vector3 dir = (player.position - transform.position).normalized; // 방향 계산
                dir.y = 0;

                if (dir != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(dir); // 목표 방향 회전
                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime); // 부드러운 회전
                }

                HandleAttack();


                break;
        }

        UpdateAnimation(); // 애니메이션 갱신
    }

    protected virtual void HandleAttack()
    {

    }

    protected virtual void ChangeState(ZombieState newState)
    {
        currentState = newState; // 상태 변경

        if (anim != null)
        {
            anim.SetBool("Attack", newState == ZombieState.Attack); // 공격 애니메이션 설정
        }

        if (newState == ZombieState.Chase)
        {
            agent.isStopped = false; // 이동 활성화
        }
        else if (newState == ZombieState.Attack)
        {
            agent.isStopped = true; // 이동 정지
            agent.ResetPath(); // 경로 초기화
        }
    }

    protected void UpdateAnimation()
    {
        if (anim != null && agent != null)
        {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentState == ZombieState.Dead) // 사망 상태 체크
            return;

        health -= damage; // 체력 감소

        if (health <= 0f)
            Die(); // 사망 처리
    }

    protected virtual void Die()
    {
        currentState = ZombieState.Dead; // 상태 변경

        if (agent != null)
        {
            agent.isStopped = true; // 이동 정지
            agent.ResetPath(); // 경로 초기화
            agent.enabled = false; // 에이전트 비활성화
        }

        StopAllCoroutines(); // 코루틴 중지

        if (anim != null)
        {
            anim.SetBool("Attack", false); // 공격 애니 OFF
            anim.SetTrigger("Die"); // 사망 애니 실행
        }

        if (deathParticle != null)
        {
            StartCoroutine(PlayDeathParticleDelayed(deathParticleDelay)); // 파티클 실행
        }

        StartCoroutine(DestroyAfterDeath()); // 오브젝트 삭제
    }

    private IEnumerator PlayDeathParticleDelayed(float delay)
    {
        yield return new WaitForSeconds(delay); // 딜레이

        if (deathParticle != null)
        {
            deathParticle.Play(true); // 파티클 재생
        }
    }

    private IEnumerator DestroyAfterDeath()
    {
        /*
        yield return null;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0); // 애니 상태 정보
        yield return new WaitForSeconds(info.length); // 애니 길이 대기

        Destroy(gameObject); // 오브젝트 삭제
        */

        // Die 상태 들어갈 때까지 대기
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            yield return null;
        }

        // 현재 Die 애니 길이 가져오기
        AnimatorStateInfo info =
            anim.GetCurrentAnimatorStateInfo(0);

        // 애니 끝날 때까지 대기
        yield return new WaitForSeconds(info.length);

        Destroy(gameObject);
    }
}