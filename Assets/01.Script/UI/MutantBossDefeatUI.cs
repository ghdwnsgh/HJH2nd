
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MutantBossDefeatUI : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField] private BossHealth bossHealth;

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
            isClear = true;
            StartCoroutine(ShowClearPanel());
        }
    }

    private IEnumerator ShowClearPanel()
    {
        // 3초 대기
        yield return new WaitForSeconds(3f);

        if (bossClearPanel != null)
        {
            bossClearPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("보스 처치 UI 표시");
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(startSceneName);
    }

    public void ShowPlayerDeathPanel()
    {
        if (isClear)
            return;

        isClear = true;

        if (bossClearPanel != null)
            bossClearPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("플레이어 사망 UI 표시!");
    }


}
