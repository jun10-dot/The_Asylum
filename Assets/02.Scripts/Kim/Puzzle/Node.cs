
using System.Collections.Generic;
using UnityEngine;

// 노드의 색상을 정의하는 열거형
public enum ColorsType
{
    Red,
    Yellow,
    Green,
    Blue,
    Cyan,
    Gray,
    Magenta,
    White,
    Black
}

/// <summary>
/// 개별 퍼즐 노드의 기능을 관리하는 클래스 
/// </summary>
public class Node : MonoBehaviour, IHoverable
{
    private Renderer ren; // 노드의 렌더러 참조
    private ColorsType colors; // 현재 설정된 색상 타입
    public List<Color> colorsList = new List<Color>(); // 색상 리스트
    public bool IsQuizNode {get; set;} = false; // 문제용 노드인지 여부(클릭 이벤트 처리X)
#region Property
    public Renderer MeshRen { get { return ren; } set { ren = value; } }
#endregion

    // 색상 타입에 따라 노드의 머티리얼 색상을 변경
    public void SetColor(ColorsType colorType)
    {
        if (ren == null)
            ren = GetComponent<Renderer>();

        Color color = Color.red; // 기본값

        // 열거형 타입에 맞는 실제 Color 값 매칭
        switch(colorType)
        {
            case ColorsType.Red:
                color = Color.red;
                break;
            case ColorsType.Yellow:
                color = Color.yellow;
                break;
            case ColorsType.Green:
                color = Color.green;
                break;
            case ColorsType.Blue:
                color = Color.blue;
                break;
            case ColorsType.Cyan:
                color = Color.cyan;
                break;
            case ColorsType.Gray:
                color = Color.gray;
                break;
            case ColorsType.Magenta:
                color = Color.magenta;
                break;
            case ColorsType.White:
                color = Color.white;
                break;
            case ColorsType.Black:
                color = Color.black;
                break;
        }

        colors = colorType;
        
        // 프리팹 원본이 바뀌는 것을 방지하기 위해 복사본으로 만들어 적용
        ren.material = new Material(ren.sharedMaterial); 
        ren.material.color = color;
    }

    // 플레이어가 노드 위에 마우스를 올리고 클릭했을 때 호출
    public void OnHover()
    {
        if(IsQuizNode) return; // 문제용 노드는 클릭 판정에서 제외
        if (Input.GetMouseButtonDown(0))
        {
            // 멀티 플레이 모드 : Master이고 인원이 2명 이상일 때 클릭 처리 
            if(PhotonNetwork.isMasterClient && PhotonNetwork.playerList.Length >=2)
            {
               MultiColorNodeManager.Instance.CheckClicked(ren.material.color);
            }
            // 싱글 모드 : 싱글일 때 클릭 처리 
            if(MultiColorNodeManager.Instance.IsSingleMode)
            {
               SingleColorNodeManager.Instance.CheckClicked(ren.material.color);
            }
        }   
    }
    public void OnHoverExit()
    {
    }
}
