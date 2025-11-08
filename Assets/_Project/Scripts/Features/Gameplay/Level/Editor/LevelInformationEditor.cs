using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelInformation))]
public class LevelInformationEditor : Editor
{
    private LevelInformation levelInfo;
    private Vector2 scrollPosition;

    // Selection
    private int selectedObjectType = 0;
    private ColorType selectedColor = ColorType.Red;
    private List<LevelObjectDataSimpleCharacter> pipeCharacters = new List<LevelObjectDataSimpleCharacter>();

    // Grid resize
    private Vector2Int newGridSize = new Vector2Int(10, 10);

    // Visual
    private GUIStyle cellButtonStyle;
    private readonly Color[] colorTypeColors = new Color[]
    {
        Color.red,      // Red
        Color.green,    // Green  
        Color.blue,     // Blue
        Color.yellow,   // Yellow
        new Color(1f, 0f, 1f), // Purple
        new Color(1f, 0.5f, 0f) // Orange
    };

    private void OnEnable()
    {
        levelInfo = (LevelInformation)target;
        newGridSize = new Vector2Int(levelInfo.GridWidth, levelInfo.GridHeight);
        InitializePipeCharacters();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Basic level info
        DrawBasicInfo();

        EditorGUILayout.Space(10);

        // Grid controls
        DrawGridControls();

        EditorGUILayout.Space(10);

        // Object selection
        DrawObjectSelection();

        EditorGUILayout.Space(10);

        // Grid visualization
        DrawGridVisualization();

        EditorGUILayout.Space(10);

        // Level statistics
        DrawLevelStats();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);

