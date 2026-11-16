
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;

    [Header("씬별 BGM")]
    [SerializeField] private AudioClip mainBGM;
    [SerializeField] private AudioClip golemBGM;
    [SerializeField] private AudioClip mutantBGM;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayBGM(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGM(scene.name);
    }

    private void PlayBGM(string sceneName)
    {
        AudioClip targetClip = null;

        switch (sceneName)
        {
            case "StartScene":
            case "BossSelectScene":
                targetClip = mainBGM;
                break;

            case "GameScene":
                targetClip = golemBGM;
                break;

            case "GameScene 2":
                targetClip = mutantBGM;
                break;
        }

        if (targetClip == null)
            return;

        // 이미 같은 음악이 재생 중이면 유지
        if (bgmSource.clip == targetClip && bgmSource.isPlaying)
            return;

        bgmSource.clip = targetClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    // 전체 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // 현재 전체 볼륨 가져오기
    public float GetMasterVolume()
    {
        return AudioListener.volume;
    }
}
