using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CupOHappiness.Toon
{
    /// <summary>Schedules the CupOHappiness Toon outline pass in Universal Render Pipeline.</summary>
    public sealed class CupOHappinessToonOutlineRendererFeature : ScriptableRendererFeature
    {
        public enum InjectionPoint
        {
            AfterRenderingOpaques = RenderPassEvent.AfterRenderingOpaques,
            BeforeRenderingTransparents = RenderPassEvent.BeforeRenderingTransparents,
            AfterRenderingTransparents = RenderPassEvent.AfterRenderingTransparents
        }

        [SerializeField] private LayerMask layerMask = -1;
        [SerializeField] private InjectionPoint injectionPoint = InjectionPoint.AfterRenderingTransparents;

        private RenderObjects renderObjects;

        public override void Create()
        {
            renderObjects = ScriptableObject.CreateInstance<RenderObjects>();
            renderObjects.settings.passTag = "CupOHappiness Toon Outline";
            renderObjects.settings.filterSettings.LayerMask = layerMask;
            renderObjects.settings.filterSettings.PassNames = new[] { "CupOHappinessOutline" };
            renderObjects.settings.Event = (RenderPassEvent)injectionPoint;
            renderObjects.Create();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderObjects.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
