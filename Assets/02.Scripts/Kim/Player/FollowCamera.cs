using System;
using UnityEngine;

/// <summary>
/// 1인칭 시점 마우스 회전 제어 클래스
/// </summary>
namespace FollowCamera
{
    [Serializable]
    public class MouseMove
    {
        private const float MinimumX = -90f; // 상단 최대 회전각
        private const float MaximumX = 90f; // 하단 최대 회전각
        private Quaternion characterTargetRot; // 캐릭터 본체(좌우) 회전값 저장
        private Quaternion cameraTargetRot; // 카메라(상하) 회전값 저장
        
        // 현재 캐릭터와 카메라의 초기 회전값을 기준값으로 설정
        public void Init(Transform character, Transform camera)
        {
            characterTargetRot = character.localRotation;
            cameraTargetRot = camera.localRotation;
        }
        
        // 마우스 이동에 따라 캐릭터와 카메라를 회전시킴
        public void LookRotation(Transform character, Transform camera)
        {
            // 마우스 이동량 입력을 받음
            float yRot = Input.GetAxis("Mouse X") * 2f;
            float xRot = Input.GetAxis("Mouse Y") * 2f;
           
            // 쿼터니언 곱셈을 통해 회전 누적 처리
            characterTargetRot *= Quaternion.Euler (0f, yRot, 0f); // 좌우 회전: 캐릭터 본체(자식 오브젝트인 카메라도) 회전
            cameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f); // 상하 회전 : 카메라만 회전

            // 수직 회전각이 심하게 꺾여 화면이 뒤집히는 것을 방지
            cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

            // 최종 계산된 회전값을 Transform에 적용
            character.localRotation = characterTargetRot;
            camera.localRotation = cameraTargetRot;      
        }

        // 수직 회전각 제한 함수
        // 각도가 90도를 넘어 루프되는 현상을 방지하기 위해 탄젠트 반각 공식 사용
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            // 쿼터니언 성분 정규화
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2f * Mathf.Rad2Deg * Mathf.Atan (q.x); 
            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX); // 오일러 각도로 변환하여 수직 회전각 제한

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX); // 제한된 회전각을 다시 쿼터니언 성분으로 복원

            return q;
        }
    }
}