using UnityEngine;

namespace LordBreakerX.Utilities
{
    public static class LayerMaskUtility
    {
        public static bool Contains(this LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }
    }
}
