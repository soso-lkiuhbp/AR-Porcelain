using UnityEngine;

public class MuteMusicInStory : MonoBehaviour
{
    void Start()
    {
        // Ω¯»Î 1_Story ≥°æ∞ ±‘›Õ£“Ù¿÷
        if (MusicManager.Instance != null && MusicManager.Instance.IsPlaying())
        {
            MusicManager.Instance.PauseMusic();
            Debug.Log("1_Story ≥°æ∞£∫“Ù¿÷“—‘›Õ£");
        }
    }

    void OnDestroy()
    {
        // ¿Îø™ 1_Story ≥°æ∞ ±ª÷∏¥“Ù¿÷
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ResumeMusic();
            Debug.Log("1_Story ≥°æ∞£∫“Ù¿÷“—ª÷∏¥");
        }
    }
}