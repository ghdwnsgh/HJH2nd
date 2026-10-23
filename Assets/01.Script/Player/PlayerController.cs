
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotateSpeed = 10f;

    [Header("점프 및 중력 설정")]
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = -20f;

    [Header("카메라")]
    [SerializeField] private Transform cameraTransform;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    [Header("공격 설정")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackCooldown = 0.5f;

    private float nextAttackTime = 0f;

    [Header("공격 콤보 설정")]
    [SerializeField] private float comboInputWindow = 0.8f;
    [SerializeField] private float battlePoseDuration = 3f;

    [Header("공격 판정 시간")]
    [SerializeField] private float attackHitDelay = 0.4f;

    private bool damageApplied = false;
    private float attackHitTimer = 0f;


    private int comboStep = 0;
    private float comboTimer = 0f;
    private float battlePoseTimer = 0f;
    private bool isInBattlePose = false;

    private bool isAttackAnimationPlaying = false;
    private float attackAnimationTimer = 0f;
    [SerializeField] private float attackAnimationFallbackTime = 1f;


    [Header("회피 설정")]
    [SerializeField] private float dodgeSpeed = 15f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float invincibleDuration = 0.3f;
    [SerializeField] private float dodgeCooldown = 2f;

    private bool isDodging;
    private float dodgeTimer;
    private float invincibleTimer;
    private float nextDodgeTime;
    private Vector3 dodgeDirection;

    // 다른 스크립트에서 무적 여부를 확인할 때 사용
    public bool IsInvincible => invincibleTimer > 0f;


    private CharacterController controller;

    private Vector2 moveInput;
    private float verticalVelocity;
    private bool isSprint;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (dodgeTimer > 0f)
        {
            dodgeTimer -= Time.deltaTime;

            if (dodgeTimer <= 0f)
            {
                isDodging = false;
            }
        }

        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }

        Move();

        UpdateAttackTimers();
    }

    private void Move()
    {
        // 카메라 기준 이동 방향
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        // 이동 방향으로 회전
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // 바닥에 있을 때 아래 방향 속도 유지
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }


        Vector3 velocity;

        if (isDodging)
        {
            // 회피 중에는 지정된 방향으로 빠르게 이동
            velocity = dodgeDirection * dodgeSpeed;
        }
        else
        {
            float currentSpeed = isSprint ? sprintSpeed : moveSpeed;
            velocity = moveDirection * currentSpeed;
        }

        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // 이동 및 달리기 애니메이션 제어
        if (animator != null)
        {
            float speed = moveInput.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsSprinting", isSprint && speed > 0.1f);
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprint = true;
        }
        else if (context.canceled)
        {
            isSprint = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // 키를 누른 순간에만 점프
        if (!context.performed)
            return;

        // 바닥에 있을 때만 점프
        if (controller.isGrounded)
        {
            verticalVelocity = jumpPower;
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    // 일반공격 : 마우스 왼쪽 클릭
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // 전투 자세 중 공격하면 새로운 콤보 시작
        if (isInBattlePose)
        {
            isInBattlePose = false;
            battlePoseTimer = 0f;
            comboStep = 0;
        }

        // 첫 공격이거나 콤보 입력 시간이 끝났다면 1단부터 시작
        if (comboStep == 0 || comboTimer <= 0f)
        {
            if (Time.time < nextAttackTime) return;

            comboStep = 1;
        }
        else
        {
            // 최대 3단 공격까지만 진행
            if (comboStep >= 3) return;

            comboStep++;
        }

        // 다음 콤보 입력 가능 시간 갱신
        comboTimer = comboInputWindow;

        // 다음 일반 공격 가능 시간 설정
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetInteger("ComboStep", comboStep);
            animator.SetTrigger("Attack");
        }

        // 공격 애니메이션 타이머 초기화
        isAttackAnimationPlaying = true;
        attackAnimationTimer = 0f;

        attackHitTimer = 0f;
        damageApplied = false;

        isInBattlePose = false;
        battlePoseTimer = 0f;

    }


    private void PerformAttackDamage()
    {
        Vector3 attackCenter = transform.position + Vector3.up * 1f + transform.forward * attackRange;

        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRadius);

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null)
                continue;

            enemy.TakeDamage(attackDamage);
            Debug.Log($"{comboStep}단 공격 적중!");
        }
    }



    // 공격 범위 확인용 임시 스크립트 
    private void OnDrawGizmosSelected()
    {
        // 공격 판정 구체의 중심 위치
        Vector3 attackCenter = transform.position + Vector3.up * 1f + transform.forward * attackRange;

        // 공격 범위를 초록색 구체로 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);
    }

    // 대쉬/회피 - 사용 중 무적
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // 대쉬 중이거나 쿨타임 중이면 회피할 수 없음
        if (isDodging || Time.time < nextDodgeTime)
            return;

        // 카메라 기준으로 이동 방향 계산
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 inputDirection = forward * moveInput.y + right * moveInput.x;

        // 이동 입력이 있으면 그 방향으로, 없으면 플레이어가 바라보는 방향으로 대시
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            dodgeDirection = inputDirection.normalized;
        }
        else
        {
            dodgeDirection = transform.forward;
        }

        isDodging = true;
        dodgeTimer = dodgeDuration;
        invincibleTimer = invincibleDuration;
        nextDodgeTime = Time.time + dodgeCooldown;
    }


    // 콤보 유지 관리용 타이머
    private void UpdateAttackTimers()
    {
        // 다음 콤보 입력 가능 시간
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0f && comboStep > 0)
            {
                comboTimer = 0f;
            }
        }


        // 공격 애니메이션 타이머 및 피해 판정
        if (isAttackAnimationPlaying)
        {
            attackAnimationTimer += Time.deltaTime;
            attackHitTimer += Time.deltaTime;

            // 지정된 타격 시점에 한 번만 피해 적용
            if (!damageApplied && attackHitTimer >= attackHitDelay)
            {
                PerformAttackDamage();
                damageApplied = true;
            }

            // 공격 애니메이션 예비 종료 시간
            if (attackAnimationTimer >= attackAnimationFallbackTime)
            {
                isAttackAnimationPlaying = false;
                isInBattlePose = true;
                battlePoseTimer = battlePoseDuration;

                if (animator != null)
                {
                    animator.SetBool("IsAttacking", true);
                }
            }
        }


        // 전투 자세 유지 시간
        if (isInBattlePose)
        {
            battlePoseTimer -= Time.deltaTime;

            if (battlePoseTimer <= 0f)
            {
                isInBattlePose = false;
                comboStep = 0;

                if (animator != null)
                {
                    animator.SetBool("IsAttacking", false);
                }
            }
        }
    }


}
