using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(
            $"{gameObject.name} 피해: {damage}, " +
            $"남은 체력: {currentHealth}"
        );

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} 처치!");
            gameObject.SetActive(false);
        }
    }
}
