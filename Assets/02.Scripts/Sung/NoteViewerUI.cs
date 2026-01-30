using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteViewerUI : MonoBehaviour
{

    public Image noteImage; // 쪽지 이미지 (UI)
    public Sprite noteSprite; // 보여줄 쪽지 원본 이미지

    public void SetNoteSprite(Sprite sprite)
    {
        noteSprite = sprite;
        //if (noteImage != null && noteSprite != null)
        //    noteImage.sprite = noteSprite;
        if (noteImage != null && noteSprite != null)
        {
            // 이미지 세팅
            noteImage.sprite = noteSprite;

            // RectTransform 가져오기
            RectTransform rt = noteImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Sprite 원본 픽셀 크기 기준
                float width = noteSprite.rect.width;
                float height = noteSprite.rect.height;

                // 화면 크기 (Canvas 크기 기준)
                Canvas canvas = noteImage.canvas;
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                float maxWidth = canvasRect.rect.width * 0.9f;   // 화면의 90% 최대
                float maxHeight = canvasRect.rect.height * 0.9f; // 화면의 90% 최대

                // 화면보다 크면 비율 맞춰 축소
                float widthRatio = maxWidth / width;
                float heightRatio = maxHeight / height;
                float scale = Mathf.Min(widthRatio, heightRatio, 1f); // 1보다 작으면 축소

                rt.sizeDelta = new Vector2(width * scale, height * scale);
                rt.anchoredPosition = Vector2.zero; // 화면 중앙
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        //// 이미지 세팅
        //if (noteImage != null && noteSprite != null)
        //    noteImage.sprite = noteSprite;

        // 마우스 커서 활성화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        // ESC나 마우스 클릭 시 닫기
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            CloseNote();
        }
    }

    void CloseNote()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InteractableItem.isInteracting = false;
        Destroy(gameObject);
    }
}
