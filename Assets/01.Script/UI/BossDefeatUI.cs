using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDefeatUI : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField] private EnemyHealth bossHealth;

    [Header("처치 UI")]
    [SerializeField] private GameObject bossClearPanel;

    [Header("시작 씬")]
    [SerializeField] private string startSceneName = "StartScene";

    private bool isClear;

    private void Start()
    {
        if (bossClearPanel != null)
        {
            bossClearPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isClear || bossHealth == null)
            return;

        if (bossHealth.CurrentHealth <= 0f)
        {
            ShowClearPanel();
        }
    }

    private void ShowClearPanel()
    {
        isClear = true;

        if (bossClearPanel != null)
        {
            bossClearPanel.SetActive(true);
        }

        // 게임 일시 정지
        Time.timeScale = 0f;

        // 마우스 잠금 해제 및 커서 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("보스 처치 UI 표시!");
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(startSceneName);
    }

}
