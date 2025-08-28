using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace enumGames.Validation.Editor
{

    [CustomEditor(typeof(SceneValidator), true)]
    public class SceneValidatorInspector : ValidatorInspector
    {
        List<Validator.Log> sceneObjectMessages = new List<Validator.Log>();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Validate scene objects"))
            {
             
                sceneObjectMessages.Clear();
                sceneObjectMessages = ValidatorUtil.ValidateActiveScene();
                
             
            }

            if (sceneObjectMessages.Count != 0)
            {
                GUILayout.Label(DIVIDER);
                GUILayout.Label("SCENE OBJECTS:");
                int logs, warnings, errors;
                PrintMessages(sceneObjectMessages);
                
            }
        }
    }
}