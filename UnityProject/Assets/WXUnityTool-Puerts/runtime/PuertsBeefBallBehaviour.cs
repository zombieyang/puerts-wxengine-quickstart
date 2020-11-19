using UnityEngine;
using Puerts;
using System;

namespace WeChat
{
    public delegate void ModuleInit(PuertsBeefBallBehaviour monoBehaviour);

    //只是演示纯用js实现MonoBehaviour逻辑的可能，
    //但从性能角度这并不是最佳实践，会导致过多的跨语言调用
    public class PuertsBeefBallBehaviour : MonoBehaviour
    {
        public string JSClassName;//可配置加载的js模块

        public Action JsStart;
        public Action JsAwake;
        public Action JsUpdate;
        public Action JsOnDestroy;
        public Action JsOnEnable;
        public Action JsOnDisable;

        static JsEnv jsEnv;

        void Awake()
        {
            if (jsEnv == null) {
                jsEnv = new JsEnv();
                jsEnv.Eval("require('lib/puerts-sdk')");
                jsEnv.Eval("global.CS = require('csharp')");
            }

            if (JSClassName == "" || JSClassName == null) 
            { 
                throw new Exception("invalid JSClassName");
            }

            var init = jsEnv.Eval<ModuleInit>("(...args) => { const m = (require('src/" + JSClassName + "').default); return new m(...args); }");

            if (init != null) init(this);

            if (JsAwake != null) JsAwake();
        }

        void OnEnable() 
        {
            if (JsOnEnable != null) JsOnEnable();
        }

        void OnDisable() 
        {
            if (JsOnDisable != null) JsOnDisable();
        }

        void Start()
        {
            if (JsStart != null) JsStart();
        }

        void Update()
        {
            if (JsUpdate != null) JsUpdate();
            jsEnv.Tick();
        }

        void OnDestroy()
        {
            if (JsOnDestroy != null) JsOnDestroy();
            JsStart = null;
            JsUpdate = null;
            JsOnDestroy = null;
            JsAwake = null;
            JsOnEnable = null;
            JsOnDisable = null;
        }
    }
}