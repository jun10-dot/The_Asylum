using UnityEngine;
using UnityEngine.UIElements;


//랜덤 리스폰 정적 연결
public class AngelSpawnConnector : MonoBehaviour
{
    public AngelReappear angel;          // Hierarchy에서 현재 씬에 소환된 Angel 넣기
    public Transform[] floor1Points;  //1층 스폰

    public Transform[] floor2Points; //2층 스폰
    void Start()
    {
     

        angel.useFloors = true; //1/2층 리스폰 옵션 켜기
        
        angel.floor1Points = floor1Points;
        angel.floor2Points = floor2Points;

        //Debug.Log("[AngelSpawnConnector] 1층/2층 스폰포인트 연결 완료!");

    }
}
