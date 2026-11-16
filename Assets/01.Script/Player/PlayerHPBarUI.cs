using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBarUI : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("체력 UI")]
    [SerializeField] private Image hpFill;

    private void Update()
    {
        if (playerHealth == null || hpFill == null)
            return;

        float hpRatio =
            playerHealth.CurrentHealth / playerHealth.MaxHealth;

        hpFill.fillAmount = hpRatio;
    }
}