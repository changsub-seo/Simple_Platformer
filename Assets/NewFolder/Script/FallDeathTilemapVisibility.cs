using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Keeps the fall-death marker visible while editing, but hides only its
/// renderer during play. The TilemapCollider2D remains active as a trigger.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TilemapRenderer))]
public sealed class FallDeathTilemapVisibility : MonoBehaviour
{
    private TilemapRenderer tilemapRenderer;

    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (Application.isPlaying)
        {
            tilemapRenderer.enabled = false;
        }
    }
}
