using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Attacking/Attack Table")]
public class AttackTable : ScriptableObject
{
    [SerializeField]
    private AttackCondition _selectCondition;

    [SerializeField]
    private List<Attack> _attacks = new List<Attack>();

    public bool CanUse(AttackController controller)
    {
        return _selectCondition == null || _selectCondition.CanUse(controller);
    }

    public void DrawGizmos(AttackController controller)
    {
        if (_selectCondition != null) _selectCondition.DrawGizmos(controller);
    }

    public void DrawGizmosSelected(AttackController controller)
    {
        if (_selectCondition != null) _selectCondition.DrawGizmosSelected(controller);
    }

    public Attack GetRandomAttack()
    {
        int attackIndex = Random.Range(0, _attacks.Count);
        return _attacks[attackIndex];
    }

    public static AttackTable Copy(AttackTable tableToCopy, AttackController creator)
    {
        AttackTable table = Instantiate(tableToCopy);

        List<Attack> copiedAttacks = new List<Attack>();

        foreach (Attack attackToCopy in table._attacks)
        {
            Attack copiedAttackInstance = Instantiate(attackToCopy);
            copiedAttackInstance.Initilize(creator);
            copiedAttacks.Add(copiedAttackInstance);
        }

        table._attacks = copiedAttacks;

        if (table._selectCondition != null)
        {
            table._selectCondition = Instantiate(table._selectCondition);
            table._selectCondition.OnIniliization();
        }

        return table;
    }

}
