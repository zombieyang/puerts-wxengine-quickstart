declare class PuertsBehaviour {
    constructor(entity: any, JSClassName: string)
    public JsStart: (...args: any[]) => void
    public JsUpdate: (...args: any[]) => void
    public JsOnDestroy: (...args: any[]) => void
    public JsAwake: (...args: any[]) => void
    public JsOnEnable: (...args: any[]) => void
    public JsOnDisable: (...args: any[]) => void
    public jsBehaviour: (...args: any[]) => void
}
declare const MiniGameAdaptor;
declare namespace PuertsBeefBallSDK {

    export function load<T>(path: string): Promise<T>

    export class Prefab {

        constructor(prefab);

        Instantiate(): CS.UnityEngine.GameObject;
    }
}