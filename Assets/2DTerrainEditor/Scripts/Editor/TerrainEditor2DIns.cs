using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainEditor2D))]
public class TerrainEditor2DIns: Editor
{
    private TerrainEditor2D _myTerrainEditor2D;

    public float BrushSize = 5;
    public float BrushHardness = 0.25f;

    private bool _showCapSettings;
    private bool _showSettings = true;
    private bool _showRndSettings;

    private bool _startEdit;
    private bool _digMode;
    private bool _brushSizeMode;

    private bool _playMode;
    
    void OnEnable()
    {
        if (EditorPrefs.HasKey("2DTE_BrushSize"))
            BrushSize = EditorPrefs.GetFloat("2DTE_BrushSize");
        if (EditorPrefs.HasKey("2DTE_BrushHardness"))
            BrushHardness = EditorPrefs.GetFloat("2DTE_BrushHardness");
        if (EditorPrefs.HasKey("2DTE_ShowCapSettings"))
            _showCapSettings = EditorPrefs.GetBool("2DTE_ShowCapSettings");
        if (EditorPrefs.HasKey("2DTE_ShowSettings"))
            _showSettings = EditorPrefs.GetBool("2DTE_ShowSettings");
        if (EditorPrefs.HasKey("2DTE_ShowRndSettings"))
            _showRndSettings = EditorPrefs.GetBool("2DTE_ShowRndSettings");

        _myTerrainEditor2D = target as TerrainEditor2D;

        _myTerrainEditor2D.GetComponent<EdgeCollider2D>().enabled = false;
        _myTerrainEditor2D.enabled = true;
    }

    public override void OnInspectorGUI()
    {
        BrushSize = EditorGUILayout.Slider("Brush size", BrushSize, 0.1f, 50);
        BrushHardness = EditorGUILayout.Slider("Brush hardness", BrushHardness, 0.1f, 1);

        EditorGUILayout.HelpBox("Hold [LMB] - raise terrain \nHold [D + LMB] - lower terrain \nHold [ALT+Mouse Wheel] - change brush size", MessageType.None);

        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();

        _myTerrainEditor2D.MainMaterial = (Material)EditorGUILayout.ObjectField("Main material", _myTerrainEditor2D.MainMaterial, typeof(Material), false);
        _myTerrainEditor2D.TextureSize = EditorGUILayout.IntField("Texture size", _myTerrainEditor2D.TextureSize);

        _myTerrainEditor2D.FixSides = EditorGUILayout.Toggle("Fix sides", _myTerrainEditor2D.FixSides);
        EditorGUI.BeginDisabledGroup(!_myTerrainEditor2D.FixSides);
        _myTerrainEditor2D.LeftFixedPoint = EditorGUILayout.FloatField("Left fixed point", _myTerrainEditor2D.LeftFixedPoint);
        _myTerrainEditor2D.RightFixedPoint = EditorGUILayout.FloatField("Right fixed point", _myTerrainEditor2D.RightFixedPoint);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        _showRndSettings = EditorGUILayout.Foldout(_showRndSettings, "Randomizer");
        if (_showRndSettings)
        {
            EditorGUI.indentLevel = 1;

            _myTerrainEditor2D.RndHeight = EditorGUILayout.Slider("Height", _myTerrainEditor2D.RndHeight, 0.1f, _myTerrainEditor2D.Height);
            _myTerrainEditor2D.RndHillsCount = EditorGUILayout.IntSlider("Hills count", _myTerrainEditor2D.RndHillsCount, 1, _myTerrainEditor2D.Width / 2);
            _myTerrainEditor2D.RndAmplitude = EditorGUILayout.Slider("Amplitude", _myTerrainEditor2D.RndAmplitude, 0.1f, _myTerrainEditor2D.Height / 2f);

            if (GUILayout.Button("Randomize"))
            {
                _myTerrainEditor2D.RandomizeTerrain();
            }

            EditorGUI.indentLevel = 0;
        }

        

        _showCapSettings = EditorGUILayout.Foldout(_showCapSettings, "Cap Settings");
        if (_showCapSettings)
        {
            EditorGUI.indentLevel = 1;
            
            _myTerrainEditor2D.CreateCap = EditorGUILayout.Toggle("Create cap", _myTerrainEditor2D.CreateCap);
            EditorGUI.BeginDisabledGroup(!_myTerrainEditor2D.CreateCap);
            _myTerrainEditor2D.CapHeight = EditorGUILayout.FloatField("Height", _myTerrainEditor2D.CapHeight);
            _myTerrainEditor2D.CapOffset = EditorGUILayout.FloatField("Offset", _myTerrainEditor2D.CapOffset);
            _myTerrainEditor2D.CapMaterial = (Material)EditorGUILayout.ObjectField("Material", _myTerrainEditor2D.CapMaterial, typeof(Material), false);
            _myTerrainEditor2D.CapTextureTiling = EditorGUILayout.IntField("Texture tiling", _myTerrainEditor2D.CapTextureTiling);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel = 0;
        }

        if (EditorGUI.EndChangeCheck())
        {
            CheckValues();
            _myTerrainEditor2D.EditMesh(_myTerrainEditor2D.GetVertsPos());
        }

        _showSettings = EditorGUILayout.Foldout(_showSettings, "Terrain Settings");
        if (_showSettings)
        {
            EditorGUI.indentLevel = 1;

            _myTerrainEditor2D.Width = EditorGUILayout.IntField("Width", _myTerrainEditor2D.Width);
            _myTerrainEditor2D.Height = EditorGUILayout.IntField("Height", _myTerrainEditor2D.Height);
            _myTerrainEditor2D.Resolution = EditorGUILayout.IntField("Resolution", _myTerrainEditor2D.Resolution);
            
            if (GUILayout.Button("Apply"))
            {
                _myTerrainEditor2D.CreateTerrain();
            }

            EditorGUI.indentLevel = 0;
        }

        if (EditorApplication.isPlaying && !_playMode)
        {
            _myTerrainEditor2D.gameObject.GetComponent<EdgeCollider2D>().enabled = true;
            _myTerrainEditor2D.UpdateCollider();
            _playMode = true;
        }
        else _playMode = false;

        
        if (GUI.changed)
        {
            CheckValues();
            
            EditorUtility.SetDirty(target);
        }
    }

