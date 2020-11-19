declare const global;
class PuertsLogic<T extends CS.WeChat.PuertsBeefBallBehaviour> {
    public behaviour: T

    constructor(behaviour) {
        this.behaviour = behaviour;
        this.behaviour.JsStart = () => this.Start();
        this.behaviour.JsUpdate = () => {
            // if (this.monoBehaviour.name == "Player1") {
            //     csharp.UnityEngine.Debug.Log(this.monoBehaviour.JSClassName);
            // }
            this.Update();
        }
        this.behaviour.JsAwake = () => this.Awake();
        this.behaviour.JsOnDestroy = () => this.OnDestroy();
        this.behaviour.JsOnDisable = () => this.OnDisable();
        this.behaviour.JsOnEnable = () => this.OnEnable();
        (this.behaviour as any).jsBehaviour = this;
    }
    Start() {}
    Update() {}
    Awake() {}
    OnDestroy() {}
    OnDisable() {}
    OnEnable() {}
}

namespace PuertsBeefBallSDK {
    export async function load<T>(path: string | CS.PuertsBeefBallSDK.RemoteResource): Promise<T> {
        let resourcepath: string = '';
        if (typeof path !== 'string') {
            resourcepath = path.resourcePath;

        } else {
            resourcepath = path;
        } 

        // 在unity里加载，要求资源必须是在resources目录下的资源
        // 且传入的参数得是resources目录后的路径，所以要预先处理一下传入的path
        if (resourcepath.toLowerCase().indexOf("resources") == -1) {
            return Promise.reject(new Error(`加载资源"${path}"时失败：资源路径不包含resources，无法加载`));
        }
        const reg = resourcepath.split(/resources\//i);
        resourcepath = reg[reg.length - 1];
        resourcepath = resourcepath.slice(0, resourcepath.lastIndexOf("."));
        
        const result = CS.UnityEngine.Resources.Load(resourcepath)
        if (result instanceof CS.UnityEngine.GameObject) {
            return Promise.resolve(new PuertsBeefBallSDK.Prefab(result) as any as T);
        }
        return Promise.resolve(result as any as T);
    }

    export class Prefab {

        protected gameObject: CS.UnityEngine.GameObject;

        constructor(gameObject: CS.UnityEngine.GameObject) {
            this.gameObject = gameObject;
        }

        public Instantiate() {
            return CS.UnityEngine.GameObject
                .Instantiate(this.gameObject) as CS.UnityEngine.GameObject;
        }
    }
}

(global as any).CS = require('csharp');
(global as any).puerts = require('puerts');
(global as any).PuertsLogic = PuertsLogic;
(global as any).PuertsBeefBallSDK = PuertsBeefBallSDK;
