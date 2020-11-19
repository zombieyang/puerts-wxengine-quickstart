
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

        // 给monobehavior用
        public PuertsSerializableScriptFile(
            string serializableTypeName,
            Dictionary<string, Type> bindingFields,
            List<string> bindingMethods,
            string logicClassName
        ) : base("")
        {
            this.serializableTypeName = serializableTypeName;
            this.bindingFields = bindingFields;
            this.bindingMethods = bindingMethods;
            this.logicClassName = logicClassName;
            isMonoBehaviour = true;
        }
        // 给其他serializable用
        public PuertsSerializableScriptFile(
            string serializableTypeName,
            Dictionary<string, Type> bindingFields,
            List<string> bindingMethods
        ) : base("")
        {
            this.serializableTypeName = serializableTypeName;
            this.bindingFields = bindingFields;
            this.bindingMethods = bindingMethods;
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
                return "/Assets/puerts/serializable/" + this.serializableTypeName + ".binding.ts";
            }
        }

        protected override string GetContent()
        {
            string content = string.Format(@"
import engine from 'engine';
@engine.decorators.serialize('{0}')
export default class {1} ",
                this.serializableTypeName,
                serializableTypeName.Replace(".", "_")
            );
            if (isMonoBehaviour) {
                content += string.Format(@"extends PuertsBehaviour {{
    constructor(entity) {{
        super(entity, '{0}')
    }}",
                    this.logicClassName
                );
            } else {
                content += "{";
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
CS.{0} = {1};
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
                    return string.Format("CS.System.Array$1<CS.{0}>", fixTypeName(itemType, isSerializeName));
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