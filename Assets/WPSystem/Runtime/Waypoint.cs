using System;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace WPSystem.Runtime
{
    /*
     * This class represents a waypoint, it's split in 2 parts, a runtime one and an editor one.
     */
    public partial class Waypoint : MonoBehaviour
    {
        private Waypoint _next;
        [SerializeField] private StickMode _StickMode;
        [SerializeField] private float _distanceToReach;
        private WaypointSystem _system;

        public StickMode StickMode
        {
            set => _StickMode = value;
        }
        
        public Waypoint NextWaypoint
        {
            get
            {
                try
                {
                    return transform.parent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<Waypoint>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        
        public Waypoint PreviousWaypoint
        {
            get
            {
                try
                {
                    return transform.parent.GetChild(transform.GetSiblingIndex() - 1).GetComponent<Waypoint>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public float DistanceToReach
        {
            get => _distanceToReach;
            set => _distanceToReach = value;
        }
        
        private void Start()
        {
            if (this.transform.parent == null || this.transform.parent.GetComponent<WaypointSystem>()  == null)
            {
                throw new UnityException("Please use the editor to spawn waypoints");
            }
        }
        
        public bool HasBeenReached(Vector3 positionToTest)
        {
            return Vector3.Distance( transform.position, positionToTest) < _distanceToReach;
        }
    }
}