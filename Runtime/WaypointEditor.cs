#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace WPSystem.Runtime
{
    [ExecuteAlways]
    /*
     * Second part of the waypoint class, it's managing editor only code.
     * Like drawing the gizmos and sticking on the terrain / navmesh
     */
    public partial class Waypoint
    {
        
        private void OnDrawGizmos()
        {
            if (_system == null)
            {
                _system = transform.parent.GetComponent<WaypointSystem>();
            }
            
            Gizmos.color = _system.WaypointsColor;
            Gizmos.DrawSphere(this.transform.position, 0.4f);
            if (WaypointSettings.GetInstance().ShowTheDistanceToReachFGizmos)
            {
                Gizmos.DrawWireSphere(this.transform.position, this.DistanceToReach);
            }

            if (this.NextWaypoint != null)
            {
                var position = this.transform.position;
                var nextPos = this.NextWaypoint.transform.position;
                var dir = (nextPos - position);
                
                Gizmos.color = _system.PathColor;
                Gizmos.DrawLine(position, nextPos);
                Gizmos.DrawWireMesh(Resources.Load<MeshFilter>("direction_arrow").sharedMesh, 
                    (nextPos + position) / 2 , 
                    (dir == Vector3.zero) 
                        ? Quaternion.identity 
                        : Quaternion.LookRotation(dir).normalized, Vector3.one / 3);
                
                if (_system.LoopType == LoopType.PingPong)
                {
                    Gizmos.DrawWireMesh(Resources.Load<MeshFilter>("direction_arrow").sharedMesh, 
                        (nextPos + position) / 2 , 
                        (dir == Vector3.zero) 
                            ? Quaternion.identity 
                            : Quaternion.LookRotation(dir * -1).normalized, Vector3.one / 3);
                }
            }
        }
        
        private void Update()
        {
            if (!Application.isEditor || Application.isPlaying) return;
            
            switch (_StickMode)
            {
                case StickMode.StickOnNavMesh:
                {
                    NavMeshHit hit;
                    NavMesh.SamplePosition(transform.position, out hit, 3, NavMesh.AllAreas);
                    var pos1 = transform.position;
                    pos1.y = hit.position.y;
                    transform.position = pos1;
                    return;
                }

                case StickMode.StickOnTerrain:
                {
                    if (Terrain.activeTerrain == null || PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        return;
                    }
                    
                    var pos = transform.position;
                    pos.y = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
                    transform.position = pos;
                    return;
                }

                case StickMode.DontStick:
                {
                    return;
                }
            }
        }
    }
}
#endif