using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(VoronoiDiagram))]
public class VoronoiDiagramEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VoronoiDiagram voronoiDiag = (VoronoiDiagram) target;

        DrawDefaultInspector();

        if(GUILayout.Button("Generate World"))
        {
            voronoiDiag.GenerateWorld();
        }
    }
}
