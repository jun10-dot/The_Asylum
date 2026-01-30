using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexaNode : MonoBehaviour, IHoverable
{
    private float rot;

    [SerializeField]
    private bool obstacle;
    [SerializeField]
    private List<bool> connectAry = new List<bool>(6);

    private int gCost = 0;
    private int hCost = 0;

    private int row = 0;
    private int col = 0;

    private HexaNode parent;

    private PhotonView pv;

    private int playerCount;

    private SoundManager sMgr;

    #region Property
    public bool Obstacle
    {
        get
        {
            return obstacle;
        }
    }

    public List<bool> ConnectAry
    {
        get
        {
            return connectAry;
        }
        set
        {
            connectAry = value;
        }
    }
    
    public int FCost
    {
        get
        {
            return hCost + gCost;
        }
    }

    public int GCost
    {
        get
        {
            return gCost;
        }
        set
        {
            gCost = value;
        }
    }

    public int HCost
    {
        get
        {
            return hCost;
        }
        set
        {
            hCost = value;
        }
    }

    public int Row
    {
        get
        {
            return row;
        }
        set
        {
            row = value;
        }
    }

    public int Col
    {
        get
        {
            return col;
        }
        set
        {
            col = value;
        }
    }

    public HexaNode Parent
    {
        get
        {
            return parent;
        }

        set
        {
            parent = value;
        }
    }
    #endregion

    void Awake()
    {
        rot = GetComponent<Transform>().rotation.z;
        pv = GetComponent<PhotonView>();
        sMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    void Start()
    {
        // 노드들에 row col 값 매겨줘야함
        row = int.Parse(this.name.Substring(4, 1)) - 1;
        col = int.Parse(this.name.Substring(6, 1)) - 1;
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, rot));
        playerCount = int.Parse(GameObject.FindGameObjectWithTag("Mgr").GetComponent<MenuManager>().
        txtConnect.text.Substring(0, 1));
    }

    // PhotonView 동기화
    [PunRPC]
    public void Rotate()
    {
        // 시작 노드와 도착 노드는 회전 불가
        if ((this.Row == 0 && this.Col == 0) || (this.Row == 3 && this.Col == 5))
            return;

        sMgr.PlayEffct(transform.position, sMgr.sfxClips[32]);
        rot -= 60.0f;
        rot %= 360.0f;
        //if (rot % 60 == 0)
            ChangeConnectionState();
        // 회전할때마다 경로 찾도록 구현
        transform.parent.GetComponent<AStar>().searchPath = true;
    }

    public void ChangeConnectionState()
    {
        // 회전하면 해당 함수 호출하여 연결상태 업데이트
        bool temp;
        temp = connectAry[5];
        connectAry[5] = connectAry[4];
        connectAry[4] = connectAry[3];
        connectAry[3] = connectAry[2];
        connectAry[2] = connectAry[1];
        connectAry[1] = connectAry[0];
        connectAry[0] = temp;

        // 변경된 연결상태 배열 전송
        transform.parent.GetComponent<AStar>().GetConnectAry(this.Row, this.Col, connectAry);
    }

    public void OnHover()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 퍼즐을 풀었다면 이후 노드 회전 불가
            if(transform.parent.GetComponent<AStar>().done)
                return;
            //Rotate();
            // 회전상태를 동기화 하기위해 RPC함수 호출
            pv.RPC("Rotate", PhotonTargets.All);
        }
    }

    public void OnHoverExit()
    {

    }
}
