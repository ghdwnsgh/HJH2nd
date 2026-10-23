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

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    [Header("공격 판정")]
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float attackForwardOffset = 1.5f;

    private BossState currentState = BossState.Idle;
    private float nextAttackTime;


    [Header("공격 애니메이션")]
    [SerializeField] private float attackWaitTime = 1f;
    [SerializeField] private float attackHitTime = 1.1f;
    [SerializeField] private float attackAnimationLength = 2.2f;

    private float attackWaitTimer;
    private float attackAnimationTimer;
    private bool isAttackAnimating;
    private bool hasAppliedDamage;


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

        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                currentState == BossState.Chase ? 1f : 0f
            );
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

        // 공격 애니메이션이 진행 중인 경우
        if (isAttackAnimating)
        {
            attackAnimationTimer += Time.deltaTime;

            // 애니메이션 시작 후 지정된 시점에 한 번만 피해 판정
            if (!hasAppliedDamage &&
                attackAnimationTimer >= attackHitTime)
            {
                PerformAttack();
                hasAppliedDamage = true;
            }

            // 애니메이션 종료
            if (attackAnimationTimer >= attackAnimationLength)
            {
                isAttackAnimating = false;
                nextAttackTime = Time.time + bossData.attackCooldown;

                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                    animator.Play("Idle", 0, 0f);
                }
            }

            return;
        }

        // 다음 공격 가능 시간이 아니라면 대기
        if (Time.time < nextAttackTime)
            return;

        // 공격 전 2초 대기
        attackWaitTimer += Time.deltaTime;

        if (attackWaitTimer >= attackWaitTime)
        {
            attackWaitTimer = 0f;
            attackAnimationTimer = 0f;
            hasAppliedDamage = false;
            isAttackAnimating = true;

            // Attack Swing 재생
            if (animator != null)
            {
                animator.Play("Attack Swing", 0, 0f);
            }
        }
    }



    private void PerformAttack()
    {
        Vector3 attackCenter = transform.position + transform.forward * attackForwardOffset;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRadius);

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(bossData.attackDamage);
                Debug.Log($"{bossData.bossName}의 공격!");
                return;
            }
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bossData.rotationSpeed * Time.deltaTime);
    }

    // 공격범위 확인용, 테스트 끝나면 지울것
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackCenter = transform.position + transform.forward * attackForwardOffset;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);
    }

}
