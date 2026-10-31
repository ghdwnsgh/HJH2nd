using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    [Header("BossSelectScene")]
    [SerializeField] private string gameSceneName = "BossSelectScene";

    // START 버튼
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // EXIT 버튼
    public void ExitGame()
    {
        Debug.Log("게임 종료");

        Application.Quit();
    }
}
