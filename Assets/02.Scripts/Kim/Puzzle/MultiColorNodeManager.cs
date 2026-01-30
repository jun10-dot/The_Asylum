using System.Collections;
using UnityEngine;

/// <summary>
/// 멀티 모드용 컬러 퍼즐 게임 매니저
/// 2인 협동: 한명은 정답 색상을 보고(출제자), 한명은 3*3 노드를 클릭(정답자)하여 진행
/// 다른 팀원이 구현한 채팅 시스템을 통해 의사 전달 가능
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class MultiColorNodeManager : MonoBehaviour, IHoverable
{
    private bool isStart; // 게임 시작 여부
    private int currentRound = 0; // 현재 라운드
    private int maxRound = 5; // 최대 라운드

    private ColorNode colorNode;
    private static MultiColorNodeManager m_instance = null;

    [SerializeField] private GameObject preColorManagerObj; // 싱글 모드 전환용 오브젝트

    private PhotonView pv; // 포톤
    private bool isQuizMaster; // true: 출제자, false: 정답자
    private bool isSingleMode; // 현재 싱글 모드 여부
    [SerializeField] private GameObject KeyToken; // 퍼즐 클리어 시 보상으로 나타날 토큰 (팀 협업 코드)
#region Property
    public static MultiColorNodeManager Instance{get { return m_instance; }}
    public bool IsStart{get { return isStart; }}
    public bool IsSingleMode { get{return isSingleMode; }}
#endregion
    void Awake()
    {
        // 컴포넌트 캐싱
        m_instance = this;
        colorNode = GameObject.FindObjectOfType<ColorNode>();
        pv = GetComponent<PhotonView>();
    }

    // 다른 플레이어가 방을 나갔을 때 호출 되는 포톤 콜백
    void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        SwitchToSingleMode();
    }

    //멀티 모드 -> 싱글 모드 전환
    private void SwitchToSingleMode()
    {
        StopAllCoroutines(); 
        currentRound = 0;
        ResetPuzzle();

        isSingleMode = true;

        // 싱글 매니저 활성화
        preColorManagerObj.SetActive(true);

        // 멀티 매니저 비활성화
        this.gameObject.SetActive(false);

        // 싱글 퍼즐 로직 시작
        var preManager = preColorManagerObj.GetComponent<SingleColorNodeManager>();
        preManager.PuzzleStart();
    }

    // 플레이어의 역할(출제자/정답자)에 따른 시야 설정
    public void SetRole(bool master)
    {
        //true : 출제자, false : 정답자
        isQuizMaster = master;
        if (isQuizMaster)
        {
            // 퀴즈 노드(1개)만 보이고 3*3노드는 숨김
            colorNode.FindNode(true);   
            colorNode.FindNodes(false); 
        }
        else
        {
            // 퀴즈 노드(1개) 숨기고 3*3노드는 보이기
            colorNode.FindNodes(true);
            colorNode.FindNode(false); 
        }
    }

    // 클릭된 색상이 정답인지 확인 (정답자쪽에서 호출)
    public void CheckClicked(Color nodeColor)
    {
        if (PhotonNetwork.playerList.Length <= 1)
        {
            SwitchToSingleMode();
            return;
        }
        if (colorNode.SaveColor == nodeColor)
        {
            Debug.Log("정답");
            pv.RPC("NextRound", PhotonTargets.All); 
        }
        else
        {
            Debug.Log("오답");
            pv.RPC("ResetPuzzle", PhotonTargets.All); 
        }
    }

    [PunRPC]
    void NextRound()
    {
        StartCoroutine(NextRound(2f));
    }

      // 오답일 경우 라운드 초기화하고 모든 노드를 비활성화 후 재시작
    [PunRPC]
    void ResetPuzzle()
    {
        currentRound = 0;
       
        if (colorNode.node != null)
        colorNode.node.gameObject.SetActive(false);

        if (colorNode.NodeArr == null) return;
        foreach (var node in colorNode.NodeArr)
        {
            node.gameObject.SetActive(false);
        }
        
        StartCoroutine(RestartAfterDelay(0.5f));
    }

    // 다음 라운드 진행
    IEnumerator NextRound(float delay)
    {
        colorNode.FindNodes(false); // 3*3 노드들 비활성화하고 2초 대기
        yield return new WaitForSeconds(delay);

        currentRound++;

        if (currentRound >= maxRound) // 최대 라운드 달성 시 종료, 아니면 계속 진행
            EndPuzzle();
        else
            yield return StartCoroutine(RunPattern());
    }

  

    IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PuzzleStart();
    }

    void PuzzleStart()
    {
        StartCoroutine(RunPattern());
    }

    // 출제자에게는 정답 색상 노출
    // 정답자게에는 선택지 노드들(3*3)을 생성 혹은 활성화
    IEnumerator RunPattern()
    {
         if (!this.gameObject.activeInHierarchy)
              yield break; // 비활성 상태면 실행 중단
         
        if (isQuizMaster) // 출제자 (퀴즈용 노드(1개))
        {
            // 출제자가 새로운 정답 색상을 보고 확인
            Node quizNode = colorNode.FindNode(true); 
            yield return new WaitForSeconds(1f); // 1초 대기 후 숨김
            quizNode = colorNode.FindNode(false);

            Color targetColor = colorNode.SaveColor;
            // 네트워크를 통해 상대방에게 색상 정보 송신 (로컬X)
            pv.RPC("SendQuizColor", PhotonTargets.Others, targetColor.r, targetColor.g, targetColor.b);

            // 3×3 비활성화
            colorNode.FindNodes(false);
        }
    }

    // 출제자가 보낸 퀴즈용 노드의 색상 정보를 받아 정답으로 설정
    [PunRPC]
    void SendQuizColor(float r, float g, float b)
    {
        colorNode.SaveColor = new Color(r, g, b);
        colorNode.FindNodes(true); // 정답자: 3*3 노드 활성화
    }

    // 퍼즐 종료: 토큰 아이템 활성화 및 사용된 노드 객체들을 파괴하여 정리
    void EndPuzzle()
    {
        KeyToken.SetActive(true);

        if(colorNode.node != null)
        Destroy(colorNode.node.gameObject);

        foreach (var node in colorNode.NodeArr)
        {
            if(node != null)
               Destroy(node.gameObject);
        }
        this.gameObject.SetActive(false);
    }

    #region interaction
    // 인터페이스, 클릭 시 실행 (빨간색 버튼 오브젝트)
    public void OnHover()
    {
        // 중복 실행 방지
        // 네트워크 환경에서 한 명(게스트)만 시작 버튼 누르도록 제한
         if (PhotonNetwork.isMasterClient && PhotonNetwork.playerList.Length >= 2) 
        return; 
        if (Input.GetMouseButtonDown(0) && !isStart)
        {
            isStart = true;

            int playerCount = PhotonNetwork.playerList.Length;

            if (playerCount >= 2) // 방에 2명 이상인 경우 멀티 모드로
            {
                if (PhotonNetwork.isMasterClient)
                    SetRole(false); // 마스터를 정답자로 설정
                else
                {
                    SetRole(true); // 게스트를 출제자로 설정
                    StartCoroutine(RunPattern()); // 멀티 퍼즐 로직 시작
                }
            }
            else
                SwitchToSingleMode(); // 혼자라면 싱글 모드로
        }
    }

    public void OnHoverExit() { }

    #endregion
}