## How to Setup to use this framework

#### Azure Storage Database
Create below tables:
> ApprovalDetails
>
> ApprovalEmailNotificationTemplates
>
> ApprovalRequestExpressionQueue
>
> ApprovalSummary
>
> ApprovalTenantInfo
>
> FeatureStatus
>
> Flighting
>
> FlightingFeature
>
> FlightingFeatureStatus
>
> FlightingRing
>
> SupportPortalUISchema 
>
> SyntheticTransactionDetails
>
> TenantDownTimeMessages
>
> TenantSummaryData
>
> TestHarnessPayload
>
> TransactionHistory
>
> UserDelegationSetting
>
> UserDelegationSettingsHistory
>
> UserPreferenceSetting


```
Note: 
- For few of the above tables samples of what data is to be inserted have been provided under samples folder. Please insert the same.
- Please follow the guidelines of storing and accessing user's personal information as per the governing rules & policies.
```

##### Creating Application Configuration
Make a note of all keys or secrets or Client Ids of the newly created resources:
> Storage Account Name and Key 
> 
> Cosmos db connection string
> 
> Application Insights Client Id
> 
> Microsoft Entra ID Client Id and Client Secret
> 
> Managed Identity Client Id and Client Secret

Update these key-values and export data into the App Configuration

##### KeyVault Settings

All the secrets needs to be stored in Azure Key vault and a respective Key Vault reference of the same can be added in Azure App Configuration as required.

> Any Client Secrets
> 
> Azure App Configuration connection string
> 
> Azure Cosmos Db Primary or Secondary Key
> 
> Azure Search Service Primary or Secondary Admin Key
> 
> Azure Service Bus Namespace Primary or Secondary connection string 
> 
> Azure Storage  connection string 

Update these key values and export data into the KeyVault configuration - ConfigurationKeys

## Test the framework

Use Postman or any other tool to send the below request to the Payload API
and pass the Operation based on what type of request it is (Create/ Update/ Delete)

```
Create = 1,
Update = 2,
Delete = 3
```

~~~~
curl --location --request POST '#PayloadApi#/api/v1/PayloadReceiver?TenantId=#DocumentTypeId#' \
--header 'Content-Type: application/json' \
--header 'Authorization: Bearer token' \
--data-raw '{
      "DocumentTypeId": "93f9b688-ddc3-43aa-b617-c05a415d1612",
      "Operation": 1,
      "ApprovalIdentifier": {
        "DisplayDocumentNumber": "REQ-34562-126",
        "DocumentNumber": "REQ-34562-126",
        "FiscalYear": "2020"
      },
      "Approvers": [
        {
          "DetailTemplate": "",
          "Delegation": "",
          "OriginalApprovers": [],
          "CanEdit": false,
          "Alias": "johndoe1",
          "Name": ""
        },
        {
          "DetailTemplate": "",
          "Delegation": "",
          "OriginalApprovers": [],
          "CanEdit": false,
          "Alias": "johndoe2",
          "Name": ""
        },
        {
          "DetailTemplate": "",
          "Delegation": "",
          "OriginalApprovers": [],
          "CanEdit": false,
          "Alias": "johndoe3",
          "Name": ""
        }
      ],
      "DeleteFor": [],
      "ActionDetail": null,
      "NotificationDetail": {
        "SendNotification": true,
        "TemplateKey": "PendingApproval",
        "To": "johndoe1@organization.com;johndoe2@organization.com,johndoe3@organization.com",
        "Cc": "johndoe4@organization.com",
        "Bcc": "johndoe5@organization.com",
        "Reminder": {
          "ReminderDates": [],
          "Frequency": 5,
          "Expiration": "9999-12-31T18:29:59.9999999+00:00",
          "ReminderTemplate": "TenantName|Reminder"
        }
      },
      "AdditionalData": {
        "RoutingId": "f34758cf-c61c-4358-990b-410d827cc287"
      },
      "SummaryData": {
        "DocumentTypeId": "93f9b688-ddc3-43aa-b617-c05a415d1612",
        "Title": "Title of the request",
        "UnitValue": "19515",
        "UnitOfMeasure": "USD",
        "SubmittedDate": "2018-04-13T00:00:00",
        "DetailPageURL": "https://tenant.com/",
        "CompanyCode": "1010",
        "ApprovalIdentifier": {
          "DisplayDocumentNumber": "REQ-34562-126",
          "DocumentNumber": "REQ-34562-126",
          "FiscalYear": "2020"
        },
        "Submitter": {
          "Alias": "johndoe11",
          "Name": "johndoe11"
        },
        "CustomAttribute": {
          "CustomAttributeName": "SupplierName",
          "CustomAttributeValue": "ABC PVT LTD"
        },
        "ApprovalHierarchy": [
          {
            "Approvers": [
              {
                "Alias": "johndoe1",
                "Name": ""
              },
              {
                "Alias": "johndoe2",
                "Name": ""
              }
            ],
            "ApproverType": "Interim"
          },
          {
            "Approvers": [
              {
                "Alias": "johndoe3",
                "Name": ""
              }
            ],
            "ApproverType": "Safe"
          }
        ],
        "ApprovalActionsApplicable": [
          "Approve",
          "Reject",
          "NeedMoreInformation"
        ],
        "AdditionalData": {
          "MS_IT_Comments": "Please review"
        },
        "Attachments": null,
        "ApproverNotes": "Justifications for the request"
      },
      "RefreshDetails": true,
      "DetailsData": {
        "DT1": "{\"CurrencyCode\":\"USD\",\"CurrentApprover\":\"johndoe1\",\"DocumentNumber\":\"Req-118480978\",\"FiscalYear\":\"2022\",\"HasPOE\":false,\"InterimApprovers\":\"NA\",\"InvoiceAmount\":\"10\",\"InvoiceDate\":\"2020-10-12T00:00:00\",\"InvoiceNumber\":\"Req-118480978\",\"IsFinalApprover\":true,\"PaymentTerms\":\"2% Discount 10 days, net 60 days\",\"Vendor\":\"HILTON INTERNATIONAL ADELAIDE-0002050274\",\"VendorComments\":\"NA\",\"VendorInvoiceNumber\":\"Req-118480978\",\"VendorName\":\"HILTON INTERNATIONAL ADELAIDE\",\"VendorNumber\":\"0002050274\",\"ApproverNotes\":\"NA\"}",
        "LINE": "{\"FooterDetail\":{\"CurrencyCode\":\"USD\",\"DutyOrCustoms\":0,\"ShippingAndHandlingOrFreight\":0,\"SubTotal\":2.5,\"Tax\":1.5,\"TotalAmount\":10},\"LineItems\":[{\"AssosiatedLineItemID\":\"001\",\"CurrencyCode\":\"USD\",\"Description\":\"Age Paws Paws Nothing\",\"ExtendedPrice\":8,\"GLAccount\":\"752001\",\"LineItemNumber\":\"1\",\"MaterialGroup\":\"82101905 - Static Media Delivery\",\"POE\":\"No\",\"PONumber\":null,\"Quantity\":1,\"SAPCostObject\":\"9991010\",\"ShipFrom\":null,\"ShipTo\":null,\"TaxCode\":\"G0\",\"UnitPrice\":10}]}"
      },
      "OperationDateTime": "2017-09-26T11:20:52.6735979Z",
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-ARConverter-Create",
        "TenantTelemetry": {}
      }
    }'
~~~~