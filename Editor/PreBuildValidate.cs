using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;


namespace enumGames.Validation.Editor
{
    public class PreBuildValidate : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;



        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Builds.Builder.ValidateBuild)
            {
                return;
            }
            List<Validator.Log> logs = ValidatorUtil.ValidateAll();
            int errorCount = 0;
            logs.ForEach(log =>
            {
                if (log.Type == LogType.Error)
                {
                    errorCount++;
                }
            });

            ValidationWindow.ShowWindow(logs);
            WriteLogFile(report, logs);            

            if(errorCount > 0)
            {
                throw new BuildFailedException("Validation errors: " + errorCount);
            }           
        }



        void WriteLogFile(BuildReport report, List<Validator.Log> logs)
        {
            string parent = Directory.GetParent(report.summary.outputPath).FullName;
            string logsDirectory = Path.Combine(parent, "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
            string logFilePath = Path.Combine(logsDirectory, "ValidationLog.txt");

            StreamWriter writer = new StreamWriter(logFilePath);
            
            foreach(Validator.Log log in logs)
            {
                writer.WriteLine(log.ToString());
            }
            writer.Close();
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", logsDirectory);
#endif
        }
    }
}

