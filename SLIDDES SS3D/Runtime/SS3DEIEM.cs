#if UNITY_EDITOR // This script is only used in the editor, but it cannot be inside the Editor folder since it has [ExecuteInEditMode]
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SLIDDES.LevelEditor.SideScroller3D
{
    /// <summary>
    /// Side Scroller 3D Excecute In Edit Mode
    /// </summary>
    [ExecuteInEditMode]
    public class SS3DEIEM : MonoBehaviour // SS#D can only communicate 1 way to this
    {
        public static SS3DEIEM Instance { get; set; }

        public static bool inUse;

        public static int currentToolIndex;

        /// <summary>
        /// The current object to create
        /// </summary>
        public static GameObject objectToCreate;
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

        [HideInInspector] public int zIndex = 0;



        private void Awake()
        {
            // Set instance
            if(Instance != null) DestroyImmediate(Instance.gameObject);
            Instance = this;
        }

        #region Enable, Destroy, OnFocus

        void OnEnable()
        {
            SceneView.duringSceneGui += this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnScene;
            // Assign parent
            CreateParentItems();            
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui -= this.OnScene;
        }


        #endregion

        void OnScene(SceneView scene)
        {
            if(!inUse) return;

            //Cursor.SetCursor(Resources.Load<Texture2D>("d_eyeDropper.Large"), Vector2.zero, CursorMode.Auto); causes flikkering
            Event e = Event.current;

            if(e.type != EventType.MouseLeaveWindow) // Prevent Screen position out of view frustum Error
            {
                GetMousePositionScene(scene);
            }
            else return;
            // move custom gameobject here to show mousepos as alternative of Gizmos

            //return;

            Handles.BeginGUI();
            //var controlID = GUIUtility.GetControlID(FocusType.Passive);
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            EventType currentEventType = Event.current.GetTypeForControl(controlID);
            // Do your drawing here using GUI.
            if(inUse)
            {
                if(GUI.Button(new Rect(10, 10, 120, 20), "Disable Editor (F6)"))
                {
                    inUse = false;
                    Debug.Log("yo");
                }
            }
            //GUIUtility.hotControl = 0;
            Handles.EndGUI();

            // Get values
            //var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            // Left mouse button
            if(e.button == 0)
            {
                if(eventType == EventType.MouseUp)
                {
                    //Debug.Log("Mouse Up!");
                    GUIUtility.hotControl = controlID;
                }
                else if(eventType == EventType.MouseDrag)
                {
                    //Debug.Log("Mouse Drag!");
                    //e.Use();
                    switch(currentToolIndex)
                    {
                        case 0: PlaceItem(scene); break;
                        case 1: RemoveItem(e, scene); break;
                        default: Debug.LogError("toolindex"); break;
                    }
                }
                else if(eventType == EventType.MouseDown)
                {
                    //Debug.Log("Mouse Down!");
                    GUIUtility.hotControl = 0;
                    //e.Use();
                    switch(currentToolIndex)
                    {
                        case 0: PlaceItem(scene); break;
                        case 1: RemoveItem(e, scene); break;
                        default: Debug.LogError("toolindex"); break;
                    }
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            // Do your drawing here using Handles.
            //Handles.BeginGUI();
            ////var controlID = GUIUtility.GetControlID(FocusType.Passive);
            //int controlID = GUIUtility.GetControlID(FocusType.Passive);
            //HandleUtility.AddDefaultControl(controlID);
            //EventType currentEventType = Event.current.GetTypeForControl(controlID);
            //// Do your drawing here using GUI.
            //if(inUse)
            //{
            //    if(GUI.Button(new Rect(10, 10, 120, 20), "Disable Editor (F6)"))
            //    {
            //        inUse = false;
            //        Debug.Log("yo");
            //    }
            //}
            ////GUIUtility.hotControl = 0;
            //Handles.EndGUI();

            // Key triggers
            Event e = Event.current;
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
            mousePositionScene.z = zIndex;
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
        }
               

        
        #region OnScene Functions

        private void PlaceItem(SceneView scene)
        {
            
            if(PositionIsOccupied(scene)) return;

            // Place item                
            if(objectToCreate != null)
            {
                GameObject a = Instantiate(objectToCreate, mousePositionScene, Quaternion.identity);

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

        
    }

    /// <summary>
    /// When the user quits the application the gameobject in the scene needs to be destroyed
    /// </summary>
    [InitializeOnLoad]
    public class Toolbar_0_Quit
    { 
        private static void EditorQuit()
        {
            EditorApplication.quitting += Quit;
        }

        private static void Quit()
        {
            Debug.Log("[Level Editor 0] Quitting the Editor");
            // Check if the instance gameobject is still there => destroy it
            if(SS3DEIEM.Instance.gameObject != null)
                MonoBehaviour.DestroyImmediate(SS3DEIEM.Instance.gameObject);
        }
    }
}

// Links
// Mouse pos in sceneView https://forum.unity.com/threads/how-to-get-mouseposition-in-scene-view.208911/
// Snap grid https://answers.unity.com/questions/1446220/snap-object-to-grid-with-offset.html
// Mouse click from scene https://answers.unity.com/questions/1260602/how-to-get-mouse-click-world-position-in-the-scene.html
// Hide tools https://forum.unity.com/threads/hiding-default-transform-handles.86760/
// Disable right mouse drag https://gamedev.stackexchange.com/questions/179984/disable-default-unity-editor-behaviour-on-input
// Mouse down / drag / up https://forum.unity.com/threads/hurr-eventtype-mouseup-not-working-on-left-clicks.99909/

// Changing mouse icon https://docs.unity3d.com/ScriptReference/EditorGUIUtility.IconContent.html
// unity editor build in icons https://unitylist.com/p/5c3/Unity-editor-icons
#endif