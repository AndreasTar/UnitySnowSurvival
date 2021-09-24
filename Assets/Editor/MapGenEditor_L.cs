/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator_L))]
public class MapGenEditor_L : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator_L mapGen = (MapGenerator_L)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("Generate Map"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
*/