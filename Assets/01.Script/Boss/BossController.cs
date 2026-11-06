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

    [Header("점프 내려찍기 설정")]
    [SerializeField] private float jumpSlamRadius = 4.5f;
    [SerializeField] private float jumpSlamWaitTime = 0.7f;
    [SerializeField] private float jumpSlamMoveTime = 1.8f;
    [SerializeField] private float jumpSlamHeight = 5f;
    [SerializeField] private float jumpSlamLandingWaitTime = 1.2f;

    private Vector3 jumpSlamStartPosition;
    private Vector3 jumpSlamTargetPosition;


    [Header("돌진 설정")]
    [SerializeField] private float chargeTriggerDistance = 12f;
    [SerializeField] private float chargeTriggerTime = 10f;
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 1.5f;
    [SerializeField] private float chargeDamage = 35f;
    [SerializeField] private float chargeHitRadius = 2f;

    private float chargeDistanceTimer;

    private Vector3 chargeDirection;
    private Vector3 chargeTargetPosition;
    private float chargeStartDistance;


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

        // 패턴 실행 중에는 거리와 관계없이 패턴을 계속 진행
        if (currentPattern != BossPattern.None)
        {
            UpdatePattern();
            return;
        }


        float distance = Vector3.Distance(transform.position, player.position);


        // 돌진 발동 조건 확인
        if (currentPattern == BossPattern.None)
        {
            if (distance >= chargeTriggerDistance)
            {
                chargeDistanceTimer += Time.deltaTime;

                if (chargeDistanceTimer >= chargeTriggerTime)
                {
                    StartPattern(BossPattern.Charge);
                    UpdatePattern();
                    return;
                }
            }
            else
            {
                // 거리가 가까워지면 측정 초기화
                chargeDistanceTimer = 0f;
            }
        }


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
            animator.SetFloat("Speed", currentState == BossState.Chase ? 1f : 0f);
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
        Vector3 attackCenter = transform.position + transform.forward * armSwingForwardOffset;
        Collider[] hits = Physics.OverlapSphere(attackCenter, armSwingRadius);

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(armSwingDamage, 0.5f, transform.position);
                Debug.Log("팔 휘두르기 적중");
                return;
            }
        }
    }

    private void PerformDoubleSlamDamage()
    {
        Vector3 attackCenter = transform.position + transform.forward * doubleSlamForwardOffset;
        Collider[] hits = Physics.OverlapSphere(attackCenter, doubleSlamRadius);

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth =
                hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(doubleSlamDamage, 0.8f, transform.position);
                Debug.Log("양손 내려찍기 적중");
                return;
            }
        }
    }

    private void PerformJumpSlamDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, jumpSlamRadius);
        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 현재 체력 전체만큼 피해를 줘서 즉사 처리
                playerHealth.TakeDamage(playerHealth.CurrentHealth, 0f,transform.position);

                Debug.Log("도약 내려찍기 적중, 플레이어 즉사!");
                return;
            }
        }

        Debug.Log("도약 내려찍기: 공격 범위 내 플레이어 없음");
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

                jumpSlamTimer = jumpSlamCooldown;

                // 점프 시작 위치 저장
                jumpSlamStartPosition = transform.position;

                // 플레이어의 현재 위치를 착지 지점으로 고정
                jumpSlamTargetPosition = player.position;
                jumpSlamTargetPosition.y = jumpSlamStartPosition.y;

                if (animator != null)
                {
                    animator.Play("Jump Slam", 0, 0f);
                }

                Debug.Log("보스 패턴 시작: 점프 내려찍기");
                break;

            case BossPattern.Charge:

                // 플레이어의 현재 위치 저장
                chargeTargetPosition = player.position;

                // 돌진 시작 방향 고정
                chargeDirection = chargeTargetPosition - transform.position;
                chargeDirection.y = 0f;
                chargeDirection.Normalize();

                // 돌진 방향으로 보스 회전
                if (chargeDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(chargeDirection);
                }

                patternTimer = 0f;
                chargeDistanceTimer = 0f;
                hasAppliedDamage = false;

                if (animator != null)
                {
                    animator.Play("Charge", 0, 0f);
                }

                Debug.Log("보스 패턴 시작: 돌진");
                break;
        }
    }

    //패턴 진행
    private void UpdatePattern()
    {
        if (currentPattern == BossPattern.None)
            return;

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

            case BossPattern.JumpSlam:

                // 0.7초 동안 점프 준비
                if (patternTimer < jumpSlamWaitTime)
                    break;

                // 점프 이동 진행 시간
                float jumpProgress =
                    (patternTimer - jumpSlamWaitTime) / jumpSlamMoveTime;

                jumpProgress = Mathf.Clamp01(jumpProgress);

                // 시작 위치에서 목표 위치까지 이동
                Vector3 jumpPosition = Vector3.Lerp(jumpSlamStartPosition, jumpSlamTargetPosition, jumpProgress);

                // 포물선 형태의 높이 추가
                jumpPosition.y += Mathf.Sin(jumpProgress * Mathf.PI) * jumpSlamHeight;

                transform.position = jumpPosition;

                // 착지 순간
                if (jumpProgress >= 1f)
                {
                    transform.position = jumpSlamTargetPosition;

                    if (!hasAppliedDamage)
                    {
                        PerformJumpSlamDamage();
                        hasAppliedDamage = true;
                    }
                }

                // 애니메이션 및 착지 대기 종료
                if (patternTimer >= jumpSlamWaitTime + jumpSlamMoveTime + jumpSlamLandingWaitTime)
                {
                    EndPattern();
                }

                break;

            case BossPattern.Charge:

                // 직선 돌진
                transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

                // 돌진 중 충돌 검사
                if (!hasAppliedDamage)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position, chargeHitRadius);

                    foreach (Collider hit in hits)
                    {
                        PlayerHealth playerHealth =
                            hit.GetComponentInParent<PlayerHealth>();

                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage(chargeDamage, 0.7f, transform.position);
                            hasAppliedDamage = true;
                            Debug.Log("돌진 적중");
                            break;
                        }
                    }
                }

                // 돌진 시간 종료
                if (patternTimer >= chargeDuration)
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
        hasAppliedDamage = false;

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


        // 점프 내려찍기 착지 범위
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            jumpSlamTargetPosition,
            jumpSlamRadius
        );
    }




}
