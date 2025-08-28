using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace enumGames.Validation.Editor
{
    public static class ValidatorUtil
    {
        /// <summary>
        /// Check to only validate types from target assemblies. By default, we check for "Assembly-CSharp.dll"
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool InTargetAssembly(System.Type type)
        {
            return type.Assembly.Location.Contains("Assembly-CSharp.dll");
        }


        public static List<Validator.Log> ValidateBuildScenes()
        {
            string openScenePath = EditorSceneManager.GetActiveScene().path;
            string[] paths = System.Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled).Select(scene => scene.path).ToArray();
            List<Validator.Log> msgs = new List<Validator.Log>();
            foreach (string path in paths)
            {
                msgs.AddRange(ValidateScene(path));
            }

            //reload og scene
            EditorSceneManager.OpenScene(openScenePath, OpenSceneMode.Single);

            return msgs;
        }



        /// <summary>
        /// Validates scene with scenePath
        /// </summary>
        /// <param name="scenePath"></param>
        /// <returns></returns>
        static List<Validator.Log> ValidateScene(string scenePath)
        {
            List<Validator.Log> msgs = new List<Validator.Log>();
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != scenePath)
            {
                try
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                catch
                {
                    msgs.Add(new Validator.Log
                    {
                        Type = LogType.Error,
                        Msg = "Invalid scene path provided: " + scenePath
                    });
                    return msgs;
                }
            }
            msgs.Add(new Validator.Log
            {
                Type = LogType.Log,
                Msg = "BEGIN SCENE VALIDATION: " + scene.name
            });

            SceneValidator sceneValidator = GameObject.FindObjectOfType<SceneValidator>();
            if(sceneValidator == null)
            {
                msgs.Add(new Validator.Log
                {
                    Type = LogType.Error,
                    Msg = "Scene does not have a scene validator object: " + scene.name
                });
            }
            else if(sceneValidator.GetType() == typeof(SceneValidator))
            {
                msgs.Add(new Validator.Log
                {
                    Type = LogType.Warning,
                    Msg = "Scene does not have custom validation: " + scene.name
                });
            }        

            GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject gameObject in gameObjects)
            {
                msgs.AddRange(Validate(gameObject));
            }
            return msgs;
        }

        /// <summary>
        /// Validates the open scene
        /// </summary>
        /// <returns></returns>
        public static List<Validator.Log> ValidateActiveScene()
        {
            return ValidateScene(EditorSceneManager.GetActiveScene().path);
        }

        public static List<Validator.Log> ValidateAllScriptableObjects()
        {
            List<Validator.Log> msgs = new List<Validator.Log>();
            string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
      
            for(int i =0; i < soGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(soGuids[i]);
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if(obj.hideFlags != HideFlags.None)
                {
                    continue;
                }

                msgs.AddRange(ValidateObject(obj));
            }
            return msgs;
        }

        public static List<Validator.Log> ValidateAll()
        {
            List<Validator.Log> logs = new List<Validator.Log>();
            logs.AddRange(ValidateBuildScenes());
            logs.AddRange(ValidateAllPrefabs());
            logs.AddRange(ValidateAllScriptableObjects());
            return logs;
        }
        public static List<Validator.Log> ValidateAllPrefabs()
        {
            List<Validator.Log> msgs = new List<Validator.Log>();
            // Find all prefab assets in the specified folder
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            // Iterate through the prefab GUIDs and load the assets
            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
     
                msgs.AddRange(Validate(gameObject));
            }

            return msgs;
        }



        /// <summary>
        /// Validates a gameObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<Validator.Log> Validate(GameObject obj)
        {
            List<Validator.Log> messages = new List<Validator.Log>();
            MonoBehaviour[] monoBehaviours = obj.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour m in monoBehaviours)
            {
               
                if (m is Validator)
                {
                    int count = messages.Count;
                    (m as Validator).Validate(ref messages);
                    if(messages.Count > count)
                    {
                        //check for errors to add button for easy selection
                        for(int i = count; i < messages.Count; i++)
                        {
                            if(messages[i].Type == LogType.Error)
                            {
                                messages.Add(new Validator.Log
                                {
                                    Msg = "Validator on " + obj.name + " has at least one error",
                                    Type = LogType.Error, 
                                    Target = obj
                                });
                                break;
                            }
                        }
                    }
                }
                if (m == null)
                {
                    messages.Add(new Validator.Log
                    {
                        Msg = "Invalid MonoBehaviour on object: " + obj.name,
                        Type = LogType.Error,
                        Target = obj
                    });
                    return messages;
                }

                if(!InTargetAssembly(m.GetType()))
                {
                    continue;
                }
                messages.AddRange(ValidateObject(m));
            }
            return messages;
        }


        
        static List<Field> GetFields(Object obj, System.Type type, Field parent, int step = 0)
        {
            if(step >= 99)
            {
                Debug.LogError(obj.name);
                Debug.LogError("Too many recursive calls were made. Last call for field: " + (parent != null ? parent.field.Name : string.Empty));
                return new List<Field>();
            }

            //base cases
            if(type == null || !InTargetAssembly(type))
            {
                return new List<Field>();
            }

            //Skip monobehaviours during recursive calls.
            //We do not want to process fields of assigned prefabs/classes here. They are processed independently
            if(type.IsClass 
                && type.IsSubclassOf(typeof(MonoBehaviour))
                && step != 0)
            {
                return new List<Field>();
            }

            //Do not get fields of primitives
            if(type.IsPrimitive 
                || type.IsEnum
                || type == typeof(string))
            {
                return new List<Field>();
            }

            

            //Get the fields
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.Public);

            List<Field> result = new List<Field>();

            step++;
            foreach (FieldInfo field in fields)
            {
                Field f = new Field
                {
                    parent = parent,
                    field = field
                };

                //Field may be a struct/class with more fields.
                f.children = GetFields(obj, field.FieldType, f, step);
                result.Add(f);
            }

            type = type.BaseType;
            if (type != null)
            {
     
                //add fields of base class, use 0 to avoid skipping 
                result.AddRange(GetFields(obj, type, null, 0));
            }
            return result;
        }


        class Field
        {
            public Field parent;
            public FieldInfo field;
            public List<Field> children;
            public int ArrayIndex = -1;
        }

        public static List<Validator.Log> ValidateObject(Object obj)
        {
            List<Field> fields = GetFields(obj, obj.GetType(), null);
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(obj);
            List<Validator.Log> logs = new List<Validator.Log>();

            foreach (Field field in fields){
                ProcessField(ref logs, editor, field);
            }
            return logs;
        }

        static SerializedProperty GetProperty(UnityEditor.Editor editor, Field field)
        {
            if (field.parent == null)
            {
                return editor.serializedObject.FindProperty(field.field.Name);
            }

            if(field.ArrayIndex != -1)
            {
                return GetProperty(editor, field.parent).GetArrayElementAtIndex(field.ArrayIndex).FindPropertyRelative(field.field.Name);
            }
            return GetProperty(editor, field.parent).FindPropertyRelative(field.field.Name);
        }

        static void ProcessField(ref List<Validator.Log> logs, UnityEditor.Editor editor, Field field)
        {

            SerializedProperty property = GetProperty(editor, field);
            if(property == null)
            {
                return;
            }

            //We must handle arrays at the serialization step
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                //Type of element in the array

                System.Type elementType = field.field.FieldType.GetElementType();
                if (field.field.FieldType.GenericTypeArguments != null && field.field.FieldType.GenericTypeArguments.Length > 0)
                {
                    elementType = field.field.FieldType.GenericTypeArguments[0];
                }

                for (int i = 0; i < property.arraySize; i++)
                {
                    List<Field> fields = GetFields(editor.target, elementType, field, 1);
                    foreach(Field f in fields)
                    {
                        f.ArrayIndex = i;
                        ProcessField(ref logs, editor, f);
                    }               
                }
            }
            
            RequiredAttribute required = (RequiredAttribute)System.Attribute.GetCustomAttribute(field.field, typeof(RequiredAttribute));
            //field has required attribute
            if (required != null)
            {
                //validate required field?
                if (!RequiredDrawer.IsValid(required, property))
                {
                    string msg = "REQUIRED FIELD: " + field.field.Name + ", OBJECT: " + editor.target.name + ", COMPONENT: " + editor.target.GetType().Name;
                    msg += field.ArrayIndex != -1 ? (", INDEX: " + field.ArrayIndex) : string.Empty;
                    LogType logType = required.Type == RequiredAttribute.Types.Error ? LogType.Error : LogType.Warning;
                    logs.Add(new Validator.Log
                    {
                        Type = logType,
                        Msg = msg,
                        Target = editor.target
                    }
                    );
                }
            }

            if(field.children != null)
            {
                foreach (Field f in field.children)
                {
                    ProcessField(ref logs, editor, f);
                }
            }            
        }


    }
}