    void CheckValues()
    {
        if (_myTerrainEditor2D.Width < 10)
            _myTerrainEditor2D.Width = 10;
        if (_myTerrainEditor2D.Height < 10)
            _myTerrainEditor2D.Height = 10;
        if (_myTerrainEditor2D.Resolution < 1)
            _myTerrainEditor2D.Resolution = 1;
        if (_myTerrainEditor2D.CapHeight < 0.1f)
            _myTerrainEditor2D.CapHeight = 0.1f;
        if (_myTerrainEditor2D.LeftFixedPoint < 0)
            _myTerrainEditor2D.LeftFixedPoint = 0;
        if (_myTerrainEditor2D.LeftFixedPoint > _myTerrainEditor2D.Height)
            _myTerrainEditor2D.LeftFixedPoint = _myTerrainEditor2D.Height;
        if (_myTerrainEditor2D.RightFixedPoint < 0)
            _myTerrainEditor2D.RightFixedPoint = 0;
        if (_myTerrainEditor2D.RightFixedPoint > _myTerrainEditor2D.Height)
            _myTerrainEditor2D.RightFixedPoint = _myTerrainEditor2D.Height;
        

        _myTerrainEditor2D.gameObject.renderer.material = _myTerrainEditor2D.MainMaterial;
    }

    private Vector2 _mousePos;
    private Vector2 _handleLocalPos;
    void OnSceneGUI()
    {
        if (Camera.current != null)
        {
            _mousePos = Camera.current.ScreenToWorldPoint(new Vector2(Event.current.mousePosition.x, (Camera.current.pixelHeight - Event.current.mousePosition.y)));
            _handleLocalPos = _mousePos - new Vector2(_myTerrainEditor2D.transform.position.x, _myTerrainEditor2D.transform.position.y);

            Handles.color = Color.green;
            Handles.CircleCap(0, _mousePos, Quaternion.identity, BrushSize);
        }

        #region DrawTerrainBorders
        Vector3 terrainPos = _myTerrainEditor2D.transform.position;
        Handles.color = new Color(1,1,1, 0.5f);
        Handles.DrawLine(terrainPos, terrainPos + new Vector3(0, _myTerrainEditor2D.Height));
        Handles.DrawLine(terrainPos, terrainPos + new Vector3(_myTerrainEditor2D.Width, 0));
        Handles.DrawLine(terrainPos + new Vector3(0, _myTerrainEditor2D.Height), terrainPos + new Vector3(0, _myTerrainEditor2D.Height) + new Vector3(_myTerrainEditor2D.Width, 0));
        Handles.DrawLine(terrainPos + new Vector3(_myTerrainEditor2D.Width, 0), terrainPos + new Vector3(0, _myTerrainEditor2D.Height) + new Vector3(_myTerrainEditor2D.Width, 0));
        #endregion

        #region Events
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            _myTerrainEditor2D.gameObject.GetComponent<EdgeCollider2D>().enabled = false;
            _startEdit = true;
        }

