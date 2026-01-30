using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    private HexaNode[][] hexaNodes = new HexaNode[4][];
    private List<HexaNode> openList = new List<HexaNode>();
    private List<HexaNode> closedList = new List<HexaNode>();

    private CostCalculator costCalculator = new CostCalculator();

    private HexaNode prevNode;
    private HexaNode curNode;
    private HexaNode startNode;
    private HexaNode targetNode;

    private List<HexaNode> pathNode;
    private List<HexaNode> curNeighbours = new List<HexaNode>();

    public GameObject defaultWire;
    public GameObject keyToken;
    public GameObject note;
    
    [HideInInspector]
    public bool searchPath = false;
    [HideInInspector]
    public bool done = false;

    private SoundManager sMgr;
    private bool soundTrigger = true;

    void Awake()
    {
        sMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    void Update()
    {
        // 노드가 넘어오고 처리해야 함
        if (searchPath)
        {
            PathFindCoroutine();
            searchPath = false;
        }
    }

    // 전달받은 노드 오브젝트마다 가지고 있는 HexaNode 컴포넌트 연결
    public void GetAry(ref GameObject[][] temp)
    {
        hexaNodes[0] = new HexaNode[7];
        hexaNodes[1] = new HexaNode[6];
        hexaNodes[2] = new HexaNode[7];
        hexaNodes[3] = new HexaNode[6];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < temp[i].Length; j++)
            {
                hexaNodes[i][j] = temp[i][j].GetComponent<HexaNode>();
            }
        }
    }

    // 노드마다 뻗어있는 전선의 상태를 받아오기 위한 함수
    public void GetConnectAry(int row, int col, List<bool> conAry)
    {
        hexaNodes[row][col].ConnectAry = conAry;
    }

    public int GetDistance(HexaNode node1, HexaNode node2)
    {
        int x = Mathf.Abs(node1.Col - node2.Col);
        int y = Mathf.Abs(node1.Row - node2.Row);

        return 1 * x + 1 * y;
    }

    private bool CheckNode(int row, int col)
    {
        // 노드 배열 범위를 벗어난경우 제외
        if (row < 0 || row >= 4)
            return false;
        if (col < 0 || (row % 2 == 0 && col >= 7) || (row % 2 == 1 && col >= 6))
            return false;
        return true;
    }

    private bool ConCheck(bool curNodeCon, bool neighbourNodeCon)
    {
        // 현재 노드와 이웃 노드가 전선으로 연결된 상태인지를 체크
        if (curNodeCon && neighbourNodeCon)
            return true;
        else
            return false;
    }

    // 경로를 찾은 경우 역추적하여 경로를 확인
    public List<HexaNode> RetracePath(HexaNode curNode)
    {
        List<HexaNode> nodes = new List<HexaNode>();

        while (curNode != null)
        {
            nodes.Add(curNode);
            curNode = curNode.Parent;
        }

        nodes.Reverse();

        return nodes;
    }

    public HexaNode[] GetNeighbours(HexaNode node)
    {
        List<HexaNode> temp = new List<HexaNode>();
        temp.Clear();

        // 가변 배열이므로 row 마다 다르게 탐색
        // (2, 1) 기준 (3, 0) (3, 1) (2, 0) (2, 2) (1, 0) (1, 1) 탐색해야함
        if (node.Row % 2 == 0)
        {
            // 좌측 상단
            if (CheckNode(node.Row + 1, node.Col - 1) &&
                (ConCheck(node.ConnectAry[0], hexaNodes[node.Row + 1][node.Col - 1].ConnectAry[2]) ||
                ConCheck(node.ConnectAry[5], hexaNodes[node.Row + 1][node.Col - 1].ConnectAry[3])))
                temp.Add(hexaNodes[node.Row + 1][node.Col - 1]);
            // 우측 상단
            if (CheckNode(node.Row + 1, node.Col) &&
                (ConCheck(node.ConnectAry[0], hexaNodes[node.Row + 1][node.Col].ConnectAry[4]) ||
                ConCheck(node.ConnectAry[1], hexaNodes[node.Row + 1][node.Col].ConnectAry[3])))
                temp.Add(hexaNodes[node.Row + 1][node.Col]);
            // 좌측
            if (CheckNode(node.Row, node.Col - 1) &&
                (ConCheck(node.ConnectAry[4], hexaNodes[node.Row][node.Col - 1].ConnectAry[2]) ||
                ConCheck(node.ConnectAry[5], hexaNodes[node.Row][node.Col - 1].ConnectAry[1])))
                temp.Add(hexaNodes[node.Row][node.Col - 1]);
            // 우측
            if (CheckNode(node.Row, node.Col + 1) &&
                (ConCheck(node.ConnectAry[1], hexaNodes[node.Row][node.Col + 1].ConnectAry[5]) ||
                ConCheck(node.ConnectAry[2], hexaNodes[node.Row][node.Col + 1].ConnectAry[4])))
                temp.Add(hexaNodes[node.Row][node.Col + 1]);
            // 좌측 하단
            if (CheckNode(node.Row - 1, node.Col - 1) &&
                (ConCheck(node.ConnectAry[3], hexaNodes[node.Row - 1][node.Col - 1].ConnectAry[1]) ||
                ConCheck(node.ConnectAry[4], hexaNodes[node.Row - 1][node.Col - 1].ConnectAry[0])))
                temp.Add(hexaNodes[node.Row - 1][node.Col - 1]);
            // 우측 하단
            if (CheckNode(node.Row - 1, node.Col) &&
                (ConCheck(node.ConnectAry[2], hexaNodes[node.Row - 1][node.Col].ConnectAry[0]) ||
                ConCheck(node.ConnectAry[3], hexaNodes[node.Row - 1][node.Col].ConnectAry[5])))
                temp.Add(hexaNodes[node.Row - 1][node.Col]);
        }
        // (1, 1) 기준 (2, 1) (2, 2) (1, 0) (1, 2) (0, 1) (0, 2) 탐색해야함
        else if (node.Row % 2 == 1)
        {
            // 좌측 상단
            if (CheckNode(node.Row + 1, node.Col) &&
                (ConCheck(node.ConnectAry[0], hexaNodes[node.Row + 1][node.Col].ConnectAry[2]) ||
                ConCheck(node.ConnectAry[5], hexaNodes[node.Row + 1][node.Col].ConnectAry[3])))
                temp.Add(hexaNodes[node.Row + 1][node.Col]);
            // 우측 상단
            if (CheckNode(node.Row + 1, node.Col + 1) &&
                (ConCheck(node.ConnectAry[0], hexaNodes[node.Row + 1][node.Col + 1].ConnectAry[4]) ||
                ConCheck(node.ConnectAry[1], hexaNodes[node.Row + 1][node.Col + 1].ConnectAry[3])))
                temp.Add(hexaNodes[node.Row + 1][node.Col + 1]);
            // 좌측
            if (CheckNode(node.Row, node.Col - 1) &&
                (ConCheck(node.ConnectAry[4], hexaNodes[node.Row][node.Col - 1].ConnectAry[2]) ||
                ConCheck(node.ConnectAry[5], hexaNodes[node.Row][node.Col - 1].ConnectAry[1])))
                temp.Add(hexaNodes[node.Row][node.Col - 1]);
            // 우측
            if (CheckNode(node.Row, node.Col + 1) &&
                (ConCheck(node.ConnectAry[1], hexaNodes[node.Row][node.Col + 1].ConnectAry[5]) ||
                ConCheck(node.ConnectAry[2], hexaNodes[node.Row][node.Col + 1].ConnectAry[4])))
                temp.Add(hexaNodes[node.Row][node.Col + 1]);
            // 좌측 하단
            if (CheckNode(node.Row - 1, node.Col) &&
                (ConCheck(node.ConnectAry[3], hexaNodes[node.Row - 1][node.Col].ConnectAry[1]) ||
                ConCheck(node.ConnectAry[4], hexaNodes[node.Row - 1][node.Col].ConnectAry[0])))
                temp.Add(hexaNodes[node.Row - 1][node.Col]);
            // 우측 하단
            if (CheckNode(node.Row - 1, node.Col + 1) &&
                (ConCheck(node.ConnectAry[2], hexaNodes[node.Row - 1][node.Col + 1].ConnectAry[0]) ||
                ConCheck(node.ConnectAry[3], hexaNodes[node.Row - 1][node.Col + 1].ConnectAry[5])))
                temp.Add(hexaNodes[node.Row - 1][node.Col + 1]);
        }
        return temp.ToArray();

    }

    public void Ready()
    {
        openList.Clear();
        closedList.Clear();

        // 시작점과 끝점은 고정
        startNode = hexaNodes[0][0];
        targetNode = hexaNodes[3][5];

        startNode.Parent = null;
        targetNode.Parent = null;

        curNode = startNode;

        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, targetNode);

        ResetColor(hexaNodes);
        soundTrigger = true;
    }

    public void PathFindCoroutine()
    {
        Ready();
        StartCoroutine(PathFind(startNode));
    }

    public IEnumerator PathFind(HexaNode start)
    {
        HexaNode[] neighbours = GetNeighbours(curNode);

        curNeighbours.Clear();

        curNeighbours.AddRange(neighbours);

        for (int i = 0; i < neighbours.Length; ++i)
        {
            // 닫힌리스트에 있거나 장애물 판정인 노드는 제외
            if (closedList.Contains(neighbours[i]))
                continue;
            if (neighbours[i].Obstacle)
                continue;

            int gCost = curNode.GCost + GetDistance(neighbours[i], curNode);

            if (openList.Contains(neighbours[i]) == false ||
                gCost < neighbours[i].GCost)
            {
                int hCost = GetDistance(neighbours[i], targetNode);
                neighbours[i].GCost = gCost;
                neighbours[i].HCost = hCost;
                neighbours[i].Parent = curNode;

                if (!openList.Contains(neighbours[i]))
                    openList.Add(neighbours[i]);
            }
        }

        closedList.Add(curNode);

        if (openList.Contains(curNode))
            openList.Remove(curNode);

        if (openList.Count > 0)
        {
            openList.Sort(costCalculator);

            if (curNode != null)
            {
                prevNode = curNode;
            }
            curNode = openList[0];
        }

        yield return null;

        // 목적지에 도달한경우
        if (curNode == targetNode)
        {
            List<HexaNode> nodes = RetracePath(curNode);
            pathNode = nodes;
            foreach (HexaNode temp in pathNode)
            {
                // 경로 보여줄때 사용
                for (int i = 0; i < temp.transform.childCount; i++)
                {
                    temp.transform.GetChild(i).GetComponent<MeshRenderer>().material.color
                    = new Color(0.0f, 1.0f, 0.0f);
                }

            }
            defaultWire.GetComponent<MeshRenderer>().material.color
            = new Color(0.0f, 1.0f, 0.0f);
            //Debug.Log("Find Path!");
            StopCoroutine(PathFind(startNode));
            note.SetActive(true);
            keyToken.SetActive(true);
            done = true;

            if(soundTrigger)
            {
                soundTrigger = false;
                sMgr.PlayEffct(transform.position, sMgr.sfxClips[33]);
            }
        }

        // 목적지에 도달하지 못했다면 재탐색
        else
        {
            for (int i = 0; i < curNode.transform.childCount; i++)
            {
                curNode.transform.GetChild(i).GetComponent<MeshRenderer>().material.color
                = new Color(0.0f, 0.0f, 1.0f);
            }
            defaultWire.GetComponent<MeshRenderer>().material.color
            = new Color(0.0f, 0.0f, 1.0f);
            StartCoroutine(PathFind(startNode));
            //Debug.Log("can't Find");
        }

    }
    
    // 전선 색상 초기화
    public void ResetColor(HexaNode[][] hexaNode)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < hexaNode[i].Length; j++)
            {
                for (int k = 0; k < hexaNodes[i][j].transform.childCount; k++)
                {
                    hexaNodes[i][j].transform.GetChild(k).GetComponent<MeshRenderer>().material.color
                    = new Color(0.0f, 0.0f, 0.0f);
                }
            }
        }

        for (int i = 0; i < startNode.transform.childCount; i++)
        {
            startNode.transform.GetChild(i).GetComponent<MeshRenderer>().material.color
            = new Color(0.0f, 0.0f, 1.0f);
        }

        for (int i = 0; i < targetNode.transform.childCount; i++)
        {
            targetNode.transform.GetChild(i).GetComponent<MeshRenderer>().material.color
            = new Color(0.0f, 0.0f, 1.0f);
        }
    }

}