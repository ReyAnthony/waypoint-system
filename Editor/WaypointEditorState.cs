﻿using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WPSystem.Editor
{
    /*
     * Keeps the state of the editorWindow
     * If is _isActive is true, we will show the plugin in the view
     */
    [CreateAssetMenu(menuName = "Waypoints Editor/WaypointEditorState")]
    [InitializeOnLoad]
    public class WaypointEditorState : ScriptableObject
    {
        //should we use the plugin ?
        [SerializeField, HideInInspector] private bool _isActive;
        private static WaypointEditorState _instance;
        
       //at each restart of the editor, it will enable the system (or not)
        static WaypointEditorState()
        {
            //we want to call LoadAll AFTER assembly reload or else it will fail with an error
            AssemblyReloadEvents.afterAssemblyReload += () => { GetInstance(); };
        }  
        
        [MenuItem("Tools/Waypoints/Enable")]
        public static void Enable()
        {
            GetInstance()._isActive = true;
            EditorUtility.SetDirty(GetInstance());
            WaypointEditorWindow.Enable();
        }
        
        [MenuItem("Tools/Waypoints/Disable")]
        public static void Disable()
        {
            GetInstance()._isActive = false;
            EditorUtility.SetDirty(GetInstance());
            WaypointEditorWindow.Disable();
        }
        
        private static WaypointEditorState GetInstance()
        {
            if (!_instance)
            {
                try
                {
                    //gets the Scriptable object instance asset in the Assets folder.
                    _instance = Resources.LoadAll<WaypointEditorState>("").First();
                }
                catch (Exception)
                {
                    Debug.LogError("No WaypointEditorState !");
                }
            }
            
            if (_instance._isActive)
            {
                WaypointEditorWindow.Enable();
            }
 
            return _instance;
        }
    }
}