using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // 외부에서 최대 체력을 설정할 때 사용
    public void InitializeHealth(float health)
    {
        maxHealth = Mathf.Max(health, 1f);
        currentHealth = maxHealth;

        Debug.Log($"{gameObject.name} 체력 초기화: {currentHealth}");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"{gameObject.name} 피해: {damage}, " + $"남은 체력: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} 처치!");
            gameObject.SetActive(false);
        }
    }
}
