# Microsoft Assent
## Assent <sub>**A***pproval* **S***olution* **S***implified for* **ENT***erprise*<sub>
Microsoft Assent (*a.k.a Approvals*) as a platform provides the "one stop shop" solution for approvers via a model that brings together disparate different approval requests in a consistent and ultra-modern model. Approvals delivers a unified approvals experience for any approval on multiple form factors - Website, Outlook Actionable email, Teams. It consolidates approvals across organization's line of business applications, building on modern technology and powered by Microsoft Azure. It serves as a showcase for solving modern IT scenarios using the latest technologies.
- Payload Receiver Service API - Accepts payload from tenant system.
- Audit Processor - Azure Function that logs the payload data into Azure Cosmos DB.
- Primary Processor - Azure Function that processes the payload pushed by payload receiver service API to service bus.
- Notification Processor - Azure Function that sends email notifications to Approvers/ Submitters as per configurations.
- WatchdogProcessor - as per configurations from tenant sends reminder email notifications to Approvers for pending approvals as per configurations from tenant.
- Core Services API - Set of Web APIs to support the Approvals UI.

## Getting Started

These instructions will get the project up and running in Azure.

### Pre-requisites

Before running the project on local machine/ or deploy the following things needs to be setup on Azure:
- Azure Subscription
- Azure Active Directory (AAD) App (along with secret)

Apart from these keep the following items handy as it would be required during deployment:
- AAD App's ClientId for Authentication in case of API
- AAD App's Secret for Authentication in case of API
- Assign a default `user_impersonation` scope for the AAD app (with Admin Consent only)
- AAD App's resource/AAP ID URI used for generating token (audience)
- TenantId for Authentication in case of API
- Issuer Url for Authentication : https://sts.windows.net/{AADTenantId}
- Custom Application Name which would be used to create AppServices/Functions (resource_name_prefix)
- Custom Resource Group Name where all the resources will be deployed
- Location/code to deploy the Azure Resources (e.g. Central US/centralus. Powershell Command: Get-AzureRmLocation)


### Installing

A step by step series of that explains how to get the components deployed in Azure

```
Step 1: Download the ARM template (template.json) from the source (scripts)
```

```
Step 2: Go to Azure Portal and search/select the service 'Deploy a custom template"
```

```
Step 3: Select 'Build your own template in the editor' and paste the content of 'template.json' in the editor
```

```
Step 4: Save and go the next step. Select the subscription, resource group & location.
Update the settings to update any of the parameter values if required and click on purchase

Note : If there is any failure, try re-deploying again before proceeding for any troubleshooting.
```

## Clean-up
It might have happened that some of the resources which got created may be already present in your subscription.
In that case, you can continue to use the same and delete the newly created resources. (e.g. Storage Account, Application Insights, ServiceBus - In case of ServiceBus make sure to create the Topics in your exisiting ServiceBus namespace before deleting).

The following table will help in deciding which components can be cleaned-up.

|Common||
|--------|------|
|  | App Configuration|
|  | KeyVault |
|  | Application Insights |
|  | ServiceBus Namespace |
|  | Storage Account |
|  | Cosmos Db |
|  | Azure Search |

## Setup and Configuration

Once all the components are deployed, go to the below components, copy the access keys and store that in Azure App Configuration.
**The secrets needs to be stored in Azure Key vault and a respective Key Vault reference of the same can be added in   Azure App Configuration as required.**

| Key Name | Source | In KeyVault ? |
|--------|------|--------|
| AzureAppConfigurationConnectionString | Azure App Configuration | Yes |
| ServiceBusConnectionString | ServiceBus | Yes |
| StorageConnectionString | Storage Account | Yes |
| StorageAccountName | Storage Account | No |
| StorageAccountKey | Storage Account | Yes |
| APPINSIGHTS_INSTRUMENTATIONKEY | Application Insights | No |


