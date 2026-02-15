using UnityEditor;
using UnityEngine.Rendering;

namespace FlatKit.Editor {
    internal static class MaterialPropertyExtensions {
        public static ShaderPropertyType GetShaderPropertyType(this MaterialProperty property) {
#if UNITY_6000_0_OR_NEWER
            return property.propertyType;
#else
            return property.type switch {
                MaterialProperty.PropType.Color => ShaderPropertyType.Color,
                MaterialProperty.PropType.Vector => ShaderPropertyType.Vector,
                MaterialProperty.PropType.Float => ShaderPropertyType.Float,
                MaterialProperty.PropType.Range => ShaderPropertyType.Range,
                MaterialProperty.PropType.Texture => ShaderPropertyType.Texture,
                _ => ShaderPropertyType.Float,
            };
#endif
        }

        public static ShaderPropertyFlags GetShaderPropertyFlags(this MaterialProperty property) {
#if UNITY_6000_0_OR_NEWER
            return property.propertyFlags;
#else
            var flags = ShaderPropertyFlags.None;
            if ((property.flags & MaterialProperty.PropFlags.HideInInspector) != 0) {
                flags |= ShaderPropertyFlags.HideInInspector;
            }

            if ((property.flags & MaterialProperty.PropFlags.NoScaleOffset) != 0) {
                flags |= ShaderPropertyFlags.NoScaleOffset;
            }

            if ((property.flags & MaterialProperty.PropFlags.PerRendererData) != 0) {
                flags |= ShaderPropertyFlags.PerRendererData;
            }

            if ((property.flags & MaterialProperty.PropFlags.Normal) != 0) {
                flags |= ShaderPropertyFlags.Normal;
            }

            if ((property.flags & MaterialProperty.PropFlags.HDR) != 0) {
                flags |= ShaderPropertyFlags.HDR;
            }

            if ((property.flags & MaterialProperty.PropFlags.Gamma) != 0) {
                flags |= ShaderPropertyFlags.Gamma;
            }

            if ((property.flags & MaterialProperty.PropFlags.NonModifiableTextureData) != 0) {
                flags |= ShaderPropertyFlags.NonModifiableTextureData;
            }

            return flags;
#endif
        }
    }
}
