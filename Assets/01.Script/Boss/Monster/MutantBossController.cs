using UnityEngine;
using System.Collections;

public class MutantBossController : MonoBehaviour
{

    private enum BossState
    {
        Idle,
        Chase,
        Attack
    }

    private BossState currentState = BossState.Idle;
    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    [Header("배리어")]
    [SerializeField] private GameObject barrier;

    [Header("전투 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("이동")]
    [SerializeField] private UnityEngine.AI.NavMeshAgent agent;

    [Header("일반 공격 설정")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float attackHitTime = 0.8f;
    [SerializeField] private float attackAnimationLength = 2f;

    [Header("공격 판정")]
    [SerializeField] private float attackRadius = 2.5f;
    [SerializeField] private float attackForwardOffset = 1.5f;

    private float attackCooldownTimer;
    private float attackTimer;
    private bool hasAppliedDamage;
    private bool isAttacking;

    [Header("콤보 공격 설정")]
    [SerializeField] private float comboCooldown = 15f;
    [SerializeField] private float comboAttackInterval = 0.5f;

    private float comboCooldownTimer;
    private float comboIntervalTimer;
    private int comboAttackCount;
    private bool isComboAttacking;

    [Header("돌진 설정")]
    [SerializeField] private float chargeTriggerDistance = 10f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeEndDistance = 3f;

    private bool isCharging;

    private string[] attackAnimations =
    {
    "attack1",
    "attack1LSpike",
    "attack1RSpike",
    "attack2",
    "attack2LSpike",
    "attack2RSpike",
    "attack3",
    "attack3RSpike",
    "attack4",
    "attack4RSpike",
    "attack5",
    "attack5LSpike"
    };

    private bool isAwake;
    private bool isAwakening;

    private bool isDead;

    private void Start()
    {
        // 시작 시 봉인 상태
        isAwake = false;
        isAwakening = false;

        // 시작 시 공격 쿨다운 
        attackCooldownTimer = attackCooldown;
        comboCooldownTimer = comboCooldown;
    }

    // 제단 4개가 모두 파괴되면 호출
    public void Awaken()
    {
        if (isAwake || isAwakening)
            return;

        isAwakening = true;

        Debug.Log("보스 활동 준비");

        // 배리어 제거
        if (barrier != null)
            barrier.SetActive(false);

        // Rage 애니메이션 재생
        if (animator != null)
            animator.Play("rage", 0, 0f);
    }


    private void Update()
    {

        if (isDead)
            return;

        // 각성 중에는 Rage 애니메이션 종료만 확인
        if (isAwakening)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("rage") &&
                stateInfo.normalizedTime >= 1f)
            {
                isAwakening = false;
                isAwake = true;

                animator.Play("idle1", 0, 0f);

                Debug.Log("보스 활동 개시, 전투 시작");
            }

            return;
        }

        // 각성 전에는 AI 작동 금지
        if (!isAwake || player == null)
            return;

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        BossState previousState = currentState;

        // 거리별 상태 결정
        if (distance > detectionRange)
            currentState = BossState.Idle;
        else if (distance > attackRange)
            currentState = BossState.Chase;
        else
            currentState = BossState.Attack;

        // 상태 변경 로그
        if (previousState != currentState)
        {
            Debug.Log($"보스 상태 변경: {previousState} → {currentState}");

            switch (currentState)
            {
                case BossState.Idle:
                    animator.Play("idle1", 0, 0f);
                    break;

                case BossState.Chase:
                    animator.Play("run1", 0, 0f);
                    break;

                case BossState.Attack:
                    animator.Play("idle1", 0, 0f);
                    break;
            }
        }

        // 상태별 행동 실행
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

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
    }

    private void Chase()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        RotateTowards(direction);

        float distance = Vector3.Distance(transform.position, player.position);

        // 돌진 시작
        if (!isCharging && distance >= chargeTriggerDistance)
        {
            isCharging = true;

            if (animator != null)
                animator.Play("run1", 0, 0f);
            Debug.Log("돌진 패턴 시전");
        }   

        // 돌진 중
        if (isCharging)
        {
            if (distance <= chargeEndDistance)
            {
                isCharging = false;
                Debug.Log("돌진 패턴 종료");
                return;
            }

            transform.position += direction * chargeSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void Attack()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            RotateTowards(direction.normalized);

        // 콤보 진행 중이면 콤보 처리
        if (isComboAttacking)
        {
            UpdateComboAttack();
            return;
        }

        // 일반 공격 진행 중이면 일반 공격 처리
        if (isAttacking)
        {
            UpdateAttack();
            return;
        }

        // 콤보 쿨타임 감소
        if (comboCooldownTimer > 0f)
            comboCooldownTimer -= Time.deltaTime;

        // 콤보 우선 확인
        if (comboCooldownTimer <= 0f)
        {
            StartComboAttack();
            return;
        }

        // 일반 공격
        if (attackCooldownTimer <= 0f)
            StartAttack();
    }


    public bool IsAwake => isAwake;




    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    private void StartAttack()
    {
        if (isAttacking || attackCooldownTimer > 0f)
            return;

        isAttacking = true;
        attackTimer = 0f;
        hasAppliedDamage = false;

        int randomIndex = Random.Range(0, attackAnimations.Length);

        animator.Play(attackAnimations[randomIndex], 0, 0f);

        attackCooldownTimer = attackCooldown;

        Debug.Log($"보스 일반 공격: {attackAnimations[randomIndex]}");
    }

    private void StartComboAttack()
    {
        isComboAttacking = true;

        comboAttackCount = 0;
        comboIntervalTimer = 0f;

        comboCooldownTimer = comboCooldown;

        // 일반 공격 진행 상태 초기화
        isAttacking = false;

        Debug.Log("보스 콤보 공격 시작!");

        PerformComboAttack();
    }

    private void UpdateAttack()
    {
        if (!isAttacking)
            return;

        attackTimer += Time.deltaTime;

        if (!hasAppliedDamage && attackTimer >= attackHitTime)
        {
            PerformAttackDamage();
            hasAppliedDamage = true;
        }

        if (attackTimer >= attackAnimationLength)
        {
            isAttacking = false;
            attackTimer = 0f;
            animator.Play("idle1", 0, 0f);
        }
    }

    private void PerformAttackDamage()
    {
        Vector3 attackCenter = transform.position + transform.forward * attackForwardOffset + Vector3.up;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRadius);

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null)
                continue;

