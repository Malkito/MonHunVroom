using System;
using UnityEngine;

namespace LordBreakerX.States
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CreateStateMenuAttribute : System.Attribute
    {
        /// <summary>
        /// The default name of the state file when created through the builder menu.
        /// </summary>
        public readonly string fileName;

        /// <summary>
        /// The name of the state in the builder menu
        /// </summary>
        public readonly string menuName;

        /// <summary>
        /// The group of states that the state belongs to in the builder menu.
        /// </summary>
        public readonly string groupName;

        /// <summary>
        /// The position of the state in its group in the builder menu.
        /// </summary>
        public readonly int order;

        public CreateStateMenuAttribute(string fileName = "", string menuName = "", string groupName = "Custom", int order = 0)
        {
            this.fileName = fileName;
            this.menuName = menuName;
            this.groupName = groupName;
            this.order = order;
        }
    }
}
