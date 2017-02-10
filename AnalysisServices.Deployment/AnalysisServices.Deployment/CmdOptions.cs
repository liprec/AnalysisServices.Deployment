using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisServices.Deployment {
    class CmdOptions {
        public bool ShowHelp;
        
        public string ModelFile = string.Empty;
        public string TargetFile = string.Empty;
        public string OptionsFile = string.Empty;
        public string ConfigFile = string.Empty;
        public string SecurityFile = string.Empty;
        public string AssemblyFile = string.Empty;
        public string ErrorMsg = string.Empty;

        public bool IsDeploy;
        public bool IsImpersonate;
        public bool IsListMode;

        public bool DeployMode;
        public bool ImpersonateMode;
        public bool ListMode;

        public string DataSourceID = string.Empty;
        public string UserName = string.Empty;
        public string SaveFile = string.Empty;
        public SecureString Password = new SecureString();

        public string LogFile = string.Empty;

        public unsafe CmdOptions(string[] args) {
            string ModelDirectory = Directory.GetCurrentDirectory();

            if (args.Length == 0) { ShowHelp = true; }
            foreach(string arg in args) {
                if ((arg.Equals("/?", StringComparison.OrdinalIgnoreCase)) || (arg.Equals("/h", StringComparison.OrdinalIgnoreCase)))
                    { ShowHelp = true; }
                if (arg.Equals("/d", StringComparison.OrdinalIgnoreCase))
                    { DeployMode = true; }
                if (arg.Equals("/i", StringComparison.OrdinalIgnoreCase))
                    { ImpersonateMode = true; }
                if (arg.Equals("/l", StringComparison.OrdinalIgnoreCase))
                    { ListMode = true; }

                if (arg.StartsWith("/m:", StringComparison.OrdinalIgnoreCase)) {
                    ModelFile = Path.Combine(ModelDirectory, arg.Substring(3));
                }
                if (arg.StartsWith("/t:", StringComparison.OrdinalIgnoreCase)) {
                    TargetFile = arg.Substring(3);
                }
                if (arg.StartsWith("/o:", StringComparison.OrdinalIgnoreCase)) {
                    OptionsFile = arg.Substring(3);
                }
                if (arg.StartsWith("/c:", StringComparison.OrdinalIgnoreCase)) {
                    ConfigFile = arg.Substring(3);
                }
                if (arg.StartsWith("/sc:", StringComparison.OrdinalIgnoreCase)) {
                    SecurityFile = arg.Substring(3);
                }
                if (arg.StartsWith("/a:", StringComparison.OrdinalIgnoreCase)) {
                    AssemblyFile = arg.Substring(3);
                }

                if (arg.StartsWith("/ds:", StringComparison.OrdinalIgnoreCase))
                    { DataSourceID = arg.Substring(4); }
                if (arg.StartsWith("/u:", StringComparison.OrdinalIgnoreCase))
                    { UserName = arg.Substring(3); }
                if (arg.StartsWith("/p:", StringComparison.OrdinalIgnoreCase))
                {
                    Password = ParsePassword(arg.Substring(3));
                }
                if (arg.StartsWith("/f:", StringComparison.OrdinalIgnoreCase))
                    { SaveFile = arg.Substring(3); }

                if (arg.StartsWith("/s:", StringComparison.OrdinalIgnoreCase))
                    { LogFile = arg.Substring(3); }
            }
            
            if ((DeployMode) && (!string.IsNullOrEmpty(ModelFile)))
                { IsDeploy = true; }
            if ((ImpersonateMode) && (!string.IsNullOrEmpty(DataSourceID)) 
                    && (!string.IsNullOrEmpty(Password.ToString())) && (!string.IsNullOrEmpty(SaveFile)))
                { IsImpersonate = true; }
            if ((ListMode) && (!string.IsNullOrEmpty(ModelFile)))
                { IsListMode = true; }

            if (IsDeploy) {
                ParseFileNames();
                ValidateFileNames();
            }

            if ((!IsDeploy) && (!IsImpersonate) && (!IsListMode))
                { ShowHelp = true; }
        }

        private unsafe SecureString ParsePassword(string pwd)
        {
            fixed (char* pChars = pwd)
            {
                return new SecureString(pChars, pwd.Length);
            }
        }

        private void ValidateFileNames() {
            if (!File.Exists(OptionsFile)) {
                ErrorMsg = "Option file doesn't exist. (" + OptionsFile + ")";
                IsDeploy = false;
            }
            if (!File.Exists(TargetFile)) {
                ErrorMsg = "Target file doesn't exist. (" + TargetFile + ")";
                IsDeploy = false;
            }
            if (!File.Exists(ModelFile)) {
                ErrorMsg = "Model file doesn't exist.";
                IsDeploy = false;
            }
        }

        private void ParseFileNames() {
        	  string ModelName = Path.GetFileNameWithoutExtension(ModelFile);
            string ModelDirectory = Path.GetDirectoryName(ModelFile);

            if (string.IsNullOrEmpty(TargetFile)) {
                TargetFile = Path.Combine(ModelDirectory, ModelName + ".deploymenttargets");
            } else {
                TargetFile = Path.Combine(ModelDirectory, TargetFile);
            }
            if (string.IsNullOrEmpty(OptionsFile)) {
                OptionsFile = Path.Combine(ModelDirectory, ModelName + ".deploymentoptions");
            } else {
                OptionsFile = Path.Combine(ModelDirectory, OptionsFile);
            }
            if (string.IsNullOrEmpty(ConfigFile)) {
                ConfigFile = Path.Combine(ModelDirectory, ModelName + ".configsettings");
            } else {
                ConfigFile = Path.Combine(ModelDirectory, ConfigFile);
            }
            if (string.IsNullOrEmpty(SecurityFile)) {
                SecurityFile = Path.Combine(ModelDirectory, ModelName + ".assecurityinformation");
            } else {
                SecurityFile = Path.Combine(ModelDirectory, SecurityFile);
            }
            if (string.IsNullOrEmpty(AssemblyFile)) {
                AssemblyFile = Path.Combine(ModelDirectory, ModelName + ".asassemblylocations");
            } else {
                AssemblyFile = Path.Combine(ModelDirectory, AssemblyFile);
            }
        }
    }
}