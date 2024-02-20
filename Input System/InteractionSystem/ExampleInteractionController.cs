using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    public class ExampleInteractionController : InteractionController
    {
        protected override void Interact(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("<color=red>Interact!</color>", gameObject);
        }
    }
}