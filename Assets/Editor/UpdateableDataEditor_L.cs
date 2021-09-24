using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(UpdateableData_L), true)]
public class UpdateableDataEditor_L : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdateableData_L data = (UpdateableData_L)target;

        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdatedValues();
        }
    }
}
