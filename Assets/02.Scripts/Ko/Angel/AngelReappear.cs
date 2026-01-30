using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//중복X
[DisallowMultipleComponent]
public class AngelReappear : MonoBehaviour
{

    //체크 주기
    public float checkInterval = 0.5f;

    //재등장 조건(거리 조절 + 시야에 보이지않을때)
    public float reappearDistance = 20f;

    //플레이어 뒤 스폰
    public float behindOffset = 3.0f;

    //1층 리스폰
    public Transform[] floor1Points;
    //2층 리스폰
    public Transform[] floor2Points;

    //정면 회피 강도(재등장시 플레이어 정면에 나오는걸 막아줌)
    [Range(0f, 0.99f)]
    public float reappearDotLimit = 0.7f;

    //가림막체크 레이어
    public LayerMask reappearObstructionMask = ~0;

    //쿨타임(과한 텔포 방지)
    public float cooldown = 10.0f;

    //1층 ,2층 나눠서 리스폰
    public bool useFloors = true;
    public enum FloorMode { AutoByY, Zones }

    public FloorMode floorMode = FloorMode.AutoByY;

    //이 높이보다 위면 2층으로 간주
    public float floorSplitY = 3.0f;
    //혹시 모를 같은 층 수직 허용 오차
    public float sameFloorBand = 1.5f;

    public int currentFloorAngel = 1; // 런타임 갱신용
    public int currentFloorPlayer = 1;

    //지난번 뒤에서 나왔는지 기록용
    bool lastWasBehind = false;

    AngelFSM_NavMesh fsm;
    NavMeshAgent agent;
    float nextCheck;
    float lastReappearTime;
    //텔포 타입 기록용
    enum TeleportType { None, Behind, RandomFloor }
    TeleportType lastTeleportType = TeleportType.None;
    //멀티플레이어용 포톤뷰
    PhotonView pv;

    void Awake()
    {
        fsm = GetComponent<AngelFSM_NavMesh>();
        agent = GetComponent<NavMeshAgent>();
        pv = GetComponent<PhotonView>();
        if (pv && !PhotonNetwork.isMasterClient)
        { if (agent) agent.enabled = false;
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // NavMesh 안전 가드
        if (!agent || !agent.isOnNavMesh) return;

        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        //플레이어 타겟
        var target = GetNearestObserver();
        if (!target) return;

        // 보이면 텔포 금지
        if (IsSeenNow()) return;

        float dist = FlatDistance(transform.position, target.position);

        //너무 가까우면 텔포 금지
        if (dist < reappearDistance) return;

        //쿨타임 중 금지
        if (Time.time - lastReappearTime < cooldown) return;

        //층 전용 로직
        int angelFloor = GetAngelFloor();
        int playerFloor = GetPlayerFloor(target);
        bool done = false;

        // 지난번에 뒤에서 나왔다면 이번엔 랜덤 먼저 시도
        bool preferRandom = (lastTeleportType == TeleportType.Behind);

        if (angelFloor == playerFloor)
        {
            if (preferRandom)
            {
                // 같은 층 랜덤 → 안 되면 같은 층 뒤
                done = TryRandomSameFloor(target, playerFloor) ||
                       TryBehindSameFloor(target, playerFloor);
            }
            else
            {
                // 같은 층 뒤 → 안 되면 같은 층 랜덤
                done = TryBehindSameFloor(target, playerFloor) ||
                       TryRandomSameFloor(target, playerFloor);
            }
        }

        if (!done)
        {
            if (preferRandom)
            {
                // 층 무시 랜덤 → 안 되면 층 무시 뒤
                done = TryRandomAnyFloor(target) ||
                       TryBehind(target);
            }
            else
            {
                // 층 무시 뒤 → 안 되면 층 무시 랜덤
                done = TryBehind(target) ||
                       TryRandomAnyFloor(target);
            }
        }

        if (done) lastReappearTime = Time.time;
    }

      
        bool TryBehind(Transform target)
        {
            // 1) 플레이어 뒤 방향
            Vector3 backFlat = -Flat(target.forward);
            if (backFlat.sqrMagnitude < 0.001f) return false;

            // 2) 후보 위치
            Vector3 candidate = target.position + backFlat.normalized * behindOffset;

            // 3) 플레이어 정면 각도 제한(앞에만 안 나오게)
            if (!PassesFrontLimit(target, candidate))
                return false;

            // 4) NavMesh 위로만 스냅
            if (agent && agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(candidate, out var hitNav, 2f, NavMesh.AllAreas))
                    candidate = hitNav.position;
            }
            // 5) 그냥 워프 (벽 살짝 끼는 건 나중에 spawn point/offset으로 조정)
            if (WarpTo(candidate))
            {
                lastTeleportType = TeleportType.Behind;
                return true;
            }
            return false;
        
        }

