using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("설정")]
    public Transform player;                    // 플레이어 Transform
    public GameObject eKeyIconPrefab;           // E아이콘 Quad Prefab
    public float interactDistance = 2f;         // 상호작용 거리
    public Vector3 eIconOffset = new Vector3(0, 0f, 0); // 아이템 위 위치
    public int outlineColorIndex = 0;   // Outline.cs color 인덱스

    private GameObject eKeyIconInstance;
    private bool isPlayerNear = false;

    // 전역 상호작용 잠금 (모든 아이템이 공유)
    public static bool isInteracting = false;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        if (player == null)
            Debug.LogError("Player 태그가 붙은 오브젝트가 필요합니다!");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool near = distance <= interactDistance;

        // 가까이 가면
        if (near && !isPlayerNear)
        {
            isPlayerNear = true;
            ShowEKeyIcon();
        }
        // 멀어지면
        else if (!near && isPlayerNear)
        {
            isPlayerNear = false;
            HideEKeyIcon();
        }

        // ⚠ 퍼즐 중이면 상호작용 비활성화
        if (isInteracting) return;
        // 상호작용
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        // 사물 이동 시 아이콘 위치 갱신
        if (eKeyIconInstance != null)
        {
            eKeyIconInstance.transform.position = transform.position + eIconOffset;
        }

    }

    void ShowEKeyIcon()
    {
        
        if (eKeyIconPrefab == null || eKeyIconInstance != null) return;

        // 월드 좌표 기준으로 생성, 부모에 붙이지 않음
        eKeyIconInstance = Instantiate(eKeyIconPrefab, transform.position + eIconOffset, Quaternion.identity);

        // 회전/스케일은 프리팹 그대로 유지
        // 부모 없이 독립적 존재, EKeyBillboard로 카메라 바라보기
    }

    void HideEKeyIcon()
    {
        if (eKeyIconInstance != null)
        {
            Destroy(eKeyIconInstance);
        }
    }

    protected virtual void Interact()
    {
        Debug.Log($"{name} 상호작용됨!");
        //GetComponent<PhonePuzzle>().Interact();
        // TODO: 퍼즐 확대/전화기 UI 띄우기 등
    }
}
