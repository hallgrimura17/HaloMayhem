using UnityEngine;

public static class AnimationExtensions
{
    public static float GetClipTimeRemaining(this Animator animator, int layerIndex)
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.length - (stateInfo.normalizedTime % stateInfo.length);
    }
}