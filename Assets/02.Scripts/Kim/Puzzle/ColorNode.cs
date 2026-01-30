
using System.Collections.Generic;
using UnityEngine;
using static System.Enum;

/// <summary>
///  퍼즐 게임의 노드(타일) 생성 및 색상 배치를 관리하는 클래스
/// </summary>

public class ColorNode : MonoBehaviour
{
    private Node[,] nodeArr; // 생성된 노드들을 저장하는 2차원 배열
    public Node node; // 퀴즈용 단일 노드 참조
    private Node nodes; // 퀴즈용 노드들 참조
    private int nodeCount = 3; // 노드 가로세로 갯수 (3*3)
    private Node m_nodePrefab; // 생성할 노드 프리팹
    private Transform board; // 노드들이 생성될 Transform
    private int m_nodeOne = 1; // 퀴즈용 단일 노드 수
    public Color SaveColor; // 정답용 컬러 저장
    private ColorsType lastQuizColor; // 이전 퀴즈용 색상 (중복 방지용)
    private float agnmentAngle = 90f; // 노드 정렬 각도
    private static readonly System.Random r = new System.Random(); // 랜덤 객체
#region Property 
    public Node[,] NodeArr {get{return nodeArr; } }
#endregion
    void Awake()
    {
        board = transform.Find("Board"); // Board 오브젝트를 찾아 루트로 설정
        m_nodePrefab = Resources.Load<Node>("Node"); // Resources에서 노드 프리팹 로드
    }

    
    // 지정된 크기(nodeCount * nodeCount)의 퀴즈용 노드를 생성
    public void CreateNode(int nodeCount)
    {
        nodeArr = new Node[nodeCount, nodeCount];
        int count = 0;
        NodeLocation(); // 보드 및 프리팹 각도 설정
        // 중복 없는 랜덤 색상 리스트 추출
        List<ColorsType> uniqueColors = GetUniqueColors(nodeCount * nodeCount);
        
        for (int row = 0; row < nodeCount; ++row)
        {
            for(int col = 0; col< nodeCount; ++col)
            {
                // 노드 생성
                nodes = Instantiate(m_nodePrefab, Vector3.zero,m_nodePrefab.transform.rotation, board);
                nodes.name = "Node :" + count++;
                nodeArr[row, col] = nodes;

                // 노드 좌표 정보 및 간격(3) 설정
                nodes.transform.localPosition = new Vector3(col * 3, -row * 3, 0);

                nodes.SetColor(uniqueColors[row * nodeCount + col]); // 셔플된 리스트에서 순서대로 색상 할당
            }
        }
    }
    
    // 노드 프리팹과 보드의 회전값을 설정하여 정렬
    void NodeLocation()
    {
        m_nodePrefab.transform.rotation = Quaternion.Euler(0f, 0f, agnmentAngle);
        board.rotation = Quaternion.Euler(0f, agnmentAngle, 0f);
    }

    // 퀴즈용 단일 노드 생성
    public void CreateOneNode(int nodeOne)
    {
         List<ColorsType> uniqueColors = GetUniqueColors(nodeOne);

        NodeLocation();
        node = Instantiate(m_nodePrefab, Vector3.zero,m_nodePrefab.transform.rotation , board);
        node.transform.localPosition = new Vector3(3,-3,0);
        node.name = "Quiz_Node";

        // 랜덤으로 나온 색상을 단일 노드로 설정
        node.SetColor(uniqueColors[r.Next(uniqueColors.Count)]);
        node.IsQuizNode = true;
    }

    // 피셔-예이츠 셔플 알고리즘을 사용하여 중복 없는 색상 리스트 생성
    private List<ColorsType> GetUniqueColors(int neededCount)
    {
        // 열거형에서 정의된 모든 ColorsType을 리스트로 변환
        List<ColorsType> colors = new List<ColorsType>((ColorsType[])GetValues(typeof(ColorsType)));

        // 리스트를 무작위로 섞기
        for(int i = colors.Count -1; i > 0; i--) 
        {
            int j = r.Next(i + 1); 
            ColorsType temp = colors[i]; 
            colors[i] = colors[j];
            colors[j] = temp;
        }

        // 필요한 개수만큼 리스트 범위를 잘라서 반환
        if(neededCount > colors.Count)
            neededCount = colors.Count;

        return colors.GetRange(0, neededCount);
    }

    // 퀴즈 노드를 활성화/비활성화하고 색상을 갱신
    public Node FindNode(bool active) 
    {
        if(node == null) 
            CreateOneNode(m_nodeOne);
        Node quiz_node = node;
         
        if (active)
        {
            
            List<ColorsType> colors = new List<ColorsType>((ColorsType[])GetValues(typeof(ColorsType)));
            ColorsType newColor;
            // 이전 퀴즈와 중복되지 않는 새로운 색상 선택
            do
               newColor = colors[r.Next(colors.Count)];             
            while (newColor == lastQuizColor && colors.Count > 1);

            quiz_node.SetColor(newColor);
            lastQuizColor = newColor;
        }
        else
            SaveColor = quiz_node.MeshRen.material.color; // 비활성화 시 현재 색상 저장

        quiz_node.gameObject.SetActive(active);
        return quiz_node;
    }

   // 3*3 퀴즈용 노드들을 활성화/비활성화하고 색상 갱신
   public void FindNodes(bool active)
   {
       if (nodeArr == null)
           CreateNode(nodeCount);

        if (active)
        {
            // 노드들을 다시 활성화할 때 색상을 다시 셔플하여 배치
            List<ColorsType> uniqueColors = GetUniqueColors(nodeCount * nodeCount);
            int index = 0;

            for (int row = 0; row < nodeCount; row++)
            {
                for (int col = 0; col < nodeCount; col++)
                {
                    if (nodeArr[row, col] != null)
                    {
                        nodeArr[row, col].SetColor(uniqueColors[index++]); 
                        nodeArr[row, col].gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            // 노드들 비활성화
            for (int row = 0; row < nodeCount; row++)
            {
                for (int col = 0; col < nodeCount; col++)
                {
                    if (nodeArr[row, col] != null)
                        nodeArr[row, col].gameObject.SetActive(false);
                }
            }
        }
   }
}