            Vector3 directionToPlayer = playerHealth.transform.position - transform.position;

            directionToPlayer.y = 0f;

            if (Vector3.Angle(transform.forward, directionToPlayer) > 60f)
                continue;

            playerHealth.TakeDamage(attackDamage, 0.5f,transform.position);

            Debug.Log("보스 공격 적중");
            return;
        }
    }

    private void UpdateComboAttack()
    {
        attackTimer += Time.deltaTime;

        // 공격 판정
        if (!hasAppliedDamage && attackTimer >= attackHitTime)
        {
            PerformAttackDamage();
            hasAppliedDamage = true;
        }

        // 현재 공격 애니메이션 종료 대기
        if (attackTimer < attackAnimationLength)
            return;

        comboAttackCount++;

        // 3회 공격 완료
        if (comboAttackCount >= 3)
        {
            isComboAttacking = false;

            attackTimer = 0f;
            animator.Play("idle1", 0, 0f);

            Debug.Log("보스 콤보 공격 종료!");
            return;
        }

        // 다음 공격까지 0.5초 대기
        comboIntervalTimer = comboAttackInterval;
        StartCoroutine(ComboAttackDelay());
    }


    private void PerformComboAttack()
    {
        int randomIndex = Random.Range(0, attackAnimations.Length);

        animator.Play(attackAnimations[randomIndex], 0, 0f);

        attackTimer = 0f;
        hasAppliedDamage = false;

        Debug.Log($"콤보 공격 {comboAttackCount + 1}회: {attackAnimations[randomIndex]}");
    }

    //코루틴
    private IEnumerator ComboAttackDelay()
    {
        yield return new WaitForSeconds(comboIntervalTimer);

        if (!isComboAttacking)
            yield break;

        PerformComboAttack();
    }

}
