using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WPSystem.Runtime
{
    public enum UIPosition
    {
        Top, 
        Down
    }
    
    [CreateAssetMenu]
#if UNITY_EDITOR
    [InitializeOnLoad] //allow to call the static constructor
#endif
    /**
     * This class is used to keep track of the default settings.
     * It is loaded at the start of the Unity editor and on domain reload. 
     */
    public class WaypointSettings : ScriptableObject
    {
        private static WaypointSettings _instance;
        
#pragma warning disable 0649
        [Header("Default settings used when spawning a new waypoint system")]
        [SerializeField, Tooltip("Used to determine what the waypoints should stick on.")] 
        private StickMode _defaultStickMode;
        
        [SerializeField, Tooltip("Used to determine at which distance we should consider that we reached a waypoint")] 
        private float _defaultDistanceToReach = 0.5f;
        
        [SerializeField, Tooltip("Default color for the waypoints")] 
        private Color _waypointsColor = Color.blue;
        
        [SerializeField, Tooltip("Default color for the path")] 
        private Color _pathColor = Color.black;
        
        [SerializeField, Tooltip("If set to true, the first waypoint will be the nearest, else the first one (the flag)")] 
        private bool _startAtTheNearest;
        
        [SerializeField, Tooltip("One-way : Go to finish then stop.\n" +
                                 "Loop : Go the end then loop to the start\n" +
                                 "Ping-pong : Go to the end then go back in the opposite direction")] 
        private LoopType _defaultLoopType;
        
        [Header("Global settings")] 
        [SerializeField,  Tooltip("Position of the UI")] private UIPosition _uiPosition;
        [SerializeField]
        private bool _showTheDistanceToReachGizmos;

#pragma warning restore 0649
        
        public StickMode DefaultStickMode => _defaultStickMode;
        public float DefaultDistanceToReach => _defaultDistanceToReach;
        public Color WaypointsColor => _waypointsColor;
        public Color PathColor => _pathColor;
        public bool StartAtTheNearest => _startAtTheNearest;
        public LoopType DefaultLoopType => _defaultLoopType;
        public UIPosition UiPosition => _uiPosition;
        public bool ShowTheDistanceToReachFGizmos => _showTheDistanceToReachGizmos;

#if UNITY_EDITOR
        static WaypointSettings()
        {
            //we want to call LoadAll AFTER assembly reload or else it will fail with an error
            AssemblyReloadEvents.afterAssemblyReload += () => { GetInstance(); };
        }

        public static WaypointSettings GetInstance()
        {
            if (!_instance)
            {
                try
                {
                    //gets the Scriptable object instance asset in the Assets folder.
                    _instance = Resources.LoadAll<WaypointSettings>("").First();
                }
                catch (Exception)
                {
                    Debug.LogError("No WaypointSettings !");
                }
            }

            return _instance;
        }
#endif
    }
}
