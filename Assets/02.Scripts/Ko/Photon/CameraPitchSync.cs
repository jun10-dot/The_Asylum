using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPitchSync : MonoBehaviour
{
    private float targetPitch;    // 네트워크로 받은 목표 pitch
    private float smoothSpeed = 20f; // 부드럽게 보간할 속도

    void Update()
    {
        // 로컬 플레이어는 직접 움직이므로 보간 X
        if (GetComponentInParent<PhotonView>().isMine) 
            return;

        // 원격 플레이어 → 부드럽게 보간
        Vector3 e = transform.localEulerAngles;
        e.x = Mathf.LerpAngle(e.x, targetPitch, Time.deltaTime * smoothSpeed);
        transform.localEulerAngles = e;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // 로컬이면 pitch 보내기
            stream.SendNext(transform.localEulerAngles.x);
        }
        else
        {
            // 원격 플레이어의 pitch 받기
            targetPitch = (float)stream.ReceiveNext();
        }
    }
}