        levelInfo.levelName = EditorGUILayout.TextField("Level Name", levelInfo.levelName);
        levelInfo.levelDescription = EditorGUILayout.TextField("Description", levelInfo.levelDescription);
        levelInfo.levelIndex = EditorGUILayout.IntField("Level Index", levelInfo.levelIndex);
        levelInfo.cellSize = EditorGUILayout.FloatField("Cell Size", levelInfo.cellSize);
    }

    private void DrawGridControls()
    {
        EditorGUILayout.LabelField("Grid Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Current Size: {levelInfo.GridWidth} x {levelInfo.GridHeight}");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        newGridSize = EditorGUILayout.Vector2IntField("New Grid Size:", newGridSize);

        if (GUILayout.Button("Resize", GUILayout.Width(80)))
        {
            if (newGridSize.x > 0 && newGridSize.y > 0)
            {
                levelInfo.SetGridSize(newGridSize.x, newGridSize.y);
                EditorUtility.SetDirty(levelInfo);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Grid size must be positive values!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("5x5", GUILayout.Width(50))) SetGridSize(5, 5);
        if (GUILayout.Button("8x8", GUILayout.Width(50))) SetGridSize(8, 8);
        if (GUILayout.Button("10x10", GUILayout.Width(50))) SetGridSize(10, 10);
        if (GUILayout.Button("12x8", GUILayout.Width(50))) SetGridSize(12, 8);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Clear All", GUILayout.Width(80)))
        {
            if (EditorUtility.DisplayDialog("Clear All Objects",
                "Are you sure you want to clear all objects from the level?", "Yes", "Cancel"))
            {
                levelInfo.ClearAllObjects();
                EditorUtility.SetDirty(levelInfo);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectSelection()
    {
        EditorGUILayout.LabelField("Object Selection", EditorStyles.boldLabel);

        string[] objectTypes = { "Empty", "Simple Character", "Pipe", "Barrel" };
        selectedObjectType = GUILayout.SelectionGrid(selectedObjectType, objectTypes, 3);

        EditorGUILayout.Space(5);

        // Type specific options
        switch (selectedObjectType)
        {
            case 1: // Simple Character
                EditorGUILayout.LabelField("Character Color:");
                selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor);

                // Color preview
                Rect colorRect = GUILayoutUtility.GetRect(50, 20);
                EditorGUI.DrawRect(colorRect, colorTypeColors[(int)selectedColor]);
                break;

            case 2: // Pipe
                EditorGUILayout.LabelField("Pipe Characters:");
                DrawPipeCharactersList();
                break;
            case 3: //Barrel
                EditorGUILayout.LabelField("Barrel Settings:");
                DrawBarrelCharacterColor();
                break;
        }
    }

    private void DrawPipeCharactersList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // Add character button
        EditorGUILayout.BeginHorizontal();
        ColorType newColor = (ColorType)EditorGUILayout.EnumPopup("Add Color:", ColorType.Red);
        if (GUILayout.Button("Add", GUILayout.Width(50)))
        {
            var newChar = new LevelObjectDataSimpleCharacter();
            newChar.colorType = newColor;
            pipeCharacters.Add(newChar);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Character list
        for (int i = 0; i < pipeCharacters.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Color preview
            Rect colorRect = GUILayoutUtility.GetRect(20, 20);
            EditorGUI.DrawRect(colorRect, colorTypeColors[(int)pipeCharacters[i].colorType]);

            // Color selection
            pipeCharacters[i].colorType = (ColorType)EditorGUILayout.EnumPopup(pipeCharacters[i].colorType);

            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                pipeCharacters.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBarrelCharacterColor()
    {
        EditorGUILayout.LabelField("Character Color:");
        selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor);

        // Color preview
        Rect colorRect = GUILayoutUtility.GetRect(50, 20);
        EditorGUI.DrawRect(colorRect, colorTypeColors[(int)selectedColor]);
    }

    private void DrawGridVisualization()
    {
        EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);

        InitializeCellButtonStyle();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

        // Draw grid
        for (int y = levelInfo.GridHeight - 1; y >= 0; y--) // Top to bottom
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < levelInfo.GridWidth; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                DrawGridCell(position);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawGridCell(Vector2Int position)
    {
        LevelObjectData objectData = levelInfo.GetObjectAtPosition(position);

        // Determine cell content and color
        string cellText = GetCellDisplayText(objectData);
        Color cellColor = GetCellDisplayColor(objectData);

        // Set button style color
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = cellColor;

        // Cell button
        if (GUILayout.Button(cellText, cellButtonStyle, GUILayout.Width(40), GUILayout.Height(40)))
        {
            OnCellClicked(position);
        }

        GUI.backgroundColor = originalColor;
    }

    private string GetCellDisplayText(LevelObjectData objectData)
    {
        if (objectData == null) return "";

        if (objectData is LevelObjectDataSimpleCharacter simpleChar)
        {
            return simpleChar.colorType.ToString().Substring(0, 1); // First letter
        }
        else if (objectData is LevelObjectDataPipe pipe)
        {
            return $"P\n{pipe.characters?.Count ?? 0}";
        }
        else if (objectData is LevelObjectTypeBarrel barrel)
        {
            return $"B\n{barrel.character.colorType.ToString().Substring(0, 1)}";
        }

        return "?";
    }

    private Color GetCellDisplayColor(LevelObjectData objectData)
    {
        if (objectData == null) return Color.white;

        if (objectData is LevelObjectDataSimpleCharacter simpleChar)
        {
            return colorTypeColors[(int)simpleChar.colorType];
        }
        else if (objectData is LevelObjectDataPipe)
        {
            return Color.gray;
        }
        else if (objectData is LevelObjectTypeBarrel barrel)
        {
            return colorTypeColors[(int)barrel.character.colorType];
        }

        return Color.white;
    }

    private void OnCellClicked(Vector2Int position)
    {
        switch (selectedObjectType)
        {
            case 0: // Empty
                levelInfo.RemoveObjectAtPosition(position);
                break;

            case 1: // Simple Character
                var simpleChar = new LevelObjectDataSimpleCharacter();
                simpleChar.colorType = selectedColor;
                levelInfo.SetObjectAtPosition(position, simpleChar);
                break;

            case 2: // Pipe
                if (pipeCharacters.Count > 0)
                {
                    var pipe = new LevelObjectDataPipe();
                    pipe.characters = new List<LevelObjectDataSimpleCharacter>(pipeCharacters);
                    levelInfo.SetObjectAtPosition(position, pipe);
                }
                break;
            case 3: //Barrel
                var barrel = new LevelObjectTypeBarrel();
                barrel.character = new LevelObjectDataSimpleCharacter() { colorType = selectedColor };
                levelInfo.SetObjectAtPosition(position, barrel);
                break;
        }

        EditorUtility.SetDirty(levelInfo);
        Repaint();
    }

    private void DrawLevelStats()
    {
        EditorGUILayout.LabelField("Level Statistics", EditorStyles.boldLabel);

        int simpleCharCount = 0;
        int pipeCount = 0;
        int totalCells = 0;

        for (int x = 0; x < levelInfo.GridWidth; x++)
        {
            for (int y = 0; y < levelInfo.GridHeight; y++)
            {
                var obj = levelInfo.GetObjectAtPosition(new Vector2Int(x, y));
                if (obj != null)
                {
                    totalCells++;
                    if (obj is LevelObjectDataSimpleCharacter) simpleCharCount++;
                    else if (obj is LevelObjectDataPipe) pipeCount++;
                }
            }
        }

        EditorGUILayout.LabelField($"Total Objects: {totalCells}");
        EditorGUILayout.LabelField($"Simple Characters: {simpleCharCount}");
        EditorGUILayout.LabelField($"Pipes: {pipeCount}");
        EditorGUILayout.LabelField($"Empty Cells: {(levelInfo.GridWidth * levelInfo.GridHeight) - totalCells}");
    }

    private void InitializeCellButtonStyle()
    {
        if (cellButtonStyle == null)
        {
            cellButtonStyle = new GUIStyle(GUI.skin.button);
            cellButtonStyle.fontSize = 8;
            cellButtonStyle.fontStyle = FontStyle.Bold;
        }
    }

    private void InitializePipeCharacters()
    {
        if (pipeCharacters == null)
        {
            pipeCharacters = new List<LevelObjectDataSimpleCharacter>();
        }
    }

    private void SetGridSize(int width, int height)
    {
        newGridSize = new Vector2Int(width, height);
        levelInfo.SetGridSize(width, height);
        EditorUtility.SetDirty(levelInfo);
    }
}