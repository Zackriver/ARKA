using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameManager))]
public class LevelDesignerEditor : Editor
{
    private int selectedHP = 1;
    private Color selectedColor = Color.white;
    private bool editingMode = false;
    private int selectedShapeIndex = 0;
    private Vector2 lastPaintedPos = Vector2.one * float.MaxValue;

    private enum SnapMode { Free, SnapToStep, SnapToNeighbor }
    private SnapMode snapMode = SnapMode.SnapToStep;

    // Ruler settings
    private Color rulerColorX = new Color(1f, 0.3f, 0.3f, 0.8f);
    private Color rulerColorY = new Color(0.3f, 1f, 0.3f, 0.8f);
    private Color gridColor = new Color(1f, 1f, 1f, 0.1f);
    private float rulerTickSpacing = 1f;

    // Y-Lock for straight line placement
    private bool yLocked = false;
    private float lockedY = 0f;
    private float lockThreshold = 0f;

    // Color Presets
    private List<Color> colorPresets = new List<Color>();
    private const string COLOR_PRESETS_KEY = "LevelDesigner_ColorPresets";
    private const string SELECTED_COLOR_KEY = "LevelDesigner_SelectedColor";
    private const string SELECTED_HP_KEY = "LevelDesigner_SelectedHP";
    private bool showColorPresets = true;
    private int selectedPresetIndex = -1;

    // Default colors
    private static readonly Color[] DEFAULT_COLORS = new Color[]
    {
        new Color(1f, 0.3f, 0.3f, 1f),      // Red
        new Color(1f, 0.5f, 0.2f, 1f),      // Orange
        new Color(1f, 0.9f, 0.2f, 1f),      // Yellow
        new Color(0.3f, 1f, 0.3f, 1f),      // Green
        new Color(0.2f, 0.8f, 1f, 1f),      // Cyan
        new Color(0.3f, 0.5f, 1f, 1f),      // Blue
        new Color(0.7f, 0.3f, 1f, 1f),      // Purple
        new Color(1f, 0.4f, 0.7f, 1f),      // Pink
        new Color(1f, 1f, 1f, 1f),          // White
        new Color(0.7f, 0.7f, 0.7f, 1f),    // Light Gray
        new Color(0.4f, 0.4f, 0.4f, 1f),    // Dark Gray
        new Color(0.6f, 0.4f, 0.2f, 1f),    // Brown
    };

    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
        LoadColorPresets();
        LoadSelectedColor();
        LoadSelectedHP();
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    // ===================== SAVE/LOAD SETTINGS =====================

    private void LoadSelectedColor()
    {
        string savedColor = EditorPrefs.GetString(SELECTED_COLOR_KEY, "");
        if (!string.IsNullOrEmpty(savedColor))
        {
            if (ColorUtility.TryParseHtmlString(savedColor, out Color color))
            {
                selectedColor = color;
                
                selectedPresetIndex = -1;
                for (int i = 0; i < colorPresets.Count; i++)
                {
                    if (ColorsAreClose(colorPresets[i], selectedColor))
                    {
                        selectedPresetIndex = i;
                        break;
                    }
                }
            }
        }
    }

    private void SaveSelectedColor()
    {
        string hexColor = "#" + ColorUtility.ToHtmlStringRGBA(selectedColor);
        EditorPrefs.SetString(SELECTED_COLOR_KEY, hexColor);
    }

    private void LoadSelectedHP()
    {
        selectedHP = EditorPrefs.GetInt(SELECTED_HP_KEY, 1);
        selectedHP = Mathf.Clamp(selectedHP, 1, 5);
    }

    private void SaveSelectedHP()
    {
        EditorPrefs.SetInt(SELECTED_HP_KEY, selectedHP);
    }

    private void LoadColorPresets()
    {
        colorPresets.Clear();

        string savedData = EditorPrefs.GetString(COLOR_PRESETS_KEY, "");

        if (string.IsNullOrEmpty(savedData))
        {
            colorPresets.AddRange(DEFAULT_COLORS);
            SaveColorPresets();
        }
        else
        {
            string[] colorStrings = savedData.Split(';');
            foreach (string colorStr in colorStrings)
            {
                if (!string.IsNullOrEmpty(colorStr))
                {
                    if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
                    {
                        colorPresets.Add(color);
                    }
                }
            }

            if (colorPresets.Count == 0)
            {
                colorPresets.AddRange(DEFAULT_COLORS);
                SaveColorPresets();
            }
        }
    }

