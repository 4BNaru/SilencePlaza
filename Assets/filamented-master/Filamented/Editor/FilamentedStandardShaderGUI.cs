// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
using System;
using UnityEngine;
using UnityEditor;

namespace SilentTools
{
    class FilamentedStandardShaderGUI : ShaderGUI
    {
        private enum WorkflowMode
        {
            Specular,
            Metallic,
            Dielectric,
            Roughness
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        public enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha,
        }

        private static class Styles
        {
            public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");

            public static GUIContent albedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent specularMapText = EditorGUIUtility.TrTextContent("Specular", "Specular (RGB) and Smoothness (A)");
            public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static GUIContent roughnessText = EditorGUIUtility.TrTextContent("Roughness", "Roughness value");
            public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
            public static GUIContent smoothnessScaleText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness scale factor");
            public static GUIContent smoothnessMapChannelText = EditorGUIUtility.TrTextContent("Source", "Smoothness texture and channel");
            public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
            public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
            public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
            public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
            public static GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");
            public static GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
            public static GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
            public static GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static GUIContent cullModeText = EditorGUIUtility.TrTextContent("Cull Mode", "Which face of the polygon should be culled from rendering");

            public static GUIContent exposureOcclusionText = EditorGUIUtility.TrTextContent("Exposure Occlusion", "Controls occlusion of specular lighting by the lightmap");
            public static GUIContent lightmapSpecularText = EditorGUIUtility.TrTextContent("Lightmap Specular", "Allows the material to derive specular lighting from the lightmap directionality.");
            public static GUIContent lmSpecMaxSmoothnessText = EditorGUIUtility.TrTextContent("Specular Smoothness Mod", "Adjusts the maximum smoothness of the material for lightmap specular to avoid artifacts from imprecise directionality.");

            public static string primaryMapsText = "Main Maps";
            public static string secondaryMapsText = "Secondary Maps";
            public static string forwardText = "Forward Rendering Options";
            public static string renderingMode = "Rendering Mode";
            public static string advancedText = "Advanced Options";
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
        }

        MaterialProperty blendMode = null;
        MaterialProperty albedoMap = null;
        MaterialProperty albedoColor = null;
        MaterialProperty alphaCutoff = null;
        MaterialProperty specularMap = null;
        MaterialProperty specularColor = null;
        MaterialProperty metallicMap = null;
        MaterialProperty metallic = null;
        MaterialProperty roughness = null;
        MaterialProperty roughnessMap = null;
        MaterialProperty smoothness = null;
        MaterialProperty smoothnessScale = null;
        MaterialProperty smoothnessMapChannel = null;
        MaterialProperty highlights = null;
        MaterialProperty reflections = null;
        MaterialProperty bumpScale = null;
        MaterialProperty bumpMap = null;
        MaterialProperty occlusionStrength = null;
        MaterialProperty occlusionMap = null;
        MaterialProperty heigtMapScale = null;
        MaterialProperty heightMap = null;
        MaterialProperty emissionColorForRendering = null;
        MaterialProperty emissionMap = null;
        MaterialProperty detailMask = null;
        MaterialProperty detailAlbedoMap = null;
        MaterialProperty detailNormalMapScale = null;
        MaterialProperty detailNormalMap = null;
        MaterialProperty uvSetSecondary = null;
        MaterialProperty cullMode = null;
        MaterialProperty exposureOcclusion = null;
        MaterialProperty lightmapSpecular = null;
        MaterialProperty lmSpecMaxSmoothness = null;

        MaterialEditor m_MaterialEditor;
        WorkflowMode m_WorkflowMode = WorkflowMode.Specular;

        bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            albedoMap = FindProperty("_MainTex", props);
            albedoColor = FindProperty("_Color", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            specularMap = FindProperty("_SpecGlossMap", props, false);
            specularColor = FindProperty("_SpecColor", props, false);
            metallicMap = FindProperty("_MetallicGlossMap", props, false);
            metallic = FindProperty("_Metallic", props, false);
            // todo: find a better way to handle this
            if (specularMap != null && specularMap.displayName == "Roughness Map") 
                m_WorkflowMode = WorkflowMode.Roughness;
            else if (specularMap != null && specularColor != null)
                m_WorkflowMode = WorkflowMode.Specular;
            else if (metallicMap != null && metallic != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
            roughness = FindProperty("_Glossiness", props);
            roughnessMap = FindProperty("_SpecGlossMap", props, false);
            smoothness = FindProperty("_Glossiness", props);
            smoothnessScale = FindProperty("_GlossMapScale", props, false);
            smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);
            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            heigtMapScale = FindProperty("_Parallax", props);
            heightMap = FindProperty("_ParallaxMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            detailMask = FindProperty("_DetailMask", props);
            detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
            detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
            detailNormalMap = FindProperty("_DetailNormalMap", props);
            uvSetSecondary = FindProperty("_UVSec", props);
            cullMode = FindProperty("_CullMode", props);

            exposureOcclusion = FindProperty("_ExposureOcclusion", props, false);
            lightmapSpecular = FindProperty("_LightmapSpecular", props, false);
            lmSpecMaxSmoothness = FindProperty("_LightmapSpecularMaxSmoothness", props, false);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material, m_WorkflowMode, false);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            bool blendModeChanged = false;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                blendModeChanged = BlendModePopup();

                // Primary properties
                GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
                DoAlbedoArea(material);
                DoSpecularMetallicArea();
                DoNormalArea();
                m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue != null ? heigtMapScale : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
                DoEmissionArea(material);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                if (EditorGUI.EndChangeCheck())
                    emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake

                EditorGUILayout.Space();

                // Secondary properties
                GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
                m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
                m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

                // Third properties
                GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
                if (highlights != null)
                    m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                if (reflections != null)
                    m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);

