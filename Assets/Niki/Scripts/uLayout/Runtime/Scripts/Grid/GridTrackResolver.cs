using System;
using UnityEngine;

namespace CupOHappiness.UI
{
    /// <summary>
    /// The resolved positions and sizes for one repeated grid track definition.
    /// The same plan is used for measurement and child placement.
    /// </summary>
    public sealed class GridTrackPlan
    {
        public int Count { get; }
        public float[] Sizes { get; }
        public float[] Positions { get; }
        public float TotalSize { get; }

        internal GridTrackPlan(float[] sizes, float[] positions, float totalSize) {
            Sizes = sizes;
            Positions = positions;
            Count = sizes.Length;
            TotalSize = totalSize;
        }

        public float UniformSize => Count == 0 ? 0 : Sizes[0];
    }

    /// <summary>
    /// Axis-neutral resolver for the repeated track contract used by GridLayout.
    /// </summary>
    public static class GridTrackResolver
    {
        // Keep inspector applicability rules next to the runtime contract.
        public static bool UsesCount(GridTrackMode mode) => mode == GridTrackMode.FixedCount;
        public static bool UsesMinTrackSize(GridTrackMode mode) => true;
        public static bool UsesFillExtraSpace(GridTrackMode mode) => true;

        public static GridTrackPlan Resolve(
            GridTrackDef definition,
            float availableExtent,
            float gap,
            int childDemand,
            bool fitContent) {
            definition = Sanitize(definition);
            gap = Mathf.Max(0, gap);
            int demand = Mathf.Max(0, childDemand);
            int maxTracks = fitContent
                ? Mathf.Max(1, demand)
                : MaxTrackCount(availableExtent, definition.minTrackSize, gap);

            int count;
            switch(definition.mode) {
                case GridTrackMode.FixedCount:
                    // FixedCount is an explicit track count. Content flows to
                    // the other axis; it must not change this axis' declared
                    // measurement or placement plan.
                    count = definition.count;
                    break;
                case GridTrackMode.AutoFit:
                    // Fit unused capacity, but preserve every demanded track
                    // when the container is smaller than its minimums.
                    count = Mathf.Max(1, Mathf.Min(maxTracks, demand));
                    count = Mathf.Max(count, demand);
                    break;
                case GridTrackMode.AutoFill:
                    count = Mathf.Max(1, maxTracks, demand);
                    break;
                default:
                    count = 1;
                    break;
            }

            float minimum = definition.minTrackSize;
            float free = availableExtent - Mathf.Max(0, count - 1) * gap - count * minimum;
            float trackSize = minimum;
            if(definition.fillExtraSpace > 0 && free > 0)
                trackSize += free / count;

            float[] sizes = new float[count];
            float[] positions = new float[count];
            float position = 0;
            for(int i = 0; i < count; i++) {
                sizes[i] = trackSize;
                positions[i] = position;
                position += trackSize + gap;
            }

            float totalSize = count * trackSize + Mathf.Max(0, count - 1) * gap;
            return new GridTrackPlan(sizes, positions, totalSize);
        }

        public static GridTrackDef Sanitize(GridTrackDef definition) {
            if(!Enum.IsDefined(typeof(GridTrackMode), definition.mode))
                definition.mode = GridTrackMode.AutoFit;
            definition.count = Mathf.Max(1, definition.count);
            definition.minTrackSize = Mathf.Max(0, definition.minTrackSize);
            definition.fillExtraSpace = Mathf.Max(0, definition.fillExtraSpace);
            return definition;
        }

        private static int MaxTrackCount(float availableExtent, float minimum, float gap) {
            float safeMinimum = Mathf.Max(1f, minimum);
            int possible = Mathf.FloorToInt((availableExtent + gap) / (safeMinimum + gap));
            return Mathf.Max(1, possible);
        }
    }
}
