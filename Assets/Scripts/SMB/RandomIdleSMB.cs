using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomIdleSMB : StateMachineBehaviour
{
    public int numberOfStates = 2;
    public float minNormTime = 0f;
    public float maxNormTime = 5f;

    protected float _randomNormTime;

    readonly int _hashRandomIdle = Animator.StringToHash("RandomIdle");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _randomNormTime = Random.Range(minNormTime, maxNormTime);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateInfo.fullPathHash)
        {
            animator.SetInteger(_hashRandomIdle, -1);
        }

        if (stateInfo.normalizedTime > _randomNormTime && !animator.IsInTransition(0))
        {
            animator.SetInteger(_hashRandomIdle, Random.Range(0, numberOfStates));
        }
    }
}
