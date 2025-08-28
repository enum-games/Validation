using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using enumGames.Validation;
using enumGames.Validation.Editor;
using UnityEditor.SceneManagement;
using System.Linq;

namespace enumGames.Validation.Editor
{
    public class ValidationWindow : EditorWindow
    {

        static List<Validator.Log> logs = new List<Validator.Log>();
        GameObject gameObject;
        ScriptableObject so;



        public static void ShowWindow(List<Validator.Log> logs)
        {
            // Show existing window instance. If one doesn't exist, make one.
            ValidationWindow window = GetWindow<ValidationWindow>("Validation");
            ValidationWindow.logs = logs;
        }

        [MenuItem("enum/Validation")]
        public static void ShowWindow()
        {
            // Show existing window instance. If one doesn't exist, make one.
            ValidationWindow window = GetWindow<ValidationWindow>("Validation");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Validate Active Scene: " + SceneManager.GetActiveScene().name))
            {
               
                logs = ValidateActiveScene();
            }

            if(GUILayout.Button("Validate All Prefabs"))
            {
               
                logs = ValidatorUtil.ValidateAllPrefabs();
            }


            if(GUILayout.Button("Validate All Scriptable Objects"))
            {

                logs = new List<Validator.Log>();
                logs.AddRange(ValidatorUtil.ValidateAllScriptableObjects());
            }

            if(GUILayout.Button("Validate All Scenes in Build Settings"))
            {
                string path = EditorSceneManager.GetActiveScene().path;

                logs = ValidatorUtil.ValidateBuildScenes();
                //reload og scene
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                
            }

            if(GUILayout.Button("Validate All"))
            {
                logs = ValidatorUtil.ValidateAll();

            }

            //validate single game object
            gameObject = (GameObject)EditorGUILayout.ObjectField("Game Object", gameObject, typeof(GameObject), false);
            if(gameObject && GUILayout.Button("Validate " + gameObject.name))
            {
                logs = ValidatorUtil.Validate(gameObject);
            }

            //validate single scriptable object
            so = (ScriptableObject)EditorGUILayout.ObjectField("Scriptable Object", so, typeof(ScriptableObject), false);
            if(so && GUILayout.Button("Validate " + so.name))
            {

                logs = ValidatorUtil.ValidateObject(so);
            }


            ValidatorInspector.PrintMessages(logs);
        }

        List<Validator.Log> ValidateActiveScene()
        {
            return ValidatorUtil.ValidateActiveScene();
        }


    }
}