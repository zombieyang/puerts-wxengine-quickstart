using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WeChat
{
    public class WXTexture : WXResource
    {

        private Texture2D texture2D;
        public WXTexture(Texture2D _texture): base(AssetDatabase.GetAssetPath(_texture.GetInstanceID()))
        {
            texture2D = _texture;
            if (unityAssetPath == null || unityAssetPath == "")
            {
                ErrorUtil.ExportErrorReporter.create()
                .setResource(this)
                .error(0, "Texture文件的unity路径为空");
            }
        }

        protected override string GetResourceType()
        {
            return "texture2d";
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
            JSONObject metadata = JSONObject.Create("{\"data\":{}, \"file\":{}}");
            string texturePath = AddFile(new TextureImageFile(texture2D));
            metadata.GetField("file").AddField("src", texturePath);
            metadata.SetField("data", TextureUtil.getMeta(texture2D));

            var editorInfo = new JSONObject(JSONObject.Type.OBJECT);
            editorInfo.AddField("assetVersion", 2);

            metadata.AddField("editorInfo", editorInfo);
            return metadata;
        }


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
