using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResourceHUD : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("씬에 배치된 HUD 참조")]
    public Image healthFill;
    public Image manaFill;
    public TextMeshProUGUI healthValue;
    public TextMeshProUGUI manaValue;
    public TextMeshProUGUI spiritValue;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        RefreshHud();
    }

    private void LateUpdate()
    {
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playerStats == null) return;

        SetBarFill(healthFill, playerStats.maxHealth > 0
            ? Mathf.Clamp01((float)playerStats.currentHealth / playerStats.maxHealth)
            : 0f);
        SetBarFill(manaFill, playerStats.maxMana > 0
            ? Mathf.Clamp01(playerStats.currentMana / playerStats.maxMana)
            : 0f);

        if (healthValue != null)
            healthValue.text = $"{Mathf.Max(0, playerStats.currentHealth)} / {Mathf.Max(0, playerStats.maxHealth)}";
        if (manaValue != null)
            manaValue.text = $"{Mathf.Max(0, Mathf.FloorToInt(playerStats.currentMana))} / {Mathf.Max(0, playerStats.maxMana)}";
        if (spiritValue != null)
            spiritValue.text = Mathf.Max(0, Mathf.RoundToInt(playerStats.currentTotalSpirit)).ToString();
    }

    private static void SetBarFill(Image fill, float ratio)
    {
        if (fill == null) return;

        // Anchor-based width updates are visible even when the fill image has no sprite.
        fill.type = Image.Type.Simple;
        RectTransform rect = fill.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(ratio, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
