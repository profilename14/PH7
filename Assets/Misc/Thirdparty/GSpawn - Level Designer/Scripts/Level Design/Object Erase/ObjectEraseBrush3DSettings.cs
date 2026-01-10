#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace GSPAWN
{
    [Flags]
    public enum ObjectEraseSurfaceTypes
    {
        None = 0,
        Grid = 1,
        Meshes = 2,
        Terrains = 4,
        All = ~0
    }

    public class ObjectEraseBrush3DSettings : PluginSettings<ObjectEraseBrush3DSettings>
    {
        [SerializeField]
        private bool                            _allowPartialOverlap        = defaultAllowPartialOverlap;
        [SerializeField]
        private float                           _radius                     = defaultRadius;
        [SerializeField]
        private float                           _eraseHeight                = defaultEraseHeight;
        [SerializeField]
        private float                           _bumpHeight                 = defaultBumpHeight;
        [SerializeField]
        private ObjectEraseSurfaceTypes         _surfaceTypes               = defaultSurfaceTypes;
        [SerializeField]
        private int                             _surfaceLayers              = defaultSurfaceLayers;

        public bool                             allowPartialOverlap         { get { return _allowPartialOverlap; } set { UndoEx.record(this); _allowPartialOverlap = value; EditorUtility.SetDirty(this); } }
        public float                            radius                      { get { return _radius; } set { UndoEx.record(this); _radius = Mathf.Max(0.1f, value); EditorUtility.SetDirty(this); } }
        public float                            eraseHeight                 { get { return _eraseHeight; } set { UndoEx.record(this); _eraseHeight = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public float                            bumpHeight                  { get { return _bumpHeight; } set { UndoEx.record(this); _bumpHeight = Mathf.Max(0.0f, value); EditorUtility.SetDirty(this); } }
        public ObjectEraseSurfaceTypes          surfaceTypes                { get { return _surfaceTypes; } set { UndoEx.record(this); _surfaceTypes = value; EditorUtility.SetDirty(this); } }
        public bool                             allowsGridSurface           { get { return (surfaceTypes & ObjectEraseSurfaceTypes.Grid) != 0; } }
        public bool                             allowsMeshSurface           { get { return (surfaceTypes & ObjectEraseSurfaceTypes.Meshes) != 0; } }
        public bool                             allowsTerrainSurface        { get { return (surfaceTypes & ObjectEraseSurfaceTypes.Terrains) != 0; } }
        public int                              surfaceLayers               { get { return _surfaceLayers; } set { UndoEx.record(this); _surfaceLayers = value; EditorUtility.SetDirty(this); } }

        public static bool                      defaultAllowPartialOverlap  { get { return true; } }
        public static float                     defaultRadius               { get { return 1.0f; } }
        public static float                     defaultEraseHeight          { get { return 1e-1f; } }
        public static float                     defaultBumpHeight           { get { return 0.05f; } }
        public static ObjectEraseSurfaceTypes   defaultSurfaceTypes         { get { return ObjectEraseSurfaceTypes.All; } }
        public static int                       defaultSurfaceLayers        { get { return ~0; } }

        public void buildUI(VisualElement parent)
        {
            UI.createToggle("_allowPartialOverlap", serializedObject, "Allow partial overlap", "If this is checked, objects will be erased even if they are not completely overlapped by the brush. " +
                "When unchecked, only objects that are completely overlapped by the brush will be erased.", parent);
            UI.createFloatField("_radius", serializedObject, "Radius", "The 3D brush radius.", 0.1f, parent);
            UI.createFloatField("_eraseHeight", serializedObject, "Erase height", "Controls how high above the surface objects are allowed to sit in order to be erased with the brush. Increasing this value will essentially turn the circle into a cylinder.", 0.0f, parent);
            UI.createFloatField("_bumpHeight", serializedObject, "Bump height", "Controls how sensitive the brush is to bumps in the surface. This is useful when using the brush to erase debris sitting on " + 
                "the floor. If the floor had bumps the brush would erase the floor along with the debris. However, this value informs the brush that the surface on which it is sitting is bumpy. " + 
                "This reduces the risk of the brush erasing the objects which sit right underneath it.", 0.0f, parent);
            UI.createEnumFlagsField(typeof(ObjectEraseSurfaceTypes), "_surfaceTypes", serializedObject, "Surface types", "Allows you to specify the types of entities that can be used as surfaces on which the 3D brush can sit.", parent);
            UI.createLayerMaskField(surfaceLayers, "_surfaceLayers", serializedObject, "Surface layers", "Only objects belonging to any of these layers can be used as a surface for the 3D brush.", parent);

            UI.createUseDefaultsButton(() => useDefaults(), parent);
        }

        public override void useDefaults()
        {
            allowPartialOverlap     = defaultAllowPartialOverlap;
            radius                  = defaultRadius;
            eraseHeight             = defaultEraseHeight;
            bumpHeight              = defaultBumpHeight;
            surfaceTypes            = defaultSurfaceTypes;
            surfaceLayers           = defaultSurfaceLayers;

            EditorUtility.SetDirty(this);
        }
    }
}
#endif