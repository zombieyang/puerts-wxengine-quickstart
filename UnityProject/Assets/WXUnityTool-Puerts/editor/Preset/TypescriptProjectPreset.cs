using System.Net.Mime;
using System.IO;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace WeChat
{
    [InitializeOnLoad]
    [DeclarePreset("typescript", null)]
    public class TypescriptExportPreset : ExportPreset
    {
        static TypescriptExportPreset()
        {
            ExportPreset.registerExportPreset("typescript", new TypescriptExportPreset());
        }

        // TS项目根目录
        static public string basePath = Path.Combine(Application.dataPath, "../Typescript/src/");

        public TypescriptExportPreset() : base()
        {
        }

        public override string GetChineseName()
        {
            return "PuerTS项目代码";
        }

        protected override void DoExport()
        {
            // 把所有开发者ts源代码文件带上
            List<string> tsFilePaths = new List<string>();
            this.findTSRecursive(
                basePath,
                tsFilePaths
            );

            List<string> scriptResources = new List<string>();
            foreach (string tsPath in tsFilePaths)
            {
                PuertsSourceCodeScriptFile file = new PuertsSourceCodeScriptFile(
                    tsPath,
                    "Assets/puerts/src/" + tsPath.Substring(tsPath.IndexOf(basePath) + basePath.Length)
                );
                scriptResources.Add(new WXEngineScriptResource(file).Export(this));
            }

            // 把puerts生成的dts文件带上
            PuertsSourceCodeScriptFile dtsFile = new PuertsSourceCodeScriptFile(
                Path.Combine(Application.dataPath, "Gen/Typing/csharp/index.d.ts"),
                "Assets/puerts/dts/csharp.d.ts"
            );
            scriptResources.Add(new WXEngineScriptResource(dtsFile).Export(this));

            // 把adaptor带上并且放入node_modules
            string[] adaptorFiles = {
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/index.js",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/minigame-adaptor-lib-patch.js",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/minigame-adaptor-lib.js",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/minigame-adaptor-lib.meta.js",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/minigame-adaptor.js",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/package.json",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/puerts-beefball.d.ts",
                Application.dataPath + "/WXUnityTool-Puerts/res~/puerts-beefball/PuertsLogic.ts"
            };
            foreach (string adaptorFile in adaptorFiles) {
                scriptResources.Add(
                    new WXEngineScriptResource(
                        new PuertsSourceCodeScriptFile(
                            adaptorFile,
                            Path.Combine("Assets/puerts/lib/adaptor/", Path.GetFileName(adaptorFile))
                        )
                    ).Export(this)
                );
            }

            ExportStore.GenerateResourcePackage("puerts", scriptResources);
        }

        // 递归找到所有ts
        protected void findTSRecursive(string targetDirectoryPath, List<string> tsFilePaths)
        {
            string[] filePaths = Directory.GetFiles(targetDirectoryPath);

            foreach (string filePath in filePaths)
            {
                if (Path.GetExtension(filePath) == ".ts")
                {
                    tsFilePaths.Add(filePath);
                }
            }

            string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
            foreach (string directoryPath in directoryPaths)
            {
                findTSRecursive(directoryPath, tsFilePaths);
            }
        }

        public override bool WillPresetShow()
        {
            return true;
        }

    }
}