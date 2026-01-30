using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{ 
   
    [HideInInspector]
    //방 이름
    public string roomName = "";
    //현재 접속 인원
    [HideInInspector]
    public int connectPlayer = 0;
    //방 최대 인원
    [HideInInspector]
    public int maxPlayers = 0;

    //룸 이름표시를위한 Text UI항목 연결 레퍼런스
    public Text textRoomName;

    //룸 최대 인원와 현재 인원을 표시할 Text UI 항목 연결 레퍼런스
    public Text textConnectInfo;

    //Text UI 항목에 룸 정보를 표시하는 함수
    public void DisplayRoomData()
    {
        textRoomName.text = roomName;
        textConnectInfo.text="(" + connectPlayer.ToString() + "/" + maxPlayers.ToString()+")";
    }    
    
    
}
