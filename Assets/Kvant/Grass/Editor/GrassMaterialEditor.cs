//
// Custom material editor for Grass surface shader
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    public class GrassMaterialEditor : ShaderGUI
    {
        MaterialProperty _colorMode;
        MaterialProperty _color;
        MaterialProperty _color2;
        MaterialProperty _metallic;
        MaterialProperty _smoothness;
        MaterialProperty _albedoMap;
        MaterialProperty _normalMap;
        MaterialProperty _normalScale;
        MaterialProperty _occHeight;
        MaterialProperty _occExp;
        MaterialProperty _occToColor;

        static GUIContent _albedoText    = new GUIContent("Albedo");
        static GUIContent _normalMapText = new GUIContent("Normal Map");

        bool _initial = true;

        void FindProperties(MaterialProperty[] props)
        {
            _colorMode   = FindProperty("_ColorMode", props);
            _color       = FindProperty("_Color", props);
            _color2      = FindProperty("_Color2", props);
            _metallic    = FindProperty("_Metallic", props);
            _smoothness  = FindProperty("_Smoothness", props);
            _albedoMap   = FindProperty("_MainTex", props);
            _normalMap   = FindProperty("_NormalMap", props);
            _normalScale = FindProperty("_NormalScale", props);
            _occHeight   = FindProperty("_OccHeight", props);
            _occExp      = FindProperty("_OccExp", props);
            _occToColor  = FindProperty("_OccToColor", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            if (ShaderPropertiesGUI(materialEditor) || _initial)
                foreach (Material m in materialEditor.targets)
                    CompleteMaterialChanges(m);

            _initial = false;
        }

        bool ShaderPropertiesGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();

            materialEditor.ShaderProperty(_colorMode, "Color Mode");

            if (_colorMode.floatValue > 0)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.x += EditorGUIUtility.labelWidth;
                rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;
                materialEditor.ShaderProperty(rect, _color, "");
                rect.x += rect.width + 4;
                materialEditor.ShaderProperty(rect, _color2, "");
            }
            else
            {
                materialEditor.ShaderProperty(_color, " ");
            }

            EditorGUILayout.Space();

            materialEditor.ShaderProperty(_metallic, "Metallic");
            materialEditor.ShaderProperty(_smoothness, "Smoothness");

            EditorGUILayout.Space();

            materialEditor.TexturePropertySingleLine(_albedoText, _albedoMap, null);
            materialEditor.TexturePropertySingleLine(_normalMapText, _normalMap, _normalMap.textureValue ? _normalScale : null);
            materialEditor.TextureScaleOffsetProperty(_albedoMap);

            EditorGUILayout.Space();

            materialEditor.ShaderProperty(_occHeight, "Occlusion Height");
            materialEditor.ShaderProperty(_occExp, "Occlusion Exponent");
            materialEditor.ShaderProperty(_occToColor, "Occlusion To Color");

            return EditorGUI.EndChangeCheck();
        }

        static void CompleteMaterialChanges(Material material)
        {
            var occh = Mathf.Max(material.GetFloat("_OccHeight"), 0.01f);
            material.SetFloat("_HeightToOcc", 1.0f / occh);

            SetKeyword(material, "_ALBEDOMAP", material.GetTexture("_MainTex"));
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_NormalMap"));
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
}
