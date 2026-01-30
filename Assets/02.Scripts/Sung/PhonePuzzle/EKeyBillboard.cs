using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EKeyBillboard : MonoBehaviour
{

    void LateUpdate()
    {

        if (Camera.main == null) return;

        // 카메라를 바라보게 (빌보드)
        //transform.forward = Camera.main.transform.forward;
        // 부모 회전의 영향을 무시하고, 카메라를 바라보도록 전역 회전 설정
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
    }
}
