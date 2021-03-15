using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SLIDDES.LevelEditor.SideScroller3D
{
    /// <summary>
    /// Side Scroller 3D
    /// </summary>
    public class SS3D : EditorWindow
    {
        /// <summary>
        /// The parent of all created items
        /// </summary>
        public Transform parentOfItems;
        /// <summary>
        /// Current mouse position of sceneView in world position
        /// </summary>
        public Vector3 mousePositionScene;
        /// <summary>
        /// Current mouse GUIPoint position
        /// </summary>
        public Vector3 mousePositionGUIPoint;

        /// <summary>
        /// Is this toolbar currently inUse?
        /// </summary>
        private bool inUse;
        /// <summary>
        /// Was the editor inUse before the user stated play mode?
        /// </summary>
        private bool inUseBeforePlayMode;
        /// <summary>
        /// The current tool selected (draw, erase, etc)
        /// </summary>
        private int currentToolIndex;
        /// <summary>
        /// The object to create when clicking
        /// </summary>
        private Object objectToCreate;
        /// <summary>
        /// Show all z layer indexes
        /// </summary>
        private bool showAllZLayers;
        private int searchbarResultAmount;
        /// <summary>
        /// The type index of view the user wants to display the assets in the editorwindow
        /// </summary>
        private int currentAssetViewIndex;
        /// <summary>
        /// Int used to check if user wants to increase or decrease z layer index
        /// </summary>
        private int currentZLayerIncreaseIndex;
        /// <summary>
        /// The Vector3.z int for placing objects in the scene
        /// </summary>
        private int zLayerIndex;
        /// <summary>
        /// If the editor should show a gui button toggle for the editor in the scene view even if inUse is false
        /// </summary>
        private bool settingsAlwaysShowGUIEditorButton = true;
        /// <summary>
        /// The label a asset needs to show up in the editor, default is "SS3D-Asset"
        /// </summary>
        private string settingsLabelName;

        // Editor Window
        private readonly int editorSpacePixels = 10;
        /// <summary>
        /// Contains the searchbar result
        /// </summary>
        private string searchbarResult = "";
        /// <summary>
        /// The size of the asset picture
        /// </summary>
        private Vector2 editorAssetDisplaySize = new Vector2(48, 48);
        /// <summary>
        /// The size of the asset text box
        /// </summary>
        private Vector2 editorAssetTextDisplaySize = new Vector2(48, 48);
        /// <summary>
        /// Used for editor scrollbar
        /// </summary>
        private Vector2 editorScrollPosition;
        private string s;
        private Color c;
        
        private bool editorFoldoutTool = true;
        private bool editorFoldoutAssets = true;
        private bool editorFoldoutSettings;

        [MenuItem("Window/SLIDDES/Level Editor/Side Scroller 3D", false)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow window = GetWindow(typeof(SS3D), false, "SS3D", true); // Name
            window.minSize = new Vector2(500, 150);
        }


        #region Awake, OnEnable, OnDestroy
        private void Awake()
        {
            // Load EditorPrefs
            currentAssetViewIndex = EditorPrefs.GetInt("ss3d_currentAssetViewIndex", 0);
            settingsLabelName = EditorPrefs.GetString("ss3d_settingsLabelName", "SS3D-Asset");
        }

        private void OnEnable()
        {
            objectToCreate = null;

            // Add callbacks
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private void OnDestroy()
        {
            inUse = false;
            
            // Remove callbacks
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.playModeStateChanged -= PlayModeChanged;

            // Save EditorPrefs
            EditorPrefs.SetInt("ss3d_currentAssetViewIndex", currentAssetViewIndex);
            EditorPrefs.SetString("ss3d_settingsLabelName", settingsLabelName);
        }

        #endregion

        /// <summary>
        /// Editor Window Code
        /// </summary>
        public void OnGUI()
        {
            // Window code goes here
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins); // Make it look like unity inspector
            editorScrollPosition = EditorGUILayout.BeginScrollView(editorScrollPosition);
            EditorGUILayout.Space();

            #region Events
            Event e = Event.current;

            // Hotkeys
            if(e.isKey && e.type == EventType.KeyDown)
            {
                if(e.keyCode == KeyCode.F6) inUse = !inUse;                
                else if(e.keyCode == KeyCode.B)
                {
                    NewToolIndex(0);
                }
                else if(e.keyCode == KeyCode.D)
                {
                    NewToolIndex(1);
                }
            }
            #endregion

            #region Tools
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            editorFoldoutTool = EditorGUILayout.Foldout(editorFoldoutTool, " Tools", true);

            if(editorFoldoutTool)
            {
                EditorGUILayout.BeginHorizontal();
                // In use button
                c = GUI.color;
                if(inUse) GUI.color = Color.green; else GUI.color = Color.red;
                if(GUILayout.Button(new GUIContent("In Use", "Toggle Editor On/Off"), GUILayout.Width(100)))
                {
                    inUse = !inUse;
                }
                GUI.color = c;
                EditorGUILayout.Space(10);

                // Tools select
                GUIContent[] g = new GUIContent[] { new GUIContent("", EditorGUIUtility.IconContent("d_Grid.PaintTool").image, "Paint With Left Mouse Button (B)\nOnly Applies To Current Z Layer!"),
                                                    new GUIContent("", EditorGUIUtility.IconContent("d_Grid.EraserTool").image, "Erase With Left Mouse Button (D)\nOnly Applies To Current Z Layer!" )};
                NewToolIndex(GUILayout.Toolbar(currentToolIndex, g, GUILayout.MaxWidth(100)));
                EditorGUILayout.Space();

                // Z index
                EditorGUIUtility.labelWidth = 10;
                EditorGUIUtility.fieldWidth = 35;
                int prevLayerIndex = zLayerIndex; // Prevent updating zLayerVisablity every frame
                zLayerIndex = EditorGUILayout.IntField("Z", zLayerIndex);
                if(zLayerIndex != prevLayerIndex) UpdateZLayerVisability();
                // Z index arrow buttons
                g = new GUIContent[] { new GUIContent("", EditorGUIUtility.IconContent("d_tab_prev").image, "-1 Z Layer"),
                                        new GUIContent("", EditorGUIUtility.IconContent("d_tab_next").image, "+1 Z Layer" )};
                int prev = currentZLayerIncreaseIndex = 2;
                currentZLayerIncreaseIndex = GUILayout.Toolbar(2, g, GUILayout.MaxWidth(50));
                if(prev != currentZLayerIncreaseIndex)
                {
                    // Increase or decrease z layer index by 1
                    if(currentZLayerIncreaseIndex == 0) zLayerIndex--;
                    else if(currentZLayerIncreaseIndex == 1) zLayerIndex++;
                    UpdateZLayerVisability();
                }
                // Z index layer visablility
                Texture tEye; if(showAllZLayers) tEye = EditorGUIUtility.IconContent("d_scenevis_visible_hover").image; else tEye = EditorGUIUtility.IconContent("d_scenevis_hidden_hover").image;
                if(GUILayout.Button(new GUIContent("", tEye, "Show All Z Layers Or Only Current Layer")))
                {
                    showAllZLayers = !showAllZLayers;
                    UpdateZLayerVisability();
                }
                EditorGUILayout.Space();

                // 2D view button
                if(GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_SceneView2D").image, "Toggle 2D View Scene"), new GUILayoutOption[] { GUILayout.MinHeight(20), GUILayout.MinWidth(20)}))
                {
                    // This stuff apperently cannot auto run, disabled 
                    if(!GetWindow<SceneView>().in2DMode)
                    {
                        Vector3 position = GetWindow<SceneView>().pivot; // Position
                        position.z = -10;
                        GetWindow<SceneView>().pivot = position;
                        GetWindow<SceneView>().in2DMode = true;
                    }
                    else
                    {
                        GetWindow<SceneView>().in2DMode = false;
                    }
                }


                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            if(editorFoldoutTool) EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            #endregion

            #region Asset display
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            editorFoldoutAssets = EditorGUILayout.Foldout(editorFoldoutAssets, " Assets", true);

            if(editorFoldoutAssets)
            {
                // Get assets labeld with SS3D-Asset              
                string[] folderContent = AssetDatabase.FindAssets("l:" + settingsLabelName, new[] { "Assets" });
                EditorGUILayout.BeginHorizontal();
                // Searchbar
                EditorGUIUtility.labelWidth = 80;
                searchbarResult = EditorGUILayout.TextField("Search Asset", searchbarResult, GUILayout.Width(250));
                // Toggle asset view
                if(GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image, "Change View Layout")))
                {
                    currentAssetViewIndex++;
                    if(currentAssetViewIndex > 2) currentAssetViewIndex = 0;
                }
                // Display loaded assets amount
                EditorGUILayout.LabelField("Loaded: " + folderContent.Length + " Assets", EditorStyles.helpBox);
                searchbarResultAmount = 0; // updated at CreateItemButton, not working, doesnt update after calc
                //EditorGUILayout.LabelField("Showing: " + searchbarResultAmount.ToString() + " Assets", EditorStyles.helpBox);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(editorSpacePixels);

                // Display assets
                EditorGUILayout.BeginVertical();

                GameObject[] prefabs = new GameObject[folderContent.Length];

                switch(currentAssetViewIndex)
                {
                    case 0:
                        // Display as grid
                        int maxRowAmount = Mathf.Clamp(Mathf.FloorToInt(Screen.width / editorAssetDisplaySize.x) - 2, 1, 99);
                        int closer = maxRowAmount;
                        bool closedLayout = true;
                        // Get prefabs
                        for(int i = 0; i < folderContent.Length; i++)
                        {
                            closer--;
                            if(i % maxRowAmount == 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                closedLayout = false;
                            }
                            prefabs[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(folderContent[i]), typeof(GameObject)) as GameObject;
                            CreateItemButton(folderContent[i]);
                            if(closer <= 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                closer = maxRowAmount;
                                closedLayout = true;
                            }
                        }
                        if(!closedLayout)
                        {
                            GUILayout.FlexibleSpace(); // Aligns object to the left
                            EditorGUILayout.EndHorizontal();
                        }
                        break;
                    case 1:
                        // Display vertically
                        for(int i = 0; i < folderContent.Length; i++)
                        {
                            prefabs[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(folderContent[i]), typeof(GameObject)) as GameObject;
                            CreateItemButton(folderContent[i]);
                        }
                        break;
                    case 2:
                        // Display vertically, name only
                        for(int i = 0; i < folderContent.Length; i++)
                        {
                            prefabs[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(folderContent[i]), typeof(GameObject)) as GameObject;
                            CreateItemButton(folderContent[i]);
                        }
                        break;
                    default: Debug.LogWarning("[SS3D] Something whent wrong with setting the current Asset view. Setting it on default 0."); currentAssetViewIndex = 0; break;
                }

                EditorGUILayout.EndVertical();
                
            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            editorFoldoutSettings = EditorGUILayout.Foldout(editorFoldoutSettings, " Settings", true);

            if(editorFoldoutSettings)
            {
                EditorGUIUtility.labelWidth = 300;              

                // Show GUI button
                settingsAlwaysShowGUIEditorButton = EditorGUILayout.Toggle(new GUIContent("Always Show GUI Editor Button", ""), settingsAlwaysShowGUIEditorButton);
                // Label name
                EditorGUILayout.BeginHorizontal();
                settingsLabelName = EditorGUILayout.TextField(new GUIContent("Asset Label To Search For", "The name of the label the asset needs to have in order to show up"), settingsLabelName);
                if(GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("Refresh").image, "Reset Values"), GUILayout.Width(30)))
                {
                    settingsLabelName = "SS3D-Asset";
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            #endregion

            // Close off
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            // Prevent user from harm
            if(Application.isPlaying) return;

            Handles.BeginGUI();
            // Show a GUI button top left sceneview for editor inUse toggle
            if(settingsAlwaysShowGUIEditorButton)
            {
                s = "Disable SS3D (F6)"; if(!inUse) s = "Enable SS3D (F6)";
                if(GUI.Button(new Rect(10, 10, 120, 20), s))
                {
                    inUse = !inUse;
                }
            }
            else if(inUse)
            {
                if(GUI.Button(new Rect(10, 10, 120, 20), "Disable SS3D (F6)"))
                {
                    inUse = false;
                }
            }
            Handles.EndGUI();
            
            // Dont execute editor when not using
            if(!inUse) return;
            
            #region EIEM

            Event e = Event.current;

            if(e.type != EventType.MouseLeaveWindow) // Prevent Screen position out of view frustum Error
            {
                GetMousePositionScene(sceneView);
            }
            else return;
            // move custom gameobject here to show mousepos as alternative of Gizmos

            // Display mouse icon with current tool icon
            switch(currentToolIndex)
            {
                case 0:
                    EditorGUIUtility.AddCursorRect(new Rect(20, 20, sceneView.position.width, sceneView.position.height), MouseCursor.ArrowPlus);
                    break;
                case 1:
                    EditorGUIUtility.AddCursorRect(new Rect(20, 20, sceneView.position.width, sceneView.position.height), MouseCursor.ArrowMinus);
                    break;
                default: break;
            }

            // Get values
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            // User input
            if(e.isMouse)
            {
                // Left mouse button
                if(e.button == 0)
                {
                    if(eventType == EventType.MouseUp)
                    {
                        GUIUtility.hotControl = controlID;
                    }
                    else if(eventType == EventType.MouseDrag)
                    {
                        switch(currentToolIndex)
                        {
                            case 0: 
                                PlaceItem(sceneView);
                                break;
                            case 1: 
                                RemoveItem(e, sceneView); 
                                break;
                            default: Debug.LogError("toolindex"); break;
                        }
                    }
                    else if(eventType == EventType.MouseDown)
                    {
                        GUIUtility.hotControl = 0;
                        switch(currentToolIndex)
                        {
                            case 0: PlaceItem(sceneView);
                                break;
                            case 1: RemoveItem(e, sceneView); 
                                break;
                            default: Debug.LogError("toolindex"); break;
                        }
                    }
                }
            }
            else if(e.isKey)
            {
                // Shortcut keys
                if(e.type == EventType.KeyDown)
                {
                    if(e.keyCode == KeyCode.F6) inUse = !inUse;
                    else if(e.keyCode == KeyCode.B)
                    {
                        currentToolIndex = 0;
                    }
                    else if(e.keyCode == KeyCode.D)
                    {
                        currentToolIndex = 1;
                    }
                }
            }

            #endregion
        }

        #region Editor Window Functions

        /// <summary>
        /// Create selectable GUI button
        /// </summary>
        /// <param name="item"></param>
        private void CreateItemButton(string assetPath)
        {
            Object item = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetPath), typeof(Object));

            // Hide button if searchbarResult is not the same
            if(searchbarResult != "")
            {
                if(!item.name.ToLower().Contains(searchbarResult.ToLower())) return;
            }
            searchbarResultAmount++;

            // Based on currentAssetViewIndex show button
            Color c = GUI.color;
            GUIContent g;
            switch(currentAssetViewIndex)
            {
                case 0:
                    // Grid button
                    EditorGUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(editorAssetDisplaySize.x), GUILayout.MaxHeight(editorAssetDisplaySize.y + editorAssetTextDisplaySize.y) });
                    if(objectToCreate == item)
                    {
                        GUI.color = Color.green;
                    }
                    g = new GUIContent("", AssetPreview.GetAssetPreview(item), item.name);
                    if(GUILayout.Button(g, GUILayout.Width(editorAssetDisplaySize.x), GUILayout.Height(editorAssetDisplaySize.y)))
                    {
                        // Select button
                        if(objectToCreate == item)
                        {
                            // User clicked already selected button, deselect it
                            objectToCreate = null;
                        }
                        else
                        {
                            objectToCreate = item;
                        }
                        Repaint();
                    }
                    EditorStyles.label.wordWrap = true;
                    EditorGUILayout.LabelField(Truncate(item.name, 20), EditorStyles.helpBox);
                    GUI.color = c;
                    EditorGUILayout.EndVertical();
                    break;
                case 1:
                    // Vertaclly layerd button
                    EditorGUILayout.BeginHorizontal(); // TODO make entire horizontal bar clickable
                    if(objectToCreate == item)
                    {
                        GUI.color = Color.green;
                    }
                    g = new GUIContent("", AssetPreview.GetAssetPreview(item), item.name);
                    if(GUILayout.Button(g, GUILayout.Width(editorAssetDisplaySize.x), GUILayout.Height(editorAssetDisplaySize.y)))
                    {
                        // Select button
                        if(objectToCreate == item)
                        {
                            // User clicked already selected button, deselect it
                            objectToCreate = null;
                        }
                        else
                        {
                            objectToCreate = item;
                        }
                        Repaint();
                    }
                    EditorStyles.label.wordWrap = true;
                    GUI.skin.label.wordWrap = true;
                    EditorGUILayout.LabelField(item.name, EditorStyles.wordWrappedLabel);
                    GUI.color = c;
                    EditorGUILayout.EndHorizontal();
                    break;
                case 2:
                    // Vertaclly layerd button with name only
                    if(objectToCreate == item)
                    {
                        GUI.color = Color.green;
                    }
                    GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                    if(GUILayout.Button(item.name))
                    {
                        // Select button
                        if(objectToCreate == item)
                        {
                            // User clicked already selected button, deselect it
                            objectToCreate = null;
                        }
                        else
                        {
                            objectToCreate = item;
                        }
                        Repaint();
                    }
                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    GUI.color = c;
                    break;
                default: Debug.LogWarning("[SS3D] Something whent wrong with setting the current Asset view. Setting it on default 0."); currentAssetViewIndex = 0; break;
            }

        } // TODO this can just be an gameobject field instead of string

        /// <summary>
        /// Switches the current toolindex of this and EIEM (execute in edit mode)
        /// </summary>
        /// <param name="newIndex"></param>
        private void NewToolIndex(int newIndex)
        {
            // Ignore if SS3DEIEM updated its currentToolIndex
            //if(currentToolIndex != SS3DEIEM.currentToolIndex) return;

            currentToolIndex = newIndex;
            //SS3DEIEM.currentToolIndex = newIndex;
            Repaint();
        }

        /// <summary>
        /// Updates the current z layer visabilty
        /// </summary>
        private void UpdateZLayerVisability()
        {
            if(parentOfItems == null) CreateParentItems();
            if(showAllZLayers)
            {
                // Show all
                foreach(Transform child in parentOfItems)
                {
                    child.gameObject.SetActive(true);
                }
            }
            else
            {
                // Hide all but 1
                foreach(Transform child in parentOfItems)
                {
                    child.gameObject.SetActive(false);
                }
                parentOfItems.Find(zLayerIndex.ToString())?.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Used as callback function when Editor Application changes mode
        /// </summary>
        private void PlayModeChanged(PlayModeStateChange playModeState)
        {
            switch(playModeState)
            {
                case PlayModeStateChange.EnteredEditMode:
                    // Enable editor when play mode is over and user was using editor
                    if(inUseBeforePlayMode)
                    {
                        inUse = true;
                        inUseBeforePlayMode = false;
                    }

                    UpdateZLayerVisability();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    // Disable editor when in play mode
                    if(inUse)
                    {
                        inUse = false;
                        inUseBeforePlayMode = true;
                    }                    
                    break;
                case PlayModeStateChange.ExitingPlayMode:                    
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region EIEM Functions (Execute In Edit Mode)

        private void CreateParentItems()
        {
            GameObject parent = GameObject.Find("[SS3D] Parent");
            if(parent == null)
            {
                // Create parent
                parent = new GameObject();
                parent.name = "[SS3D] Parent";
                parent.transform.position = Vector3.zero;
                parentOfItems = parent.transform;
            }
            else
            {
                // Assign existing parent
                parentOfItems = parent.transform;
            }
        }

        private void GetMousePositionScene(SceneView scene)
        {
            Vector3 mousePosition = Event.current.mousePosition;

            // Check if mousePosition is in sceneView
            if(mousePosition.x >= 0 && mousePosition.x <= scene.camera.pixelWidth && mousePosition.y >= 0 && mousePosition.y <= scene.camera.pixelHeight) { } else return;
            mousePositionGUIPoint = mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePositionGUIPoint);

            mousePositionScene = ray.origin;
            mousePositionScene = SnapToGrid(mousePositionScene) - new Vector3(0.5f, 0.5f, 0);
            mousePositionScene.z = zLayerIndex;
        }

        private Vector3 SnapToGrid(Vector3 v)
        {
            Vector3 gridPos = v;
            int gridSize = 1;
            gridPos.x = Mathf.Round(v.x / gridSize) * gridSize + gridSize / 2f;
            gridPos.y = Mathf.Round(v.y / gridSize) * gridSize + gridSize / 2f;
            return gridPos;
        }

        /// <summary>
        /// Check if the position under mouse world space is occupied by gameobject
        /// </summary>
        /// <returns></returns>
        private bool PositionIsOccupied(SceneView scene)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePositionGUIPoint);

            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.transform.gameObject != null)
                {
                    //print(hit.transform.gameObject.name);
                    return true;
                }
            }
            return false;
        } // TODO replace this with a SO grid system that keeps track of all placed objects positions

        private void PlaceItem(SceneView sceneView)
        {
            if(PositionIsOccupied(sceneView)) return;
                        
            // Place item                
            if(objectToCreate != null)
            {
                //GameObject a = Instantiate(objectToCreate, mousePositionScene, Quaternion.identity) as GameObject;
                GameObject a = PrefabUtility.InstantiatePrefab(objectToCreate) as GameObject;
                a.transform.position = mousePositionScene;
                a.transform.rotation = Quaternion.identity;

                if(parentOfItems == null) CreateParentItems();

                // Find parent layer zIndex
                Transform t = parentOfItems.Find(mousePositionScene.z.ToString());
                if(t == null)
                {
                    // Create parent child
                    GameObject b = new GameObject(mousePositionScene.z.ToString());
                    b.transform.SetParent(parentOfItems);
                    t = b.transform;
                }

                a.transform.SetParent(t);
                Undo.RegisterCreatedObjectUndo(a, "Created " + a.name); // Add to undo stack
            }
            else
            {
                Debug.LogWarning("[SS3D] Please select an object from the 'Assets' dropdown to place.");
            }
        }

        private void RemoveItem(Event e, SceneView scene)
        {
            Vector3 mousePos = e.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;

            Ray ray = scene.camera.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                // Check if hit is in same current layer
                if(hit.transform.parent != null && hit.transform.parent.name == mousePositionScene.z.ToString())
                {
                    Undo.DestroyObjectImmediate(hit.transform.gameObject);
                }
            }
        }

        #endregion

        #region Static functions

        /// <summary>
        /// Limit string size
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxChars"></param>
        /// <returns></returns>
        public static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars);
        } // IMPROVE no need for it to be static

        #endregion
    }
}

// Used links
// Drawing SceneGUI https://answers.unity.com/questions/58018/drawing-to-the-scene-from-an-editorwindow.html
// Find assets https://docs.unity3d.com/2020.1/Documentation/ScriptReference/AssetDatabase.FindAssets.html
// Get assets without resources folder https://gamedev.stackexchange.com/questions/160497/how-to-instantiate-prefab-outside-resources-folder/160537
// Load asset at path https://docs.unity3d.com/2020.1/Documentation/ScriptReference/AssetDatabase.LoadAssetAtPath.html
// Creating prefabs https://docs.unity3d.com/ScriptReference/PrefabUtility.InstantiatePrefab.html