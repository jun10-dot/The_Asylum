using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PicPuzl : InteractableItem
{
    [Header("퍼즐 설정")]
    public GameObject picpuzlPrefab; // 퍼즐 UI 프리팹
    public Littledoor linkedDoor;    // 퍼즐 성공 시 활성화될 문 연결
    

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
        GameObject ui = Instantiate(picpuzlPrefab);


        // 퍼즐 UI에 이 PhonePuzzle 연결
        PicPuzlUi puzzleUI = ui.GetComponent<PicPuzlUi>();
        if (puzzleUI != null)
        {
            puzzleUI.linkedPuzzle = this;
        }
        else
        {
            Debug.LogWarning("PicPuzlUI 스크립트를 찾을 수 없습니다!");
        }
    }

    // 퍼즐 UI에서 호출할 함수
    public void CallSolvePuzzle()
    {
        // RPC 호출 (모든 클라이언트 동기화)
        pv.RPC("OnPuzzleSolved", PhotonTargets.All);
    }


    // 모든 플레이어에게 동기화되는 퍼즐 완료 처리
    [PunRPC]
    public void OnPuzzleSolved()
    {
        if (isSolved) return;

        isSolved = true;
        Debug.Log("퍼즐 해결 완료! 문 작동!");

        // 문 활성화
        if (linkedDoor != null)
        {
            linkedDoor.OpenDoor(); // RPC 호출
            //linkedDoor.isUnlocked = true;
            //linkedDoor.enabled = true;
            Debug.Log("연결된 문이 활성화됨!");
        }
        else
        {
            Debug.LogWarning("연결된 Littledoor가 없습니다!");
        }
        // 퍼즐 오브젝트(자물쇠) 제거
        Debug.Log("자물쇠 오브젝트 제거!");
        Destroy(transform.parent.gameObject);


        // 상호작용 해제
        InteractableItem.isInteracting = false;
    }
}
