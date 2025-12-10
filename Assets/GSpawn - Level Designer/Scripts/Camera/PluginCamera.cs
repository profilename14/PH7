#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace GSPAWN
{
    public class PluginCamera
    {
        public static Camera camera { get { return SceneView.lastActiveSceneView.camera; } }
    }
}
#endif