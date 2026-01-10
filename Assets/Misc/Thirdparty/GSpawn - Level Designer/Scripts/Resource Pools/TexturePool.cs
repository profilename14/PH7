#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

namespace GSPAWN
{
    public class TexturePool : Singleton<TexturePool>
    {
        Texture2D _objectEraseCursor;
        Texture2D _objectEraseBrush2D;
        Texture2D _objectEraseBrush3D;

        Texture2D _modularSnapSpawn;
        Texture2D _modularWallSpawn;
        Texture2D _segmentsSpawn;
        Texture2D _boxSpawn;
        Texture2D _propsSpawn;
        Texture2D _scatterBrushSpawn;
        Texture2D _tileRuleBrushSpawn;
        Texture2D _curveSpawn;
        Texture2D _physicsSimulation;

        Texture2D _moveGizmo;
        Texture2D _rotationGizmo;
        Texture2D _scaleGizmo;
        Texture2D _universalGizmo;
        Texture2D _extrudeGizmo;
        Texture2D _mirrorGizmo;
        Texture2D _gizmoMeshPivot;
        Texture2D _gizmoCenterPivot;
        Texture2D _gizmoGlobalTransformSpace;
        Texture2D _gizmoLocalTransformSpace;

        Texture2D _selectionRect;
        Texture2D _selectionSegments;
        Texture2D _selectionBox;

        Texture2D _earthHammer;
        Texture2D _earthHand;
        Texture2D _earthDelete;
        Texture2D _warning;

        Texture2D _hotkeys;
        Texture2D _questionMark;
        Texture2D _lightGizmo;
        Texture2D _particleSystemGizmo;
        Texture2D _cameraGizmo;

        Texture2D _visible;

        Texture2D _grid;
        Texture2D _camera;
        Texture2D _prefab;
        Texture2D _white;
        Texture2D _refresh;
        Texture2D _libraryDb;

        Texture2D _scissors;
        Texture2D _location;
        Texture2D _itemArrowRight;
        Texture2D _itemArrowUp;
        Texture2D _itemArrowTop;
        Texture2D _itemArrowDown;
        Texture2D _itemArrowBottom;
        Texture2D _moveUp;
        Texture2D _moveDown;
        Texture2D _moveTop;
        Texture2D _moveBottom;

        Texture2D _dragArrow;
        Texture2D _transform;
        Texture2D _settings;
        Texture2D _projection;
        Texture2D _layers;
        Texture2D _greenSphere;
        Texture2D _terrain;
        Texture2D _eraser;
        Texture2D _eraser_gray;
        Texture2D _hand;
        Texture2D _handNo;
        Texture2D _lightBulb;
        Texture2D _lightBulbGray;
        Texture2D _delete;
        Texture2D _clear;
        Texture2D _vertexSnap;
        Texture2D _boxSnap;
        Texture2D _objectSurfaceSnap;
        Texture2D _ping;
        Texture2D _objectGroup;
        Texture2D _objectGroupRotated;
        Texture2D _defaultObjectGroup;
        Texture2D _objectGroupDelete;
        Texture2D _objectGroupHand;
        Texture2D _intPattern;
        Texture2D _terrainFlatten;
        Texture2D _chemistry;
        Texture2D _createAddNew;
        Texture2D _sync;
        Texture2D _misc;
        Texture2D _selectionGrow;
        Texture2D _scenePicking_notPickable;
        Texture2D _brush;
        Texture2D _tileGrid;
        Texture2D _filter;
        Texture2D _ramp;
        Texture2D _tileNeighborRadius;
        Texture2D _autoRefresh;
        Texture2D _defaultObject;
        Texture2D _tileRuleSpawnFlexiBoxBrush;
        Texture2D _tileRuleSpawnConnect;
        Texture2D _fixOverlaps;
        Texture2D _load;
        Texture2D _decor;
        Texture2D _tag;
        Texture2D _pin;
        Texture2D _pin_Disabled;
        Texture2D _ruler;
        Texture2D _overlap;
        Texture2D _overlap_Off;

