
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 설정")]
    [SerializeField] private float targetHeight = 1.5f;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = 15f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // ESC를 누르면 마우스 잠금 전환
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool isLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = isLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isLocked;
        }

        // 마우스 입력으로 카메라 회전
        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 카메라 피벗 회전
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 플레이어 위치 추적
        transform.position = target.position + Vector3.up * targetHeight;
    }
}
