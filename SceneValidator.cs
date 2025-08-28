using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace enumGames.Validation
{
    [ExecuteInEditMode]
    /// <summary>
    /// Each scene is required to have a SceneValidator script when using enum Builds
    /// </summary>
    public class SceneValidator : Validator
    {

        public string ActiveScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;



        public override void Validate(ref List<Log> logs)
        {
      
        }
    }
}