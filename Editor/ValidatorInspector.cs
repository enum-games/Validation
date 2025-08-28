using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
    
namespace enumGames.Validation
{
    [CustomEditor(typeof(Validator), true), CanEditMultipleObjects]
    public class ValidatorInspector : Attributes.Editor.DocumentationEditor
    {
        protected const string DIVIDER = "-------------------------";
        Validator Validator => this.target as Validator;
        static Vector2 scrollPosition;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            List<Validator.Log> messages = new List<Validator.Log>();
            Validator.Validate(ref messages);

            PrintMessages(messages);
        }

        public static void PrintMessages(List<Validator.Log> messages)
        {
            if(messages == null || messages.Count == 0)
            {
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
            int logs = 0;
            int warnings = 0;
            int errors = 0;

            for (int i = 0; i < messages.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                string prfx = messages[i].Type == LogType.Log ? "LOG: " : messages[i].Type == LogType.Warning ? "WARNING: " :  messages[i].Type == LogType.Error ? "ERROR: " : string.Empty;
                GUIStyle style = Validator.LogStyle;
                if (messages[i].Type == LogType.Log)
                {
                    logs++;
                    style = Validator.LogStyle;
                }
                else if (messages[i].Type == LogType.Warning)
                {
                    warnings++;
                    style = Validator.WarningStyle;
                }
                else if (messages[i].Type == LogType.Error)
                {
                    errors++;
                    style = Validator.ErrorStyle;
                }
                else
                {
                    style = Validator.LogStyle;
                }


                EditorGUILayout.LabelField(prfx + messages[i].Msg, style, GUILayout.ExpandWidth(true));
                if(messages[i].Target != null && GUILayout.Button("Select"))
                {
                    EditorGUIUtility.PingObject(messages[i].Target);
                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.LabelField("-------------------------", Validator.LogStyle);
            EditorGUILayout.LabelField($"Logs: {logs}", Validator.LogStyle);
            EditorGUILayout.LabelField($"Warnings: {warnings}", Validator.WarningStyle);
            EditorGUILayout.LabelField($"Errors: {errors}", Validator.ErrorStyle);
            EditorGUILayout.EndScrollView();
        }
    }
}