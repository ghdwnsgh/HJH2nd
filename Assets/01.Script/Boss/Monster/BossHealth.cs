using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField] private float maxHealth = 1000f;

    [Header("보스 상태")]
    [SerializeField] private MutantBossController bossController;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    private float currentHealth;
    private bool isDead;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        // 보스가 각성하기 전에는 피해를 받지 않음
        if (bossController == null || !bossController.IsAwake)
            return;

        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"보스 체력: {currentHealth} / {maxHealth}");

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

        Debug.Log("보스 처치!");

        if (bossController != null)
            bossController.Die();

        if (animator != null)
            animator.Play("death1", 0, 0f);
    }
}
