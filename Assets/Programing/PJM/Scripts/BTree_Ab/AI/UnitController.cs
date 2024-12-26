using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class UnitController : MonoBehaviour
{
    // 임시 공격 후딜레이, 현재 미사용
    private float _tempDelay = 0.5f;
    private bool _inAttackDelay;
    
    private UnitView _unitViewer;
    public UnitView UnitViewer { get => _unitViewer; private set => _unitViewer = value; }
    
    protected BehaviourTreeRunner _BTRunner;
    /*protected Animator _unitAnimator;
    public Animator UnitAnimator { get => _unitAnimator; set => _unitAnimator = value; }*/
    protected Transform _detectedEnemy;
    public Transform DetectedEnemy { get => _detectedEnemy; protected set => _detectedEnemy = value; }
    
    protected Transform _currentTarget;
    public Transform CurrentTarget { get => _currentTarget; protected set => _currentTarget = value; }
    
    protected int unitID;
    public int UnitID { get { return unitID; } }

    // 카메라 범위
    protected Vector2 _bottomLeft;
    protected Vector2 _topRight;
    
    
    [SerializeField] protected float _detectRange;
    public float DetectRange { get => _detectRange; protected set => _detectRange = value; }
    
    [SerializeField] protected float _attackRange;
    public float AttackRange { get => _attackRange; protected set => _attackRange = value; }
    
    [SerializeField] protected float _moveSpeed;
    public float MoveSpeed { get => _moveSpeed; protected set => _moveSpeed = value; }
    
    [SerializeField] protected LayerMask _allianceLayer;
    public LayerMask AllianceLayer { get => _allianceLayer; protected set => _allianceLayer = value; }
    [SerializeField] protected LayerMask _enemyLayer;
    public LayerMask EnemyLayer { get => _enemyLayer; protected set => _enemyLayer = value; }
    
    protected bool _isAttacking = false;
    public bool IsAttacking { get => _isAttacking; protected set => _isAttacking = value;}
    
    [SerializeField] protected bool _isPriorityTargetFar;
    public bool IsPriorityTargetFar { get => _isPriorityTargetFar; set => _isPriorityTargetFar = value; }

    // 스킬있는 적이 나중에 생길수도 있음 혹은 보스라던가
    public float CoolTimeCounter { get; set; }
    public bool IsSkillRunning { get; set; }


    protected virtual void Start()
    {
        SetLayer();
        SetDetectingArea();
        //UnitAnimator = GetComponent<Animator>();
        _unitViewer = GetComponent<UnitView>();
        BaseNode rootNode = SetBTree();
        _BTRunner = new BehaviourTreeRunner(rootNode);
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
            return;

        _BTRunner.Operate();
    }


    protected abstract BaseNode SetBTree(); // 각 유닛이 구현할 행동 트리 메서드

    /*public bool IsAnimationRunning(string stateName)
    {
        /*AnimatorStateInfo stateInfo = UnitAnimator.GetCurrentAnimatorStateInfo(0);
        
        return stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1.0f;#1#
        if (UnitAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            var normalizedTime = UnitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            return normalizedTime != 0 && normalizedTime < 1.0f;
        }
        return false;
    }

    protected bool IsAnimationFinished(string stateName)
    {
        var stateInfo = UnitAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(stateName))
        {
            Debug.Log($"[IsAnimationFinished] {stateName} 상태에서 Normalized Time: {stateInfo.normalizedTime}");
            return stateInfo.normalizedTime >= 1.0f;
        }
        Debug.Log($"[IsAnimationFinished] 현재 상태는 {stateName}이 아님.");
        return false;
    }*/

    protected virtual void SetLayer()
    {
        string myLayerName = LayerMask.LayerToName(gameObject.layer);
        EnemyLayer = myLayerName == "UserCharacter" ? LayerMask.GetMask("Enemy") : LayerMask.GetMask("UserCharacter");
        AllianceLayer = LayerMask.GetMask(myLayerName);
    }

    /*protected bool IsPerformingAttackOrSkill()
    {
        if (UnitViewer.UnitAnimator.GetBool(UnitViewer.ParameterHash[(int)AniState.Attack]))
        {
            return true;
        }
        
    }*/
    
    
    protected BaseNode.ENodeState SetTargetToAttack()
    {
        if (DetectedEnemy != null)
        {
            CurrentTarget = DetectedEnemy;
            return BaseNode.ENodeState.Success;
        }
        return BaseNode.ENodeState.Failure;
    }
    
    protected BaseNode.ENodeState PerformAttack()
    {
        // 원하는것
        // 해당 노드에 들어왔을 때 공격 시작중이 아니었을때(처음 공격시작) 공격 시작, 공격시작 로그 1번
        // 공격중일 때(Running 반환) 공격 진행 중 로그 지속적으로 출력
        // 공격이 종료되었을 때 공격 종료 로그 1번, Success 반환
        // 위의 과정이 명확히 완료되어야 공격 선딜, 후딜도 추가할 수 있을 것 같다
        
        
        // 일단 후딜은 생각하지 말기
        /*if (_inAttackDelay)
            return BaseNode.ENodeState.Failure;*/
        
        // Attack 파라미터를 False로 바꿔주더라도 바로 다음 행동트리를 돌 때 True로 바꿔서 제대로 된 애니메이션 전환이 일어나지 않나?
        
        
        // 타겟이 유효하지 않을때 // 지워도 문제가 해결되지 않음
        if (CurrentTarget == null)
        {
            UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Attack], false);
            IsAttacking = false;
            Debug.Log(" 타겟이 유효하지 않아 공격 실패."); // 공격 애니메이션 진행중 대상이 사라졌을 경우?
            return BaseNode.ENodeState.Failure;
        }
        
        // 공격을 시작
        // 공격 파라미터가 False였을 경우에만 True로 바꿔주며 공격 시작
        UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Run], false);
        if(!UnitViewer.UnitAnimator.GetBool(UnitViewer.ParameterHash[(int)Parameter.Attack]))
        
        //if (!IsAttacking)
        {
            UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Attack], true);
            Debug.Log($"{CurrentTarget.gameObject.name}에 {gameObject.name}이 공격을 시작!");
            IsAttacking = true; // true로 바꿔줬으니 다음 트리 순회때 해당 조건문 실행x
            
            // 공격 애니메이션의 길이 + 지정된 공격 후딜레이 후 공격을 종료시켜줄 코루틴 // 현재 미사용
            // 공격 판정은 들어간 뒤 후 딜레이가 적용되어야 하므로 바꿀 필요가 있음
            //StartCoroutine(AttackRoutine("Attacking"));
            return BaseNode.ENodeState.Running;
        }
        
        // 공격 진행중, Attack 파라미터 True 상태
        //if (IsAttacking) // 위의 조건이 있어 사실상 필요 없을지도
            
        
        //if(UnitViewer.IsAnimationRunning())
        
        
        
        //if(UnitViewer.UnitAnimator.GetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack]))
        {
            var stateInfo = UnitViewer.UnitAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attacking"))
            {
                if (stateInfo.normalizedTime < 1.0f)
                {
                    Debug.Log($"{gameObject.name}가 {CurrentTarget.gameObject.name}를 공격 중");
                    return BaseNode.ENodeState.Running;
                }
                else if (stateInfo.normalizedTime >= 1.0f)
                {
                    // 공격 애니메이션이 끝났을 경우
                    Debug.Log($"{gameObject.name}가 {CurrentTarget.gameObject.name}에 대한 공격을 완료");
                    UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Attack], false);
                    IsAttacking = false;
                    return BaseNode.ENodeState.Success;
                }
            }
            else // 
            {
                // 트랜지션 이동중 애니메이션이 블렌딩 되어서? 애니메이션 상태로 확인하면 아래 로그가 출력됨
                Debug.Log("IsAttacking이 True지만 현재 애니메이션 상태가 Attacking이 아님"); // IsAttacking이 True면 일단 공격중이니 Running 반환
                //Debug.Log("Attack Bool 파라미터가 True지만 현재 애니메이션 상태가 Attacking이 아님");
                return BaseNode.ENodeState.Running;
            }
        }
        
        // 공격을 끝내야 할 경우, AttackRoutine 코루틴에서 애니메이션 길이 이후 IsAttacking을 false로 바꿔줬을 때
        // Attack 파라미터는 아직 True 상태
        //if (!IsAttacking)
        
        //if(!UnitViewer.IsAnimationRunning("Attacking"))
        
        /*if(stateInfo.IsName("Attacking") && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log($"공격 종료됨");
            UnitViewer.UnitAnimator.SetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack], false);
            IsAttacking = false;
            //StartCoroutine(AttackDelayRoutine());
            return BaseNode.ENodeState.Success;
        }*/
        
        Debug.LogWarning("예상치 못한 상태에서 공격 실패.");
        return BaseNode.ENodeState.Failure;
        
        

        /*// 공격을 시작해야하는 경우
        if()

        // 공격이 진행중일 때
        if (IsAttacking)
        {

        }
        //if(UnitViewer.IsAnimationRunning("Attacking"))
        if(UnitViewer.UnitAnimator.GetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack]))
        {
            Debug.Log($"{CurrentTarget.gameObject.name}에 {gameObject.name}이 공격 중!");
            //StartCoroutine(AttackRoutine(("Attacking")));
            return BaseNode.ENodeState.Running;
        }


        // 공격 시작, 애니메이터 Attack 이 false였을 때 = 아직 공격 시작을 하지 않았을 때
       // if (!_unitViewer.UnitAnimator.GetBool(_unitViewer.parameterHash[(int)UnitView.AniState.Attack]))
       // 애니메이션 스테이트 인포로 하면 정확한 상황을 받지 못할 가능성이 있다, 어택을 하고 있는지 확인할 bool 변수로 해본다
        //if(!IsAttacking)

        // getbool이 ture : 공격애니메이션 진행중일때
        if(UnitViewer.UnitAnimator.GetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack]))
        {
            UnitViewer.UnitAnimator.SetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack], true);
            Debug.Log($"{CurrentTarget.gameObject.name}에 {gameObject.name}이 공격 시작!");
            IsAttacking = true;
            return BaseNode.ENodeState.Success;
        }

        //if (!AttackTriggered)
        // Attack이 True : 공격이 진행중일 때
        //if(IsAnimationRunning(animationName))

        /*var stateInfo = UnitAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[PerformAttack] 현재 상태: {stateInfo.IsName(animationName)}, Normalized Time: {stateInfo.normalizedTime}");#1#

        if(!IsAttacking)
        {
            // 공격 모션이 끝남, 공격모션이 끝나고 한번만 실행되어야 함
            //Debug.Log($"공격 종료됨 어택트리거 상태 : {AttackTriggered}");
            //UnitAnimator.SetBool("Attack", false);
            UnitViewer.UnitAnimator.SetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack], false);
            Debug.Log($"공격 종료됨");
            //IsAttacking = false;
            return BaseNode.ENodeState.Success;
        }

        Debug.LogWarning("예상치 못한 상태에서 공격 실패.");
        return BaseNode.ENodeState.Failure;*/
    }

    protected bool IsSkillAlreadyRunning()
    {
        return IsSkillRunning;
    }

    protected bool CheckMoveable()
    {
        return !(IsAttacking || IsSkillRunning);
    }
    
    // coroutine
    protected IEnumerator AttackRoutine(string animationName)
    {
        /*while (UnitViewer.IsAnimationRunning(animationName))
        {
            yield return null;
        }*/
        
        //UnitAnimator.SetTrigger("Attack");
        
        // 애니메이션의 길이만큼 대기 후 리셋 + 후딜레이?
        // 현재 애니메이터의 info를 가져오기 때문에 실제 공격 애니메이션의 길이인지 확신 할 수 없음 다른방법 필요
        //Debug.Log($"{UnitViewer.UnitAnimator.GetCurrentAnimatorStateInfo(0).length + _tempDelay}초 후 리셋");
        yield return new WaitForSeconds(1.0f); // 애니메이션 길이
        //UnitViewer.UnitAnimator.SetBool(UnitViewer.parameterHash[(int)UnitView.AniState.Attack], false);
        IsAttacking = false;
        Debug.Log($"{animationName} 애니메이션 완료: 공격 리셋됨.");
    }

    /*protected IEnumerator AttackDelayRoutine()
    {
        _inAttackDelay = true;
        yield return new WaitForSeconds(_tempDelay);
        if (_inAttackDelay)
            _inAttackDelay = false;
    }*/

    protected BaseNode.ENodeState ChaseTarget()
    {
        if (DetectedEnemy != null)
        {
            float sqrDistance = Vector2.SqrMagnitude(DetectedEnemy.position - transform.position);
            if (sqrDistance > _attackRange * _attackRange) // 타겟이 공격 범위보다 멀때
            {
                UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Run], true);
                transform.position = Vector2.MoveTowards(transform.position, DetectedEnemy.position, _moveSpeed * Time.deltaTime);
                
                Debug.Log($"타겟 {DetectedEnemy.gameObject.name}를 추적 중");
                return BaseNode.ENodeState.Running;
            }
            else // 타겟이 공격 범위 내에 있을때 , 행동트리 후반에 있어서 공격으로 바로 넘어가서 뜨지 않음
            {
                UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Run], false);
                Debug.Log($"타겟 {DetectedEnemy.gameObject.name} 추적완료");
                return BaseNode.ENodeState.Success;
            }
        }
        // 타겟이 없을때
        Debug.Log("타겟 없음");
        UnitViewer.UnitAnimator.SetBool(UnitViewer.ParameterHash[(int)Parameter.Run], false);
        return BaseNode.ENodeState.Failure;
    }

    protected BaseNode.ENodeState StayIdle()
    {
        Debug.Log("Idle 상태");
        //UnitAnimator.SetTrigger("Idle");
        return BaseNode.ENodeState.Success;
    }

    protected bool CheckAttackRange()
    {
        if (DetectedEnemy == null)
            return false;
        float sqrDistance = Vector2.SqrMagnitude(DetectedEnemy.position - transform.position);
        return sqrDistance <= AttackRange * AttackRange;
    }

    protected BaseNode.ENodeState SetDetectedTarget()
    {
        // 이미 감지된 적이 있었을경우엔 수행할 필요 없음,  바로 chase로 전환
        if (DetectedEnemy != null)
            return BaseNode.ENodeState.Success;
        
        Collider2D[] detectedColliders = Physics2D.OverlapAreaAll(_bottomLeft,_topRight, _enemyLayer);

        if (detectedColliders.Length == 0)
        {
            DetectedEnemy = null;
            return BaseNode.ENodeState.Failure;
        }
        
        float minDistance = float.MaxValue;
        float maxDistance = float.MinValue;
        Transform closetEnemy = null;
        Transform farthestEnemy = null;

        foreach (Collider2D collider in detectedColliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closetEnemy = collider.transform;
            }

            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestEnemy = collider.transform;
            }
        }
        if (IsPriorityTargetFar)
        {
            // 가장 먼 타겟을 DetectedEnemy 로 설정
            DetectedEnemy = farthestEnemy;
        }
        else
        {
            // 가장 가까운 타겟을 DetectedEnemy로 설정
            DetectedEnemy = closetEnemy;
        }

        return BaseNode.ENodeState.Success;
        
    }
    
    
    
    // others
    protected void SetDetectingArea()
    {
        if (Camera.main != null)
        {
            _bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
            _topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        }
    }
    protected void OnDrawGizmos()
    {
        string layerName = LayerMask.LayerToName(gameObject.layer);
        Gizmos.color = (layerName == "UserCharacter") ? Color.green : Color.red;
        
        //Gizmos.color = Color.yellow;
        if(_detectedEnemy != null)
            Gizmos.DrawLine(transform.position, _detectedEnemy.position);

        Gizmos.color = Color.cyan;
        Vector2 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        
        Gizmos.DrawLine(new Vector3(bottomLeft.x, bottomLeft.y, 0), new Vector3(topRight.x, bottomLeft.y, 0)); // 아래쪽
        Gizmos.DrawLine(new Vector3(bottomLeft.x, topRight.y, 0), new Vector3(topRight.x, topRight.y, 0));    // 위쪽
        Gizmos.DrawLine(new Vector3(bottomLeft.x, bottomLeft.y, 0), new Vector3(bottomLeft.x, topRight.y, 0)); // 왼쪽
        Gizmos.DrawLine(new Vector3(topRight.x, bottomLeft.y, 0), new Vector3(topRight.x, topRight.y, 0));    // 오른쪽

        /*Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);*/
    }
}

