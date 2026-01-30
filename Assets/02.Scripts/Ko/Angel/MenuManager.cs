using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    //접속된 플레이어수 UI
    public Text txtConnect;
    //채팅창
    public Text txtLogMsg;
    //RPC 호출을 위한 연결 레퍼런스
    PhotonView pv;
    public InputField inputText;
    public Text txtChat;
    private Transform[] playerPos;
    //입력중
    private bool isTyping = false;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        playerPos = GameObject.Find("PlayerSpawnPoint").GetComponentsInChildren<Transform>();
        //플레이어 생성 함수 호출
        StartCoroutine(this.CreatePlayer());
        //포톤 클라우드 메세지 수신 false해놓은거 다시 연결
        PhotonNetwork.isMessageQueueRunning = true;

        //룸에 입장후 기존 유저 정보 출력
        GetConnectPlayerCount();
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        string msg = "\n<color=#ffffff>[" + PhotonNetwork.player.NickName + "] 님이 입장하셨습니다.</color>";


        //RPC 호출 및 뒤늦게 들어온 플레이어를 위한 로그 저장 및 내보냄 처리
        pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);

        //룸에 있는 네트워크 간 통신 완료 과정 1초 대기
        yield return new WaitForSeconds(1.0f);


    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            //아직 입력중 아닐시 채팅창 활성화
            if(!isTyping)
            {
                StartTyping();

            }
            else
            {
                //입력중 일시 Enter > 채팅 전송
                if (!string.IsNullOrEmpty(inputText.text))
                    OnChatstart();
                StopTyping();
            }

        }

        //ESC 누를시 입력 취소
        if(Input.GetKeyDown(KeyCode.Escape) && isTyping)
        {
            StopTyping();
        }
    }

    IEnumerator CreatePlayer()
    {
        //입장한 방 정보 가져오기
        Room currRoom = PhotonNetwork.room;

        object[] ex = new object[3];
        ex[0] = 3;
        ex[1] = 4;
        ex[2] = 5;

        GameObject player = PhotonNetwork.Instantiate("MainPlayer", playerPos[currRoom.PlayerCount].position, playerPos[currRoom.PlayerCount].rotation, 0, ex);

        player.name = "Player";

        // 미니맵 캠 start
        GameObject.Find("MiniMapCam").GetComponent<MiniMapCam>().miniCamStart = true;

        yield return null;
    }

    //방 유저 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        //현재 입장한 방 정보를 가져온다(레퍼런스 연결)
        Room currRoom = PhotonNetwork.room;

        //현재 방 접속자 수와 최대 접속 가능한 수를 문자열로 구성한후 UI에 출력
        txtConnect.text = currRoom.PlayerCount.ToString()
                           + "/"
                           + currRoom.MaxPlayers.ToString();
    }
    //플레이어가 룸으로 접속했을때 호출되는 콜백함수
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        //ID,이름,커스텀 속성
        Debug.Log(newPlayer.ToStringFull());
        //룸에 현재 접속자 정보를 가져옴
        GetConnectPlayerCount();
    }

    //플레이어가 방을 나갈경우 호출되는 콜백함수
    void OnPhotonPlayerDisconnected(PhotonPlayer outPlyer)
    {
        //룸에 현재 접속자 정보를 가져옴
        GetConnectPlayerCount();
    }
    [PunRPC]
    void LogMsg(string msg)
    {
        //로그 메세지 Text UI에 표시
        txtLogMsg.text = txtLogMsg.text + msg;
    }
    //룸 나가기 이벤트 연결 함수
    public void OnClickExitRoom()
    {
        string msg = "\n<color=#ffffff>["
                        + PhotonNetwork.player.NickName
                        + "] Disconnected</color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);

        //방을 빠져나가며 네트워크 객체 삭제
        PhotonNetwork.LeaveRoom();
    }

    //방에서 접속종료됫을때 호출되는 콜백함수
    void OnLeftRoom()
    {
        //로비로 이동
        SceneManager.LoadScene("Title");
    }
    [PunRPC]
    //채팅기능
    void Chat(string message)
    {
        txtLogMsg.text = message;
    }
    void StartTyping()
    {
        isTyping = true;
        inputText.gameObject.SetActive(true); //입력창 보이기
        inputText.interactable = true;
        inputText.ActivateInputField();//타이핑


    }
    void StopTyping()
    {
        isTyping = false;
        inputText.DeactivateInputField();//해체
        inputText.interactable = false;
    }
    public void OnChatstart()
    {

        string msg = "\n<color=#ff0000>["
                   + PhotonNetwork.player.NickName
                   + "]</color>";
        string message = msg + " <color=#ffffff>" + inputText.text + "</color>";

        txtLogMsg.text += message;
        pv.RPC("Chat", PhotonTargets.AllBuffered, txtLogMsg.text);

        inputText.text = string.Empty;
    }

}