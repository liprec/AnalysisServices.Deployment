using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.DeploymentEngine;
using Microsoft.DataWarehouse;

namespace AnalysisServices.Deployment
{
    // Helper class with one on one copy of Wizard code.
    internal class DeployHelpers
    {
        internal static void AdjustSecurityinformation(ConfigurationSettings configurationSettings, SecurityInformation securityInformation, DeploymentOptions deploymentOptions, System.Collections.Hashtable storedSecurityInformation)
        {
            if (configurationSettings == null)
            {
                return;
            }
            foreach (ConfigurationDataSource configurationDataSource in (System.Collections.IEnumerable)configurationSettings.Database.DataSources)
            {
                if (storedSecurityInformation != null)
                {
                    storedSecurityInformation[configurationDataSource] = configurationDataSource.ConnectionString;
                }
                DataSourceSecurityInformation dataSourceSecurityInformation = null;
                if (securityInformation != null)
                {
                    dataSourceSecurityInformation = securityInformation.DataSources[configurationDataSource.ID];
                }
                if (string.IsNullOrEmpty(deploymentOptions.OutputScript))
                {
                    if (dataSourceSecurityInformation != null)
                    {
                        if (!string.IsNullOrEmpty(dataSourceSecurityInformation.User))
                        {
                            configurationDataSource.ConnectionString = DataWarehouseUtilities.ConvertSecureStringToString(DataWarehouseUtilities.ReplaceUserIDAndPasswordInConnectionString(configurationDataSource.ConnectionString, DataWarehouseUtilities.ConvertStringToSecureString(dataSourceSecurityInformation.User), DataWarehouseUtilities.ConvertStringToSecureString(dataSourceSecurityInformation.Password)));
                        }
                        if (configurationDataSource.ImpersonationInfo != null && ImpersonationMode.ImpersonateAccount == configurationDataSource.ImpersonationInfo.ImpersonationMode)
                        {
                            configurationDataSource.ImpersonationInfo.Password = dataSourceSecurityInformation.ImpersonationPassword;
                        }
                    }
                }
                else
                {
                    configurationDataSource.ConnectionString = DataWarehouseUtilities.ConvertSecureStringToString(DataWarehouseUtilities.ReplaceUserIDAndPasswordInConnectionString(configurationDataSource.ConnectionString, null, null));
                }
            }
            foreach (ConfigurationAssembly configurationAssembly in (System.Collections.IEnumerable)configurationSettings.Database.Assemblies)
            {
                ImpersonationSecurityInformation impersonationSecurityInformation = null;
                if (securityInformation != null)
                {
                    impersonationSecurityInformation = securityInformation.Assemblies[configurationAssembly.ID];
                }
                if (impersonationSecurityInformation != null && configurationAssembly.ImpersonationInfo != null && ImpersonationMode.ImpersonateAccount == configurationAssembly.ImpersonationInfo.ImpersonationMode)
                {
                    configurationAssembly.ImpersonationInfo.Password = impersonationSecurityInformation.ImpersonationPassword;
                }
                else if (configurationAssembly.ImpersonationInfo != null)
                {
                    configurationAssembly.ImpersonationInfo.Password = null;
                }
            }
            ImpersonationSecurityInformation impersonationSecurityInformation2 = null;
            if (securityInformation != null)
            {
                impersonationSecurityInformation2 = securityInformation.DatabaseDataSourceSecurityInformation;
            }
            if (impersonationSecurityInformation2 != null && configurationSettings.Database.DataSourceImpersonationInfo != null && ImpersonationMode.ImpersonateAccount == configurationSettings.Database.DataSourceImpersonationInfo.ImpersonationMode)
            {
                configurationSettings.Database.DataSourceImpersonationInfo.Password = impersonationSecurityInformation2.ImpersonationPassword;
                return;
            }
            if (configurationSettings.Database.DataSourceImpersonationInfo != null)
            {
                configurationSettings.Database.DataSourceImpersonationInfo.Password = null;
            }
        }

        internal static void AdjustSecurityinformation(Database database, SecurityInformation secInformation, Hashtable hashSecInformation)
        {
            if (database == null)
            {
                return;
            }
            foreach (DataSource dataSource in database.DataSources)
            {
                if (hashSecInformation != null)
                {
                    hashSecInformation[dataSource] = dataSource.ConnectionString;
                }
                DataSourceSecurityInformation dsSecInformation = null;
                if (secInformation != null)
                {
                    dsSecInformation = secInformation.DataSources[dataSource.ID];
                }
                if (dsSecInformation == null)
                {
                    dataSource.ConnectionString = DataWarehouseUtilities.ConvertSecureStringToString(DataWarehouseUtilities.ReplaceUserIDAndPasswordInConnectionString(dataSource.ConnectionString, null, null));
                }
                else
                {
                    dataSource.ConnectionString = DataWarehouseUtilities.ConvertSecureStringToString(DataWarehouseUtilities.ReplaceUserIDAndPasswordInConnectionString(dataSource.ConnectionString, DataWarehouseUtilities.ConvertStringToSecureString(dsSecInformation.User), DataWarehouseUtilities.ConvertStringToSecureString(dsSecInformation.Password)));
                    if (dataSource.ImpersonationInfo != null && ImpersonationMode.ImpersonateAccount == dataSource.ImpersonationInfo.ImpersonationMode)
                    {
                        dataSource.ImpersonationInfo.Password = dsSecInformation.ImpersonationPassword;
                    }
                }
            }
        }
    }
}
