using UnityEngine;
using UnityEngine.Tilemaps;

public enum CameraRestrictionMode
{
    CenterPoint = 0,
    FullViewport = 1
}

public sealed class CameraBounds : MonoBehaviour
{
    [Header("A/B comparison")]
    [Tooltip("CenterPoint: camera center cannot enter blocker tiles. FullViewport: the full visible screen avoids them.")]
    public CameraRestrictionMode restrictionMode = CameraRestrictionMode.FullViewport;

    [Tooltip("Assign the dedicated CameraBlockerTilemap. Tiles on this map affect only the camera.")]
    public Tilemap cameraBlockerTilemap;

    [Header("Optional rectangular limit")]
    [Tooltip("Enable this only when an additional rectangular world limit is needed.")]
    public bool useRectBounds;
    public Rect worldBounds = new Rect(-15f, -15f, 315f, 50f);

    private const float OverlapEpsilon = 0.001f;

    public Vector3 ResolveCameraPosition(Vector3 currentPosition, Vector3 desiredPosition, Camera targetCamera)
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            return desiredPosition;
        }

        if (useRectBounds)
        {
            desiredPosition = ClampToRect(desiredPosition, targetCamera);
        }

        if (cameraBlockerTilemap == null)
        {
            return desiredPosition;
        }

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        Vector2 delta = desiredPosition - currentPosition;
        Vector3 resolved = currentPosition;

        // Move along the dominant axis first, then the other axis. If one axis
        // is blocked, the second can still move, letting the camera slide along walls.
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            resolved = MoveAxisWithoutEnteringBlockers(resolved, delta.x, true, halfWidth, halfHeight);
            resolved = MoveAxisWithoutEnteringBlockers(resolved, delta.y, false, halfWidth, halfHeight);
        }
        else
        {
            resolved = MoveAxisWithoutEnteringBlockers(resolved, delta.y, false, halfWidth, halfHeight);
            resolved = MoveAxisWithoutEnteringBlockers(resolved, delta.x, true, halfWidth, halfHeight);
        }

        resolved.z = desiredPosition.z;
        return resolved;
    }

    private Vector3 MoveAxisWithoutEnteringBlockers(Vector3 start, float distance, bool horizontal, float halfWidth, float halfHeight)
    {
        if (Mathf.Approximately(distance, 0f)) return start;

        float cellStep = GetCellWorldSize(horizontal) * 0.2f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(distance) / Mathf.Max(cellStep, 0.02f)));
        Vector3 result = start;

        for (int i = 1; i <= steps; i++)
        {
            float travelled = distance * (i / (float)steps);
            Vector3 candidate = start;
            if (horizontal) candidate.x += travelled;
            else candidate.y += travelled;

            if (OverlapsBlocker(candidate, halfWidth, halfHeight)) break;
            result = candidate;
        }

        return result;
    }

    private bool OverlapsBlocker(Vector3 cameraPosition, float halfWidth, float halfHeight)
    {
        Vector3Int centerCell = cameraBlockerTilemap.WorldToCell(new Vector3(cameraPosition.x, cameraPosition.y, 0f));
        if (restrictionMode == CameraRestrictionMode.CenterPoint)
        {
            return cameraBlockerTilemap.GetTile(centerCell) != null;
        }

        float left = cameraPosition.x - halfWidth + OverlapEpsilon;
        float right = cameraPosition.x + halfWidth - OverlapEpsilon;
        float bottom = cameraPosition.y - halfHeight + OverlapEpsilon;
        float top = cameraPosition.y + halfHeight - OverlapEpsilon;

        Vector3Int minCell = cameraBlockerTilemap.WorldToCell(new Vector3(left, bottom, 0f));
        Vector3Int maxCell = cameraBlockerTilemap.WorldToCell(new Vector3(right, top, 0f));

        // Include a neighboring cell on each side to cover exact grid-edge cases;
        // the world-space overlap check below rejects cells that only touch an edge.
        for (int x = minCell.x - 1; x <= maxCell.x + 1; x++)
        {
            for (int y = minCell.y - 1; y <= maxCell.y + 1; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, centerCell.z);
                if (cameraBlockerTilemap.GetTile(cell) == null) continue;

                Vector3 cellOrigin = cameraBlockerTilemap.CellToWorld(cell);
                Vector3 cellEnd = cameraBlockerTilemap.CellToWorld(cell + Vector3Int.one);
                float cellLeft = Mathf.Min(cellOrigin.x, cellEnd.x);
                float cellRight = Mathf.Max(cellOrigin.x, cellEnd.x);
                float cellBottom = Mathf.Min(cellOrigin.y, cellEnd.y);
                float cellTop = Mathf.Max(cellOrigin.y, cellEnd.y);

                if (right > cellLeft && left < cellRight && top > cellBottom && bottom < cellTop)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private float GetCellWorldSize(bool horizontal)
    {
        Grid grid = cameraBlockerTilemap.layoutGrid;
        if (grid == null) return 1f;

        Vector3 localAxis = horizontal ? Vector3.right : Vector3.up;
        float localSize = horizontal ? grid.cellSize.x : grid.cellSize.y;
        return Mathf.Abs(grid.transform.TransformVector(localAxis * localSize).magnitude);
    }

    private Vector3 ClampToRect(Vector3 cameraPosition, Camera targetCamera)
    {
        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        cameraPosition.x = ClampAxis(cameraPosition.x, worldBounds.xMin, worldBounds.xMax, halfWidth);
        cameraPosition.y = ClampAxis(cameraPosition.y, worldBounds.yMin, worldBounds.yMax, halfHeight);
        return cameraPosition;
    }

    private static float ClampAxis(float value, float minimum, float maximum, float cameraHalfExtent)
    {
        float lowerLimit = minimum + cameraHalfExtent;
        float upperLimit = maximum - cameraHalfExtent;
        if (lowerLimit > upperLimit) return (minimum + maximum) * 0.5f;
        return Mathf.Clamp(value, lowerLimit, upperLimit);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying || !useRectBounds) return;

        Color previousColor = Gizmos.color;
        Vector3 center = new Vector3(worldBounds.center.x, worldBounds.center.y, 0f);
        Vector3 size = new Vector3(worldBounds.width, worldBounds.height, 0.1f);
        Gizmos.color = new Color(0.15f, 0.65f, 1f, 0.06f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.15f, 0.65f, 1f, 0.9f);
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = previousColor;
    }
}
