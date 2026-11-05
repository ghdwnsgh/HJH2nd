using UnityEngine;

public class BossController : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        Chase,
        Attack
    }

    private enum BossPattern
    {
        None,
        ArmSwing,       // 팔 휘두르기
        DoubleSlam,     // 양손 내려찍기
        JumpSlam,       // 점프 내려찍기
        Charge          // 돌진
    }

    [Header("보스 데이터")]
    [SerializeField] private BossData bossData;

    [Header("대상")]
    [SerializeField] private Transform player;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    private BossState currentState = BossState.Idle;

    [Header("패턴 쿨타임")]
    [SerializeField] private float armSwingCooldown = 10f;
    [SerializeField] private float doubleSlamCooldown = 30f;
    [SerializeField] private float jumpSlamCooldown = 60f;

    private float armSwingTimer;
    private float doubleSlamTimer;
    private float jumpSlamTimer;

    [Header("팔 휘두르기 설정")]
    [SerializeField] private float armSwingDamage = 25f;
    [SerializeField] private float armSwingRadius = 2f;
    [SerializeField] private float armSwingForwardOffset = 1.5f;

    [SerializeField] private float armSwingWaitTime = 1f;
    [SerializeField] private float armSwingHitTime = 1.1f;
    [SerializeField] private float armSwingAnimationLength = 2.2f;

    private float patternTimer;
    private bool hasAppliedDamage;

    [Header("양손 내려찍기 설정")]
    [SerializeField] private float doubleSlamDamage = 50f;
    [SerializeField] private float doubleSlamRadius = 3.5f;
    [SerializeField] private float doubleSlamForwardOffset = 2f;

    [SerializeField] private float doubleSlamWaitTime = 1.5f;
    [SerializeField] private float doubleSlamHitTime = 1.8f;
    [SerializeField] private float doubleSlamAnimationLength = 3f;

    private BossPattern currentPattern = BossPattern.None;


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

        // 게임 시작 시 패턴 쿨타임 적용
        armSwingTimer = armSwingCooldown;
        doubleSlamTimer = doubleSlamCooldown;
        jumpSlamTimer = jumpSlamCooldown;
    }


    private void Update()
    {
        //패턴 쿨타임 감소 호출
        UpdatePatternCooldowns();

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
        // 패턴 실행 중에는 다른 행동을 하지 않음
        if (currentPattern != BossPattern.None)
        {
            UpdatePattern();
            return;
        }

        // 공격 시작 전까지 플레이어를 계속 바라봄
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();
            RotateTowards(direction);
        }

        // 사용 가능한 패턴 선택
        BossPattern selectedPattern = SelectPattern();

        if (selectedPattern == BossPattern.None)
        {
            // 사용 가능한 패턴이 없으면 제자리 대기
            return;
        }

        StartPattern(selectedPattern);
    }

    private void PerformArmSwingDamage()
    {
        Vector3 attackCenter =
            transform.position +
            transform.forward * armSwingForwardOffset;

        Collider[] hits = Physics.OverlapSphere(
            attackCenter,
            armSwingRadius
        );

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth =
                hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(armSwingDamage);

                Debug.Log("보스의 팔 휘두르기 적중!");
                return;
            }
        }
    }

    private void PerformDoubleSlamDamage()
    {
        Vector3 attackCenter =
            transform.position +
            transform.forward * doubleSlamForwardOffset;

        Collider[] hits = Physics.OverlapSphere(
            attackCenter,
            doubleSlamRadius
        );

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth =
                hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(doubleSlamDamage);

                Debug.Log("보스의 양손 내려찍기 적중!");
                return;
            }
        }
    }


    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bossData.rotationSpeed * Time.deltaTime);
    }

   

    // 보스 패턴 쿨타임 감소
    private void UpdatePatternCooldowns()
    {
        if (armSwingTimer > 0f)
            armSwingTimer -= Time.deltaTime;

        if (doubleSlamTimer > 0f)
            doubleSlamTimer -= Time.deltaTime;

        if (jumpSlamTimer > 0f)
            jumpSlamTimer -= Time.deltaTime;
    }

    private BossPattern SelectPattern()
    {
        // 쿨타임이 긴 패턴부터 확인
        if (jumpSlamTimer <= 0f)
            return BossPattern.JumpSlam;

        if (doubleSlamTimer <= 0f)
            return BossPattern.DoubleSlam;

        if (armSwingTimer <= 0f)
            return BossPattern.ArmSwing;

        return BossPattern.None;
    }

    // 패턴 시작
    private void StartPattern(BossPattern pattern)
    {
        currentPattern = pattern;
        patternTimer = 0f;
        hasAppliedDamage = false;

        switch (currentPattern)
        {
            case BossPattern.ArmSwing:

                armSwingTimer = armSwingCooldown;

                if (animator != null)
                {
                    animator.Play("Attack Swing", 0, 0f);
                }

                Debug.Log("보스 패턴 시작: 팔 휘두르기");
                break;

            case BossPattern.DoubleSlam:

                doubleSlamTimer = doubleSlamCooldown;

                if (animator != null)
                {
                    animator.Play("Double Slam", 0, 0f);
                }

                Debug.Log("보스 패턴 시작: 양손 내려찍기");
                break;

            case BossPattern.JumpSlam:
                Debug.Log("점프 내려찍기는 아직 미구현");
                break;

            case BossPattern.Charge:
                Debug.Log("돌진은 아직 미구현");
                break;
        }
    }

    //패턴 진행
    private void UpdatePattern()
    {
        patternTimer += Time.deltaTime;

        switch (currentPattern)
        {
            case BossPattern.ArmSwing:

                // 공격 애니메이션의 지정된 시점에 피해 적용
                if (!hasAppliedDamage &&
                    patternTimer >= armSwingHitTime)
                {
                    PerformArmSwingDamage();
                    hasAppliedDamage = true;
                }

                // 공격 애니메이션 종료
                if (patternTimer >= armSwingAnimationLength)
                {
                    EndPattern();
                }

                break;


            case BossPattern.DoubleSlam:

                // 공격 판정
                if (!hasAppliedDamage &&
                    patternTimer >= doubleSlamHitTime)
                {
                    PerformDoubleSlamDamage();
                    hasAppliedDamage = true;
                }

                // 애니메이션 종료
                if (patternTimer >= doubleSlamAnimationLength)
                {
                    EndPattern();
                }

                break;
        }
    }


    //패턴 종료
    private void EndPattern()
    {
        Debug.Log($"보스 패턴 종료: {currentPattern}");

        currentPattern = BossPattern.None;
        patternTimer = 0f;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.Play("Idle", 0, 0f);
        }
    }


    //공격범위 확인용
    private void OnDrawGizmosSelected()
    {
        // 팔 휘두르기 범위
        Gizmos.color = Color.red;

        Vector3 armSwingCenter =
            transform.position +
            transform.forward * armSwingForwardOffset;

        Gizmos.DrawWireSphere(
            armSwingCenter,
            armSwingRadius
        );

        // 양손 내려찍기 범위
        Gizmos.color = Color.yellow;

        Vector3 doubleSlamCenter =
            transform.position +
            transform.forward * doubleSlamForwardOffset;

        Gizmos.DrawWireSphere(
            doubleSlamCenter,
            doubleSlamRadius
        );
    }


}
