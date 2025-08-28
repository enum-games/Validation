using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace enumGames.Validation
{

    /// <summary>
    /// Implement this class to validate your objects. Custom inspector will draw all log messages when object is selected
    /// </summary>
    public abstract class Validator : MonoBehaviour
    {
        static GUIStyle m_logStyle, m_warningStyle, m_errorStyle;
        public static GUIStyle LogStyle
        {
            get
            {
                if (m_logStyle == null)
                {
                    m_logStyle = new GUIStyle();
                    m_logStyle.normal.textColor = Color.white;
                }
                return m_logStyle;
            }
        }
        public static GUIStyle WarningStyle
        {
            get
            {
                if (m_warningStyle == null)
                {
                    m_warningStyle = new GUIStyle();
                    m_warningStyle.normal.textColor = Color.yellow;
                }
                return m_warningStyle;
            }
        }
        public static GUIStyle ErrorStyle
        {
            get
            {
                if (m_errorStyle == null)
                {
                    m_errorStyle = new GUIStyle();
                    m_errorStyle.normal.textColor = Color.red;
                }
                return m_errorStyle;
            }
        }


        public class Log
        {
            public LogType Type;
            public string Msg;
            public Object Target;

            public override string ToString()
            {
                if(Type == LogType.Error)
                {
                    return "ERROR: " + Msg;
                }
                else if(Type == LogType.Warning)
                {
                    return "WARNING: " + Msg;
                }
                return Msg;
            }
        }

        /// <summary>
        /// I
        /// </summary>
        /// <param name="assert"></param>
        /// <param name="errorMsg"></param>
        /// <param name="logs"></param>
        public static Log AssertTrue(bool assert, string errorMsg, ref List<Log> logs, LogType type = LogType.Error)
        {
            if (!assert)
            {
                Log log = new Log
                {
                    Type = type,
                    Msg = errorMsg
                };
                logs.Add(log);
                return log;
            }
            return null;
        }


        public abstract void Validate(ref List<Log> logs);
    }
}

