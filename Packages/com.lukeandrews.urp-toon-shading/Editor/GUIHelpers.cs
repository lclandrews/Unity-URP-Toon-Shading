using UnityEngine;
using UnityEditor;

namespace ToonShading.Editor
{
    public static class GUIHelpers
    {
        public static Gradient ShaderValuesToGradient(int type, int length, Vector4[] keys)
        {
            if (keys == null)
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
                if (i <= 7)
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
            Color c = EditorGUI.ColorField(r, GUIContent.none, inColor, eyeDropper, alpha, hdr);
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
}