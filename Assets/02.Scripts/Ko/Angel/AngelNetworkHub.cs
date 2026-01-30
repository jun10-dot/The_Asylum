using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AngelNetworkHub : MonoBehaviour
{
    public AngelFSM_NavMesh fsm;

    PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();               // ← 허브에 PhotonView 필수
        if (!pv) Debug.LogError("[Hub] PhotonView missing on AngelNetworkHub.");
        if (!fsm) fsm = GetComponent<AngelFSM_NavMesh>();
    }

    [PunRPC]
    void RPC_AddObserver(int headViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient) return;
        var v = PhotonView.Find(headViewID);
        if (v && v.transform && fsm) fsm.AddObserver(v.transform);
    }

    [PunRPC]
    void RPC_RemoveObserver(int headViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient) return;
        var v = PhotonView.Find(headViewID);
        if (v && v.transform && fsm) fsm.RemoveObserver(v.transform);
    }
}