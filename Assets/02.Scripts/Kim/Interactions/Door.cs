using System.Collections;
using UnityEngine;

// 인터페이스 기반 양방향으로 회전하는 문 클래스
// 플레이어의 위치에 따라 열리는 방향을 계산
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PhotonView))]
public class Door : MonoBehaviour, IHoverable, IPlayerReceiver
{
    [SerializeField] private AudioClip[] doorAudio; // 0: 열기, 1: 닫기
    private AudioSource audioSource;
    private PhotonView pv;
    private Transform player; // 플레이어의 현재 위치 받아오기
    private float maxAngle = 100f; // 최대 회전 각도
    private float speed = 105f; // 회전 속도
    private float rotatedAmount = 0f; // 누적 회전량 체크 
    private int rotDirection = 0; // 회전 방향 결정 (1 or -1)
    private bool openOrClose;  // true : 닫힘 , false : 열림
    private Quaternion saveRotation; // 초기 닫힌 상태의 회전값 저장
    public enum DoorState{closed, moving, opend}; // 닫힌 상태, 움직이는 상태, 열린 상태
    private DoorState state = DoorState.closed; // 초기 닫힌 상태로 설정


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        pv = GetComponent<PhotonView>();
        saveRotation = transform.rotation; // 시작 시 회전값 저장
    }

    // PlayerInteraction 스크립트에서 매 프레임 호출되는 인터페이스 함수
    public void OnHover()
    {
        // 열리는 중일 때는 입력 처리X, 상호작용 불가
        if (state == DoorState.moving) return; 

        switch(state)
        {
            case DoorState.closed:
                Closed();
                break;
            case DoorState.opend:
                Opend();
                break;
        }
    }

    public void OnHoverExit() { }
    
    // 닫힌 상태에서 입력 대기
    void Closed() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            // 플레이어 위치를 기반으로 문이 열릴 방향 계산
            int direction = CalculateDirection();
            // 모든 클라이언트들에게 문이 회전한다는 것을 알림
            pv.RPC("StartMove", PhotonTargets.All, false, direction);
        }
    }

    // 플레이어 위치를 받아오기 위한 인터페이스 함수
    public void SetPlayer(Transform player) => this.player = player;
    
    // 벡터 내적(Dot)을 활용해 플레이어가 문의 앞/뒤 중 어디에 있는지 판별
    int CalculateDirection() 
    {
        Vector3 doorForward = transform.forward;
        Vector3 playerToDoor = (transform.position - player.position).normalized;
        float dotProduct = Vector3.Dot(doorForward, playerToDoor);
        return (dotProduct >= 0) ? 1 : -1; // 앞쪽 : 1, 뒤쪽 -1 반환
    }

    // 모든 클라이언트들에게 동시에 문 회전 시작
    [PunRPC]
    void StartMove(bool currentOpenStatus, int direction)
    {
        openOrClose = currentOpenStatus;
        rotDirection = direction;
        state = DoorState.moving;
        
        // 사운드 동기화 재생
        audioSource.PlayOneShot(openOrClose ? doorAudio[1] : doorAudio[0]);
        
        StartCoroutine(RotateDoor());
    }

    // 매 프레임 조건 체크하는 Update()함수 대신, 코루틴 사용
    IEnumerator RotateDoor()
    {
        rotatedAmount = 0f;
        while (rotatedAmount < maxAngle)
        {
            float step = speed * Time.deltaTime;
 
            transform.Rotate(Vector3.up * step * rotDirection);
            rotatedAmount += step;
            yield return null;
        }
        if (openOrClose) // 닫힌 상태: 미세 오차 방지를 위해 초기 회전값 할당
            transform.rotation = saveRotation;
        state = openOrClose ? DoorState.closed : DoorState.opend; // 상태 갱신
    }

    // 열린 상태에서 입력 대기
    void Opend() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            // 위치 방향 계산 없이 열렸던 방향 반대로 음수 값 설정
            pv.RPC("StartMove", PhotonTargets.All, true, -rotDirection); 
        }
    }
}