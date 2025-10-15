using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class test : EditorWindow {

    [MenuItem("tools/LOD")]

    public static void Open() {
        GetWindow<test>();
    }

    public LODGroup[] LODTransforms;

    void OnGUI() {

        displayValue("LODTransforms");

        if (GUILayout.Button("test")) {
            LODTransforms = FindObjectsByType<LODGroup>(FindObjectsSortMode.None);
        }
        if (GUILayout.Button("disableLods") && LODTransforms != null) {
            foreach (LODGroup item in LODTransforms) {
                item.enabled = false;
            }
        }

    }

    void displayValue(string a ){
        SerializedObject i = new SerializedObject(this);
        EditorGUILayout.PropertyField(i.FindProperty(a));
        i.ApplyModifiedProperties();
    }


}
