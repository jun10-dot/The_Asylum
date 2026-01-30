using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowCam : MonoBehaviour
{


    //따라다닐 오브젝트의 Transform을 연결
    public Transform target;
    // 오브젝트와의 거리
    public float distance = 10.0f;
    //오브젝트와의 높이
    public float height = 5.0f;
    //오브젝트의 Y축 이동시 따라가는 자연스러운 속도
    public float heightDamping = 2.0f;
    //오브젝트의 Y축 회동시 따라가는 자연스러운 속도
    public float rotationDamping = 3.0f;



    // 한 프레임에 모든 Update가 실행된 후 호출되는 함수로
    // 주로 카메라의 이동이나 Update와 따로 실행되야 할 로직에 사용
    void LateUpdate()
    {
        // 만약 타겟이 없으면 멈춤
        if (!target)
            return;

        // 타겟의 오일러 앵글값 Y를 할당(원하는 결과값 셋팅)
        float wantedRotationAngle = target.eulerAngles.y;
        // 타겟의 Y축 과 height 만큼 떨어진 위치로 값을 할당(원하는 결과값 셋팅) 
        float wantedHeight = target.position.y + height;

        //현재 게임오브젝트의 오일러 앵글값 Y를 할당
        float currentRotationAngle = transform.eulerAngles.y;
        //현재 게임오브젝트의 위치 포지션 Y 를 할당
        float currentHeight = transform.position.y;

        //현재 회전값을 원하는 회전값으로 자엽스럽게 변화도록 
        //rotationDamping * Time.deltaTime(디바이스 마다 일정) 인자를 전달. 
        //Mathf.LerpAngle의 마지막 인자는 1 이 100% 이면 0은 0%
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        //카메라의 자연스러운 Y 값 추적을 위하여 Mathf.Lerp을 사용
        //Mathf.Lerp(현재 값,목표 값, 변위 값)
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Quaternion.Euler 함수는 Vector3 값을 Quaternion 형으로 반환해준다
        // Transform의 rotation은 Quaternion 형 이다.
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // 전에는 Transform.position이 바로 대입되었으나
        // 최근에는 접근제어에 의해서 다음과 같이 대입한다.

        //현재 타겟 트렌스폼의 포지션을 저장
        Vector3 tempDis = target.position;
        // 현재 포지션으로 부터.. 
        // 현재 로테이션에 Vector3.forward 즉 (0,0,1) * distance
        // 를 곱한 값(즉 이 값만큼 떨어진 크기의 벡터) 만큼 떨어진 벡터를 셋팅 
        tempDis -= currentRotation * Vector3.forward * distance;

        // tempDis의 y 값을 currentHeight로 셋팅
        tempDis.y = currentHeight;
        //게임오브젝트의 Transform 컴포넌트의 position을 tempDis로 셋팅
        transform.position = tempDis;

        // LookAt 함수는 인자로 전달된 Transform 값을 참조하여 
        // 대상체를 바라봄
        transform.LookAt(target);
    }
}