                EditorGUILayout.Space();

                // Added properties
                if (exposureOcclusion != null)
                    m_MaterialEditor.ShaderProperty(exposureOcclusion, Styles.exposureOcclusionText);
                if (lightmapSpecular != null)
                    m_MaterialEditor.ShaderProperty(lightmapSpecular, Styles.lightmapSpecularText);
                if (lmSpecMaxSmoothness != null)
                    m_MaterialEditor.ShaderProperty(lmSpecMaxSmoothness, Styles.lmSpecMaxSmoothnessText);

                EditorGUILayout.Space();

                GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);

                m_MaterialEditor.RenderQueueField();
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj, m_WorkflowMode, blendModeChanged);
            }

            m_MaterialEditor.ShaderProperty(cullMode, Styles.cullModeText.text);

            m_MaterialEditor.EnableInstancingField();
            m_MaterialEditor.DoubleSidedGIField();
        }

        internal void DetermineWorkflow(MaterialProperty[] props)
        {
            if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
                m_WorkflowMode = WorkflowMode.Specular;
                if (FindProperty("_SpecGlossMap", props, false).displayName == "Roughness Map") 
                    m_WorkflowMode = WorkflowMode.Roughness; 
            else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"), true);
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[] { material }));
            MaterialChanged(material, m_WorkflowMode, true);
        }

        bool BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            bool result = EditorGUI.EndChangeCheck();
            if (result)
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;

            return result;
        }

        void DoNormalArea()
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
            if (bumpScale.floatValue != 1
                && UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget))
                if (m_MaterialEditor.HelpBoxWithButton(
                    EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
                    EditorGUIUtility.TrTextContent("Fix Now")))
                {
                    bumpScale.floatValue = 1;
                }
        }

        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
            }
        }

        void DoEmissionArea(Material material)
        {
            // Emission for GI?
            if (m_MaterialEditor.EmissionEnabledProperty())
            {
                bool hadEmissionTexture = emissionMap.textureValue != null;

                // Texture and HDR color controls
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering.colorValue.maxColorComponent;
                if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering.colorValue = Color.white;

                // change the GI flag and fix it up with emissive as black if necessary
                m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
            }
        }

        void DoSpecularMetallicArea()
        {
            if (m_WorkflowMode == WorkflowMode.Roughness)
            {
                m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, metallicMap.textureValue != null ? null : metallic);
                m_MaterialEditor.TexturePropertySingleLine(Styles.roughnessText, roughnessMap, roughnessMap.textureValue != null ? null : roughness);
                return;
            }
            bool hasGlossMap = false;
            if (m_WorkflowMode == WorkflowMode.Specular)
            {
                hasGlossMap = specularMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap, hasGlossMap ? null : specularColor);
            }
            else if (m_WorkflowMode == WorkflowMode.Metallic)
            {
                hasGlossMap = metallicMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, hasGlossMap ? null : metallic);
            }

            bool showSmoothnessScale = hasGlossMap;
            if (smoothnessMapChannel != null)
            {
                int smoothnessChannel = (int)smoothnessMapChannel.floatValue;
                if (smoothnessChannel == (int)SmoothnessMapChannel.AlbedoAlpha)
                    showSmoothnessScale = true;
            }

            int indentation = 2; // align with labels of texture properties
            m_MaterialEditor.ShaderProperty(showSmoothnessScale ? smoothnessScale : smoothness, showSmoothnessScale ? Styles.smoothnessScaleText : Styles.smoothnessText, indentation);

            ++indentation;
            if (smoothnessMapChannel != null)
                m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode, bool overrideRenderQueue)
        {
            int minRenderQueue = -1;
            int maxRenderQueue = 5000;
            int defaultRenderQueue = -1;
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_ZWrite", 1.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = -1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest - 1;
                    defaultRenderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_ZWrite", 1.0f);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }

            if (overrideRenderQueue || material.renderQueue < minRenderQueue || material.renderQueue > maxRenderQueue)
            {
                if (!overrideRenderQueue)
                    Debug.LogFormat("Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", minRenderQueue, maxRenderQueue);
                material.renderQueue = defaultRenderQueue;
            }
        }

        static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;
            else
                return SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
            if (workflowMode == WorkflowMode.Roughness)
            {
                SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
                SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
            } 
            else 
            {
                if (workflowMode == WorkflowMode.Specular)
                    SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
                else if (workflowMode == WorkflowMode.Metallic)
                    SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
            }
            SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
            SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
            }

            // New properties

            SetKeyword(material, "_LIGHTMAPSPECULAR", material.GetFloat("_LightmapSpecular") == 1? true : false);
        }

        static void MaterialChanged(Material material, WorkflowMode workflowMode, bool overrideRenderQueue)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"), overrideRenderQueue);

            SetMaterialKeywords(material, workflowMode);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
} // namespace SilentTools