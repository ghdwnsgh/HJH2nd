using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f;

    [Header("사망 애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathAnimationLength = 3f;

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
        if (isDead)
            return;

        if (playerController != null && playerController.IsInvincible)
        {
            Debug.Log("회피 성공");
            return;
        }

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
            playerController.enabled = false;

        // 사망 애니메이션 재생
        if (animator != null)
            animator.Play("Death", 0, 0f);

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathAnimationLength);

        // 플레이어 사망 후 게임오버 UI 표시
        MutantBossDefeatUI defeatUI =
            FindFirstObjectByType<MutantBossDefeatUI>();

        if (defeatUI != null)
            defeatUI.ShowPlayerDeathPanel();
    }
}
