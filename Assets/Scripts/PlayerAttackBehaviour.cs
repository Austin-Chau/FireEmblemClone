using UnityEngine;

class PlayerAttackBehaviour : StateMachineBehaviour
{
    public Unit.ActionCallbackContainer callbackContainer { private get; set; }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        callbackContainer.PerformCallback();
    }
}