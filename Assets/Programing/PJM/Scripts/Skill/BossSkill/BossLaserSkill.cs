using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "NewLaserSkill", menuName = "Skills/BossSkill/Laser")]
public class BossLaserSkill : Skill
{
    // 우선 보스만 사용할 수 있음
    // 후에 캐스팅 스킬이라는 이름으로 일부 재활용 가능
    //public enum AttackType {Laser, None}
    
    

    //public AttackType attackType;
    public float startUpRatio; // 실제 스킬 발동전까지
    public float recoveryRatio; // 발동 종료 후 회수
    public float tickNumber;
    public GameObject laserPrefab;
    public float laserPrefabBaseScale;

    //[Header("Skill Num : 3 - skill0 |  4- skill1")]
    //[Range(3,4)]
    //[SerializeField] private int _skillNum = 4;
    //public Parameter SkillNumAsParameter {get => (Parameter)_skillNum;}

    public override BaseNode.ENodeState SetTargets(BaseUnitController caster, List<BaseUnitController> targets)
    {
        ResetTargets(targets);

        if (TargetAll)
        {
            string myLayerName = LayerMask.LayerToName(caster.gameObject.layer);
            if (myLayerName == "UserCharacter")
            {
                foreach (var target in BattleSceneManager.Instance.enemyUnits)
                {
                    if (target == null || !target.gameObject.activeSelf)
                        continue;
                    targets.Add(target);
                    Debug.Log(target.gameObject.name);
                }
            }
            else
            {
                foreach (var target in BattleSceneManager.Instance.myUnits)
                {
                    if (target == null || !target.gameObject.activeSelf)
                        continue;
                    targets.Add(target);
                    Debug.Log(target.gameObject.name);
                }
            }

            return targets.Count > 0 ? BaseNode.ENodeState.Success : BaseNode.ENodeState.Failure;
        }


        Collider2D[] detectedColliders = Physics2D.OverlapCircleAll(caster.transform.position, SkillRange, caster.EnemyLayer);
        if (detectedColliders.Length == 0)
        {
            return BaseNode.ENodeState.Failure;
        }

        foreach (var collider in detectedColliders)
        {
            if (collider.TryGetComponent(out BaseUnitController target))
            {
                targets.Add(target);
            }
            else
            {
                Debug.LogWarning($"{collider.gameObject.name}은 baseUnitCollider를 가지지 않음");
            }
        }

        if (targets.Count == 0)
        {
            Debug.LogWarning("적합한 타겟을 찾지 못했습니다.");
            return BaseNode.ENodeState.Failure;
        }

        // 거리를 기준으로 정렬
        targets.Sort((a, b) =>
        {
            float distanceA = Vector2.Distance(caster.transform.position, a.transform.position);
            float distanceB = Vector2.Distance(caster.transform.position, b.transform.position);
            return IsPriorityTargetFar ? distanceB.CompareTo(distanceA) : distanceA.CompareTo(distanceB);
        });

        // 최대 타겟 수만큼만 타겟에 남기기
        //for (int i = 0; i < MaxTargetingNum; i++)

        if (targets.Count > MaxTargetingNum)
        {
            targets.RemoveRange(MaxTargetingNum, targets.Count - MaxTargetingNum);
        }
        
        // Todo : 여기서 타겟지정 이펙트 띄우기?

        return targets.Count > 0 ? BaseNode.ENodeState.Success : BaseNode.ENodeState.Failure;
    }

