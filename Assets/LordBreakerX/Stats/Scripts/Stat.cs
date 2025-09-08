using LordBreakerX.Utilities.Math;
using UnityEngine;

namespace LordBreakerX.Stats
{
    [System.Serializable]
    public class Stat
    {
        private string _id;

        [SerializeField]
        private StatType _statType;

        [SerializeField]
        private float _value; // spicific value

        [SerializeField]
        private float _minValue; // range value

        [SerializeField]
        private float _maxValue; // range value

        [SerializeField]
        private AnimationCurve _curveValue = GetDefaultCurvedValue(); // curved value

        public string ID { get => _id; }

        public float GetValue(float percentage)
        {
            return _statType switch
            {
                StatType.Static => _value,
                StatType.Range => Percentage.MapToFloat(percentage, _minValue, _maxValue),
                StatType.Curve => _curveValue.Evaluate(percentage / 100),
                _ => 0,
            };
        }

        public Stat(string id, float value)
        {
            _id = id;
            _statType = StatType.Static;
            _value = value;
        }

        public Stat(string id, float minValue, float maxValue)
        {
            _id = id;
            _statType = StatType.Range;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public Stat(string id, AnimationCurve curvedValue)
        {
            _id = id;
            _statType = StatType.Curve;
            _curveValue = curvedValue;
        }

        private static AnimationCurve GetDefaultCurvedValue()
        {
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0, 0);
            animationCurve.AddKey(100, 100);

            return animationCurve;
        }

        public void OnValidation()
        {
            EnsureKeyAtTime(0f);
            EnsureKeyAtTime(100f);
        }

        private void EnsureKeyAtTime(float time)
        {
            foreach (Keyframe keyframe in _curveValue.keys)
            {
                if (keyframe.time == time)
                {
                    return;
                }
            }

            float value = _curveValue.Evaluate(time);
            _curveValue.AddKey(new Keyframe(time, value));
        }
    }
}
