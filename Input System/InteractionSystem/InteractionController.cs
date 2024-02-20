using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    public abstract class InteractionController : MonoBehaviour, IInteractable
    {
        [SerializeField] protected TMP_Text _interactText;
        private InputAction _inputAction;

        protected virtual void Start()
        {
            InputSystem inputSystem = new();
            inputSystem.Enable();
            
            _interactText.text = $"Press {inputSystem.Player.Interact.GetBindingDisplayString()} to interact";
            SetInputAction(inputSystem.Player.Interact);
        }
        
        public virtual void StartInteraction()
        {
            _inputAction.performed += Interact;
            _interactText.gameObject.SetActive(true);
        }

        public virtual void StopInteraction()
        {
            _inputAction.performed -= Interact;
            _interactText.gameObject.SetActive(false);
        }

        protected void SetInputAction(InputAction inputAction) => _inputAction = inputAction;

        protected abstract void Interact(InputAction.CallbackContext callbackContext);
    }
}