using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public class AngelFSM_NavMesh : MonoBehaviour
{
    //상태 머신(정지,추격)
    public enum AngelState { Frozen, Stalking }

    //보는사람 카메라 Transform 등록
    public List<Transform> observers = new List<Transform>();

    //가림막체크 레이어
    public LayerMask obstructionMask = ~0;
    //추적 시작 및 종료 거리
    public float StartDistance = 9f;
    public float StopDistance = 16f; 
    //관찰관련 시야
    public float sightDistance = 19f;
    [Range(0f, 1f)] public float sightDot = 0.6f;
    public float unseenGrace = 0.15f;

    //이동/경로
    public float moveSpeed = 1.8f;
    public float stoppingDistance = 0.2f;
    public float repathInterval = 0.15f;

    //문,벽 멈출시 Idle 연출 시간 
    [SerializeField] private float safeRoomGrace = 3.0f;
    private float blockedSince = -1f;

    //한번 텔포후 잠금
    [SerializeField] private bool safeRoomLock = false;
    //잠금 상태에서도 주기적으로 길이 열렸는지 탐색 할 간격
    [SerializeField] private float safeRoomUnlockProbeInterval = 0.5f;
    private float nextProbeTime = 0f;

    //애니메이션
    public Animator animator;
    private string animIdle = "Idle";
    private string animWalk = "Walk";
    static readonly int HashWalk = Animator.StringToHash("Walk");

    //사운드
 
    public AudioSource sfx;        // 3D AudioSource
    public AudioClip sfxFreeze;    // 멈출 때
    public AudioClip sfxStep;      // 발소리

    //발소리 최소 간격(초). 속도에 따라 곱해짐
    public float footstepBaseInterval = 0.9f; // 기본 템포
    //속도 보정(값이 작을수록 빨리 찍음)                                                                                          `1` 11                                                                                                                                                                                                  ``  1111111111111111111
    public float footstepSpeedScale = 0.5f;

    // 내부
    NavMeshAgent agent;
    AngelState state;
    float lastSeenTime;
    float lastPathTime;

    // 발소리 쿨다운
    float nextStepTime;

    //멀티플레이어용
    PhotonView pv;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        pv = GetComponent<PhotonView>();

        // 마스터만 AI 판단/경로계산
        if (pv && !PhotonNetwork.isMasterClient)
        {
            if (agent) agent.enabled = false;
            enabled = false;
            return;
        }

        // NavMeshAgent 세팅
        agent.stoppingDistance = stoppingDistance;
        agent.speed = moveSpeed;
        agent.updateRotation = true;

        // 관찰자 기본 등록(비상용)
        if (observers.Count == 0 && Camera.main) observers.Add(Camera.main.transform);

        ChangeState(AngelState.Frozen);
        SetWalk(false);

        // 발소리 초기화
        nextStepTime = Time.time + 0.25f;
    }

    void Update()
    {
        // 보임 판정
        bool seen = IsSeenByAnyObserver();
        if (seen) lastSeenTime = Time.time;

        // 상태 머신
        switch (state)
        {
            case AngelState.Frozen: TickFrozen(seen); break;
            case AngelState.Stalking: TickStalking(seen); break;
        }

        // 애니메이션 워크 스위치
        if (animator)
        {
            bool moving = agent.enabled && !agent.isStopped && agent.velocity.sqrMagnitude > 0.02f;
            SetWalk(state == AngelState.Stalking && moving);
        }
    }

    //상태 전환
    void ChangeState(AngelState next)
    {
        state = next;

        switch (state)
        {
            case AngelState.Frozen:
                agent.isStopped = true;
                agent.ResetPath();
                // 정지 사운드 재생(모든 클라에 전파)
                PlayFreezeSfxAll();
                if (animator && !string.IsNullOrEmpty(animIdle)) animator.Play(animIdle, 0, 0f);
                SetWalk(false);
                break;

            case AngelState.Stalking:
                agent.isStopped = false;
                if (animator && !string.IsNullOrEmpty(animWalk)) animator.Play(animWalk, 0, 0f);
                SetWalk(true);
                break;
        }

        // 상태 동기화(애니메이터 Walk Bool만 전송)
        if (pv && PhotonNetwork.isMasterClient && pv.viewID != 0)
            pv.RPC("RPC_Setstate", PhotonTargets.Others, (int)state, animator ? animator.GetBool(HashWalk) : false);
    }

    [PunRPC]
    void RPC_Setstate(int s, bool walkOn)
    {
        if (animator) animator.SetBool(HashWalk, walkOn);
    }

    //보면 정지
    void TickFrozen(bool seen)
    {
        if (!seen && Time.time - lastSeenTime >= unseenGrace)
        {
            Transform target = GetNearestObserver();
            if (!target) return;

            float dist = FlatDistance(transform.position, target.position);
            if (dist <= StartDistance)
                ChangeState(AngelState.Stalking);
        }
    }

    //안보이면 추격 + 발소리
    void TickStalking(bool seen)
    {
        // 1) 쳐다보면 정지
        if (seen)
        {
            ChangeState(AngelState.Frozen);
            return;
        }

        // 2) 타겟(플레이어 카메라) 찾기
        Transform target = GetNearestObserver();
        if (!target) return;

        // 3) 먼저 경로부터 검사 (문/벽 때문에 막혔는지)
        var path = new NavMeshPath();
        agent.CalculatePath(target.position, path);

        bool pathBlocked = (path.status == NavMeshPathStatus.PathPartial ||
                            path.status == NavMeshPathStatus.PathInvalid);

        if (pathBlocked)
        {
            // 처음 막힌 시간 기록
            if (blockedSince < 0f)
                blockedSince = Time.time;

            // 문 앞에서 멈칫
            agent.isStopped = true;
            agent.ResetPath();

            // safeRoomGrace 초 동안 계속 막혀 있으면 텔포 시도
            if (Time.time - blockedSince >= safeRoomGrace)
            {
                var reappear = GetComponent<AngelReappear>();
                if (reappear != null)
                {
                    reappear.TryReappearNow(target);  // 여기서 뒤 텔포 / 랜덤 텔포
                }

                // 한 번 텔포한 뒤에는 일단 Frozen 상태로
                ChangeState(AngelState.Frozen);
                blockedSince = -1f;  // 타이머 리셋
            }

            // 경로가 막혀 있으면 여기서 끝 (아래 로직 진행 X)
            return;
        }
        else
        {
            // 경로가 다시 열렸으면 타이머 리셋
            blockedSince = -1f;
        }

        // 4) 이제 거리 기반 로직 처리
        float dist = FlatDistance(transform.position, target.position);

        // 너무 멀어지면 추적 중단 (이제는 "길이 열려 있을 때만" 적용됨)
        if (dist >= StopDistance)
        {
            ChangeState(AngelState.Frozen);
            return;
        }

        // 5) 정상 추적 로직
        if (Time.time - lastPathTime >= repathInterval)
        {
            agent.speed = moveSpeed;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            lastPathTime = Time.time;
        }
    


        //발소리 마스터에서만 판정
        bool moving = agent.enabled && !agent.isStopped && agent.velocity.sqrMagnitude > 0.05f;
        if (moving && Time.time >= nextStepTime)
        {
            PlayFootstepAll();
            float interval = footstepBaseInterval / Mathf.Max(0.1f, (moveSpeed * footstepSpeedScale));
            nextStepTime = Time.time + interval;
        }
    }

    //시야 판정
    bool IsSeenByAnyObserver()
    {
        if (observers == null || observers.Count == 0) return false;

        Vector3 myCenter = GetCenter();
        for (int i = 0; i < observers.Count; i++)
        {
            var obs = observers[i];
            if (!obs) continue;

            Vector3 toAngel = myCenter - obs.position;
            float dist = toAngel.magnitude;
            if (dist > sightDistance) continue;

            Vector3 dir = toAngel.normalized;
            float dot = Vector3.Dot(obs.forward, dir);
            if (dot < sightDot) continue;

            if (!Physics.Raycast(obs.position, dir, dist, obstructionMask))
                return true;
        }
        return false;
    }

    //가까운 타겟 찾기
    Transform GetNearestObserver()
    {
        Transform best = null;
        float bestSqr = float.MaxValue;
        Vector3 p = transform.position;
        for (int i = 0; i < observers.Count; i++)
        {
            var t = observers[i];
            if (!t) continue;
            float sq = (t.position - p).sqrMagnitude;
            if (sq < bestSqr) { bestSqr = sq; best = t; }
        }
        return best;
    }

    //1층 2층 바닥면 기준 거리 계산
    float FlatDistance(Vector3 a, Vector3 b) { a = Flat(a); b = Flat(b); return Vector3.Distance(a, b); }
    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

    //몬스터 시야/레이캐스트 기준 위치
    Vector3 GetCenter()
    {
        var col = GetComponent<CapsuleCollider>();
        if (col) return transform.TransformPoint(col.center + Vector3.up * (col.height * 0.25f));
        return transform.position + Vector3.up * 1.0f;
    }

    void SetWalk(bool on)
    {
        if (animator) animator.SetBool(HashWalk, on);
    }

    //SFX 재생(로컬)동기화
    void PlayLocal(AudioClip clip)
    {
        if (sfx && clip) sfx.PlayOneShot(clip);
    }

    void PlayFreezeSfxAll()
    {
        // 마스터 로컬 재생
        PlayLocal(sfxFreeze);
        // 게스트에게도 재생 지시
        if (pv && PhotonNetwork.isMasterClient && pv.viewID != 0)
            pv.RPC("RPC_PlaySfx", PhotonTargets.Others, 0); // 0 = Freeze
    }

    void PlayFootstepAll()
    {
        PlayLocal(sfxStep);
        if (pv && PhotonNetwork.isMasterClient && pv.viewID != 0)
            pv.RPC("RPC_PlaySfx", PhotonTargets.Others, 1); // 1 = Step
    }

    [PunRPC]
    void RPC_PlaySfx(int kind)
    {
        switch (kind)
        {
            case 0: PlayLocal(sfxFreeze); break;
            case 1: PlayLocal(sfxStep); break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
#endif

    public void AddObserver(Transform t) { if (t && !observers.Contains(t)) observers.Add(t); }
    public void RemoveObserver(Transform t) { if (t) observers.Remove(t); }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
          PhotonView pv = other.GetComponent<PhotonView>();
         
           if (pv != null && pv.isMine)
           {
               ChangeState(AngelState.Frozen);
               PlayerDeath.Instance.DeadScene(other.transform);
           }  
        }
    }
}