
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WeChat
{
    class PuertsBeefBallBehaviourConverter : WXComponent
    {
        public override string getTypeName()
        {
            return "PuertsBehaviour";
        }

        public override int GetHashCode()
        {
            return behaviour.GetInstanceID();
        }

        PuertsBeefBallBehaviour behaviour;
        public PuertsBeefBallBehaviourConverter(PuertsBeefBallBehaviour behaviour)
        {
            this.behaviour = behaviour;
        }

        protected override JSONObject ToJSON(WXHierarchyContext context)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);

            json.AddField("type", behaviour.GetType().FullName);
            json.AddField("data", data);

            Dictionary<string, Type> bindingFields = new Dictionary<string, Type>();
            List<string> bindingMethods = new List<string>();

            if (this.behaviour != null)
            {
                BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
                Type myObjectType = behaviour.GetType();
                FieldInfo[] fields = myObjectType.GetFields(flags);

                foreach (FieldInfo field in fields)
                {
                    if (
                        // 排除Action
                        field.FieldType.BaseType != typeof(System.MulticastDelegate) &&

                        !field.IsDefined(typeof(NonSerializedAttribute), true) &&

                        // 排除hideInInspector
                        !field.IsDefined(typeof(HideInInspector), true)
                    )
                    {
                        JSONObject result = WXMonoBehaviourPropertiesHandler.HandleField(field, behaviour, context);

                        if (result != null)
                        {
                            data.AddField(field.Name, result);
                            bindingFields.Add(field.Name, field.FieldType);
                        }
                    }
                }
                MethodInfo[] methods = myObjectType.GetMethods(flags);
                foreach (MethodInfo method in methods) 
                {
                    bindingMethods.Add(method.Name);
                }
            }   

            string script = new WXEngineScriptResource(
                new PuertsSerializableScriptFile(
                    behaviour.GetType().FullName,
                    bindingFields,
                    bindingMethods,
                    behaviour.JSClassName
                )
            ).Export(context.preset);
            
            context.AddResource(script);
            data.AddField("__uuid", script);
            data.AddField("active", true);

            return json;
        }
    }
}