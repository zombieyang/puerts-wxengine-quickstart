import engine from "engine";

export default class PuertsLogic<T extends PuertsBehaviour> {
    protected behaviour: T;

    constructor(script: T) {
        this.behaviour = script;
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
    }

    public Start(): void {
        
    }
    public Update(): void {
        
    }
    public Awake(): void {
        
    }
    public OnDestroy(): void {
        
    }
    public OnDisable(): void {
        
    }
    public OnEnable(): void {
        
    }
}

(window as any).PuertsLogic = PuertsLogic;