        if (Event.current.type == EventType.MouseUp)
        {
            _startEdit = false;
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
            _digMode = true;
        if (Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.D)
            _digMode = false;
        if (Event.current.alt)
        {
            if (Event.current.type == EventType.ScrollWheel && Event.current.delta.y > 0)
                BrushSize -= 0.2f;
            if (Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0)
                BrushSize += 0.2f;
            if (BrushSize <= 0.1f)
                BrushSize = 0.1f;

            Event.current.Use();
        }

        
        #endregion

        #region StartEditMesh
        if (_startEdit)
        {
            Vector3[] vertsPos = _myTerrainEditor2D.GetVertsPos();

            for (int i = 0; i < vertsPos.Length; i += 2)
            {
                if (Vector2.Distance(vertsPos[i], _handleLocalPos) <= BrushSize)
                {
                    float vertOffset = BrushSize - Vector2.Distance(vertsPos[i], _handleLocalPos);

                    if (_digMode)
                        vertsPos[i] -= new Vector3(0, vertOffset * (BrushHardness * 0.1f));
                    else vertsPos[i] += new Vector3(0, vertOffset * (BrushHardness * 0.1f));
                    if (vertsPos[i].y < 0.1f)
                        vertsPos[i].y = 0.1f;
                    if (vertsPos[i].y > _myTerrainEditor2D.Height)
                        vertsPos[i].y = _myTerrainEditor2D.Height;
                        
                }
            }
            _myTerrainEditor2D.EditMesh(vertsPos);

            Selection.activeGameObject = _myTerrainEditor2D.gameObject;
        }
        #endregion

        #region ConfigureHandles

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        if (_myTerrainEditor2D.renderer != null)
            EditorUtility.SetSelectedWireframeHidden(_myTerrainEditor2D.renderer, true);
        if (_myTerrainEditor2D.CapObj != null)
        {
            if (_myTerrainEditor2D.CapObj.renderer != null)
                EditorUtility.SetSelectedWireframeHidden(_myTerrainEditor2D.CapObj.renderer, true);
        }
        
        #endregion
    }


    public void SaveMesh(GameObject obj, Mesh mesh)
    {
        string path = "Assets/2DTerrainEditor/SavedMeshes/";

        AssetDatabase.Refresh();

        if (!AssetDatabase.Contains(mesh))
        {
            AssetDatabase.CreateAsset(mesh, path + mesh.name + ".asset");
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.SaveAssets();
        }
    }

    void OnDisable()
    {
        EditorPrefs.SetFloat("2DTE_BrushSize", BrushSize);
        EditorPrefs.SetFloat("2DTE_BrushHardness", BrushHardness);
        EditorPrefs.SetBool("2DTE_ShowCapSettings", _showCapSettings);
        EditorPrefs.SetBool("2DTE_ShowSettings", _showSettings);
        EditorPrefs.SetBool("2DTE_ShowRndSettings", _showRndSettings);

        if (_myTerrainEditor2D != null)
        {
            SaveMesh(_myTerrainEditor2D.gameObject, _myTerrainEditor2D.GetComponent<MeshFilter>().sharedMesh);
            if (_myTerrainEditor2D.CreateCap)
                SaveMesh(_myTerrainEditor2D.CapObj, _myTerrainEditor2D.CapObj.GetComponent<MeshFilter>().sharedMesh);

            _myTerrainEditor2D.GetComponent<EdgeCollider2D>().enabled = true;
            _myTerrainEditor2D.UpdateCollider();
        }
    }
}

public static class Terrain2DCreator
{
    [MenuItem("GameObject/Create Other/Terrain2D")]
    static void CreateTerrain2D()
    {
        GameObject terrain2D = new GameObject("New Terrain2D");
        terrain2D.AddComponent<TerrainEditor2D>();

        if (!EditorPrefs.HasKey("LastMeshId"))
            EditorPrefs.SetInt("LastMeshId", 0);
        int lastMeshId = EditorPrefs.GetInt("LastMeshId");
        terrain2D.GetComponent<TerrainEditor2D>().MeshId = lastMeshId;
        lastMeshId++;
        EditorPrefs.SetInt("LastMeshId", lastMeshId);

        terrain2D.GetComponent<TerrainEditor2D>().CreateTerrain();
        Selection.activeGameObject = terrain2D.gameObject;
    }
}
