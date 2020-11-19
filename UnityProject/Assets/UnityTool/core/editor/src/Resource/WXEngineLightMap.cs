using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeChat
{

    class WXLightMap : WXResource
    {
        protected override string GetResourceType()
        {
            return "texture2d";
        }

        private Texture2D lightmapColor;
        public WXLightMap(Texture2D _lightmapColor): base(AssetDatabase.GetAssetPath(_lightmapColor.GetInstanceID()))
        {
            lightmapColor = _lightmapColor;
            if (unityAssetPath == null || unityAssetPath == "")
            {
                ErrorUtil.ExportErrorReporter.create()
                .setResource(this)
                .error(0, "Lightmap文件的unity路径为空");
            }
        }

        public override string GetExportPath()
        {
            return wxFileUtil.cleanIllegalChar(unityAssetPath.Split('.')[0], false) + ".texture2d";
        }

        public override string GetHash()
        {
            return WXUtility.GetMD5FromAssetPath(unityAssetPath);
        }

        protected override JSONObject ExportResource(ExportPreset preset)
        {
            if (string.IsNullOrEmpty(unityAssetPath))
            {
                Debug.LogError("WXBeefBallTexture null lightmap.");
                return null;
            }

            JSONObject metadata = JSONObject.Create("{\"data\":{}, \"file\":{}}");
            string texturePath = AddFile(new TextureImageFile(lightmapColor));
            metadata.GetField("file").AddField("src", texturePath);
            metadata.SetField("data", TextureUtil.getMeta(lightmapColor));

            var editorInfo = new JSONObject(JSONObject.Type.OBJECT);
            editorInfo.AddField("assetVersion", 2);

            metadata.AddField("editorInfo", editorInfo);
            return metadata;
        }

        // 这个跟Texture2D.cs里一样，后面DRY掉
        private class TextureImageFile : WXEngineImageFile
        {
            private Texture2D copyTexture;

            public TextureImageFile(Texture2D sourceTexture)
                : base(AssetDatabase.GetAssetPath(sourceTexture.GetInstanceID()))
            {
                copyTexture = TextureUtil.DuplicateTexture2D(sourceTexture);
            }

            public override string GetExportPath()
            {
                if (TextureUtil.ResolveFileExt(copyTexture.format) == TextureUtil.EnumTexFileExt.JPG)
                {
                    return wxFileUtil.cleanIllegalChar(unityAssetPath.Split('.')[0], false) + ".jpg";
                }
                else
                {
                    return wxFileUtil.cleanIllegalChar(unityAssetPath.Split('.')[0], false) + ".png";
                }
            }

            protected override byte[] GetContent()
            {

                if (TextureUtil.ResolveFileExt(copyTexture.format) == TextureUtil.EnumTexFileExt.JPG)
                {
                    return copyTexture.EncodeToJPG();
                }
                else
                {
                    return copyTexture.EncodeToPNG();
                }
            }
        }
    }

}