    private void SaveColorPresets()
    {
        List<string> colorStrings = new List<string>();
        foreach (Color color in colorPresets)
        {
            colorStrings.Add("#" + ColorUtility.ToHtmlStringRGBA(color));
        }
        EditorPrefs.SetString(COLOR_PRESETS_KEY, string.Join(";", colorStrings));
    }

    // ===================== COLOR PRESETS UI =====================

    private void DrawColorPresets()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        showColorPresets = EditorGUILayout.Foldout(showColorPresets, "🎨 Color Presets", true);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset", GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("Reset Colors", "Reset color presets to defaults?", "Yes", "No"))
            {
                colorPresets.Clear();
                colorPresets.AddRange(DEFAULT_COLORS);
                SaveColorPresets();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (!showColorPresets)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        GUILayout.Space(3);

        int colorsPerRow = 12;
        float buttonSize = 18f;

        int rowCount = Mathf.CeilToInt((float)colorPresets.Count / colorsPerRow);

        for (int row = 0; row < rowCount; row++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int col = 0; col < colorsPerRow; col++)
            {
                int index = row * colorsPerRow + col;
                if (index >= colorPresets.Count) break;

                Color presetColor = colorPresets[index];
                bool isSelected = (selectedPresetIndex == index);

                Rect buttonRect = GUILayoutUtility.GetRect(buttonSize, buttonSize, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));

                if (isSelected)
                {
                    EditorGUI.DrawRect(new Rect(buttonRect.x - 2, buttonRect.y - 2, buttonRect.width + 4, buttonRect.height + 4), Color.white);
                }

                EditorGUI.DrawRect(buttonRect, presetColor);

                Color borderColor = isSelected ? Color.yellow : new Color(0, 0, 0, 0.5f);
                DrawRectBorder(buttonRect, borderColor, isSelected ? 2 : 1);

                Event e = Event.current;
                if (buttonRect.Contains(e.mousePosition))
                {
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        selectedColor = presetColor;
                        selectedPresetIndex = index;
                        SaveSelectedColor();
                        e.Use();
                        Repaint();
                    }
                    else if (e.type == EventType.MouseDown && e.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        int capturedIndex = index;
                        menu.AddItem(new GUIContent("Remove Color"), false, () =>
                        {
                            colorPresets.RemoveAt(capturedIndex);
                            SaveColorPresets();
                            if (selectedPresetIndex == capturedIndex)
                                selectedPresetIndex = -1;
                            else if (selectedPresetIndex > capturedIndex)
                                selectedPresetIndex--;
                        });
                        menu.AddItem(new GUIContent("Edit Color"), false, () =>
                        {
                            selectedPresetIndex = capturedIndex;
                            selectedColor = colorPresets[capturedIndex];
                            SaveSelectedColor();
                        });
                        menu.ShowAsContext();
                        e.Use();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();

        Color newColor = EditorGUILayout.ColorField(GUIContent.none, selectedColor, false, true, false, GUILayout.Width(50), GUILayout.Height(20));
        if (newColor != selectedColor)
        {
            selectedColor = newColor;
            selectedPresetIndex = -1;
            SaveSelectedColor();
        }

        if (GUILayout.Button("+ Add", GUILayout.Width(50), GUILayout.Height(20)))
        {
            bool exists = false;
            foreach (Color c in colorPresets)
            {
                if (ColorsAreClose(c, selectedColor))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                colorPresets.Add(selectedColor);
                selectedPresetIndex = colorPresets.Count - 1;
                SaveColorPresets();
            }
            else
            {
                EditorUtility.DisplayDialog("Color Exists", "This color already exists in presets.", "OK");
            }
        }

        string hexCode = "#" + ColorUtility.ToHtmlStringRGB(selectedColor);
        string newHex = EditorGUILayout.TextField(hexCode, GUILayout.Width(70));
        if (newHex != hexCode && ColorUtility.TryParseHtmlString(newHex, out Color parsedColor))
        {
            selectedColor = parsedColor;
            selectedPresetIndex = -1;
            SaveSelectedColor();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (selectedPresetIndex >= 0 && selectedPresetIndex < colorPresets.Count)
        {
            if (!ColorsAreClose(colorPresets[selectedPresetIndex], selectedColor))
            {
                colorPresets[selectedPresetIndex] = selectedColor;
                SaveColorPresets();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRectBorder(Rect rect, Color color, int thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private bool ColorsAreClose(Color a, Color b, float threshold = 0.02f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    // ===================== EXISTING METHODS =====================

    private void OnUndoRedo()
    {
        GameManager gm = (GameManager)target;
        if (gm != null && gm.currentLevelData != null)
        {
            SyncPreviewWithData(gm);
        }
    }

    private BrickShape GetSelectedShape(GameManager gm)
    {
        if (selectedShapeIndex <= 0 || gm.availableShapes.Count == 0) return null;
        int idx = selectedShapeIndex - 1;
        if (idx < gm.availableShapes.Count) return gm.availableShapes[idx];
        return null;
    }

    private Vector2 SnapPosition(GameManager gm, Vector2 raw, float brickHeight)
    {
        BrickShape shape = GetSelectedShape(gm);
        Vector2 step = gm.GetStepSize(shape);

        switch (snapMode)
        {
            case SnapMode.Free:
                float snappedX = Mathf.Round(raw.x * 100f) / 100f;
                float snappedY = Mathf.Round(raw.y * 100f) / 100f;

                if (yLocked)
                {
                    if (Mathf.Abs(raw.y - lockedY) > lockThreshold)
                    {
                        lockedY = snappedY;
                    }
                    else
                    {
                        snappedY = lockedY;
                    }
                }

                return new Vector2(snappedX, snappedY);

            case SnapMode.SnapToStep:
                return new Vector2(
                    Mathf.Round(raw.x / step.x) * step.x,
                    Mathf.Round(raw.y / step.y) * step.y
                );

            case SnapMode.SnapToNeighbor:
                return SnapToNearestNeighbor(gm, raw, step);

            default:
                return raw;
        }
    }

    private Vector2 SnapToNearestNeighbor(GameManager gm, Vector2 raw, Vector2 step)
    {
        if (gm.currentLevelData == null || gm.currentLevelData.bricks.Count == 0)
        {
            return new Vector2(
                Mathf.Round(raw.x / step.x) * step.x,
                Mathf.Round(raw.y / step.y) * step.y
            );
        }

        BrickData closest = null;
        float closestDist = float.MaxValue;
        foreach (var b in gm.currentLevelData.bricks)
        {
            float dist = Vector2.Distance(b.position, raw);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = b;
            }
        }

        if (closest == null)
        {
            return new Vector2(
                Mathf.Round(raw.x / step.x) * step.x,
                Mathf.Round(raw.y / step.y) * step.y
            );
        }

        Vector2 diff = raw - closest.position;
        return new Vector2(
            closest.position.x + Mathf.Round(diff.x / step.x) * step.x,
            closest.position.y + Mathf.Round(diff.y / step.y) * step.y
        );
    }

    // ===================== SYNC METHODS =====================

    private void ClearAllPreviewBricks(GameManager gm)
    {
        if (gm.activeBricks != null)
        {
            for (int i = gm.activeBricks.Count - 1; i >= 0; i--)
            {
                if (gm.activeBricks[i] != null)
                {
                    DestroyImmediate(gm.activeBricks[i]);
                }
            }
            gm.activeBricks.Clear();
        }

        Brick[] allBricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        foreach (Brick brick in allBricks)
        {
            if (brick != null && brick.gameObject != null)
            {
                if ((brick.gameObject.hideFlags & HideFlags.DontSave) != 0)
                {
                    DestroyImmediate(brick.gameObject);
                }
            }
        }

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && (obj.hideFlags & HideFlags.DontSave) != 0)
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null && gm.IsInBounds(obj.transform.position))
                {
                    DestroyImmediate(obj);
                }
            }
        }
    }

    private void SyncPreviewWithData(GameManager gm)
    {
        ClearAllPreviewBricks(gm);

        if (gm.currentLevelData == null) return;

        foreach (BrickData brick in gm.currentLevelData.bricks)
        {
            GameObject newBrick = gm.CreateBrick(brick, true);
            if (newBrick != null)
            {
                gm.activeBricks.Add(newBrick);
            }
        }
    }

    private bool HasBrickAtPosition(GameManager gm, Vector2 position, float threshold)
    {
        if (gm.currentLevelData != null)
        {
            foreach (var brick in gm.currentLevelData.bricks)
            {
                if (Vector2.Distance(brick.position, position) < threshold)
                {
                    return true;
                }
            }
        }

        if (gm.activeBricks != null)
        {
            foreach (var brick in gm.activeBricks)
            {
                if (brick != null && Vector2.Distance(brick.transform.position, position) < threshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool RemoveBrickAt(GameManager gm, Vector2 position, float threshold)
    {
        bool removedAny = false;

        if (gm.currentLevelData != null)
        {
            int removedFromData = gm.currentLevelData.bricks.RemoveAll(
                b => Vector2.Distance(b.position, position) < threshold
            );
            if (removedFromData > 0)
            {
                EditorUtility.SetDirty(gm.currentLevelData);
                removedAny = true;
            }
        }

        if (gm.activeBricks != null)
        {
            for (int i = gm.activeBricks.Count - 1; i >= 0; i--)
            {
                if (gm.activeBricks[i] != null &&
                    Vector2.Distance(gm.activeBricks[i].transform.position, position) < threshold)
                {
                    DestroyImmediate(gm.activeBricks[i]);
                    gm.activeBricks.RemoveAt(i);
                    removedAny = true;
                }
            }
        }

        Brick[] allBricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        foreach (Brick brick in allBricks)
        {
            if (brick != null && brick.gameObject != null)
            {
                if (Vector2.Distance(brick.transform.position, position) < threshold)
                {
                    if ((brick.gameObject.hideFlags & HideFlags.DontSave) != 0)
                    {
                        DestroyImmediate(brick.gameObject);
                        removedAny = true;
                    }
                }
            }
        }

        return removedAny;
    }

    // ===================== INSPECTOR GUI =====================

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GameManager gm = (GameManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Level Designer", EditorStyles.boldLabel);

        bool newEditing = GUILayout.Toggle(editingMode, "Enable Painting Mode", "Button");
        if (newEditing != editingMode)
        {
            editingMode = newEditing;

            if (editingMode)
            {
                SyncPreviewWithData(gm);
            }

            if (!editingMode)
            {
                yLocked = false;
            }

            SceneView.RepaintAll();
        }

        if (!editingMode) return;

        EditorGUILayout.HelpBox(
            "Shift + Left Click = Place Brick\n" +
            "Ctrl + Left Click = Erase Brick\n" +
            "Drag while holding modifier for multiple.",
            MessageType.Info
        );

        // Snap mode
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Placement Mode", EditorStyles.boldLabel);
        SnapMode newSnapMode = (SnapMode)EditorGUILayout.EnumPopup("Snap Mode", snapMode);
        if (newSnapMode != snapMode)
        {
            snapMode = newSnapMode;
            yLocked = false;
        }

        BrickShape selShape = GetSelectedShape(gm);
        Vector2 step = gm.GetStepSize(selShape);

        switch (snapMode)
        {
            case SnapMode.Free:
                EditorGUILayout.HelpBox(
                    "Place bricks anywhere.\n" +
                    "Y-position locks for straight rows.\n" +
                    "Move up/down past brick height for new row.",
                    MessageType.None
                );
                GUILayout.Space(3);
                rulerTickSpacing = EditorGUILayout.Slider("Ruler Grid Spacing", rulerTickSpacing, 0.5f, 5f);

                if (yLocked)
                {
                    EditorGUILayout.HelpBox("Y-Locked at: " + lockedY.ToString("F2"), MessageType.Info);
                }
                break;
            case SnapMode.SnapToStep:
                EditorGUILayout.HelpBox(
                    "Snaps to brick+gap increments.\n" +
                    "Step X (Columns): " + step.x.ToString("F2") + "\n" +
                    "Step Y (Rows): " + step.y.ToString("F2"),
                    MessageType.None
                );
                break;
            case SnapMode.SnapToNeighbor:
                EditorGUILayout.HelpBox(
                    "Snaps relative to nearest existing brick.",
                    MessageType.None
                );
                break;
        }

        // Shape selector
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Brick Shape", EditorStyles.boldLabel);

        if (gm.availableShapes.Count > 0)
        {
            string[] shapeNames = new string[gm.availableShapes.Count + 1];
            shapeNames[0] = "Default (Square)";
            for (int i = 0; i < gm.availableShapes.Count; i++)
            {
                shapeNames[i + 1] = gm.availableShapes[i] != null
                    ? gm.availableShapes[i].shapeName
                    : "(Missing)";
            }
            selectedShapeIndex = EditorGUILayout.Popup("Shape", selectedShapeIndex, shapeNames);
        }
        else
        {
            EditorGUILayout.HelpBox("No shapes. Using default square.", MessageType.None);
            selectedShapeIndex = 0;
        }

        // ========== COLOR PRESETS ==========
        GUILayout.Space(5);
        DrawColorPresets();

        // Brick settings
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Brick Settings", EditorStyles.boldLabel);
        
        int newHP = EditorGUILayout.IntSlider("Brick Health", selectedHP, 1, 5);
        if (newHP != selectedHP)
        {
            selectedHP = newHP;
            SaveSelectedHP();
        }

        // Show current color preview
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Color:", GUILayout.Width(85));
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(40, 18, GUILayout.Width(40)), selectedColor);
        EditorGUILayout.EndHorizontal();

        Vector2 bSize = gm.GetBrickWorldSize(selShape);
        int brickCount = gm.currentLevelData != null ? gm.currentLevelData.bricks.Count : 0;
        int previewCount = gm.activeBricks != null ? gm.activeBricks.Count : 0;

        if (brickCount != previewCount)
        {
            EditorGUILayout.HelpBox(
                "⚠️ DESYNC DETECTED!\n" +
                "Data bricks: " + brickCount + "\n" +
                "Preview bricks: " + previewCount + "\n" +
                "Click 'Sync Preview' to fix.",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Brick Size: " + bSize.x.ToString("F2") + " x " + bSize.y.ToString("F2") + "\n" +
                "Gap X (Columns): " + gm.brickGapX.ToString("F2") + "\n" +
                "Gap Y (Rows): " + gm.brickGapY.ToString("F2") + "\n" +
                "Total bricks: " + brickCount,
                MessageType.None
            );
        }

        // Buttons
        GUILayout.Space(5);

        if (gm.currentLevelData == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a LevelData asset to 'Current Level Data' first!",
                MessageType.Warning
            );
        }

        if (GUILayout.Button("🔄 Sync Preview with Data"))
        {
            SyncPreviewWithData(gm);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Force Refresh Scene"))
        {
            SyncPreviewWithData(gm);
            SceneView.RepaintAll();
        }

        if (gm.currentLevelData != null && GUILayout.Button("Save Level Data"))
        {
            EditorUtility.SetDirty(gm.currentLevelData);
            AssetDatabase.SaveAssetIfDirty(gm.currentLevelData);
            AssetDatabase.Refresh();
        }

        if (gm.currentLevelData != null && gm.currentLevelData.bricks.Count > 0)
        {
            GUILayout.Space(5);
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button("🗑️ Clear ALL Bricks"))
            {
                if (EditorUtility.DisplayDialog("Clear All Bricks",
                    "Are you sure you want to delete all " + gm.currentLevelData.bricks.Count + " bricks?",
                    "Yes, Clear All", "Cancel"))
                {
                    Undo.RecordObject(gm.currentLevelData, "Clear All Bricks");
                    gm.currentLevelData.bricks.Clear();
                    EditorUtility.SetDirty(gm.currentLevelData);
                    ClearAllPreviewBricks(gm);
                    SceneView.RepaintAll();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        if (snapMode == SnapMode.Free && yLocked)
        {
            if (GUILayout.Button("Unlock Y-Position"))
            {
                yLocked = false;
                SceneView.RepaintAll();
            }
        }
    }

    // ===================== SCENE GUI =====================

    private void OnSceneGUI()
    {
        GameManager gm = (GameManager)target;
        if (!editingMode || gm.currentLevelData == null || gm.brickPrefab == null) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (!groundPlane.Raycast(ray, out float distance)) return;

        Vector3 mousePos = ray.GetPoint(distance);

        BrickShape selectedShape = GetSelectedShape(gm);
        Vector2 bSize = gm.GetBrickWorldSize(selectedShape);

        lockThreshold = bSize.y;

        Vector2 rawPos = new Vector2(mousePos.x, mousePos.y);
        Vector2 finalPos = SnapPosition(gm, rawPos, bSize.y);
        bool inBounds = gm.IsInBounds(finalPos);

        Vector3 cursorSize = new Vector3(bSize.x, bSize.y, 1f);
        string label = selectedShape != null ? selectedShape.shapeName : "Square";

        float distanceThreshold = Mathf.Max(bSize.x, bSize.y) * 0.6f;

        // Draw bounds
        Handles.color = new Color(0, 1, 1, 0.15f);
        Handles.DrawWireCube(
            new Vector3(gm.boundsCenter.x, gm.boundsCenter.y, 0),
            new Vector3(gm.boundsSize.x, gm.boundsSize.y, 0)
        );

        // Draw ruler in Free mode
        if (snapMode == SnapMode.Free)
        {
            DrawRuler(gm, finalPos, rawPos);
        }

        // Draw snap guides for SnapToStep mode
        if (snapMode == SnapMode.SnapToStep && inBounds)
        {
            Vector2 stepSize = gm.GetStepSize(selectedShape);
            Handles.color = new Color(1f, 1f, 1f, 0.04f);

            float left = gm.boundsCenter.x - gm.boundsSize.x / 2f;
            float right = gm.boundsCenter.x + gm.boundsSize.x / 2f;
            float bottom = gm.boundsCenter.y - gm.boundsSize.y / 2f;
            float top = gm.boundsCenter.y + gm.boundsSize.y / 2f;

            for (float x = Mathf.Ceil(left / stepSize.x) * stepSize.x; x <= right; x += stepSize.x)
                Handles.DrawLine(new Vector3(x, bottom, 0), new Vector3(x, top, 0));
            for (float y = Mathf.Ceil(bottom / stepSize.y) * stepSize.y; y <= top; y += stepSize.y)
                Handles.DrawLine(new Vector3(left, y, 0), new Vector3(right, y, 0));
        }

        // Cursor preview
        Vector3 center = new Vector3(finalPos.x, finalPos.y, 0);

        bool hasBrickAtPosition = HasBrickAtPosition(gm, finalPos, distanceThreshold);

        if (inBounds)
        {
            if (e.control || e.command)
            {
                Handles.color = hasBrickAtPosition ? Color.red : Color.gray;
                Handles.DrawWireCube(center, cursorSize);

                GUIStyle eraseStyle = new GUIStyle(EditorStyles.boldLabel);
                eraseStyle.normal.textColor = hasBrickAtPosition ? Color.red : Color.gray;

                Handles.Label(
                    center + Vector3.up * (bSize.y * 0.5f + 0.15f),
                    hasBrickAtPosition ? "🗑️ ERASE" : "NO BRICK",
                    eraseStyle
                );
            }
            else
            {
                Handles.color = selectedColor;
                Handles.DrawWireCube(center, cursorSize);
                Color dim = selectedColor;
                dim.a = 0.3f;
                Handles.color = dim;
                Handles.DrawWireCube(center, cursorSize * 0.9f);

                string posLabel = label + " HP:" + selectedHP;
                if (snapMode == SnapMode.Free)
                {
                    posLabel += "\nX:" + finalPos.x.ToString("F2") + " Y:" + finalPos.y.ToString("F2");
                    if (yLocked)
                    {
                        posLabel += " [Y-LOCKED]";
                    }
                }

                Handles.Label(
                    center + Vector3.up * (bSize.y * 0.5f + 0.15f),
                    posLabel,
                    EditorStyles.boldLabel
                );
            }
        }
        else
        {
            Handles.color = Color.red;
            Handles.DrawWireCube(center, cursorSize);
            Handles.Label(
                center + Vector3.up * (bSize.y * 0.5f + 0.15f),
                "OUT OF BOUNDS",
                EditorStyles.boldLabel
            );
        }

        SceneView.currentDrawingSceneView.Repaint();

        if (!inBounds) return;

        // SHIFT + LEFT CLICK = Place
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            && e.button == 0 && e.shift && !e.control && !e.command)
        {
            if (e.type == EventType.MouseDrag && finalPos == lastPaintedPos)
            {
                e.Use();
                return;
            }
            lastPaintedPos = finalPos;

            if (snapMode == SnapMode.Free && !yLocked)
            {
                yLocked = true;
                lockedY = finalPos.y;
            }

            Undo.RecordObject(gm.currentLevelData, "Place Brick");

            RemoveBrickAt(gm, finalPos, distanceThreshold);

            Vector3 scale = new Vector3(bSize.x, bSize.y, 1f);
            gm.currentLevelData.AddBrick(finalPos, selectedHP, selectedColor, scale, selectedShape);
            EditorUtility.SetDirty(gm.currentLevelData);

            PlacePreviewBrick(gm, finalPos, selectedColor, selectedShape);

            e.Use();
        }

        // CTRL + LEFT CLICK = Erase
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            && e.button == 0 && (e.control || e.command) && !e.shift)
        {
            if (e.type == EventType.MouseDrag && finalPos == lastPaintedPos)
            {
                e.Use();
                return;
            }
            lastPaintedPos = finalPos;

            Undo.RecordObject(gm.currentLevelData, "Remove Brick");

            RemoveBrickAt(gm, finalPos, distanceThreshold);

            e.Use();
        }

        if (e.type == EventType.MouseUp)
        {
            lastPaintedPos = Vector2.one * float.MaxValue;

            if (snapMode == SnapMode.Free)
            {
                yLocked = false;
            }
        }
    }

    // ===================== DRAWING =====================

    private void DrawRuler(GameManager gm, Vector2 cursorPos, Vector2 rawCursorPos)
    {
        float left = gm.boundsCenter.x - gm.boundsSize.x / 2f;
        float right = gm.boundsCenter.x + gm.boundsSize.x / 2f;
        float bottom = gm.boundsCenter.y - gm.boundsSize.y / 2f;
        float top = gm.boundsCenter.y + gm.boundsSize.y / 2f;

        Handles.color = gridColor;
        for (float x = Mathf.Ceil(left / rulerTickSpacing) * rulerTickSpacing; x <= right; x += rulerTickSpacing)
        {
            Handles.DrawLine(new Vector3(x, bottom, 0), new Vector3(x, top, 0));
        }
        for (float y = Mathf.Ceil(bottom / rulerTickSpacing) * rulerTickSpacing; y <= top; y += rulerTickSpacing)
        {
            Handles.DrawLine(new Vector3(left, y, 0), new Vector3(right, y, 0));
        }

        Handles.color = new Color(1f, 1f, 1f, 0.3f);
        if (left <= 0 && right >= 0)
        {
            Handles.DrawLine(new Vector3(0, bottom, 0), new Vector3(0, top, 0));
        }
        if (bottom <= 0 && top >= 0)
        {
            Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(right, 0, 0));
        }

        if (yLocked)
        {
            Handles.color = new Color(1f, 1f, 0f, 0.6f);
            Handles.DrawLine(
                new Vector3(left, lockedY, 0),
                new Vector3(right, lockedY, 0)
            );

            Handles.DrawSolidRectangleWithOutline(
                new Vector3[]
                {
                    new Vector3(left, lockedY - lockThreshold, 0),
                    new Vector3(right, lockedY - lockThreshold, 0),
                    new Vector3(right, lockedY + lockThreshold, 0),
                    new Vector3(left, lockedY + lockThreshold, 0)
                },
                new Color(1f, 1f, 0f, 0.05f),
                new Color(1f, 1f, 0f, 0.2f)
            );
        }

        Handles.color = rulerColorX;
        Handles.DrawLine(
            new Vector3(left, cursorPos.y, 0),
            new Vector3(right, cursorPos.y, 0)
        );

        Handles.color = rulerColorY;
        Handles.DrawLine(
            new Vector3(cursorPos.x, bottom, 0),
            new Vector3(cursorPos.x, top, 0)
        );

        GUIStyle xStyle = new GUIStyle(EditorStyles.boldLabel);
        xStyle.normal.textColor = rulerColorX;
        Handles.Label(
            new Vector3(left - 0.5f, cursorPos.y, 0),
            "Y:" + cursorPos.y.ToString("F2"),
            xStyle
        );

        GUIStyle yStyle = new GUIStyle(EditorStyles.boldLabel);
        yStyle.normal.textColor = rulerColorY;
        Handles.Label(
            new Vector3(cursorPos.x, bottom - 0.3f, 0),
            "X:" + cursorPos.x.ToString("F2"),
            yStyle
        );
    }

    // ===================== HELPERS =====================

    private void PlacePreviewBrick(GameManager gm, Vector2 position, Color color, BrickShape shape)
    {
        Vector2 size = gm.GetBrickWorldSize(shape);
        BrickData tempData = new BrickData
        {
            position = position,
            health = selectedHP,
            color = color,
            brickScale = new Vector3(size.x, size.y, 1f),
            shape = shape
        };

        GameObject newBrick = gm.CreateBrick(tempData, true);
        if (newBrick != null)
        {
            gm.activeBricks.Add(newBrick);
        }
    }
}