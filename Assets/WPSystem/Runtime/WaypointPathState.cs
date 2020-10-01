using UnityEngine;

namespace WPSystem.Runtime
{
    public class WaypointPathState
    {
        //The actual starting waypoint -will not change)
        private readonly LoopType _loopType;
        private readonly WaypointSystem _system;

        //THe current target waypoint
        private Waypoint _currentWaypoint;
        
        //the transform to follow
        private Transform _transform;
        
        //if looptype is ping-pong
        //if true, we are going backward on the waypoints
        private bool _isPong;

        public Waypoint CurrentWaypoint => _currentWaypoint;
        public delegate void OnWaypointReached(Waypoint current, Waypoint next);
        public delegate void OnLastWaypointReached();
        private event OnWaypointReached _wayPointReachedSubscribers;
        private event OnLastWaypointReached _lastWaypointReachedSubscribers;
        private Waypoint StartingWaypoint => _system.StartingWaypoint;
        
        public WaypointPathState(Transform toFollow, Waypoint startingWaypoint, LoopType loopType, bool startAtTheNearest, WaypointSystem system)
        {
            _loopType = loopType;
            _system = system;
            _transform = toFollow;

            if (startAtTheNearest)
            {
                SetTheClosestAsCurrent();
            }
            else
            {
                _currentWaypoint = startingWaypoint;
            }
        }
        
        public void SusbscribeOnWaypointReached(OnWaypointReached onReached)
        {
            _wayPointReachedSubscribers += onReached;
        }

        public void SubscribeOnLastWaypointReached(OnLastWaypointReached onReached)
        {
            _lastWaypointReachedSubscribers += onReached;
        }
        
        private void SetTheClosestAsCurrent()
        {
            //iterate over all waypoints and select the closest
            var iterator = StartingWaypoint;
            var closest = iterator;
            while(iterator != null)
            {
                var currentDist = Mathf.Abs(Vector3.Distance(_transform.position, iterator.transform.position));
                var closestDist = Mathf.Abs(Vector3.Distance(_transform.position, closest.transform.position));
                if(currentDist < closestDist)
                {
                    closest = iterator;
                }

                iterator = iterator.NextWaypoint;
            }

            _currentWaypoint = closest;
        }

        public void Update()
        {
            if (_transform == null) return;
            
            //did we delete the current at runtime ??
            if (_currentWaypoint == null)
            {
                //then set the closest as the new current
                SetTheClosestAsCurrent();
                //notify the listeners -- maybe a bit hackish ? Need a new type of event ?
                _wayPointReachedSubscribers?.Invoke(_currentWaypoint, _currentWaypoint);
            }

            var old = _currentWaypoint;
            
            //iterate backwards if pingpong
            if (_loopType == LoopType.PingPong && _isPong)
            {
                //if reached the next one (but not the first one !)
                if (_currentWaypoint.HasBeenReached(_transform.position) && _currentWaypoint.PreviousWaypoint != null)
                {
                    //set the previous as current
                    _currentWaypoint = _currentWaypoint.PreviousWaypoint;
                    _wayPointReachedSubscribers?.Invoke(old, _currentWaypoint);
                    return;
                }
            
                //has reached the first one
                if (_currentWaypoint.HasBeenReached(_transform.position) && _currentWaypoint.PreviousWaypoint == null)
                {
                    //stop ponging
                    _isPong = false;
                    _currentWaypoint = StartingWaypoint.NextWaypoint;
                    _wayPointReachedSubscribers?.Invoke(old, _currentWaypoint);
                }
            }
            //otherwize if one-way or loop then iterate normally
            else
            {
                //reached the next one (but not the last one !)
                if (_currentWaypoint.HasBeenReached(_transform.position) && _currentWaypoint.NextWaypoint != null)
                {
                    _currentWaypoint = _currentWaypoint.NextWaypoint;
                    _wayPointReachedSubscribers?.Invoke(old, _currentWaypoint);
                    return;
                }
            
                //has reached the end
                if (_currentWaypoint.HasBeenReached(_transform.position) && CurrentWaypoint.NextWaypoint == null)
                {
                    switch (_loopType)
                    {
                        case LoopType.Loop:
                            _currentWaypoint = StartingWaypoint;
                            _wayPointReachedSubscribers?.Invoke(old, CurrentWaypoint);
                            break;
                        case LoopType.OneWay:
                            _transform = null;
                            _lastWaypointReachedSubscribers?.Invoke();
                            break;
                        case LoopType.PingPong:
                            _currentWaypoint = _currentWaypoint.PreviousWaypoint;
                            _wayPointReachedSubscribers?.Invoke(old, CurrentWaypoint);
                            _isPong = true;
                            break;
                    }
                }
            }
        }

        /**
         * Used for debug purpose to make it end faster.
         * (eg. I use it to skip cutscenes while I debug my game.
         */
        public void InstantFinish()
        {
            _transform = null;
            _lastWaypointReachedSubscribers?.Invoke();
        }
    }
}