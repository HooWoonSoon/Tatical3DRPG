using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryData))]
public class InventoryDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        InventoryData data = (InventoryData)target;

        data.itemName = EditorGUILayout.TextField("Item Name", data.itemName);
        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);
        data.description = EditorGUILayout.TextField("Description", data.description);
        Type[] allowedTypes = { Type.Inventory };
        int newIndex = EditorGUILayout.Popup("Type", 0, Array.ConvertAll(allowedTypes, t => t.ToString()));
        data.type = allowedTypes[newIndex];

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inventory Details", EditorStyles.boldLabel);
        data.range = EditorGUILayout.IntField("Range", data.range);
        data.consumableType = (ConsumableType)EditorGUILayout.EnumPopup("Consumable Type", data.consumableType);
        if (data.consumableType == ConsumableType.Health)
        {
            data.healthAmount = EditorGUILayout.IntField("Health Amount", data.healthAmount);
        }
        if (data.consumableType == ConsumableType.Damage)
        {
            data.damageAmount = EditorGUILayout.IntField("Damage Amount", data.damageAmount);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}