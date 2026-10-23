using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBarUI : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField] private EnemyHealth bossHealth;

    [Header("체력바 UI")]
    [SerializeField] private Image hpFill;

    [Header("보스 이름")]
    [SerializeField] private TMP_Text bossNameText;

    private void Start()
    {
        if (bossHealth == null)
        {
            Debug.LogError("BossHPBarUI: 보스 체력이 연결되지 않았습니다!");
            return;
        }

        if (hpFill == null)
        {
            Debug.LogError("BossHPBarUI: 체력바 이미지가 연결되지 않았습니다!");
            return;
        }

        if (bossNameText != null)
        {
            bossNameText.text = "ROCK GOLEM";
        }

        UpdateHPBar();
    }

    private void Update()
    {
        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (bossHealth == null || hpFill == null)
            return;

        float hpRatio =
            bossHealth.CurrentHealth / bossHealth.MaxHealth;

        hpFill.fillAmount = Mathf.Clamp01(hpRatio);
    }
}
