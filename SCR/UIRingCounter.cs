using UnityEngine;
using UnityEngine.UI;

public class UIRingCounter : MonoBehaviour
{
    public static UIRingCounter Instance;

    public Text ringText;
    public Image ringIcon;
    public Sprite[] ringSprites;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateRingCount(int count)
    {
        if (ringText != null)
        {
            ringText.text = count.ToString();

            if (ringIcon != null && ringSprites.Length > 1)
            {
                ringIcon.sprite = count > 50 ? ringSprites[1] : ringSprites[0];
            }
        }
    }
}