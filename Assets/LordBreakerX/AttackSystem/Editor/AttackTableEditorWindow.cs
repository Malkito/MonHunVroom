using LordBreakerX.AttackSystem;
using LordBreakerX.Tables;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackTableEditorWindow : TableEditorWindow<ScriptableAttack>
{
    [MenuItem("Window/Attack Table Editor")]
    private static void OpenWindowMenuItem()
    {
        AttackTableEditorWindow window = CreateInstance<AttackTableEditorWindow>();
        window.titleContent = new GUIContent("Attack Table Editor");
        window.Show();
    }

    protected override ScriptableWeightTable<ScriptableAttack> CreateDefaultTable()
    {
        ScriptableAttackTable table = CreateInstance<ScriptableAttackTable>();
        return table;
    }

    protected override List<ScriptableWeightTable<ScriptableAttack>> GetTables()
    {
        List<ScriptableWeightTable<ScriptableAttack>> tables = new List<ScriptableWeightTable<ScriptableAttack>>();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableAttackTable");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!string.IsNullOrEmpty(assetPath))
            {
                ScriptableAttackTable table = AssetDatabase.LoadAssetAtPath<ScriptableAttackTable>(assetPath);

                if (table != null)
                {
                    tables.Add(table);
                }
            }
        }

        return tables;
    }
}
