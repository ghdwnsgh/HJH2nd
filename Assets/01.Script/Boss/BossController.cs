using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        Chase,
        Attack
    }

    [Header("보스 데이터")]
    [SerializeField] private BossData bossData;

    [Header("대상")]
    [SerializeField] private Transform player;

    private BossState currentState = BossState.Idle;
    private float nextAttackTime;

    private void Start()
    {
        if (bossData == null)
        {
            Debug.LogError("BossData가 연결되지 않았습니다!", this);
            return;
        }

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.InitializeHealth(bossData.maxHealth);
        }
        else
        {
            Debug.LogWarning("EnemyHealth 컴포넌트가 없습니다!", this);
        }
    }


    private void Update()
    {
        if (bossData == null || player == null)
            return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead)
        {
            currentState = BossState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);


        BossState previousState = currentState;

        if (distance > bossData.detectionRange)
            currentState = BossState.Idle;
        else if (distance > bossData.attackRange)
            currentState = BossState.Chase;
        else
            currentState = BossState.Attack;

        if (previousState != currentState)
        {
            Debug.Log($"{bossData.bossName} 상태 변경: " + $"{previousState} → {currentState}");
        }


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
        // 대기 상태
    }

    private void Chase()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        RotateTowards(direction);

        transform.position += direction * bossData.moveSpeed * Time.deltaTime;
    }


    private void Attack()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();
            RotateTowards(direction);
        }

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + bossData.attackCooldown;
        PerformAttack();
    }

    private void PerformAttack()
    {
        float distance = Vector3.Distance(transform.position,player.position);

        if (distance > bossData.attackRange)
            return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(bossData.attackDamage);
            Debug.Log($"{bossData.bossName}의 공격!");
        }
    }


    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bossData.rotationSpeed * Time.deltaTime);
    }
}
    