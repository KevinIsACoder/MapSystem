using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MilitarySimulator.Core.Utils
{
    public static class YamlMapper
    {
        static Dictionary<YamlData, int> m_LevelMap = new Dictionary<YamlData, int>();

        public static T Map<T>(string yaml)
        {
            var yamlData = ToObject(yaml);
            var obj = MapObject(yamlData, typeof(T));
            return (T)obj;
        }

        public static string Dump(object obj)
        {
            var type = obj.GetType();
            var yaml = new YamlData();
            DumpObject(yaml, obj, type);
            return DumpYaml(yaml, 0);
        }

        public static T ToObject<T>(string yaml)
        {
            var yamlData = ToObject(yaml);
            var obj = MapObject(yamlData, typeof(T));
            return (T)obj;
        }

        public static YamlData ToObject(string yaml)
        {
            m_LevelMap.Clear();
            YamlData baseYaml = new YamlData();
            m_LevelMap.Add(baseYaml, -2);
            YamlData parent = baseYaml;
            var lines = yaml.Split('\n');
            foreach (var line in lines)
            {
                var level = 0;
                var key = "";
                var value = "";
                var isKey = true;
                var isLeaf = false;
                var isVaild = false;

                for (var i = 0; i < line.Length; i++)
                {
                    var c = line[i];
                    if (isKey)
                    {
                        if (c == ' ')
                        {
                            level++;
                            continue;
                        }
                        if (c == ':')
                        {
                            isKey = false;
                            continue;
                        }
                        key += c;
                        isVaild = true;
                    }
                    else
                    {
                        if (c != '\r' && c != ' ')
                        {
                            isLeaf = true;
                        }

                        value += c;
                    }
                }
                if (!isVaild)
                {
                    continue;
                }

                YamlData data = isLeaf ? new YamlData(value.Trim(' ', '\t')) : new YamlData();

                m_LevelMap.Add(data, level);

                if (level > m_LevelMap[parent])
                {
                    parent[key] = data;
                    parent = data;
                }
                else
                {
                    while (level <= m_LevelMap[parent])
                    {
                        parent = parent.Parent;
                    }
                    parent[key] = data;
                    parent = data;
                }
            }
            m_LevelMap.Clear();
            return baseYaml;
        }

        public static string DumpYaml(YamlData data, int level)
        {
            if (data.IsLeaf)
            {
                return data.Value;
            }

            var yaml = "";
            foreach (var child in data.Children)
            {
                var space = GetSpace(level);
                for (var i = 0; i < level; i += 2)
                {
                    space += "  ";
                }
                if (child.Value.IsLeaf)
                {
                    yaml += $"{space}{child.Key}: {child.Value.Value}\n";
                }
                else
                {
                    yaml += $"{space}{child.Key}:\n";
                    yaml += DumpYaml(child.Value, level + 2);
                }
            }
            return yaml;
        }

        #region Private

        public static object MapObject(YamlData yaml, System.Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                return GetVaildValue(yaml.Value, type);
            }

            if (type == typeof(Vector3))
            {
                var vector3 = new Vector3();
                vector3.x = float.Parse(yaml["x"].Value);
                vector3.y = float.Parse(yaml["y"].Value);
                vector3.z = float.Parse(yaml["z"].Value);
                return vector3;
            }
            if (type == typeof(Vector2))
            {
                var vector2 = new Vector2();
                vector2.x = float.Parse(yaml["x"].Value);
                vector2.y = float.Parse(yaml["y"].Value);
                return vector2;
            }
            if (type == typeof(Vector4))
            {
                var vector4 = new Vector4();
                vector4.x = float.Parse(yaml["x"].Value);
                vector4.y = float.Parse(yaml["y"].Value);
                vector4.z = float.Parse(yaml["z"].Value);
                vector4.w = float.Parse(yaml["w"].Value);
                return vector4;
            }
            if (type == typeof(Quaternion))
            {
                var quaternion = new Quaternion();
                quaternion.x = float.Parse(yaml["x"].Value);
                quaternion.y = float.Parse(yaml["y"].Value);
                quaternion.z = float.Parse(yaml["z"].Value);
                quaternion.w = float.Parse(yaml["w"].Value);
                return quaternion;
            }
            if (type == typeof(Color))
            {
                var color = new Color();
                color.r = float.Parse(yaml["r"].Value);
                color.g = float.Parse(yaml["g"].Value);
                color.b = float.Parse(yaml["b"].Value);
                color.a = float.Parse(yaml["a"].Value);
                return color;
            }
            if (type == typeof(Color32))
            {
                var color = new Color32();
                color.r = byte.Parse(yaml["r"].Value);
                color.g = byte.Parse(yaml["g"].Value);
                color.b = byte.Parse(yaml["b"].Value);
                color.a = byte.Parse(yaml["a"].Value);
                return color;
            }
            if (type == typeof(AnimationCurve))
            {
                var curve = new AnimationCurve();
                foreach (var kvp in yaml.Children)
                {
                    var v = kvp.Value;
                    var time = float.Parse(v["time"].Value);
                    var value = float.Parse(v["value"].Value);
                    var inTangent = float.Parse(v["inTangent"].Value);
                    var outTangent = float.Parse(v["outTangent"].Value);
                    var keyframe = new Keyframe(time, value, inTangent, outTangent);
                    curve.AddKey(keyframe);
                }
                return curve;
            }

            if (type == typeof(Gradient))
            {
                var gradient = new Gradient();
                foreach (var kvp in yaml.Children)
                {
                    var v = kvp.Value;
                    var color = new Color();
                    color.r = float.Parse(v["color"]["r"].Value);
                    color.g = float.Parse(v["color"]["g"].Value);
                    color.b = float.Parse(v["color"]["b"].Value);
                    color.a = float.Parse(v["color"]["a"].Value);
                    var time = float.Parse(v["time"].Value);
                    gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(color, time) }, new GradientAlphaKey[] { });
                }
                return gradient;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var array = System.Array.CreateInstance(elementType, yaml.Children.Count);
                for (var i = 0; i < yaml.Children.Count; i++)
                {
                    var child = yaml.Children.ElementAt(i);
                    array.SetValue(MapObject(child.Value, elementType), i);
                }
                return array;
            }
            if (IsList(type))
            {
                foreach (var item in type.GetFields(System.Reflection.BindingFlags.ExactBinding))
                {
                    Debug.Log(item.Name);
                }
                var list = System.Activator.CreateInstance(type) as IList;
                Type elementType = type.GetGenericArguments()[0];
                for (var i = 0; i < yaml.Children.Count; i++)
                {
                    var child = yaml.Children.ElementAt(i);
                    list.Add(MapObject(child.Value, elementType));
                }
                return list;
            }
            if (IsDictionary(type))
            {
                var dictionary = System.Activator.CreateInstance(type) as IDictionary;
                foreach (var child in yaml.Children)
                {
                    var key = GetVaildValue(child.Key, type.GenericTypeArguments[0]);
                    var value = MapObject(child.Value, type.GenericTypeArguments[1]);
                    dictionary.Add(key, value);
                }
                return dictionary;
            }

            var obj = System.Activator.CreateInstance(type);

            foreach (var data in yaml.Children)
            {
                var child = data.Value;
                var field = type.GetField(data.Key);
                if (field != null)
                {
                    var childType = field.FieldType;
                    field.SetValue(obj, MapObject(child, childType));
                }
            }

            return obj;
        }

        private static bool IsDictionary(System.Type type)
        {
            var types = type.GetInterfaces();
            foreach (var t in types)
            {
                if (t == typeof(IDictionary))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsList(System.Type type)
        {
            var types = type.GetInterfaces();
            foreach (var t in types)
            {
                if (t == typeof(IList))
                {
                    return true;
                }
            }
            return false;
        }

        private static void DumpObject(YamlData yaml, object obj, System.Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                yaml.Value = obj.ToString();
                return;
            }

            if (obj is IDictionary)
            {
                var dictionary = obj as IDictionary;
                var valueType = type.GenericTypeArguments[1];
                bool isPrimitive = IsPrimitive(valueType);
                foreach (var key in dictionary.Keys)
                {
                    var value = dictionary[key];
                    yaml.AddChild(key.ToString(), isPrimitive);
                    DumpObject(yaml[key.ToString()], value, valueType);
                }
                return;
            }
            if (obj is IList)
            {
                var array = obj as IList;
                int i = 0;
                foreach (var item in array)
                {
                    var childType = item.GetType();
                    var child = yaml.AddChild("Element_" + i.ToString(), IsPrimitive(childType));
                    DumpObject(child, item, childType);
                    i++;
                }
                return;
            }
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var array = obj as System.Array;
                bool isPrimitive = IsPrimitive(elementType);
                for (var i = 0; i < array.Length; i++)
                {
                    var item = array.GetValue(i);
                    var child = yaml.AddChild("Element_" + i.ToString(), isPrimitive);
                    DumpObject(child, item, elementType);
                }
                return;
            }

            if (type == typeof(Vector3))
            {
                DampVector3(yaml, (Vector3)obj);
                return;
            }
            if (type == typeof(Vector2))
            {
                DampVector2(yaml, (Vector2)obj);
                return;
            }
            if (type == typeof(Vector4))
            {
                DampVector4(yaml, (Vector4)obj);
                return;
            }
            if (type == typeof(Quaternion))
            {
                DampQuaternion(yaml, (Quaternion)obj);
                return;
            }
            if (type == typeof(Color))
            {
                DampColor(yaml, (Color)obj);
                return;
            }
            if (type == typeof(Color32))
            {
                DampColor32(yaml, (Color32)obj);
                return;
            }
            if (type == typeof(AnimationCurve))
            {
                var curve = (AnimationCurve)obj;
                DampAnimationCurve(yaml, curve);
            }

            if (type == typeof(Gradient))
            {
                var gradient = (Gradient)obj;
                DampGradient(yaml, gradient);
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsStatic || field.Name == "Empty")
                {
                    continue;
                }
                if (field.FieldType.IsEnum)
                {
                    var value = field.GetValue(obj);
                    if (value != null)
                    {
                        yaml.AddChild(field.Name, value.ToString());
                    }
                    continue;
                }
                if (field.FieldType.IsPrimitive || field.FieldType == typeof(string))
                {
                    var value = field.GetValue(obj);
                    if (value != null)
                    {
                        yaml.AddChild(field.Name, value.ToString());
                    }
                    continue;
                }

                var child = yaml.AddChild(field.Name);
                //Debug.Log(field.Name + ":" + type + " obj:" + obj);
                DumpObject(child, field.GetValue(obj), field.FieldType);
            }
        }

        private static bool IsPrimitive(System.Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type.IsEnum;
        }

        private static void DampGradient(YamlData yaml, Gradient gradient)
        {
            var length = gradient.colorKeys.Length;
            for (int i = 0; i < length; i++)
            {
                var keyframe = gradient.colorKeys[i];
                var key = yaml.AddChild("_ColorKey" + i.ToString());
                key.AddChild("color").AddChild("r", keyframe.color.r.ToString());
                key["color"].AddChild("g", keyframe.color.g.ToString());
                key["color"].AddChild("b", keyframe.color.b.ToString());
                key["color"].AddChild("a", keyframe.color.a.ToString());
                key.AddChild("time", keyframe.time.ToString());
            }
        }

        private static void DampAnimationCurve(YamlData yaml, AnimationCurve curve)
        {
            int length = curve.keys.Length;

            for (int i = 0; i < length; i++)
            {
                var keyframe = curve.keys[i];
                var key = yaml.AddChild("_KeyFram" + i.ToString());
                key.AddChild("time", keyframe.time.ToString());
                key.AddChild("value", keyframe.value.ToString());
                key.AddChild("inTangent", keyframe.inTangent.ToString());
                key.AddChild("outTangent", keyframe.outTangent.ToString());
            }
        }

        private static void DampColor32(YamlData yaml, Color32 color)
        {
            yaml.AddChild("r", color.r.ToString());
            yaml.AddChild("g", color.g.ToString());
            yaml.AddChild("b", color.b.ToString());
            yaml.AddChild("a", color.a.ToString());
        }

        private static void DampColor(YamlData yaml, Color color)
        {
            yaml.AddChild("r", color.r.ToString());
            yaml.AddChild("g", color.g.ToString());
            yaml.AddChild("b", color.b.ToString());
            yaml.AddChild("a", color.a.ToString());
        }

        private static void DampQuaternion(YamlData yaml, Quaternion quaternion)
        {
            yaml.AddChild("x", quaternion.x.ToString());
            yaml.AddChild("y", quaternion.y.ToString());
            yaml.AddChild("z", quaternion.z.ToString());
            yaml.AddChild("w", quaternion.w.ToString());
        }

        private static void DampVector4(YamlData yaml, Vector4 vector4)
        {
            yaml.AddChild("x", vector4.x.ToString());
            yaml.AddChild("y", vector4.y.ToString());
            yaml.AddChild("z", vector4.z.ToString());
            yaml.AddChild("w", vector4.w.ToString());
        }

        private static void DampVector2(YamlData yaml, Vector2 vector2)
        {
            yaml.AddChild("x", vector2.x.ToString());
            yaml.AddChild("y", vector2.y.ToString());
        }

        private static void DampVector3(YamlData yaml, Vector3 vector3)
        {
            yaml.AddChild("x", vector3.x.ToString());
            yaml.AddChild("y", vector3.y.ToString());
            yaml.AddChild("z", vector3.z.ToString());
        }

        private static object GetVaildValue(string value, System.Type type)
        {
            if (type.IsEnum)
            {
                return System.Enum.Parse(type, value);
            }
            if (type == typeof(string))
            {
                return value;
            }
            if (type == typeof(int))
            {
                return int.Parse(value);
            }
            if (type == typeof(float))
            {
                return float.Parse(value);
            }
            if (type == typeof(double))
            {
                return double.Parse(value);
            }
            if (type == typeof(long))
            {
                return long.Parse(value);
            }
            if (type == typeof(bool))
            {
                return bool.Parse(value);
            }
            if (type == typeof(byte))
            {
                return byte.Parse(value);
            }
            if (type == typeof(char))
            {
                return char.Parse(value);
            }
            if (type == typeof(short))
            {
                return short.Parse(value);
            }
            if (type == typeof(uint))
            {
                return uint.Parse(value);
            }
            if (type == typeof(ulong))
            {
                return ulong.Parse(value);
            }
            if (type == typeof(ushort))
            {
                return ushort.Parse(value);
            }
            return null;
        }

        private static string GetSpace(int level)
        {
            switch (level)
            {
                case 0:
                    return "";
                case 2:
                    return "  ";
                case 4:
                    return "    ";
                case 6:
                    return "      ";
                case 8:
                    return "        ";
                default:
                    break;
            }
            var space = "";
            for (var i = 0; i < level; i += 2)
            {
                space += "  ";
            }
            return space;
        }

        #endregion
    }
    public class YamlData
    {
        private Dictionary<string, YamlData> _children;
        private YamlData _parent;
        private string _value;
        private readonly bool _isLeaf;

        public Dictionary<string, YamlData> Children => _children;
        public YamlData Parent => _parent;
        public bool IsLeaf => _isLeaf;
        public YamlData this[string key]
        {
            get
            {
                if (_children.TryGetValue(key, out var data))
                {
                    return data;
                }
                return null;
            }
            set
            {
                if (_children.ContainsKey(key))
                {
                    _children[key] = value;
                }
                else
                {
                    _children.Add(key, value);
                }
                value._parent = this;
            }
        }

        public string Value
        {
            get
            {
                if (_isLeaf)
                {
                    return _value;
                }
                return null;
            }
            set
            {
                if (_isLeaf)
                {
                    _value = value;
                }
            }
        }

        public YamlData(bool isLeaf = false)
        {
            _children = new Dictionary<string, YamlData>();
            _isLeaf = isLeaf;
        }

        public YamlData(string value)
        {
            _value = value;
            _isLeaf = true;
        }

        public YamlData AddChild(string key)
        {
            return AddChild(key, false);
        }

        public YamlData AddChild(string key, bool isLeaf)
        {
            var data = new YamlData(isLeaf);
            return AddChild(key, data);
        }

        public YamlData AddChild(string key, string value)
        {
            var data = new YamlData(value);
            return AddChild(key, data);
        }

        public YamlData AddChild(string key, YamlData data)
        {
            if (_children.ContainsKey(key))
            {
                _children[key] = data;
            }
            else
            {
                _children.Add(key, data);
            }
            data._parent = this;
            return data;
        }

        public void RemoveChild(string key)
        {
            if (_children.ContainsKey(key))
            {
                _children.Remove(key);
            }
        }

        public YamlData AddVector3(string name, Vector3 vector3)
        {
            var data = AddChild(name);
            data.AddChild("x", vector3.x.ToString());
            data.AddChild("y", vector3.y.ToString());
            data.AddChild("z", vector3.z.ToString());
            return data;
        }

        public Vector3 ToVector3()
        {
            var x = float.Parse(this["x"].Value);
            var y = float.Parse(this["y"].Value);
            var z = float.Parse(this["z"].Value);
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return YamlMapper.DumpYaml(this, 0);
        }

    }

}