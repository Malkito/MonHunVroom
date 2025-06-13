using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Attacking/Attack Table")]
public class AttackTable : ScriptableObject
{
    [SerializeField]
    private AttackCondition _selectCondition;

    [SerializeField]
    private List<AttackEntry> _attacks = new List<AttackEntry>();

    private int _totalWeight;

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
        int weight = Random.Range(0, _totalWeight);

        foreach (AttackEntry entry in _attacks) 
        {
            if (weight <= entry.Weight)
            {
                return entry.ChoosenAttack;
            }
            else
            {
                weight -= entry.Weight;
            }
        }
        
        return _attacks[0].ChoosenAttack;
    }

    public static AttackTable Copy(AttackTable tableToCopy, AttackController creator)
    {
        AttackTable table = Instantiate(tableToCopy);

        List<AttackEntry> copiedEntries = new List<AttackEntry>();

        foreach (AttackEntry entryToCopy in table._attacks)
        {
            Attack copiedAttackInstance = Instantiate(entryToCopy.ChoosenAttack);
            copiedAttackInstance.Initilize(creator);
            copiedEntries.Add(new AttackEntry(copiedAttackInstance, entryToCopy.Weight));
        }

        table._attacks = copiedEntries;

        if (table._selectCondition != null)
        {
            table._selectCondition = Instantiate(table._selectCondition);
            table._selectCondition.OnIniliization();
        }

        table.Inililize();

        return table;
    }

    private void Inililize()
    {
        _totalWeight = 0;

        foreach(AttackEntry entry in  _attacks)
        {
            _totalWeight += entry.Weight;
        }
    }

}
