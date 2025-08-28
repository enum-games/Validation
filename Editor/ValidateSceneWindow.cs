using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace enumGames.Validation.Editor

{
    public class ValidateSceneWindow : EditorWindow
    {
        List<Validator.Log> logs = new List<Validator.Log>();

       
        public static void ShowWindow()
        {
            // Show existing window instance. If one doesn't exist, make one.
            ValidateSceneWindow window = GetWindow<ValidateSceneWindow>(SceneManager.GetActiveScene().name + " Validation");
            window.logs.AddRange(ValidatorUtil.ValidateActiveScene());
        }

        private void OnGUI()
        {
           if(GUILayout.Button("Revalidate " + SceneManager.GetActiveScene().name))
           {
                logs.AddRange(ValidatorUtil.ValidateActiveScene());
           }
           for(int i = 0; i < logs.Count; i++)
           {
                EditorGUILayout.BeginHorizontal();
                if(logs[i].Type == LogType.Error)
                {
                    EditorGUILayout.LabelField(logs[i].Msg, GUIs.Styles.ErrorStyle);
                }
                else if(logs[i].Type == LogType.Warning)
                {
                    EditorGUILayout.LabelField(logs[i].Msg, GUIs.Styles.WarnStyle);
                }
                else
                {
                    EditorGUILayout.LabelField(logs[i].Msg);
                }

                if (logs[i].Target && GUILayout.Button("select"))
                {
                    Selection.activeObject = logs[i].Target;
                }
                EditorGUILayout.EndHorizontal();
           }
        }
    }
}

