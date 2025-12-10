#if UNITY_EDITOR
using UnityEngine;

namespace GSPAWN
{
    public class TerrainObjectOverlapFilter
    {
        public bool filterObject(GameObject gameObject)
        {
            return true;
        }
    }
}
#endif