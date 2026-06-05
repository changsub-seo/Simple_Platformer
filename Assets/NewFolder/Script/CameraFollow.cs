using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    // Lerp 대신 SmoothDamp를 사용하기 위한 변수들
    public float smoothTime = 0.25f; // 목표 위치에 도달하는 데 걸리는 대략적인 시간 (작을수록 빨리 쫓아감)
    private Vector3 velocity = Vector3.zero; // 현재 속도를 저장할 변수 (내부적으로 사용됨)

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            
            // SmoothDamp: 스프링처럼 아주 부드럽고 자연스럽게 목표 위치로 이동시킵니다.
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}