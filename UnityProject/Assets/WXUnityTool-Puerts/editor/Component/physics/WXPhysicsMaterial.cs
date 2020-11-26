using UnityEngine;

namespace WeChat {
    public class WXPhysicsMaterial {
        private PhysicMaterial material;
        public WXPhysicsMaterial(PhysicMaterial mat) {
            this.material = mat;
        }

        public JSONObject ToJSON() {
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);

            if (this.material != null)
            {
                data.AddField("dynamicFriction", this.material.dynamicFriction);
                data.AddField("staticFriction", this.material.staticFriction);
                data.AddField("bounciness", this.material.bounciness);
                data.AddField("frictionCombine", (int)this.material.frictionCombine);
                data.AddField("bounceCombine", (int)this.material.bounceCombine);
            }

            return data;
        }
    }
}