using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [Header("움직일 대상 (보통 창 전체)")]
    public RectTransform targetPanel; 

    private Vector2 pointerOffset;
    private Canvas parentCanvas;
    private RectTransform canvasRectTransform;

    void Awake()
    {
        // 타겟 패널을 지정하지 않았다면, 기본적으로 부모 오브젝트를 타겟으로 잡습니다.
        if (targetPanel == null)
            targetPanel = transform.parent.GetComponent<RectTransform>();
            
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRectTransform = parentCanvas.transform as RectTransform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 클릭 시 창을 맨 앞으로 가져옵니다. (다른 창들에 가려지지 않게)
        targetPanel.SetAsLastSibling(); 
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel, 
            eventData.position, 
            eventData.pressEventCamera, 
            out pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetPanel == null || parentCanvas == null) return;

        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPointerPosition))
        {
            // 1. 마우스를 따라 창 위치 이동
            targetPanel.localPosition = localPointerPosition - pointerOffset;
            
            // 2. 화면 밖으로 나가지 않도록 제한 (Clamping)
            ClampToWindow();
        }
    }

    // ⭐ 화면 밖으로 이탈하지 못하게 막는 핵심 함수
    private void ClampToWindow()
    {
        Vector3[] panelCorners = new Vector3[4];
        targetPanel.GetWorldCorners(panelCorners); // 창의 4개 모서리 좌표 구하기

        Vector3[] canvasCorners = new Vector3[4];
        canvasRectTransform.GetWorldCorners(canvasCorners); // 전체 화면(캔버스)의 4개 모서리 좌표 구하기

        // X축 (좌우) 이탈 방지
        if (panelCorners[0].x < canvasCorners[0].x) // 왼쪽으로 나갔을 때
            targetPanel.position += new Vector3(canvasCorners[0].x - panelCorners[0].x, 0, 0);
        else if (panelCorners[2].x > canvasCorners[2].x) // 오른쪽으로 나갔을 때
            targetPanel.position -= new Vector3(panelCorners[2].x - canvasCorners[2].x, 0, 0);

        // Y축 (위아래) 이탈 방지
        if (panelCorners[0].y < canvasCorners[0].y) // 아래로 나갔을 때
            targetPanel.position += new Vector3(0, canvasCorners[0].y - panelCorners[0].y, 0);
        else if (panelCorners[2].y > canvasCorners[2].y) // 위로 나갔을 때
            targetPanel.position -= new Vector3(0, panelCorners[2].y - canvasCorners[2].y, 0);
    }
}