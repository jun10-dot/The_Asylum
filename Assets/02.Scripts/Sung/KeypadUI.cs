using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeypadUI : MonoBehaviour
{
    [Header("버튼 부모 오브젝트")]
    public Transform buttonParent; // 버튼들이 들어있는 부모 (예: PhonePanel)

    [Header("UI")]
    public InputField inputField; // 입력 표시
    public Button enterButton;    // 정답 확인 버튼

    [Header("아이템 관련")]
    public GameObject itemToSpawn;
    public Transform itemSpawnPoint;

    [HideInInspector] public KeypadPuzl linkedPuzzle; // 퍼즐 본체와 연결

    private List<int> correctCode = new List<int> { 1, 9, 4, 0 };
    private bool puzzleCompleted = false;



    // Start is called before the first frame update
    void Start()
    {
        // 버튼 자동 인식
        Button[] buttons = buttonParent.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            string name = btn.gameObject.name; // 예: "Button_3"
            if (int.TryParse(name.Replace("Btn_", ""), out int num))
            {
                btn.onClick.AddListener(() => OnNumberPressed(num));
            }
            else
            {
                Debug.LogWarning($"{btn.name} 이름에서 숫자를 찾을 수 없습니다!");
            }
        }

        // Enter 버튼 연결
        if (enterButton != null)
            enterButton.onClick.AddListener(CheckCode);

        // InputField 초기화
        if (inputField != null)
            inputField.text = "";

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

        if (inputField.text.Length >= 9) return; // 최대 9자리 제한

        inputField.text += num.ToString();
    }

    void CheckCode()
    {
        if (puzzleCompleted) return;

        string input = inputField.text;
        if (input.Length != correctCode.Count)
        {
            Debug.Log("자리수가 맞지 않습니다.");
            inputField.text = ""; // 길이 맞지 않아도 엔터 후 초기화
            return;
        }

        for (int i = 0; i < correctCode.Count; i++)
        {
            if ((input[i] - '0') != correctCode[i])
            {
                Debug.Log("오답, 다시 시도");
                inputField.text = "";
                return;
            }
        }

        // 정답일 때
        PuzzleSuccess();
    }

    void PuzzleSuccess()
    {
        puzzleCompleted = true;
        Debug.Log("입력 성공!");

        // 퍼즐 본체에 알리기
        //linkedPuzzle?.OnPuzzleSolved();
        linkedPuzzle.CallSolvePuzzle();

        if (itemToSpawn != null && itemSpawnPoint != null)
        {
            Instantiate(itemToSpawn, itemSpawnPoint.position, Quaternion.identity);
        }

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
