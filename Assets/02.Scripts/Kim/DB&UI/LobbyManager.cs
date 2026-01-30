
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
public class LobbyManager : MonoBehaviour
{
    // --- UI 오브젝트 연결 --- //
    public event Action OnPanelChanged;
    private static LobbyManager instance;
    [SerializeField] private GameObject mainLobby; // 메인 로비 화면 
    [SerializeField] private GameObject userInputField; // 로그인 화면 
    [SerializeField] private GameObject roomLobby; // 방 목록 화면 
    [SerializeField] private GameObject joinMembership; // 회원가입 화면
    [SerializeField] private GameObject languagePanel; // 언어 선택 화면
#region Property
    public static LobbyManager Instance {get{return instance; } }
#endregion

    void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 활성화
    }
 
    // [메인로비 -> 로그인 화면] 으로 전환
    public void UserInputField()
    {
        mainLobby.SetActive(false);
        userInputField.SetActive(true);
        languagePanel.SetActive(false);
    }

    // [로그인 화면 -> 메인 로비]로 돌아가기 
    public void BackPreLobby() 
    {
        userInputField.SetActive(false);
        mainLobby.SetActive(true);
        OnPanelChanged?.Invoke();
    }

    // [회원가입 화면 -> 로그인 화면]으로 돌아가기
    public void BackPreUserInputField() //이전유저입력 창으로 돌아가기
    {
        joinMembership.SetActive(false);
        userInputField.SetActive(true);
        OnPanelChanged?.Invoke();
    }

    public void EnterRoomLobby()
    {
        userInputField.SetActive(false);
        roomLobby.SetActive(true);
    }

    // [ 로그인 화면 -> 회원가입 화면] 으로 전환
    public void EnterRegistration() 
    {
        userInputField.SetActive(false);
        joinMembership.SetActive(true);

        OnPanelChanged?.Invoke();
    }

    // 언어 선택 화면 켜고 끄기
    public void ShowLanguagePanel() => languagePanel.SetActive(true);
    public void HideLanguagePanel() => languagePanel.SetActive(false);
    
    // 게임 종료
    public void QuitGame()
    {
       #if UNITY_EDITOR
            EditorApplication.isPlaying = false; 
       #else
           Application.Quit(); 
       #endif
    }
}
