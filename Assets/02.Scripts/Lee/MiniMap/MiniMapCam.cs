using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCam : MonoBehaviour
{
    private Transform playerPos;
    private Camera layer;

    [HideInInspector]
    public bool miniCamStart = false;

    void Awake()
    {
        layer = GetComponent<Camera>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //layer.cullingMask = 0;     // cullingMask 레이어 Everything 설정        
    }

    // Update is called once per frame
    void Update()
    {
        if(miniCamStart)
        {
            playerPos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            miniCamStart = false;
        }
        // 플레이어의 좌표와 미니맵 카메라 좌표 동기화
        transform.position = new Vector3(playerPos.transform.position.x, 50.0f, playerPos.transform.position.z);
        
        if (playerPos.transform.position.y >= 4.0f)     // 2층에 있는 경우
        {
            ChangeCullingMask(0);
        }
        else if (playerPos.transform.position.y < 4.0f /*&& playerPos.transform.position.y >= 0.0f*/)   // 1층에 있는 경우
        {
            ChangeCullingMask(1);
        }
    }

    void ChangeCullingMask(int layerIdx)
    {
        switch(layerIdx)
        {
            case 0:     // 2층 레이어 렌더링
                layer.cullingMask = (1 << LayerMask.NameToLayer("PLAYER"))
                                    | (1 << LayerMask.NameToLayer("Stair"))
                                    //| (1 << LayerMask.NameToLayer("1stFloor"))    // 선호에 따라 1층 렌더링 여부 선택 가능
                                    | (1 << LayerMask.NameToLayer("2ndFloor"));
                //layer.cullingMask |= 1 << LayerMask.NameToLayer("2ndFloor");
                break;
            case 1:     // 2층 레이어 렌더링 제외
                layer.cullingMask = (1 << LayerMask.NameToLayer("PLAYER"))
                                    | (1 << LayerMask.NameToLayer("Stair"))
                                    | (1 << LayerMask.NameToLayer("1stFloor"));
                //layer.cullingMask = ~(1 << LayerMask.NameToLayer("2ndFloor"));
                break;
        }
    }
}
