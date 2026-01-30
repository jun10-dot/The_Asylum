using UnityEngine;
using UnityEngine.UI; 

public class Login : MonoBehaviour
{
    [SerializeField] private GameObject textCommand; // 로그인 실패 텍스트
    [SerializeField] private InputField registerInputField; // 회원 가입 이름 입력창
    [SerializeField] private InputField loginInputField; // 로그인 이름 입력창
    [SerializeField] private GameObject registerText; // 회원가입 성공 텍스트
    [SerializeField] private GameObject registerCommand; // 회원 가입 실패 텍스트
    private string loginName; // 로그인 성공 시 저장 될 유저 이름
    public string LoginName {get{return loginName; } }

    void OnEnable()
    {
        if(LobbyManager.Instance == null) return;
        LobbyManager.Instance.OnPanelChanged += ResetLoginUI;
    }

    void OnDisable()
    {
        if(LobbyManager.Instance == null) return;
        LobbyManager.Instance.OnPanelChanged -= ResetLoginUI;
    }

    void ResetLoginUI()
    {
        loginInputField.text = null;
        registerInputField.text = null;
        textCommand.SetActive(false);
        registerText.SetActive(false);
        registerCommand.SetActive(false);
    }
    // 로그인 성공 후 방 목록 화면으로 이동
    public void NextRoomLobby() 
    {
        ////데이터베이스 로직
        //string inputName = loginInputField.text.Trim(); // 양쪽 끝 공백 제거된 텍스트
        //LoginManager.Instance.UserName = inputName;
        // PHP 서버에 회원가입으로 생성된 이름이 존재하는지 확인, 결과는 콜백함수로 전달
        //StartCoroutine(LoginManager.Instance.CheckUserExist(inputName, OnLoginResponse));

        /// 데이터 베이스 적용 안한 버전 ///
        if (loginInputField.text.Trim() == "") // 입력값이 비어있으면 진행X
            return;

        loginName = loginInputField.text; // 입력한 이름을 저장
        LobbyManager.Instance.EnterRoomLobby();
        ////////////////////////////////////
    }

    //// PHP 서버로부터 로그인 응답을 받았을 때 호출되는 함수
    public void OnLoginResponse(LoginResponse response)
    {
        if (response.status == "success") // 로그인 성공 시
        {
            loginName = LoginManager.Instance.UserName;
            LobbyManager.Instance.EnterRoomLobby();
            return;
        }
        else // 로그인 실패 시
        {
            Debug.Log("존재하지 않는 유저입니다.");
            textCommand.SetActive(true);
            return;
        }
    }

     //// 유저 이름 저장 및 회원가입 요청 함수
    public void SaveUserName()
    {
        // 입력창의 텍스트를 가져오고 양쪽 끝 공백 제거
        string name = registerInputField.text.Trim();
        if (name.Length >= 3) // 이름이 3글자 이상인지 체크
        {
            LoginManager.Instance.UserName = name; // PHP 서버로 보낼 유저 이름 설정
            // 서버에 이름 전송하고 응답이 오면 콜백 함수 실행
            StartCoroutine(LoginManager.Instance.SendUserId(RegisterAndGetToken));
        }
        else // 입력 숫자가 3글자 미만인 경우 실패
        {
            registerText.SetActive(false);
            registerCommand.SetActive(true);
        }
    }

    //// PHP 서버로부터 회원가입 응답을 받았을 때 호출되는 함수
    public void RegisterAndGetToken(LoginResponse response)
    {
        if (response.status == "success") // 가입 성공
        {
            registerText.SetActive(true);
            registerCommand.SetActive(false);
            Debug.Log("회원가입 완료!");
        }
        else // 이미 이름이 있는 경우(중복) 실패
        {
            registerText.SetActive(false);
            registerCommand.SetActive(true);
            Debug.Log("회원가입 실패");
        }
    }
}
