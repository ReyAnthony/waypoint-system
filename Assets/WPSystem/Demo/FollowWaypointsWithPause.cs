using System.Collections.Generic;
using UnityEngine;
using WPSystem.Runtime;

namespace WPSystem.Demo
{
    /**
     * Basic class to show how the tool works.
     */
    public class FollowWaypointsWithPause : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Runtime.WaypointSystem _waypointSystem;
        
        [Header("Event")] 
        [SerializeField] private List<Waypoint> _waypointsWhereWePause;
        [SerializeField] private float _pauseTime;
#pragma warning restore 0649
        
        private Transform _dest;
        private bool _delayNextMove;
        private float _timer;

        private void Start()
        {
            var state = _waypointSystem.RegisterNewPathStateInstance(this.transform);
            
            state.SusbscribeOnWaypointReached((current, next) =>
            {
                if (_waypointsWhereWePause.Count != 0 && _waypointsWhereWePause != null && _waypointsWhereWePause.Contains(current))
                {
                    _delayNextMove = true;
                    _timer = 0;
                }
                _dest = next.transform;
            });
            
            state.SubscribeOnLastWaypointReached(() =>
            {
                _waypointSystem.UnRegisterPathState(this.transform);
                _dest = null;
            });
            
            _dest = state.CurrentWaypoint.transform;
        }

        private void Update()
        {
            if (_dest == null) return;
            
            if (_delayNextMove)
            {
                _timer += Time.deltaTime;
                if (_timer > _pauseTime)
                {
                    _delayNextMove = false;
                }
                return;
            }
            
            transform.LookAt(_dest);
            transform.position = (transform.position + transform.forward * Time.deltaTime * 5);
        }
    }
}