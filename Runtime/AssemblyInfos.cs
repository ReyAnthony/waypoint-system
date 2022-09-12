using System.Runtime.CompilerServices;

//this allows to show internals to the editor scripts, but to hide them to the user as they should not use them.
#if UNITY_EDITOR
[assembly: InternalsVisibleTo("WaypointSystem-editor")]
#endif
