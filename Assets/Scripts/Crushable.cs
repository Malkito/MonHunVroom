using LordBreakerX.Utilities;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Crushable : NetworkBehaviour
{
    [SerializeField]
    [Tooltip("Will this object be destoyed after being fully crushed")]
    private bool _destroyAfterAnimation = false;

    [SerializeField]
    [Tooltip("The layers of objects that can crush this object")]
    private LayerMask _crusherLayers;

    [SerializeField]
    [Tooltip("controls the crushing of the object")]
    private AnimationCurve _yScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private float _currentCrushTime = 0;

    private float _orignalYScale;

    private float _fullyCrushedTime;

    private GameObject _crusher;

    private Collider _crushableCollider;

    private bool _isFullyCrushed = false;

    private void Awake()
    {
        _orignalYScale = transform.localScale.y;

        _fullyCrushedTime = _yScaleCurve.keys[_yScaleCurve.keys.Length - 1].time;

        _crushableCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_crusherLayers.Contains(collision.gameObject.layer))
        {
            _crusher = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == _crusher) 
        {
            _crusher = null;
        }
    }

    private void Update()
    {
        if (_crusher != null && !_isFullyCrushed)
        {
            _currentCrushTime += Time.deltaTime;
            float yScale = _orignalYScale * _yScaleCurve.Evaluate(_currentCrushTime);
            transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);

            if (_currentCrushTime >= _fullyCrushedTime)
            {

                _crushableCollider.isTrigger = true;

                if (_destroyAfterAnimation)
                    Destroy(gameObject);
                else
                    Destroy(this);
            }
        }
    }
}
