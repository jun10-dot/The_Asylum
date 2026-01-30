using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
/// <summary>
/// 플레이어 사망 처리, 데스 캔버스,비디오 연출 및 멀티플레이어 관전 시스템을 관리하는 싱글톤 클래스
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private Canvas deathCanvas; // 동영상 끝난 후 출력할 데스 캔버스
    //[SerializeField] private Canvas ChatCanvas;
    [SerializeField] private GameObject deathVideo; // 사망 후 데스 비디오 재생
    private static PlayerDeath instance;
    private bool isSpectating; // 현재 관전 중인지 여부
    private bool isSpectatorEnding; // 관전이 종료되었는지 확인
    //private int activePlayerCount;
#region Property
    public static PlayerDeath Instance {get{return instance; } }
    public bool IsSpectatorEnding { get{return isSpectatorEnding;} } 
    //public int ActivePlayerCount {get {return activePlayerCount; } }
#endregion
  
    void Awake()
    {
        if(instance == null)
        instance = this;
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 게임 시작 시 마우스 커서 잠금
    }
   
    // 플레이어 사망 시 호출
    // 컨트롤 중지, 카메라 전환, 사망 연출 및 네트워크 동기화
    public void DeadScene(Transform player)
    {
        PhotonPlayerCtrl playerCtrl = player.GetComponent<PhotonPlayerCtrl>(); 
        PhotonView playerPv = player.GetComponent<PhotonView>();

        playerCtrl.StopController();  //플레이어 컨트롤 모두 끄기
        playerCtrl.OffCamera(); // 플레이어 카메라 끄기

        ShowDeath(); // 데스 비디오 재생
        // 모든 클라이언트들에게 해당 플레이어 사망 알림
        playerPv.RPC("DisablePlayer", PhotonTargets.AllBuffered); 
        DeathLength(); // 비디오 종료 이벤트 구독
    }

    // 모든 UI를 숨기고 데스 비디오 활성화
    public void ShowDeath()
    {
        foreach (Canvas c in FindObjectsOfType<Canvas>())
            c.gameObject.SetActive(false);

        deathVideo.SetActive(true);
    }

    // 비디오 재생 완료 시점 파악하기 위한 이벤트 구독
    public void DeathLength()
    {
       VideoPlayer v = deathVideo.GetComponent<VideoPlayer>();
        v.loopPointReached += VideoEnd; // 비디오가 끝까지 재생되면 VideoEnd 실행
    }

    // 비디오 재생 종료 후 처리
    // 비디오 끄고 데스 캔버스 출력
    void VideoEnd(VideoPlayer p)
    {
        deathVideo.SetActive(false);
        DeathCanvas(true);
    }

    void DeathCanvas(bool isDeathUIActive)
    {
        deathCanvas.gameObject.SetActive(isDeathUIActive);
    }

    // [관전 시스템] 생존해 있는 다른 플레이어를 찾아 시야를 공유
    public void EnterSpectatorMode()
    {
        if (isSpectating) return;

        DeathCanvas(false);

        // 하이라이키에 활성화 되어 있고, 내 캐릭터가 아닌 다른 캐릭터 리스트 추출
        var alivePlayer = FindObjectsOfType<PhotonPlayerCtrl>().Where
            (p => p.gameObject.activeInHierarchy && !p.GetComponent<PhotonView>().isMine)
            .ToList();
        
        // 생존자가 없다면 메인 화면으로 전환
        if (alivePlayer.Count == 0)
        {
            SceneManager.LoadScene("Title");
            return;
        }

        // 첫 번째 생존자의 카메라 정보를 가져와 관전 시작
        PhotonPlayerCtrl target = alivePlayer[0];
        target.Spectate(target); // 대상의 1인칭 시점 활성화
        //ChatCanvas.gameObject.SetActive(true);
        isSpectating = true;
    }

    // 관전 중이던 대상이 사망했을 때 호출 (PhotonPlayerCtrl의 DisablePlayer에서 호출)
    public void OnSpectatedTargetDead()
    {
        // 관전자 상태가 아니면 무시
        if (!isSpectating) return;

        // 관전 종료
        isSpectating = false;
        isSpectatorEnding = true;

        // 관전하던 대상마저 죽으면 데스 캔버스만 출력
        DeathCanvas(true);
    }

}