        //같은 층 뒤 스폰
        bool TryBehindSameFloor(Transform target, int playerFloor)
        {
            Vector3 back = -Flat(target.forward);
            if (back.sqrMagnitude < 0.001f) return false;
            Vector3 candidate = target.position + back.normalized * behindOffset;

            if (!IsSameFloor(candidate, target.position, playerFloor)) return false;
            if (!PassesFrontLimit(target, candidate)) return false;
            if (agent && agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(candidate, out var hitNav, 2f, NavMesh.AllAreas))
                    candidate = hitNav.position;
            }

            if (WarpTo(candidate))
            {
                lastTeleportType = TeleportType.Behind;
                return true;
            }
            return false;

        }
    

        //같은 층 랜덤
        bool TryRandomSameFloor(Transform target, int playerFloor)
        {
            Transform[] points = (playerFloor == 1) ? floor1Points : floor2Points;
            if (points == null || points.Length == 0) return false;

            Transform pick = null;
            int tries = 8;
            for (int i = 0; i < tries; i++)
            {
                var t = points[Random.Range(0, points.Length)];
                if (!t) continue;
                if (!PassesFrontLimit(target, t.position)) continue;
                if (!IsSameFloor(t.position, target.position, playerFloor)) continue;

                bool occluded = IsOccluded(target.position, t.position);
                pick = t;
                if (occluded) break; // 가려진 곳 우선
            }
            if (pick && WarpTo(pick.position))
            {
                lastTeleportType = TeleportType.RandomFloor;
                return true;
            }
            return false;

        }
    
    

        //층 무시 랜덤(모든 포인트 풀에서)
        bool TryRandomAnyFloor(Transform target)
        {
            int len1 = floor1Points != null ? floor1Points.Length : 0;
            int len2 = floor2Points != null ? floor2Points.Length : 0;
            if (len1 + len2 == 0) return false;

            for (int i = 0; i < 8; i++)
            {
                Transform t = PickAny();
                if (!t) continue;
                if (!PassesFrontLimit(target, t.position)) continue;
                if (IsOccluded(target.position, t.position))
                {
                    if (WarpTo(t.position))
                    {
                        lastTeleportType = TeleportType.RandomFloor;
                        return true;
                    }
                }
            }
            return false;

            Transform PickAny()
            {
                int total = len1 + len2;
                int idx = Random.Range(0, total);
                if (idx < len1) return floor1Points[idx];
                return floor2Points[idx - len1];
            }
        }
    

    //워프 (NavMesh 우선)
    bool WarpTo(Vector3 worldPos)
    {
        // 멀티면 RPC로 분리: AngelNetSync 사용 권장
        // var net = GetComponent<AngelNetSync>(); if (net) { net.WarpAll(worldPos); return true; }

        // NavMesh 위 좌표로 스냅
        if (NavMesh.SamplePosition(worldPos, out var hit, 3f, NavMesh.AllAreas))
            worldPos = hit.position;

        bool ok;

        if (agent && agent.isOnNavMesh) ok=agent.Warp(worldPos);
        else
        {
            transform.position = worldPos;
            ok = true;
        }

        Face(worldPos);

        if(pv && PhotonNetwork.isMasterClient)
            pv.RPC("RPC_Warp",PhotonTargets.Others,transform.position,transform.rotation);


        return ok;
    }
    [PunRPC]
    void RPC_Warp(Vector3 pos,Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
    //경로 막힐시 텔포 시도 
    public bool TryReappearNow(Transform player)
    {
        // 보이면 금지 + 내부 쿨타임
        if (IsSeenNow()) return false;
        if (Time.time - lastReappearTime < cooldown) return false;

        int pf = GetPlayerFloor(player);

        //  이번 텔포에서 "랜덤을 우선할지" 여부 (번갈아가며)
        bool preferRandom = lastWasBehind;   // 직전에 뒤텔포 했으면 이번엔 랜덤 우선

        bool ok = false;

        if (useFloors)
        {
            if (preferRandom)
            {
                // 이번 턴은 랜덤 계열 먼저 시도
                ok = TryRandomSameFloor(player, pf)
                ||TryRandomAnyFloor(player)
                 ||  TryBehindSameFloor(player, pf);
            }
            else
            {
                // 이번 턴은 뒤텔포 먼저 시도 (예전 방식)
                ok = TryBehindSameFloor(player, pf)
                ||TryRandomSameFloor(player, pf)
                  || TryRandomAnyFloor(player);
            }
        }
        else
        {
            if (preferRandom)
            {
                ok = TryRandomAnyFloor(player)
                   ||TryBehind(player);
            }
            else
            {
                ok = TryBehind(player)
                   ||TryRandomAnyFloor(player);
            }
        }

        if (ok)
        {
            lastReappearTime = Time.time;
            //  성공했으면 플래그 토글 다음에는 우선순위 반대로
            lastWasBehind = !lastWasBehind;
        }

        return ok;
    }
    void Face(Vector3 targetPos)
    {
        Vector3 dir = Flat(targetPos - transform.position);
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
   
    
    //시야/관찰자
    Transform GetNearestObserver()
    {
        if (fsm && fsm.observers != null && fsm.observers.Count > 0)
        {
            Transform best = null; float bestSqr = float.MaxValue; Vector3 p = transform.position;
            foreach (var t in fsm.observers)
            {
                if (!t) continue;
                float sq = (t.position - p).sqrMagnitude;
                if (sq < bestSqr) { bestSqr = sq; best = t; }
            }
            return best;
        }
        return Camera.main ? Camera.main.transform : null;
    }
    //카메라 기준 몬스터 레이캐스트를 이용한 검사
    bool IsSeenNow()
    {
        if (!fsm || fsm.observers == null || fsm.observers.Count == 0) return false;

        Vector3 myCenter = GetCenter();
        foreach (var obs in fsm.observers)
        {
            if (!obs) continue;
            Vector3 toMe = myCenter - obs.position;
            float dist = toMe.magnitude;
            if (dist > fsm.sightDistance) continue;

            Vector3 dir = toMe.normalized;
            if (Vector3.Dot(obs.forward, dir) < fsm.sightDot) continue;

            if (!Physics.Raycast(obs.position, dir, dist, fsm.obstructionMask))
                return true;
        }
        return false;
    }

    //층 관련
    int GetPlayerFloor(Transform player)
    {
        if (floorMode == FloorMode.AutoByY) return (player.position.y > floorSplitY) ? 2 : 1;
        return currentFloorPlayer;
    }
    int GetAngelFloor()
    {
        if (floorMode == FloorMode.AutoByY) return (transform.position.y > floorSplitY) ? 2 : 1;
        return currentFloorAngel;
    }
    bool IsSameFloor(Vector3 a, Vector3 b, int playerFloor)
    {
        if (floorMode == FloorMode.AutoByY)
        {
            // 높이차 허용 밴드 내이면 같은 층 취급
            return Mathf.Abs(a.y - b.y) <= sameFloorBand;
        }
        else
        {
            // Zones 모드: 플레이어 층과 동일해야 함(보수적으로 y도 함께 고려)
            int aFloor = (a.y > floorSplitY) ? 2 : 1;
            return aFloor == playerFloor;
        }
    }
    //공용 관련

    //플레이어 졍면 각도 제한 및 검사
    bool PassesFrontLimit(Transform viewer, Vector3 candidate)
    {
        Vector3 dir = Flat(candidate - viewer.position).normalized;
        return Vector3.Dot(viewer.forward, dir) <= reappearDotLimit;
    }
    //레이캐스트로 시야 차단 검사
    bool IsOccluded(Vector3 from, Vector3 to)
    {
        return Physics.Linecast(from + Vector3.up * 1.6f, to + Vector3.up * 1.0f, reappearObstructionMask);
    }
    //시야 판정 상체 높이
    Vector3 GetCenter()
    {
        var col = GetComponent<CapsuleCollider>();
        return col ? transform.TransformPoint(col.center + Vector3.up * (col.height * 0.25f))
                   : transform.position + Vector3.up * 1.0f;
    }
    
    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
    static float FlatDistance(Vector3 a, Vector3 b) { a = Flat(a); b = Flat(b); return Vector3.Distance(a, b); }

   
   
}