#if UNITY_EDITOR
namespace GSPAWN
{
    public struct TerrainRaycastConfig
    {
        public bool useInterpolatedNormal;

        public static readonly TerrainRaycastConfig defaultConfig = new TerrainRaycastConfig() { useInterpolatedNormal = true };
    }
}
#endif