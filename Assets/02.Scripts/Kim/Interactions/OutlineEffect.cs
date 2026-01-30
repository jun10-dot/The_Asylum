using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outline
{
    /// <summary>
    /// 아이템을 바라보고 있을 시 외곽선 효과를 부여하는 클래스 
    /// </summary>
    [Serializable]
    public class OutlineEffect
    {
        public Material outline; // 외곽선 효과용 머티리얼
        public Material saveOutline; // 이전 상태 저장
        public List<Material> materialList = new List<Material>(); // 머티리얼 변경을 위한 리스트

        // Project창 리소스 폴더에 외곽선 쉐이더를 로드하고 머티리얼 초기화
        public void Init()
        {
            Shader shader = Resources.Load<Shader>("ItemOutlineShader");
            if (shader != null)
                outline = new Material(shader);
        }
        
        // 아이템의 MeshRenderer에 외곽선 머티리얼 추가
        public void SeletedOutline(MeshRenderer render)
        {
            materialList.Clear(); // 기존 머티리얼 리스트 정리
            materialList.AddRange(render.sharedMaterials); // 기존 오브젝트가 가진 머티리얼 목록 가져옴(원본 참조)
            materialList.Add(outline); // 외곽선 머티리얼을 목록 맨 뒤에 추가

            render.materials = materialList.ToArray(); // 변경된 목록을 적용
        }

        // 아이템 머티리얼 목록에 추가되었던 외곽선머티리얼 제거
        public void NoneSeletedOutline(MeshRenderer render)
        {
            materialList.Clear(); // 기존 머티리얼 리스트 정리
            materialList.AddRange(render.sharedMaterials); // 현재 적용된 머티리얼 목록 가져옴(원본 참조)
            // 쉐이더 이름을 비교하여 외곽선 머티리얼만 제거
            materialList.RemoveAll(mat => mat.shader.name == outline.shader.name);
           
            render.materials = materialList.ToArray(); // 변경된 목록을 다시 적용
        }
    }
}