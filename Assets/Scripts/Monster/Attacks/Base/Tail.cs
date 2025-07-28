using LordBreakerX.Attributes;
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
    private float _launchPower = 50;

    private void OnTriggerEnter(Collider other)
    {
        AnimatorStateInfo stateInfo = _monsterAnimator.GetCurrentAnimatorStateInfo(0);

        if (!other.gameObject.CompareTag(_monsterTag) && stateInfo.IsName("tail swipe"))
        {
            dealDamage damage = other.gameObject.GetComponent<dealDamage>();
            if (damage != null)
            {
                damage.dealDamage(EnemyStatManager.TailswipeDamage, Color.red, _monster);
                LaunchHit(other.gameObject);
            }
        }
    }

    private void LaunchHit(GameObject other)
    {
        Rigidbody rigidbody = other.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.AddForce(transform.forward * _launchPower, ForceMode.Impulse);
        }
    }
}
