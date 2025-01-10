using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AoESkill", menuName = "Skills/AoESkill")]
public class AoESkill : Skill
{
    public float areaAngle;
    public LayerMask targetLayer;
    


    protected override BaseNode.ENodeState SetTargets(BaseUnitController caster, List<BaseUnitController> targets)
    {
        ResetTargets(targets);
        
        // 타겟 객체가 하나라도 범위에 있으면 Success 아니라면 Failure
        OverlapAreaAllWithAngle(caster, targets);
        return targets.Count == 0 ? BaseNode.ENodeState.Failure : BaseNode.ENodeState.Success;
    }

    protected override BaseNode.ENodeState Perform(BaseUnitController caster, List<BaseUnitController> targets)
    {
        if (targets.Count == 0)
        {
            Debug.Log($"{SkillName}: 타겟이 없습니다.");
            return BaseNode.ENodeState.Failure;
        }

        //if (!unitAnimator.GetBool("Skill"))
        if (caster.UnitViewer.UnitAnimator == null)
        {
            Debug.LogWarning("애니메이터 없음;");
            return BaseNode.ENodeState.Failure;
        }
        
        caster.UnitViewer.UnitAnimator.SetBool(caster.UnitViewer.ParameterHash[(int)Parameter.Run], false);
        
               // 스킬 시전 시작
        //if (!caster.IsSkillRunning)
        if(!caster.UnitViewer.UnitAnimator.GetBool(caster.UnitViewer.ParameterHash[(int)Parameter.Skill]))
        {
            caster.UnitViewer.UnitAnimator.SetBool(caster.UnitViewer.ParameterHash[(int)Parameter.Skill], true);
            Debug.Log($" {caster.gameObject.name} 스킬 시전");
            caster.CoolTimeCounter = Cooltime;
            caster.IsSkillRunning = true;
            return BaseNode.ENodeState.Running;
        }
        
        
        var stateInfo = caster.UnitViewer.UnitAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("UsingSkill"))
        {
            if (stateInfo.normalizedTime < 1.0f)
            {
                Debug.Log($"{caster.gameObject.name} : '{SkillName}' 사용 중.");
                return BaseNode.ENodeState.Running;
            }
            else if (stateInfo.normalizedTime >= 1.0f)
            {
                {
                    Debug.Log($"{caster.gameObject.name} : '{SkillName}' 사용 완료.");
                    
                    caster.UnitViewer.UnitAnimator.SetBool(caster.UnitViewer.ParameterHash[(int)Parameter.Skill],
                        false);
                    caster.IsSkillRunning = false;

                    OverlapAreaAllWithAngle(caster, targets);
                    float skillDamage = caster.UnitModel.AttackPoint * SkillRatio;
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
                                Debug.Log("qqqq");
                                target.UnitModel.TakeCrowdControl(CrowdControl, CcDuration, caster);
                            }
                        }
                    }
                    return BaseNode.ENodeState.Success;
                }
            }
        }
        return BaseNode.ENodeState.Failure;
    }

    protected void OverlapAreaAllWithAngle(BaseUnitController caster, List<BaseUnitController> targets)
    {
        Vector2 dir =  caster.gameObject.transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(caster.CenterPosition.position, SkillRange, targetLayer);
        foreach (var col in hitColliders)
        {
            if(col == null)
                continue;
            BaseUnitController target = col.GetComponentInParent<BaseUnitController>();
            if(target == null)
                continue;
            
            Vector2 dirToCol = (col.transform.position - caster.CenterPosition.position).normalized;
            //float dot = Vector2.Dot(dir, dirToCol);
            float angle = Vector2.Angle(dir, dirToCol);
            
            
            if (angle <= areaAngle * 0.5f) // 위 아래 부채꼴 모양
            {
                targets.Add(target);
            }
        }
    }

    /*protected void OverlapOneWithAngle(BaseUnitController caster, List<BaseUnitController> targets)
    {
        Vector2 dir =  caster.gameObject.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(caster.CenterPosition.position, SkillRange, targetLayer);
        foreach (var col in hitColliders)
        {
            if(col == null)
                continue;
            BaseUnitController target = col.GetComponentInParent<BaseUnitController>();
            if(target == null)
                continue;
            
            Vector2 dirToCol = (col.transform.position - caster.CenterPosition.position).normalized;
            float dot = Vector2.Dot(dir, dirToCol);
            if (dot >= 0f) // 정면 180도
            {
                targets.Add(target);
                return;
            }
        }
    }*/
}
