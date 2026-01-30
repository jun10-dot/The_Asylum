using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance; // 싱글톤

    [Header("토스트 메시지 프리팹")]
    public GameObject toastPrefab; // 프리팹 (Text 포함된 UI)
    public Canvas uiCanvas; // Screen Space - Overlay 캔버스

    [Header("표시 설정")]
    public float fadeTime = 0.5f;     // 페이드 시간
    public float defaultDuration = 2f; // 기본 표시 시간
    public float lineSpacing = 2f;    // 줄 간격

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowToast(string message, float duration = -1f)
    {
        if (toastPrefab == null || uiCanvas == null)
        {
            Debug.LogWarning("ToastManager: toastPrefab 또는 uiCanvas가 설정되지 않았습니다.");
            return;
        }

        if (duration <= 0)
            duration = defaultDuration;

        // 프리팹 인스턴스 생성
        GameObject toastObj = Instantiate(toastPrefab, uiCanvas.transform);
        Text msgText = toastObj.GetComponentInChildren<Text>();
        if (msgText != null)
        {
            msgText.text = message;

            // 여러 줄 자동 줄바꿈
            msgText.horizontalOverflow = HorizontalWrapMode.Overflow;
            msgText.verticalOverflow = VerticalWrapMode.Overflow;
            msgText.alignment = TextAnchor.MiddleCenter;

            // 줄 간격 (lineSpacing)
            msgText.lineSpacing = lineSpacing;
        }

        // 🔹 CanvasGroup 없으면 추가
        CanvasGroup group = toastObj.GetComponent<CanvasGroup>();
        if (group == null)
            group = toastObj.AddComponent<CanvasGroup>();

        group.alpha = 1f;

        // 페이드 아웃 실행
        StartCoroutine(FadeAndDestroy(toastObj, duration));
    }

    private IEnumerator FadeAndDestroy(GameObject toastObj, float duration)
    {
        CanvasGroup group = toastObj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = toastObj.AddComponent<CanvasGroup>();
        }

        // 유지 시간
        yield return new WaitForSeconds(duration);

        // 서서히 사라지기
        float fadeTime = 0.5f;
        while (group.alpha > 0)
        {
            group.alpha -= Time.deltaTime / fadeTime;
            yield return null;
        }

        Destroy(toastObj);
    }

}