/*private BaseNode.ENodeState CheckAutoOn()
{
    if (BattleManager.Instance.IsAutoBattle)
    {
        return BaseNode.ENodeState.Success;
    }

    return BaseNode.ENodeState.Failure;
}

private BaseNode.ENodeState DetectEnemys()
{
    var overlapColliders = Physics2D.OverlapCircleAll(transform.position, _detectRange, LayerMask.GetMask("Player"));
    if (overlapColliders != null && overlapColliders.Length > 0)
    {
        _detectedEnemy = overlapColliders[0].transform;
        return BaseNode.ENodeState.Success;
    }

    _detectedEnemy = null;
    return BaseNode.ENodeState.Failure;
}


private BaseNode.ENodeState MoveToEnemy()
{
    if (_detectedEnemy != null)
    {
        float sqrDistance = Vector2.SqrMagnitude(_detectedEnemy.position - transform.position);
        if (sqrDistance < _attackRange * _attackRange)
        {
            return BaseNode.ENodeState.Success;
        }

        if (sqrDistance > Mathf.Epsilon)
        {
            transform.position = Vector2.MoveTowards(transform.position, _detectedEnemy.position, _moveSpeed * Time.deltaTime);
            return BaseNode.ENodeState.Running;
        }

    }
    return BaseNode.ENodeState.Failure;
}

private BaseNode.ENodeState DoAttack()
{
    if (IsAnimationRunning("attackStateNameTemp"))
    {
        return BaseNode.ENodeState.Running;
    }

    return BaseNode.ENodeState.Success;
}

private BaseNode.ENodeState TempMethod()
{
    return BaseNode.ENodeState.Success;
}*/

/*if (_detectedEnemy != null)
            return true;

        // 현재 카메라에 보이는 전체 영역 탐지

        //Rect screenRect = new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        Collider2D[] detectedEnemys = Physics2D.OverlapAreaAll(_bottomLeft,_topRight, _enemyLayer);

        if (detectedEnemys.Length > 0)
        {
            _detectedEnemy = detectedEnemys[0].transform;
            return true;
        }

        return false;*/
        
/*// 가장 먼저 탐지한 적을 우선적으로 공격할 경우
Collider2D[] detectedColliders = Physics2D.OverlapCircleAll(transform.position, _detectRange, _enemyLayer);
if (detectedColliders.Length > 0)
{
    _detectedEnemy = detectedColliders[0].transform;
    return true;
}
_detectedEnemy = null;
return false;*/