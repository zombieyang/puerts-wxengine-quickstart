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

    public class WXMeshCollider : WXComponent
    {
        private MeshCollider collider;

        public override string getTypeName() {
            var result = collider ? collider.GetType().ToString() : "UnityEngine.MeshCollider";
            return WXMonoBehaviourExportHelper.EscapeNamespace(result);
        }

        public WXMeshCollider(MeshCollider collider)
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
                data.AddField("convex", this.collider.convex);
                data.AddField("isTrigger", this.collider.isTrigger);
                data.AddField("cookingOptions", (int)this.collider.cookingOptions);
                data.AddField("material", new WXPhysicsMaterial(this.collider.material).ToJSON());
                Mesh mesh = this.collider.sharedMesh;
                if (mesh != null)
                {
                    WXMesh meshConverter = new WXMesh(mesh);
                    string meshPath = meshConverter.Export(context.preset);
                    data.AddField("mesh", meshPath);
                    context.AddResource(meshPath);
                }
            }

            return json;
        }
    }
}
