using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("환경설정 패널")]
    [SerializeField] private GameObject settingPanel;

    [Header("전체 음량")]
    [SerializeField] private Slider volumeSlider;

    [Header("게임 일시정지 설정")]
    [SerializeField] private bool pauseGameWhenOpened = false;
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        settingPanel.SetActive(false);

        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = volume;
        AudioListener.volume = volume;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        // 게임 씬에서만 ESC 작동
        if (!pauseGameWhenOpened)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel.activeSelf)
                CloseSettings();
            else
                OpenSettings();
        }
    }

    public void OpenSettings()
    {
        settingPanel.SetActive(true);

        if (pauseGameWhenOpened)
        {
            Time.timeScale = 0f;

            if (playerController != null)
                playerController.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSettings()
    {
        settingPanel.SetActive(false);

        if (pauseGameWhenOpened)
        {
            Time.timeScale = 1f;

            if (playerController != null)
                playerController.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ChangeVolume(float volume)
    {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
