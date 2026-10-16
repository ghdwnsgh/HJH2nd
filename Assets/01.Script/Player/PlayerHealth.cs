
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

    public void TakeDamage(float damage)
    {
        // 이미 사망했다면 피해를 받지 않음
        if (isDead)
            return;

        // 회피 무적 시간 중에는 피해를 받지 않음
        if (playerController != null &&
            playerController.IsInvincible)
        {
            Debug.Log("회피 성공! 피해를 받지 않았습니다.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("플레이어 사망!");

        // 플레이어 조작 비활성화
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Destroy(gameObject);
    }


}
