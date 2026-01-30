using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    public AudioClip clickSound;               // 각 버튼마다 다른 소리 가능
    private static AudioSource uiAudioSource;  // 모든 버튼이 공유

    void Start()
    {
        // Canvas에 붙은 AudioSource를 찾아서 공유
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        uiAudioSource = canvas.GetComponent<AudioSource>();
        if (uiAudioSource == null) return;

        // 버튼 클릭 시 함수 연결
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        uiAudioSource.PlayOneShot(clickSound);
    }
}
