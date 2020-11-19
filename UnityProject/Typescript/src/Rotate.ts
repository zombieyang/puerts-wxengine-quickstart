




export default class Rotate extends PuertsLogic<CS.Rotate> {
    constructor(behaviour) {
        super(behaviour);
    }

    protected box: CS.UnityEngine.Transform;
    async Awake() {
        const prefab: PuertsBeefBallSDK.Prefab = await PuertsBeefBallSDK
            .load<PuertsBeefBallSDK.Prefab>(this.behaviour.prefab);

        this.box = prefab.Instantiate().transform;
        this.box.parent = this.behaviour.transform;

        // unity在设置物体parent后，会改变物体的position以保持它worldPosition不变。
        // 微信引擎则不会
        // adaptor还没处理，这里要先兼容一下。
        const vector = new CS.UnityEngine.Vector3(0, 0, 0)
        this.box.position = vector;
        // this.box.localPosition = vector
    }

    Update() {
        if (this.box) {
            let r = CS.UnityEngine.Vector3.op_Multiply(
                CS.UnityEngine.Time.deltaTime * 10,
                CS.UnityEngine.Vector3.up
            );
            this.box.transform.Rotate(r.x, r.y, r.z);
        }
    }
}