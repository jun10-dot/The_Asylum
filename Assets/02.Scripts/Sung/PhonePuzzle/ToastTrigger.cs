using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastTrigger : MonoBehaviour
{
    [SerializeField, TextArea] private string message = "아이템을 획득했습니다!";

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 닿았을 때만 작동
        if (other.CompareTag("Player"))
        {
            // 토스트 메시지 호출
            ToastManager.Instance.ShowToast(message);

            // 필요하면 오브젝트 제거 (예: 아이템 습득 후 사라짐)
            //Destroy(gameObject);
        }
    }
}
