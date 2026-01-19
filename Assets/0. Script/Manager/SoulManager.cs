using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class SoulManager : MonoBehaviour
{
    Player player;

    // 데이터 원본. 
    List<SoulData> allSouls;

    // 인게임 인스턴스. 플레이 중에 보유되는 영성들
    List<SoulInstance> curSouls = new List<SoulInstance>();
    public List<SoulInstance> CurSouls => curSouls;

    // 보유 영성 UI에서 받는 이벤트. 단순히 열 때마다 새로 갖고와도 되지만 실시간 업데이트를 위해
    public event Action<List<SoulInstance>> soulGot;

    public static SoulManager Instance {get; private set;}
    void Awake()
    {   
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 데이터 읽어오기. Resources/SoulDatas 폴더
        SoulData[] loaded = Resources.LoadAll<SoulData>("SoulDatas");
        allSouls = loaded.ToList();

        // 참조 가져오기.
        SceneContext sceneContext = FindFirstObjectByType<SceneContext>();
        player = sceneContext.player;
    }

    public void EnrollSoul(SoulData data)
    {
        if (data == null) return;

        SoulInstance inst = curSouls.Find(s => s.data == data);

        // 이미 보유한 영성일 경우 → 스택 증가
        if (inst != null)
        {
            // maxStack <= 0 → 무한 스택
            if (data.maxStack <= 0 || inst.stack < data.maxStack)
            {
                inst.stack++;

                // 이번에 추가된 스택 1개분만 효과 적용
                ApplySoulDataEffectsOnce(data.effect);
            }
            // 더 이상 쌓을 수 없으면 무시
            return;
        }

        // 처음 얻는 경우 리스트에 등록
        SoulInstance newInst = new SoulInstance(data);
        curSouls.Add(newInst);

        // 새로 얻은 1개분 효과 적용
        ApplySoulDataEffectsOnce(data.effect);
    }

    /// SoulData 안에 있는 모든 SoulEffect를 한 번씩 실행
    /// (실제 로직은 SoulEffect 쪽에 있음)
    public void ApplySoulDataEffectsOnce(SoulEffect effect)
    {
        if (effect == null) return;
        effect.ApplyOnce(player);
        
        // 등록 완료 되면 UI등에 알리기
        soulGot?.Invoke(curSouls);
    }

    #region 영성 뽑기
    // 이전에 뽑았던 영성을 제외하고 num 만큼 영성 뽑기
    public SoulData[] GetSouls(SoulData[] previousCandidates, int num)
    {
        if (num <= 0 || allSouls == null || allSouls.Count == 0)
        {
            Debug.Log($"GetSouls 오류: {num}, {allSouls == null}, {allSouls.Count == 0}");
            return Array.Empty<SoulData>();
        }

        // 0. 먼저 조건(캐릭터/레벨/기타) 만족하는 애들만 필터링
        List<SoulData> candidates = allSouls
            .Where(soul => soul.soulType == SoulType.Soul)
            .Where(soul => soul != null && soul.CanOffer(player))
            .Where(soul =>
            {
                // SoulManager에서 현재 스택 조회
                SoulInstance inst = CurSouls.Find(s => s.data == soul);

                // 없는 경우는 새로 획득 가능
                if (inst == null) return true;

                // maxStack <= 0 → 무제한, 그 외에는 스택 한도 미만만 후보
                return soul.maxStack <= 0 || inst.stack < soul.maxStack;
            })
            .ToList();


        if (candidates.Count == 0)
        {
            Debug.Log("GetSouls: 조건을 만족하는 영성이 없음");
            return Array.Empty<SoulData>();
        }

        List<SoulData> result = new List<SoulData>(num);

        // 1. 직전에 나왔던 애들을 제외한 "새로운" 후보 목록 만들기
        List<SoulData> freshCandidates = new List<SoulData>();

        for (int i = 0; i < candidates.Count; i++)
        {
            SoulData soul = candidates[i];

            bool wasInPrevious = false;
            if (previousCandidates != null)
            {
                for (int j = 0; j < previousCandidates.Length; j++)
                {
                    if (previousCandidates[j] == soul)
                    {
                        wasInPrevious = true;
                        break;
                    }
                }
            }

            if (!wasInPrevious)
            {
                freshCandidates.Add(soul);
            }
        }

        // 2. freshCandidates에서 먼저 뽑기 (중복 없이 랜덤)
        int need = num;
        int takeFromFresh = Mathf.Min(need, freshCandidates.Count);

        for (int i = 0; i < takeFromFresh; i++)
        {
            int index = UnityEngine.Random.Range(0, freshCandidates.Count);
            SoulData chosen = freshCandidates[index];
            result.Add(chosen);
            freshCandidates.RemoveAt(index);   // 중복 방지
        }

        need = num - result.Count;

        // 3. 아직 더 필요하면, "이제는 기존에 나왔던 것도 포함해서" 뽑기
        if (need > 0)
        {
            // result에 이미 들어간 것만 제외한 전체 후보(candidates)를 다시 후보로
            List<SoulData> fallbackCandidates = new List<SoulData>();

            for (int i = 0; i < candidates.Count; i++)
            {
                SoulData soul = candidates[i];

                bool alreadyInResult = false;
                for (int j = 0; j < result.Count; j++)
                {
                    if (result[j] == soul)
                    {
                        alreadyInResult = true;
                        break;
                    }
                }

                if (!alreadyInResult)
                {
                    fallbackCandidates.Add(soul);
                }
            }

            int takeFromFallback = Mathf.Min(need, fallbackCandidates.Count);

            for (int i = 0; i < takeFromFallback; i++)
            {
                int index = UnityEngine.Random.Range(0, fallbackCandidates.Count);
                SoulData chosen = fallbackCandidates[index];
                result.Add(chosen);
                fallbackCandidates.RemoveAt(index);
            }
        }

        return result.ToArray();
    }
    #endregion
}

