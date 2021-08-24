using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

using MyString = System.String;

[Serializable]

// public class Variant   
// {
//     public MyString vl;
// }
class Variant {
    public string Value;
    public Variant(string value) {
        Value = value;
    }
    public static implicit operator string(Variant d) {
        return d.Value;
    }
    public static implicit operator Variant(string d) {
        return new Variant(d);
    }
}

[Serializable]
class V2
{
    [SerializeField]
    private Variant a1;
    [SerializeField]
    private Variant a2;
    [SerializeField]
    private Variant a3;

}


[CustomPropertyDrawer(typeof(Variant))]
public class VariantEditor : PropertyDrawer
{
    public string[] options = new string[] {"Cube", "Sphere", "Plane"};
    public int index = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        var vl = property.FindPropertyRelative("Value").stringValue;
        for (var i = 0; i < options.Length; i++)
        {
            if (options[i] == vl)
            {
                index = i;
                break;
            }
        }
            
        index = EditorGUI.Popup(position , index, options);
        property.FindPropertyRelative("Value").stringValue = options[index];
        EditorGUI.EndProperty();
    }

}