using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Reflection;

[Serializable] // MySQL 데이터 값을 구조체 변수에 자동으로 채워주고 인스펙터에 보이게함
public struct LocalizationData
{
    // 각 변수명은 MySQL 테이블의 키 값과 반드시 일치해야 데이터가 들어옴
    public string PlayButton;
    public string LanguageButton;
    public string ExitButton;

    public string BackButton;
    public string InputButton;
    public string SignupButton;
    public string CommandText;

    public string SbackButton;
    public string SaveButton;
    public string SuccessText;
    public string WarningText;

    public string RoomText;
    public string RoomTextEnter;
    public string RoomTextCreate;
}


[Serializable]
public class LocalizationRoot  
{                             
    public LocalizationData Localization; //JSON 키 이름과 필드 이름이 일치해야 함
}

public class LanguageManager : MonoBehaviour 
{
    private static LanguageManager instance;
    
    // 텍스트 키와 번역된 문구를 매칭하여 저장하는 자료구조(Dictionary)
    private Dictionary<string, string> languageData = new Dictionary<string, string>();
    public event Action OnLanguageChanged; // 언어가 변경되었음을 알리는 이벤트
    private LocalizationData CurrentData; 
#region Property
    public Dictionary<string, string> LanguageData {get{return languageData; } }

    public static LanguageManager Instance {get{return instance; } }
#endregion
    void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // 언어 변경 요청 시 실행되는 함수
    public void ChangeLanguage(string lang) 
    {
        // 서버통신은 시간이 걸리므로 코루틴으로 실행
        StartCoroutine(LoadLanguage(lang));

        // (런타임 재시작 시 유지용) 언어 설정 저장
        PlayerPrefs.SetString("Language", lang); 
        PlayerPrefs.Save(); 
    }

    public IEnumerator LoadLanguage(string lang)
    {
        // 서버 경로에 불러오고 싶은 언어 파라미터를 담아 PHP 서버의 데이터를 요청 전송
        UnityWebRequest www = UnityWebRequest.Get(Server_URL.BaseURL + "/controller/getLocalization.php?lang=" + lang); 
        yield return www.SendWebRequest();// 서버에서 응답이 올 때 까지 대기

        if(www.result != UnityWebRequest.Result.Success) 
        {
            Debug.Log("서버 통신 실패");
            yield break;
        }

        // JSON 데이터를 유니티가 읽을 수 있는 클래스로 변환
        LocalizationRoot root = JsonUtility.FromJson<LocalizationRoot>(www.downloadHandler.text);
        CurrentData = root.Localization;

        RefreshDictionary(CurrentData);

        OnLanguageChanged?.Invoke();// 데이터 로드가 끝난 후, 구독 중인 UI에 언어 갱신
    }

    // 구조체의 필드명과 값을 자동으로 딕셔너리에 담아주는 함수
    void RefreshDictionary(LocalizationData data)
    {
        languageData.Clear();
        // LocalizationData 구조체의 모든 필드 정보를 가져옴
        FieldInfo[] fields = typeof(LocalizationData).GetFields();

        foreach (var field in fields) // 필드 이름을 키로, 실제 내용을 값으로 추가
            languageData.Add(field.Name, (string)field.GetValue(data)); 
    }
}