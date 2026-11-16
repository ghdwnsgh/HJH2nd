
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MutantBossHPBarUI : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField] private BossHealth bossHealth;

    [Header("체력바 UI")]
    [SerializeField] private Image hpFill;

    [Header("보스 이름")]
    [SerializeField] private TMP_Text bossNameText;

    private void Start()
    {
        if (bossHealth == null)
        {
            Debug.LogError("MutantBossHPBarUI: 보스 체력이 연결되지 않았습니다!");
            return;
        }

        if (hpFill == null)
        {
            Debug.LogError("MutantBossHPBarUI: 체력바 이미지가 연결되지 않았습니다!");
            return;
        }

        if (bossNameText != null)
        {
            bossNameText.text = "MUTANT";
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
