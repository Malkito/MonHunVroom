using LordBreakerX.Utilities.Tags;
using UnityEngine;

public class Tail : MonoBehaviour
{
    [SerializeField]
    private Animator _monsterAnimator;

    [SerializeField]
    private GameObject _monster;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = "Monster";

    [SerializeField]
    private float _damage = 50;

    private void OnTriggerEnter(Collider other)
    {
        AnimatorStateInfo stateInfo = _monsterAnimator.GetCurrentAnimatorStateInfo(0);

        if (!other.gameObject.CompareTag(_monsterTag) && stateInfo.IsName("tail swipe"))
        {
            dealDamage damage = other.gameObject.GetComponent<dealDamage>();
            if (damage != null) damage.dealDamage(_damage, Color.red, _monster);
        }
    }
}
