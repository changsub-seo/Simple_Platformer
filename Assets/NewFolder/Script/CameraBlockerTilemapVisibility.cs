using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Hides the camera-only marker artwork in play without changing its tile data.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TilemapRenderer))]
public sealed class CameraBlockerTilemapVisibility : MonoBehaviour
{
    private void Awake()
    {
        if (Application.isPlaying)
        {
            GetComponent<TilemapRenderer>().enabled = false;
        }
    }
}
