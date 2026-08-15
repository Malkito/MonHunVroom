using LordBreakerX.AttackSystem;
using LordBreakerX.Tables;
using UnityEditor;

[CustomEditor(typeof(ScriptableWeightTable<ScriptableAttack>), true)]
public class ScriptableAttackTableEditor : ScriptableWeightTableEditor<ScriptableAttack>
{
    public override string EntriesHeader => "Attack Entries";

    public override string EntryPropertiesHeader => "Attack Entry Properties";

    public override string GeneralPropertiesLabel => "General Properties";

    public override string ValueLabel => "Attack";
}
