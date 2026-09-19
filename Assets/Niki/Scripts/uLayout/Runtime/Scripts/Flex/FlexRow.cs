/*
    Copyright (c) 2026 Nikita Mitrovich

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
*/
using UnityEngine;

namespace CupOHappiness.UI
{
    /// <summary>
    /// Layout preset locked to the Row direction (or RowReverse when
    /// reversed). Add via uLayout > Flex > Flex Row.
    /// </summary>
    [AddComponentMenu("uLayout/Flex/Flex Row")]
    public class FlexRow : Layout
    {
        [Header("Direction")]
        [Tooltip("When checked, the row flows right to left.")]
        [SerializeField] protected bool m_reverse;

        public bool Reverse {
            get => m_reverse;
            set { if(m_reverse != value) { m_reverse = value; SetDirty(); } }
        }

        private LayoutDirection DefaultDirection => m_reverse ? LayoutDirection.RowReverse : LayoutDirection.Row;

        protected override void OnValidate() {
            EnforceDirection();
            base.OnValidate();
        }

        protected override void OnEnable() {
            EnforceDirection();
            base.OnEnable();
        }

        public override void SetDirty() {
            EnforceDirection();
            base.SetDirty();
        }

        private void EnforceDirection() {
            m_direction = DefaultDirection;
        }
    }
}

