using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelInformation))]
public class LevelInformationEditor : Editor
{
    private LevelInformation levelInfo;
    private Vector2 scrollPosition;
    private GUIStyle cellButtonStyle;
    
    // Selection tools
    private int selectedTool = 0; // 0: Empty, 1: Simple, 2: Barrel, 3: Pipe
    private ColorType selectedColor = ColorType.Red;
    private PipeDirection selectedPipeDirection = PipeDirection.Forward;
    private List<ColorType> pipeColors = new List<ColorType>();
    
    // Color mapping
    private readonly Color[] colorMap = {
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
        pipeColors.Add(ColorType.Red);
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Header
        EditorGUILayout.LabelField("Level Information Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Default inspector for basic fields
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // Grid Controls
        DrawGridControls();
        
        EditorGUILayout.Space(10);
        
        // Tool Selection
        DrawToolSelection();
        
        EditorGUILayout.Space(10);
        
        // Visual Grid Editor
        DrawVisualGridEditor();
        
        EditorGUILayout.Space(10);
        
        // Level Statistics
        DrawLevelStats();
        
        EditorGUILayout.Space(10);
        
        // Utility Buttons
        DrawUtilityButtons();
        
        serializedObject.ApplyModifiedProperties();
        
        // Force save if anything changed
        if (GUI.changed)
        {
            levelInfo.ForceSerialize();
        }
    }
    
    private void DrawGridControls()
    {
        EditorGUILayout.LabelField("Grid Controls", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"Current Size: {levelInfo.GridWidth} x {levelInfo.GridHeight}");
        
        EditorGUILayout.Space(5);
        
        // Quick size buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("5x5"))
            SetGridSize(5, 5);
        if (GUILayout.Button("8x8"))
            SetGridSize(8, 8);
        if (GUILayout.Button("10x10"))
            SetGridSize(10, 10);
        if (GUILayout.Button("12x8"))
            SetGridSize(12, 8);
            
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLevelStats()
    {
        EditorGUILayout.LabelField("Level Statistics", EditorStyles.boldLabel);
        
        int simpleCount = 0;
        int pipeCount = 0;
        int barrelCount = 0;
        int totalObjects = 0;
        
        // Count objects
        for (int x = 0; x < levelInfo.GridWidth; x++)
        {
            for (int y = 0; y < levelInfo.GridHeight; y++)
            {
                var instanceData = levelInfo.GetObjectAtPosition(new Vector2Int(x, y));
                if (instanceData != null)
                {
                    totalObjects++;
                    if (instanceData is SimpleCharacterData)
                        simpleCount++;
                    else if (instanceData is PipeData)
                        pipeCount++;
                    else if (instanceData is BarrelData)
                        barrelCount++;
                }
            }
        }
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField($"Grid: {levelInfo.GridWidth} x {levelInfo.GridHeight}");
        EditorGUILayout.LabelField($"Total Objects: {totalObjects}");
        EditorGUILayout.LabelField($"Simple: {simpleCount}");
        EditorGUILayout.LabelField($"Pipes: {pipeCount}");
        EditorGUILayout.LabelField($"Barrels: {barrelCount}");
        EditorGUILayout.LabelField($"Empty Cells: {(levelInfo.GridWidth * levelInfo.GridHeight) - totalObjects}");
        EditorGUILayout.EndVertical();
    }
    
    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear All Objects"))
        {
            if (EditorUtility.DisplayDialog("Clear All", 
                "Clear all objects from level?", "Yes", "Cancel"))
            {
                levelInfo.ClearAllObjects();
                EditorUtility.SetDirty(levelInfo);
            }
        }
        
        if (GUILayout.Button("Validate Level"))
        {
            var (isValid, errors) = levelInfo.ValidateLevel();
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", "Level is valid!", "OK");
            }
            else
            {
                string errorMsg = string.Join("\n", errors);
                EditorUtility.DisplayDialog("Validation Failed", errorMsg, "OK");
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawToolSelection()
    {
        EditorGUILayout.LabelField("Tool Selection", EditorStyles.boldLabel);
        
        string[] tools = { "Empty", "Simple Character", "Barrel", "Pipe" };
        selectedTool = GUILayout.SelectionGrid(selectedTool, tools, 4);
        
        EditorGUILayout.Space(5);
        
        // Tool-specific options
        switch (selectedTool)
        {
            case 1: // Simple Character
                EditorGUILayout.LabelField("Character Color:");
                selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor);
                
                // Color preview
                Rect colorRect = GUILayoutUtility.GetRect(50, 20);
                EditorGUI.DrawRect(colorRect, GetColorFromType(selectedColor));
                break;
                
            case 2: // Barrel
                EditorGUILayout.LabelField("Barrel Color:");
                selectedColor = (ColorType)EditorGUILayout.EnumPopup(selectedColor);
                
                // Color preview
                Rect barrelColorRect = GUILayoutUtility.GetRect(50, 20);
                EditorGUI.DrawRect(barrelColorRect, GetColorFromType(selectedColor));
                break;
                
            case 3: // Pipe
                EditorGUILayout.LabelField("Pipe Settings:");
                selectedPipeDirection = (PipeDirection)EditorGUILayout.EnumPopup("Direction:", selectedPipeDirection);
                
                EditorGUILayout.LabelField("Pipe Colors:");
                DrawPipeColorList();
                break;
        }
    }
    
    private void DrawPipeColorList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // Add color button
        EditorGUILayout.BeginHorizontal();
        ColorType newColor = (ColorType)EditorGUILayout.EnumPopup("Add Color:", ColorType.Red);
        if (GUILayout.Button("Add", GUILayout.Width(50)))
        {
            pipeColors.Add(newColor);
        }
        EditorGUILayout.EndHorizontal();
        
        // Color list
        for (int i = 0; i < pipeColors.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Color preview
            Rect colorRect = GUILayoutUtility.GetRect(20, 20);
            EditorGUI.DrawRect(colorRect, GetColorFromType(pipeColors[i]));
            
            pipeColors[i] = (ColorType)EditorGUILayout.EnumPopup(pipeColors[i]);
            
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                pipeColors.RemoveAt(i);
                i--;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("Clear All"))
        {
            pipeColors.Clear();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawVisualGridEditor()
    {
        EditorGUILayout.LabelField("Visual Grid Editor", EditorStyles.boldLabel);
        
        if (levelInfo.GridWidth <= 0 || levelInfo.GridHeight <= 0)
        {
            EditorGUILayout.HelpBox("Set a valid grid size first!", MessageType.Warning);
            return;
        }
        
        InitializeCellButtonStyle();
        
        // Calculate scroll area height
        float cellSize = 35f;
        float maxHeight = Mathf.Min(cellSize * levelInfo.GridHeight + 20, 400f);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(maxHeight));
        
        // Draw grid (top to bottom)
        for (int y = levelInfo.GridHeight - 1; y >= 0; y--)
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
        CharacterInstanceData instanceData = levelInfo.GetObjectAtPosition(position);
        
        string cellText = GetCellText(instanceData);
        Color cellColor = GetCellColor(instanceData);
        
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = cellColor;
        
        if (GUILayout.Button(cellText, cellButtonStyle, GUILayout.Width(30), GUILayout.Height(30)))
        {
            OnCellClicked(position);
        }
        
        GUI.backgroundColor = originalColor;
    }
    
    private string GetCellText(CharacterInstanceData instanceData)
    {
        if (instanceData == null) return "";
        
        if (instanceData is SimpleCharacterData)
            return "S";
        else if (instanceData is BarrelData)
            return "B";
        else if (instanceData is PipeData)
            return "P";
        
        return "?";
    }
    
    private Color GetCellColor(CharacterInstanceData instanceData)
    {
        if (instanceData == null) return Color.white;
        
        if (instanceData is SimpleCharacterData simple)
            return GetColorFromType(simple.characterColorType);
        else if (instanceData is BarrelData barrel)
            return GetColorFromType(barrel.characterColorType);
        else if (instanceData is PipeData pipe)
            return pipe.characterColorTypes.Count > 0 ? GetColorFromType(pipe.characterColorTypes[0]) : Color.gray;
        
        return Color.white;
    }
    
    private void OnCellClicked(Vector2Int position)
    {
        switch (selectedTool)
        {
            case 0: // Empty
                levelInfo.RemoveObjectAtPosition(position);
                break;
                
            case 1: // Simple Character
                var simpleData = new SimpleCharacterData();
                simpleData.characterColorType = selectedColor;
                levelInfo.SetObjectAtPosition(position, simpleData);
                break;
                
            case 2: // Barrel
                var barrelData = new BarrelData();
                barrelData.characterColorType = selectedColor;
                levelInfo.SetObjectAtPosition(position, barrelData);
                break;
                
            case 3: // Pipe
                var pipeData = new PipeData();
                pipeData.pipeDirection = selectedPipeDirection;
                pipeData.characterColorTypes = new List<ColorType>(pipeColors);
                levelInfo.SetObjectAtPosition(position, pipeData);
                break;
        }
        
        levelInfo.ForceSerialize();
        EditorUtility.SetDirty(levelInfo);
        Repaint();
    }
    
    private Color GetColorFromType(ColorType colorType)
    {
        if ((int)colorType < colorMap.Length)
            return colorMap[(int)colorType];
        return Color.white;
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
    
    private void SetGridSize(int width, int height)
    {
        levelInfo.SetGridSize(width, height);
        EditorUtility.SetDirty(levelInfo);
    }
}
