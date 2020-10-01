using System;
using System.Collections.Generic;
using UnityEngine;

namespace WPSystem.Runtime
{
    /**
     * This class is managing the whole system.
     * It's splitted in two part.
     */
    public partial class WaypointSystem : MonoBehaviour
    {
        private Dictionary<Transform, WaypointPathState> _states;
        private List<(Transform, Action)> _delayedUnregister;

        public Waypoint StartingWaypoint => _startingWaypoint.GetComponent<Waypoint>();
        public Waypoint LastWayPoint => this.transform.GetChild(this.transform.childCount - 1).GetComponent<Waypoint>();
        public Color WaypointsColor => _waypointsColor;
        
#pragma warning disable 0649
        [SerializeField, HideInInspector] private GameObject _startingWaypoint;
        
        [Header("Changing these will not impact already registered pathStates")]
        [SerializeField, Tooltip("One-way : Go to finish then stop.\n" +
                                 "Loop : Go the end then loop to the start\n" +
                                 "Ping-pong : Go to the end then go back in the opposite direction")] 
        private LoopType _loopType;
        
        [Header("Changing these will impact the whole system")]

        [SerializeField, Tooltip("Color for the waypoints")] private Color _waypointsColor;
        [SerializeField, Tooltip("Should you start at the starting waypoint ? Or the nearest ?")] 
        private bool _startAtTheNearest;
        
        [Header("Defaults, changing them will not change already spawned waypoints"), Space(25)]
        [SerializeField, Tooltip("Used to determine what the waypoints should stick on.")] 
        private StickMode _defaultStickMode;
        
        [SerializeField, Tooltip("Used to determine at which distance we should consider that we reached a waypoint")] 
        private float _defaultDistanceToReach;
#pragma warning restore 0649
        
        public LoopType LoopType
        {
            get => _loopType;
            set => _loopType = value;
        }

        public bool StartAtTheNearest
        {
            get => _startAtTheNearest;
            set => _startAtTheNearest = value;
        }
        
        //Call this to start using the system
        public WaypointPathState RegisterNewPathStateInstance(Transform t)
        {
            CreateStatesIfNull();
            
            //if we register right after an unregsiter, we need to clean the delayed unregister
            //or else it will be deleted right after register
            ClearDelayedUnregsiterIfNeeded(t);
            
            //WaypointPathState will track the position of the transform and send events
            _states[t] = new WaypointPathState(t, _startingWaypoint.GetComponent<Waypoint>(), _loopType, _startAtTheNearest, this);
            return _states[t];
        }
        
        //Call this to start using the system
        public WaypointPathState RegisterNewPathStateInstance(Transform t, LoopType overridenLoopType, bool overridenNearest)
        {
            CreateStatesIfNull();
            ClearDelayedUnregsiterIfNeeded(t);
            
            _states[t] = new WaypointPathState(t, _startingWaypoint.GetComponent<Waypoint>(), overridenLoopType, overridenNearest, this);
            return _states[t];
        }

        private void ClearDelayedUnregsiterIfNeeded(Transform t)
        {
            if (_delayedUnregister != null && _delayedUnregister.Exists(tuple => tuple.Item1 == t))
            {
                _delayedUnregister?.Clear();
            }
        }
        private void CreateStatesIfNull()
        {
            if (_states == null)
            {
                _states = new Dictionary<Transform, WaypointPathState>();
            }
        }

        //Call this to stop sending events for the state
        public void UnRegisterPathState(Transform t)
        {
            if (_states.ContainsKey(t))
            {
                if (_delayedUnregister == null)
                {
                    _delayedUnregister = new List<(Transform, Action)>();
                }
                
                //we need to delay the unregister to avoid modifying collection
                //(unregister pathstate is usually called from the SubscribeOnLastWaypointReached event)
                _delayedUnregister.Add((t, () => { _states.Remove(t); }));
            }
        }
        
        private void Update()
        {
            if (_states == null) return;
            
            //iterate over all states registered for this system and update them
            foreach (var waypointPathState in _states)
            {
                //ignore the unregistered
                if (_delayedUnregister != null && 
                    _delayedUnregister.Exists((t) => t.Item1 == waypointPathState.Key))
                {
                    continue;
                }
                waypointPathState.Value.Update();
            }

            //unregister the unregistered states
            _delayedUnregister?.ForEach(d => d.Item2());
            _delayedUnregister?.Clear();
        }
    }
}