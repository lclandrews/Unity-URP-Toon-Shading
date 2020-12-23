using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.ToonShaderGUI
{
    public static class ToonMasterGUI
    {
        public static class Styles
        {
            public static GUIContent mainLightingText = new GUIContent("Surface Shading",
                "The ramp properties of a surfaces shading.");

            public static GUIContent additionalLightingText = new GUIContent("Light Attenuation",
                "The ramp properties of light attenuation.");

            public static GUIContent mainShadowText = new GUIContent("Shadow Limit",
                "Used to control the shadow limit of a surfaces shading.");

            public static GUIContent mainLightEdgeText = new GUIContent("Highlight Limit",
                "Used to control the highlight limit of a surfaces shading.");

            public static GUIContent addShadowText = new GUIContent("Shadow Limit",
                "Used to control the shadow limit of light attenuation.");

            public static GUIContent addLightEdgeText = new GUIContent("Highlight Limit",
                "Used to control the highlight limit of light attenuation.");

            public static GUIContent edgeSoftnessText = new GUIContent("Edge Softness",
                "Used to control the edge between lighting bands.");

            public static GUIContent midToneText = new GUIContent("Mid Tone Value",
                "Controls the value of the mid tones.");

            public static GUIContent maskMapText = new GUIContent("Mask Map",
                "Map that stores: Specular (R), Occlusion (G), Emission (B), Smoothness (A).");

            public static GUIContent maskFillSpecular = new GUIContent("Fill Specular",
                "Ignore the (R) channel of the mask map.");

            public static GUIContent maskFillOcclusion = new GUIContent("Fill Occlusion",
                "Ignore the (G) channel of the mask map.");

            public static GUIContent maskFillEmission = new GUIContent("Fill Emission",
                "Ignore the (B) channel of the mask map.");

            public static GUIContent maskFillSmoothness = new GUIContent("Fill Smoothness",
                "Ignore the (A) channel of the mask map.");

            public static GUIContent ambientColorText = new GUIContent("Ambient Light Color",
                "Property that sets the ambient light color for all toon shaded objects in the scene.");

            public static GUIContent mainGradText = new GUIContent("Main Light Ramp",
                "Ramp texture that controls the shading bands of the main light.");

            public static GUIContent addGradText = new GUIContent("Additional Light Ramp",
                "Ramp texture that controls the shading bands of additional lights.");

            public static GUIContent specularMapText = new GUIContent("Specular Map", 
                "Sets and configures the map and color for the Specular lighting.");

            public static GUIContent smoothnessText = new GUIContent("Smoothness",
                "Controls the spread of highlights and reflections on the surface.");

            public static GUIContent highlightsText = new GUIContent("Specular",
                "When enabled, the Material reflects the shine from direct lighting.");

            public static GUIContent specColorText = new GUIContent("Specular Color",
                "Controls the specular tint of highlights.");

            public static GUIContent emissionColorText = new GUIContent("Emission Color",
                "Controls the tint of emissive colors.");

            public static GUIContent specTex = new GUIContent("Specular Texture",
                "Map used to add texture to specular highlights.");

            public static GUIContent specTexTile = new GUIContent("Tiling",
                "Used to uv scale and rotation of the specular texture map.");

            public static GUIContent specTexRot = new GUIContent("Rotation",
                "Used to uv scale and rotation of the specular texture map.");

            public static GUIContent edgeShineText = new GUIContent("Edge Shine",
                "Enables edge shine.");

            public static GUIContent edgeShineColorText = new GUIContent("Edge Shine Color",
                "Edge shine color.");

            public static GUIContent backlightText = new GUIContent("Back Lighting",
                "Enables back lighting.");

            public static GUIContent backlightStrengthText = new GUIContent("Strength",
                "Back lighting strength.");

            public static GUIContent occlusionText = new GUIContent("Occlusion Map",
                "Sets an occlusion map to simulate shadowing from ambient lighting.");

            public static GUIContent occlusionStrengthText = new GUIContent("Occlusion Strength",
                "Sets the strength of the occlusion map.");

            public static readonly GUIContent AdvLighting = new GUIContent("Lighting",
                "These settings describe the way light interacts with the surface.");
        }

        public const int queueOffsetRange = 50;

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

        readonly string m_Key;
        bool m_Loaded;
        T m_Value;

        readonly SetParameter m_Setter;
        readonly GetParameter m_Getter;

        public SavedParameter(string key, T value, GetParameter getter, SetParameter setter)
        {
            Assert.IsNotNull(setter);
            Assert.IsNotNull(getter);

            m_Key = key;
            m_Loaded = false;
            m_Value = value;
            m_Setter = setter;
            m_Getter = getter;
        }

        void Load()
        {
            if (m_Loaded)
                return;

            m_Loaded = true;
            m_Value = m_Getter(m_Key, m_Value);
        }

        public T value
        {
            get
            {
                Load();
                return m_Value;
            }
            set
            {
                Load();

                if (m_Value.Equals(value))
                    return;

                m_Value = value;
                m_Setter(m_Key, value);
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
            c.color = color;
            value = JsonUtility.ToJson(c);
        }

        public Color GetValueAsColor()
        {
            ColorJSON c = JsonUtility.FromJson<ColorJSON>(value);
            return c.color;
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
            c.color = Color.black;
            return JsonUtility.ToJson(c);
        }

        public SavedString(string key, string value)
            : base(key, value, EditorPrefs.GetString, EditorPrefs.SetString) { }
    }

    [Serializable]
    public class GradientJSON
    {
        public GradientMode mode = GradientMode.Blend;
        public List<Vector4> colorKeys = new List<Vector4>();
        public List<Vector2> alphaKeys = new List<Vector2>();

        public void FromGradient(Gradient gradient)
        {
            if(gradient != null)
            {
                colorKeys.Clear();
                alphaKeys.Clear();
                mode = gradient.mode;
                for(int i = 0; i < gradient.colorKeys.Length; i++)
                {
                    Color c = gradient.colorKeys[i].color;
                    float t = gradient.colorKeys[i].time;
                    colorKeys.Add(new Vector4(c.r, c.g, c.b, t));
                }
                for (int i = 0; i < gradient.alphaKeys.Length; i++)
                {
                    float a = gradient.alphaKeys[i].alpha;
                    float t = gradient.colorKeys[i].time;
                    alphaKeys.Add(new Vector4(a, t));
                }
            }
        }

        public Gradient ToGradient()
        {
            Gradient g = new Gradient();
            g.mode = mode;
            g.colorKeys = new GradientColorKey[colorKeys.Count];
            g.alphaKeys = new GradientAlphaKey[alphaKeys.Count];
            for (int i = 0; i < colorKeys.Count; i++)
            {
                Vector4 v = colorKeys[i];
                Color c = new Color(v.x, v.y, v.z);
                g.colorKeys[i] = new GradientColorKey(c, v.w);
            }
            for (int i = 0; i < alphaKeys.Count; i++)
            {
                Vector2 v = alphaKeys[i];
                g.alphaKeys[i] = new GradientAlphaKey(v.x, v.y);
            }
            return g;
        }
    }

    [Serializable]
    public class ColorJSON
    {
        public Color color = Color.white;
    }
}