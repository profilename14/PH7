#if UNITY_EDITOR
//#define MODULAR_WALL_PREFAB_PROFILE_NO_DUPLICATE_PREFABS
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace GSPAWN
{
    public enum ModularWallRuleId
    {
        StraightWall = 0,
        InnerCorner,
        OuterCorner,
    }

    public enum ModularWallAxis
    {
        X = 0,
        Y, 
        Z
    }

    public static class ModularWallRuleIdEx
    {
        private static int                  _numRuleIds;
        private static ModularWallRuleId[]  _ruleIdArray;

        static ModularWallRuleIdEx()
        {
            var allIds = Enum.GetValues(typeof(ModularWallRuleId));
            _numRuleIds = allIds.Length;

            _ruleIdArray = new ModularWallRuleId[_numRuleIds];
            foreach (var item in allIds)
            {
                var ruleId = (ModularWallRuleId)item;
                _ruleIdArray[(int)ruleId] = ruleId;
            }
        }

        public static int                   numRuleIds      { get { return _numRuleIds; } }
        public static ModularWallRuleId[]   ruleIdArray     { get { return _ruleIdArray; } }
    }

    [Serializable]
    public class ModularWallProperties
    {
        [SerializeField]
        public ModularWallAxis      upAxis              = ModularWallAxis.Y;
        [SerializeField]
        public ModularWallAxis      forwardAxis         = ModularWallAxis.Z;
        [SerializeField]
        public bool                 invertForwardAxis   = false;
        [SerializeField]
        public ModularWallAxis      innerAxis           = ModularWallAxis.X;
        [SerializeField]
        public bool                 invertInnerAxis     = false;

        [SerializeField]
        public float        height          = 0.0f;
        [SerializeField]
        public float        forwardSize     = 0.0f;
    }

    [Serializable]
    public class ModularWallRelativeTransform
    {
        [SerializeField]
        public Vector3      position;
        [SerializeField]
        public Quaternion   rotation;

        public void reset()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }
    }

    public class ModularWallPrefabProfile : Profile
    {
        [SerializeField]
        private GameObject _examplePrefab = null;
        [SerializeField]
        private GameObject _ep_InnerCorner = null;
        [SerializeField]
        private GameObject _ep_OuterCorner = null;
        [SerializeField]
        private GameObject _ep_MidStraight = null;
        [SerializeField]
        private GameObject _ep_StraightToOuter = null;
        [SerializeField]
        private GameObject _ep_StraightToInner = null;
        [SerializeField]
        private GameObject _ep_PillarInnerCornerBegin = null;
        [SerializeField]
        private GameObject _ep_PillarInnerCornerEnd = null;
        [SerializeField]
        private GameObject _ep_PillarOuterCornerBegin = null;
        [SerializeField]
        private GameObject _ep_PillarOuterCornerEnd = null;
        [SerializeField]
        private GameObject _ep_PillarInnerCornerMiddle = null;
        [SerializeField]
        private GameObject _ep_PillarOuterCornerMiddle = null;

        [SerializeField]
        private ModularWallProperties _wallProperties = new ModularWallProperties();
        [SerializeField]
        private bool _truncateForwardSize = false;
        [SerializeField]
        private bool _spawnPillars = false;
        [SerializeField]
        private string _pillarProfileName = RandomPrefabProfileDb.defaultProfileName;

        [SerializeField]
        private ModularWallRelativeTransform _innerCornerRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _outerCornerRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _straightRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarMidStraightBeginRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarMidStraightEndRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarInnerCornerBeginRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarInnerCornerEndRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarOuterCornerBeginRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarOuterCornerEndRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarInnerCornerMiddleRT = new ModularWallRelativeTransform();
        [SerializeField]
        private ModularWallRelativeTransform _pillarOuterCornerMiddleRT = new ModularWallRelativeTransform();
        [SerializeField]
        private float _straight_ForwardPushDistance_Inner;
        [SerializeField]
        private float _straight_InnerPushDistance_Inner;
        [SerializeField]
        private float _straight_ForwardPushDistance_Outer;
        [SerializeField]
        private float _straight_InnerPushDistance_Outer;

        private static ObjectBounds.QueryConfig _wallBoundsQConfig = new ObjectBounds.QueryConfig()
        {
            includeInactive  = false,
            includeInvisible = false,
            objectTypes      = GameObjectType.Mesh
        };
        private static ObjectBounds.QueryConfig _pillarBoundsQConfig = new ObjectBounds.QueryConfig()
        {
            includeInvisible   = false,
            includeInactive    = false,
            objectTypes        = GameObjectType.Mesh
        };

        private SerializedObject _serializedObject;
        [SerializeField]
        private List<ModularWallPrefab> _straightWallPrefabs = new List<ModularWallPrefab>();
        [SerializeField]
        private List<ModularWallPrefab> _innerCornerPrefabs = new List<ModularWallPrefab>();
        [SerializeField]
        private List<ModularWallPrefab> _outerCornerPrefabs = new List<ModularWallPrefab>();
        [NonSerialized]
        private List<ModularWallPrefab>[] _prefabListArray = new List<ModularWallPrefab>[Enum.GetValues(typeof(ModularWallRuleId)).Length];

        [NonSerialized]
        private CumulativeProbabilityTable<ModularWallPrefab> _straightWallTable = new CumulativeProbabilityTable<ModularWallPrefab>();
        [NonSerialized]
        private CumulativeProbabilityTable<ModularWallPrefab> _innerCornerTable = new CumulativeProbabilityTable<ModularWallPrefab>();
        [NonSerialized]
        private CumulativeProbabilityTable<ModularWallPrefab> _outerCornerTable = new CumulativeProbabilityTable<ModularWallPrefab>();
        [NonSerialized]
        private CumulativeProbabilityTable<ModularWallPrefab>[] _tableArray = new CumulativeProbabilityTable<ModularWallPrefab>[Enum.GetValues(typeof(ModularWallRuleId)).Length];

        [NonSerialized]
        private List<ModularWallPrefab> _mdWallPrefabBuffer = new List<ModularWallPrefab>();
        [NonSerialized]
        private List<GameObject> _objectBuffer = new List<GameObject>();

        public GameObject examplePrefab
        {
            get { return _examplePrefab; }
            set
            {
                if (value == null) 
                { 
                    _examplePrefab = null;  
                    onExamplePrefabChanged(); 
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    if (value.isSceneObject())
                    {
                        Debug.LogError("The example prefab must be a prefab asset.");
                        return;
                    }    

                    // Note: Don't return if this fails. The user may be using a piece config which is
                    //       not recognized by the detector.
                    if (!ModularWallPrefabPieceDetector.detectWallPieces(value))
                    {
                        EditorUtility.DisplayDialog("Wall Piece Detection", "The plugin tried to automatically identify the wall pieces, but failed. " +
                            "You may have to manually identify the wall pieces and name them properly.", "Ok");
                    }
                    if (!validateExamplePrefab(value)) return;

                    UndoEx.record(this);
                    _examplePrefab = value;
                    onExamplePrefabChanged();
                    EditorUtility.SetDirty(this);
                }
            }
        }
        public bool hasInnerOuterCorners { get { return _ep_InnerCorner != null && _ep_OuterCorner != null; } }
        public bool hasPillars
        {
            get
            {
                return (hasInnerCornerPillarBegin || hasOuterCornerPillarBegin ||
                        hasInnerCornerPillarEnd || hasOuterCornerPillarEnd ||
                        hasInnerCornerPillarMiddle || hasOuterCornerPillarMiddle)
                        && pillarProfile.getNumUsedPrefabs() != 0;
            }
        }
        public bool hasInnerCornerPillarBegin { get { return _ep_PillarInnerCornerBegin != null; } }
        public bool hasOuterCornerPillarBegin { get { return _ep_PillarOuterCornerBegin != null; } }
        public bool hasInnerCornerPillarEnd { get { return _ep_PillarInnerCornerEnd != null; } }
        public bool hasOuterCornerPillarEnd { get { return _ep_PillarOuterCornerEnd != null; } }
        public bool hasInnerCornerPillarMiddle { get { return _ep_PillarInnerCornerMiddle != null; } }
        public bool hasOuterCornerPillarMiddle { get { return _ep_PillarOuterCornerMiddle != null; } }
        public ModularWallRelativeTransform innerCornerRT { get { return _innerCornerRT; } }
        public ModularWallRelativeTransform outerCornerRT { get { return _outerCornerRT; } }
        public ModularWallRelativeTransform straightRT { get { return _straightRT; } }
        public ModularWallRelativeTransform pillarMidStraightBeginRT { get { return _pillarMidStraightBeginRT; } }
        public ModularWallRelativeTransform pillarMidStraightEndRT { get { return _pillarMidStraightEndRT; } }
        public ModularWallRelativeTransform pillarInnerCornerBeginRT { get { return _pillarInnerCornerBeginRT; } }
        public ModularWallRelativeTransform pillarOuterCornerBeginRT { get { return _pillarOuterCornerBeginRT; } }
        public ModularWallRelativeTransform pillarInnerCornerEndRT { get { return _pillarInnerCornerEndRT; } }
        public ModularWallRelativeTransform pillarOuterCornerEndRT { get { return _pillarOuterCornerEndRT; } }
        public ModularWallRelativeTransform pillarInnerCornerMiddleRT { get { return _pillarInnerCornerMiddleRT; } }
        public ModularWallRelativeTransform pillarOuterCornerMiddleRT { get { return _pillarOuterCornerMiddleRT; } }
        public float straight_ForwardPushDistance_Inner { get { return _straight_ForwardPushDistance_Inner; } }
        public float straight_InnerPushDistance_Inner { get { return _straight_InnerPushDistance_Inner; } }
        public float straight_ForwardPushDistance_Outer { get { return _straight_ForwardPushDistance_Outer; } }
        public float straight_InnerPushDistance_Outer { get { return _straight_InnerPushDistance_Outer; } }
        public ModularWallAxis wallUpAxis
        {
            get { return _wallProperties.upAxis; }
            set
            {
                if (value == wallForwardAxis) return;

                UndoEx.record(this);
                _wallProperties.upAxis = value;
                updateWallProperties();
                updateRelationships();
                EditorUtility.SetDirty(this);
            }
        }
        public ModularWallAxis wallForwardAxis { get { return _wallProperties.forwardAxis; } }
        public bool invertFowardAxis { get { return _wallProperties.invertForwardAxis; } }
        public ModularWallAxis wallInnerAxis { get { return _wallProperties.innerAxis; } }
        public float wallHeight { get { return _wallProperties.height; } }
        public float wallForwardSize { get { return _wallProperties.forwardSize; } }
        public bool truncateForwardSize
        {
            get { return _truncateForwardSize; }
            set
            {
                if (value == _truncateForwardSize) return;

                UndoEx.record(this);
                _truncateForwardSize = value;
                updateWallProperties();
                updateRelationships();
                EditorUtility.SetDirty(this);
            }
        }
        public bool spawnPillars { get { return _spawnPillars; } set { UndoEx.record(this); _spawnPillars = value; EditorUtility.SetDirty(this); } }
        public string pillarProfileName
        {
            get { return _pillarProfileName; }
            set
            {
                UndoEx.record(this);
                _pillarProfileName = value;
                if (string.IsNullOrEmpty(_pillarProfileName)) _pillarProfileName = RandomPrefabProfileDb.defaultProfileName;
                EditorUtility.SetDirty(this);
            }
        }
        public RandomPrefabProfile pillarProfile
        {
            get
            {
                var profile = RandomPrefabProfileDb.instance.findProfile(_pillarProfileName);
                if (profile == null) profile = RandomPrefabProfileDb.instance.defaultProfile;
                return profile;
            }
        }
        public int numPrefabs
        {
            get
            {
                int num = 0;
                foreach (var prefabList in _prefabListArray)
                    num += prefabList.Count;

                return num;
            }
        }
        public SerializedObject serializedObject
        {
            get
            {
                if (_serializedObject == null) _serializedObject = new SerializedObject(this);
                return _serializedObject;
            }
        }

        public static string innerCornerName                    { get { return "InnerCorner"; } }
        public static string outerCornerName                    { get { return "OuterCorner"; } }
        public static string middleStraightName                 { get { return "MiddleStraight"; } }
        public static string firstStraightName                  { get { return "FirstStraight"; } }         // Same as straightToOuterName
        public static string lastStraightName                   { get { return "LastStraight"; } }          // Same as straightToInnerName
        public static string straightToOuterName                { get { return "StraightToOuter"; } }       // Same as firstStraightName
        public static string straightToInnerName                { get { return "StraightToInner"; } }       // Same as lastStraightName

        public static string pillarInnerCornerBeginName         { get { return "PillarInnerCornerBegin"; } }
        public static string pillarOuterCornerBeginName         { get { return "PillarOuterCornerBegin"; } }
        public static string pillarInnerCornerEndName           { get { return "PillarInnerCornerEnd"; } }
        public static string pillarOuterCornerEndName           { get { return "PillarOuterCornerEnd"; } }
        public static string pillarInnerCornerMiddleName        { get { return "PillarInnerCornerMiddle"; } }
        public static string pillarOuterCornerMiddleName        { get { return "PillarOuterCornerMiddle"; } }

        public static string innerCornerName_LC                 { get { return "innercorner"; } }
        public static string outerCornerName_LC                 { get { return "outercorner"; } }
        public static string middleStraightName_LC              { get { return "middlestraight"; } }
        public static string firstStraightName_LC               { get { return "firststraight"; } }         // Same as straightToOuterName_LC
        public static string lastStraightName_LC                { get { return "laststraight"; } }          // Same as straightToInnerName_LC
        public static string straightToOuterName_LC             { get { return "straighttoouter"; } }       // Same as firstStraightName_LC
        public static string straightToInnerName_LC             { get { return "straighttoinner"; } }       // Same as lastStraightName_LC

        public static string pillarInnerCornerBeginName_LC      { get { return "pillarinnercornerbegin"; } }
        public static string pillarOuterCornerBeginName_LC      { get { return "pillaroutercornerbegin"; } }
        public static string pillarInnerCornerEndName_LC        { get { return "pillarinnercornerend"; } }
        public static string pillarOuterCornerEndName_LC        { get { return "pillaroutercornerend"; } }
        public static string pillarInnerCornerMiddleName_LC     { get { return "pillarinnercornermiddle"; } }
        public static string pillarOuterCornerMiddleName_LC     { get { return "pillaroutercornermiddle"; } }

        public void refreshPrefabData()
        {
            examplePrefab = _examplePrefab;
            onExamplePrefabChanged();
        }

        public bool isAnyPrefabUsed(ModularWallRuleId ruleId)
        {
            return isAnyPrefabUsed(_prefabListArray[(int)ruleId]);
        }

        public Vector3 getModularWallUpAxis(GameObject gameObject)
        {
            return gameObject.transform.modularWallToLocalAxis(wallUpAxis, false);
        }

        public Vector3 getModularWallForwardAxis(GameObject gameObject)
        {
            return gameObject.transform.modularWallToLocalAxis(wallForwardAxis, _wallProperties.invertForwardAxis);
        }

        public Vector3 getModularWallInnerAxis(GameObject gameObject)
        {
            return gameObject.transform.modularWallToLocalAxis(wallInnerAxis, _wallProperties.invertInnerAxis);
        }

        public AxisDescriptor getModularWallUpAxisDesc(GameObject gameObject)
        {
            return gameObject.transform.modularWallToLocalAxisDesc(wallUpAxis, false);
        }

        public AxisDescriptor getModularWallForwardAxisDesc(GameObject gameObject)
        {
            return gameObject.transform.modularWallToLocalAxisDesc(wallForwardAxis, _wallProperties.invertForwardAxis);
        }

        public int getNumPrefabsInRule(ModularWallRuleId ruleId)
        {
            var prefabList = _prefabListArray[(int)ruleId];
            return prefabList.Count;
        }

        public ModularWallPrefab getPrefab(ModularWallRuleId ruleId, int prefabIndex)
        {
            var prefabList = _prefabListArray[(int)ruleId];
            return prefabList[prefabIndex];
        }

        public ModularWallPrefab getFirstUsedPrefab(ModularWallRuleId ruleId)
        {
            var prefabList = _prefabListArray[(int)ruleId];
            foreach (var prefab in prefabList)
            {
                if (prefab.used) return prefab;
            }

            return null;
        }

        public ModularWallPrefab getBestPrefabForSpawnGuide()
        {
            float minVolume                 = float.MaxValue;
            ModularWallPrefab bestPrefab    = null;
            var prefabList = _prefabListArray[(int)ModularWallRuleId.StraightWall];
            foreach (var prefab in prefabList)
            {
                if (!prefab.used) continue;

                if (bestPrefab == null)
                {
                    bestPrefab = prefab;
                    minVolume = calcStraightWallOBB(bestPrefab.prefabAsset).volume;
                }
                else
                {
                    float v = calcStraightWallOBB(prefab.prefabAsset).volume;
                    if (v < minVolume)
                    {
                        bestPrefab = prefab;
                        minVolume = v;
                    }
                }
            }

            return bestPrefab != null ? bestPrefab : getFirstUsedPrefab(ModularWallRuleId.StraightWall);
        }

        public void onPrefabsSpawnChanceChanged()
        {
            refreshAllPrefabTables();
        }

        public void onPrefabsUsedStateChanged()
        {
            refreshAllPrefabTables();
        }

        public void onPrefabsUseDefaults()
        {
            refreshAllPrefabTables();
        }

        public OBB calcStraightWallOBB(GameObject wallObject)
        {
            OBB obb             = ObjectBounds.calcHierarchyWorldOBB(wallObject, _wallBoundsQConfig);
            Vector3 obbSize     = obb.size;
            obbSize[(int)_wallProperties.forwardAxis]   = _wallProperties.forwardSize;
            obbSize[(int)_wallProperties.upAxis]        = _wallProperties.height;
            obb.size            = obbSize;

            return obb;
        }

        public OBB calcPillarOBB(GameObject pillarObject)
        {
            OBB obb = ObjectBounds.calcHierarchyWorldOBB(pillarObject, _pillarBoundsQConfig);
            return obb;
        }

        public static ObjectBounds.QueryConfig getWallBoundsQConfig()
        {
            return _wallBoundsQConfig;
        }

        public ModularWallPrefab pickPrefab(ModularWallRuleId ruleId)
        {
            var table = _tableArray[(int)ruleId];
            return table.pickEntity();
        }

        public void resetPrefabPreviews()
        {
            foreach (var prefabList in _prefabListArray)
            {
                int numPrefabs = prefabList.Count;
                PluginProgressDialog.begin("Refreshing Prefab Previews");
                for (int prefabIndex = 0; prefabIndex < numPrefabs; ++prefabIndex)
                {
                    var prefab = prefabList[prefabIndex];
                    PluginProgressDialog.updateItemProgress(prefab.prefabAsset.name, (prefabIndex + 1) / (float)numPrefabs);
                    prefab.resetPreview();
                }

                PluginProgressDialog.end();
            }
        }

        public void regeneratePrefabPreviews()
        {
            foreach (var prefabList in _prefabListArray)
            {
                int numPrefabs = prefabList.Count;
                PluginProgressDialog.begin("Regenerating Prefab Previews");
                for (int prefabIndex = 0; prefabIndex < numPrefabs; ++prefabIndex)
                {
                    var prefab = prefabList[prefabIndex];
                    PluginProgressDialog.updateItemProgress(prefab.prefabAsset.name, (prefabIndex + 1) / (float)numPrefabs);
                    prefab.regeneratePreview();
                }

                PluginProgressDialog.end();
            }
        }

        public void onPrefabAssetWillBeDeleted(GameObject prefabAsset)
        {
            if (prefabAsset == _examplePrefab)
            {
                clearExamplePrefabs();

                foreach (var prefabList in _prefabListArray)
                {
                    foreach (var mdWallPrefab in prefabList)
                    {
                        AssetDbEx.removeObjectFromAsset(mdWallPrefab, this);
                        DestroyImmediate(mdWallPrefab);
                    }
                    prefabList.Clear();
                }

                Debug.LogWarning("ModularWallPrefabProfile: Example prefab asset has been destroyed. All wall prefabs have been removed.");
                return;
            }

            foreach (var prefabList in _prefabListArray)
            {
                var prefabsToRemove = prefabList.FindAll(item => item.prefabAsset == prefabAsset);
                foreach (var mdWallPrefab in prefabsToRemove)
                {
                    prefabList.Remove(mdWallPrefab);
                    AssetDbEx.removeObjectFromAsset(mdWallPrefab, this);
                    DestroyImmediate(mdWallPrefab);
                }
            }
     
            refreshAllPrefabTables();
            updateWallProperties();
        }

        public ModularWallPrefab createPrefab(PluginPrefab pluginPrefab, ModularWallRuleId ruleId)
        {
            if (_examplePrefab == null)
            {
                Debug.LogError("ModularWallPrefabProfile: Missing example prefab.");
                return null;
            }

            #if MODULAR_WALL_PREFAB_PROFILE_NO_DUPLICATE_PREFABS
            if (!containsPrefab(pluginPrefab))
            #endif
            {
                UndoEx.saveEnabledState();
                UndoEx.enabled = false;

                UndoEx.record(this);
                var mdWallPrefab            = UndoEx.createScriptableObject<ModularWallPrefab>();
                mdWallPrefab.pluginPrefab   = pluginPrefab;
                mdWallPrefab.name           = mdWallPrefab.pluginPrefab.prefabAsset.name;

                _prefabListArray[(int)ruleId].Add(mdWallPrefab);
                AssetDbEx.addObjectToAsset(mdWallPrefab, this);

                EditorUtility.SetDirty(this);
                refreshPrefabTable(_tableArray[(int)ruleId], _prefabListArray[(int)ruleId]);

                UndoEx.restoreEnabledState();
                UndoEx.record(this);

                return mdWallPrefab;
            }

            #if MODULAR_WALL_PREFAB_PROFILE_NO_DUPLICATE_PREFABS
            return null;
            #endif
        }

        public void createPrefabs(List<PluginPrefab> pluginPrefabs, ModularWallRuleId ruleId, List<ModularWallPrefab> createdPrefabs, bool appendCreated, string progressTitle)
        {
            _mdWallPrefabBuffer.Clear();
            if (_examplePrefab == null)
            {
                Debug.LogError("ModularWallPrefabProfile: Missing example prefab.");
                return;
            }

            PluginProgressDialog.begin(progressTitle);
            if (!appendCreated) createdPrefabs.Clear();

            UndoEx.saveEnabledState();
            UndoEx.enabled = false;

            for (int prefabIndex = 0; prefabIndex < pluginPrefabs.Count; ++prefabIndex)
            {
                var pluginPrefab = pluginPrefabs[prefabIndex];
                PluginProgressDialog.updateItemProgress(pluginPrefab.prefabAsset.name, (prefabIndex + 1) / (float)pluginPrefabs.Count);

                #if MODULAR_WALL_PREFAB_PROFILE_NO_DUPLICATE_PREFABS
                if (!containsPrefab(pluginPrefab))
                #endif
                {
                    var mdWallPrefab            = UndoEx.createScriptableObject<ModularWallPrefab>();
                    mdWallPrefab.pluginPrefab   = pluginPrefab;
                    mdWallPrefab.name           = mdWallPrefab.pluginPrefab.prefabAsset.name;

                    AssetDbEx.addObjectToAsset(mdWallPrefab, this);
                    createdPrefabs.Add(mdWallPrefab);
                    _mdWallPrefabBuffer.Add(mdWallPrefab);
                }
            }

            UndoEx.record(this);
            var prefabList = _prefabListArray[(int)ruleId];
            foreach (var mdWallPrefab in _mdWallPrefabBuffer)
                prefabList.Add(mdWallPrefab);

            EditorUtility.SetDirty(this);
            refreshPrefabTable(_tableArray[(int)ruleId], prefabList);
            PluginProgressDialog.end();

            UndoEx.restoreEnabledState();
            UndoEx.record(this);
        }

        public void deletePrefab(ModularWallPrefab prefab)
        {
            if (prefab != null)
            {
                int numRules = _prefabListArray.Length;
                for (int i = 0; i < numRules; ++i)
                {
                    var prefabList = _prefabListArray[i];
                    if (prefabList.Contains(prefab))
                    {
                        UndoEx.record(this);
                        prefabList.Remove(prefab);
                        UndoEx.destroyObjectImmediate(prefab);

                        EditorUtility.SetDirty(this);
                        refreshPrefabTable(_tableArray[i], prefabList);
                        break;
                    }
                }
            }
        }

        public void deletePrefabs(List<ModularWallPrefab> prefabs)
        {
            if (prefabs.Count != 0)
            {
                UndoEx.record(this);
                _mdWallPrefabBuffer.Clear();

                int numRules = _prefabListArray.Length;
                for (int i = 0; i < numRules; ++i)
                {
                    var prefabList = _prefabListArray[i];
                    foreach (var mdWallPrefab in prefabs)
                    {
                        if (prefabList.Contains(mdWallPrefab))
                        {
                            prefabList.Remove(mdWallPrefab);
                            _mdWallPrefabBuffer.Add(mdWallPrefab);
                        }
                    }
                }

                foreach (var prefab in _mdWallPrefabBuffer)
                    UndoEx.destroyObjectImmediate(prefab);

                EditorUtility.SetDirty(this);
                refreshAllPrefabTables();
            }
        }

        public void deletePrefabs(List<PluginPrefab> pluginPrefabs)
        {
            if (pluginPrefabs.Count != 0)
            {
                UndoEx.record(this);
                _mdWallPrefabBuffer.Clear();

                int numRules = _prefabListArray.Length;
                for (int i = 0; i < numRules; ++i)
                {
                    var prefabList = _prefabListArray[i];
                    foreach (var pluginPrefab in pluginPrefabs)
                    {
                        for (int mdPrefabIndex = 0; mdPrefabIndex < prefabList.Count;)
                        {
                            var mdPrefab = prefabList[mdPrefabIndex];
                            if (pluginPrefabs.Contains(mdPrefab.pluginPrefab))
                            {
                                prefabList.RemoveAt(mdPrefabIndex);
                                _mdWallPrefabBuffer.Add(mdPrefab);
                            }
                            else ++mdPrefabIndex;
                        }
                    }
                }

                foreach (var prefab in _mdWallPrefabBuffer)
                    UndoEx.destroyObjectImmediate(prefab);

                EditorUtility.SetDirty(this);
                refreshAllPrefabTables();
            }
        }

        public void deleteAllPrefabs()
        {
            if (numPrefabs != 0)
            {
                UndoEx.record(this);
                _mdWallPrefabBuffer.Clear();

                foreach (var prefabList in _prefabListArray)
                {
                    _mdWallPrefabBuffer.AddRange(prefabList);
                    prefabList.Clear();
                }

                foreach (var prefab in _mdWallPrefabBuffer)
                    UndoEx.destroyObjectImmediate(prefab);

                EditorUtility.SetDirty(this);
                refreshAllPrefabTables();
            }
        }

        public bool containsPrefab(ModularWallPrefab prefab)
        {
            foreach (var prefabList in _prefabListArray)
            {
                if (prefabList.Contains(prefab)) return true;
            }

            return false;
        }

        public bool containsWallPiecePrefabAsset(GameObject prefabAsset)
        {
            foreach (var prefabList in _prefabListArray)
            {
                foreach (var prefab in prefabList)
                {
                    if (prefab.prefabAsset == prefabAsset) return true;
                }
            }

            return false;
        }

        public bool containsPillarPrefabAsset(GameObject prefabAsset)
        {
            var prefabProfile = pillarProfile;
            return prefabProfile.containsPrefabAsset(prefabAsset);
        }

        public void getPrefabs(ModularWallRuleId ruleId, List<ModularWallPrefab> prefabs)
        {
            prefabs.Clear();
            prefabs.AddRange(_prefabListArray[(int)ruleId]);
        }

        private bool isAnyPrefabUsed(List<ModularWallPrefab> prefabs)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.used) return true;
            }

            return false;
        }

        private void refreshAllPrefabTables()
        {
            int numTables = _tableArray.Length;
            for (int i = 0; i < numTables; ++i) 
                refreshPrefabTable(_tableArray[i], _prefabListArray[i]);
        }

        private void refreshPrefabTable(CumulativeProbabilityTable<ModularWallPrefab> prefabTable, List<ModularWallPrefab> prefabs)
        {
            prefabTable.clear();
            foreach (var prefab in prefabs)
            {
                if (prefab.used) prefabTable.addEntity(prefab, prefab.spawnChance);
            }
            prefabTable.refresh();
        }

        private void initializeTableArray()
        {
            _tableArray[(int)ModularWallRuleId.StraightWall]    = _straightWallTable;
            _tableArray[(int)ModularWallRuleId.InnerCorner]     = _innerCornerTable;
            _tableArray[(int)ModularWallRuleId.OuterCorner]     = _outerCornerTable;
        }

        private void initializeRulePrefabListArray()
        {
            _prefabListArray[(int)ModularWallRuleId.StraightWall]   = _straightWallPrefabs;
            _prefabListArray[(int)ModularWallRuleId.InnerCorner]    = _innerCornerPrefabs;
            _prefabListArray[(int)ModularWallRuleId.OuterCorner]    = _outerCornerPrefabs;
        }

        private void onExamplePrefabChanged()
        {
            clearExamplePrefabs();

            if (_examplePrefab != null)
            {
                _examplePrefab.getAllChildren(false, false, _objectBuffer);
                foreach (var child in _objectBuffer)
                {
                    string childName = child.name.ToLower();
               
                    if (childName == innerCornerName_LC)                    _ep_InnerCorner = child;
                    else if (childName == outerCornerName_LC)               _ep_OuterCorner = child;
                    else if (childName == middleStraightName_LC)            _ep_MidStraight = child;
                    else if (childName == firstStraightName_LC || 
                             childName == straightToOuterName_LC)               _ep_StraightToOuter = child;
                    else if (childName == lastStraightName_LC ||
                             childName == straightToInnerName_LC)               _ep_StraightToInner = child;
                    else if (childName == pillarInnerCornerBeginName_LC)    _ep_PillarInnerCornerBegin = child;
                    else if (childName == pillarOuterCornerBeginName_LC)    _ep_PillarOuterCornerBegin = child;
                    else if (childName == pillarInnerCornerEndName_LC)      _ep_PillarInnerCornerEnd = child;
                    else if (childName == pillarOuterCornerEndName_LC)      _ep_PillarOuterCornerEnd = child;
                    else if (childName == pillarInnerCornerMiddleName_LC)   _ep_PillarInnerCornerMiddle = child;
                    else if (childName == pillarOuterCornerMiddleName_LC)   _ep_PillarOuterCornerMiddle = child;
                }

                // Auto assign prefabs
                bool updateUI = false;
                PluginPrefab rulePluginPrefab;

                if (getNumPrefabsInRule(ModularWallRuleId.InnerCorner) == 0)
                {
                    if (_ep_InnerCorner != null)
                    {
                        rulePluginPrefab = PrefabLibProfileDb.instance.getPrefab(_ep_InnerCorner.getPrefabAsset());
                        if (rulePluginPrefab != null)
                        {
                            createPrefab(rulePluginPrefab, ModularWallRuleId.InnerCorner);
                            updateUI = true;
                        }
                    }
                }

                if (getNumPrefabsInRule(ModularWallRuleId.OuterCorner) == 0)
                {
                    if (_ep_OuterCorner != null)
                    {
                        rulePluginPrefab = PrefabLibProfileDb.instance.getPrefab(_ep_OuterCorner.getPrefabAsset());
                        if (rulePluginPrefab != null)
                        {
                            createPrefab(rulePluginPrefab, ModularWallRuleId.OuterCorner);
                            updateUI = true;
                        }
                    }
                }

                if (getNumPrefabsInRule(ModularWallRuleId.StraightWall) == 0)
                {
                    rulePluginPrefab = null;

                    if (_ep_MidStraight != null)
                        rulePluginPrefab = PrefabLibProfileDb.instance.getPrefab(_ep_MidStraight.getPrefabAsset());

                    if (rulePluginPrefab == null && _ep_StraightToOuter != null) 
                        rulePluginPrefab = PrefabLibProfileDb.instance.getPrefab(_ep_StraightToOuter.getPrefabAsset());

                    if (rulePluginPrefab == null && _ep_StraightToInner != null) 
                        rulePluginPrefab = PrefabLibProfileDb.instance.getPrefab(_ep_StraightToInner.getPrefabAsset());

                    if (rulePluginPrefab != null)
                    {
                        createPrefab(rulePluginPrefab, ModularWallRuleId.StraightWall);
                        updateUI = true;
                    }
                }

                if (updateUI)
                    ModularWallPrefabProfileDbUI.instance.refresh();
            }

            updateWallProperties();
            updateRelationships();
        }

        private GameObject getWallPieceByName(string name, List<GameObject> wallPieces)
        {
            string lowerName = name.ToLower();
            foreach (var child in wallPieces)
            {
                string childName = child.name.ToLower();
                if (childName == lowerName) return child;
            }

            return null;
        }

        private bool isWallPiecePillar(GameObject wallPiece)
        {
            string  name = wallPiece.name.ToLower();
            return  name == pillarInnerCornerBeginName_LC || name == pillarInnerCornerEndName_LC ||
                    name == pillarOuterCornerBeginName_LC || name == pillarOuterCornerEndName_LC ||
                    name == pillarInnerCornerMiddleName_LC || name == pillarOuterCornerMiddleName_LC;
        }

        private bool isWallPieceStraight(GameObject wallPiece)
        {
            string name = wallPiece.name.ToLower();
            return name == middleStraightName_LC || name == firstStraightName_LC ||
                   name == lastStraightName_LC;
        }

        private void clearExamplePrefabs()
        {
            _ep_InnerCorner                 = null;
            _ep_OuterCorner                 = null;
            _ep_MidStraight                 = null;
            _ep_StraightToOuter               = null;
            _ep_StraightToInner                = null;
            _ep_PillarInnerCornerBegin      = null;
            _ep_PillarInnerCornerEnd        = null;
            _ep_PillarOuterCornerBegin      = null;
            _ep_PillarOuterCornerEnd        = null;
            _ep_PillarInnerCornerMiddle     = null;
            _ep_PillarOuterCornerMiddle     = null;
        }

        private bool validateExamplePrefab(GameObject examplePrefab)
        {
            if (examplePrefab.isSceneObject())
            {
                Debug.LogError("ModularWallPrefabProfile: Scene objects are not allowed.");
                return false;
            }

            bool foundInnerChild            = false;
            bool foundOuterChild            = false;
            bool foundMiddleStraightChild   = false;
            bool foundFirstStraightChild    = false;
            bool foundLastStraightChild     = false;

            examplePrefab.getAllChildren(false, false, _objectBuffer);
            foreach (var child in _objectBuffer)
            {
                string childName = child.name.ToLower();

                if (!foundInnerChild)
                {
                    if (childName == innerCornerName_LC)
                    {
                        foundInnerChild = true;
                        if (!child.hierarchyHasMesh(false, false))
                        {
                            showHierarchyNoMeshMessage(child.name);
                            return false;
                        }
                        continue;
                    }
                }
                if (!foundOuterChild)
                {
                    if (childName == outerCornerName_LC)
                    {
                        foundOuterChild = true;
                        if (!child.hierarchyHasMesh(false, false))
                        {
                            showHierarchyNoMeshMessage(child.name);
                            return false;
                        }
                        continue;
                    }
                }
                if (!foundMiddleStraightChild)
                {
                    if (childName == middleStraightName_LC)
                    {
                        foundMiddleStraightChild = true;
                        if (!child.hierarchyHasMesh(false, false))
                        {
                            showHierarchyNoMeshMessage(child.name);
                            return false;
                        }

                        // Note: Why was this necessary. Seems to work fine with non-zero rotation.
                        // Note: Must have 0 rotation along all axes.
                        //Vector3 rotation = child.transform.rotation.eulerAngles;
                       /* if (rotation != Vector3.zero)
                        {
                            Debug.LogError("ModularWallPrefabProfile: Middle Straight piece requires 0 rotation around all axes.");
                            return false;
                        }*/

                        continue;
                    }
                }
                if (!foundFirstStraightChild)
                {
                    if (childName == firstStraightName_LC || childName == straightToOuterName_LC)
                    {
                        foundFirstStraightChild = true;
                        if (!child.hierarchyHasMesh(false, false))
                        {
                            showHierarchyNoMeshMessage(child.name);
                            return false;
                        }
                            
                        continue;
                    }
                }
                if (!foundLastStraightChild)
                {
                    if (childName == lastStraightName_LC || childName == straightToInnerName_LC)
                    {
                        foundLastStraightChild = true;
                        if (!child.hierarchyHasMesh(false, false))
                        {
                            showHierarchyNoMeshMessage(child.name);
                            return false;
                        }
                            
                        continue;
                    }
                }
            }

            if (!foundMiddleStraightChild)
            {
                Debug.LogError("ModularWallPrefabProfile: Middle Straight piece is missing from example prefab.");
                return false;
            }
            if (!foundFirstStraightChild)
            {
                Debug.LogError("ModularWallPrefabProfile: First Straight piece is missing from example prefab.");
                return false;
            }
            if (!foundLastStraightChild)
            {
                Debug.LogError("ModularWallPrefabProfile: Last Straight piece is missing from example prefab.");
                return false;
            }

            return true;
        }

        private void showHierarchyNoMeshMessage(string hierarchyName)
        {
            Debug.LogError("ModularWallPrefabProfile: Invalid hierarchy '" + hierarchyName + "'. The hierarchy doesn't contain a mesh.");
        }

        private void updateRelationships()
        {
            if (_examplePrefab == null)
            {
                _innerCornerRT.reset();
                _outerCornerRT.reset();
                _straightRT.reset();
                _pillarMidStraightBeginRT.reset();
                _pillarMidStraightEndRT.reset();
                _pillarInnerCornerBeginRT.reset();
                _pillarInnerCornerEndRT.reset();
                _pillarOuterCornerBeginRT.reset();
                _pillarOuterCornerEndRT.reset();
                _pillarInnerCornerMiddleRT.reset();
                _pillarOuterCornerMiddleRT.reset();

                _straight_ForwardPushDistance_Inner     = 0.0f;
                _straight_InnerPushDistance_Inner       = 0.0f;
                _straight_ForwardPushDistance_Outer     = 0.0f;
                _straight_InnerPushDistance_Outer       = 0.0f;

                return;
            }

            OBB midStraightOBB                      = calcStraightWallOBB(_ep_MidStraight);
            Quaternion inverseRotation              = Quaternion.Inverse(midStraightOBB.rotation);
            _straightRT.position                    = inverseRotation * (_ep_MidStraight.transform.position - midStraightOBB.center);
            _straightRT.rotation                    = inverseRotation * _ep_MidStraight.transform.rotation;

            if (hasInnerOuterCorners)
            {
                calcInnerCornerRelationships();
                calcOuterCornerRelationships();
            }

            if (hasPillars) calcPillarRelationships();

            Vector3 middleStraightFwAxis            = getModularWallForwardAxis(_ep_MidStraight);
            OBB lastStraightOBB                     = calcStraightWallOBB(_ep_StraightToInner);
            _straight_ForwardPushDistance_Inner     = Vector3Ex.absDot((lastStraightOBB.center - midStraightOBB.center), middleStraightFwAxis);
            _straight_ForwardPushDistance_Inner     -= _wallProperties.forwardSize;
            _straight_ForwardPushDistance_Inner     = MathEx.roundCorrectError(_straight_ForwardPushDistance_Inner, 1e-5f);

            Vector3 pushAxis                        = -getModularWallInnerAxis(_ep_MidStraight);
            Vector3 pushedCenter                    = lastStraightOBB.center + pushAxis * _wallProperties.forwardSize;
            _straight_InnerPushDistance_Inner       = Vector3Ex.absDot((pushedCenter - midStraightOBB.center), pushAxis);
            _straight_InnerPushDistance_Inner       = MathEx.roundCorrectError(_straight_InnerPushDistance_Inner, 1e-5f);

            OBB firstStraightOBB                    = calcStraightWallOBB(_ep_StraightToOuter);
            _straight_ForwardPushDistance_Outer     = Vector3Ex.absDot((firstStraightOBB.center - midStraightOBB.center), middleStraightFwAxis);
            _straight_ForwardPushDistance_Outer     -= _wallProperties.forwardSize;
            _straight_ForwardPushDistance_Outer     = MathEx.roundCorrectError(_straight_ForwardPushDistance_Outer, 1e-5f);
           
            pushAxis                                = getModularWallInnerAxis(_ep_MidStraight);
            pushedCenter                            = firstStraightOBB.center + pushAxis * _wallProperties.forwardSize;
            _straight_InnerPushDistance_Outer       = Vector3Ex.absDot((pushedCenter - midStraightOBB.center), pushAxis);
            _straight_InnerPushDistance_Outer       = MathEx.roundCorrectError(_straight_InnerPushDistance_Outer, 1e-5f);
        }

        private void calcInnerCornerRelationships()
        {
            OBB lastStraightOBB             = calcStraightWallOBB(_ep_StraightToInner);
            Vector3 lastFWAxis              = getModularWallForwardAxis(_ep_StraightToInner);
            OBB midStraightOBB              = calcStraightWallOBB(_ep_MidStraight);
            Vector3 midFWAxis               = getModularWallForwardAxis(_ep_MidStraight);

            // Note: Average inner axes (forward axes transposed).
            Vector3 avgAxis                 = (lastFWAxis - midFWAxis).normalized;

            lastStraightOBB.center          -= lastFWAxis * _wallProperties.forwardSize;
            midStraightOBB.center           += midFWAxis * _wallProperties.forwardSize;
            Vector3 avgCenter               = (lastStraightOBB.center + midStraightOBB.center) / 2.0f;

            Quaternion inverseRotation      = Quaternion.Inverse(Quaternion.LookRotation(avgAxis, getModularWallUpAxis(_ep_MidStraight)));
            _innerCornerRT.position         = inverseRotation * (_ep_InnerCorner.transform.position - avgCenter);
            _innerCornerRT.rotation         = inverseRotation * _ep_InnerCorner.transform.rotation;
        }

        private void calcOuterCornerRelationships()
        {
            OBB firstStraightOBB            = calcStraightWallOBB(_ep_StraightToOuter);
            Vector3 firstFWAxis             = getModularWallForwardAxis(_ep_StraightToOuter);
            OBB midStraightOBB              = calcStraightWallOBB(_ep_MidStraight);
            Vector3 midFWAxis               = getModularWallForwardAxis(_ep_MidStraight);

            // Note: Average inner axes (forward axes transposed).
            Vector3 avgAxis                 = (firstFWAxis - midFWAxis).normalized;

            firstStraightOBB.center         += firstFWAxis * _wallProperties.forwardSize;
            midStraightOBB.center           -= midFWAxis * _wallProperties.forwardSize;
            Vector3 avgCenter               = (firstStraightOBB.center + midStraightOBB.center) / 2.0f;

            Quaternion inverseRotation      = Quaternion.Inverse(Quaternion.LookRotation(avgAxis, getModularWallUpAxis(_ep_MidStraight)));
            _outerCornerRT.position         = inverseRotation * (_ep_OuterCorner.transform.position - avgCenter);
            _outerCornerRT.rotation         = inverseRotation * _ep_OuterCorner.transform.rotation;
        }

        private void calcPillarRelationships()
        {
            if (_ep_PillarInnerCornerBegin != null)
            {
                Quaternion inverseRotation          = Quaternion.Inverse(_ep_MidStraight.transform.rotation);
                _pillarMidStraightEndRT.position    = inverseRotation * (_ep_PillarInnerCornerBegin.transform.position - _ep_MidStraight.transform.position);
                _pillarMidStraightEndRT.rotation    = inverseRotation * _ep_PillarInnerCornerBegin.transform.rotation;
            }
            else _pillarMidStraightEndRT.reset();

            if (_ep_PillarOuterCornerEnd != null)
            {
                Quaternion inverseRotation              = Quaternion.Inverse(_ep_MidStraight.transform.rotation);
                _pillarMidStraightBeginRT.position      = inverseRotation * (_ep_PillarOuterCornerEnd.transform.position - _ep_MidStraight.transform.position);
                _pillarMidStraightBeginRT.rotation      = inverseRotation * _ep_PillarOuterCornerEnd.transform.rotation;
            }
            else _pillarMidStraightBeginRT.reset();

            if (_ep_PillarInnerCornerBegin != null)
            {
                if (_ep_InnerCorner != null)
                {
                    Quaternion inverseRotation          = Quaternion.Inverse(_ep_InnerCorner.transform.rotation);
                    _pillarInnerCornerBeginRT.position  = inverseRotation * (_ep_PillarInnerCornerBegin.transform.position - _ep_InnerCorner.transform.position);
                    _pillarInnerCornerBeginRT.rotation  = inverseRotation * _ep_PillarInnerCornerBegin.transform.rotation;
                }
                else
                {
                    // When no inner corner prefab is available, calculate the transform relative to the first imaginary corner part.
                    GameObject firstPart, secondPart;
                    getImaginaryInnerCornerParts(out firstPart, out secondPart);

                    Quaternion inverseRotation          = Quaternion.Inverse(firstPart.transform.rotation);
                    _pillarInnerCornerBeginRT.position  = inverseRotation * (_ep_PillarInnerCornerBegin.transform.position - calcStraightWallOBB(firstPart).center);
                    _pillarInnerCornerBeginRT.rotation  = inverseRotation * _ep_PillarInnerCornerBegin.transform.rotation;
                }
            }
            else _pillarInnerCornerBeginRT.reset();

            if (_ep_PillarInnerCornerEnd != null)
            {
                if (_ep_InnerCorner != null)
                {
                    Quaternion inverseRotation          = Quaternion.Inverse(_ep_InnerCorner.transform.rotation);
                    _pillarInnerCornerEndRT.position    = inverseRotation * (_ep_PillarInnerCornerEnd.transform.position - _ep_InnerCorner.transform.position);
                    _pillarInnerCornerEndRT.rotation    = inverseRotation * _ep_PillarInnerCornerEnd.transform.rotation;
                }
                else
                {
                    // When no inner corner prefab is available, calculate the transform relative to the second imaginary corner part.
                    GameObject firstPart, secondPart;
                    getImaginaryInnerCornerParts(out firstPart, out secondPart);

                    Quaternion inverseRotation          = Quaternion.Inverse(secondPart.transform.rotation);
                    _pillarInnerCornerEndRT.position    = inverseRotation * (_ep_PillarInnerCornerEnd.transform.position - calcStraightWallOBB(secondPart).center);
                    _pillarInnerCornerEndRT.rotation    = inverseRotation * _ep_PillarInnerCornerEnd.transform.rotation;
                }
            }
            else _pillarInnerCornerEndRT.reset();

            if (_ep_PillarOuterCornerBegin != null)
            {
                if (_ep_OuterCorner != null)
                {
                    Quaternion inverseRotation          = Quaternion.Inverse(_ep_OuterCorner.transform.rotation);
                    _pillarOuterCornerBeginRT.position  = inverseRotation * (_ep_PillarOuterCornerBegin.transform.position - _ep_OuterCorner.transform.position);
                    _pillarOuterCornerBeginRT.rotation  = inverseRotation * _ep_PillarOuterCornerBegin.transform.rotation;
                }
                else
                {
                    GameObject firstPart, secondPart;
                    getImaginaryOuterCornerParts(out firstPart, out secondPart);

                    Quaternion inverseRotation          = Quaternion.Inverse(firstPart.transform.rotation);
                    _pillarOuterCornerBeginRT.position  = inverseRotation * (_ep_PillarOuterCornerBegin.transform.position - calcStraightWallOBB(firstPart).center);
                    _pillarOuterCornerBeginRT.rotation  = inverseRotation * _ep_PillarOuterCornerBegin.transform.rotation;
                }
            }
            else _pillarOuterCornerBeginRT.reset();

            if (_ep_PillarOuterCornerEnd != null)
            {
                if (_ep_OuterCorner != null)
                {
                    Quaternion inverseRotation          = Quaternion.Inverse(_ep_OuterCorner.transform.rotation);
                    _pillarOuterCornerEndRT.position    = inverseRotation * (_ep_PillarOuterCornerEnd.transform.position - _ep_OuterCorner.transform.position);
                    _pillarOuterCornerEndRT.rotation    = inverseRotation * _ep_PillarOuterCornerEnd.transform.rotation;
                }
                else
                {
                    GameObject firstPart, secondPart;
                    getImaginaryOuterCornerParts(out firstPart, out secondPart);

                    Quaternion inverseRotation          = Quaternion.Inverse(secondPart.transform.rotation);
                    _pillarOuterCornerEndRT.position    = inverseRotation * (_ep_PillarOuterCornerEnd.transform.position - calcStraightWallOBB(secondPart).center);
                    _pillarOuterCornerEndRT.rotation    = inverseRotation * _ep_PillarOuterCornerEnd.transform.rotation;
                }
            }
            else _pillarOuterCornerEndRT.reset();

            if (_ep_PillarInnerCornerMiddle != null)
            {
                if (_ep_InnerCorner != null)
                {
                    Quaternion inverseRotation              = Quaternion.Inverse(_ep_InnerCorner.transform.rotation);
                    _pillarInnerCornerMiddleRT.position     = inverseRotation * (_ep_PillarInnerCornerMiddle.transform.position - _ep_InnerCorner.transform.position);
                    _pillarInnerCornerMiddleRT.rotation     = inverseRotation * _ep_PillarInnerCornerMiddle.transform.rotation;
                }
                else
                {
                    GameObject firstPart, secondPart;
                    getImaginaryInnerCornerParts(out firstPart, out secondPart);
                    OBB lastStraightOBB     = calcStraightWallOBB(_ep_StraightToInner);
                    OBB middleStraightOBB   = calcStraightWallOBB(_ep_MidStraight);
                    OBB firstPartOBB        = calcStraightWallOBB(firstPart);
                    OBB secondPartOBB       = calcStraightWallOBB(secondPart);

                    Vector3 parentPosition  = (firstPartOBB.center + secondPartOBB.center) * 0.5f;
                    Vector3 look            = (lastStraightOBB.center - secondPartOBB.center).normalized;
                    Vector3 right           = (middleStraightOBB.center - firstPartOBB.center).normalized;
                    Vector3 up              = Vector3.Cross(look, right).normalized;

                    Quaternion inverseRotation              = Quaternion.Inverse(Quaternion.LookRotation(look, up));
                    _pillarInnerCornerMiddleRT.position     = inverseRotation * (_ep_PillarInnerCornerMiddle.transform.position - parentPosition);
                    _pillarInnerCornerMiddleRT.rotation     = inverseRotation * _ep_PillarInnerCornerMiddle.transform.rotation;
                }
            }
            else _pillarInnerCornerMiddleRT.reset();

            if (_ep_PillarOuterCornerMiddle != null)
            {
                if (_ep_OuterCorner != null)
                {
                    Quaternion inverseRotation          = Quaternion.Inverse(_ep_OuterCorner.transform.rotation);
                    _pillarOuterCornerMiddleRT.position = inverseRotation * (_ep_PillarOuterCornerMiddle.transform.position - _ep_OuterCorner.transform.position);
                    _pillarOuterCornerMiddleRT.rotation = inverseRotation * _ep_PillarOuterCornerMiddle.transform.rotation;
                }
                else
                {
                    GameObject firstPart, secondPart;
                    getImaginaryOuterCornerParts(out firstPart, out secondPart);
                    OBB firstStraightOBB    = calcStraightWallOBB(_ep_StraightToOuter);
                    OBB middleStraightOBB   = calcStraightWallOBB(_ep_MidStraight);
                    OBB firstPartOBB        = calcStraightWallOBB(firstPart);
                    OBB secondPartOBB       = calcStraightWallOBB(secondPart);

                    Vector3 parentPosition  = (firstPartOBB.center + secondPartOBB.center) * 0.5f;
                    Vector3 look            = (firstStraightOBB.center - firstPartOBB.center).normalized;
                    Vector3 right           = (middleStraightOBB.center - secondPartOBB.center).normalized;
                    Vector3 up              = Vector3.Cross(look, right).normalized;                   

                    Quaternion inverseRotation              = Quaternion.Inverse(Quaternion.LookRotation(look, up));
                    _pillarOuterCornerMiddleRT.position     = inverseRotation * (_ep_PillarOuterCornerMiddle.transform.position - parentPosition);
                    _pillarOuterCornerMiddleRT.rotation     = inverseRotation * _ep_PillarOuterCornerMiddle.transform.rotation;
                }
            }
            else _pillarOuterCornerMiddleRT.reset();
        }

        private void getImaginaryInnerCornerParts(out GameObject firstPart, out GameObject secondPart)
        {
            firstPart   = null;
            secondPart  = null;
            if (hasInnerOuterCorners) return;

            _examplePrefab.getAllChildren(false, false, _objectBuffer);
            var lastStraight    = getWallPieceByName(lastStraightName_LC, _objectBuffer);
            var lastStraightOBB = calcStraightWallOBB(lastStraight);
            lastStraightOBB.inflate(1e-2f);
            foreach (var piece in _objectBuffer)
            {
                if (isWallPiecePillar(piece) || isWallPieceStraight(piece)) continue;

                OBB pieceOBB = ObjectBounds.calcHierarchyWorldOBB(piece, getWallBoundsQConfig());
                if (!pieceOBB.isValid) continue;

                if (pieceOBB.intersectsOBB(lastStraightOBB))
                {
                    secondPart = piece;
                    break;
                }
            }

            if (secondPart == null) return;

            var secondPartOBB = ObjectBounds.calcHierarchyWorldOBB(secondPart, getWallBoundsQConfig());
            foreach (var piece in _objectBuffer)
            {
                if (isWallPiecePillar(piece) || isWallPieceStraight(piece) || piece == secondPart) continue;

                OBB pieceOBB = ObjectBounds.calcHierarchyWorldOBB(piece, getWallBoundsQConfig());
                if (!pieceOBB.isValid) continue;

                if (pieceOBB.intersectsOBB(secondPartOBB))
                {
                    firstPart = piece;
                    break;
                }
            }
        }

        private void getImaginaryOuterCornerParts(out GameObject firstPart, out GameObject secondPart)
        {
            firstPart   = null;
            secondPart  = null;
            if (hasInnerOuterCorners) return;

            _examplePrefab.getAllChildren(false, false, _objectBuffer);
            var firstStraight       = getWallPieceByName(firstStraightName_LC, _objectBuffer);
            var firstStraightOBB    = calcStraightWallOBB(firstStraight);
            firstStraightOBB.inflate(1e-2f);
            foreach (var piece in _objectBuffer)
            {
                if (isWallPiecePillar(piece) || isWallPieceStraight(piece)) continue;

                OBB pieceOBB = ObjectBounds.calcHierarchyWorldOBB(piece, getWallBoundsQConfig());
                if (!pieceOBB.isValid) continue;

                if (pieceOBB.intersectsOBB(firstStraightOBB))
                {
                    firstPart = piece;
                    break;
                }
            }

            if (firstPart == null) return;

            var firstPartOBB = ObjectBounds.calcHierarchyWorldOBB(firstPart, getWallBoundsQConfig());
            foreach (var piece in _objectBuffer)
            {
                if (isWallPiecePillar(piece) || isWallPieceStraight(piece) || piece == firstPart) continue;

                OBB pieceOBB = ObjectBounds.calcHierarchyWorldOBB(piece, getWallBoundsQConfig());
                if (!pieceOBB.isValid) continue;

                if (pieceOBB.intersectsOBB(firstPartOBB))
                {
                    secondPart = piece;
                    break;
                }
            }
        }

        private void detectForwardAxis()
        {
            // Note: Requires wall pieces which have the forward size larger then the inner size.
            // Note: Don't use 'calcStraightWallOBB' here because it uses info which is not yet available.
            OBB middleStraightOBB               = ObjectBounds.calcHierarchyWorldOBB(_ep_MidStraight, getWallBoundsQConfig());
            Vector3 obbSize                     = middleStraightOBB.size;

            int a0                              = ((int)_wallProperties.upAxis + 1) % 3;
            int a1                              = ((int)_wallProperties.upAxis + 2) % 3;
            int fwAxisIndex                     = obbSize[a0] < obbSize[a1] ? a1 : a0;

            _wallProperties.forwardAxis         = (ModularWallAxis)fwAxisIndex;
            _wallProperties.invertForwardAxis   = false;

            Vector3 fwAxis  = _ep_MidStraight.transform.getLocalAxis(fwAxisIndex);
            Vector3 vec     = _ep_StraightToInner.transform.position - _ep_MidStraight.transform.position;
            if (Vector3.Dot(vec, fwAxis) < 0.0f) _wallProperties.invertForwardAxis = true;
        }

        private void detectInnerAxis()
        {
            int fwAxisIndex = (int)wallForwardAxis;
            int upAxisIndex = (int)wallUpAxis;

            if (Mathf.Abs(fwAxisIndex - upAxisIndex) == 2) _wallProperties.innerAxis = ModularWallAxis.Y;
            else
            {
                if (fwAxisIndex > upAxisIndex) _wallProperties.innerAxis = (ModularWallAxis)((fwAxisIndex + 1) % 3);
                else _wallProperties.innerAxis = (ModularWallAxis)((upAxisIndex + 1) % 3);
            }

            _wallProperties.invertInnerAxis = false;
            Vector3 vec = _ep_StraightToInner.transform.position - _ep_MidStraight.transform.position;
            if (Vector3.Dot(vec, getModularWallInnerAxis(_ep_MidStraight)) < 0.0f) _wallProperties.invertInnerAxis = !_wallProperties.invertInnerAxis;
        }

        private void updateWallProperties()
        {
            if (_examplePrefab == null)
            {
                _wallProperties.height      = 0.0f;
                _wallProperties.forwardSize = 0.0f;
                return;
            }

            detectForwardAxis();
            detectInnerAxis();

            GameObject straightPrefab   = _ep_MidStraight;
            Vector3 modelSize           = PrefabDataDb.instance.getData(straightPrefab).modelSize;

            _wallProperties.height      = modelSize[(int)_wallProperties.upAxis];

            if (_truncateForwardSize) _wallProperties.forwardSize = (int)modelSize[(int)_wallProperties.forwardAxis];
            else _wallProperties.forwardSize = modelSize[(int)_wallProperties.forwardAxis];
        }

        private void OnEnable()
        {
            initializeTableArray();
            initializeRulePrefabListArray();
            refreshAllPrefabTables();

            Undo.undoRedoPerformed += onUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= onUndoRedo;
        }

        private void onUndoRedo()
        {
            refreshAllPrefabTables();
        }

        private void OnDestroy()
        {
            deleteAllPrefabs();
        }
    }
}
#endif