using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PhotonView))]
public class Littledoor : MonoBehaviour, IHoverable, IPlayerReceiver
{
    [SerializeField] private AudioClip[] doorAudio;
    private AudioSource m_AudioSource;
    private PhotonView pv;
    
    private Transform player;
    float targetAngle = 100f;
    float speed = 105f;
    float rotatedAmount = 0f;
    int rotDirection = 0;
    bool openOrClose; // 기본 닫힌 상태(false)
    private Quaternion saveRotation;
    public enum DoorState{closed, moving, opend};
    public DoorState state = DoorState.closed;

    public bool isUnlocked = false; // 퍼즐 완료 시 true로 설정

    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        pv = GetComponent<PhotonView>();
        enabled = false; // 퍼즐 전까지 상호작용 불가
        saveRotation = transform.rotation;
    }
    public void OpenDoor()
    {
        enabled = true;      // 퍼즐 풀린 후 활성화
        isUnlocked = true;   // 상호작용 가능
        Debug.Log("문이 퍼즐 완료로 활성화됨!");
    }

    public void OnHover()
    {
        if (!isUnlocked) return;
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

    public void OnHoverExit()
    {

    }

    void Closed() // 닫힌 상태 -> 열어야함
    {
        if(Input.GetMouseButtonDown(0))
        {
            int direction = CalculateDirection();
            pv.RPC("RPC_StartMove", PhotonTargets.All, false, direction);
        }
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }
    int CalculateDirection() // 열리는 중
    {
        Vector3 doorForward = transform.forward;
        Vector3 playerToDoor = (transform.position - player.position).normalized;
        float dotProduct = Vector3.Dot(doorForward, playerToDoor);
        return (dotProduct >= 0) ? 1 : -1;
    }

    [PunRPC]
    void RPC_StartMove(bool currentOpenStatus, int direction)
    {
        if (state == DoorState.moving) return; // 이미 움직이고 있다면 무시

        openOrClose = currentOpenStatus;
        rotDirection = direction;
        state = DoorState.moving;
        
        // 사운드 재생 로직도 RPC 안으로 넣어야 모두에게 들립니다.
        m_AudioSource.PlayOneShot(openOrClose ? doorAudio[0] : doorAudio[1]);
        
        StartCoroutine(RotateDoor());
    }


    IEnumerator RotateDoor()
    {
        rotatedAmount = 0f;
        Quaternion startRot = transform.rotation;
        while (rotatedAmount < targetAngle)
        {
            float step = speed * Time.deltaTime;
            if (rotatedAmount + step > targetAngle)
                step = targetAngle - rotatedAmount;

            transform.Rotate(Vector3.up * step * rotDirection);
            rotatedAmount += step;
            yield return null;
        }
        if (openOrClose)
            transform.rotation = saveRotation;
        state = openOrClose ? DoorState.closed : DoorState.opend;
    }

    void Opend() // 열린 상태 -> 닫아야함
    {
        if(Input.GetMouseButtonDown(0))
        {
            pv.RPC("RPC_StartMove", PhotonTargets.All, true, -rotDirection);
        }
    }
    // 동기화용 (보정용)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
            stream.SendNext(transform.rotation);
        
        else
            transform.rotation = (Quaternion)stream.ReceiveNext();
    }
}
