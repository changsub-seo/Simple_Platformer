using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("씬에 배치된 게임 오버 UI")]
    public GameObject panel;
    public Button restartButton;

    [Header("목숨 표시")]
    [SerializeField] private float lifeIconFrameDuration = 0.12f;

    private TMP_Text livesText;
    private Image lifeIcon;
    private Sprite[] lifeAnimationFrames = Array.Empty<Sprite>();
    private float animationTimer;
    private int currentLives;

    private void Awake()
    {
        Instance = this;
        transform.localScale = Vector3.one;
        ConfigureRetroScreen();

        if (panel != null) panel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(ContinueOrReturnToMenu);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (restartButton != null) restartButton.onClick.RemoveListener(ContinueOrReturnToMenu);
    }

    private void Update()
    {
        if (panel == null || !panel.activeInHierarchy) return;

        AnimateLifeIcon();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ContinueOrReturnToMenu();
        }
    }

    public static void Show(int remainingLives)
    {
        if (Instance == null)
        {
            Debug.LogError("GameOverUI가 현재 씬에 없습니다. 씬에 게임 오버 UI를 배치해 주세요.");
            Time.timeScale = 0f;
            return;
        }

        Instance.currentLives = Mathf.Max(0, remainingLives);
        Instance.UpdateLifeCounter();
        if (Instance.panel != null) Instance.panel.SetActive(true);

        PauseMenuManager pauseMenu = FindObjectOfType<PauseMenuManager>();
        if (pauseMenu != null) pauseMenu.enabled = false;

        Time.timeScale = 0f;
    }

    public void ContinueOrReturnToMenu()
    {
        Time.timeScale = 1f;

        if (currentLives <= 0)
        {
            SceneManager.LoadScene("MainMenuScene");
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ConfigureRetroScreen()
    {
        if (panel == null) return;

        Image backdrop = panel.GetComponent<Image>();
        if (backdrop != null)
        {
            backdrop.color = Color.black;
            backdrop.raycastTarget = true;
        }

        Transform content = panel.transform.Find("GameOverPanel");
        if (content == null) return;

        RectTransform contentRect = content.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
        }

        Image contentImage = content.GetComponent<Image>();
        if (contentImage != null) contentImage.color = new Color(0f, 0f, 0f, 0f);

        // The original scene title duplicated the retro GAME OVER message.
        Transform oldTitle = content.Find("Title");
        if (oldTitle != null) oldTitle.gameObject.SetActive(false);

        TMP_Text message = content.Find("Message")?.GetComponent<TMP_Text>();
        if (message != null)
        {
            message.text = "WORLD 1-1";
            message.fontSize = 30f;
            message.fontStyle = FontStyles.Bold;
            message.characterSpacing = 4f;
            message.color = Color.white;
            message.alignment = TextAlignmentOptions.Center;
            message.enableWordWrapping = false;
            message.rectTransform.anchoredPosition = new Vector2(0f, 100f);
            message.rectTransform.sizeDelta = new Vector2(600f, 180f);
            message.raycastTarget = false;
        }

        CreateLifeCounter(content);

        Transform restartLabelTransform = content.Find("RestartButton/RestartLabel");
        TMP_Text restartLabel = restartLabelTransform != null
            ? restartLabelTransform.GetComponent<TMP_Text>()
            : null;
        if (restartLabel != null)
        {
            restartLabel.fontSize = 16f;
            restartLabel.fontStyle = FontStyles.Bold;
            restartLabel.characterSpacing = 1.5f;
            restartLabel.color = new Color(1f, 0.86f, 0.35f);
            restartLabel.alignment = TextAlignmentOptions.Center;
            restartLabel.enableWordWrapping = false;
            restartLabel.raycastTarget = false;
        }

        if (restartButton != null)
        {
            Image buttonImage = restartButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = new Color(0f, 0f, 0f, 0f);
            RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = new Vector2(0f, -115f);
                buttonRect.sizeDelta = new Vector2(480f, 54f);
            }
        }

        CreateHeaderLabel("RetroHeaderLeft", "PLAYER 1", TextAlignmentOptions.TopLeft,
            new Vector2(0f, 1f), new Vector2(32f, -24f));
        CreateHeaderLabel("RetroHeaderRight", "STAGE 1", TextAlignmentOptions.TopRight,
            new Vector2(1f, 1f), new Vector2(-32f, -24f));
    }

    private void CreateLifeCounter(Transform parent)
    {
        Transform existingIcon = parent.Find("LifeCharacterIcon");
        if (existingIcon != null)
        {
            lifeIcon = existingIcon.GetComponent<Image>();
        }
        else
        {
            GameObject iconObject = new GameObject("LifeCharacterIcon",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(parent, false);
            lifeIcon = iconObject.GetComponent<Image>();
        }

        RectTransform iconRect = lifeIcon.rectTransform;
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-52f, -8f);
        iconRect.sizeDelta = new Vector2(96f, 96f);
        lifeIcon.preserveAspect = true;
        lifeIcon.raycastTarget = false;

        lifeAnimationFrames = Resources.LoadAll<Sprite>("Character/Idle_W_Character");
        Array.Sort(lifeAnimationFrames, (a, b) => string.CompareOrdinal(a.name, b.name));
        if (lifeAnimationFrames.Length == 0)
        {
            Sprite stillImage = Resources.Load<Sprite>("Character/W_Character");
            if (stillImage != null) lifeAnimationFrames = new[] { stillImage };
            else Debug.LogWarning("목숨 아이콘 이미지를 Resources/Character에서 찾지 못했습니다.");
        }
        if (lifeAnimationFrames.Length > 0) lifeIcon.sprite = lifeAnimationFrames[0];

        Transform existingText = parent.Find("LivesText");
        if (existingText != null)
        {
            livesText = existingText.GetComponent<TMP_Text>();
        }
        else
        {
            GameObject textObject = new GameObject("LivesText",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            livesText = textObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = livesText.rectTransform;
        textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(48f, -8f);
        textRect.sizeDelta = new Vector2(100f, 52f);
        livesText.fontSize = 30f;
        livesText.fontStyle = FontStyles.Bold;
        livesText.color = Color.white;
        livesText.alignment = TextAlignmentOptions.MidlineLeft;
        livesText.characterSpacing = 2f;
        livesText.raycastTarget = false;
        livesText.enableWordWrapping = false;
    }

    private void UpdateLifeCounter()
    {
        if (livesText != null) livesText.text = $"× {currentLives}";

        TMP_Text message = panel != null
            ? panel.transform.Find("GameOverPanel/Message")?.GetComponent<TMP_Text>()
            : null;
        if (message != null)
        {
            message.text = currentLives <= 0
                ? "GAME OVER\n\nWORLD 1-1"
                : "WORLD 1-1";
        }

        TMP_Text restartLabel = panel != null
            ? panel.transform.Find("GameOverPanel/RestartButton/RestartLabel")?.GetComponent<TMP_Text>()
            : null;
        if (restartLabel != null)
        {
            restartLabel.text = currentLives > 0
                ? "PRESS SPACE OR ENTER"
                : "PRESS SPACE OR ENTER TO MAIN MENU";
        }
    }

    private void AnimateLifeIcon()
    {
        if (lifeIcon == null || lifeAnimationFrames.Length < 2) return;

        animationTimer += Time.unscaledDeltaTime;
        if (animationTimer < lifeIconFrameDuration) return;

        animationTimer = 0f;
        int next = (Array.IndexOf(lifeAnimationFrames, lifeIcon.sprite) + 1) % lifeAnimationFrames.Length;
        lifeIcon.sprite = lifeAnimationFrames[next];
    }

    private void CreateHeaderLabel(string objectName, string label,
        TextAlignmentOptions alignment, Vector2 anchor, Vector2 position)
    {
        Transform existing = panel.transform.Find(objectName);
        if (existing != null) return;

        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(panel.transform, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(220f, 36f);

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18f;
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 2f;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.enableWordWrapping = false;
    }
}
