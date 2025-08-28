using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace enumGames.Validation.Editor
{
    /// <summary>
    /// Asserts a range of required conditions based on serialized property type
    /// </summary>
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredDrawer : PropertyDrawer
    {

        public static bool IsValid(RequiredAttribute attribute, SerializedProperty property) => !IsInvalid(attribute, property);


        static bool IsEmptyOrContainsNullArray(SerializedProperty property) => property.isArray &&
            (property.arraySize == 0 || Enumerable.Range(0, property.arraySize).Any(index => property.GetArrayElementAtIndex(index) == null));
        static bool IsNull(SerializedProperty property) => property.propertyType == SerializedPropertyType.ObjectReference
            && property.objectReferenceValue == null;

        static bool IsNullOrEmpty(SerializedProperty property) => property.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(property.stringValue);
        static bool IsFalse(SerializedProperty property) => property.propertyType == SerializedPropertyType.Boolean && !property.boolValue;

        static bool IsClearColor(SerializedProperty property) => property.propertyType == SerializedPropertyType.Color && property.colorValue == Color.clear;

        static bool InValidFloatRange(RequiredAttribute attribute, SerializedProperty property) => property.propertyType == SerializedPropertyType.Float 
            && (property.floatValue < attribute.Min || property.floatValue > attribute.Max);

        static bool InvalidWwiseId(SerializedProperty property)
        {
            var prop = GetWwiseProperty(property);
            if (prop != null && prop.objectReferenceValue == null)
            {
                return true;
            }
            return false;
        }

        static SerializedProperty GetWwiseProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("WwiseObjectReference"); ;
        }

        static bool IsInvalid(RequiredAttribute attribute, SerializedProperty property) => 
            IsNull(property) 
            || IsFalse(property) 
            || IsClearColor(property) 
            || InValidFloatRange(attribute, property)
            || IsEmptyOrContainsNullArray(property)
            || IsNullOrEmpty(property)
            || InvalidWwiseId(property);


        RequiredAttribute Attribute => this.attribute as RequiredAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsInvalid(Attribute, property))
            {
                return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
            }

            float height = base.GetPropertyHeight(property, label);

            if (GetWwiseProperty(property) != null && !IsInvalid(Attribute, property))
            {
                height *= 2f;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            RequiredAttribute required = (RequiredAttribute)attribute;
            float yOffset = 0f;
            if(IsInvalid(Attribute, property))
            {
                
                yOffset = EditorGUIUtility.singleLineHeight;
                bool isError = required.Type == RequiredAttribute.Types.Error;
                Texture2D icon = isError ? GUIs.Styles.ErrorIcon : GUIs.Styles.WarnIcon;

                //icon
                Rect iconPosition = new Rect(position.x, position.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                GUI.Label(iconPosition, icon);

                //message
                GUIStyle style = isError ? GUIs.Styles.ErrorStyle : GUIs.Styles.WarnStyle;
                string defaultMessage = property.displayName + " is required";
                string message = string.IsNullOrEmpty(required.CustomMessage) ? defaultMessage : required.CustomMessage;
                Rect labelPosition = new Rect(position.x + EditorGUIUtility.singleLineHeight, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelPosition, message, style);
            }
            
            EditorGUI.PropertyField(new Rect(position.x, position.y + yOffset, position.width, position.height - yOffset), property, new GUIContent(property.displayName));

            EditorGUI.EndProperty();
        }
    }
}