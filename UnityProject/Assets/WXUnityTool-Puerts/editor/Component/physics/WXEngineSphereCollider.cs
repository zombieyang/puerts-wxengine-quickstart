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
    public class WXSphereCollider : WXComponent
    {
        private SphereCollider collider;

        public override string getTypeName() {
            var result = collider ? collider.GetType().ToString() : "UnityEngine.SphereCollider";
            return WXMonoBehaviourExportHelper.EscapeNamespace(result);
        }

        public WXSphereCollider(SphereCollider collider)
        {
            this.collider = collider;
        }

        protected override JSONObject ToJSON(WXHierarchyContext context)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("type", getTypeName());
            json.AddField("data", data);
            data.AddField("active", true);

            if (this.collider != null)
            {
                data.AddField("isTrigger", this.collider.isTrigger);
                data.AddField("material", new WXPhysicsMaterial(this.collider.material).ToJSON());

                JSONObject center = new JSONObject(JSONObject.Type.ARRAY);
                center.Add(this.collider.center.x);
                center.Add(this.collider.center.y);
                center.Add(this.collider.center.z);
                data.AddField("center", center);

                data.AddField("radius", this.collider.radius);
            }

            return json;
        }
    }
}