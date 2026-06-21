using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// ============================================================
// 数据类（保留原结构，供 Inspector 配置）
// ============================================================

[System.Serializable]
public class GlazeLevel
{
    public string correctIngredient;
    public GameObject modelPrefab;
    public string taskName;
}

[System.Serializable]
public class IngredientOption
{
    public string ingredientName;
    public Sprite buttonSprite;
}

// ============================================================
// LabVideoSceneController — 合并 3_LabScene + 4_VideoScene
// 功能：釉料关卡选择 + 视频播放（面板切换，不跳转场景）
// ============================================================

public class LabVideoSceneController : MonoBehaviour
{
    public static LabVideoSceneController Instance;

    // ──────────── 釉料关卡部分 ────────────

    [Header("━━ 关卡配置 ━━")]
    public GlazeLevel[] levels;

    [Header("原料池（按钮图片 + 文字）")]
    public IngredientOption[] allIngredients;

    [Header("关卡 UI 引用")]
    public Transform bottleModelContainer;
    public Button gotoARBtn;
    public Button nextLevelBtn;
    public Button retryBtn;
    public Image glowBorder;
    public Text missionText;
    public Text levelText;
    public Button[] ingredientBtns;
    public AudioSource audioSource;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;

    [Header("初始模型")]
    public GameObject startModelPrefab;

    [Header("模型缩放")]
    [Tooltip("生成模型时的统一缩放值，0 则不修改")]
    public float modelScale = 1f;

    [Header("模型初始旋转")]
    [Tooltip("绕X轴初始旋转，默认0")]
    public float modelRotationX = 0f;
    [Tooltip("绕Y轴初始旋转，默认0")]
    public float modelRotationY = 0f;

    [Header("拖拽旋转设置")]
    [Tooltip("拖拽左右旋转速度（建议 3~10）")]
    public float dragRotationSpeed = 5f;

    // 拖拽累计Y轴旋转（在初始旋转基础上叠加）
    private float dragRotY = 0f;
    private bool isDragging = false;
    private Vector2 lastMousePos;

    private int curLevel = 0;
    private GameObject curModel;

    // ──────────── 视频部分（极简）────────────

    [Header("━━ 视频配置 ━━")]
    [Tooltip("VideoPlayer 组件（Inspector 未赋值时自动按名称查找）")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;

    [Header("视频面板")]
    [Tooltip("视频 UI 面板容器，默认隐藏，点击 openVideoBtn 后显示")]
    public GameObject videoPanel;

    [Header("视频按钮")]
    [Tooltip("场景中始终可见的「观看视频」按钮")]
    public Button openVideoBtn;
    [Tooltip("视频面板内的「退出」按钮")]
    public Button closeVideoBtn;

    private bool isVideoPrepared = false;

    // ──────────── 生命周期 ────────────

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // --- 关卡初始化 ---
        gotoARBtn.gameObject.SetActive(false);
        nextLevelBtn.gameObject.SetActive(false);
        retryBtn.gameObject.SetActive(false);

        retryBtn.onClick.RemoveAllListeners();
        retryBtn.onClick.AddListener(OnRetry);

        nextLevelBtn.onClick.RemoveAllListeners();
        nextLevelBtn.onClick.AddListener(NextLevel);

        gotoARBtn.onClick.RemoveAllListeners();
        gotoARBtn.onClick.AddListener(GoToARScene);

        SpawnStartModel();
        LoadLevel(curLevel);

        // --- 视频初始化 ---
        InitVideo();

        // --- 视频面板默认隐藏 ---
        if (videoPanel != null)
            videoPanel.SetActive(false);

        // --- 入口/关闭按钮 ---
        if (openVideoBtn != null)
        {
            openVideoBtn.onClick.RemoveAllListeners();
            openVideoBtn.onClick.AddListener(ShowVideoPanel);
        }

        if (closeVideoBtn != null)
        {
            closeVideoBtn.onClick.RemoveAllListeners();
            closeVideoBtn.onClick.AddListener(HideVideoPanel);
        }
    }

    void Update()
    {
        HandleDragRotation();
    }

    // ──────────── 拖拽旋转（仅左右，Y轴）────────────

