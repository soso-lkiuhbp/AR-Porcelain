using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public GameObject musicManagerPrefab; // 在 Inspector 中拖入你做的预制体

    void Awake()
    {
        if (MusicManager.Instance == null)
        {
            Instantiate(musicManagerPrefab);
        }
    }
}