        Texture2D _wallRule_Straight;
        Texture2D _wallRule_InnerCorner;
        Texture2D _wallRule_OuterCorner;

        public Texture2D objectEraseCursor
        {
            get
            {
                if (_objectEraseCursor == null) _objectEraseCursor = loadUITexture("ObjectEraseCursor_24");
                return _objectEraseCursor;
            }
        }
        public Texture2D objectEraseBrush2D
        {
            get
            {
                if (_objectEraseBrush2D == null) _objectEraseBrush2D = loadUITexture("ObjectEraseBrush2D_24");
                return _objectEraseBrush2D;
            }
        }
        public Texture2D objectEraseBrush3D
        {
            get
            {
                if (_objectEraseBrush3D == null) _objectEraseBrush3D = loadUITexture("ObjectEraseBrush3D_24");
                return _objectEraseBrush3D;
            }
        }
        public Texture2D modularSnapSpawn
        {
            get
            {
                if (_modularSnapSpawn == null)
                {
                    if (UIValues.isProSkin) _modularSnapSpawn = loadSystemIconTexture("d_SceneViewSnap@2x");
                    else _modularSnapSpawn = loadSystemIconTexture("SceneViewSnap@2x");
                }
                return _modularSnapSpawn;
            }
        }
        public Texture2D modularWallSpawn
        {
            get
            {
                if (_modularWallSpawn == null)
                {
                    if (UIValues.isProSkin) _modularWallSpawn = loadUITexture("ModularWallSpawn_24");
                    else _modularWallSpawn = loadUITexture("ModularWallSpawn_24");
                }
                return _modularWallSpawn;
            }
        }
        public Texture2D segmentsSpawn
        {
            get
            {
                if (_segmentsSpawn == null) _segmentsSpawn = loadUITexture("SegmentsSpawn_24");
                return _segmentsSpawn;
            }
        }
        public Texture2D boxSpawn
        {
            get
            {
                if (_boxSpawn == null) _boxSpawn = loadUITexture("BoxSpawn_24");
                return _boxSpawn;
            }
        }
        public Texture2D propsSpawn
        {
            get
            {
                if (_propsSpawn == null) _propsSpawn = loadUITexture("PropsSpawn_24");
                return _propsSpawn;
            }
        }
        public Texture2D scatterBrushSpawn
        {
            get
            {
                if (_scatterBrushSpawn == null) _scatterBrushSpawn = loadUITexture("ScatterBrushSpawn_24");
                return _scatterBrushSpawn;
            }
        }
        public Texture2D tileRuleBrushSpawn
        {
            get
            {
                if (_tileRuleBrushSpawn == null) _tileRuleBrushSpawn = loadSystemIconTexture("GridBrush Icon");
                return _tileRuleBrushSpawn;
            }
        }
        public Texture2D curveSpawn
        {
            get
            {
                if (_curveSpawn == null) _curveSpawn = loadUITexture("CurveSpawn_24");
                return _curveSpawn;
            }
        }
        public Texture2D physicsSimulation
        {
            get
            {
                if (_physicsSimulation == null)
                {
                    #if UNITY_6000_0_OR_NEWER
                    if (UIValues.isProSkin) _physicsSimulation = loadSystemIconTexture("d_PhysicsMaterial Icon");
                    else _physicsSimulation = loadSystemIconTexture("PhysicsMaterial Icon");
                    #else
                    if (UIValues.isProSkin) _physicsSimulation = loadSystemIconTexture("d_PhysicMaterial Icon");
                    else _physicsSimulation = loadSystemIconTexture("PhysicMaterial Icon");
                    #endif
                }
                return _physicsSimulation;
            }
        }
        public Texture2D moveGizmo
        {
            get
            {
                if (_moveGizmo == null)
                {
                    if (UIValues.isProSkin) _moveGizmo = loadSystemIconTexture("d_MoveTool");
                    else _moveGizmo = loadSystemIconTexture("MoveTool");
                }
                return _moveGizmo;
            }
        }
        public Texture2D rotationGizmo
        {
            get
            {
                if (_rotationGizmo == null)
                {
                    if (UIValues.isProSkin) _rotationGizmo = loadSystemIconTexture("d_RotateTool");
                    else _rotationGizmo = loadSystemIconTexture("RotateTool");
                }
                return _rotationGizmo;
            }
        }
        public Texture2D scaleGizmo
        {
            get
            {
                if (_scaleGizmo == null)
                {
                    if (UIValues.isProSkin) _scaleGizmo = loadSystemIconTexture("d_ScaleTool");
                    else _scaleGizmo = loadSystemIconTexture("ScaleTool");
                }
                return _scaleGizmo;
            }
        }
        public Texture2D universalGizmo
        {
            get
            {
                if (_universalGizmo == null)
                {
                    if (UIValues.isProSkin) _universalGizmo = loadSystemIconTexture("d_TransformTool");
                    else _universalGizmo = loadSystemIconTexture("TransformTool");
                }
                return _universalGizmo;
            }
        }
        public Texture2D extrudeGizmo
        {
            get
            {
                if (_extrudeGizmo == null)
                {
                    if (UIValues.isProSkin) _extrudeGizmo = loadUITexture("ExtrudeGizmoDark");
                    else _extrudeGizmo = loadUITexture("ExtrudeGizmo");
                }
                return _extrudeGizmo;
            }
        }
        public Texture2D mirrorGizmo
        {
            get
            {
                if (_mirrorGizmo == null)
                {
                    if (UIValues.isProSkin) _mirrorGizmo = loadUITexture("MirrorGizmoDark");
                    else _mirrorGizmo = loadUITexture("MirrorGizmo");
                }
                return _mirrorGizmo;
            }
        }
        public Texture2D gizmoMeshPivot
        {
            get
            {
                if (_gizmoMeshPivot == null)
                {
                    if (UIValues.isProSkin) _gizmoMeshPivot = loadSystemIconTexture("d_ToolHandlePivot");
                    else _gizmoMeshPivot = loadSystemIconTexture("ToolHandlePivot");
                }
                return _gizmoMeshPivot;
            }
        }
        public Texture2D gizmoCenterPivot
        {
            get
            {
                if (_gizmoCenterPivot == null)
                {
                    if (UIValues.isProSkin) _gizmoCenterPivot = loadSystemIconTexture("d_ToolHandleCenter");
                    else _gizmoCenterPivot = loadSystemIconTexture("ToolHandleCenter");
                }
                return _gizmoCenterPivot;
            }
        }
        public Texture2D gizmoGlobalTransformSpace
        {
            get
            {
                if (_gizmoGlobalTransformSpace == null)
                {
                    if (UIValues.isProSkin) _gizmoGlobalTransformSpace = loadSystemIconTexture("d_ToolHandleGlobal");
                    else _gizmoGlobalTransformSpace = loadSystemIconTexture("ToolHandleGlobal");
                }
                return _gizmoGlobalTransformSpace;
            }
        }
        public Texture2D gizmoLocalTransformSpace
        {
            get
            {
                if (_gizmoLocalTransformSpace == null)
                {
                    if (UIValues.isProSkin) _gizmoLocalTransformSpace = loadSystemIconTexture("d_ToolHandleLocal");
                    else _gizmoLocalTransformSpace = loadSystemIconTexture("ToolHandleLocal");
                }
                return _gizmoLocalTransformSpace;
            }
        }
        public Texture2D selectionRect
        {
            get
            {
                if (_selectionRect == null)
                {
                    if (UIValues.isProSkin) _selectionRect = loadUITexture("SelectionRectDark_24");
                    else _selectionRect = loadUITexture("SelectionRect_24");
                }
                return _selectionRect;
            }
        }
        public Texture2D selectionSegments
        {
            get
            {
                if (_selectionSegments == null)
                {
                    if (UIValues.isProSkin) _selectionSegments = loadUITexture("SelectionSegmentsDark_24");
                    else _selectionSegments = loadUITexture("SelectionSegments_24");
                }
                return _selectionSegments;
            }
        }
        public Texture2D selectionBox
        {
            get
            {
                if (_selectionBox == null)
                {
                    if (UIValues.isProSkin) _selectionBox = loadUITexture("SelectionBoxDark_24");
                    else _selectionBox = loadUITexture("SelectionBox_24");
                }
                return _selectionBox;
            }
        }
        public Texture2D earthHammer
        {
            get
            {
                if (_earthHammer == null) _earthHammer = loadUITexture("EarthHammer_24");
                return _earthHammer;
            }
        }
        public Texture2D earthHand
        {
            get
            {
                if (_earthHand == null) _earthHand = loadUITexture("EarthHand_24");
                return _earthHand;
            }
        }
        public Texture2D earthDelete
        {
            get
            {
                if (_earthDelete == null) _earthDelete = loadUITexture("EarthDelete_24");
                return _earthDelete;
            }
        }
        public Texture2D warning
        {
            get
            {
                if (_warning == null) _warning = loadSystemIconTexture("Warning");
                return _warning;
            }
        }
        public Texture2D hotkeys
        {
            get
            {
                if (_hotkeys == null) _hotkeys = loadUITexture("Hotkeys_32");
                return _hotkeys;
            }
        }
        public Texture2D questionMark
        {
            get
            {
                if (_questionMark == null) _questionMark = loadUITexture("QuestionMark_256");
                return _questionMark;
            }
        }
        public Texture2D lightGizmo
        {
            get
            {
                if (_lightGizmo == null) _lightGizmo = loadSystemIconTexture("Main Light Gizmo");
                return _lightGizmo;
            }
        }
        public Texture2D particleSystemGizmo
        {
            get
            {
                if (_particleSystemGizmo == null) _particleSystemGizmo = loadSystemIconTexture("ParticleSystem Gizmo");
                return _particleSystemGizmo;
            }
        }
        public Texture2D cameraGizmo
        {
            get
            {
                if (_cameraGizmo == null) _cameraGizmo = loadSystemIconTexture("Camera Gizmo");
                return _cameraGizmo;
            }
        }
        public Texture2D visible
        {
            get
            {
                if (_visible == null)
                {
                    if (UIValues.isProSkin) _visible = loadSystemIconTexture("d_scenevis_visible_hover");
                    else _visible = loadSystemIconTexture("scenevis_visible_hover");
                }
                return _visible;
            }
        }
        public Texture2D grid
        {
            get
            {
                if (_grid == null)
                {
                    if (UIValues.isProSkin) _grid = loadSystemIconTexture("d_GridView");
                    else _grid = loadSystemIconTexture("GridView");
                }
                return _grid;
            }
        }
        public Texture2D camera
        {
            get
            {
                if (_camera == null)
                {
                    if (UIValues.isProSkin) _camera = loadSystemIconTexture("d_FrameCapture");
                    else _camera = loadSystemIconTexture("FrameCapture");
                }
                return _camera;
            }
        }
        public Texture2D prefab
        {
            get
            {
                if (_prefab == null)
                {
                    if (UIValues.isProSkin) _prefab = loadSystemIconTexture("d_Prefab Icon");
                    else _prefab = loadSystemIconTexture("Prefab Icon");
                }
                return _prefab;
            }
        }
        public Texture2D white
        {
            get
            {
                if (_white == null) _white = loadUITexture("White");
                return _white;
            }
        }
        public Texture2D refresh
        {
            get
            {
                if (_refresh == null)
                {
                    if (UIValues.isProSkin) _refresh = loadSystemIconTexture("d_Refresh");
                    else _refresh = loadSystemIconTexture("Refresh");
                }
                return _refresh;
            }
        }
        public Texture2D libraryDb
        {
            get
            {
                if (_libraryDb == null) _libraryDb = loadUITexture("LibraryDb");
                return _libraryDb;
            }
        }
        // Note: Was used before. Currently not available.
        public Texture2D scissors
        {
            get
            {
                if (_scissors == null) _scissors = loadUITexture("Scissors");
                return _scissors;
            }
        }
        public Texture2D location
        {
            get
            {
                if (_location == null) _location = loadUITexture("Location");
                return _location;
            }
        }
        public Texture2D itemArrowRight
        {
            get
            {
                if (_itemArrowRight == null) _itemArrowRight = loadUITexture("ItemArrowRight");
                return _itemArrowRight;
            }
        }
        public Texture2D itemArrowUp
        {
            get
            {
                if (_itemArrowUp == null) _itemArrowUp = loadUITexture("ItemArrowUp");
                return _itemArrowUp;
            }
        }
        public Texture2D itemArrowTop
        {
            get
            {
                if (_itemArrowTop == null) _itemArrowTop = loadUITexture("ItemArrowTop");
                return _itemArrowTop;
            }
        }
        public Texture2D itemArrowDown
        {
            get
            {
                if (_itemArrowDown == null) _itemArrowDown = loadUITexture("ItemArrowDown");
                return _itemArrowDown;
            }
        }
        public Texture2D itemArrowBottom
        {
            get
            {
                if (_itemArrowBottom == null) _itemArrowBottom = loadUITexture("ItemArrowBottom");
                return _itemArrowBottom;
            }
        }
        public Texture2D moveUp
        {
            get
            {
                if (_moveUp == null) _moveUp = loadUITexture("MoveUp");
                return _moveUp;
            }
        }
        public Texture2D moveDown
        {
            get
            {
                if (_moveDown == null) _moveDown = loadUITexture("MoveDown");
                return _moveDown;
            }
        }
        public Texture2D moveTop
        {
            get
            {
                if (_moveTop == null) _moveTop = loadUITexture("MoveTop");
                return _moveTop;
            }
        }
        public Texture2D moveBottom
        {
            get
            {
                if (_moveBottom == null) _moveBottom = loadUITexture("MoveBottom");
                return _moveBottom;
            }
        }
        public Texture2D dragArrow
        {
            get
            {
                if (_dragArrow == null)
                {
                    if (UIValues.isProSkin) _dragArrow = loadSystemIconTexture("d_DragArrow");
                    else _dragArrow = loadSystemIconTexture("DragArrow");
                }
                return _dragArrow;
            }
        }
        public Texture2D transform
        {
            get
            {
                if (_transform == null)
                {
                    if (UIValues.isProSkin) _transform = loadSystemIconTexture("d_Transform Icon");
                    else _transform = loadSystemIconTexture("Transform Icon");
                }
                return _transform;
            }
        }
        public Texture2D settings
        {
            get
            {
                if (_settings == null)
                {
                    if (UIValues.isProSkin) _settings = loadSystemIconTexture("d_Settings Icon");
                    else _settings = loadSystemIconTexture("Settings Icon");
                }
                return _settings;
            }
        }
        public Texture2D projection
        {
            get
            {
                if (_projection == null) _projection = loadUITexture("Projection_32");
                return _projection;
            }
        }
        public Texture2D layers
        {
            get
            {
                if (_layers == null) _layers = loadUITexture("Layers");
                return _layers;
            }
        }
        public Texture2D greenSphere
        {
            get
            {
                if (_greenSphere == null) _greenSphere = loadSystemIconTexture("sv_icon_dot3_pix16_gizmo");
                return _greenSphere;
            }
        }
        public Texture2D terrain
        {
            get
            {
                if (_terrain == null)
                {
                    if (UIValues.isProSkin) _terrain = loadSystemIconTexture("d_Terrain Icon");
                    else _terrain = loadSystemIconTexture("Terrain Icon");
                }
                return _terrain;
            }
        }
        public Texture2D eraser
        {
            get
            {
                if (_eraser == null) _eraser = loadUITexture("Eraser_32");
                return _eraser;
            }
        }
        public Texture2D eraser_gray
        {
            get
            {
                if (_eraser_gray == null)
                {
                    if (UIValues.isProSkin) _eraser_gray = loadSystemIconTexture("d_Grid.EraserTool");
                    else _eraser_gray = loadSystemIconTexture("Grid.EraserTool");
                }
                return _eraser_gray;
            }
        }
        public Texture2D hand
        {
            get
            {
                if (_hand == null) _hand = loadUITexture("Hand_32");
                return _hand;
            }
        }
        public Texture2D handNo
        {
            get
            {
                if (_handNo == null) _handNo = loadUITexture("HandNo_32");
                return _handNo;
            }
        }
        public Texture2D lightBulb
        {
            get
            {
                if (_lightBulb == null) _lightBulb = loadUITexture("LightBulb");
                return _lightBulb;
            }
        }
        public Texture2D lightBulbGray
        {
            get
            {
                if (_lightBulbGray == null) _lightBulbGray = loadUITexture("LightBulbGray");
                return _lightBulbGray;
            }
        }
        public Texture2D delete
        {
            get
            {
                if (_delete == null) _delete = loadUITexture("Delete_64");
                return _delete;
            }
        }
        public Texture2D clear
        {
            get
            {
                if (_clear == null) _clear = loadSystemIconTexture("P4_DeletedLocal");
                return _clear;
            }
        }
        public Texture2D vertexSnap
        {
            get
            {
                if (_vertexSnap == null) _vertexSnap = loadUITexture("VertexSnap_32");
                return _vertexSnap;
            }
        }
        public Texture2D boxSnap
        {
            get
            {
                if (_boxSnap == null) _boxSnap = loadUITexture("BoxSnap_32");
                return _boxSnap;
            }
        }
        public Texture2D objectSurfaceSnap
        {
            get
            {
                if (_objectSurfaceSnap == null) _objectSurfaceSnap = loadUITexture("ObjectSurfaceSnap_32");
                return _objectSurfaceSnap;
            }
        }
        public Texture2D ping
        {
            get
            {
                if (_ping == null)
                {
                    if (UIValues.isProSkin) _ping = loadSystemIconTexture("orangeLight");
                    else _ping = loadSystemIconTexture("d_greenLight");
                }
                return _ping;
            }
        }
        public Texture2D objectGroup
        {
            get
            {
                if (_objectGroup == null) _objectGroup = loadUITexture("ObjectGroup");
                return _objectGroup;
            }
        }
        public Texture2D objectGroupRotated
        {
            get
            {
                if (_objectGroupRotated == null) _objectGroupRotated = loadUITexture("ObjectGroupRotated");
                return _objectGroupRotated;
            }
        }
        public Texture2D defaultObjectGroup
        {
            get
            {
                if (_defaultObjectGroup == null) _defaultObjectGroup = loadUITexture("DefaultObjectGroup");
                return _defaultObjectGroup;
            }
        }
        public Texture2D objectGroupDelete
        {
            get
            {
                if (_objectGroupDelete == null) _objectGroupDelete = loadUITexture("ObjectGroupDelete");
                return _objectGroupDelete;
            }
        }
        public Texture2D objectGroupHand
        {
            get
            {
                if (_objectGroupHand == null) _objectGroupHand = loadUITexture("ObjectGroupHand");
                return _objectGroupHand;
            }
        }
        public Texture2D intPattern
        {
            get
            {
                if (_intPattern == null) _intPattern = loadUITexture("IntPattern_24");
                return _intPattern;
            }
        }
        public Texture2D terrainFlatten
        {
            get
            {
                if (_terrainFlatten == null) _terrainFlatten = loadUITexture("TerrainFlatten_32");
                return _terrainFlatten;
            }
        }
        public Texture2D chemistry
        {
            get
            {
                if (_chemistry == null) _chemistry = loadUITexture("Chemistry_32");
                return _chemistry;
            }
        }
        public Texture2D createAddNew
        {
            get
            {
                if (_createAddNew == null)
                {
                    if (UIValues.isProSkin) _createAddNew = loadSystemIconTexture("d_CreateAddNew");
                    else _createAddNew = loadSystemIconTexture("CreateAddNew");
                }

                return _createAddNew;
            }
        }
        public Texture2D sync
        {
            get
            {
                if (_sync == null)
                {
                    if (UIValues.isProSkin) _sync = loadSystemIconTexture("d_SyncSearch");
                    else _sync = loadSystemIconTexture("SyncSearch");
                }

                return _sync;
            }
        }
        public Texture2D misc
        {
            get
            {
                if (_misc == null) _misc = loadSystemIconTexture("sv_icon_dot9_pix16_gizmo");
                return _misc;
            }
        }
        public Texture2D selectionGrow
        {
            get
            {
                if (_selectionGrow == null)
                {
                    if (UIValues.isProSkin) _selectionGrow = loadSystemIconTexture("d_ScaleConstraint Icon");
                    else _selectionGrow = loadSystemIconTexture("ScaleConstraint Icon");
                }

                return _selectionGrow;
            }
        }
        public Texture2D scenePicking_notPickable
        {
            get
            {
                if (_scenePicking_notPickable == null)
                {
                    if (UIValues.isProSkin) _scenePicking_notPickable = loadSystemIconTexture("d_scenepicking_notpickable");
                    else _scenePicking_notPickable = loadSystemIconTexture("scenepicking_notpickable");
                }

                return _scenePicking_notPickable;
            }
        }
        public Texture2D brush
        {
            get
            {
                if (_brush == null)
                {
                    if (UIValues.isProSkin) _brush = loadSystemIconTexture("d_Grid.PaintTool");
                    else _brush = loadSystemIconTexture("Grid.PaintTool");
                }

                return _brush;
            }
        }
        public Texture2D tileGrid
        {
            get
            {
                if (_tileGrid == null) _tileGrid = loadSystemIconTexture("d_Tilemap Icon");
                return _tileGrid;
            }
        }
        public Texture2D filter
        {
            get
            {
                if (_filter == null) _filter = loadUITexture("Filter");
                return _filter;
            }
        }
        public Texture2D ramp
        {
            get
            {
                if (_ramp == null) _ramp = UIValues.isProSkin ? loadUITexture("RampDark") : loadUITexture("Ramp");
                return _ramp;
            }
        }
        public Texture2D tileNeighborRadius
        {
            get
            {
                if (_tileNeighborRadius == null) _tileNeighborRadius = UIValues.isProSkin ? loadSystemIconTexture("d_CanvasScaler Icon") : loadSystemIconTexture("CanvasScaler Icon");
                return _tileNeighborRadius;
            }
        }
        public Texture2D autoRefresh
        {
            get
            {
                if (_autoRefresh == null) _autoRefresh = UIValues.isProSkin ? loadSystemIconTexture("d_playLoopOn") : loadSystemIconTexture("playLoopOn");
                return _autoRefresh;
            }
        }
        public Texture2D defaultObject
        {
            get
            {
                if (_defaultObject == null) _defaultObject = UIValues.isProSkin ? loadSystemIconTexture("d_ObjectMode") : loadSystemIconTexture("ObjectMode");
                return _defaultObject;
            }
        }
        public Texture2D tileRuleSpawnBoxBrush
        {
            get
            {
                return selectionRect;
            }
        }
        public Texture2D tileRuleSpawnFlexiBoxBrush
        {
            get
            {
                if (_tileRuleSpawnFlexiBoxBrush == null) _tileRuleSpawnFlexiBoxBrush
                        = UIValues.isProSkin ? loadUITexture("TileRuleSpawnFlexiBoxBrushDark_24") : loadUITexture("TileRuleSpawnFlexiBoxBrush_24");
                return _tileRuleSpawnFlexiBoxBrush;
            }
        }
        public Texture2D tileRuleSpawnConnect
        {
            get
            {
                if (_tileRuleSpawnConnect == null) _tileRuleSpawnConnect
                        = UIValues.isProSkin ? loadUITexture("TileRuleSpawnConnectDark") : loadUITexture("TileRuleSpawnConnect");
                return _tileRuleSpawnConnect;
            }
        }
        public Texture2D tileRuleSpawnSegmentBrush
        {
            get
            {
                return selectionSegments;
            }
        }
        public Texture2D fixOverlaps
        {
            get
            {
                if (_fixOverlaps == null) _fixOverlaps = loadUITexture("FixOverlaps");
                return _fixOverlaps;
            }
        }
        public Texture2D load
        {
            get
            {
                if (_load == null) _load = loadUITexture("Load");
                return _load;
            }
        }
        public Texture2D decor
        {
            get
            {
                if (_decor == null) _decor = loadUITexture("Decor");
                return _decor;
            }
        }
        public Texture2D tag
        {
            get
            {
                if (_tag == null) _tag = loadUITexture("Tag");
                return _tag;
            }
        }
        public Texture2D pin
        {
            get
            {
                if (_pin == null) _pin = loadUITexture("Pin");
                return _pin;
            }
        }
        public Texture2D pin_Disabled
        {
            get
            {
                if (_pin_Disabled == null) _pin_Disabled = loadUITexture("Pin_Disabled");
                return _pin_Disabled;
            }
        }
        public Texture2D ruler
        {
            get
            {
                if (_ruler == null) _ruler = loadUITexture("Ruler_32");
                return _ruler;
            }
        }
        public Texture2D overlap
        {
            get
            {
                if (_overlap == null) _overlap = loadUITexture("Overlap");
                return _overlap;
            }
        }
        public Texture2D overlap_Off
        {
            get
            {
                if (_overlap_Off == null) _overlap_Off = loadUITexture("Overlap_Off");
                return _overlap_Off;
            }
        }
        public Texture2D wallRule_Straight
        {
            get
            {
                if (_wallRule_Straight == null) _wallRule_Straight = loadUITexture("WallRule_Straight_64");
                return _wallRule_Straight;
            }
        }
        public Texture2D wallRule_InnerCorner
        {
            get
            {
                if (_wallRule_InnerCorner == null) _wallRule_InnerCorner = loadUITexture("WallRule_InnerCorner_64");
                return _wallRule_InnerCorner;
            }
        }
        public Texture2D wallRule_OuterCorner
        {
            get
            {
                if (_wallRule_OuterCorner == null) _wallRule_OuterCorner = loadUITexture("WallRule_OuterCorner_64");
                return _wallRule_OuterCorner;
            }
        }

