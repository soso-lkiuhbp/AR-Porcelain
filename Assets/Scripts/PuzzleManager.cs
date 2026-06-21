using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    [Header("场景设置")]
    public string nextSceneName = "3_Laboratory";

    [Header("拼图设置")]
    public Transform pieceGrid;
    public Transform dropGrid;
    public GameObject piecePrefab;

    [Header("拼图原图")]
    public Sprite[] pieceSprites;

    [Header("音效")]
    public AudioSource sfxSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip completeSound;
    public AudioClip buttonClickSound;  // 新增：按钮点击音效

    [Header("通关按钮")]
    public Button nextSceneButton;          // 前往下一个场景的按钮

    private int correctCount = 0;
    private int totalPieces = 9;
    private Dictionary<int, int> correctMapping = new Dictionary<int, int>();
    private List<GameObject> pieces = new List<GameObject>();
    private Vector2[] fixedPositions;
    private bool isPuzzleComplete = false;

    void Start()
    {
        SetupCorrectMapping();
        CalculateFixedPositions();
        CreatePieces();

        // 初始隐藏按钮
        if (nextSceneButton != null)
        {
            nextSceneButton.gameObject.SetActive(false);
            nextSceneButton.onClick.AddListener(LoadNextScene);
        }
    }

    void SetupCorrectMapping()
    {
        for (int i = 0; i < totalPieces; i++)
        {
            correctMapping[i] = i;
        }
    }

    void CalculateFixedPositions()
    {
        fixedPositions = new Vector2[totalPieces];

        float startX = -400f;
        float startY = 100f;
        float spacing = 280f;

        for (int i = 0; i < totalPieces; i++)
        {
            int row = i / 3;
            int col = i % 3;

            float x = startX + col * spacing;
            float y = startY - row * spacing;

            fixedPositions[i] = new Vector2(x, y);
        }
    }

    void CreatePieces()
    {
        List<int> randomOrder = new List<int>();
        for (int i = 0; i < totalPieces; i++)
        {
            randomOrder.Add(i);
        }

        for (int i = 0; i < randomOrder.Count; i++)
        {
            int temp = randomOrder[i];
            int randomIndex = Random.Range(i, randomOrder.Count);
            randomOrder[i] = randomOrder[randomIndex];
            randomOrder[randomIndex] = temp;
        }

        string debugStr = "碎片顺序: ";
        foreach (int id in randomOrder)
        {
            debugStr += id + " ";
        }
        Debug.Log(debugStr);

        for (int i = 0; i < totalPieces; i++)
        {
            int pieceRealId = randomOrder[i];

            GameObject piece = Instantiate(piecePrefab, pieceGrid);
            piece.name = "Piece_" + pieceRealId.ToString("00");

            RectTransform rt = piece.GetComponent<RectTransform>();
            rt.anchoredPosition = fixedPositions[i];
            rt.sizeDelta = new Vector2(267, 267);

            PuzzlePiece pieceScript = piece.GetComponent<PuzzlePiece>();
            pieceScript.Init(pieceRealId, this);

            Image img = piece.GetComponent<Image>();
            if (pieceSprites != null && pieceSprites.Length > pieceRealId && pieceSprites[pieceRealId] != null)
            {
                img.sprite = pieceSprites[pieceRealId];
                img.preserveAspect = false;
            }

            pieces.Add(piece);
        }
    }

    public void ReturnPieceToOriginalPosition(int pieceId)
    {
        GameObject targetPiece = null;
        foreach (GameObject piece in pieces)
        {
            if (piece != null && piece.name == "Piece_" + pieceId.ToString("00"))
            {
                targetPiece = piece;
                break;
            }
        }

        if (targetPiece != null)
        {
            targetPiece.transform.SetParent(pieceGrid);

            int index = pieces.IndexOf(targetPiece);
            if (index >= 0 && index < fixedPositions.Length)
            {
                RectTransform rt = targetPiece.GetComponent<RectTransform>();
                rt.anchoredPosition = fixedPositions[index];
            }

            CanvasGroup cg = targetPiece.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
        }
    }

    public bool TryPlacePiece(int pieceId, int slotId)
    {
        if (isPuzzleComplete) return false;

        if (correctMapping[pieceId] == slotId)
        {
            correctCount++;
            PlaySound(correctSound);

            if (correctCount == totalPieces && !isPuzzleComplete)
            {
                OnPuzzleComplete();
            }
            return true;
        }
        else
        {
            PlaySound(wrongSound);
            return false;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void OnPuzzleComplete()
    {
        isPuzzleComplete = true;
        PlaySound(completeSound);

        // 显示按钮
        if (nextSceneButton != null)
        {
            nextSceneButton.gameObject.SetActive(true);
            Debug.Log("拼图完成！按钮已显示");
        }
        else
        {
            Debug.LogWarning("nextSceneButton 未赋值，无法显示按钮");
        }
    }

    void LoadNextScene()
    {
        // 播放按钮点击音效（使用新增的 buttonClickSound）
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
        SceneManager.LoadScene(nextSceneName);
    }
}