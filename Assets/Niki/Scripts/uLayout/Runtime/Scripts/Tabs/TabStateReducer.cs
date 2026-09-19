using System;

namespace CupOHappiness.UI
{
    public enum TabCommandType
    {
        Select,
        Clear,
        Next,
        Previous
    }

    public readonly struct TabCommand
    {
        public readonly TabCommandType Type;
        public readonly int Index;

        public TabCommand(TabCommandType type, int index = -1)
        {
            Type = type;
            Index = index;
        }
    }

    public readonly struct TabState
    {
        public readonly int SelectedIndex;

        public TabState(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }

    public readonly struct TabSnapshot
    {
        public readonly bool[] Selectable;
        public readonly bool AllowNoSelection;
        public readonly bool WrapNavigation;

        public int Count => Selectable?.Length ?? 0;

        public TabSnapshot(bool[] selectable, bool allowNoSelection, bool wrapNavigation)
        {
            Selectable = selectable;
            AllowNoSelection = allowNoSelection;
            WrapNavigation = wrapNavigation;
        }
    }

    public enum TabEffectType
    {
        ApplySelection,
        NotifyDeselected,
        NotifySelected,
        NotifyIndexChanged
    }

    public readonly struct TabEffect
    {
        public readonly TabEffectType Type;
        public readonly int Index;

        public TabEffect(TabEffectType type, int index)
        {
            Type = type;
            Index = index;
        }
    }

    public readonly struct TabTransition
    {
        public readonly bool Accepted;
        public readonly TabState State;
        public readonly TabEffect[] Effects;

        public bool Changed => Effects != null && Effects.Length > 0;

        public TabTransition(bool accepted, TabState state, TabEffect[] effects)
        {
            Accepted = accepted;
            State = state;
            Effects = effects ?? Array.Empty<TabEffect>();
        }
    }

    public static class TabStateReducer
    {
        public static TabTransition Reduce(
            TabState current,
            TabSnapshot snapshot,
            TabCommand command)
        {
            int next;
            switch(command.Type)
            {
                case TabCommandType.Select:
                    next = command.Index;
                    break;
                case TabCommandType.Clear:
                    next = -1;
                    break;
                case TabCommandType.Next:
                    return Navigate(current, snapshot, 1);
                case TabCommandType.Previous:
                    return Navigate(current, snapshot, -1);
                default:
                    return Reject(current);
            }

            if(next == -1) {
                if(!snapshot.AllowNoSelection)
                    return Reject(current);
            }
            else if(!IsSelectable(snapshot, next)) {
                return Reject(current);
            }

            if(next == current.SelectedIndex)
                return new TabTransition(true, current, Array.Empty<TabEffect>());

            return Accept(current, next);
        }

        private static TabTransition Navigate(TabState current, TabSnapshot snapshot, int direction)
        {
            if(snapshot.Count == 0)
                return Reject(current);

            int start = current.SelectedIndex >= 0 && current.SelectedIndex < snapshot.Count
                ? current.SelectedIndex
                : direction > 0 ? -1 : snapshot.Count;

            for(int step = 1; step <= snapshot.Count; step++) {
                int candidate = start + direction * step;
                if(snapshot.WrapNavigation)
                    candidate = (candidate % snapshot.Count + snapshot.Count) % snapshot.Count;
                else if(candidate < 0 || candidate >= snapshot.Count)
                    return Reject(current);

                if(IsSelectable(snapshot, candidate))
                    return Accept(current, candidate);
            }

            return Reject(current);
        }

        private static bool IsSelectable(TabSnapshot snapshot, int index)
        {
            return index >= 0 && index < snapshot.Count && snapshot.Selectable[index];
        }

        private static TabTransition Reject(TabState current)
        {
            return new TabTransition(false, current, Array.Empty<TabEffect>());
        }

        private static TabTransition Accept(TabState current, int next)
        {
            int effectCount = 2 + (current.SelectedIndex >= 0 ? 1 : 0) + (next >= 0 ? 1 : 0);
            TabEffect[] effects = new TabEffect[effectCount];
            int effectIndex = 0;
            effects[effectIndex++] = new TabEffect(TabEffectType.ApplySelection, next);
            if(current.SelectedIndex >= 0)
                effects[effectIndex++] = new TabEffect(TabEffectType.NotifyDeselected, current.SelectedIndex);
            if(next >= 0)
                effects[effectIndex++] = new TabEffect(TabEffectType.NotifySelected, next);
            effects[effectIndex] = new TabEffect(TabEffectType.NotifyIndexChanged, next);
            return new TabTransition(true, new TabState(next), effects);
        }
    }
}
