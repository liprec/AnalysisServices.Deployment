using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DataWarehouse;
using Microsoft.AnalysisServices.DeploymentEngine;
using System.Diagnostics;
using Microsoft.AnalysisServices;
using System.Xml;
using System.Collections;

namespace AnalysisServices.Deployment
{
    class Program
    {
        private static TextWriter traceWriter = null;

        static void Main(string[] args)
        {
            CmdOptions cmdLineOptions = new CmdOptions(args);
            if (cmdLineOptions.ShowHelp)
            {
                ShowHelp(cmdLineOptions.ErrorMsg);
                return;
            }
            if (cmdLineOptions.IsDeploy)
            {
                if (string.IsNullOrEmpty(cmdLineOptions.LogFile))
                {
                    traceWriter = Console.Out;
                }
                else
                {
                    traceWriter = new StreamWriter(cmdLineOptions.LogFile);
                }

                DeployDatabase(cmdLineOptions);
            }
            if (cmdLineOptions.IsImpersonate)
            {
                ImpersonateUserInfo(cmdLineOptions);
            }
        }

        private static void ImpersonateUserInfo(CmdOptions cmdLineOptions)
        {
            SecurityInformation secInformation = new SecurityInformation();
            DataSourceSecurityInformation dsSecInformation = new DataSourceSecurityInformation();
            dsSecInformation.ID = cmdLineOptions.DataSourceID;
            dsSecInformation.ImpersonationPassword = cmdLineOptions.Password;
            if (string.IsNullOrEmpty(cmdLineOptions.UserName))
                { dsSecInformation.User = cmdLineOptions.UserName; }
            secInformation.DataSources.Add(dsSecInformation);

            StringWriter secInfoString = null; 
            StreamWriter secInfoStream = null;
            try
            {
                secInfoString = new StringWriter();
                secInformation.Serialize(secInfoString);
                secInfoString.Flush();
                string enc = DataWarehouseUtilities.EncryptData(secInfoString.ToString(), secInformation.GetType().Name);
                secInfoStream = new StreamWriter(cmdLineOptions.SaveFile);
                secInfoStream.Write(enc);
                secInfoStream.Flush();
            }
            finally
            {
                if (secInfoString != null)
                {
                    secInfoString.Close();
                    secInfoString = null;
                }
                if (secInfoStream != null)
                {
                    secInfoStream.Close();
                    secInfoStream = null;
                }
            }
        }

        private static void DeployDatabase(CmdOptions cmdLineOptions)
        {
            try
            {
                XmlTextReader xmlTextReader = null;
                string fileName = string.Empty;
                fileName = cmdLineOptions.ModelFile;
                xmlTextReader = new System.Xml.XmlTextReader(fileName);
                xmlTextReader.DtdProcessing = DtdProcessing.Prohibit;
                object obj = Utils.Deserialize(xmlTextReader, new Database());
                if (!(obj is Database))
                {
                    throw new Exception("Not a model file.");
                }
                xmlTextReader.Close();
                xmlTextReader = null;
                Database database = (Database)obj;

                DeploymentOptions depOptions = new DeploymentOptions();
                depOptions.Deserialize(cmdLineOptions.OptionsFile);

                DeploymentTarget depTarget = new DeploymentTarget();
                depTarget.Deserialize(cmdLineOptions.TargetFile);

                ConfigurationSettings depConfig = new ConfigurationSettings();
                if (File.Exists(cmdLineOptions.ConfigFile))
                {
                    depConfig.Deserialize(cmdLineOptions.ConfigFile);
                }
                depConfig.ExtractFromObjectModel(database, false, true);

                //TODO Assembly deployment
                AssemblyLocations depAsmLocations = new AssemblyLocations();
                
                SecurityInformation depSecInformation = new SecurityInformation();
                if (File.Exists(cmdLineOptions.SecurityFile))
                {
                    TextReader secInfoEnc = null;
                    TextReader secInfoDec = null;
                    try
                    {
                        secInfoEnc = new StreamReader(cmdLineOptions.SecurityFile);
                        string enc = secInfoEnc.ReadToEnd();
                        string dec = DataWarehouseUtilities.DecryptData(enc);
                        secInfoDec = new StringReader(dec);
                        depSecInformation.Deserialize(secInfoDec);
                    }
                    finally
                    {
                        if (secInfoEnc != null)
                        {
                            secInfoEnc.Close();
                            secInfoEnc = null;
                        }
                        if (secInfoDec != null)
                        {
                            secInfoDec.Close();
                            secInfoDec = null;
                        }
                    }
                }

                Hashtable hashSecInformation1 = new Hashtable();
                Hashtable hashSecInformation2 = new Hashtable();
                DeployHelpers.AdjustSecurityinformation(database, depSecInformation, hashSecInformation1);
                DeployHelpers.AdjustSecurityinformation(depConfig, depSecInformation, depOptions, hashSecInformation2);

                DeploymentTracer progress = new DeploymentTracer();

                Engine.Deploy(database, depOptions, depConfig, depTarget, depAsmLocations, progress, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ShowHelp(string ErrorMsg = "")
        {
            FileVersionInfo fInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetCallingAssembly().Location);
            Console.WriteLine();
            Console.WriteLine("AnalysisServices.Deployment version " + fInfo.FileVersion);
            Console.WriteLine("-------------------------------------------------------");
            if (ErrorMsg != "")
            {
                Console.WriteLine("Error: " + ErrorMsg);
                Console.WriteLine();
            }
            Console.WriteLine("/d             Deployment mode");
            Console.WriteLine("/m:[filename]  Model definition");
            Console.WriteLine("/t:[filename]  Deployment target");
            Console.WriteLine("/o:[filename]  Deployment options");
            Console.WriteLine("/c:[filename]  Deployment config");
            Console.WriteLine("/s:[filename]  Deployment security");
            Console.WriteLine("/a:[filename]  Deployment assembly");
            Console.WriteLine();
            Console.WriteLine("/i             Impersonation mode");
            Console.WriteLine("/ds:[ID]       Datasource ID");
            Console.WriteLine("/u:[username]  Impersonation username");
            Console.WriteLine("/p:[password]  Impersonation password");
            Console.WriteLine("/f:[filename]  Export filename");
            Console.WriteLine();
            Console.WriteLine("/?, /h         This help");
            Console.ReadLine();
        }

        internal static void TraceProgress(string message)
        {
            if (traceWriter != null)
            {
                traceWriter.WriteLine(message);
                traceWriter.Flush();
                return;
            }
        }
    }
}
