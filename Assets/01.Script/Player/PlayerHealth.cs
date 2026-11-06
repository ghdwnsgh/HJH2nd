
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead;
    private PlayerController playerController;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(float damage, float stunDuration, Vector3 attackerPosition)
    {
        // 이미 사망했다면 피해를 받지 않음
        if (isDead)
            return;

        // 회피 무적 시간 중에는 피해와 피격 모션 무시
        if (playerController != null && playerController.IsInvincible)
        {
            Debug.Log("회피 성공");
            return;
        }

        // 패링 성공 시 피해 무효화
        if (playerController != null && playerController.TryParry())
        {
            Debug.Log("패링 성공");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        // 체력이 남아 있다면 피격 처리
        if (playerController != null)
        {
            playerController.ApplyHit(stunDuration, attackerPosition);
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("플레이어 사망");

        // 플레이어 조작 비활성화
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Destroy(gameObject);
    }


}
