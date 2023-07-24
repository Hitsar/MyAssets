using UnityEngine;

namespace Player
{
    public class PlayerVFX
    {
        private readonly Animator _animator;
        
        public PlayerVFX(PlayerMovement playerMovement, Animator animator)
        {
            _animator = animator;
            
            playerMovement.OnMove += Move;
            playerMovement.OnSprint += Sprint;
            playerMovement.OnJump += Jump;
            playerMovement.OnCouch += Couch;
            playerMovement.OnDash += Dash;
            playerMovement.OnFall += Fall;
        }
        
        private void Move(bool isStand) => _animator.SetBool("IsStand", isStand);
        
        private void Sprint(bool isSprint) => _animator.SetBool("IsSprint", isSprint);

        private void Jump() => _animator.SetTrigger("Jump");
        
        private void Couch(bool isCouch) => _animator.SetBool("IsCouch", isCouch);

        private void Dash() => _animator.SetTrigger("Dash");

        private void Fall(bool isFall) => _animator.SetBool("IsFall", isFall);
    }
}