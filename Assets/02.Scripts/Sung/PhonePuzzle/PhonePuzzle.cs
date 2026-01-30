using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PhonePuzzle : InteractableItem
{
    [Header("퍼즐 설정")]
    public GameObject phonePuzzleUIPrefab; // 퍼즐 UI 프리팹
    public GameObject rewardItem;     // 성공 시 등장할 아이템 프리팹
    public Transform dropPoint;             // 아이템이 떨어질 위치
    public GameObject hintPrefab;     // 성공 시 등장할 힌트 프리팹
    public Transform hintdropPoint2;             // 힌트 떨어질 위치

    private bool isSolved = false;          // 이미 퍼즐이 풀렸는지 여부

    private PhotonView pv;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    protected override void Interact()
    {
        Debug.Log("전화기 UI");
        if (isSolved || InteractableItem.isInteracting) return; // 이미 해결했다면 재실행 금지

        // 상호작용 잠금
        InteractableItem.isInteracting = true;

        // 퍼즐 UI 띄우기
        GameObject ui = Instantiate(phonePuzzleUIPrefab);


        // 퍼즐 UI에 이 PhonePuzzle 연결
        PhonePuzzleUI puzzleUI = ui.GetComponent<PhonePuzzleUI>();
        if (puzzleUI != null)
        {
            puzzleUI.linkedPuzzle = this;
        }
        else
        {
            Debug.LogWarning("PhonePuzzleUI 스크립트를 찾을 수 없습니다!");
        }
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
        if (hintPrefab != null)
        {
            Vector3 pos2 = hintdropPoint2 != null ? hintdropPoint2.position : transform.position + Vector3.up * 0.5f;
            Instantiate(hintPrefab, pos2, Quaternion.identity);
        }
        // 상호작용 해제
        InteractableItem.isInteracting = false;
    }

    // 로컬에서 호출할 함수 (UI → 퍼즐본체)
    public void CallPuzzleSolved()
    {
        pv.RPC("OnPuzzleSolved", PhotonTargets.AllBuffered);
    }
}
