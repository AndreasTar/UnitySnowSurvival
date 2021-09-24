using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldMapGen))]
public class MapGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WorldMapGen worldMapGen = (WorldMapGen)target;

        if (DrawDefaultInspector() && worldMapGen.autoUpdate)
        {
            worldMapGen.GenerateWorldMap();
        }

        if (GUILayout.Button("Generate"))
        {
            worldMapGen.GenerateWorldMap();
        }
    }
}
