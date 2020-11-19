namespace WeChat
{
    class PuertsBeefBallRootBehaviour : WXComponent
    {
        public override string getTypeName()
        {
            return "PuertsRoot";
        }
        
        protected override JSONObject ToJSON(WXHierarchyContext context)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject data = new JSONObject(JSONObject.Type.OBJECT);

            PuertsBeefBallRootScript scriptFile = new PuertsBeefBallRootScript();
            string scriptPath = new WXEngineScriptResource(scriptFile).Export(context.preset);
            context.AddResource(scriptPath);

            data.AddField("__uuid", scriptPath);
            data.AddField("active", true);

            json.AddField("type", getTypeName());
            json.AddField("data", data);

            return json;
        }

        class PuertsBeefBallRootScript : WXEngineTextFile
        {
            public PuertsBeefBallRootScript(): base("") {
                
            }

            public override string GetExportPath()
            {
                return "Assets/puerts/lib/rootComponent.ts";
            }

            public override string GetHash()
            {
                return "CONST_RootComponent";
            }
            protected override string GetContent()
            {
                return @"
import './adaptor/index'

@engine.decorators.serialize('PuertsRoot')
export default class PuertsRoot extends engine.Script {
    Awake() {
        if (this.getComponent(MiniGameAdaptor.RootMonoBehaviour) == null) {
            this.addComponent(MiniGameAdaptor.RootMonoBehaviour);
        }
    }
};            
                
                ";
            }
        }
    }
}