using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisServices.Deployment
{
    class CmdOptions
    {
        public bool ShowHelp;
        
        public bool DeployMode;
        public string ModelFile = string.Empty;
        public string TargetFile = string.Empty;
        public string OptionsFile = string.Empty;
        public string ConfigFile = string.Empty;
        public string SecurityFile = string.Empty;
        public string AssemblyFile = string.Empty;
        public string ErrorMsg = string.Empty;
        public bool IsDeploy;

        public bool ImpersonateMode;
        public string DataSourceID = string.Empty;
        public string UserName = string.Empty;
        public string Password = string.Empty;
        public string SaveFile = string.Empty;
        public bool IsImpersonate;

        public string LogFile = string.Empty;

        private string ModelDirectory;

        public CmdOptions(string[] args)
        {
            ModelDirectory = Directory.GetCurrentDirectory();

            if (args.Length == 0) { ShowHelp = true; }
            foreach(string arg in args)
            {
                if ((arg.Equals("/?", StringComparison.OrdinalIgnoreCase)) || (arg.Equals("/h", StringComparison.OrdinalIgnoreCase)))
                    { ShowHelp = true; }
                if (arg.Equals("/d", StringComparison.OrdinalIgnoreCase))
                    { DeployMode = true; }
                if (arg.StartsWith("/m:", StringComparison.OrdinalIgnoreCase))
                    { ModelFile = Path.Combine(ModelDirectory, arg.Substring(3)); }
                if (arg.StartsWith("/t:", StringComparison.OrdinalIgnoreCase))
                    { TargetFile = Path.Combine(ModelDirectory, arg.Substring(3)); }
                if (arg.StartsWith("/o:", StringComparison.OrdinalIgnoreCase))
                    { OptionsFile = Path.Combine(ModelDirectory, arg.Substring(3)); }
                if (arg.StartsWith("/c:", StringComparison.OrdinalIgnoreCase))
                    { ConfigFile = Path.Combine(ModelDirectory, arg.Substring(3)); }
                if (arg.StartsWith("/s:", StringComparison.OrdinalIgnoreCase))
                    { SecurityFile = Path.Combine(ModelDirectory, arg.Substring(3)); }
                if (arg.StartsWith("/a:", StringComparison.OrdinalIgnoreCase))
                    { AssemblyFile = Path.Combine(ModelDirectory, arg.Substring(3)); }
                if (arg.StartsWith("/l:", StringComparison.OrdinalIgnoreCase))
                    { LogFile = Path.Combine(ModelDirectory, arg.Substring(3)); }

                if (arg.Equals("/i", StringComparison.OrdinalIgnoreCase))
                    { ImpersonateMode = true; }
                if (arg.StartsWith("/ds:", StringComparison.OrdinalIgnoreCase))
                    { DataSourceID = arg.Substring(4); }
                if (arg.StartsWith("/u:", StringComparison.OrdinalIgnoreCase))
                    { UserName = arg.Substring(3); }
                if (arg.StartsWith("/p:", StringComparison.OrdinalIgnoreCase))
                    { Password = arg.Substring(3); }
                if (arg.StartsWith("/f:", StringComparison.OrdinalIgnoreCase))
                    { SaveFile = arg.Substring(3); }
            }
            if ((DeployMode) && (!string.IsNullOrEmpty(ModelFile)))
                { IsDeploy = true; }
            if ((ImpersonateMode) && (!string.IsNullOrEmpty(DataSourceID)) 
                    && (!string.IsNullOrEmpty(Password)) && (!string.IsNullOrEmpty(SaveFile)))
                { IsImpersonate = true; }

            if (IsDeploy)
            {
                ParseFileNames();
                ValidateFileNames();
            }

            if ((!IsDeploy) && (!IsImpersonate))
                { ShowHelp = true; }
        }

        private void ValidateFileNames()
        {
            if (!File.Exists(OptionsFile))
            {
                ErrorMsg = "Option file doesn't exist. (" + OptionsFile + ")";
                IsDeploy = false;
            }
            if (!File.Exists(TargetFile))
            {
                ErrorMsg = "Target file doesn't exist. (" + TargetFile + ")";
                IsDeploy = false;
            }
            if (!File.Exists(ModelFile))
            {
                ErrorMsg = "Model file doesn't exist.";
                IsDeploy = false;
            }
        }

        private void ParseFileNames()
        {
        	string ModelName = Path.GetFileNameWithoutExtension(ModelFile);
	
            if (string.IsNullOrEmpty(TargetFile))
                { TargetFile = Path.Combine(ModelDirectory, ModelName + ".deploymenttargets"); }
            if (string.IsNullOrEmpty(OptionsFile))
                { OptionsFile = Path.Combine(ModelDirectory, ModelName + ".deploymentoptions"); }
            if (string.IsNullOrEmpty(ConfigFile))
                { ConfigFile = Path.Combine(ModelDirectory, ModelName + ".configsettings"); }
            if (string.IsNullOrEmpty(SecurityFile))
                { SecurityFile = Path.Combine(ModelDirectory, ModelName + ".assecurityinformation"); }
            if (string.IsNullOrEmpty(AssemblyFile))
                { AssemblyFile = Path.Combine(ModelDirectory, ModelName + ".asassemblylocations"); }
        }
    }
}
