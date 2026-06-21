using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class StoryManager : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI storyText;  // 剧情文字

    [Header("剧情文案")]
    public string[] sentences;          // 每一句台词

    [Header("参数设置")]
    public float typingSpeed = 0.05f;   // 打字速度（秒/字）
    public float delayBetweenLines = 1.5f; // 每句话之间的延迟

    [Header("跳转设置")]
    public string nextSceneName = "2_Puzzle";  // 下一场景名称

    private int currentLine = 0;
    private bool isTyping = false;

    void Start()
    {
        // 开始播放第一句
        StartCoroutine(PlayStory());
    }

    IEnumerator PlayStory()
    {
        // 循环播放每一句
        while (currentLine < sentences.Length)
        {
            // 逐字显示当前句子
            yield return StartCoroutine(TypeSentence(sentences[currentLine]));

            // 等待一段时间再显示下一句
            yield return new WaitForSeconds(delayBetweenLines);

            currentLine++;
        }

        // 所有句子播放完毕，跳转场景
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        storyText.text = "";

        // 逐字添加字符
        foreach (char letter in sentence.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}