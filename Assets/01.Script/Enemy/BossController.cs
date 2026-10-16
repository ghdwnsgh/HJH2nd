
using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        Chase,
        Attack
    }

    [Header("대상")]
    [SerializeField] private Transform player;

    [Header("감지 및 이동")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("공격 설정")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 2f;


    private BossState currentState = BossState.Idle;
    private float nextAttackTime;

    private void Update()
    {
        if (player == null)
            return;

        // 플레이어 사망 시 추적 중지
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead)
        {
            currentState = BossState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position,player.position);

        // 거리와 현재 상황에 따라 상태 전환
        if (distance > detectionRange)
        {
            currentState = BossState.Idle;
        }
        else if (distance > attackRange)
        {
            currentState = BossState.Chase;
        }
        else
        {
            currentState = BossState.Attack;
        }

        // 현재 상태 실행
        switch (currentState)
        {
            case BossState.Idle:
                Idle();
                break;

            case BossState.Chase:
                Chase();
                break;

            case BossState.Attack:
                Attack();
                break;
        }
    }

    private void Idle()
    {
        // 현재는 대기 상태에서 아무 행동도 하지 않음
    }

    private void Chase()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // 플레이어 방향으로 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 플레이어에게 접근
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void Attack()
    {
        // 공격 상태에서는 플레이어를 바라봄
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            // 공격 순간에도 플레이어가 사정거리 안에 있는지 확인
            float distance = Vector3.Distance(transform.position,player.position);

            if (distance <= attackRange)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log("피격!");
                }
            }
        }

    }
}
