using System.IO;
using UnityEngine;

namespace WeChat
{
    class PuertsSourceCodeScriptFile : WXEngineTextFile
    {
        protected string exportPath;
        public PuertsSourceCodeScriptFile(string filePath) : base(filePath)
        {
            exportPath = filePath;
        }

        public PuertsSourceCodeScriptFile(string filePath, string exportPath) : base(filePath)
        {
            this.exportPath = exportPath;
        }

        public override string GetExportPath()
        {
            return exportPath;
            // if (unityAssetPath.Contains(basePath))
            // {
            //     return Path.Combine(
            //         "Assets/puerts/src/",
            //         unityAssetPath.Substring(unityAssetPath.IndexOf(basePath) + basePath.Length)
            //     );
            // }
            // else if (unityAssetPath.Contains(Application.dataPath))
            // {
            //     return Path.Combine(
            //         "Assets/puerts/lib/",
            //         unityAssetPath.Substring(unityAssetPath.IndexOf(Application.dataPath) + Application.dataPath.Length + 1/*末尾多个/*/)
            //     );
            // }
            // else
            // {
            // }
        }

        protected override string GetContent()
        {
            return File.ReadAllText(unityAssetPath);
        }
    }
}