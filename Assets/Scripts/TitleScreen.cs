using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Attach to a Canvas GameObject in Title scene
/// Requires: Canvas > Panel > Title Text + Start Button (set up in Inspector)
/// Scene name for gameplay must be "MainScene" (or change gameSceneName below)
public class TitleScreen : MonoBehaviour
{
    [Header("Scene to Load")]
    public string gameSceneName = "MainScene";

    [Header("UI References")]
    public Text titleText;
    public Text subtitleText;
    public Button startButton;
    public Image backgroundPanel;

    [Header("Cyberpunk Colors")]
    public Color titleColor   = new Color(0f, 1f, 1f); 
    public Color subtitleColor = new Color(0.8f, 0f, 1f); 
    public Color bgColor       = new Color(0.02f, 0f, 0.05f, 1f); 

    private float pulseTime = 0f;

    void Start()
    {
        // Apply cyberpunk style
        if (backgroundPanel != null)
            backgroundPanel.color = bgColor;

        if (titleText != null)
        {
            titleText.text = "NEON FLOCK";
            titleText.color = titleColor;
            titleText.fontSize = 72;
        }

        if (subtitleText != null)
        {
            subtitleText.text = "[ A CYBERPUNK BOIDS SIMULATION ]";
            subtitleText.color = subtitleColor;
            subtitleText.fontSize = 24;
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);

            // Style the button text
            Text btnText = startButton.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = ">>> START <<<";
                btnText.color = new Color(1f, 0.9f, 0f);
                btnText.fontSize = 32;
            }

            // Style button background
            ColorBlock cb = startButton.colors;
            cb.normalColor      = new Color(0.05f, 0f, 0.15f);
            cb.highlightedColor = new Color(0.1f, 0f, 0.3f);
            cb.pressedColor     = new Color(0.3f, 0f, 0.5f);
            startButton.colors  = cb;
        }
    }

    void Update()
    {
        pulseTime += Time.deltaTime;

        // Pulse the title color between cyan and white
        if (titleText != null)
        {
            float pulse = Mathf.Sin(pulseTime * 2f) * 0.5f + 0.5f;
            titleText.color = Color.Lerp(titleColor, Color.white, pulse * 0.3f);
        }

        // Also allow pressing Space or Enter to start
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            OnStartClicked();
        }
    }

    void OnStartClicked()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // Brief pause for button feedback feel
        yield return new WaitForSeconds(0.15f);
        SceneManager.LoadScene(gameSceneName);
    }
}