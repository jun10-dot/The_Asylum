using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteItem : InteractableItem
{
    [Header("쪽지 보기 설정")]
    public GameObject noteUIPrefab; // UI 프리팹
    public Sprite noteSprite;       // 쪽지 이미지 (예: 스크린샷, 텍스처 등)


    protected override void Interact()
    {
        //if (InteractableItem.isInteracting) return;

        //InteractableItem.isInteracting = true;

        // Canvas 찾기 (씬에 반드시 존재)
        Canvas canvas = GameObject.Find("HintUi").GetComponent<Canvas>();

        
        GameObject ui = Instantiate(noteUIPrefab, canvas.transform);
        
        NoteViewerUI viewer = ui.GetComponent<NoteViewerUI>();

        if (viewer != null)
        {
            viewer.SetNoteSprite(noteSprite);
        }
        else
        {
            Debug.LogWarning("NoteViewerUI 스크립트를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    Interact();
                }
            }
        }
    }
}
