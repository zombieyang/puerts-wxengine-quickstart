window.__minigamePrivate = {}
if (!window.MiniGameAdaptor) {
    require('./minigame-adaptor-lib')
    require('./minigame-adaptor-lib.meta')
    require('./minigame-adaptor-lib-patch')
    require('./minigame-adaptor')
}
require('./PuertsLogic')
window.CS = {
    UnityEngine: MiniGameAdaptor,
    MiniGameAdaptor,
    System: window.System
};

window.puerts = {
    $typeof(val) {
        return val;
    },

    $ref(val) {
        const ref = {
            v: val
        }
        Object.defineProperty(ref, "value", {
            set(v) {
                ref.v = v
            },
            get() {
                return ref.v
            }
        })
        return ref;
    }
}

window.PuertsBehaviour = class PuertsBehaviour extends MiniGameAdaptor.MonoBehaviour {
    constructor(entity, JSClassName) {
        super(entity);

        const PuertsLogic = require("../../src/" + JSClassName).default;
        this.jsBehaviour = new PuertsLogic(this);
    }

    Awake() {
        if (this.JsAwake != null) this.JsAwake();
    }
    OnEnable() {
        if (this.JsOnEnable != null) this.JsOnEnable();
    }

    OnDisable() {
        if (this.JsOnDisable != null) this.JsOnDisable();
    }

    Start() {
        if (this.JsStart != null) this.JsStart();
    }

    Update() {
        if (this.JsUpdate != null) this.JsUpdate();
    }

    OnDestroy() {
        if (this.JsOnDestroy != null) this.JsOnDestroy();
        this.JsStart = null;
        this.JsUpdate = null;
        this.JsOnDestroy = null;
        this.JsAwake = null;
        this.JsOnEnable = null;
        this.JsOnDisable = null;
        this.jsBehaviour = null
    }
}

window.PuertsBeefBallSDK = {

    load: function load(path) {
        const promise = engine.loader.load(path).promise;
        return promise.then(result => {
            if (result instanceof engine.Prefab) {
                return new this.Prefab(result);
            }
            return result;
        })
    },

    Prefab: class {

        constructor(prefab) {
            this.prefab = prefab;
        }

        Instantiate() {
            const entity = this.prefab.instantiate();
            engine.game.root.transform.addChild(entity.transform);
            const gameObject = new MiniGameAdaptor.GameObject.$ctor3(entity);
            return gameObject;
        }
    }
}