using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.ToonShaderGUI
{
    public static class ToonMainGUI
    {
        public static class Styles
        {
            public static GUIContent MainLightingText = new GUIContent("Surface Shading",
                "The ramp properties of a surfaces shading.");

            public static GUIContent AdditionalLightingText = new GUIContent("Light Attenuation",
                "The ramp properties of light attenuation.");

            public static GUIContent MainShadowText = new GUIContent("Shadow Limit",
                "Used to control the shadow limit of a surfaces shading.");

            public static GUIContent MainLightEdgeText = new GUIContent("Highlight Limit",
                "Used to control the highlight limit of a surfaces shading.");

            public static GUIContent AddShadowText = new GUIContent("Shadow Limit",
                "Used to control the shadow limit of light attenuation.");

            public static GUIContent AddLightEdgeText = new GUIContent("Highlight Limit",
                "Used to control the highlight limit of light attenuation.");

            public static GUIContent EdgeSoftnessText = new GUIContent("Edge Softness",
                "Used to control the edge between lighting bands.");

            public static GUIContent MidToneText = new GUIContent("Mid Tone Value",
                "Controls the value of the mid tones.");

            public static GUIContent MaskMapText = new GUIContent("Mask Map",
                "Map that stores: Specular (R), Occlusion (G), Emission (B), Smoothness (A).");

            public static GUIContent MaskFillSpecular = new GUIContent("Fill Specular",
                "Ignore the (R) channel of the mask map.");

            public static GUIContent MaskFillOcclusion = new GUIContent("Fill Occlusion",
                "Ignore the (G) channel of the mask map.");

            public static GUIContent MaskFillEmission = new GUIContent("Fill Emission",
                "Ignore the (B) channel of the mask map.");

            public static GUIContent MaskFillSmoothness = new GUIContent("Fill Smoothness",
                "Ignore the (A) channel of the mask map.");

            public static GUIContent AmbientColorText = new GUIContent("Ambient Light Color",
                "Property that sets the ambient light color for all toon shaded objects in the scene.");

            public static GUIContent MainGradText = new GUIContent("Main Light Ramp",
                "Ramp texture that controls the shading bands of the main light.");

            public static GUIContent AddGradText = new GUIContent("Additional Light Ramp",
                "Ramp texture that controls the shading bands of additional lights.");

            public static GUIContent SpecularMapText = new GUIContent("Specular Map", 
                "Sets and configures the map and color for the Specular lighting.");

            public static GUIContent SmoothnessText = new GUIContent("Smoothness",
                "Controls the spread of highlights and reflections on the surface.");

            public static GUIContent HighlightsText = new GUIContent("Specular",
                "When enabled, the Material reflects the shine from direct lighting.");

            public static GUIContent SpecColorText = new GUIContent("Specular Color",
                "Controls the specular tint of highlights.");

            public static GUIContent EmissionColorText = new GUIContent("Emission Color",
                "Controls the tint of emissive colors.");

            public static GUIContent SpecTex = new GUIContent("Specular Texture",
                "Map used to add texture to specular highlights.");

            public static GUIContent SpecTexTile = new GUIContent("Tiling",
                "Used to uv scale and rotation of the specular texture map.");

            public static GUIContent SpecTexRot = new GUIContent("Rotation",
                "Used to uv scale and rotation of the specular texture map.");

            public static GUIContent EdgeShineText = new GUIContent("Edge Shine",
                "Enables edge shine.");

            public static GUIContent EdgeShineColorText = new GUIContent("Edge Shine Color",
                "Edge shine color.");

            public static GUIContent BacklightText = new GUIContent("Back Lighting",
                "Enables back lighting.");

            public static GUIContent BacklightStrengthText = new GUIContent("Strength",
                "Back lighting strength.");

            public static GUIContent OcclusionText = new GUIContent("Occlusion Map",
                "Sets an occlusion map to simulate shadowing from ambient lighting.");

            public static GUIContent OcclusionStrengthText = new GUIContent("Occlusion Strength",
                "Sets the strength of the occlusion map.");

            public static readonly GUIContent AdvLighting = new GUIContent("Lighting",
                "These settings describe the way light interacts with the surface.");

            public static readonly GUIContent OutlineText = new GUIContent("Enable",
                "Enables outline.");

            public static readonly GUIContent OutlineSettingsText = new GUIContent("Outline",
                "All outline settings.");

            public static readonly GUIContent OutlineWidthText = new GUIContent("Width",
                "Controls with width of the mesh outline.");

            public static readonly GUIContent OutlineWidthMapText = new GUIContent("Width Map",
                "An additional map used to variably control the width of the outline.");

            public static readonly GUIContent OutlineColorText = new GUIContent("Color",
                "Color applied to the outline.");

            public static readonly GUIContent OutlineFadeText = new GUIContent("Fade",
                "Whether to smoothly fade out the outline when culled.");

            public static readonly GUIContent OutlineFadeStartText = new GUIContent("Fade Start",
                "The distance at which the outline should begin to fade out.");

            public static readonly GUIContent OutlineFadeEndText = new GUIContent("Fade End",
                "The distance at which the outline should have faded out completely.");

            public static readonly GUIContent OutlineCullText = new GUIContent("Cull Distance",
                "The distance at which the outline should be culled.");

            public static readonly GUIContent OutlineOffsetZ = new GUIContent("Depth Offset",
                "Offsets the outline in the depth (Z) direction of the camera.");
        }
        public const int QueueOffsetRange = 50;

        public static Gradient ShaderValuesToGradient(int type, int length, Vector4[] keys)
        {
            if(keys == null)
            {
                keys = new Vector4[length];
            }
            Gradient grad = new Gradient();
            grad.mode = type == 0 ? GradientMode.Blend : GradientMode.Fixed;
            GradientColorKey[] gradKeys = new GradientColorKey[length];
            GradientColorKey key = new GradientColorKey();
            for (int i = 0; i < length; i++)
            {
                key.color = new Color(keys[i].x, keys[i].y, keys[i].z);
                key.time = keys[i].w;
                gradKeys[i] = key;
            }
            grad.colorKeys = gradKeys;
            return grad;
        }

        public static void GradientToShaderValues(Gradient grad, out int type, out int length, out Vector4[] keys)
        {
            type = grad.mode == GradientMode.Blend ? 0 : 1;
            length = grad.colorKeys.Length;
            keys = new Vector4[8];
            Vector4 v = new Vector4();
            for (int i = 0; i < length; i++)
            {
                if(i <= 7)
                {                    
                    v.x = grad.colorKeys[i].color.r;
                    v.y = grad.colorKeys[i].color.g;
                    v.z = grad.colorKeys[i].color.b;
                    v.w = grad.colorKeys[i].time;
                    keys[i] = v;
                }
            }
        }

        public static Color DrawColorField(Color inColor, GUIContent label, bool eyeDropper, bool alpha, bool hdr)
        {            
            Rect r = EditorGUILayout.GetControlRect();
            r.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(r, label);
            r.x = EditorGUIUtility.labelWidth + 18.0f;
            r.width = EditorGUIUtility.fieldWidth;
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Color c =  EditorGUI.ColorField(r, GUIContent.none, inColor, eyeDropper, alpha, hdr);
            EditorGUI.indentLevel = indent;
            return c;
        }

        public static Gradient DrawMiniGradientField(Gradient inGradient, GUIContent label, bool hdr)
        {            
            Rect r = EditorGUILayout.GetControlRect();
            r.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(r, label);
            r.x = EditorGUIUtility.labelWidth + 18.0f;
            r.width = EditorGUIUtility.fieldWidth;
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Gradient g = EditorGUI.GradientField(r, GUIContent.none, inGradient, hdr);
            EditorGUI.indentLevel = indent;
            return g;
        }
    }

    public class SavedParameter<T>
        where T : IEquatable<T>
    {
        public delegate void SetParameter(string key, T value);
        public delegate T GetParameter(string key, T defaultValue);

        private readonly string _key;
        private bool _loaded;
        private T _value;

        private readonly SetParameter _setter;
        private readonly GetParameter _getter;

        public SavedParameter(string key, T value, GetParameter getter, SetParameter setter)
        {
            Assert.IsNotNull(setter);
            Assert.IsNotNull(getter);

            _key = key;
            _loaded = false;
            _value = value;
            _setter = setter;
            _getter = getter;
        }

        void Load()
        {
            if (_loaded)
                return;

            _loaded = true;
            _value = _getter(_key, _value);
        }

        public T value
        {
            get
            {
                Load();
                return _value;
            }
            set
            {
                Load();

                if (_value.Equals(value))
                    return;

                _value = value;
                _setter(_key, value);
            }
        }
    }

    // Pre-specialized class for easier use and compatibility with existing code
    public class SavedBool : SavedParameter<bool>
    {
        public SavedBool(string key, bool value)
            : base(key, value, EditorPrefs.GetBool, EditorPrefs.SetBool) { }
    }

    public class SavedInt : SavedParameter<int>
    {
        public SavedInt(string key, int value)
            : base(key, value, EditorPrefs.GetInt, EditorPrefs.SetInt) { }
    }

    public class SavedFloat : SavedParameter<float>
    {
        public SavedFloat(string key, float value)
            : base(key, value, EditorPrefs.GetFloat, EditorPrefs.SetFloat) { }
    }

    public class SavedString : SavedParameter<string>
    {
        public void SetValueAsGradient(Gradient grad)
        {
            GradientJSON gj = new GradientJSON();
            gj.FromGradient(grad);
            value = JsonUtility.ToJson(gj);
        }

        public Gradient GetValueAsGradient()
        {
            GradientJSON gj = JsonUtility.FromJson<GradientJSON>(value);
            return gj.ToGradient();
        }

        public void SetValueAsColor(Color color)
        {
            ColorJSON c = new ColorJSON();
            c.Color = color;
            value = JsonUtility.ToJson(c);
        }

        public Color GetValueAsColor()
        {
            ColorJSON c = JsonUtility.FromJson<ColorJSON>(value);
            return c.Color;
        }

        public static string GetDefaultGradientString()
        {
            GradientJSON gj = new GradientJSON();
            gj.FromGradient(new Gradient());
            return JsonUtility.ToJson(gj);
        }

        public static string GetDefaultColorString()
        {
            ColorJSON c = new ColorJSON();
            c.Color = Color.black;
            return JsonUtility.ToJson(c);
        }

        public SavedString(string key, string value)
            : base(key, value, EditorPrefs.GetString, EditorPrefs.SetString) { }
    }

    [Serializable]
    public class GradientJSON
    {
        public GradientMode Mode = GradientMode.Blend;
        public List<Vector4> ColorKeys = new List<Vector4>();
        public List<Vector2> AlphaKeys = new List<Vector2>();

        public void FromGradient(Gradient gradient)
        {
            if(gradient != null)
            {
                ColorKeys.Clear();
                AlphaKeys.Clear();
                Mode = gradient.mode;
                for(int i = 0; i < gradient.colorKeys.Length; i++)
                {
                    Color c = gradient.colorKeys[i].color;
                    float t = gradient.colorKeys[i].time;
                    ColorKeys.Add(new Vector4(c.r, c.g, c.b, t));
                }
                for (int i = 0; i < gradient.alphaKeys.Length; i++)
                {
                    float a = gradient.alphaKeys[i].alpha;
                    float t = gradient.colorKeys[i].time;
                    AlphaKeys.Add(new Vector4(a, t));
                }
            }
        }

        public Gradient ToGradient()
        {
            Gradient g = new Gradient();
            g.mode = Mode;
            g.colorKeys = new GradientColorKey[ColorKeys.Count];
            g.alphaKeys = new GradientAlphaKey[AlphaKeys.Count];
            for (int i = 0; i < ColorKeys.Count; i++)
            {
                Vector4 v = ColorKeys[i];
                Color c = new Color(v.x, v.y, v.z);
                g.colorKeys[i] = new GradientColorKey(c, v.w);
            }
            for (int i = 0; i < AlphaKeys.Count; i++)
            {
                Vector2 v = AlphaKeys[i];
                g.alphaKeys[i] = new GradientAlphaKey(v.x, v.y);
            }
            return g;
        }
    }

    [Serializable]
    public class ColorJSON
    {
        public Color Color = Color.white;
    }
}