using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RaidBossUnitController : EnemyBaseUnitController
{
    [SerializeField] private Skill[] _bossSkills;
    protected Skill[] BossSkills {get => _bossSkills; set => _bossSkills = value; }
    
    private int _skillIndex;
    
    [SerializeField] private Skill _bossSkill0;
    protected Skill BossSkill0 => _bossSkill0;
    [SerializeField] private Skill _bossSkill1;
    protected Skill BossSkill1 => _bossSkill1;
    private List<BaseUnitController> _skillTargets;
    protected List<BaseUnitController> SkillTargets { get => _skillTargets; set => _skillTargets = value; }
    //[SerializeField] private Transform _muzzlePoint;
    //public Transform MuzzlePoint { get => _muzzlePoint; set => _muzzlePoint = value; }
    [HideInInspector] private GameObject laserObejct;
    public GameObject LaserObejct {get => laserObejct; set => laserObejct = value; }
    
    private BossSkillRuntimeData _skillRuntimeData;
    public BossSkillRuntimeData SkillRuntimeData { get => _skillRuntimeData; set => _skillRuntimeData = value; }

    // 더 많으면 배열 혹은 리스트로
    private bool _isSkill0Running;
    public bool IsSkill0Running { get => _isSkill0Running; set => _isSkill0Running = value; }
    private bool _isSkill1Running;
    public bool IsSkill1Running { get => _isSkill1Running; set => _isSkill1Running = value; }
    
    public Skill curSkill { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        SkillTargets = new List<BaseUnitController>();
        CoolTimeCounter = 2.0f;
    }

    protected override BaseNode SetBTree()
    {
        return new SelectorNode //Behaviour Selector
        (
            new List<BaseNode>
            {
                new SelectorNode
                (
                    new List<BaseNode>
                    {
                        new ActionNode(CheckUnitDying),
                        new ActionNode(CheckCrowdControl),
                    }
                ),

                new SelectorNode 
                (
                    new List<BaseNode>
                    {
                        new DecoratorNode
                        (
                            new ConditionNode(() => IsSkillRunning),
                            new ActionNode(PerformChosenSkill)
                            
                        ),
                        new SequenceNode // skillable Dicision Sequence
                        (
                            new List<BaseNode>()
                            {
                                new ConditionNode(CheckSkillCooltimeBack),
                                new ActionNode(ChooseNextSkill),
                                new ActionNode(SetTargetsForChosenSkill),
                                new ActionNode(PerformChosenSkill)
                                
                            }
                        ),
                    }
                ),

                /*new SequenceNode // Attack Dicision // 평타로직 필요할 경우를 대비해 주석처리
                (
                    new List<BaseNode>
                    {
                        new ConditionNode(CheckAttackRange),
                        new ActionNode(SetTargetToAttack),
                        new ActionNode(PerformAttack)
                    }
                ),*/
                new SequenceNode
                (
                    new List<BaseNode>
                    {
                        new ActionNode(SetDetectedTarget),
                        // CheckMoveable ?
                        new ActionNode(ChaseTarget)
                    }
                ),
                new ActionNode(StayIdle)
            }
        );
    }

    private BaseNode.ENodeState ChooseNextSkill()
    {
        if (BossSkills == null || BossSkills.Length == 0)
        {
            Debug.LogWarning("보스 스킬 배열 공란");
            return BaseNode.ENodeState.Failure;
        }
        _skillIndex = (_skillIndex + 1) % BossSkills.Length; // bossSkills의 배열을 순회하고 다시 0으로
        Debug.Log($"현재 스킬 인덱스 : {_skillIndex}");
        return BaseNode.ENodeState.Success;
    }

    private BaseNode.ENodeState PerformChosenSkill()
    {
        return BossSkills[_skillIndex].Perform(this, SkillTargets);
    }

    private BaseNode.ENodeState SetTargetsForChosenSkill()
    {
        return BossSkills[_skillIndex].SetTargets(this, SkillTargets);
    }
    
    
    protected override bool IsSkillAlreadyRunning()
    {
        return IsSkillRunning;
        
        //return CurSkill != null;
    }

    /*private BaseNode.ENodeState PerformCurSkill()
    {
        if (CurSkill == null)
            return BaseNode.ENodeState.Failure;
        return CurSkill.
    }*/

    

    /*protected bool CheckBossPhase(float time)
    {
        
    }*/
}
