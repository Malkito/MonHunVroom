using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CupOHappiness
{
    public sealed class BarsManager : MonoBehaviour
    {
        public float lossAmount = 0.3f;
        public float fillAmount = 0.2f;
        public float customDuration = 0.4f;
        public bool useCustomDuration;
        public List<ProceduralProgressBar> bars = new();

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.qKey.wasPressedThisFrame)
                foreach (var bar in bars)
                    if (bar != null)
                        if (useCustomDuration) bar.BarLoss(lossAmount, customDuration);
                        else bar.BarLoss(lossAmount);

            if (keyboard.eKey.wasPressedThisFrame)
                foreach (var bar in bars)
                    if (bar != null)
                        if (useCustomDuration) bar.BarFill(fillAmount, customDuration);
                        else bar.BarFill(fillAmount);

            if (keyboard.rKey.wasPressedThisFrame)
                foreach (var bar in bars)
                    if (bar != null)
                        bar.UpdateBarFillAmount(0.5f);
        }
    }
}
