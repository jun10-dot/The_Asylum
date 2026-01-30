using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;


public class KeypadPuzl : InteractableItem
{
    [Header("퍼즐 설정")]
    public GameObject keypadUIPrefab; // 퍼즐 UI 프리팹
    public GameObject rewardItem;     // 성공 시 등장할 아이템 프리팹
    public Transform dropPoint;             // 아이템이 떨어질 위치
    public GameObject rewardItemPrefab2;     // 성공 시 등장할 아이템 프리팹2
    public Transform dropPoint2;             // 아이템이 떨어질 위치2

    private bool isSolved = false;          // 이미 퍼즐이 풀렸는지 여부

    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }


    protected override void Interact()
    {
        Debug.Log("키패드 UI");
        if (isSolved || InteractableItem.isInteracting) return; // 이미 해결했다면 재실행 금지

        // 상호작용 잠금
        InteractableItem.isInteracting = true;

        // 퍼즐 UI 띄우기
        GameObject ui = Instantiate(keypadUIPrefab);


        // 퍼즐 UI에 이 PhonePuzzle 연결
        KeypadUI puzzleUI = ui.GetComponent<KeypadUI>();
        if (puzzleUI != null)
        {
            puzzleUI.linkedPuzzle = this;
        }
        else
        {
            Debug.LogWarning("KeypadUI 스크립트를 찾을 수 없습니다!");
        }
    }


    // 퍼즐 UI에서 호출 (RPC)
    public void CallSolvePuzzle()
    {
        pv.RPC("OnPuzzleSolved", PhotonTargets.All);
    }

    [PunRPC]
    public void OnPuzzleSolved()
    {
        if (isSolved) return;

        isSolved = true;
        Debug.Log("퍼즐 해결 완료! 아이템 지급!");

        // 아이템 드롭
        // if (rewardItemPrefab != null)
        // {
        //     Vector3 pos = dropPoint != null ? dropPoint.position : transform.position + Vector3.up * 0.5f;
        //     Instantiate(rewardItemPrefab, pos, Quaternion.identity);
        // }
        rewardItem.SetActive(true);
        // 아이템 2 드롭
        if (rewardItemPrefab2 != null)
        {
            Vector3 pos2 = dropPoint2 != null ? dropPoint2.position : transform.position + Vector3.up * 0.5f;
            Instantiate(rewardItemPrefab2, pos2, Quaternion.identity);
        }
        // 상호작용 해제
        InteractableItem.isInteracting = false;
    }
}
