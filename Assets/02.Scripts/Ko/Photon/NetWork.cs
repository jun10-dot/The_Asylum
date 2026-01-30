using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Network : MonoBehaviour
{
    //버전 관리
    public string version = "Ver 0.1.0"; 
    //로그 레벨
    public PhotonLogLevel LogLevel = PhotonLogLevel.Full; 
    //플레이어 닉네임 입력
    public InputField roomName;
    //룸 이름 입력
    public InputField userId;
    //룸 리스트 출력
    public GameObject scrollContents;
    //룸 목록만큼 생성될 프리팹연결
    public GameObject roomItem;
    //플레이어 스폰 위치
    public Transform playerPos; 
    [SerializeField] private Login login;
    private void Awake()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(version); //포톤 서버 접속
            PhotonNetwork.logLevel = LogLevel; //로그 레벨 설정
            PhotonNetwork.playerName = "GUEST" + Random.Range(1, 9999); //랜덤 플레이어 이름 설정

            // 특정 클라우드 서버에 직접 접속 하는 함수로, 인자는 포톤 클라우드 서버 IP 주소, Port 번호, AppID, 버전
            // PhotonNetwork.ConnectToMaster( "string serverAddress", 3306, "asdafasdda01091207", version  );
        }
        //룸 이름을 무작위로 설정
        roomName.text = "Room_" + Random.Range(0, 999).ToString("000");

        // ScrollContents의 Pivot 좌표를 Top, Left로 설정
        scrollContents.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);
    }
    //포톤 클라우드에 정상적으로 접속한 후 로비에 입장하면 호출되는 콜백함수
    void OnJoinedLobby()
    {
        Debug.Log("로비 접속 성공");

        //userId.text = GetUserId();
        //PhotonNetwork.JoinRandomRoom();//랜덤 방 접속 시도
    }
    string GetUserId()
    {
        string userId = PlayerPrefs.GetString("USER_ID");

        //유저 아이디가 Null일 경우 랜덤 아이디 생성
        if(string.IsNullOrEmpty(userId))
        {

            userId = "USER_" + Random.Range(1, 9999).ToString("0000");
           
        }
        return userId;
    }
    //포톤 클라우드는 Random Match Making 기능 제공(로비 입장 후 이미 생성된 룸 중에서 무작위로 선택해 입장)
    //무작위 룸 접속(입장)에 실패한 경우 호출되는 콜백 함수 
    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("방 접속 실패, 새로운 방 생성");

        bool isSucces = PhotonNetwork.CreateRoom("Room1");

        Debug.Log("[정보] 게임 방 생성 완료: " + isSucces);
    }

    void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        //오류 코드
        Debug.Log(codeAndMsg[0].ToString());
        //오류 메시지
        Debug.Log(codeAndMsg[1].ToString());

        Debug.Log("방 생성 실패 =" + codeAndMsg[1]);
    }

    void OnJoinedRoom()
    {
        Debug.Log("방 접속 성공");


        //플레이어 오브젝트 생성
        //CreatePlayer();
        //룸 씬으로 전환하는 코루틴 실행
        StartCoroutine(this.LoadStage());


    }
    //void CreatePlayer()
    //{
    //    float pos = Random.Range(-100f, 100f);
    //    PhotonNetwork.Instantiate("MainPlayer", playerPos.position, playerPos.rotation, 0);
    //}
    IEnumerator LoadStage()
    {
        //씬 전환동안 네트워크메시지 수신 중단
        PhotonNetwork.isMessageQueueRunning = false;
        //백그라운드로 씬로딩
        AsyncOperation ao = SceneManager.LoadSceneAsync("RoomEscape");

        yield return ao;

        Debug.Log("씬 로딩 완료");
    }
    public void OnClickJoinRandomRoom()
    {
        if(login.LoginName !=null)
        //플레이어 닉네임 설정
        PhotonNetwork.player.NickName = login.LoginName;
        //랜덤으로 지정된 룸 입장
        PhotonNetwork.JoinRandomRoom();
    }
    public void OnClickCreateRoom()
    {
        string _roomName = roomName.text;

        //룸 이름이 없거나 Null일 경우 룸 이름 지정
        if(string.IsNullOrEmpty(roomName.text))
        {
            _roomName = "Room_" + Random.Range(0, 999).ToString("000");
        }
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = login.LoginName;

        //생성할 룸의 조건 설정
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen=true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 5;

        //지정한 조건에 맞는 룸 생성 함수
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }
    
    //생성된 룸 목록이 변경됐을 때 호출되는 콜백 함수(최초 룸 접속시 호출)
    void OnReceivedRoomListUpdate()
    {
        //룸 을 생성후 다시 룸 목록을 받았을떄 새로 갱신하기위해 기존 생선된RoomItem을 삭제시키기위한 함수
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("ROOM_ITEM"))
        {
            Destroy(obj);
        }
       
        int rowCount = 0;
        //스크롤 영역 초기화
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

      
        //GetRoomList 함수는 RoomInfo 클래스 타입의 배열을 반환
        foreach (RoomInfo _room in PhotonNetwork.GetRoomList())
        {
            Debug.Log(_room.Name);
            //RoomItem 프리팹을 동적으로 생성
            GameObject room = (GameObject)Instantiate(roomItem);
            //생성한 RoomItem 프리팹의 Parent를 지정
            room.transform.SetParent(scrollContents.transform, false);

            //룸 정보를 표시하기위하여 텍스트 정보 전달.
            RoomData roomData = room.GetComponent<RoomData>();
            roomData.roomName =_room.Name;
            roomData.connectPlayer = _room.PlayerCount;
            roomData.maxPlayers=_room.MaxPlayers;

            //텍스트 정보 표시
            roomData.DisplayRoomData();
            
            //버튼 컴포넌트에 클릭 이벤트를 동적으로 연결
            roomData.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnClickRoomItem(roomData.roomName); Debug.Log("방 입장 " + roomData.roomName); });
            
           
            scrollContents.GetComponent<GridLayoutGroup>().constraintCount = ++rowCount;

            //스크롤 높이 증가
            scrollContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 20);

        }
    }
    //RoomItem클릭시 호출될 이벤트 연결 함수
    void OnClickRoomItem(string roomName)
    {
        //닉네임 설정
        PhotonNetwork.player.NickName = login.LoginName;
        //만들어진 방이름으로 전달된 룸에 입장
        PhotonNetwork.JoinRoom(roomName);
    }
    void OnGUI()
    {

        //화면 좌측 상단에 접속 과정에 대한 로그를 출력(포톤 클라우드 접속 상태 메시지 출력)
        // PhotonNetwork.ConnectUsingSettings 함수 호출시 속성 PhotonNetwork.connectionStateDetailed는
        //포톤 클라우드 서버에 접속하는 단계별 메시지를 반환함.
        //Joined Lobby 메시지시 포톤 클라우드 서버로 접속해 로비에 안전하게 입장했다는 뜻
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

        //만약 포톤네트워크에 연결 되었다면...
        //if (PhotonNetwork.connected)
        //{
        //    GUI.Label(new Rect(0, 50, 200, 100), "Connected");

        //    //룸 리스트를 배열로 받아온다.
        //    RoomInfo[] roomList = PhotonNetwork.GetRoomList();

        //    if (roomList.Length > 0)
        //    {
        //        foreach (RoomInfo info in roomList)
        //        {
        //            GUI.Label(new Rect(0, 80, 400, 100), "Room: " + info.Name
        //                + " PlayerCount/MaxPlayer :" + info.PlayerCount + "/" + info.MaxPlayers //현재 플레이어/최대 플레이어
        //                + " CustomProperties Count " + info.CustomProperties.Count // 설정한 CustomProperties 수 
        //                + " Map ???: " + info.CustomProperties.ContainsKey("Map") //키로 설정한 Map이 있나
        //                + " Map Count " + info.CustomProperties["Map"] // 설정한 키 값 
        //                + " GameType ??? " + info.CustomProperties.ContainsKey("GameType") //키로 설정한 GameType이 있나
        //                + " GameType " + info.CustomProperties["GameType"]);// 설정한 키 값 
        //        }
        //    }
        //    else
        //    {
        //        GUI.Label(new Rect(0, 80, 400, 100), "No Room List");
        //    }
        //}
        ////PhotonServerSettings 값 가져오기
        //{
        //    GUI.Label(new Rect(0, 170, 400, 100), "AppID  :  " +
        //        PhotonNetwork.PhotonServerSettings.AppID);
        //    GUI.Label(new Rect(0, 200, 200, 100), "HostType  :  " +
        //        PhotonNetwork.PhotonServerSettings.HostType);
        //    GUI.Label(new Rect(0, 230, 200, 100), "ServerAddress  :  " +
        //        PhotonNetwork.PhotonServerSettings.ServerAddress);
        //    GUI.Label(new Rect(0, 260, 200, 100), "ServerPort  :  " +
        //        PhotonNetwork.PhotonServerSettings.ServerPort);
        //    //PhotonNetwork.PhotonServerSettings.UseCloud(); 

        //    //핑 테스트
        //    int pingTime = PhotonNetwork.GetPing();
        //    GUI.Label(new Rect(0, 310, 200, 100), "Ping: " + pingTime.ToString());
        //}
    }
}
