#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace WPSystem.Runtime
{
    /*
     * This is the second part of the class, it contains everything related to the editor UI. 
     */
    [ExecuteAlways]
    public partial class WaypointSystem
    {
        //when we listen for hierarchy change, we can"t undo directly and need to delay the call
        private Action _delayedUndo;
        //OnTransformChildrenChanged can be called multipletimes, but we won't show the popup multiple times
        private bool _calledThisFrame;

        //Create the system at the designed position
        internal static void Create(Vector3 spawnPos)
        {
            var go = new GameObject();
            go.transform.position = spawnPos;
            go.name = "Waypoints System";
            
            var waypointSystem = go.AddComponent<WaypointSystem>();
            waypointSystem.InitDefaultValueFromScriptable();
            waypointSystem.AddNewWayPoint();
            
            Undo.RegisterCreatedObjectUndo(go, "Created waypoints");
            Selection.activeObject = go;
        }
        
        private void OnDrawGizmos()
        {
            if (transform.childCount == 0) return;
            
            Gizmos.DrawWireMesh(Resources.Load<MeshFilter>("start_flag").sharedMesh, _startingWaypoint.transform.position);
            
            if (_loopType == LoopType.Loop)
            {
                //draws the last arrow when looping (the rest is drawed bu the waypoint themselves)
                Gizmos.color = this._pathColor;
                var lastWp = this.transform.GetChild(this.transform.childCount - 1);
                Gizmos.DrawLine(_startingWaypoint.transform.position, lastWp.transform.position);
                Gizmos.DrawWireMesh(Resources.Load<MeshFilter>("direction_arrow").sharedMesh,
                    (lastWp.transform.position + _startingWaypoint.transform.position) / 2,
                    Quaternion.LookRotation((_startingWaypoint.transform.position - lastWp.transform.position)
                        .normalized),
                    Vector3.one / 3);
            }
        }
        
        //execute always allow this to be called while in editor mode
        //is called twice for some reasons sometimes..
        private void OnTransformChildrenChanged()
        {
            bool wasUndone = false;
            /*
             * We want to check if anything other than a waypoint was dragged
             * in as a children to the system.
             *
             * If so, we undo the action. 
             */
            foreach (Transform t in transform)
            {
                if (t.gameObject.GetComponent<Waypoint>() == null)
                {
                    wasUndone = true;
                    //we need to delay the undo or else, it messes up with the transforms for some reason..
                    _delayedUndo = Undo.PerformUndo;
                }
            }

            if (wasUndone && !_calledThisFrame)
            {
                EditorUtility.DisplayDialog("error", "Please don't put anything under the waypointSystem", "OK");
                _calledThisFrame = true;
            }
        }

        //run after the hierarchy modifications 
        //perfect for delayed undo
        private void LateUpdate()
        {
            _delayedUndo?.Invoke();
            _delayedUndo = null;
            _calledThisFrame = false;
        }

        private void InitDefaultValue(Waypoint wp)
        {
            wp.StickMode = _defaultStickMode;
            wp.DistanceToReach = _defaultDistanceToReach;
        }

        //uses WaypointSettings to get the default back and set them. 
        private void InitDefaultValueFromScriptable()
        {
            _defaultStickMode = WaypointSettings.GetInstance().DefaultStickMode;
            _loopType = WaypointSettings.GetInstance().DefaultLoopType;
            _waypointsColor = WaypointSettings.GetInstance().WaypointsColor;
            _pathColor = WaypointSettings.GetInstance().PathColor;
            _defaultDistanceToReach = WaypointSettings.GetInstance().DefaultDistanceToReach;
            StartAtTheNearest = WaypointSettings.GetInstance().StartAtTheNearest;
        }
        
        internal void DeleteWaypoints()
        {
            //We delete the last child until there's only one lef.
            while (transform.childCount > 1)
            {
                var toDelete = transform.GetChild(transform.childCount - 1).gameObject;
                Undo.DestroyObjectImmediate(toDelete);
            }
            
            //then we set it as active.
            Selection.activeObject = this.transform.GetChild(0).gameObject;
        }

        internal void DeleteWaypoint(int index)
        {
            var objToDelete =  transform.GetChild(index).gameObject;
            Undo.DestroyObjectImmediate(objToDelete);
            
            //We always needs at least one wp
            if ( _startingWaypoint == null && transform.childCount == 0)
            {
                AddNewWayPoint();
            }
            //change the starting waypoint if we deleted it
            else if ( _startingWaypoint == null && transform.childCount > 0)
            {
                var objToSelect =  transform.GetChild(index > 0 ? index -1 : 0).gameObject;
                var s = new SerializedObject(this);
                s.FindProperty("_startingWaypoint").objectReferenceValue = objToSelect;
                s.ApplyModifiedProperties();
                Selection.activeObject = objToSelect;
            }
            //nominal case, just get the previous or the first one
            else
            {
                var objToSelect =  transform.GetChild(index > 0 ? index -1 : 0).gameObject;
                Selection.activeObject = objToSelect;
            }
        }
        
        internal void AddNewWayPoint()
        {
            //creates a new waypoint
            GameObject BuildWp()
            {
                var o = new GameObject();
                o.name = "Waypoint";
                var wp = o.AddComponent<Waypoint>();
                //inits it with the system default
                InitDefaultValue(wp);
                return o;
            }

            GameObject obj;
            if (_startingWaypoint == null)
            {
                //if there is no starting wp
                //inits it and set it as the first one. 
                //might happen if you destroyed it.
                obj = BuildWp();
                obj.transform.parent = this.transform;
                obj.transform.position = this.transform.position;
                Selection.activeObject = obj;
                var s = new SerializedObject(this);
                s.FindProperty("_startingWaypoint").objectReferenceValue = obj;
                s.ApplyModifiedProperties();
                obj.transform.SetSiblingIndex(0);
                
                Undo.RegisterCreatedObjectUndo(obj, "Created waypoints");
            }
            else
            {
                //just create it and set it to the last position
                var lastWp = transform.GetChild(transform.childCount -1);
                obj = BuildWp();
                obj.transform.parent = transform;
                obj.transform.position = lastWp.transform.position;
                Selection.activeObject = obj;
            }
            
            //to undo the creation
            Undo.RegisterCreatedObjectUndo(obj, "Created waypoints");
        }

        internal void AddNewWayPoint(int index)
        {
            //create the waypoint as seen upper
            AddNewWayPoint();
            
            var objToSelect = transform.GetChild(transform.childCount - 1);
            //if we added one at the end, just put it at the last
            if (transform.childCount - 1 == index)
            {
                objToSelect.transform.position = (transform.GetChild(index - 1).transform.position);
            }
            //else set the position in between.
            else
            {
                objToSelect.transform.position =
                    (transform.GetChild(index).transform.position + transform.GetChild(index - 1).transform.position) / 2;
            }
            
            //place it at the position
            objToSelect.SetSiblingIndex(index);
            Selection.activeObject = objToSelect.gameObject;
        }
        
        internal void DestroySystem()
        {
            //due to unity undo serialization, upon undo, we will have lost some refs
            //so, do not use in play mode...
            //I could maintain a scripable object with ref etc.. 
            //But that's a lot of work for a single use case
            Undo.DestroyObjectImmediate(this.gameObject);
        }
        
        /*
         * Resets all the waypoint to use the system default
         */
        internal void ResetWaypointToDefaultsFromSystem()
        {
            Undo.RegisterFullObjectHierarchyUndo(this.gameObject, "Resetted waypoints to system default");
            foreach (Transform t in transform)
            {
                InitDefaultValue(t.GetComponent<Waypoint>());
            }
        }
        
        /**
         * Reset all to use the values from WaypointSettings
         */
        internal void ResetSystemToDefault()
        {
            Undo.RegisterFullObjectHierarchyUndo(this.gameObject, "Resetted System to scriptable object default");
            InitDefaultValueFromScriptable();
        }

        internal void SetLoopType(LoopType loopType)
        {
            Undo.RegisterFullObjectHierarchyUndo(this.gameObject, "changed link loop value");
            _loopType = loopType;
        }
    }
}
#endif