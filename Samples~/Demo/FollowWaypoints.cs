using UnityEngine;
using WPSystem.Runtime;

namespace WPSystem.Demo
{
    /**
     * Basic class to show how the tool works.
     */
    public class FollowWaypoints : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Runtime.WaypointSystem _waypointSystem;
        
        [Header("Enforce new values at register")] 
        [SerializeField] private bool _enforce; 
        [SerializeField] private bool _startAtNeareast; 
        [SerializeField] private LoopType _loopType; 
#pragma warning restore 0649
        
        private Transform _dest;
        
        private void Start()
        {
            WaypointPathState state;
            
            if (!_enforce)
            {
                //Here we ask the referenced waypointSystem to give us an instance of the path State
                //This instance is keeping track of the position of the player and will send events
                state = _waypointSystem.RegisterNewPathStateInstance(this.transform);
            }
            //optionally, you might want to subscribe with an override
            //This will not affect the Waypoint System, but only the subscriber !
            //So, keep in mind that the path that you see in the Editor will not match. 
            else
            {
                state = _waypointSystem.RegisterNewPathStateInstance(this.transform, _loopType, _startAtNeareast);
            }

            //this event is sent everytime a waypoint is reached.
            state.SusbscribeOnWaypointReached((current, next) =>
            {
                _dest = next.transform;
            });
            
            //This event is called when the tracked transform has reached the end of the path
            //It will only be called when using a one-way path.
            state.SubscribeOnLastWaypointReached(() =>
            {
                //You might want to unregister the state to avoid unecessary calculations.
                //You can unregister it at any time if you want to stop tracking the transform
                //If you call this, no event will be triggered anymore
                //If you want to delete / add nodes at runtime (which I don't do or recommend), you should call this before !
                _waypointSystem.UnRegisterPathState(this.transform);
                _dest = null;
            });

            //setting the current destination
            //keep in mind that the 2 previous subscriptions are EVENTS, which mean they will be called later.
            
            //DO NOT set it to _waypointSystem.StartingWaypoint.
            //If the startAtTheNearest attribute is true, the starting waypoint is not necessarily the first one. 
            //You need to work with the WayPointPathState class.
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