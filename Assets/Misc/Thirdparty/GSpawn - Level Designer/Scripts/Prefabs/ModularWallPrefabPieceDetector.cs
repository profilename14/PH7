#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace GSPAWN
{
    public static class ModularWallPrefabPieceDetector
    {
        class WallPiece
        {
            public GameObject   gameObject;
            public OBB          obb = OBB.getInvalid();
            public PluginPrefab pluginPrefab;
        }
        static List<WallPiece> _wallPieceBuffer = new List<WallPiece>();

        public static bool detectWallPieces(GameObject parent)
        {
            // We will have to detect based on the number of children:
            //  -5 children => first/middle/last straight + inner corner + outer corner.
            //  -other      => unknown config.
            int childCount = parent.transform.childCount;
            switch (childCount)
            {
                case 5:  return detect_5_Pieces(parent);
                default: return false;
            }
        }

        static bool detect_5_Pieces(GameObject parent)
        {
            // Do we already have the wall pieces in place
            if (parent.hasDirectChild(ModularWallPrefabProfile.innerCornerName_LC, true) &&
                parent.hasDirectChild(ModularWallPrefabProfile.outerCornerName_LC, true) &&
                (parent.hasDirectChild(ModularWallPrefabProfile.straightToOuterName_LC, true) || 
                 parent.hasDirectChild(ModularWallPrefabProfile.firstStraightName_LC, true)) &&
                (parent.hasDirectChild(ModularWallPrefabProfile.straightToInnerName_LC, true) || 
                 parent.hasDirectChild(ModularWallPrefabProfile.lastStraightName_LC, true)) &&
                parent.hasDirectChild(ModularWallPrefabProfile.middleStraightName_LC, true)) return true;

            // Extract the wall piece objects
            if (!extractChildWallPieces(parent, _wallPieceBuffer))
                return false;

            // Calculate the OBB of the entire hierarchy
            OBB hierarchyOBB = ObjectBounds.calcHierarchyWorldOBB(parent, ModularWallPrefabProfile.getWallBoundsQConfig());
            if (!hierarchyOBB.isValid)
            {
                Debug.LogError("Failed to calculate the bounding volume for the child wall pieces. Wall pieces must be mesh objects.");
                return false;
            }

            // Now parse the wall piece objects. We will first identify the middle straight piece.
            // This is the piece whose OBB center is closest to the center of the hierarchy OBB
            // ALONG the LARGEST OBB AXIS.
            Vector3 obbSize         = hierarchyOBB.size;
            Vector3 obbCenter       = hierarchyOBB.center;
            Vector3 largestAxis     = hierarchyOBB.getAxis(obbSize.getMaxAbsCompIndex());
            float dMin              = float.MaxValue;
            WallPiece midStraight   = null;
            foreach (var piece in _wallPieceBuffer)
            {
                // Calculate distance from OBB center along longest axis
                float d = (piece.obb.center - obbCenter).absDot(largestAxis);
                if (d < dMin)
                {
                    dMin = d;
                    midStraight = piece;
                }
            }

            // Make sure we have a straight piece
            if (midStraight == null)
            {
                Debug.LogError("Missing Middle Straight piece. Please make sure all required wall pieces exist.");
                return false;
            }

            // Name the mid straight piece
            midStraight.gameObject.name = ModularWallPrefabProfile.middleStraightName;

            // Find the inner corner prefab based on tag
            WallPiece innerCorner = null;
            foreach (var piece in _wallPieceBuffer)
            {
                // Check for match
                if ((piece.pluginPrefab.tags & PluginPrefabTags.WallInnerCorner) != 0)
                {
                    innerCorner = piece;
                    break;
                }
            }

            // No inner corner?
            if (innerCorner == null)
            {
                Debug.LogError("Missing Inner Corner piece. Please make sure all required wall pieces exist.");
                return false;
            }

            // Name the inner corner piece
            innerCorner.gameObject.name = ModularWallPrefabProfile.innerCornerName;

            // Now find the outer corner. The object whose OBB center is closest
            // to the middle straight piece should be the outer corner.
            dMin = float.MaxValue;
            WallPiece outerCorner = null;
            foreach (var piece in _wallPieceBuffer)
            {
                // Ignore piece we already know about
                if (piece == innerCorner || piece == midStraight)
                    continue;

                // Calculate distance between OBB centers
                float d = (midStraight.obb.center - piece.obb.center).magnitude;

                // If the distance is smaller than what we have so far, update piece
                if (d < dMin)
                {
                    dMin = d;
                    outerCorner = piece;
                }
            }

            // Name the outer corner piece
            outerCorner.gameObject.name = ModularWallPrefabProfile.outerCornerName;

            // The piece closest to the outer corner is the straight-to-outer piece.
            // The piece closest to the inner corner is the straight-to-inner piece.
            WallPiece straightToOuter   = null;
            WallPiece straightToInner   = null;
            float dMinO = float.MaxValue;
            float dMinI = float.MaxValue;
            foreach (var piece in _wallPieceBuffer)
            {
                // Ignore piece we already know about
                if (piece == innerCorner || piece == outerCorner || piece == midStraight)
                    continue;

                // Calculate distance between OBB centers
                float d0 = (outerCorner.obb.center - piece.obb.center).magnitude;
                float d1 = (innerCorner.obb.center - piece.obb.center).magnitude;

                // If the distance is smaller than what we have so far, update piece
                if (d0 < dMinO)
                {
                    dMinO               = d0;
                    straightToOuter     = piece;
                }
                if (d1 < dMinI)
                {
                    dMinI               = d1;
                    straightToInner     = piece;
                }
            }

            // Name the 2 pieces
            straightToOuter.gameObject.name = ModularWallPrefabProfile.straightToOuterName;
            straightToInner.gameObject.name = ModularWallPrefabProfile.straightToInnerName;

            // Make sure we don't have 2 wall pieces referencing the same game object
            int pieceCount = _wallPieceBuffer.Count;
            for (int i = 0; i < pieceCount; ++i)
            {
                WallPiece piece = _wallPieceBuffer[i];
                for (int j = i + 1; j < pieceCount; ++j)
                {
                    WallPiece otherPiece = _wallPieceBuffer[j];
                    if (piece.gameObject == otherPiece.gameObject)
                    {
                        Debug.LogError("Two wall pieces are referencing the same object: " + piece.gameObject.name + ". " + 
                            "Please check wall piece placement and try to remove any ambiguities. If not possible, the wall pieces will have to identified and named manually.");
                        return false;
                    }
                }
            }

            // Success!
            return true;
        }

        static bool extractChildWallPieces(GameObject parent, List<WallPiece> wallPieces)
        {
            wallPieces.Clear();

            int childCount = parent.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                GameObject child = parent.transform.GetChild(i).gameObject;
                OBB obb = ObjectBounds.calcHierarchyWorldOBB(child, ModularWallPrefabProfile.getWallBoundsQConfig());
                if (!obb.isValid)
                {
                    Debug.LogError("One of the child wall pieces has an invalid bounding volume: " + child.name + ". Wall pieces must be mesh objects.");
                    return false;
                }

                GameObject prefabAsset = child.getOutermostPrefabAsset();
                if (prefabAsset == null)
                {
                    Debug.LogError("One of the child wall pieces is not a prefab instance: " + child.name + ".");
                    return false;
                }
                PluginPrefab pluginPrefab = PrefabLibProfileDb.instance.getPrefab(prefabAsset);
                if (pluginPrefab == null)
                {
                    Debug.LogError("One of the child wall pieces is not an instance of a valid plugin prefab: " + child.name + ".");
                    return false;
                }

                WallPiece wallPiece     = new WallPiece();
                wallPiece.gameObject    = child;
                wallPiece.obb           = obb;
                wallPiece.pluginPrefab  = pluginPrefab;
                wallPieces.Add(wallPiece);
            }

            return true;
        }
    }
}
#endif