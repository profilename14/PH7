using UnityEngine;
using UnityEngine.Rendering;

namespace FlatKit {
public static class RendererFeatureUtils {
    public static void SetKeyword(Material material, string keyword, bool enabled) {
        if (material == null) {
            return;
        }

#if UNITY_2021_2_OR_NEWER
        if (material.shader != null) {
            // Unity 2021.2+ lets us query the shader's keyword space; only call SetKeyword when the symbol is actually declared
            // to avoid "keyword doesn't exist" errors on Unity 6.3.
            var keywordSpace = material.shader.keywordSpace;
            LocalKeyword localKeyword = keywordSpace.FindKeyword(keyword);
            if (localKeyword.isValid) {
                material.SetKeyword(localKeyword, enabled);
                return;
            }
        }
#endif

        if (enabled) {
            material.EnableKeyword(keyword);
        } else {
            material.DisableKeyword(keyword);
        }
    }
}
}