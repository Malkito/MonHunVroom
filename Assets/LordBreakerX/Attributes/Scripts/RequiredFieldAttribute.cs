using UnityEngine;

namespace LordBreakerX.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RequiredFieldAttribute : PropertyAttribute
    {
        public RequiredFieldAttribute()
        {

        }
    }
}