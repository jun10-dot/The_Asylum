using System.Collections;
using UnityEngine;

/// <summary>
/// 싱글 모드용 컬러 퍼즐 게임 매니저
/// 퀴즈 색상 노드 1개를 보여준 뒤, 플레이어가 같은 색상 노드를 클릭하는지 관리
/// </summary>

public class SingleColorNodeManager : MonoBehaviour
{
    private bool isStart; // 게임 시작 여부
    private int currentRound = 0; // 현재 라운드
    private int maxRound = 5; // 최대 라운드

    private ColorNode colorNode; 
    private static SingleColorNodeManager instance = null;
    [SerializeField] private GameObject KeyToken; // 퍼즐 클리어 시 보상으로 나타날 토큰 (팀 협업 코드)
#region Property
    public static SingleColorNodeManager Instance {get{return instance; } }
    public bool IsStart { get { return isStart; } }
#endregion
    void Awake()
    {
        instance = this;
        colorNode = GameObject.FindObjectOfType<ColorNode>();
    }
  
    // 퍼즐 게임을 코루틴으로 시작
    public void PuzzleStart()
    {
        StartCoroutine(RunPattern());
    }

    // 정답 색상 노출 후 선택지 노드들을 생성 혹은 활성화
    IEnumerator RunPattern()
    {
        colorNode.FindNode(true); // 정답 노드를 1초 동안 보여줌
        yield return new WaitForSeconds(1f); 
        colorNode.FindNode(false); // 정답 노드를 가리고 0.3초 동안 대기
        yield return new WaitForSeconds(0.3f);
        colorNode.FindNodes(true); // 플레이어가 선택할 수 있는 3*3 노드 활성화
    }

    // 플레이어가 클릭한 노드의 색상이 정답과 일치하는지 판정
    public void CheckClicked(Color nodeColor)
    {
        // ColorNode에 저장된 정답 색상과 클릭한 색상 비교
        if(colorNode.SaveColor == nodeColor)
        {
            Debug.Log("정답");
            StartCoroutine(NextRound(2f)); // 다음 라운드 진행
        }
        else
        {
            Debug.Log("오답");
            ResetPuzzle(); // 퍼즐 리셋 (0 라운드 부터)
        }
    }

    // 정답일 경우, 라운드를 증가시키고 다음 패턴 실행
    IEnumerator NextRound(float delay)
    {
        colorNode.FindNodes(false); // 3*3 노드들 비활성화하고 2초 대기
        yield return new WaitForSeconds(delay);
        currentRound++;
        if(currentRound >= maxRound) // 최대 라운드 달성 시 종료, 아니면 계속 진행
            EndPuzzle();
        else
            yield return StartCoroutine(RunPattern());
    }

    // 오답일 경우 라운드 초기화하고 모든 노드를 비활성화 후 재시작
    void ResetPuzzle()
    {
        currentRound = 0;
        // 정답 노드 및 3*3 노드들 모두 비활성화
        if (colorNode.node != null)
            colorNode.node.gameObject.SetActive(false);

        if (colorNode.NodeArr == null) return;
        foreach (var node in colorNode.NodeArr)
        {
            node.gameObject.SetActive(false);
        }
        
        // 0.5초 후 퍼즐 재시작
        StartCoroutine(RestartAfterDelay(0.5f));
    }

    IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PuzzleStart();
    }

    // 퍼즐 종료: 토큰 아이템 활성화 및 사용된 노드 객체들을 파괴하여 정리
    void EndPuzzle()
    {
        KeyToken.SetActive(true);
        Destroy(colorNode.node.gameObject);
        foreach(var node in colorNode.NodeArr)
        {
            if(node != null)
               Destroy(node.gameObject);
        }
        this.gameObject.SetActive(false); // 매니저 객체 비활성화 (해당 퍼즐 끝)
    }
}