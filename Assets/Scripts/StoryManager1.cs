using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StoryManager1 : MonoBehaviour
{
    [Header("UI")]
    public Text storyText;             // 普通 UI Text（非 TMP）

    [Header("剧情")]
    public string[] sentences;         // 每一句台词

    [Header("参数")]
    public float typingSpeed = 0.05f;  // 打字速度（秒/字符）
    public float delayBetweenLines = 1.5f; // 每行间隔

    private int currentLine = 0;
    private bool isTyping = false;

    void Start()
    {
        StartCoroutine(PlayStory());
    }

    IEnumerator PlayStory()
    {
        while (currentLine < sentences.Length)
        {
            yield return StartCoroutine(TypeSentence(sentences[currentLine]));
            yield return new WaitForSeconds(delayBetweenLines);
            currentLine++;
        }

        // 剧情结束后跳转回开始场景
        yield return new WaitForSeconds(0.5f);
        GoToStartScene();
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        storyText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void GoToStartScene()
    {
        SceneManager.LoadScene("StartScenes");
    }
}