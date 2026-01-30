using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

// PHP 서버로부터 받을 로그인 데이터 구조체
[Serializable]
public class LoginResponse
{
    public string status;
}

// PHP 서버 통신 및 로그인 시스템
public class LoginManager : MonoBehaviour
{
    private static LoginManager instance;
    private string userName; // 입력 받은 유저 이름 저장
#region Property
    public static LoginManager Instance {get{return instance; } }
    public string UserName {get{return userName; } set{userName = value; } }
#endregion  

    void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    // PHP 서버에 유저 이름을 전달하고 응답을 처리하는 코루틴 
    private IEnumerator PostUserId(string url, Action<LoginResponse> callback)
    {
        // 서버에 보낼 form 데이터 생성
        WWWForm form = new WWWForm(); 
        form.AddField("userId",userName);

        // UnityWebRequest를 사용하여 서버에 데이터 전송
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 통신 실패");
                yield break;
            }
            // 서버가 보낸 Json 데이터를 유니티가 쓸 수 있게 클래스 형태로 변환
            string json = www.downloadHandler.text;
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(json);

            if (res.status == "success") // 로그인 성공 시 공통적으로 토큰을 저장
                Debug.Log(userName + "로그인 성공");

            callback?.Invoke(res); // 결과를 콜백 함수로 전달
        }
    }

    // PHP 서버에 접근하기 위한 경로
    public IEnumerator SendUserId(Action<LoginResponse> callback)
    {
        string url = Server_URL.BaseURL + "/controller/login.php";
        yield return PostUserId(url, callback);
    }
    // PHP 서버에 해당 유저 이름이 존재하는지 확인하는 함수
    // 입력한 이름이 MySQL 테이블에 존재하는지 체크하고 결과를 반환 받음
    public IEnumerator CheckUserExist(string userNameInput, Action<LoginResponse> callback)
    {
        userName = userNameInput;
        string url = Server_URL.BaseURL + "/controller/login_check.php";

        yield return PostUserId(url, callback);
    }
}
