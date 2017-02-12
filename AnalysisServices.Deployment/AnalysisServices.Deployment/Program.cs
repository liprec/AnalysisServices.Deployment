using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DataWarehouse;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.DeploymentEngine;
using System.Diagnostics;
using System.Xml;
using System.Collections;

namespace AnalysisServices.Deployment
{
    class Program {
        private static TextWriter traceWriter = null;

        static unsafe void Main(string[] args) {
            CmdOptions cmdLineOptions = new CmdOptions(args);
            if (cmdLineOptions.ShowHelp) {
                ShowHelp(cmdLineOptions.ErrorMsg);
                return;
            }
            if (cmdLineOptions.IsDeploy) {
                if (string.IsNullOrEmpty(cmdLineOptions.LogFile)) {
                    traceWriter = Console.Out;
                }
                else {
                    traceWriter = new StreamWriter(cmdLineOptions.LogFile);
                }

                DeployDatabase(cmdLineOptions);
            }
            if (cmdLineOptions.IsImpersonate) {
                ImpersonateUserInfo(cmdLineOptions);
            }
            if (cmdLineOptions.IsListMode) {
                ListDataSources(cmdLineOptions);
            }
        }

        private static void ImpersonateUserInfo(CmdOptions cmdLineOptions)
        {
            SecurityInformation secInformation = new SecurityInformation();
            DataSourceSecurityInformation dsSecInformation = new DataSourceSecurityInformation();
            dsSecInformation.ID = cmdLineOptions.DataSourceID;
            dsSecInformation.ImpersonationPassword = new System.Net.NetworkCredential(string.Empty, cmdLineOptions.Password).Password;
            if (string.IsNullOrEmpty(cmdLineOptions.UserName)) {
                dsSecInformation.User = cmdLineOptions.UserName;
            }
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

        private static void DeployDatabase(CmdOptions cmdLineOptions) {
            try {
                XmlTextReader xmlTextReader = null;
                string fileName = string.Empty;
                fileName = cmdLineOptions.ModelFile;
                xmlTextReader = new System.Xml.XmlTextReader(fileName);
                xmlTextReader.DtdProcessing = DtdProcessing.Prohibit;
                object obj = Utils.Deserialize(xmlTextReader, new Database());
                if (!(obj is Database)) {
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
                if (File.Exists(cmdLineOptions.ConfigFile)) {
                    depConfig.Deserialize(cmdLineOptions.ConfigFile);
                }
                depConfig.ExtractFromObjectModel(database, false, true);

                //TODO Assembly deployment
                AssemblyLocations depAsmLocations = new AssemblyLocations();
                
                SecurityInformation depSecInformation = new SecurityInformation();
                if (File.Exists(cmdLineOptions.SecurityFile)) {
                    TextReader secInfoEnc = null;
                    TextReader secInfoDec = null;
                    try {
                        secInfoEnc = new StreamReader(cmdLineOptions.SecurityFile);
                        string enc = secInfoEnc.ReadToEnd();
                        string dec = DataWarehouseUtilities.DecryptData(enc);
                        secInfoDec = new StringReader(dec);
                        depSecInformation.Deserialize(secInfoDec);
                    }
                    finally {
                        if (secInfoEnc != null) {
                            secInfoEnc.Close();
                            secInfoEnc = null;
                        }
                        if (secInfoDec != null) {
                            secInfoDec.Close();
                            secInfoDec = null;
                        }
                    }
                                }
                if (!(string.IsNullOrEmpty(cmdLineOptions.DataSourceID)))
                {
                    DataSourceSecurityInformation dsSecInformation = new DataSourceSecurityInformation();
                    dsSecInformation.ID = cmdLineOptions.DataSourceID;
                    dsSecInformation.ImpersonationPassword = new System.Net.NetworkCredential(string.Empty, cmdLineOptions.Password).Password;

                    if (string.IsNullOrEmpty(cmdLineOptions.UserName)) {
                        dsSecInformation.User = cmdLineOptions.UserName;
                    }

                    depSecInformation.DataSources.Add(dsSecInformation);
                }
                Hashtable hashSecInformation1 = new Hashtable();
                Hashtable hashSecInformation2 = new Hashtable();
                DeployHelpers.AdjustSecurityinformation(database, depSecInformation, hashSecInformation1);
                DeployHelpers.AdjustSecurityinformation(depConfig, depSecInformation, depOptions, hashSecInformation2);

                DeploymentTracer progress = new DeploymentTracer();

                Engine.Deploy(database, depOptions, depConfig, depTarget, depAsmLocations, progress, null);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.StackTrace);
            }
            finally {
                cmdLineOptions.Dispose();
            }
        }

        private static void ShowHelp(string ErrorMsg = "") {
            string outline = "{0,-20}{1}";
            
            FileVersionInfo fInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetCallingAssembly().Location);

            Console.WriteLine(new String('-', 43));
            Console.WriteLine("AnalysisServices.Deployment version " + fInfo.FileVersion);
            Console.WriteLine("(c) 2017 Jan Pieter Posthuma");
            Console.WriteLine(new String('-', 43));
            if (ErrorMsg != "") {
                Console.WriteLine("Error: " + ErrorMsg);
                Console.WriteLine();
            }
            Console.WriteLine("AnalysisServices.Deployment [<parameters>]");
//            Console.WriteLine("  [/d] | /m[:filename] | [/t[:filename]] | [/o[:filename]] | [/c[:filename]]");
//            Console.WriteLine("  [/d] | /m[:filename] | [/t[:filename]] | [/o[:filename]] | [/c[:filename]]");
//            Console.WriteLine("  [/i] | /m[:filename] | [/ds[:filename]] | [/u[:filename]] | [/p[:filename]]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine(string.Format(outline, "/d", "Deployment mode - full automated mode"));
            Console.WriteLine(string.Format(outline, "/i", "Impersonation mode - create a encrypted security file which can be used in the 'Deployment mode'."));
            Console.WriteLine(string.Format(outline, "", "Encryption is user bound."));
            Console.WriteLine();
            Console.WriteLine(string.Format(outline, "/m[:filename]", "Model definition"));
            Console.WriteLine(string.Format(outline, "/t[:filename]", "Deployment target"));
            Console.WriteLine(string.Format(outline, "/o[:filename]", "Deployment options"));
            Console.WriteLine(string.Format(outline, "/c[:filename]", "Deployment config"));
            Console.WriteLine(string.Format(outline, "/sc[:filename]", "Deployment security"));
            Console.WriteLine(string.Format(outline, "/a[:filename]", "Deployment assembly"));
            Console.WriteLine();
            Console.WriteLine(string.Format(outline, "/ds[:ID]", "Datasource ID"));
            Console.WriteLine(string.Format(outline, "/u[:username]", "Impersonation username"));
            Console.WriteLine(string.Format(outline, "/p[:password]", "Impersonation password"));
            Console.WriteLine(string.Format(outline, "/f[:filename]", "Export filename"));
            Console.WriteLine();
            Console.WriteLine(string.Format(outline, "/l", "List mode - Provides the ID(s) of 1 or all datasources defined in the model file"));
            Console.WriteLine(string.Format(outline, "/ds[:ID]", "Datasource ID"));
            Console.WriteLine();
            Console.WriteLine(string.Format(outline, "/s[:logfile]", "Log output to file"));
            Console.WriteLine();
            Console.WriteLine(string.Format(outline, "/?, /h", "This help"));
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void ListDataSources(CmdOptions cmdLineOptions) {
            string outline = "{0,-30}{1}";
            try {
                XmlTextReader xmlTextReader = null;
                string fileName = string.Empty;
                fileName = cmdLineOptions.ModelFile;
                xmlTextReader = new System.Xml.XmlTextReader(fileName);
                xmlTextReader.DtdProcessing = DtdProcessing.Prohibit;
                object obj = Utils.Deserialize(xmlTextReader, new Database());

                if (!(obj is Database)) {
                    throw new Exception("Not a model file.");
                }
                xmlTextReader.Close();
                xmlTextReader = null;
                Database database = (Database)obj;

                Console.WriteLine();
                Console.WriteLine(string.Format(outline, "DataSource", "ID"));
                Console.WriteLine(new String('=', 50));

                if (String.IsNullOrEmpty(cmdLineOptions.DataSourceID)) {
                    foreach (DataSource ds in database.DataSources)
                    {
                        Console.WriteLine(string.Format(outline, ds.Name, ds.ID));
                    }
                } else {
                    DataSource ds = database.DataSources.Find(cmdLineOptions.DataSourceID);
                    Console.WriteLine(string.Format(outline, ds.Name, ds.ID));
                }

                Console.WriteLine();
                Console.WriteLine("{0} datasource(s) found", database.DataSources.Count);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.StackTrace);
            }
        }

        internal static void TraceProgress(string message) {
            if (traceWriter != null) {
                traceWriter.WriteLine(message);
                traceWriter.Flush();
                return;
            }
        }
    }
}
