#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using NeuralBattalion.Data;
using NeuralBattalion.Terrain;

namespace NeuralBattalion.Editor
{
    /// <summary>
    /// Custom editor window for creating and editing levels.
    /// Provides visual level editing tools.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private LevelData currentLevel;
        private TileType selectedTileType = TileType.Brick;
        private int gridWidth = 26;
        private int gridHeight = 26;
        private TileType[,] grid;
        private Vector2 scrollPosition;
        private float cellSize = 15f;
        private bool isDrawing = false;

        [MenuItem("Neural Battalion/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            grid = new TileType[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = TileType.Empty;
                }
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawTilePalette();
            DrawGrid();
            DrawActions();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                NewLevel();
            }

            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                LoadLevel();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                SaveLevel();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Grid Size:", GUILayout.Width(60));
            gridWidth = EditorGUILayout.IntField(gridWidth, GUILayout.Width(40));
            EditorGUILayout.LabelField("x", GUILayout.Width(15));
            gridHeight = EditorGUILayout.IntField(gridHeight, GUILayout.Width(40));

            if (GUILayout.Button("Resize", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ResizeGrid();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTilePalette()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tile Type:", GUILayout.Width(70));

            string[] tileNames = System.Enum.GetNames(typeof(TileType));
            int selected = (int)selectedTileType;
            selected = EditorGUILayout.Popup(selected, tileNames, GUILayout.Width(100));
            selectedTileType = (TileType)selected;

            // Color preview
            GUILayout.Space(10);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(20, 20), GetTileColor(selectedTileType));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Fill All", GUILayout.Width(70)))
            {
                FillGrid(selectedTileType);
            }

            if (GUILayout.Button("Clear All", GUILayout.Width(70)))
            {
                FillGrid(TileType.Empty);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            if (grid == null) return;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            Rect gridArea = GUILayoutUtility.GetRect(
                gridWidth * cellSize + 20,
                gridHeight * cellSize + 20
            );

            // Handle mouse input
            Event e = Event.current;
            if (gridArea.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown || (e.type == EventType.MouseDrag && isDrawing))
                {
                    isDrawing = true;
                    Vector2 localPos = e.mousePosition - gridArea.position;
                    int x = Mathf.FloorToInt(localPos.x / cellSize);
                    int y = gridHeight - 1 - Mathf.FloorToInt(localPos.y / cellSize);

                    if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                    {
                        if (e.button == 0) // Left click - paint
                        {
                            grid[x, y] = selectedTileType;
                        }
                        else if (e.button == 1) // Right click - erase
                        {
                            grid[x, y] = TileType.Empty;
                        }
                        Repaint();
                    }
                }
                else if (e.type == EventType.MouseUp)
                {
                    isDrawing = false;
                }
            }

            // Draw grid cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Rect cellRect = new Rect(
                        gridArea.x + x * cellSize,
                        gridArea.y + (gridHeight - 1 - y) * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );

                    EditorGUI.DrawRect(cellRect, GetTileColor(grid[x, y]));
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();

            currentLevel = (LevelData)EditorGUILayout.ObjectField(
                "Level Data:",
                currentLevel,
                typeof(LevelData),
                false
            );

            EditorGUILayout.EndHorizontal();
        }

        private Color GetTileColor(TileType type)
        {
            return type switch
            {
                TileType.Empty => new Color(0.8f, 0.8f, 0.7f),
                TileType.Brick => new Color(0.8f, 0.4f, 0.2f),
                TileType.Steel => Color.gray,
                TileType.Water => new Color(0.2f, 0.4f, 0.8f),
                TileType.Trees => new Color(0.2f, 0.6f, 0.2f),
                TileType.Ice => new Color(0.7f, 0.9f, 1f),
                TileType.Base => Color.yellow,
                TileType.PlayerSpawn => Color.green,
                TileType.EnemySpawn => Color.red,
                _ => Color.magenta
            };
        }

        private void NewLevel()
        {
            if (EditorUtility.DisplayDialog("New Level",
                "Create a new level? Unsaved changes will be lost.", "Yes", "No"))
            {
                InitializeGrid();
                currentLevel = null;
            }
        }

        private void LoadLevel()
        {
            string path = EditorUtility.OpenFilePanel("Load Level", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                currentLevel = AssetDatabase.LoadAssetAtPath<LevelData>(path);

                if (currentLevel != null)
                {
                    gridWidth = currentLevel.GridWidth;
                    gridHeight = currentLevel.GridHeight;
                    grid = currentLevel.ParseTileData();
                }
            }
        }

        private void SaveLevel()
        {
            if (currentLevel == null)
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Level",
                    "NewLevel",
                    "asset",
                    "Save level data"
                );

                if (string.IsNullOrEmpty(path)) return;

                currentLevel = CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(currentLevel, path);
            }

            // Update level data
            currentLevel.SetTileDataFromGrid(grid);
            EditorUtility.SetDirty(currentLevel);
            AssetDatabase.SaveAssets();

            Debug.Log($"[LevelEditor] Level saved: {AssetDatabase.GetAssetPath(currentLevel)}");
        }

        private void ResizeGrid()
        {
            TileType[,] newGrid = new TileType[gridWidth, gridHeight];

            int copyWidth = Mathf.Min(gridWidth, grid?.GetLength(0) ?? 0);
            int copyHeight = Mathf.Min(gridHeight, grid?.GetLength(1) ?? 0);

            for (int x = 0; x < copyWidth; x++)
            {
                for (int y = 0; y < copyHeight; y++)
                {
                    newGrid[x, y] = grid[x, y];
                }
            }

            grid = newGrid;
        }

        private void FillGrid(TileType type)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = type;
                }
            }
        }
    }
}
#endif
