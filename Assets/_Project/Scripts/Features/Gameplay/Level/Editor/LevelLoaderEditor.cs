using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelLoader))]
public class LevelLoaderEditor : Editor
{
    private LevelInformation selectedLevel;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        LevelLoader levelLoader = (LevelLoader)target;
        
        // Level selection
        selectedLevel = (LevelInformation)EditorGUILayout.ObjectField("Level to Load", selectedLevel, typeof(LevelInformation), false);
        
        EditorGUILayout.Space(5);
        
        // Load button
        GUI.enabled = selectedLevel != null;
        if (GUILayout.Button("Load Level", GUILayout.Height(30)))
        {
            levelLoader.LoadLevel(selectedLevel);
        }
        GUI.enabled = true;
    }
}