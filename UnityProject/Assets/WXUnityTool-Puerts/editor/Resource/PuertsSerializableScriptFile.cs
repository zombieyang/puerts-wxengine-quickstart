using System.IO;

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WeChat
{
    class PuertsSerializableScriptFile : WXEngineTextFile
    {
        private string serializableTypeName;
        private Dictionary<string, Type> bindingFields;
        private List<string> bindingMethods;

        private bool isMonoBehaviour;
        private string logicClassName;
        private List<string> requireList;

        // 用于构造一个monobehaviour
        public PuertsSerializableScriptFile(
            string serializableTypeName,
            Dictionary<string, Type> bindingFields,
            List<string> bindingMethods,
            string logicClassName,
            List<string> requireList
        ) : base("")
        {
            this.serializableTypeName = serializableTypeName;
            this.bindingFields = bindingFields;
            this.bindingMethods = bindingMethods;
            this.logicClassName = logicClassName;
            this.requireList = requireList;
            isMonoBehaviour = true;
        }
        // 用于构造一个非monobehavior的serializable
        public PuertsSerializableScriptFile(
            string serializableTypeName,
            Dictionary<string, Type> bindingFields,
            List<string> bindingMethods
        ) : base("")
        {
            this.serializableTypeName = serializableTypeName;
            this.bindingFields = bindingFields;
            this.bindingMethods = bindingMethods;
            isMonoBehaviour = false;
        }

        public override string GetExportPath()
        {
            if (isMonoBehaviour)
            {
                TextAsset scriptAsset = Resources.Load<TextAsset>("src/" + this.logicClassName + ".js");
                unityAssetPath = AssetDatabase.GetAssetPath(scriptAsset.GetInstanceID());
                return unityAssetPath.Replace(".js.txt", ".binding.ts");

            }
            else
            {
                return "Assets/puerts/serializable/" + this.serializableTypeName + ".binding.ts";
            }
        }

        protected override string GetContent()
        {
            string content = "import engine from 'engine';";
            
            if (isMonoBehaviour) {
                if (requireList.Count != 0) {
                    // 加入require语句
                    int stepToAssets = 0;
                    string path = GetExportPath();
                    while (path != "Assets") {
                        stepToAssets++;
                        path = Path.GetDirectoryName(path);

                        if (stepToAssets > 20) {
                            throw new Exception("无法找到脚本位置");
                        }
                    }
                    string upward = "";
                    for (int i = 0; i < stepToAssets; i++) {
                        upward += "../";
                    }

                    foreach (string require in requireList) {
                        content += string.Format("\nimport '{0}';", upward + require.Substring(0, require.Length - Path.GetExtension(require).Length));
                    } 
                }

                content += string.Format(@"
@engine.decorators.serialize('{0}')
export default class {1} extends PuertsBehaviour {{
    constructor(entity) {{
        super(entity, '{2}')
    }}",
                    this.serializableTypeName,
                    serializableTypeName.Replace(".", "_"),
                    this.logicClassName
                );

            } else {
                content += string.Format(@"
@engine.decorators.serialize('{0}')
export default class {1} {{",
                    this.serializableTypeName,
                    serializableTypeName.Replace(".", "_")
                );
            }

            foreach (string key in bindingFields.Keys)
            {
                content = content + string.Format(@"
    @engine.decorators.property({{
        type: {0}
    }})
    public {1}: {2}",
                    fixTypeName(bindingFields[key], true).Replace("UnityEngine", "MiniGameAdaptor"),
                    key,
                    fixTypeName(bindingFields[key], false)
                );
            }
            foreach (string methodName in bindingMethods)
            {
                content = content + string.Format(@"
    public {0}(...args) {{
        this.Js{0} && this.Js{0}(...args)
    }}
                    ", methodName);
            }
            content += string.Format(@"
}}
//@ts-ignore
registerPuertsClass('{0}', {1});
                ", serializableTypeName, serializableTypeName.Replace(".", "_"));
            return content;
        }

        // 转化基础数据类型为number等
        // serializeName代表是微信引擎的序列化名字还是Typescript的类型名字
        private string fixTypeName(Type type, bool isSerializeName)
        {
            string typeName = type.FullName;
            if (
                typeName.Equals("System.Single") ||
                typeName.Equals("System.Double") ||
                typeName.Equals("System.Int16") ||
                typeName.Equals("System.Int32") ||
                typeName.Equals("System.Int64")
            )
            {
                if (isSerializeName)
                {
                    return @"""number""";

                }
                else
                {
                    return "number";
                }
            }
            else if (typeName.Equals("System.Boolean"))
            {
                if (isSerializeName)
                {
                    return @"""boolean""";

                }
                else
                {
                    return "boolean";
                }
            }
            else if (typeName.Equals("System.String"))
            {
                if (isSerializeName)
                {
                    return @"""string""";

                }
                else
                {
                    return "string";
                }
            }
            else if (type == typeof(PuertsBeefBallSDK.RemoteResource)) 
            {   
                if (isSerializeName)
                {
                    return @"""string""";

                }
                else
                {
                    return "string";
                }
            }
            else if (type.IsArray/*还有List*/)
            {
                Type itemType = type.GetElementType();

                if (isSerializeName)
                {
                    return String.Format("CS.UnityEngine.ListFactory({{ 'type': {0}, 'isArray' : {1} }})", fixTypeName(itemType, isSerializeName), "true");

                }
                else
                {
                    return string.Format("CS.System.Array$1<{0}>", fixTypeName(itemType, isSerializeName));
                }
            }
            // else if (type.IsParameterized && type.GetDefinition() != null &&
            //     type.GetDefinition().FullTypeName.Name.Equals("List", StringComparison.CurrentCulture))
            // {
            //     var paramType = type as ParameterizedType;
            //     IType itemType = paramType.GetTypeArgument(0);

            //     if (!importedList.Contains(itemType))
            //     {
            //         importedList.Add(itemType);
            //         var tpSrcPath = findImportFilePath(itemType);
            //         if (!String.IsNullOrWhiteSpace(tpSrcPath))
            //         {
            //             var tpOutputPath = convertToOutputPath(translator, outputPath, tpSrcPath);
            //             if (!tpOutputPath.Equals(jsFilePath, StringComparison.CurrentCulture))
            //             {
            //                 var importPath = GetRelativePath(tpOutputPath, Path.GetDirectoryName(jsFilePath));
            //                 importPath = new ConfigHelper().ConvertPath(importPath, '/');
            //                 properties += String.Format("import '{0}'\r\n", importPath);
            //             }
            //         }
            //     }

            //     string itemstr = GetValidTypeName(getGeneralType(itemType));
            //     typestr = String.Format("UnityEngine.ListFactory({{ 'type': '{0}', 'isArray' : {1} }})", itemstr, "false");
            // }
            else if (type.IsEnum)
            {
                if (isSerializeName)
                {
                    return @"""number""";

                }
                else
                {
                    return "number";
                }
            }
            // else if (type.GetDefinition() != null && type.GetDefinition().GetAttribute(fieldInfo.Compilation.FindType(typeof(SerializableAttribute))) != null)
            // {
            //     if (!importedList.Contains(type))
            //     {
            //         importedList.Add(type);
            //         var tpSrcPath = findImportFilePath(type);
            //         if (!String.IsNullOrWhiteSpace(tpSrcPath))
            //         {
            //             var tpOutputPath = convertToOutputPath(translator, outputPath, tpSrcPath);
            //             if (!tpOutputPath.Equals(jsFilePath, StringComparison.CurrentCulture))
            //             {
            //                 var importPath = GetRelativePath(tpOutputPath, Path.GetDirectoryName(jsFilePath));
            //                 importPath = new ConfigHelper().ConvertPath(importPath, '/');
            //                 properties += String.Format("import '{0}'\r\n", importPath);
            //             }
            //         }

            //     }
            //     typestr = type.ToString();

            // }
            // else
            // {
            //     // UnityEngine.Object的wrapper处理

            //     if (IsSubClassOf(type, "UnityEngine.Object") && fieldInfo.GetAttribute(fieldInfo.Compilation.FindType(typeof(NonSerializedAttribute))) == null)
            //     {
            //         typestr = "UnityEngine.UnityComponentWrapper(\"" + type.ToString() + "\")";
            //     }
            //     else
            //     {
            //         if (tpName.Equals("UnityEngine.Vector2") || tpName.Equals("UnityEngine.Vector3") || tpName.Equals("UnityEngine.Vector4")
            //          || tpName.Equals("UnityEngine.Quaternion") || tpName.Equals("UnityEngine.Matrix4x4") || tpName.Equals("UnityEngine.Color")
            //          || tpName.Equals("UnityEngine.Rect") || tpName.Equals("UnityEngine.LayerMask"))
            //         {
            //             typestr = GetValidTypeName((type.ToString()));
            //         }
            //         else
            //         {
            //             continue;
            //         }
            //     }
            // }

            if (isSerializeName)
            {
                bool isComponent = false;
                Type findingComponent = type.BaseType;
                while (findingComponent != typeof(System.Object))
                {
                    if (findingComponent.Equals(typeof(UnityEngine.Component)))
                    {
                        isComponent = true;
                        break;
                    }
                    findingComponent = findingComponent.BaseType;
                }

                // Debug.Log(isComponent);
                if (isComponent)
                {
                    return "MiniGameAdaptor.UnityComponentWrapper(\"" + type.FullName + "\")";

                }
                else
                {
                    return "\"" + type.FullName + "\"";
                }

            }
            else
            {
                return "CS." + type.FullName;
            }
        }
    }
}