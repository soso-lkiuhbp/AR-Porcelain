using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;

    [Header("Play / Pause")]
    public Button playButton;
    public Button pauseButton;

    [Header("Speed Button")]
    public Button speedButton;
    public Text speedText;

    // 倍速顺序（最后一个代表“恢复原速”）
    private float[] speeds = { 1.5f, 2f, 3f, 0.5f };
    private int speedIndex = -1; // -1 表示“倍速”状态

    private bool isPrepared = false;

    void Start()
    {
        videoPlayer.prepareCompleted += vp =>
        {
            isPrepared = true;
            rawImage.texture = videoPlayer.targetTexture;
        };
        videoPlayer.Prepare();

        playButton.onClick.AddListener(PlayVideo);
        pauseButton.onClick.AddListener(PauseVideo);
        speedButton.onClick.AddListener(CycleSpeed);

        UpdateSpeedUI();
    }

    void PlayVideo()
    {
        if (!isPrepared) return;
        videoPlayer.Play();
    }

    void PauseVideo()
    {
        if (!isPrepared) return;
        videoPlayer.Pause();
    }

    void CycleSpeed()
    {
        if (!isPrepared) return;

        speedIndex++;

        // 超过数组长度，回到“倍速”
        if (speedIndex >= speeds.Length)
        {
            speedIndex = -1;
        }

        ApplySpeed();
    }

    void ApplySpeed()
    {
        if (speedIndex == -1)
        {
            videoPlayer.playbackSpeed = 1f;
            speedText.text = "倍速";
        }
        else
        {
            float speed = speeds[speedIndex];
            videoPlayer.playbackSpeed = speed;
            speedText.text = speed + "x";
        }
    }

    void UpdateSpeedUI()
    {
        ApplySpeed();
    }
}