#### Import Configuration to Azure App Configuration
```
Step 1: Download the configuration file (AppCofiguration.json) from the samples folder
```
```
Step 2: Add/update the values for the following keys in the JSON
```
| Key Name | Source | In KeyVault ? |
|--------|------|--------|
| AADTenantId | Azure Active Directory (AAD) Tenant ID | No |
| AntiCorruptionMessage | Message to be shown on the UI while taking action (if applicable) | No |
| ApprovalsAudienceUrl | AAD Resource (APP ID URL) | No |
| ApprovalsBaseUrl | Approvals Website Base URL | No |
| ApprovalsCoreServicesURL | Approvals API's Base URL | No |
| AzureSearchServiceName | Azure Search | No |
| AzureSearchServiceQueryApiKey | Azure Search | Yes |
| CosmosDbAuthKey | Azure Cosmos DB | Yes |
| CosmosDbEndPoint | Azure Cosmos DB | No |
| DetailControllerExceptionMessage | Error message to be shown on the UI when details loading fails  | No |
| EnvironmentName |  Environment Name where this solution is getting deployed (e.g., DEV/ TEST etc.) | No |
| GraphAPIAuthString | AAD Authority URL with {0} replaced with the TenantID - https://login.windows.net/{0} | No |
| GraphAPIClientId | AAD Client ID which has permissions to Access Microsoft Graph to get user data | No |
| GraphAPIClientSecret | AAD Client Secret - used to access Microsoft Graph | Yes |
| NotificationBroadcastUri | Notification Service's REST endpoint | No |
| NotificationFrameworkAuthKey | AAD Client Secret - used for Authentication with Notification Framework/service  | Yes |
| NotificationFrameworkClientId | AAD Client ID - used for Authentication with Notification Framework/service  | No |
| ReceiptAcknowledgmentMessage | Message to be shown on the UI while taking action (if applicable | No |
| ServiceBusConnectionString | Azure Service Bus | Yes |
| ServiceBusIssuerSecret | Azure Service Bus | Yes |
| ServiceBusNamespace | Azure Service Bus | No |
| ServiceComponentId | [Optional] Used for Logging | No |
| ServiceLineName | [Optional] Used for Logging | No |
| ServiceName | [Optional] Used for Logging | No |
| ServiceOfferingName | [Optional] Used for Logging | No |
| ServiceParameterAuthKey | AAD Client Secret - used for Authentication with LoB apps endpoints/service | Yes |
| ServiceParameterClientID | AAD Client ID - used for Authentication with LoB apps endpoints/service | No |
| StorageAccountKey | Azure Storage | Yes |
| StorageAccountName | Azure Storage | No |
| SupportEmailId | e.g., mailto:help@contoso.com | No |
| SyntheticTransactionsApproverAliasList | [Optional] (;) separated list of aliases which would be the allowed approvers for creating synthetic transaction requests | No |
| UrlPlaceholderTenants | [Optional] Int32 identifiers for simulating LoB apps in self-server portal | No |
| WhitelistDomains | Domains which will be allowed to access Assent | No |

```
Step 3: Go to the App Configuration service on Azure Portal and select the resource
where the configuration needs to be imported.
```
```
Step 4: Go to 'Operations' -> 'Import/Export'
```
```
Step 5: Select 'Import' in the toggle button and
choose 'Configuration file' from the dropdown 'Source service'.
```
```
Step 6: In the 'For language' drop down select 'Other'
```
```
Step 7: Choose 'Json' as the value from the 'File type' dropdown and
select the 'AppConfiguration.json' updated in the previous step file from the File Explorer.
```
```
Step 8: The configuration should be fetched successfully and the UI should show options to save/apply
Select the 'Label' under which the configurations needs to be added (e.g., DEV) and click 'Apply'
```
#### Update Application Settings
* For all the components created above please add their respective system assigned managed identity in the 'Access policies' section of the KeyVault.
* Please select the 'Configure from template' option to be Secret Management and assign just the `Get` and `List` permission
* For the Function Apps add/update the below AppSetting keys:
  > APPINSIGHTS_INSTRUMENTATIONKEY
  > > This is an instrumentation key of Application Insights which was created from ARM Template.
  >
  > AzureAppConfigurationUrl
  > > This would be Azure App Configuration's endpoint URL.
  >
  > AppConfigurationLabel
  > > This would be Azure App Configuration's label value corresponding to the environment the App service is running for.
  >
  > AzureWebJobsStorage
  > > This would be Key vault Reference to storage account's connection string.
  >
  > AzureWebJobsDashboard
  > > This would be Key vault Reference to storage account's connection string.
  >
  > ComponentName
  > > Name of the component which could be name of the component like *ApprovalsPrimaryProcessor or ApprovalsNotificationProcessor*.
  >
  > FUNCTIONS_EXTENSION_VERSION : ~3
  >
  > SubscriptionName
  > > The Service Bus Topic's subscription name which is to be listened by the function app for new messages.
* Apart from above seeting, below AppSetting keys should also be added to Watchdog function:
  > Schedule
  > > A CRON expression or a TimeSpan value (0 */5 * * * *).
* For the Azure App Services add/update the below AppSetting keys:
  > APPINSIGHTS_INSTRUMENTATIONKEY
  > > This is an instrumentation key of Application Insights which was created from ARM Template.
  >
  > AzureAppConfigurationUrl
  > > This would be Azure App Configuration's endpoint URL.
  >
  > AppConfigurationLabel
  > > This would be Azure App Configuration's label value corresponding to the environment the App service is running for.
  >
  > ComponentName
  > > Name of the component which could be name of the component like *ApprovalsCoreServicesAPI or ApprovalsPayloadServiceAPI*.
  >
  > ValidAppIds
  > > This is AzureAD App's ClientIds which are authorized to access this component (; separated).
  >
	```
    Note: The connection string should be the KeyVault url
    i.e. Enter the value in this format: @Microsoft.KeyVault(SecretUri=<keyvault Secret Identifier url for AzureAppConfigurationConnectionString>)
    ```
#### Setup Authentication/Access Permission

* Setup Authentication for APIs and Function Apps
  * Update the Reply Urls section of the AzureAD App created earlier with the URLs of the App Services and FunctionApps (HttpTriggered) URLs suffixed with '/auth/login/aad/callback'
  * In the 'Authentication' section of the AppServices / FunctionApps (HttpTriggered),
    * Add or update the Authentication values (ClientId/Secret/Issuer/Audience)
    * Select 'Login with Azure Active Directory' for the option 'Action to take when the request is not authenticated'

* Permissions needed needed for System assigned Managed Identity of below Azure Components
    * Payload Receiver Service API:
        * App Configuration Data Reader
        * Azure Service Bus Data Sender
        * Cosmos DB Built-in Data Contributor
        * Key Vault Secrets User
        * Storage Blob Data Contributor
        * Storage Table Data Contributor

    * Audit Processor:
        * App Configuration Data Reader
        * Azure Service Bus Data Owner
        * Cosmos DB Built-in Data Contributor
        * Key Vault Secrets User
        * Storage Blob Data Contributor
        * Storage Table Data Contributor

    * Primary Processor:
        * App Configuration Data Reader
        * Azure Service Bus Data Owner
        * Cosmos DB Built-in Data Contributor
        * Key Vault Secrets User
        * Storage Blob Data Contributor
        * Storage Table Data Contributor

    * Notification Processor:
        * App Configuration Data Reader
        * Azure Service Bus Data Owner
        * Cosmos DB Built-in Data Contributor
        * Key Vault Secrets User
        * Storage Blob Data Contributor
        * Storage Table Data Contributor

    * Watchdog Processor:
        * App Configuration Data Reader
        * Cosmos DB Built-in Data Contributor
        * Key Vault Secrets User
        * Storage Blob Data Contributor
        * Storage Table Data Contributor

    * Core Services API:
        * App Configuration Data Reader
        * Cosmos DB Built-in Data Contributor
        * Key Vault Secrets User
        * Storage Blob Data Contributor
        * Storage Table Data Contributor
        
    *Note: As of today only way to assign Cosmos DB Built-in Data Contributor is via the PowerShell or az cli below is the command fot the same:*
    ```
        az cosmosdb sql role assignment create --account-name "Cosmosdb account name" --resource-group "Name of resource group where cosmosdb exists" --scope "/" --principal-id "System assigned identity to to which this Role Assignment is being granted" --role-definition-id "00000000-0000-0000-0000-000000000002"
    ```
    For more information please read: [Configure role-based access control for your Azure Cosmos DB account with Azure AD | Microsoft Learn](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-setup-rbac)

## Deploy
Deploy the code in these new components using Azure DevOps (Build and Release pipelines)

The deployment might fail sometimes due to locked files. Try restarting the service, before redeploying.
If the issue persists, add the following AppSettings in the service configuration
```
    "MSDEPLOY_RENAME_LOCKED_FILES": "1"
```

## How to Setup to use this framework

See the [SETUP.md](SETUP.md) file for details

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.