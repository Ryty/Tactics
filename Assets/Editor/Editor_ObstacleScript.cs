using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Obstacle))]
public class Editor_ObstacleScript : Editor
{
    public override void OnInspectorGUI()
    {
        Obstacle script = (Obstacle)target;

        script.CoverValue = EditorGUILayout.IntField("Cover value", script.CoverValue);

        script.OrientCheckMask = EditorTools.LayerMaskField("Orientation Check mask", script.OrientCheckMask);

        EditorGUILayout.LabelField("Cover clearances:");
            script.CoverClearances[0] = EditorGUILayout.Toggle("North", script.CoverClearances[0]);
            script.CoverClearances[1] = EditorGUILayout.Toggle("South", script.CoverClearances[1]);
            script.CoverClearances[2] = EditorGUILayout.Toggle("West", script.CoverClearances[2]);
            script.CoverClearances[3] = EditorGUILayout.Toggle("East", script.CoverClearances[3]);
    }
}
