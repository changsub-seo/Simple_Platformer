using UnityEngine;
using System.Collections;

public class SceneStartFader : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    void Start()
    {
        // 씬이 시작되자마자 서서히 밝아지는 코루틴 실행
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 0f;
    }
}