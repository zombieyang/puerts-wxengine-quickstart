using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using System.IO;
using System.Text;

namespace WeChat
{

    public class WXRigidbody : WXComponent
    {
        private Rigidbody rigidbody;

        public override string getTypeName() {
            var result = rigidbody ? rigidbody.GetType().ToString() : "UnityEngine.Rigidbody";
            return WXMonoBehaviourExportHelper.EscapeNamespace(result);
        }

        public WXRigidbody(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;
        }

        protected override JSONObject ToJSON(WXHierarchyContext context)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("type", getTypeName());
            json.AddField("data", data);
            data.AddField("active", true);

            if (this.rigidbody != null)
            {
                data.AddField("mass", this.rigidbody.mass);
                data.AddField("drag", this.rigidbody.drag);
                data.AddField("angularDrag", this.rigidbody.angularDrag);
                data.AddField("useGravity", this.rigidbody.useGravity);
                data.AddField("isKinematic", this.rigidbody.isKinematic);
                data.AddField("interpolation", (int)this.rigidbody.interpolation);
                data.AddField("collisionDetectionMode", (int)this.rigidbody.collisionDetectionMode);
                data.AddField("constraints", (int)this.rigidbody.constraints);
            }

            return json;
        }
    }
}
