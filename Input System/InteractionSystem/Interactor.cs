using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace InteractionSystem
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float _interactRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;
        
        private IInteractable _currentInteractable;
        private CancellationTokenSource _cancellationToken;
        private Collider2D[] _colliders = new Collider2D[8];
        
        private async void FindInteractable()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(60, _cancellationToken.Token);
                
                int foundCollidersCount = Physics2D.OverlapCircleNonAlloc(transform.position, _interactRadius, _colliders, _interactableLayer);
                
                if (foundCollidersCount == 0)
                {
                    if (_currentInteractable == null) continue;
                    
                    StopInteraction(_currentInteractable);
                    continue;
                }

                float[] distances = new float[foundCollidersCount];
                for (int i = 0; i < foundCollidersCount; i++) distances[i] = (_colliders[i].transform.position - transform.position).sqrMagnitude;

                if (!_colliders[Array.IndexOf(distances, distances.Min())].TryGetComponent(out IInteractable interactable) && interactable == _currentInteractable) continue;
                
                _currentInteractable?.StopInteraction();
                StartInteraction(interactable);
                
                Array.Clear(_colliders, 0, foundCollidersCount);
            }
        }

        public void StartInteraction(IInteractable interactable)
        {
            interactable.StartInteraction();
            _currentInteractable = interactable;
        }

        public void StopInteraction(IInteractable interactable)
        {
            interactable.StopInteraction();
            _currentInteractable = null;
        }

        private void OnEnable()
        {
            _cancellationToken = new();
            FindInteractable();
        }

        private void OnDisable()
        {
            if (_currentInteractable != null) StopInteraction(_currentInteractable);
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
        }

        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, _interactRadius);
    }
}