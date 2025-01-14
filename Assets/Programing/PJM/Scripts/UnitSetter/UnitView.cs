using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Parameter
{
    Idle, Run ,Attack, Skill0, Skill1, Win ,Die, Size
}

public class UnitView : MonoBehaviour
{

    public BaseUnitController unit;
    private Animator _unitAnimator;
    public Animator UnitAnimator { get => _unitAnimator;}

    private int[] _parameterHash;

    public int[] ParameterHash { get => _parameterHash; private set => _parameterHash = value; }
    //public AnimatorStateInfo stateInfo;

    private int[] _animationHash = new int[]
    {
        Animator.StringToHash("IdleState"),
        Animator.StringToHash("Attacking"),
    };



    private void Awake()
    {
        ParameterHash = new int[(int)Parameter.Size];
        ParameterHash[(int)Parameter.Idle] = Animator.StringToHash("Idle");
        ParameterHash[(int)Parameter.Run] = Animator.StringToHash("Run");
        ParameterHash[(int)Parameter.Attack] = Animator.StringToHash("Attack");
        ParameterHash[(int)Parameter.Skill0] = Animator.StringToHash("Skill0");
        ParameterHash[(int)Parameter.Skill1] = Animator.StringToHash("Skill1");
        ParameterHash[(int)Parameter.Win] = Animator.StringToHash("Win");
        ParameterHash[(int)Parameter.Die] = Animator.StringToHash("Die");
    }

    private void Start()
    {
        unit = GetComponent<BaseUnitController>();
        _unitAnimator = GetComponentInChildren<Animator>();
    }
    
    public bool IsAnimationRunning(string stateName)
    {
        /*if (UnitAnimator.IsInTransition(0))
        {
            Debug.Log($"트랜지션 중, 상태: {stateName}");
            return true; 
            // 트랜지션 중일 때도 실행 중으로 간주
        }*/
        
        if (UnitAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            var normalizedTime = UnitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            return normalizedTime != 0 && normalizedTime < 1.0f;
        }
        return false;
    }

    public void CheckNeedFlip(Transform unit1, Transform unit2)
    {
        if ((unit1.position.x > unit2.position.x && unit1.localScale.x < 0) || // 오른쪽을 봐야 하지만 왼쪽을 보고 있는 경우
            (unit1.position.x < unit2.position.x && unit1.localScale.x > 0))   // 왼쪽을 봐야 하지만 오른쪽을 보고 있는 경우
        {
            Vector3 newScale = unit1.localScale;
            newScale.x *= -1;
            unit1.localScale = newScale;
        }
    }
    
    private void FlipScaleX(bool boolValue)
    {
        Vector3 newScale = transform.localScale; // 현재 로컬 스케일 가져오기
        newScale.x = boolValue ? -Mathf.Abs(newScale.x) : Mathf.Abs(newScale.x); // X 값을 양수/음수로 설정
        transform.localScale = newScale; // 수정된 스케일 다시 적용
    }

    /*public void PlayAnimation(int animationHash)
    {
        if (animationHash >= 0 && animationHash < animationHash.Length)
        {
            unitAnimator.Play(_animationHash[animationHash],0,0);
        }
        else
        {
            Debug.LogError("애니메이션 해시 범위 오류");
        }
    }

    public bool IsAnimationFinished()
    {
        stateInfo = unitAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/


}
