using UnityEngine;
using WPSystem.Runtime;

namespace WPSystem.Demo
{
    /**
     * Basic class to show how the tool works.
     */
    public class ChangeLoopTypeAtRuntime : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Runtime.WaypointSystem _waypointSystem;
        [SerializeField] private LoopType _loopType;
#pragma warning restore 0649
        
        private Transform _dest;
        private int _loop;
        private WaypointPathState _state;
        private LoopType _initialLoopType;

        private void Start()
        {
            _initialLoopType = _loopType;
            _state = _waypointSystem.RegisterNewPathStateInstance(transform, _loopType, false);
            
            _state.SusbscribeOnWaypointReached((current, next) =>
            {
                _dest = next.transform;
            });
            
            _dest = _state.CurrentWaypoint.transform;
        }

        private void Update()
        {
            if (_dest == null) return;

            if (_loopType != _initialLoopType)
            {
                _initialLoopType = _loopType;
                RegisterToNewPathState();
            }
            
            transform.LookAt(_dest);
            transform.position = (transform.position + transform.forward * Time.deltaTime * 5);
        }

        private void RegisterToNewPathState()
        {
            //we need to unregister 
            _waypointSystem.UnRegisterPathState(this.transform);
            //then we create a new path state !
            _state = _waypointSystem.RegisterNewPathStateInstance(this.transform, _loopType, true);
            //recreate the events
            _state.SusbscribeOnWaypointReached((current, next) =>
            {
                _dest = next.transform;
            });
            //just in case it changed (ping pong ?)
            _dest = _state.CurrentWaypoint.transform;
        }
    }
}