using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace WeChat
{

    /**
     * 专门处理导出插件错误的通用类
     */
    public class ErrorUtil
    {

        public static void InitializeError()
        {

        }

        public static void InitializeWarning()
        {

        }

        /**
         * 错误报告器，方便开发者找到出问题的资源或者节点
         * ExportErrorReporter reporter = new ExportErrorReporter();
         * reporter.setEntity(xxx).setResource(xxx).warn(2, "警告信息");
         */
        public class ExportErrorReporter
        {
            public static ExportErrorReporter create()
            {
                return new ExportErrorReporter();
            }
            // 对warn进行计数，防止导出完成后开发者没看到
            public static int warnCount;
            public static void cleanWarnCount()
            {
                warnCount = 0;
            }

            protected WXEntity entity;
			protected GameObject gameObject;
            protected WXHierarchyContext hierarchyContext;
            protected WXResource resource;
            protected Exception exception;

			public ExportErrorReporter setEntity(WXEntity entity)
			{
				this.entity = entity;
				return this;
			}
			public ExportErrorReporter setGameObject(GameObject gameObject)
			{
				this.gameObject = gameObject;
				return this;
			}
			public ExportErrorReporter setHierarchyContext(WXHierarchyContext hierarchyContext)
            {
                this.hierarchyContext = hierarchyContext;
                return this;
            }
            public ExportErrorReporter setResource(WXResource resource)
            {
                this.resource = resource;
                return this;
            }

            public void warn(int id, string message)
            {
                string messageTemplate = string.Format("WARN({0})：{1}\n", id, message);

				if (entity != null)
				{
					messageTemplate += string.Format("相关GameObject名称：{0}。\n", entity.gameObject.name);
				}
				else if (gameObject != null)
				{
					messageTemplate += string.Format("相关GameObject名称：{0}。\n", gameObject.name);
				}
				if (hierarchyContext != null)
                {
                    messageTemplate += string.Format("相关Prefab和Scene路径：{0}。\n", hierarchyContext.resourcePath);
                }
                if (resource != null)
                {
                    messageTemplate += string.Format("相关资源路径：{0}。\n", resource.GetExportPath());
                }

                Debug.LogWarning(messageTemplate);
                warnCount++;
            }

            public void error(int id, string message)
            {
                string messageTemplate = string.Format("ERROR({0})：{1}。\n", id, message);

				if (entity != null)
				{
					messageTemplate += string.Format("相关GameObject名称：{0}。\n", entity.gameObject.name);
				}
				else if (gameObject != null)
				{
					messageTemplate += string.Format("相关GameObject名称：{0}。\n", gameObject.name);
				}
                if (hierarchyContext != null)
                {
                    messageTemplate += string.Format("相关Prefab和Scene路径：{0}。\n", hierarchyContext.resourcePath);
                }
                if (resource != null)
                {
                    messageTemplate += string.Format("相关资源路径：{0}。\n", resource.GetExportPath());
                }

                Debug.LogError(messageTemplate);
                throw new Exception("遇到以上错误，终止导出。");
            }
        }
    }

}