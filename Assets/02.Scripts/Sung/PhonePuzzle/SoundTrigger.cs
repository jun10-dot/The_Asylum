using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [Header("사운드 매니저에서 재생할 효과음 인덱스")]
    [Tooltip("SoundManager의 sfxClips 배열에서 사용할 인덱스 번호 (0부터 시작)")]
    public int sfxIndex = 0;

    [Header("플레이어")]
    public string targetTag = "Player";

    [Header("재생 옵션")]
    [Tooltip("체크하면 한 번만 재생됩니다. 체크 해제 시, 닿을 때마다 반복 재생됩니다.")]
    public bool playOnce = true;

    private bool hasPlayed = false; // 한 번만 재생되게 하려면 true로 유지

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // 🔹 "한 번만 재생"이 아닐 경우는 항상 재생되도록
            if (!hasPlayed || !playOnce)
            {
                SoundManager sm = FindObjectOfType<SoundManager>();
                if (sm != null && sm.sfxClips != null && sm.sfxClips.Length > sfxIndex)
                {
                    sm.PlayEffct(transform.position, sm.sfxClips[sfxIndex]);
                }
                else
                {
                    Debug.LogWarning("Error!");
                }

                hasPlayed = true;
            }
        }
    }
}