    void HandleDragRotation()
    {
        if (curModel == null) return;

        // 鼠标/触摸按下 → 开始拖拽
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }

        // 鼠标/触摸抬起 → 结束拖拽
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // 拖拽中 → 累计Y轴旋转（左右拖 = Y轴转）
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            dragRotY += delta.x * dragRotationSpeed * 0.1f;
            lastMousePos = Input.mousePosition;
        }

        // 应用总旋转 = 初始旋转 + 拖拽累计旋转（仅Y轴）
        float totalY = modelRotationY + dragRotY;
        curModel.transform.localRotation = Quaternion.Euler(modelRotationX, totalY, 0);
    }

    // ──────────── 视频初始化 ────────────

    void InitVideo()
    {
        // 防御：Inspector 未赋值时自动查找
        if (videoPlayer == null)
        {
            GameObject host = GameObject.Find("VideoPlayerHost");
            if (host != null)
                videoPlayer = host.GetComponent<VideoPlayer>();

            if (videoPlayer == null)
            {
                VideoPlayer[] vps = FindObjectsOfType<VideoPlayer>();
                foreach (var vp in vps)
                {
                    if (vp != null && vp.isActiveAndEnabled)
                    {
                        videoPlayer = vp;
                        break;
                    }
                }
            }
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("[LabVideoSceneController] VideoPlayer not found. Video feature disabled.");
            if (openVideoBtn != null) openVideoBtn.interactable = false;
            return;
        }

        videoPlayer.prepareCompleted += OnVideoPrepareCompleted;

        try
        {
            videoPlayer.Prepare();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[LabVideoSceneController] VideoPlayer.Prepare() failed: " + e.Message);
            videoPlayer = null;
            if (openVideoBtn != null) openVideoBtn.interactable = false;
            return;
        }
    }

    // ──────────── 视频面板切换 ────────────

    void OnVideoPrepareCompleted(VideoPlayer vp)
    {
        isVideoPrepared = true;
        if (rawImage != null && vp.targetTexture != null)
            rawImage.texture = vp.targetTexture;
    }

    /// <summary>
    /// 点击 openVideoBtn → 显示视频面板 + 自动从头播放
    /// </summary>
    public void ShowVideoPanel()
    {
        // 暂停背景音乐
        MusicManager.Instance?.PauseMusic();

        if (videoPanel != null)
            videoPanel.SetActive(true);

        if (isVideoPrepared && videoPlayer != null)
        {
            videoPlayer.time = 0;  // 从头播放
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// 点击 closeVideoBtn → 停止视频 + 隐藏面板
    /// </summary>
    public void HideVideoPanel()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        // 恢复背景音乐
        MusicManager.Instance?.ResumeMusic();

        if (videoPanel != null)
            videoPanel.SetActive(false);
    }

    // ──────────── 关卡逻辑（来自原 GlazeLevelManager）────────────

    void SpawnStartModel()
    {
        if (startModelPrefab == null) return;
        curModel = Instantiate(startModelPrefab, bottleModelContainer);
        curModel.transform.localPosition = Vector3.zero;
        // 重置拖拽累计值
        dragRotY = 0f;
        curModel.transform.localRotation = Quaternion.Euler(modelRotationX, modelRotationY, 0);
        if (modelScale > 0)
            curModel.transform.localScale = Vector3.one * modelScale;
        SetLayerRecursively(curModel, LayerMask.NameToLayer("Bottle"));
    }

    void LoadLevel(int index)
    {
        GlazeLevel lv = levels[index];

        levelText.text = "第" + (index + 1) + "/" + levels.Length + "关";
        missionText.text = "当前任务:[" + lv.taskName + "]请从釉料架中选择正确的原料。";

        gotoARBtn.gameObject.SetActive(false);
        nextLevelBtn.gameObject.SetActive(false);
        retryBtn.gameObject.SetActive(false);

        SetIngredientButtonsInteractable(true);

        // 查找正确答案
        IngredientOption correctOption = null;
        foreach (var opt in allIngredients)
        {
            if (opt.ingredientName == lv.correctIngredient)
            {
                correctOption = opt;
                break;
            }
        }

        // 错误选项池
        List<IngredientOption> pool = new List<IngredientOption>();
        foreach (var opt in allIngredients)
        {
            if (opt != correctOption)
                pool.Add(opt);
        }
        Shuffle(pool);

        List<IngredientOption> selected = new List<IngredientOption>();
        for (int i = 0; i < 3; i++)
            selected.Add(pool[i]);
        selected.Add(correctOption);
        Shuffle(selected);

        // 设置按钮
        for (int i = 0; i < ingredientBtns.Length; i++)
        {
            if (i < 4)
            {
                ingredientBtns[i].gameObject.SetActive(true);

                Text btnText = ingredientBtns[i].GetComponentInChildren<Text>();
                Image btnImage = ingredientBtns[i].GetComponent<Image>();

                btnText.text = selected[i].ingredientName;
                btnImage.sprite = selected[i].buttonSprite;

                ingredientBtns[i].onClick.RemoveAllListeners();
                string tmp = selected[i].ingredientName;
                ingredientBtns[i].onClick.AddListener(() => OnClickIngredient(tmp));
            }
            else
            {
                ingredientBtns[i].gameObject.SetActive(false);
            }
        }
    }

    void OnClickIngredient(string name)
    {
        if (name == levels[curLevel].correctIngredient)
            OnCorrect();
        else
            OnWrong();
    }

    void OnCorrect()
    {
        audioSource.PlayOneShot(correctSfx);

        if (curModel != null)
            Destroy(curModel);

        curModel = Instantiate(levels[curLevel].modelPrefab, bottleModelContainer);
        curModel.transform.localPosition = Vector3.zero;
        // 重置拖拽累计值
        dragRotY = 0f;
        curModel.transform.localRotation = Quaternion.Euler(modelRotationX, modelRotationY, 0);
        if (modelScale > 0)
            curModel.transform.localScale = Vector3.one * modelScale;
        SetLayerRecursively(curModel, LayerMask.NameToLayer("Bottle"));

        SetIngredientButtonsInteractable(false);

        if (curLevel == levels.Length - 1)
        {
            gotoARBtn.gameObject.SetActive(true);
        }
        else
        {
            nextLevelBtn.gameObject.SetActive(true);
        }
    }

    void OnWrong()
    {
        audioSource.PlayOneShot(wrongSfx);
        SetIngredientButtonsInteractable(false);
        retryBtn.gameObject.SetActive(true);
        StartCoroutine(ShakeAndGlow());
    }

    public void OnRetry()
    {
        retryBtn.gameObject.SetActive(false);
        SetIngredientButtonsInteractable(true);
    }

    IEnumerator ShakeAndGlow()
    {
        if (glowBorder == null)
        {
            Debug.LogError("glowBorder 未赋值", this);
            yield break;
        }

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null && transform.parent != null)
            rt = transform.parent.GetComponent<RectTransform>();

        if (rt == null)
            yield break;

        Vector3 origin = rt.localPosition;
        glowBorder.gameObject.SetActive(true);
        glowBorder.color = new Color(1, 0, 0, 0.6f);

        for (int i = 0; i < 6; i++)
        {
            rt.localPosition = origin + new Vector3(
                Random.Range(-8, 8),
                Random.Range(-8, 8),
                0
            );
            yield return new WaitForSeconds(0.03f);
        }

        rt.localPosition = origin;
        glowBorder.color = new Color(1, 0, 0, 0);
    }

    void SetIngredientButtonsInteractable(bool interactable)
    {
        foreach (Button btn in ingredientBtns)
        {
            if (btn != null)
                btn.interactable = interactable;
        }
    }

    public void NextLevel()
    {
        // 销毁当前模型，重新生成素胚
        if (curModel != null)
            Destroy(curModel);
        SpawnStartModel();

        curLevel++;
        if (curLevel < levels.Length)
            LoadLevel(curLevel);
        else
            Debug.Log("所有关卡完成");
    }

    public void GoToARScene()
    {
        SceneManager.LoadScene("5_ARScene");
    }

    /// <summary>
    /// 递归设置对象及其所有子对象的 Layer
    /// </summary>
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void Shuffle(List<IngredientOption> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            IngredientOption temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public GameObject GetCurrentModelPrefab()
    {
        return levels[curLevel].modelPrefab;
    }
}
