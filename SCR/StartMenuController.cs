using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StartMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public GameObject menuPanel;
    public Button stageSelectButton;
    public Button creditsButton;

    [Header("Fade Settings")]
    public Image fadeOverlay;
    public float fadeDuration = 1.0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip startSound;
    public AudioClip selectSound;

    [Header("Scene Names")]
    public string stageSelectScene = "StageSelect";
    public string creditsScene = "Credits";

    private bool menuActivated = false;
    private bool isTransitioning = false;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private bool prevStartButton = false;
    private bool prevAButton = false;
    private bool prevBButton = false;
    private bool prevUpButton = false;
    private bool prevDownButton = false;

    void Start()
    {
        raycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        startButton.onClick.AddListener(OnStartPressed);
        stageSelectButton.onClick.AddListener(() => OnMenuButtonPressed(stageSelectScene));
        creditsButton.onClick.AddListener(() => OnMenuButtonPressed(creditsScene));

        startButton.gameObject.SetActive(true);
        menuPanel.SetActive(false);

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = new Color(0, 0, 0, 0);
        }
    }

    void Update()
    {

        bool currentStartButton = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Start);
        bool currentAButton = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.A);
        bool currentBButton = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.B);
        bool currentUpButton = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Up);
        bool currentDownButton = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.Down);

        if ((currentStartButton && !prevStartButton) ||
            Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            OnStartPressed();
        }


        if (menuActivated && !isTransitioning)
        {

            if ((currentAButton && !prevAButton) ||
                Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                stageSelectButton.onClick.Invoke();
            }

            else if ((currentBButton && !prevBButton) ||
                     Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                creditsButton.onClick.Invoke();
            }

            else if ((currentUpButton && !prevUpButton) ||
                     Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
            else if ((currentDownButton && !prevDownButton) ||
                     Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
        }

        prevStartButton = currentStartButton;
        prevAButton = currentAButton;
        prevBButton = currentBButton;
        prevUpButton = currentUpButton;
        prevDownButton = currentDownButton;


        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTouch(touch.position);
            }
        }
    }

    private void MoveSelection(int direction)
    {
        if (!menuActivated || isTransitioning) return;
        if (!stageSelectButton.gameObject.activeSelf || !creditsButton.gameObject.activeSelf) return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Button current = null;

        if (selectedObject != null)
        {
            current = selectedObject.GetComponent<Button>();
        }

        if (current == stageSelectButton)
        {
            if (direction > 0)
            {
                creditsButton.Select();
                PlaySound(selectSound);
            }
            else
            {
                stageSelectButton.Select();
                PlaySound(selectSound);
            }
        }
        else if (current == creditsButton)
        {
            if (direction > 0)
            {
                stageSelectButton.Select();
                PlaySound(selectSound);
            }
            else
            {
                creditsButton.Select();
                PlaySound(selectSound);
            }
        }
        else
        {
            stageSelectButton.Select();
            PlaySound(selectSound);
        }
    }

    private void ProcessTouch(Vector2 touchPosition)
    {
        if (isTransitioning) return;

        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = touchPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == startButton.gameObject && startButton.gameObject.activeSelf)
            {
                OnStartPressed();
                return;
            }

            if (menuActivated)
            {
                if (result.gameObject == stageSelectButton.gameObject)
                {
                    OnMenuButtonPressed(stageSelectScene);
                    return;
                }
                else if (result.gameObject == creditsButton.gameObject)
                {
                    OnMenuButtonPressed(creditsScene);
                    return;
                }
            }
        }
    }

    void OnStartPressed()
    {
        if (menuActivated || isTransitioning) return;

        PlaySound(startSound);
        startButton.gameObject.SetActive(false);
        menuPanel.SetActive(true);
        menuActivated = true;

        if (stageSelectButton != null && stageSelectButton.gameObject.activeSelf)
        {
            stageSelectButton.Select();
        }
    }

    void OnMenuButtonPressed(string sceneName)
    {
        if (isTransitioning) return;

        PlaySound(selectSound);
        isTransitioning = true;
        StartCoroutine(FadeAndLoadScene(sceneName)); 
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        menuPanel.SetActive(false);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            if (fadeOverlay != null)
            {
                fadeOverlay.color = new Color(0, 0, 0, alpha);
            }

            yield return null;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.color = Color.black;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}