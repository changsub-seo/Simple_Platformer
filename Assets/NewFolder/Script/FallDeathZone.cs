using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class FallDeathZone : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        BoxCollider2D zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider == null) return;

        Color previousColor = Gizmos.color;
        Gizmos.color = new Color(1f, 0.12f, 0.12f, 0.16f);
        Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        Gizmos.color = new Color(1f, 0.08f, 0.08f, 0.95f);
        Gizmos.DrawWireCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        Gizmos.color = previousColor;
    }

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider != null && !zoneCollider.isTrigger)
        {
            Debug.LogError("FallDeathZone의 Collider2D는 Is Trigger로 설정해야 합니다.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats != null) playerStats.DieFromFall();
    }
}
