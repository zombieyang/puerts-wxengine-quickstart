using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WeChat
{

    public class WXMeshRenderer : WXComponent
    {
        private MeshRenderer renderer;

        public override string getTypeName()
        {
            return "MeshRenderer";
        }

        public WXMeshRenderer(MeshRenderer renderer)
        {
            this.renderer = renderer;
        }

        protected override JSONObject ToJSON(WXHierarchyContext context)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("type", "MeshRenderer");
            json.AddField("data", data);

            MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                WXMesh meshConverter = new WXMesh(mesh);
                string meshPath = meshConverter.Export(context.preset);
                data.AddField("mesh", meshPath);
                context.AddResource(meshPath);
            }
            else
            {
                ErrorUtil.ExportErrorReporter.create()
                    .setGameObject(renderer.gameObject)
                    .setHierarchyContext(context)
                    .error(0, "Mesh资源转换失败，没法拿到对应的MeshFilter或者它上面的mesh");
            }

            JSONObject materialArray = new JSONObject(JSONObject.Type.ARRAY);
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                if (material != null)
                {
                    WXMaterial materialConverter = new WXMaterial(material, renderer);
                    string materialPath = materialConverter.Export(context.preset);
                    materialArray.Add(materialPath);
                    context.AddResource(materialPath);
                }
            }
            data.AddField("materials", materialArray);

            int lightmapIndex = renderer.lightmapIndex;
            JSONObject litmapScaleArr = new JSONObject(JSONObject.Type.ARRAY);
            data.AddField("lightMapScaleOffset", litmapScaleArr);
            litmapScaleArr.Add(renderer.lightmapScaleOffset.x);
            litmapScaleArr.Add(renderer.lightmapScaleOffset.y);
            litmapScaleArr.Add(renderer.lightmapScaleOffset.z);
            litmapScaleArr.Add(renderer.lightmapScaleOffset.w);
            data.AddField("lightMapIndex", lightmapIndex);

            ShadowCastingMode mode = renderer.shadowCastingMode;
            StaticEditorFlags shadowFlags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            if (mode == ShadowCastingMode.Off || (shadowFlags & StaticEditorFlags.LightmapStatic) != 0)
            {
                data.AddField("castShadow", false);
            }
            else
            {
                data.AddField("castShadow", true);
            }

            bool receiveShadow = renderer.receiveShadows;
            data.AddField("receiveShadow", receiveShadow);
            return json;
        }
    }


}

// WXBeefBallComponentExporter.TypeStruct data = new WXBeefBallComponentExporter.TypeStruct();
// data.componentType = typeof(Transform);
// data.exporterType = typeof(WXBBTransform3DComponent);
// WXBeefBallComponentExporter.typeMap.Add()