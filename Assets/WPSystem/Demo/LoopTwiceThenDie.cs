using UnityEngine;
using WPSystem.Runtime;

namespace WPSystem.Demo
{
    /**
     * Basic class to show how the tool works.
     */
    public class LoopTwiceThenDie : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Runtime.WaypointSystem _waypointSystem;
        [SerializeField] private int _maxLoop;
#pragma warning restore 0649
        
        private Transform _dest;
        private int _loop;

        private void Start()
        {
            var state = _waypointSystem.RegisterNewPathStateInstance(this.transform, LoopType.Loop, false);
            
            state.SusbscribeOnWaypointReached((current, next) =>
            {
                if (current == _waypointSystem.LastWayPoint)
                {
                    _loop++;
                    if (_loop >= _maxLoop)
                    {
                        _waypointSystem.UnRegisterPathState(transform);
                        Destroy(this.gameObject);
                    }
                }
                _dest = next.transform;
            });
            
            _dest = state.CurrentWaypoint.transform;
        }

        private void Update()
        {
            if (_dest == null) return;
            
            transform.LookAt(_dest);
            transform.position = (transform.position + transform.forward * Time.deltaTime * 5);
        }
    }
}