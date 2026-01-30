using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhonePuzzleUI : MonoBehaviour
{

    [Header("버튼 부모 오브젝트")]
    public Transform buttonParent; // 버튼들이 들어있는 부모 (예: PhonePanel)

    [Header("아이템 관련")]
    public GameObject itemToSpawn;
    public Transform itemSpawnPoint;

    [HideInInspector] public PhonePuzzle linkedPuzzle; // PhonePuzzle 연결용

    private List<int> correctCode = new List<int> { 1, 8, 4, 2, 5, 9, 7, 3 };
    private List<int> inputSequence = new List<int>();
    private bool puzzleCompleted = false;

    

    // Start is called before the first frame update
    void Start()
    {
        // 버튼 자동 인식
        Button[] buttons = buttonParent.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            string name = btn.gameObject.name; // 예: "Button_3"
            if (int.TryParse(name.Replace("btn_", ""), out int num))
            {
                btn.onClick.AddListener(() => OnNumberPressed(num));
            }
            else
            {
                Debug.LogWarning($"{btn.name} 이름에서 숫자를 찾을 수 없습니다!");
            }
        }

        // 퍼즐 열릴 때 마우스 커서 보이게
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    public void OnNumberPressed(int num)
    {
        if (puzzleCompleted) return;

        inputSequence.Add(num);
        Debug.Log($"입력: {num}");

        for (int i = 0; i < inputSequence.Count; i++)
        {
            if (inputSequence[i] != correctCode[i])
            {
                Debug.Log("다시 시도");
                inputSequence.Clear();
                return;
            }
        }

        if (inputSequence.Count == correctCode.Count)
        {
            PuzzleSuccess();
        }
    }

    void PuzzleSuccess()
    {
        puzzleCompleted = true;
        Debug.Log("입력 성공!");


        // 여기서는 Photon RPC를 직접 호출하지 않음
        linkedPuzzle.CallPuzzleSolved();

        // 퍼즐 본체에 알리기
        /*
        linkedPuzzle?.OnPuzzleSolved();

        if (itemToSpawn != null && itemSpawnPoint != null)
        {
            Instantiate(itemToSpawn, itemSpawnPoint.position, Quaternion.identity);
        }
        */

        ClosePuzzle(); // 퍼즐 자동 닫기
    }

    void ClosePuzzle()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InteractableItem.isInteracting = false;
        Destroy(gameObject);
    }
}