    public override BaseNode.ENodeState Perform(BaseUnitController caster, List<BaseUnitController> targets)
    {
        var raidBossCaster = caster as RaidBossUnitController;
        if (raidBossCaster == null)
        {
            Debug.LogWarning("다운캐스팅 실패");
            return BaseNode.ENodeState.Failure;
        }
        
        //if (targets[0] == null || !targets[0].gameObject.activeSelf)
        if (targets.Count == 0)
        {
            Debug.Log($"{SkillName}: 타겟이 없습니다.");
            return BaseNode.ENodeState.Failure;
        }

        if (raidBossCaster.UnitViewer.UnitAnimator == null)
        {
            Debug.LogWarning("애니메이터 없음;");
            return BaseNode.ENodeState.Failure;
        }

        // 도발한 대상이 있다면 타겟초기화 
        if (raidBossCaster.TauntSource != null && raidBossCaster.TauntSource.gameObject.activeSelf)
        {
            ResetTargets(targets);
            targets.Add(raidBossCaster.TauntSource);
            raidBossCaster.TauntSource = null;
        }

        var stateInfo = raidBossCaster.UnitViewer.UnitAnimator.GetCurrentAnimatorStateInfo(0);

        
        switch (raidBossCaster.CurSkillState)
        {
            case SkillState.None:
            {   
                //raidBossCaster.CurSkill = this;
                raidBossCaster.IsSkill1Running = true; // Need Fix 
                raidBossCaster.UnitViewer.UnitAnimator.SetBool(raidBossCaster.UnitViewer.ParameterHash[(int)Parameter.Run], false);
                raidBossCaster.UnitViewer.UnitAnimator.SetBool(raidBossCaster.UnitViewer.ParameterHash[(int)Parameter.Skill1], true);
                Debug.Log($" {raidBossCaster.gameObject.name} 스킬 시전");
                raidBossCaster.CoolTimeCounter = Cooltime;
                raidBossCaster.IsSkillRunning = true;
                raidBossCaster.CurSkillState = SkillState.Charging;
                raidBossCaster.SkillRuntimeData = new BossSkillRuntimeData(startUpRatio, recoveryRatio, tickNumber);
                return BaseNode.ENodeState.Running;
            }
            case SkillState.Charging:
            {
                if (stateInfo.normalizedTime > startUpRatio)
                {
                    raidBossCaster.CurSkillState = SkillState.Firing;
                    CreateLaserObject(raidBossCaster);
                }

                return BaseNode.ENodeState.Running;
            }
            case SkillState.Firing:
            {
                // (recoveryRatio - startUpRatio) / tickNumber : Done까지 normalizedTime이 해당 수치만큼 지날때마다 데미지 적용
                // 다만 문제가 있다
                // 예를들어 startUpRatio : 0.1, recoveryRatio : 0.9 , tickNumber를 4라고 하면
                // normalizedTime이 0.1부터 0.2 증가할때마다 데미지를 적용할것이다
                // 그런데 조건을 if(normalizedTime == 0.3) 이렇게 하면 오차때문에 제대로 작동하지 않을것 
                // 따로 BossSkillRuntimeData 클래스를 만들어주어 so에서 공유자원으로 사용될 여지가 있는 위험을 배제함
                if(raidBossCaster.LaserObejct == null) 
                {
                   Debug.LogWarning("레이저 생성되지 않음.");
                   return BaseNode.ENodeState.Failure;
                }
                
                AttackWithLaser(raidBossCaster.MuzzlePoint, targets[0].CenterPosition, raidBossCaster.LaserObejct);
                raidBossCaster.SkillRuntimeData.CheckAndDealDamage(stateInfo.normalizedTime,raidBossCaster,targets[0],SkillRatio);

                
                if (stateInfo.normalizedTime > recoveryRatio)
                {
                    raidBossCaster.CurSkillState = SkillState.Done;
                }

                return BaseNode.ENodeState.Running;
            }
            case SkillState.Done:
            {
                if (stateInfo.normalizedTime >= 1.0f)
                {
                    raidBossCaster.CurSkillState = SkillState.None;
                    raidBossCaster.UnitViewer.UnitAnimator.SetBool(raidBossCaster.UnitViewer.ParameterHash[(int)Parameter.Skill1],
                        false);
                    raidBossCaster.IsSkillRunning = false;
                    Debug.Log("스킬 완료");
                    if (raidBossCaster.LaserObejct != null)
                    {
                        Destroy(raidBossCaster.LaserObejct);
                    }

                    //raidBossCaster.CurSkill = null;
                    raidBossCaster.IsSkill1Running = false; // Need Fix 
                    return BaseNode.ENodeState.Success;
                }
                // 스킬 회수중
                Debug.Log("스킬 회수중");
                return BaseNode.ENodeState.Running;
            }
            default:
                Debug.LogWarning("Failure Flag");
                return BaseNode.ENodeState.Failure;
        }
    }

    /*private void InitTickDamageThresholds()
    {
        if (tickNumber <= 0)
        {
            Debug.LogWarning("틱을 줄 횟수는 반드시 1 이상이어야 함");
            return;
        }
    }*/

    private void CreateLaserObject(RaidBossUnitController raidBossCaster)
    {
        // 레이저를 만들어서 caster와 타겟의 위치를 이어주어야 하는데 둘의 이동에 따라 크기조정과 회전도 해줘야한다
        // 레이저의 프리팹은 GameObject 형식으로 해당 so에 저장하되, Instantiate 해준 객체는 계속 변경해줘야하기에 so에 저장해두긴 공유자원의 문제가 있음
        // 당장 생각나는건 caster의 baseUnitController에 공간을 두는것인데 레이저를 사용하지 않는 유닛이 더 많아 다소 껄끄럽다, null로 두면 그만이긴 한데
        if (laserPrefab == null)
        {
            Debug.LogWarning("레이저 프리팹 설정되지 않음");
            return;
        }
        Debug.Log("레이저 생성됨");
        raidBossCaster.LaserObejct = Instantiate(laserPrefab);
    }

    private void AttackWithLaser(Transform bossMuzzlePoint, Transform target, GameObject laserObject)
    {
        Debug.Log("AttackWithLaser 실행됨");
        if (target == null || !target.gameObject.activeSelf)
        {
            Debug.Log("타겟이 없음");
            return;
        }
            
        Vector2 dir = target.position - bossMuzzlePoint.position;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float laserScale = distance / laserPrefabBaseScale; // 크기 맞춰줌
        laserObject.transform.localScale = new Vector3(laserScale, 1, 1);
        laserObject.transform.rotation = Quaternion.Euler(0 ,0, angle);
        laserObject.transform.position = (bossMuzzlePoint.position + target.position) * 0.5f;
        // createLaserObject에서 만들어준 레이저 오브젝트를 caster와 target의 위치에 따라 움직여주고 회전시켜줄 메서드
        // 어떻게하지
    }
    
}
