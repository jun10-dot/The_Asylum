using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;



//강당 우는 천사 리스폰
public class SpawnAngelOnEnter : Photon.MonoBehaviour
{
    //리소스 경로 가져오기
    //public string resourcesPath = "01.Scriptes/Resources/WeepingAngels";
    
    //프리팹 이름
    public string prefabPath = "Weeping Angels";
    //스폰 위치
    public Transform spawnPoint;

    //추가 관찰자(멀티/2P 카메라)
    public Transform[] extraObservers;

    //한번만 소환


    bool spawned;


    private void OnTriggerEnter(Collider other)
    {
        if (spawned) return;
        if (!other.CompareTag("Player")) return;

        // 멀티면 마스터만 스폰
        if (!PhotonNetwork.isMasterClient) return;

   

        // Photon 사용 시
        

        // 싱글/테스트
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (!prefab)
        {
            Debug.LogError($"[SpawnAngelOnEnter] Resources/{prefabPath} 프리팹을 찾을 수 없습니다.");
            return;
        }
        var go = PhotonNetwork.Instantiate(prefabPath, GetSpawnPos(), GetSpawnRot(), 0);


        // 관찰자(카메라) 등록: 트리거 들어온 플레이어의 카메라를 우선
        var fsm = go.GetComponent<AngelFSM_NavMesh>();
        if (fsm)
        {
            var cam = other.GetComponentInChildren<Camera>();
            if (cam) fsm.AddObserver(cam.transform);

            if (extraObservers != null)
                foreach (var t in extraObservers)
                    if (t) fsm.AddObserver(t);
        }

        if (PhotonNetwork.connected && PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
            return;  // 마스터가 아닌 플레이어는 소환하지 않음
        spawned = true;
    }

    Vector3 GetSpawnPos() => (spawnPoint ? spawnPoint.position : transform.position);
    Quaternion GetSpawnRot() => (spawnPoint ? spawnPoint.rotation : transform.rotation);
}