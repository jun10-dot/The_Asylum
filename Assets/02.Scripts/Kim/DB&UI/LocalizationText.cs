using UnityEngine;
using UnityEngine.UI;

public class LocalizationText : MonoBehaviour
{
    [SerializeField] private string key; // LocalizationData 구조체 필드명 혹은 MySQL 테이블 키 값과 일치하도록 입력
    private Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void OnEnable() 
    {
        if(LanguageManager.Instance == null)
            return;
        
        LanguageManager.Instance.OnLanguageChanged += UpdateText; // 이벤트 구독
        UpdateText(); // 화면 전환 시 갱신 안된 UI 바로 호출
    }

    void OnDisable() 
    {
        LanguageManager.Instance.OnLanguageChanged -= UpdateText; // 이벤트 구독 해제
    }

    public void UpdateText() //언어변경될때마다 호출
    {
        // 딕셔너리에 해당 키가 있는지 확인 후 텍스트 교체
        if (LanguageManager.Instance.LanguageData.ContainsKey(key)) 
        {                                         
            text.text = LanguageManager.Instance.LanguageData[key]; 
        }
    }
}
