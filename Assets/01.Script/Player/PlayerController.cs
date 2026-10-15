
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotateSpeed = 10f;

    [Header("중력 설정")]
    [SerializeField] private float gravity = -20f;

    [Header("카메라")]
    [SerializeField] private Transform cameraTransform;

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
        Move();
    }

    private void Move()
    {
        // 카메라 기준 방향 계산
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

        // 바닥 판정 및 중력
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 걷기 또는 달리기 속도 선택
        float currentSpeed = isSprint ? sprintSpeed : moveSpeed;

        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
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
}
