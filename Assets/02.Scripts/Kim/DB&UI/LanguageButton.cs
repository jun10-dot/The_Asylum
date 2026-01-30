
using UnityEngine;

public class LanguageButton : MonoBehaviour
{
    //한국어 버튼 = "korean", 영어 버튼 = "english" (인스펙터에 입력)
     [SerializeField] private string language; 
    
    // 버튼에 함수 연결
     public void OnClickLanguage()
     {
        // 매니저에게 언어 변경을 요청
         LanguageManager.Instance.ChangeLanguage(language);
     }
}
