using UnityEngine;
using UnityEngine.EventSystems;

// ⭐ IBeginDragHandler가 반드시 추가되어야 OnDrag가 정상적으로 발동합니다.
public class UIDragger : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    [Header("이동시킬 창 (비워두면 부모가 자동 지정됨)")]
    public RectTransform targetWindow;

    private Canvas canvas;

    void Awake()
    {
        if (targetWindow == null)
        {
            targetWindow = transform.parent.GetComponent<RectTransform>();
        }
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 시 인벤토리 창을 다른 UI들보다 맨 앞으로 가져옵니다.
        if (targetWindow != null)
        {
            targetWindow.SetAsLastSibling();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 유니티 이벤트 시스템이 드래그를 인식하게 만드는 필수 트리거 (내용은 비워둬도 무방)
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetWindow == null || canvas == null) return;

        // 마우스가 이동한 거리(delta)만큼 해상도 스케일 비율에 맞춰 창을 이동시킵니다.
        targetWindow.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
        // 화면 밖으로 나가지 못하게 막기
        ClampToWindow();
    }

    private void ClampToWindow()
    {
        Vector3[] canvasCorners = new Vector3[4];
        Vector3[] panelCorners = new Vector3[4];

        RectTransform canvasRect = canvas.transform as RectTransform;
        canvasRect.GetWorldCorners(canvasCorners);
        targetWindow.GetWorldCorners(panelCorners);

        float tx = 0f;
        float ty = 0f;

        // 좌우상하 경계선을 넘어가면 반대쪽으로 밀어내기
        if (panelCorners[0].x < canvasCorners[0].x) tx = canvasCorners[0].x - panelCorners[0].x;
        if (panelCorners[2].x > canvasCorners[2].x) tx = canvasCorners[2].x - panelCorners[2].x;
        if (panelCorners[0].y < canvasCorners[0].y) ty = canvasCorners[0].y - panelCorners[0].y;
        if (panelCorners[2].y > canvasCorners[2].y) ty = canvasCorners[2].y - panelCorners[2].y;

        targetWindow.position = new Vector3(targetWindow.position.x + tx, targetWindow.position.y + ty, targetWindow.position.z);
    }
}