using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//이 선언이 있어야 UI관련 컴포넌트를 연결 및 사용 가능
using UnityEngine.UI;

//현재 스크립트에서 넓게는 현재 게임오브젝트에서 반드시 필요로하는 컴포넌트를 Attribute로 명시하여 해당 컴포넌트의 자동 생성 및 삭제되는 것을 막는다.
[RequireComponent(typeof(AudioSource))] //지워도 스크립트 던지면 다시 고대로 넣을 수 있음
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // 싱글톤

    // 🔹 배경음악용 AudioSource
    private AudioSource bgmSource;

    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    //사운드 Volume 설정 변수
    // 볼륨 & 뮤트 설정
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public bool isBgmMute = false;
    public bool isSfxMute = false;

    
    // UI 연결
    public Slider bgmSl;
    public Slider sfxSl;

    public Button bgmMuteBtn;
    public Button sfxMuteBtn;

    public Image bgmMuteBtnImg;
    public Image sfxMuteBtnImg;

    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    // 사운드 UI 관련
    //Sound 오브젝트 연결 변수 
    public GameObject Sound;
    //Sound Ui버튼 오브젝트 연결 변수 
    public GameObject PlaySoundBtn;

    private List<AudioSource> sfxSources = new List<AudioSource>();

    //AudioSource audio;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;         // 현재 SoundManager를 Instance로 등록
            DontDestroyOnLoad(this.gameObject); // 씬 전환에도 유지
            bgmSource = GetComponent<AudioSource>();
            LoadData();
        }
        else
        {
            Destroy(gameObject); // 이미 Instance가 있으면 중복 제거
        }
        //DontDestroyOnLoad(this.gameObject);
        //bgmSource = GetComponent<AudioSource>();
        ////게임 로드. 원래는 스타트에 넣는게 좋음. 
        //LoadData();
    }

    // Start is called before the first frame update
    void Start()
    {
        // UI 초기값 설정
        if (bgmSl != null)
        {
            bgmSl.value = bgmVolume;
        }
        if (sfxSl != null)
        {
            sfxSl.value = sfxVolume;
        }
        // 뮤트 버튼 이미지 업데이트
        UpdateMuteButtonVisual();

        // 버튼 클릭 시 뮤트 전환 이벤트 연결
        bgmMuteBtn.onClick.AddListener(ToggleBgmMute);
        sfxMuteBtn.onClick.AddListener(ToggleSfxMute);

        PlaySoundBtn.SetActive(true); //비활성화 되어 있던 사운드 유아이 실행 버튼이 활성화 되어져 보일 거이다.
        AudioSet();
    }

    //스테이지 시작시 호출되는 함수
    // 배경음악 재생
    public void PlayBackground(int stage)
    {
        if (bgmClips.Length == 0 || stage - 1 >= bgmClips.Length)
            return;

        // AudioSource의 사운드 연결
        bgmSource.clip = bgmClips[stage - 1];
        // AudioSource 셋팅
        AudioSet();
        bgmSource.loop = true;
        // 사운드 플레이. Mute 설정시 사운드 안나옴
        bgmSource.Play();
    }

    // 효과음 재생
    public void PlayEffct(Vector3 pos, AudioClip sfx)
    {
        //Mute 옵션 설정시 이 함수를 바로 빠져나가자.
        if (isSfxMute || sfx == null)
        {
            return;
        }

        //게임 오브젝트의 동적 생성하자.
        GameObject _soundObj = new GameObject("sfx");
        //사운드 발생 위치 지정하자. //로컬 포지션 넣는게 좋다!!(월드 위치)
        _soundObj.transform.position = pos;
        //생성한 게임오브젝트에 AudioSource 컴포넌트를 추가하자.
        AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();
        //AudioSource 속성을 설정 
        //사운드 파일 연결하자.
        _audioSource.clip = sfx;
        //설정되어있는 볼륨을 적용시키자. 즉 soundVolume 으로 게임전체 사운드 볼륨 조절.
        _audioSource.volume = sfxVolume;
        // 뮤트 상태 적용
        _audioSource.mute = isSfxMute;
        //사운드 3d 셋팅에 최소 범위를 설정하자.
        _audioSource.minDistance = 15.0f;
        //사운드 3d 셋팅에 최대 범위를 설정하자.
        _audioSource.maxDistance = 30.0f;

        //사운드를 실행시키자.
        _audioSource.Play();

        //모든 사운드가 플레이 종료되면 동적 생성된 게임오브젝트 삭제하자.
        Destroy(_soundObj, sfx.length + 0.02f);


        //GameObject sfxObj = new GameObject("SFX_" + sfx.name);
        //sfxObj.transform.position = pos;

        //AudioSource src = sfxObj.AddComponent<AudioSource>();
        //src.clip = sfx;
        //src.volume = sfxVolume;     // ← 현재 슬라이더 볼륨 반영
        //src.mute = isSfxMute;       // ← 뮤트 상태 반영
        //src.spatialBlend = 0f;      // 2D 사운드로 (필요시 3D로 바꿔도 됨)
        //src.Play();
        //// 🔹 사운드 끝나면 제거
        //Destroy(sfxObj, sfx.length + 0.2f);

    }

    public void PlayButtonSfx()
    {
        if (sfxClips.Length == 0) return;

        // 씬에 있는 SoundManager 싱글톤 참조
        SoundManager sm = FindObjectOfType<SoundManager>();
        if (sm == null) return;

        // UI 버튼 소리도 3D로 재생
        Vector3 playPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        sm.PlayEffct(playPos, sm.sfxClips[0]);
    }

    // 배경음악 볼륨 조절
    public void SetBgmVolume()
    {
        bgmVolume = bgmSl.value;
        AudioSet();
        SaveData();
    }

    // 효과음 볼륨 조절
    public void SetSfxVolume()
    {
        sfxVolume = sfxSl.value;
        foreach (var src in sfxSources)
        {
            if (src != null)
                src.volume = sfxVolume;
        }
        SaveData();
    }

    // 배경음악 뮤트 토글
    public void ToggleBgmMute()
    {
        isBgmMute = !isBgmMute; // 뮤트 상태 전환
        AudioSet();
        UpdateMuteButtonVisual();
        SaveData();
    }

    // 효과음 뮤트 토글
    public void ToggleSfxMute()
    {
        isSfxMute = !isSfxMute;
        foreach (var src in sfxSources)
        {
            if (src != null)
                src.mute = isSfxMute;
        }
        UpdateMuteButtonVisual();
        SaveData();
    }

    //AudioSource 셋팅 (사운드 UI에서 설정 한 값의 적용 )
    void AudioSet() //레퍼런스 빼고 어웨이크에 넣으면 됨!!!
    {
        //AudioSource의 볼륨 셋팅 
        bgmSource.volume = bgmVolume;
        //AudioSource의 Mute 셋팅 
        bgmSource.mute = isBgmMute;
    }

    // 버튼 이미지 업데이트
    void UpdateMuteButtonVisual()
    {
        if (bgmMuteBtnImg != null)
            bgmMuteBtnImg.sprite = isBgmMute ? musicOffSprite : musicOnSprite;

        if (sfxMuteBtnImg != null)
            sfxMuteBtnImg.sprite = isSfxMute ? musicOffSprite : musicOnSprite;
    }

    //사운드 UI 창 오픈 
    public void SoundUiOpen()
    {
        // 사운드 UI 활성화 
        Sound.SetActive(true);
        // 사운드 UI 오픈 버튼 비활성화 
        PlaySoundBtn.SetActive(false);

        // 마우스 커서 보이기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //사운드 UI 창 닫음
    public void SoundUiClose()
    {
        // 사운드 UI 비 활성화 
        Sound.SetActive(false);
        // 사운드 UI 오픈 버튼 활성화 
        PlaySoundBtn.SetActive(true);

        // 마우스 커서 숨기기
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //게임 세이브 
        SaveData();
    }

//게임 사운드데이타 저장 
    public void SaveData()
    {
        //PlayerPrefs 클래스 내부 함수에는 bool형을 저장해주는 함수가 없다.
        //bool형 데이타는 형변환을 해야  PlayerPrefs.SetInt() 함수를 사용가능
        PlayerPrefs.SetFloat("BGM_VOLUME", bgmVolume);
        PlayerPrefs.SetFloat("SFX_VOLUME", sfxVolume);
        PlayerPrefs.SetInt("BGM_MUTE", System.Convert.ToInt32(isBgmMute));
        PlayerPrefs.SetInt("SFX_MUTE", System.Convert.ToInt32(isSfxMute));
        PlayerPrefs.Save();
    }




    // Update is called once per frame
    void Update()
    {
        // M 키로 사운드 UI 열고/닫기 토글
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (Sound.activeSelf)
                SoundUiClose();
            else
                SoundUiOpen();
        }

        // ESC 키로 UI 닫기 (열려있을 때만)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Sound.activeSelf)
                SoundUiClose();
        }
    }


    

    //게임 사운드데이타 불러오기 
    //바로 사운드 UI 슬라이드 와 토글에 적용하자.
    public void LoadData()
    {
        //int 형 데이타는 bool 형으로 형변환.
        bgmVolume = PlayerPrefs.GetFloat("BGM_VOLUME", 1.0f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1.0f);
        isBgmMute = System.Convert.ToBoolean(PlayerPrefs.GetInt("BGM_MUTE", 0));
        isSfxMute = System.Convert.ToBoolean(PlayerPrefs.GetInt("SFX_MUTE", 0));

        // 첫 실행 시 기본값 저장
        int isSave = PlayerPrefs.GetInt("ISSAVE", 0);
        if (isSave == 0)
        {
            bgmVolume = 1.0f;
            sfxVolume = 1.0f;
            isBgmMute = false;
            isSfxMute = false;

            PlayerPrefs.SetInt("ISSAVE", 1);
            SaveData();
        }

        // 슬라이더에 즉시 반영
        if (bgmSl != null)
            bgmSl.value = bgmVolume;
        if (sfxSl != null)
            sfxSl.value = sfxVolume;

        AudioSet();
        UpdateMuteButtonVisual();
    }





}
