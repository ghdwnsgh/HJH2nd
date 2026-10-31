
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BossSelectController : MonoBehaviour
{
    [Header("보스 정보창")]
    [SerializeField] private GameObject bossInfoPanel;

    [Header("보스 정보 텍스트")]
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text bossDescriptionText;

    [Header("보스 씬")]
    [SerializeField] private string bossSceneName = "BossScene";

    private void Start()
    {
        // 시작할 때 정보창 숨기기
        bossInfoPanel.SetActive(false);
    }

    // 골렘 버튼 클릭
    public void ShowGolemInfo()
    {
        bossNameText.text = "Boss Name";

        bossDescriptionText.text = "Boss Info";

        bossInfoPanel.SetActive(true);
    }

    // 정보창 닫기
    public void CloseBossInfo()
    {
        bossInfoPanel.SetActive(false);
    }

    // 보스 입장
    public void EnterBoss()
    {
        SceneManager.LoadScene(bossSceneName);
    }
}
