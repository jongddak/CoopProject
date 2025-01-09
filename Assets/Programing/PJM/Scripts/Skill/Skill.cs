using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class Skill : ScriptableObject
{
    [SerializeField] private string _skillName;
    public string SkillName {get => _skillName; set => _skillName = value; }
    
    [SerializeField] private bool _targetAll;
    public bool TargetAll {get => _targetAll; protected set => _targetAll = value; }
    // TargetAll이 true면 아래 skillrange와 maxTargetingNum은 숨기고싶다
    
    [SerializeField] private float _skillRange;
    public float SkillRange {get => _skillRange; protected set => _skillRange = value; }


    [SerializeField] private int _maxTargetingNum;
    public int MaxTargetingNum {get => _maxTargetingNum; protected set => _maxTargetingNum = value; }

    [SerializeField] private bool _isPriorityTargetFar;
    public bool IsPriorityTargetFar {get => _isPriorityTargetFar; protected set => _isPriorityTargetFar = value; }
    
    [SerializeField] private float skillRatio;
    public float SkillRatio {get => skillRatio; protected set => skillRatio = value; }
    [SerializeField] private float cooltime;
    public float Cooltime {get => cooltime; protected set => cooltime = value; }
    [SerializeField] private CrowdControls crowdControl = CrowdControls.None;
    public CrowdControls CrowdControl {get => crowdControl;}
    [SerializeField] float ccDuration;

    public float CcDuration
    {
        get
        {
            if (crowdControl == CrowdControls.None)
                return 0f;
            return ccDuration;
        }
    }

    [SerializeField] private Sprite _skillIcon;
    public Sprite SkillIcon {get => _skillIcon;set => _skillIcon = value; }
    
    [SerializeField] private GameObject _vfxToTarget;
    public GameObject VFXToTarget {get => _vfxToTarget;}
    
    [SerializeField] private GameObject _vfxToMine;
    public GameObject VFXToMine {get => _vfxToMine;}


    /*protected Transform skillTarget; // 여기 있어도 괜찮나? 계속 바뀔텐데 데이터 컨테이너에 있을 얘가 아닌가?
    public Transform SkillTarget { get => skillTarget; protected set => skillTarget = value; }*/
    //protected List<Transform> skillTargets;
    //public List<Transform> SkillTargets { get => skillTargets; protected set => skillTargets = value; }

    // 거리 체크
    /*protected virtual bool CheckRange(Transform caster, Transform target)
    {
        if (target == null)
            return false;
        float sqrDistance = (target.position - caster.position).sqrMagnitude;
        return sqrDistance <= skillRange * skillRange;
    }*/

    // 타겟 설정
    protected abstract BaseNode.ENodeState SetTargets(BaseUnitController caster, List<BaseUnitController> targets);

    // 스킬 실행
    protected abstract BaseNode.ENodeState Perform(BaseUnitController caster, List<BaseUnitController> targets);

    /*protected virtual void ResetTargets()
    {
        SkillTargets.Clear();
    }*/

    // 스킬 행동 트리를 반환하는 메서드
    //public abstract SequenceNode CreateSkillBTree(Transform caster, LayerMask enemyLayer, bool isPriorityTargetFar, Animator unitAnimator);
    
    public SequenceNode CreateSkillBTree(BaseUnitController caster,List<BaseUnitController> targets)
    {
        return new SequenceNode
        (
            new List<BaseNode>()
            {
                //new ConditionNode(() => CheckRange(caster)),
                new ActionNode(() => SetTargets(caster,targets)), 
                new ActionNode(() => Perform(caster, targets))
            }
        );
    }

    public BaseNode CreatePerformNode(BaseUnitController caster,List<BaseUnitController> targets)
    {
        return new ActionNode(() => Perform(caster, targets));
    }

    protected void SpawnEffect(Transform targetTransform, GameObject effectPrefab)
    {
        if(effectPrefab == null)
            return;
        
        GameObject particleObject = Instantiate(effectPrefab, targetTransform.position, Quaternion.identity);
        if (particleObject == null)
            return;
        
        if (particleObject.TryGetComponent<ParticleSystem>(out var particleSystem))
        {
            Destroy(particleObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(particleObject);
        }
    }

    protected void ResetTargets(List<BaseUnitController> targets)
    {
        targets.Clear();
    }
}

