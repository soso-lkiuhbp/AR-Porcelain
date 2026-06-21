using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("按钮")]
    public Button btnStart;
    public Button btnIntro;
    public Button btnExit;

    [Header("音乐控制")]
    public Button btnBGM;
    public Image bgmIcon;
    public Sprite playIcon;
    public Sprite muteIcon;

    [Header("点击音效")]
    public AudioSource clickSource;
    public AudioClip clickSound;

    [Header("弹窗")]
    public GameObject introPanel;

    void Start()
    {
        // 找出所有 AudioSource 并打印状态
        AudioSource[] allSources = FindObjectsOfType<AudioSource>(true);
        Debug.Log($"=== 场景中共有 {allSources.Length} 个 AudioSource ===");

        foreach (AudioSource source in allSources)
        {
            Debug.Log($"物体: {source.gameObject.name} | 正在播放: {source.isPlaying} | Clip: {(source.clip != null ? source.clip.name : "无")} | 循环: {source.loop}");

            // 如果是正在播放的且不是 MusicManager，尝试停止
            if (source.isPlaying && source.gameObject != MusicManager.Instance?.gameObject)
            {
                Debug.LogWarning($"停止物体 {source.gameObject.name} 的播放");
                source.Stop();
            }
        }

        // 绑定按钮事件
        btnStart.onClick.AddListener(OnStartClick);
        btnIntro.onClick.AddListener(OnIntroClick);
        btnExit.onClick.AddListener(OnExitClick);

        if (btnBGM != null)
            btnBGM.onClick.AddListener(OnBGMClick);

        // 强制播放音乐
        if (MusicManager.Instance != null)
        {
            AudioSource source = MusicManager.Instance.GetComponent<AudioSource>();

            // 检查和设置
            if (source == null)
            {
                Debug.LogError("MusicManager 没有 AudioSource 组件！");
            }
            else if (source.clip == null)
            {
                Debug.LogError("MusicManager 的 AudioSource 没有设置 AudioClip！请在 Inspector 中拖入背景音乐");
            }
            else
            {
                Debug.Log("开始播放音乐: " + source.clip.name);
                source.Play();
                Debug.Log("播放后 isPlaying: " + source.isPlaying);
            }

            // 同步图标
            bgmIcon.sprite = (source != null && source.isPlaying) ? playIcon : muteIcon;
        }
        else
        {
            Debug.LogError("MusicManager.Instance 不存在！请确保场景中有 MusicManager 物体");
        }

        if (introPanel != null)
            introPanel.SetActive(false);
    }

    void OnStartClick()
    {
        PlayClickSound();
        Invoke("LoadGameScene", 0.1f);
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene("1_Story");
    }

    void OnIntroClick()
    {
        PlayClickSound();
        if (introPanel != null)
            introPanel.SetActive(true);
    }

    void OnExitClick()
    {
        PlayClickSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnBGMClick()
    {
        PlayClickSound();

        if (MusicManager.Instance == null) return;

        if (MusicManager.Instance.IsPlaying())
        {
            MusicManager.Instance.PauseMusic();
            bgmIcon.sprite = muteIcon;
        }
        else
        {
            MusicManager.Instance.ResumeMusic();
            bgmIcon.sprite = playIcon;
        }
    }

    void PlayClickSound()
    {
        if (clickSource != null && clickSound != null)
        {
            clickSource.PlayOneShot(clickSound, 0.7f);
        }
    }

    public void CloseIntroPanel()
    {
        PlayClickSound();
        if (introPanel != null)
            introPanel.SetActive(false);
    }
}