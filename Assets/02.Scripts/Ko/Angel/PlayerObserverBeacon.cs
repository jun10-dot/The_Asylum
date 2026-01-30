using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerObserverBeacon : MonoBehaviour
{
    public Transform head;      
    PhotonView pv;
    PhotonView hubPV;
    int headID;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        //head 비어 있으면 자식에서 카메라 자동 찾기
        if (!head)
        {
            var cam = GetComponentInChildren<Camera>(true);
            if (cam) head = cam.transform;
        }

        if (!head)
        {
            Debug.LogError("[Beacon] Head(Camera) 참조가 없습니다. 프리팹에 카메라가 없거나 슬롯이 비었습니다.");
            return;
        }

        //카메라에 미리 붙어있는 PhotonView 가져오기 (프리팹에 ‘미리’ 붙어있어야 함)
        var headView = head.GetComponent<PhotonView>();
        if (!headView)
        {
            Debug.LogError("[Beacon] Head(Camera)에 PhotonView가 없습니다. 프리팹에 ‘미리’ 붙여주세요.");
            return;
        }

        headID = headView.viewID;
    }

    void Start()
    {
        if (!pv || !pv.isMine) return;
        InvokeRepeating(nameof(TryBindHubAndRegister), 0.1f, 0.5f);
    }

    void TryBindHubAndRegister()
    {
        if (!hubPV)
        {
            var hub = FindObjectOfType<AngelNetworkHub>();
            if (hub) hubPV = hub.GetComponent<PhotonView>();
            if (!hubPV) return;
        }

        hubPV.RPC("RPC_AddObserver", PhotonTargets.MasterClient, headID);
        CancelInvoke(nameof(TryBindHubAndRegister));
    }

    void OnDestroy()
    {
        if (!pv || !pv.isMine) return;
        if (hubPV) hubPV.RPC("RPC_RemoveObserver", PhotonTargets.MasterClient, headID);
    }
}
