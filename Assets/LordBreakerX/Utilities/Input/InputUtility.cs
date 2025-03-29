using UnityEngine;
using UnityEngine.InputSystem;

namespace LordBreakerX.Utilities.Input
{
    public static class InputUtility
    {

        /// <summary>
        /// Gets the input map from an input action asset with the ability to have a custom unity warning when its not found
        /// </summary>
        /// <param name="inputAsset"> the input action asset to find the action map from </param>
        /// <param name="message"> The warning message to show when no input action map was found </param>
        /// <param name="mapNameOrID"> the name or id of the input action map to use </param>
        /// <returns> the input action map OR null if not found</returns>
        public static InputActionMap GetInputMap(this InputActionAsset inputAsset, string mapNameOrID, string message = "")
        {
            InputActionMap inputMap = inputAsset.FindActionMap(mapNameOrID);

            if (inputMap == null && !string.IsNullOrEmpty(message)) Debug.LogWarning(message);

            return inputMap; 
        }

        /// <summary>
        /// Gets the input map from an input action asset with a generic warning message when is not found
        /// </summary>
        /// <param name="inputAsset"> the input action asset to find the action map from </param>
        /// <param name="mapNameOrID"> the name or id of the input action map to use </param>
        /// <returns></returns>
        public static InputActionMap GetInputMap(this InputActionAsset inputAsset, string mapNameOrID)
        {
            return GetInputMap(inputAsset, mapNameOrID, $"Input Action Map with ID {mapNameOrID} is missing from the provided input action asset but is required!");
        }

        /// <summary>
        /// Gets the input action from an input action map with the ability to have a custom unity warning when its not found
        /// </summary>
        /// <param name="inputMap"> the input action map to find the action map from </param>
        /// <param name="actionNameOrId"> the name or id of the input action to use  </param>
        /// <param name="message"> the warning message to show when no input action was found </param>
        /// <returns> the input action OR null if not found </returns>
        public static InputAction GetInputAction(this InputActionMap inputMap, string actionNameOrId, string message = "")
        {
            InputAction action = inputMap.FindAction(actionNameOrId);
            if (action == null && !string.IsNullOrEmpty(message)) Debug.LogWarning(message);
            return action;
        }

        /// <summary>
        /// Gets the input action from an input action map with a generic warning message when is not found
        /// </summary>
        /// <param name="inputMap"> the input action map to find the action map from </param>
        /// <param name="actionNameOrId"> the name or id of the input action to use </param>
        /// <returns></returns>
        public static InputAction GetInputAction(this InputActionMap inputMap, string actionNameOrId)
        {
            return GetInputAction(inputMap, actionNameOrId, $"Input Action with ID {actionNameOrId} is missing from the provided Action Map but is required!");
        }

        /// <summary>
        /// Enables the specified input action and subscribes the provided callback to both performed and canceled events.
        /// </summary>
        /// <param name="inputAction">The input action to enable.</param>
        /// <param name="callback">The callback to invoke when the action is performed or canceled.</param>
        public static void ActivateInputAction(InputAction inputAction, System.Action<InputAction.CallbackContext> callback)
        {
            if (inputAction == null) return;

            inputAction.Enable();
            inputAction.performed += callback;
            inputAction.canceled += callback;
        }

        /// <summary>
        /// Disables the specified input action and unsubscribes the provided callback from both performed and canceled events.
        /// </summary>
        /// <param name="inputAction">The input action to disable.</param>
        /// <param name="callback">The callback to remove from the action's performed and canceled events.</param>
        public static void DeactivateInputAction(InputAction inputAction, System.Action<InputAction.CallbackContext> callback)
        {
            if (inputAction == null) return;

            inputAction.performed -= callback;
            inputAction.canceled -= callback;
            inputAction.Disable();
        }
    }
}
