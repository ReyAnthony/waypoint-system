using UnityEditor;
using UnityEngine;
using WPSystem.Runtime;

namespace WPSystem.Editor
{
    /*
     * Class for the in-editor buttons
     * Not much to say, pretty straightforward.
     */
    internal class WaypointEditorWindow : EditorWindow
    {
        private static Waypoint _wp;
        private static WaypointSystem _wps;
        private static bool _hasEnabled;
        
        public static void Enable()
        {
            //bool to avoid adding the function multiple times to the callback 
            if (!_hasEnabled)
            {
                SceneView.duringSceneGui += OnScene; //hooking to the scene GUI, to draw on it. 
                
                //just used to intercept the delete event in hierarchy. 
                EditorApplication.hierarchyWindowItemOnGUI += (id, rect) =>
                {
                    if (_wp == null) return;
                    HandleWaypointDeletion();
                };
                _hasEnabled = true;
            }
            
        }
        
        public static void Disable()
        {
            SceneView.duringSceneGui -= OnScene;
            _hasEnabled = false;
        }

        private static void OnScene(SceneView sceneview)
        {
            void BeginCentered(bool area)
            {
                if(area) Handles.BeginGUI();
                if(area) GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                if(area) GUILayout.Space(WaypointSettings.GetInstance().UiPosition == UIPosition.Top ? 5 : Screen.height - 100);
                    GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                            
            }

            void EndCentered(bool area)
            {
                        GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                if(area) GUILayout.EndArea();
                if(area) Handles.EndGUI();
            }

            if (Selection.activeObject != null)
            {
                if (Selection.activeObject.GetType() != typeof(GameObject)) return;
                
                var activeObject = Selection.activeGameObject;
                //non generic method is faster http://chaoscultgames.com/2014/03/unity3d-mythbusting-performance/
                _wp = (Waypoint) activeObject.GetComponent(typeof(Waypoint));
                _wps = (Runtime.WaypointSystem) activeObject.GetComponent(typeof(Runtime.WaypointSystem));
            }
            
            //if no system or waypoint is selection
            if (_wp == null && _wps == null)
            {
                BeginCentered(true);
                if (GUILayout.Button("Create new waypoint sytem"))
                {
                    //Get a point where to spawn the system
                    Camera sceneCam = SceneView.currentDrawingSceneView.camera;
                    Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f,0.5f,10f));
                    Runtime.WaypointSystem.Create(spawnPos);
                }
                EndCentered(true);
                return;
            }

            //if the system is selected
            if (_wps)
            {
                if (_wps.transform.childCount == 0)
                {
                    _wps.AddNewWayPoint();
                    Selection.activeObject = _wp.gameObject;
                }
                
                _wp = _wps.StartingWaypoint;
            }

            var wpsParent = _wp.GetComponentInParent<WaypointSystem>();
            HandleWaypointDeletion();
            
            BeginCentered(true);
            if (GUILayout.Button("Insert (end)"))
            {
                wpsParent.AddNewWayPoint();
            }
            
            if (GUILayout.Button("Insert (after)"))
            {
                var si = _wp.transform.GetSiblingIndex() +1;
                wpsParent.AddNewWayPoint(si);
            }
            
            if (GUILayout.Button("Delete all"))
            {
                wpsParent.DeleteWaypoints();
            }
            
            if (GUILayout.Button("Delete selected"))
            {
                var si = _wp.transform.GetSiblingIndex();
                wpsParent.DeleteWaypoint(si );
            }
            
            if (GUILayout.Button("Delete system"))
            {
                wpsParent.DestroySystem();
            }
            
            EndCentered(false);
            BeginCentered(false);

            if (wpsParent.LoopType == LoopType.Loop)
            {
                if (GUILayout.Button("Loop"))
                {
                    wpsParent.SetLoopType(LoopType.PingPong); 
                }
            }
            else if (wpsParent.LoopType == LoopType.PingPong)
            {
                if (GUILayout.Button("PingPong"))
                {
                    wpsParent.SetLoopType(LoopType.OneWay);
                }
            }
            else
            {
                if (GUILayout.Button("One-way"))
                {
                    wpsParent.SetLoopType(LoopType.Loop);
                }
            }

            if (GUILayout.Button("Reset waypoints to system default"))
            {
                wpsParent.ResetWaypointToDefaultsFromSystem();
            }
            
            if (GUILayout.Button("Reset system to default"))
            {
                wpsParent.ResetSystemToDefault();
            }
            
            EndCentered(true);
        }
        
        private static void HandleWaypointDeletion()
        {
            var wpsParent = _wp.GetComponentInParent<WaypointSystem>();
            //Intercept the delete event
            if (Event.current != null && Event.current.isKey && Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Delete))
            {
                Event.current.Use(); //eat user events (like delete)
                //delete the system
                if (_wps != null)
                {
                    _wps.DestroySystem();
                }
                else
                {
                    var si = _wp.transform.GetSiblingIndex();
                    wpsParent.DeleteWaypoint(si);
                }
            }
        }
    }
}