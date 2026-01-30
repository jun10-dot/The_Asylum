using UnityEngine;

/// <summary>
/// 인터페이스 기반으로 플레이어 상호작용을 처리하는 클래스
/// Raycast를 통해 상호작용 대상을 탐색  
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("거리")]
    [SerializeField] private float rayDistance = 1.5f; // 상호작용 가능한 최대 거리
    [SerializeField] private Camera rayCamera;

    private Ray ray;
    private RaycastHit hitInfo;
    private PhotonView pv;

    // 매 프레임 GetComponent 호출을 방지하기 위한 인터페이스 캐싱
    private IHoverable hover;
    private IPlayerReceiver playerInfo;
    private GameObject interaction; // 현재 상호작용 대상 오브젝트
    
    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!pv.isMine) // 자신의 플레이어만 상호작용이 가능하도록 제한
            return;
        RaySelected();
    }

    // 정중앙 마우스 위치를 기준으로 Raycast를 발사하여 오브젝트를 감지
    void RaySelected()
    {
        ray = rayCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, rayDistance))
        {
            // 타겟 감지 및 현재 타겟 할당
            if(interaction != hitInfo.transform.gameObject) 
            {
                interaction = hitInfo.transform.gameObject; 
                ClearSelection();
            }
            // 감지된 타겟의 상속 받은 인터페이스 로직 실행
            HandleDetection(hitInfo.transform); 
            SendPlayerInfo(hitInfo.transform);
        }
        else // 허공을 바라보면 상태 초기화
            ClearSelection();
    }

    // 상호작용 상태를 초기화하며 타겟의 인터페이스 로직 실행
    void ClearSelection()
    {
        if(hover == null) return; 
        hover.OnHoverExit(); // ex : 외곽선 효과 제거
        hover = null;
        interaction = null;
        playerInfo = null;
    }

    // 감지된 타겟의 인터페이스 로직 실행
    void HandleDetection(Transform targetRay)
    {
        if(hover == null)
           hover = targetRay.GetComponent<IHoverable>(); 
        if(hover == null) return; 
           hover.OnHover(); // ex : 외곽선 효과, 문 상호작용, 그 외 팀원코드 (대부분 클릭 로직)

    }

    // 타겟이 플레이어 정보가 필요한 경우
    void SendPlayerInfo(Transform targetRay)
    {
        if(playerInfo == null)
           playerInfo = targetRay.GetComponent<IPlayerReceiver>();
        if(playerInfo == null) return;
        playerInfo.SetPlayer(transform); // ex : 문 로직에 플레이어 참조 전달
    }
}