using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlatKit;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ObjectOutlineEditorUtils {
    private static readonly GUIStyle RichHelpBoxStyle = new(EditorStyles.helpBox) { richText = true };

    public static void SetActive(Material material, bool active) {
        // Work directly with the active URP Renderer Data to avoid dependency on a Camera context.
        var rendererData = GetRendererData();
        if (rendererData == null) {
            const string m = "<b>ScriptableRendererData</b> is required to manage per-object outlines.\n" +
                             "Please assign a <b>URP Asset</b> in the Graphics settings.";
            EditorGUILayout.LabelField(m, RichHelpBoxStyle);
            return;
        }

        // Find existing feature on the renderer data.
        var feature = rendererData.rendererFeatures
            .FirstOrDefault(f => f != null && f.GetType() == typeof(ObjectOutlineRendererFeature));

        // Create feature on demand when enabling outlines.
        if (feature == null && active) {
            feature = ScriptableObject.CreateInstance<ObjectOutlineRendererFeature>();
            feature.name = "Flat Kit Per Object Outline";
            AddRendererFeature(rendererData, feature);

            var addedMsg = $"<color=grey>[Flat Kit]</color> <b>Added</b> <color=green>{feature.name}</color> Renderer " +
                           $"Feature to <b>{rendererData.name}</b>.";
            Debug.Log(addedMsg, rendererData);
        }

        // If disabling and there's no feature, nothing to do.
        if (feature == null) return;

        // Register/unregister the material and check usage.
        if (feature is not ObjectOutlineRendererFeature outlineFeature) {
            Debug.LogError("ObjectOutlineRendererFeature not found");
            return;
        }

        var featureIsUsed = outlineFeature.RegisterMaterial(material, active);

        // Remove the feature asset if no materials are using it anymore.
        if (!featureIsUsed) {
            RemoveRendererFeature(rendererData, feature);
            var removedMsg = $"<color=grey>[Flat Kit]</color> <b>Removed</b> <color=green>{feature.name}</color> Renderer " +
                             $"Feature from <b>{rendererData.name}</b> because no materials are using it.";
            Debug.Log(removedMsg, rendererData);
        }
    }

    private static void AddRendererFeature(ScriptableRendererData rendererData, ScriptableRendererFeature feature) {
        // Save the asset as a sub-asset.
        AssetDatabase.AddObjectToAsset(feature, rendererData);
        rendererData.rendererFeatures.Add(feature);
        rendererData.SetDirty();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void RemoveRendererFeature(ScriptableRendererData rendererData, ScriptableRendererFeature feature) {
        rendererData.rendererFeatures.Remove(feature);
        rendererData.SetDirty();

        // Remove the asset.
        AssetDatabase.RemoveObjectFromAsset(feature);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [CanBeNull]
    private static ScriptableRenderer GetRenderer(Camera camera) {
        if (!camera) {
            return null;
        }

        var additionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (!additionalCameraData) {
            return null;
        }

        var renderer = additionalCameraData.scriptableRenderer;
        return renderer;
    }

    private static List<ScriptableRendererFeature> GetRendererFeatures(ScriptableRenderer renderer) {
        var property =
            typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null) return null;
        var features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
        return features;
    }

    internal static ScriptableRendererData GetRendererData() {
#if UNITY_6000_0_OR_NEWER
        var srpAsset = GraphicsSettings.defaultRenderPipeline;
#else
        var srpAsset = GraphicsSettings.renderPipelineAsset;
#endif
        if (srpAsset == null) {
            const string m = "<b>Flat Kit</b> No SRP asset found. Please assign a URP Asset in the Graphics settings " +
                             "to enable per-object outlines.";
            Debug.LogError(m);
            return null;
        }

        var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var rendererDataList = (ScriptableRendererData[])field!.GetValue(srpAsset);

        var rendererData = rendererDataList.FirstOrDefault();
        if (rendererData == null) {
            Debug.LogError("No ScriptableRendererData found");
            return null;
        }

        return rendererData;
    }
}