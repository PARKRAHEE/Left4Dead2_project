using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZCreep : ZombieBase
{
    #region 설정값

    [Header("점프 시작 거리")]
    [SerializeField] private float jumpRange = 15f;

    [Header("점프 높이")]
    [SerializeField] private float jumpHeight = 3f;

    [Header("점프 시간")]
    [SerializeField] private float jumpDuration = 1f;

    [Header("점프 쿨타임")]
    [SerializeField] private float jumpCooldown = 5f;

    #endregion

    #region 상태 변수

    private bool hasScreamed = false;
    private bool isJumping = false;
    private bool isScreaming = false;

    private float lastJumpTime;

    #endregion

    #region Animator Hash

    private readonly int speedHash =
        Animator.StringToHash("Speed");

    private readonly int attackHash =
        Animator.StringToHash("Attack");

    #endregion

    #region 업데이트

    protected override void Update()
    {
        if (currentState == ZombieState.Dead || player == null)
            return;

        // 비명/점프 중엔 FSM 정지
        if (isJumping || isScreaming)
            return;

        float distance =
            Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            // =========================
            // Idle
            // =========================
            case ZombieState.Idle:

                agent.isStopped = true;

                anim.SetFloat(speedHash, 0);

                // 첫 발견
                if (distance <= detectRange &&
                    !hasScreamed)
                {
                    StartCoroutine(ScreamRoutine());
                }

                break;

            // =========================
            // Chase
            // =========================
            case ZombieState.Chase:

                // 너무 멀면 다시 점프
                if (distance >= jumpRange &&
                    Time.time >= lastJumpTime + jumpCooldown)
                {
                    StartCoroutine(ScreamRoutine());

                    return;
                }

                // 공격 범위
                if (distance <= attackRange + 0.2f)
                {
                    ChangeState(ZombieState.Attack);

                    return;
                }

                agent.isStopped = false;

                agent.SetDestination(player.position);

                anim.SetBool(attackHash, false);

                break;

            // =========================
            // Attack
            // =========================
            case ZombieState.Attack:

                // 멀어지면 추적
                if (distance > attackRange + 0.5f)
                {
                    anim.SetBool(attackHash, false);

                    ChangeState(ZombieState.Chase);

                    return;
                }

                agent.isStopped = true;
                agent.ResetPath();

                anim.SetFloat(speedHash, 0);

                // 플레이어 바라보기
                Vector3 dir =
                    (player.position - transform.position).normalized;

                dir.y = 0;

                if (dir != Vector3.zero)
                {
                    Quaternion rot =
                        Quaternion.LookRotation(dir);

                    transform.rotation =
                        Quaternion.Slerp(
                            transform.rotation,
                            rot,
                            10f * Time.deltaTime);
                }

                anim.SetBool(attackHash, true);

                break;
        }

        UpdateAnimation();
    }

    #endregion

    #region 비명

    private IEnumerator ScreamRoutine()
    {
        isScreaming = true;

        hasScreamed = true;

        agent.isStopped = true;
        agent.ResetPath();

        anim.SetFloat(speedHash, 0);

        // 비명
        anim.CrossFade("Scream", 0.1f);

        yield return null;

        AnimatorStateInfo info =
            anim.GetCurrentAnimatorStateInfo(0);

        // 비명 끝까지 대기
        yield return new WaitForSeconds(info.length);

        // 점프 시작
        StartCoroutine(JumpRoutine());
    }

    #endregion

    #region 점프

    private IEnumerator JumpRoutine()
    {
        isJumping = true;

        lastJumpTime = Time.time;

        // 점프 애니
        anim.CrossFade("Jump", 0.1f);

        yield return new WaitForSeconds(0.2f);

        Vector3 startPos = transform.position;

        // 플레이어 방향
        Vector3 dir =
            (player.position - transform.position).normalized;

        // 플레이어 앞 착지
        Vector3 targetPos =
            player.position - dir * 1.5f;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / jumpDuration;

            Vector3 pos =
                Vector3.Lerp(startPos, targetPos, t);

            // 포물선
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;

        // 착지 애니
        anim.CrossFade("JumpDown", 0.05f);

        yield return new WaitForSeconds(0.5f);

        // NavMesh 위치 동기화
        agent.Warp(transform.position);

        // 이동 재개
        agent.isStopped = false;
        agent.SetDestination(player.position);

        isJumping = false;
        isScreaming = false;

        ChangeState(ZombieState.Chase);
    }

    #endregion

    #region 공격 처리

    protected override void HandleAttack()
    {
        // Animator Transition 사용
    }

    #endregion

    #region 상태 변경

    protected override void ChangeState(ZombieState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case ZombieState.Idle:

                anim.SetBool(attackHash, false);

                break;

            case ZombieState.Chase:

                anim.SetBool(attackHash, false);

                agent.isStopped = false;

                break;

            case ZombieState.Attack:

                agent.isStopped = true;
                agent.ResetPath();

                anim.SetBool(attackHash, true);

                break;
        }
    }

    #endregion
}