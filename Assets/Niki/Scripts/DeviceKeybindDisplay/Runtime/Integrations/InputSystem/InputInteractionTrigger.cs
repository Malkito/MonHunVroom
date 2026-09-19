using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    /// <summary>
    /// Reusable trigger zone for the built-in Unity Input System.
    /// While a PlayerInput is inside the trigger, pressing the configured action invokes the event.
    /// </summary>
    [MovedFrom(true, "CupOHappiness.DeviceKeybindDisplay.Integrations.UnityInputActions", "CupOHappiness.DeviceKeybindDisplay.UnityInputActions", "UnityInputActionInteractionTrigger")]
    public class InputInteractionTrigger : MonoBehaviour
    {
        [Header("Binding Target")]
        [SerializeField, Tooltip("Optional action map name used to narrow the action lookup before falling back to action name only.")]
        private string actionMapName;
        [SerializeField, Tooltip("Action to listen for while a player is inside the trigger.")]
        private string actionName = "Confirm";

        [Header("Events")]
        [SerializeField, Tooltip("Invoked when a player presses the configured action while inside the trigger.")]
        private UnityEvent<GameObject, PlayerInput> onConfirmed;

        private sealed class ActiveInteraction
        {
            public PlayerInput PlayerInput;
            public InputAction Action;
            public System.Action<InputAction.CallbackContext> Callback;
        }

        private readonly Dictionary<Collider, ActiveInteraction> _activeInteractions = new();

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(
                $"[InputInteractionTrigger] Trigger enter. other='{other.name}', tag='{other.tag}', layer={other.gameObject.layer}, root='{other.transform.root.name}', attachedRigidbody='{other.attachedRigidbody?.name ?? "null"}'.",
                this);

            if (_activeInteractions.ContainsKey(other))
            {
                Debug.Log($"[InputInteractionTrigger] Collider '{other.name}' is already tracked.", this);
                return;
            }

            var playerInput = other.GetComponentInParent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.Log($"[InputInteractionTrigger] No PlayerInput found in parents of '{other.name}'.", this);
                return;
            }

            if (playerInput.actions == null)
            {
                Debug.Log($"[InputInteractionTrigger] PlayerInput found on '{other.name}' but actions is null.", this);
                return;
            }

            var action = ResolveAction(playerInput);
            if (action == null)
            {
                Debug.Log($"[InputInteractionTrigger] Failed to resolve action '{actionMapName}/{actionName}' for '{other.name}'.", this);
                return;
            }

            Debug.Log($"[InputInteractionTrigger] Player detected. playerObject='{other.gameObject.name}', actionMap='{actionMapName}', action='{actionName}', resolvedAction='{action.name}'.", this);

            System.Action<InputAction.CallbackContext> callback = context => OnActionPerformed(other.gameObject, playerInput, context);
            action.performed += callback;

            _activeInteractions[other] = new ActiveInteraction
            {
                PlayerInput = playerInput,
                Action = action,
                Callback = callback
            };
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log(
                $"[InputInteractionTrigger] Trigger exit. other='{other.name}', tag='{other.tag}', layer={other.gameObject.layer}, root='{other.transform.root.name}'.",
                this);

            if (!_activeInteractions.TryGetValue(other, out var interaction))
            {
                Debug.Log($"[InputInteractionTrigger] Exit ignored. No active interaction tracked for '{other.name}'.", this);
                return;
            }

            if (interaction.Action != null && interaction.Callback != null)
            {
                Debug.Log($"[InputInteractionTrigger] Unsubscribing action '{interaction.Action.name}' for '{other.name}'.", this);
                interaction.Action.performed -= interaction.Callback;
            }

            _activeInteractions.Remove(other);
        }

        private void OnDestroy()
        {
            foreach (var pair in _activeInteractions)
            {
                var interaction = pair.Value;
                if (interaction?.Action != null && interaction.Callback != null)
                {
                    interaction.Action.performed -= interaction.Callback;
                }
            }

            _activeInteractions.Clear();
        }

        private InputAction ResolveAction(PlayerInput playerInput)
        {
            if (playerInput == null || playerInput.actions == null || string.IsNullOrEmpty(actionName))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(actionMapName))
            {
                var mappedAction = playerInput.actions.FindAction($"{actionMapName}/{actionName}", throwIfNotFound: false);
                if (mappedAction != null)
                {
                    return mappedAction;
                }
            }

            return playerInput.actions.FindAction(actionName, throwIfNotFound: false);
        }

        private void OnActionPerformed(GameObject playerObject, PlayerInput playerInput, InputAction.CallbackContext _)
        {
            Debug.Log($"[InputInteractionTrigger] Confirmed by '{playerObject.name}' using PlayerInput '{playerInput.name}'.", this);
            onConfirmed?.Invoke(playerObject, playerInput);
        }
    }
}
