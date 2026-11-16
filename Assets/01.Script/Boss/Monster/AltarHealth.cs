
using UnityEngine;

public class AltarHealth : MonoBehaviour
{
    [Header("제단 체력")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDestroyed;

    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"{gameObject.name} 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            DestroyAltar();
        }
    }

    private void DestroyAltar()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        Debug.Log($"{gameObject.name} 파괴!");

        Destroy(gameObject);
    }
}
