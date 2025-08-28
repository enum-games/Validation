using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace enumGames.Validation
{
    public class RequiredAttribute : PropertyAttribute
    {
        public enum Types { Error, Warning};

        string customMessage = string.Empty;

        Types type = Types.Error;

        public string CustomMessage => customMessage;
        public Types Type => type;


        public float Min => min;
        public float Max => max;


        float min = -1f, max = -1f;
        public RequiredAttribute()
        {

        }

        /// <summary>
        /// Required a float to be within range [min, max] inclusive
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public RequiredAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public RequiredAttribute(Types type)
        {
            this.type = type;
        }
        public RequiredAttribute(string customMessage)
        {
            this.customMessage = customMessage;
        }

        public RequiredAttribute(Types type, string customMessage)
        {
            this.type = type;
            this.customMessage = customMessage;
        }

    }
}
