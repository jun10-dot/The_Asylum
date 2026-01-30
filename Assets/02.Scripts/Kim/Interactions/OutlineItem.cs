using Outline;
using UnityEngine;

// 인터페이스 기반 외곽선 이벤트가 발생하는 아이템 클래스
// 입력 시 인터페이스를 통해 특정 이벤트 실행 (ex: 입력 시 아이템 수집)
public class OutlineItem : MonoBehaviour, IHoverable
{
    private MeshRenderer ren;
    [SerializeField] private OutlineEffect outline; // 외곽선 효과를 제어하는 스크립트 참조
    private IClickable click;
    void Awake()
    {
        ren = GetComponent<MeshRenderer>();
        if(outline != null)
            outline.Init(); // 외곽선 시스템 초기화
        // 현재 오브젝트에 붙어있는 인터페이스 컴포넌트 캐싱
        click = GetComponent<IClickable>(); 
    }

    // 플레이어의 Raycast와 닿으면 외곽선 효과 발생
    public void OnHover()
    {
        // IClickable와 연동하여, 아이템이 감지되는 동안의 클릭 입력 처리
        if(click != null)
           click.OnClick(); 
        if (ren.materials.Length > 1 || ren == null) return;
        outline.SeletedOutline(ren);
    }

    // Raycast에 벗어나면 외곽선 효과 제거
    public void OnHoverExit()
    {
        if(ren != null)
        outline.NoneSeletedOutline(ren);
    }
}

