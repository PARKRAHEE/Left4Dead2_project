using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBase : MonoBehaviour
{
    #region 인스펙터 (값 설정)
    [Header("Stats")]
    [SerializeField] private float health = 50f;        // 체력
    [SerializeField] private float attackRange = 2f;    // 공격 가능 거리

    [Header("Move")]
    [SerializeField] private float moveSpeed = 3.5f;    // 이동 속도
    #endregion

    #region 내부 변수 (자동 할당)
    protected Transform player;        // 플레이어 위치
    protected NavMeshAgent agent;      // 길찾기 이동
    protected Animator anim;           // 애니메이션
    #endregion

    #region 내부 함수
    // 초기 설정
    // 플레이어 찾기
    // 컴포넌트 가져오기
    // 이동 속도 설정
    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        agent.speed = moveSpeed;

        agent.stoppingDistance = attackRange;
    }

    // 매 프레임 실행
    // 플레이어와 거리 계산
    // 멀면 추적 / 가까우면 공격
    // 애니메이션 업데이트
    protected virtual void Update()
    {
        HandleBehavior();

        UpdateAnimation();
    }

    protected virtual void HandleBehavior()
    {
        if (agent == null)
            return;

        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
            Chase();
        else
            Attack();
    }
    #endregion

    #region 이동 & 공격 기능
    // 플레이어를 향해 이동
    protected virtual void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    // 공격 상태 (기본은 멈춤)
    // → 자식 클래스에서 오버라이드 가능
    protected virtual void Attack()
    {
        agent.isStopped = true;

        agent.velocity = Vector3.zero;
        agent.ResetPath();

        Debug.Log("공격!");
    }
    #endregion

    #region 애니메이션 처리
    // 이동 속도 기반으로 애니메이션 전환
    // Speed 값 → Idle / Run 전환
    protected void UpdateAnimation()
    {
        if (anim != null && agent != null)
        {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
        }
    }
    #endregion

    #region 데미지 & 사망 처리
    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
            Die();
    }

    // 사망 처리
    protected virtual void Die()
    {
        Debug.Log("좀비 사망");
        Destroy(gameObject);
    }
    #endregion
}
