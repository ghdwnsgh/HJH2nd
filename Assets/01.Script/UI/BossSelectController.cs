
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossSelectController : MonoBehaviour
{
    [Header("보스 정보창")]
    [SerializeField] private GameObject golemInfoPanel;
    [SerializeField] private GameObject mutantInfoPanel;

    [Header("보스 씬")]
    [SerializeField] private string golemSceneName = "GolemBossScene";
    [SerializeField] private string mutantSceneName = "MutantBossScene";

    private void Start()
    {
        golemInfoPanel.SetActive(false);
        mutantInfoPanel.SetActive(false);
    }

    // 골렘 버튼 클릭
    public void ShowGolemInfo()
    {
        golemInfoPanel.SetActive(true);
    }

    // 뮤턴트 버튼 클릭
    public void ShowMutantInfo()
    {
        mutantInfoPanel.SetActive(true);
    }

    // 골렘 정보창 닫기
    public void CloseGolemInfo()
    {
        golemInfoPanel.SetActive(false);
    }

    // 뮤턴트 정보창 닫기
    public void CloseMutantInfo()
    {
        mutantInfoPanel.SetActive(false);
    }

    // 골렘 씬 입장
    public void EnterGolemBoss()
    {
        SceneManager.LoadScene(golemSceneName);
    }

    // 뮤턴트 씬 입장
    public void EnterMutantBoss()
    {
        SceneManager.LoadScene(mutantSceneName);
    }
}
