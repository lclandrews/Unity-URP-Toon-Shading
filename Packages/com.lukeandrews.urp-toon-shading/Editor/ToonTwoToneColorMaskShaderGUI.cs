using UnityEditor;

using UnityEngine;

namespace ToonShading.Editor
{
    internal class ToonTwoToneColorMaskShaderGUI : ToonTwoToneShaderGUI
    {
        private MaterialProperty _colorMaskMap = null;
        private MaterialProperty _colorMaskRColor = null;
        private MaterialProperty _colorMaskGColor = null;
        private MaterialProperty _colorMaskBColor = null;
        private MaterialProperty _colorMaskAColor = null;

        protected override string VariantName
        {
            get { return "TwoToneColorMask"; }
        }

        public override void FindProperties(MaterialProperty[] properties)
        {
            _colorMaskMap = FindProperty(Properties.ColorMaskMap, properties);
            _colorMaskRColor = FindProperty(Properties.ColorMaskRColor, properties);
            _colorMaskGColor = FindProperty(Properties.ColorMaskGColor, properties);
            _colorMaskBColor = FindProperty(Properties.ColorMaskBColor, properties);
            _colorMaskAColor = FindProperty(Properties.ColorMaskAColor, properties);
            base.FindProperties(properties);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            DrawBaseProperties(material);
            DrawMaskMapArea(ToonStandardProperties, materialEditor, material);
            DrawColorMaskArea(materialEditor, material);
            DrawSpecularArea(ToonStandardProperties, materialEditor, material);
            DrawNormalArea(materialEditor, ToonStandardProperties.BumpMapProp, ToonStandardProperties.BumpScaleProp);
            DrawBacklightArea(ToonStandardProperties, materialEditor, material);
            DrawEdgeShineArea(ToonStandardProperties, materialEditor, material);
            DrawOcclusionArea(materialEditor, ToonStandardProperties.MaskMap, ToonStandardProperties.OcclusionStrength);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        public void DrawColorMaskArea(MaterialEditor materialEditor, Material material)
        {
            if (_colorMaskMap != null)
            {
                materialEditor.TexturePropertySingleLine(GUIStyles.ColorMaskMapText, _colorMaskMap);

                if (_colorMaskMap.textureValue != null)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    Color rColor = GUIHelpers.DrawColorField(_colorMaskRColor.colorValue, GUIStyles.ColorMaskRColor, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _colorMaskRColor.colorValue = rColor;
                    }

                    EditorGUI.BeginChangeCheck();
                    Color gColor = GUIHelpers.DrawColorField(_colorMaskGColor.colorValue, GUIStyles.ColorMaskGColor, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _colorMaskGColor.colorValue = gColor;
                    }

                    EditorGUI.BeginChangeCheck();
                    Color bColor = GUIHelpers.DrawColorField(_colorMaskBColor.colorValue, GUIStyles.ColorMaskBColor, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _colorMaskBColor.colorValue = bColor;
                    }

                    EditorGUI.BeginChangeCheck();
                    Color aColor = GUIHelpers.DrawColorField(_colorMaskAColor.colorValue, GUIStyles.ColorMaskAColor, true, false, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _colorMaskAColor.colorValue = aColor;
                    }
                    EditorGUI.indentLevel -= 2;
                }
            }
        }
    }    
}
