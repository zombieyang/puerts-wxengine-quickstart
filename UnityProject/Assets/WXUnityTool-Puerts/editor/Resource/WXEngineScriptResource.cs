namespace WeChat {
    class WXEngineScriptResource: WXResource {
        private WXEngineTextFile scriptFile;
        public WXEngineScriptResource(WXEngineTextFile scriptFile): base("") {
            this.scriptFile = scriptFile;
        }

        public override string GetHash()
        {
            return scriptFile.GetHash();
        }

        public override string GetExportPath() {
            return scriptFile.GetExportPath();
        }

        protected override string GetResourceType()
        {
            return "script";
        }

        protected override JSONObject ExportResource(ExportPreset preset) {
            AddFile(scriptFile);
            return null;
        }

        protected override bool DoExport()
        {
            JSONObject json = ExportResource(usingPreset);

            string exportPath = GetExportPath();
            
            ExportStore.AddResource(
                exportPath,
                GetResourceType(),
                dependencies,
                useFile,
                // 有importSetting的时候才传importSetting
                importSetting == null ? null : importSetting
            );

            return true;
        }
    }
}