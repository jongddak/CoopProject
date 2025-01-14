
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBossSkill", menuName = "Skills/BossSkill/AoE")]
public class BossAoESkill : AoESkill
{
    /*public float areaAngle;
    public LayerMask targetLayer;*/
    
    // Need Fix
    /*[Header("Skill Num : 3 - skill0 | 4- skill1")]
    [Range(3,4)]
    [SerializeField] private int _skillNum = 3;
    public Parameter SkillNumAsParameter {get => (Parameter)_skillNum;}*/
    
/*protected override BaseNode.ENodeState SetTargets(BaseUnitController caster, List<BaseUnitController> targets)
    {
        ResetTargets(targets);
        
        // 타겟 객체가 하나라도 범위에 있으면 Success 아니라면 Failure
        OverlapAreaAllWithAngle(caster, targets);
        return targets.Count == 0 ? BaseNode.ENodeState.Failure : BaseNode.ENodeState.Success;
    }*/

public override BaseNode.ENodeState Perform(BaseUnitController caster, List<BaseUnitController> targets)
    {
        var raidBossCaster = caster as RaidBossUnitController;
        if (raidBossCaster == null)
        {
            Debug.LogWarning("다운캐스팅 실패");
            return BaseNode.ENodeState.Failure;
        }
        
        if (targets.Count == 0)
        {
            Debug.Log($"{SkillName}: 타겟이 없습니다.");
            return BaseNode.ENodeState.Failure;
        }

        //if (!unitAnimator.GetBool("Skill"))
        if (raidBossCaster.UnitViewer.UnitAnimator == null)
        {
            Debug.LogWarning("애니메이터 없음;");
            return BaseNode.ENodeState.Failure;
        }
        
        //int skillParameterHash = raidBossCaster.UnitViewer.SkillParameterHash[(int)SkillParameterNumber];
        raidBossCaster.UnitViewer.UnitAnimator.SetBool(raidBossCaster.UnitViewer.ParameterHash[(int)Parameter.Run], false);
        
        // 스킬 시전 시작
        //if (!caster.IsSkillRunning)
        //if(!raidBossCaster.UnitViewer.UnitAnimator.GetBool(skillParameterHash))
        if(!GetBoolSkillParameter(raidBossCaster))
        {
            //raidBossCaster.CurSkill = this;
            //raidBossCaster.IsSkill0Running = true; // Need Fix 
            //raidBossCaster.UnitViewer.UnitAnimator.SetBool(skillParameterHash, true);
            SetBoolSkillParameter(raidBossCaster, true);
            Debug.Log($" {raidBossCaster.gameObject.name} 스킬 시전");
            raidBossCaster.CoolTimeCounter = Cooltime;
            raidBossCaster.IsSkillRunning = true;
            return BaseNode.ENodeState.Running;
        }
        
        var stateInfo = raidBossCaster.UnitViewer.UnitAnimator.GetCurrentAnimatorStateInfo(0);
        {
            if (stateInfo.normalizedTime < 1.0f)
            {
                Debug.Log($"{raidBossCaster.gameObject.name} : '{SkillName}' 사용 중.");
                return BaseNode.ENodeState.Running;
            }
            else if (stateInfo.normalizedTime >= 1.0f)
            {
                {
                    Debug.Log($"{raidBossCaster.gameObject.name} : '{SkillName}' 사용 완료.");
                    
                    //raidBossCaster.UnitViewer.UnitAnimator.SetBool(skillParameterHash, false);
                    SetBoolSkillParameter(raidBossCaster, false);
                    raidBossCaster.IsSkillRunning = false;

                    OverlapAreaAllWithAngle(raidBossCaster, targets);
                    float skillDamage = raidBossCaster.UnitModel.AttackPoint * SkillRatio;
                    Debug.Log($"스킬 데미지 {skillDamage} ");
                    foreach (var target in targets)
                    {
                        // 데미지 주는 로직
                        // 데미지를 줄 인원 수 선택 필요
                        if (target.gameObject != null)
                        {
                            target.UnitModel.TakeDamage(Mathf.RoundToInt(skillDamage)); // 소숫점 버림, 반올림할지 선택 필요
                            //Debug.Log($"{SkillName}으로 {(int)attackDamage} 만큼 데미지를 {target}에 가함");
                            if (CrowdControl != CrowdControls.None)
                            {
                                target.UnitModel.TakeCrowdControl(CrowdControl, CcDuration, raidBossCaster);
                            }
                        }
                    }

                    //raidBossCaster.CurSkill = null;
                    //raidBossCaster.IsSkill0Running = false; // Need Fix 
                    return BaseNode.ENodeState.Success;
                    
                }
            }
        }
        return BaseNode.ENodeState.Failure;
    }
}
