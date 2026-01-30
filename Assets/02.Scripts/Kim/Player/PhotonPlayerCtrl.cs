
using FollowCamera;
using UnityEngine;

/// <summary>
/// 포톤 네트워크 환경에서의 플레이어 컨트롤
/// 로컬/원격 플레이어 제어 및 애니메이션 동기화
/// 관전 시스템 및 외부 AI 관찰자 등록(다른 팀원이 담당한 AI 관찰자) 기능 포함.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PhotonView))]
public class PhotonPlayerCtrl : MonoBehaviour
{
    [Header("걷는 속도")]  [SerializeField] private float walkSpeed;
    [Header("발소리")][SerializeField] private AudioClip[] footstepSound;
    [SerializeField] private bool flip; // 발소리 좌우 구분을 위한 플래그
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Light flashLight; // 시야 확보를 위한 손전등
    [HideInInspector] public MouseMove mouseMove;

    private bool canControl = true; 
    private Animator anim;
    private float anim_Walk; 
    private Vector3 moveDir = Vector3.zero;
    private Vector3 input;
    private const float gravity = 10f;
    private CharacterController controller;
    private AudioSource audioSource;

    private float distCount = 0f;
    [SerializeField] private float stepDistance;
    // 포톤 네트워크
    PhotonView pv = null;
  
    //위치 정보를 송수신할 때 사용할 변수 선언 및 초기값 설정 
    Vector3 currPos = Vector3.zero;
    Quaternion currRot = Quaternion.identity;

    void Awake()
    { 
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        // 포톤 네트워크 컴포넌트 캐싱
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this; //포톤 관찰 컴포넌트 설정
        pv.synchronization = ViewSynchronization.UnreliableOnChange; //전송 속도가 빠른 Unreliable로 설정
        currPos = transform.position;
        currRot = transform.rotation;
    }
    void Start()
    {
        if (pv.isMine) // 로컬 플레이어 설정
        {
            // 1인칭 시점 확보를 위한 본인 메시 비활성화
            foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>())
               r.enabled = false;
             
            playerCamera.gameObject.SetActive(true); // 카메라 오브젝트 활성화
            flashLight.enabled = true; // Light 컴포넌트 활성화

            // MouseMove 로직 초기화        
            mouseMove.Init(transform, playerCamera.transform); 
        }
        else // 원격 플레이어 설정
        {
            // 타인의 카메라는 렌더링하지 않음
            playerCamera.enabled = false; 
            flashLight.enabled = false;
        }
    }

    void Update()
    {
        if (pv.isMine) 
        {
            if (!canControl) return; // 몬스터에게 붙잡힐 시 플레이어의 이동, 물리연산 등 행동 중단 
            // 로컬 플레이어 로직
            HandleMovement();
        }
        else
        {
            // 원격 플레이어 부드러운 위치 및 회전 보간
            transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, currRot, Time.deltaTime * 10f);
        }
    }

    // 로컬 플레이어 이동 처리 및 물리 연산
    void HandleMovement()
    {
        if (controller.isGrounded)
        {
            GetInput(); // 공중에서 입력을 막기 위해 여기서 호출
            Vector3 desireMove = (transform.forward * input.y) + (transform.right * input.x);
            moveDir.x = desireMove.x * walkSpeed;
            moveDir.z = desireMove.z * walkSpeed;
        }
        else
            moveDir.y -= gravity * Time.deltaTime; // 중력 적용
        
        mouseMove.LookRotation(transform, playerCamera.transform);
        AnimControll();
        controller.Move(moveDir * Time.deltaTime);
        PlayFootStepAudio();
    }

    void GetInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        input = new Vector2(horizontal, vertical);

        if(input.sqrMagnitude > 1) // 대각선 이동 속도 보정
            input.Normalize();
    }

    void AnimControll()
    {
        anim_Walk = input.magnitude;
        anim.SetFloat("Walk",Mathf.Abs(anim_Walk));
    }

    // 실제 이동 거리에 따라 발소리 재생
    void PlayFootStepAudio()
    {
        // 땅을 밟고있지 않다면 소리 off
        if (!controller.isGrounded) 
              return;
        // 실제 수평 이동 속도 계산
        float currentSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        distCount += currentSpeed * Time.deltaTime; // 실제 이동 거리 누적
        
        if(distCount >= stepDistance) // 이동 속도에 맞춘 사운드 재생
        {
            flip =! flip;
            int stepSound = flip? 0 : 1;
            audioSource.PlayOneShot(footstepSound[stepSound]);
            distCount = 0f;
        }
    }

    // 외부에서 사망 트리거 발생 시 컨트롤러 중지
    public bool StopController()
    {
        canControl = false;
        return canControl;
    }

    // 플레이어 파괴
    [PunRPC]
    public void DisablePlayer()
    {
        Destroy(gameObject);

        if (!pv.isMine)
           PlayerDeath.Instance.OnSpectatedTargetDead();
        
    }
   
    // 사망 시 본인 카메라 및 손전등 비활성화
    public void OffCamera()
    {
        if (pv.isMine)
        {
            playerCamera.enabled = false; 
            flashLight.enabled = false;  
        }
    }

    // 관찰 모드 진입 시 다른 플레이어의 시야를 공유
    public void Spectate(PhotonPlayerCtrl target)
    {
        // 관찰 대상이 없다면 관전 불가
        if (target == null || !target.gameObject.activeInHierarchy) 
        {
            // 로컬 카메라 완전히 끄기 (No Camera Rendering 방지)
            if (pv.isMine && playerCamera != null)
                playerCamera.enabled = false;
            return;
        }

        target.playerCamera.enabled = true; 
        target.flashLight.enabled = true;   

        // 관찰 대상의 1인칭 시점 확보를 위해 대상의 메시 렌더링 끄기
        foreach (var mr in target.GetComponentsInChildren<SkinnedMeshRenderer>())
            mr.enabled = false; 
    }

    // 네트워크 데이터 송수신 처리 (위치, 회전, 애니메이션 파라미터)
    // 로컬의 위치,회전.. 값을 원격에 부드럽게 복제하기 위한 데이터 직렬화 
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // 로컬 데이터(위치, 회전, 애니메이션)를  네트워크에 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(anim_Walk);
        }

        else
        {
            // 네트워크에서 원격으로 수신 전달
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            anim_Walk = (float)stream.ReceiveNext();

            // 애니메이션 파라미터 적용
            anim.SetFloat("Walk",Mathf.Abs(anim_Walk));
        }

    }
}
