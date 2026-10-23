using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    [Header("GameScene")]
    [SerializeField] private string GameScene = "GameScene";

    // START 버튼
    public void StartGame()
    {
        SceneManager.LoadScene(GameScene);
    }

    // EXIT 버튼
    public void ExitGame()
    {
        Debug.Log("게임 종료");

        Application.Quit();
    }
}
