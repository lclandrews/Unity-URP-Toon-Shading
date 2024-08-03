using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.Assertions;

namespace ToonShading.Editor
{
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