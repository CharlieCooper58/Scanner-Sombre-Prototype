using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Animation
{
    public class AnimatorHandler : MonoBehaviour
    {
        int currentStatePriority;
        Animator _animator;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        public void PlayTargetAnimation(string animationName, int priority)
        {
            if (priority > currentStatePriority)
            {
                currentStatePriority = priority;
                _animator.Play(animationName);
            }
        }

        public void ResetStatePriority()
        {
            currentStatePriority = 0;
        }
    }
}

