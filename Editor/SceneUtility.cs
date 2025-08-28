using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace enumGames.Validation.Editor
{
    public static class SceneUtility
    {
        public static string GetBuildScenePathByName(string sceneName)
        {
            // Iterate over all scenes in the build settings
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                // Load the scene asset at the specified path
                string scenePath = scene.path;
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

                // Check if the scene name matches the requested name
                if (sceneAsset != null && sceneAsset.name == sceneName)
                {
                    return scenePath;
                }
            }

            // If no match is found, return null
            return null;
        }
    }
}