        private Texture2D[] _wallRuleTextures;
        public Texture2D getModularWallRuleTexture(ModularWallRuleId ruleId)
        {
            if (_wallRuleTextures == null)
            {
                _wallRuleTextures = new Texture2D[Enum.GetValues(typeof(ModularWallRuleId)).Length];
                _wallRuleTextures[(int)ModularWallRuleId.StraightWall]      = wallRule_Straight;
                _wallRuleTextures[(int)ModularWallRuleId.InnerCorner]       = wallRule_InnerCorner;
                _wallRuleTextures[(int)ModularWallRuleId.OuterCorner]       = wallRule_OuterCorner;
            }

            return _wallRuleTextures[(int)ruleId];
        }

        public static float getModularWallRuleTextureSize()
        {
            return 64.0f;
        }

        private static Texture2D loadUITexture(string textuerName)
        {
            var texture = Resources.Load<Texture2D>(getUITexturePath(textuerName));
            if (texture == null) texture = EditorGUIUtility.whiteTexture;

            return texture;
        }

        private static string getUITexturePath(string textureName)
        {
            return "Textures/UI/" + textureName;
        }

        private static Texture2D loadSystemIconTexture(string iconName)
        {
            var texture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            if (texture == null) texture = EditorGUIUtility.whiteTexture;

            return texture;
        }
    }
}
#endif