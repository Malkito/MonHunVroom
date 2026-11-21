using LordBreakerX.EditorUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LordBreakerX.States
{
    public class StateAttributeResult
    {
        public Type ModifierType { get; private set; }
        public CreateStateMenuAttribute Attribute { get; private set;  }

        public StateAttributeResult(Type modifierType, CreateStateMenuAttribute attribute)
        {
            this.ModifierType = modifierType;
            this.Attribute = attribute;
        }
    }

    public class StateBuilderMenu : BuilderMenu<StateReference>
    {
        public StateBuilderMenu(Action<Builder<StateReference>> onClose) : base(onClose)
        {
        }

        protected override string BuilderTitle => "States";

        protected override IEnumerable<Builder<StateReference>> GetBuilders()
        {
            List<Builder<StateReference>> stateBuilders = new List<Builder<StateReference>>();
            
            foreach(StateAttributeResult result in GetTypesWithAttribute())
            {
                string fileName = result.Attribute.fileName;
                string menuName = result.Attribute.menuName;
                string groupName = result.Attribute.groupName;

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = result.ModifierType.Name;
                }

                if (string.IsNullOrEmpty(menuName))
                {
                    menuName = result.ModifierType.Name;
                }

                State state = (State)Activator.CreateInstance(result.ModifierType);
                state.ID = fileName;

                Texture icon = IconUtility.GetTypeIcon(result.ModifierType);

                StateReference stateRef = StateReference.Create(state);
                stateRef.name = fileName;

                Builder<StateReference> builder = new Builder<StateReference>(icon, groupName, menuName, stateRef);
                stateBuilders.Add(builder);
            }

            return stateBuilders;
        }

        private static List<StateAttributeResult> GetTypesWithAttribute()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly =>
                {
                    try { return assembly.GetTypes(); }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types.Where(t => t != null);
                    }
                })
                .Select(type => new
                {
                    Type = type,
                    Attribute = type.GetCustomAttribute<CreateStateMenuAttribute>()
                })
                .Where(x => x.Attribute != null)
                .Select(x => new StateAttributeResult(x.Type, x.Attribute))
                .ToList();
        }
    }
}
