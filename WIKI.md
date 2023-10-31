# Microsoft Assent <sub>**A***pproval* **S***olution* **S***implified for* **ENT***erprise*<sub>

## Contents
- [Introduction](#introduction)</br>
- [Form Factors Supported](#form-factors-supported)</br>
- [Key Features](#key-features)</br>
- [Key technical requirements for integrating with Approvals](#key-technical-requirements-for-integrating-with-approvals)</br>
- [Nitty Gritty of integration](#nitty-gritty-of-integration)</br>
  - [Basic lifecycle of a approval request](#basic-lifecycle-of-an-approval-request)</br>
  - [Details of Approval creation process](#details-of-approval-creation-process)</br>
    - [ApprovalRequestExpression](#approvalRequestExpression)</br>
      - [Create Payload](#create-payload)</br>
      - [Update Payload](#update-payload)</br>
      - [Delete Payload](#delete-payload)</br>
      - [Details of all properties involved in create, update and delete payloads](#details-of-all-properties-involved-in-create-update-and-delete-payloads)</br>
  - [Details of Approval process](#details-of-approval-process)</br>
    - [Approval Request](#approval-request)</br>
      - [Single approval scenario](#single-approval-scenario)</br>
      - [Bulk approval scenario](#bulk-approval-scenario)</br>
    - [Approval Response](#approval-response)</br>
      - [Approval Response for single approval success scenario](#Approval-response-for-single-approval-success-scenario)</br>
      - [Approval Response for single approval failure scenario](#Approval-response-for-single-approval-failure-scenario)</br>
      - [Approval Response for bulk approvals success and failure scenario](#Approval-response-for-bulk-approvals-success-and-failure-scenario)</br>
    - [Details of attaching documents while performing actions for Tenant](#details-of-attaching-documents-for-tenants)</br>

## Introduction

Microsoft Assent (*a.k.a Approvals*) as a platform provides the “one stop shop” solution for approvers via a model that brings together disparate different approval requests in a consistent and ultra-modern model. Approvals delivers a unified approvals experience for any approval on multiple form factors - Website, Outlook Actionable email, Teams. It consolidates approvals across organization's line of business applications, building on modern technology and powered by Microsoft Azure. It serves as a showcase for solving modern IT scenarios using the latest technologies.

## Form Factors Supported

- Website.
- Outlook Actionable email (both Desktop & Mobile).
- Teams (Teams Approval Hub).

## Key Features

- **Approvals UI & back end integration**: This is the minimum feature set provided by Approvals, which includes showing and allowing -users to take action on approvals.
- **Email notifications**: Notify approver of new pending approval.
- **Reminder notifications**: Remind approver to take action on an existing pending approval.
- **Mobile Friendly Actionable Emails**: Notify Approver of new pending approval or an existing pending approval while giving approval- functionality in the email itself.
- **Bulk Approvals**: This enables a user to select multiple items and approve them all at once.
- **Approval hierarchy**: There can be multiple levels of approvers for a single approval. For example, interim approvers, SAFE -approvers, final approvers, etc.
- **Concurrent approvers**: One approval can be pending with multiple approvers at the same time, with any one of those approvers able -to complete the approval.
- **Multiple approval actions**: There are more approval action available beyond Approve or Reject.
- **Attachments**: There are attached documents or files as part of the approval.

## Key technical requirements for onboarding on to with Microsoft Assent

- Requires below set of APIs from the team onboarding to Microsoft Assent (*a.k.a Approvals*) and related details:
  - **Summary API**: This is to fetch summary information of the request. Its HTTP method type is GET. It API should return exact same JSON that they send in SummaryData property of the Create/Update/Delete payload (i.e. value of property can changes as per requirement but contract remains same).
  - **Details API**: This is to fetch details information of the request. Its HTTP method type is GET. Details API endpoints will be equal to number of properties agreed upon and being sent by tenant application with ```DetailsData``` property of the Create/Update/Delete payload. Only difference here is in Create/Update/Delete payload it is string and their API would return JSON. This API also helps to pull data of historical records from onboarding team after the request lifecycle completes.
  - **Action API**: This is to post an action taken by an approver to a tenant. Its HTTP method type is POST.
  - **Attachments API**: This is to fetch attachment details related to the request. Its HTTP method type is GET. This is only required if tenant has attachments as part of the approval request.
  - **ClientID/APP ID**: This is tenant’s APP ID which is used by tenant to acquire token on behalf of Approvals while sending payloads to Approvals. This APP ID needs to be whitelisted on Approvals end, so that incoming tenant’s request are accepted by Approvals.
  - **Resource URL**: This is the resource URL for which Approvals needs to acquire token for, while calling tenant’s summary and details API.
  - **API Client ID/Microsoft Entra ID App name**: This is the tenant’s Microsoft Entra ID App ID for which Approvals will take delegation of, so as to acquire token for the above Resource URL.
- Other integration details to be exchanged:
  - All html email templates (in case of subscribing to emails functionality) should be designed by tenant as per business requirements and shared with Approvals so that they are configured accordingly on Approvals end.
  - Tenant’s icons which will be displayed as application icon in Approvals, need to be designed and shared with Approvals for configurations. Icons need to be in transparent background and of below size:
      - 16X16
      - 30X30
      - 40X40
  - Implementation of end-to-end telemetry between Approvals and tenants is highly recommended. There are request level and transaction level correlations which are expected to be passed for telemetry purposes.

## Nitty Gritty of integration

### Basic lifecycle of an approval request

To create an approval request gets within Approvals, a tenant will need to send a payload to Approvals with required details depending on a scenario - Create, Update and Delete (more on this below). To understand a basic lifecycle of request with system’s point of view, let’s take a scenario wherein a request must go through 2 approval stages:
- **Step 1**: The tenant will send a create payload (ARX) to Approvals with 1^st^ level approver’s details. This will create a request in 1^st^ level approver’s queue within Approvals.
- **Step 2**: Let’s say, 1^st^ level approver approves the request from Approvals.
- **Step 3**: Approvals will then call tenant’s Action API to inform that the request was approved.
- **Step 4**: The tenant will now send an update payload to Approvals which will have 2^nd^ level approver’s details and will also have information of previous approver in ‘delete for’ section of the update payload – which in turn would delete the approval request from previous approver’s queue and creates it in 2^nd^ level approver’s queue within Approvals. Approval history is also updated based on what is sent in ‘action details’ section of update payload.
- **Step 5**: Let’s say, 2^nd^ level approver approves the request from Approvals.
- **Step 6**: Approvals will again call Tenant’s Action API to inform that the request was approved.
- **Step 7**: The tenant will now send delete payload to Approvals which will have 2^nd^ level approver’s details in ‘delete for’ section of the delete payload – which in turn would delete the approval request from 2^nd^ level approver’s queue within Approvals. Approval history is also updated based on what is sent in ‘action details’ section of delete payload.

### Details of Approval creation process

The tenant system needs to make HTTP POST call to Approvals payload receiver service to send the payloads (also known as ApprovalRequestExpression) using Microsoft Entra ID authentication token. Example of an endpoint: https://payload.contoso.com/api/v1/PayloadReceiver?TenantId=DocumentTypeId
*where DocumentTypeId a GUID in the form of a string provided by Approvals, It is unique for each tenant and is used to identify the tenant on Approvals end*.

#### ApprovalRequestExpression

Also known as ARX, this is an inbound call i.e., sent to Approvals from tenant. It helps in orchestrating the approval workflow and can be completely controlled by tenant for right reasons. The tenant side code drives the approval progress and notification with maximum flexibility. There are three important types of operations performed by the payloads which are received in the form of ARX. They are:
- **Create**: This is used for creating an approval record under an approver's queue.
- **Update**: This is used for creating an approval record under next approver's queue and at the same time deleting from previous approver's queue from the approval chain.
- **Delete**: This is used for deleting an approval record from an approver's queue.

Below are the details of each payload in JSON format:

##### Create Payload

- Below is the sample create payload:

    ```json
    {
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
        "Attachments": [
          {
            "ID": "2792560",
            "DocumentType": null,
            "Name": "Image1.png",
            "Url": ""
          },
          {
            "ID": "",
            "DocumentType": null,
            "Name": "Image2.png",
            "Url": "https://urltothedocument.com"
          }
        ],
        "ApproverNotes": "Justifications for the request"
      },
      "RefreshDetails": true,
      "DetailsData": {
        "DT1": "{\"CurrencyCode\":\"USD\",\"CurrentApprover\":\"johndoe1\",\"DocumentNumber\":\"Req-118480978\",\"FiscalYear\":\"2022\",\"InvoiceAmount\":\"10\",\"InvoiceDate\":\"2020-10-12T00:00:00\",\"InvoiceNumber\":\"Req-118480978\",\"IsFinalApprover\":true,\"PaymentTerms\":\"2% Discount 10 days, net 60 days\",\"Vendor\":\"VendorName-1232050274\",\"VendorComments\":\"NA\",\"VendorInvoiceNumber\":\"Req-118480978\",\"VendorName\":\"VendorName\",\"VendorNumber\":\"1232050274\",\"ApproverNotes\":\"NA\"}",
        "LINE": "{\"FooterDetail\":{\"CurrencyCode\":\"USD\",\"DutyOrCustoms\":0,\"ShippingAndHandlingOrFreight\":0,\"SubTotal\":2.5,\"Tax\":1.5,\"TotalAmount\":10},\"LineItems\":[{\"AssosiatedLineItemID\":\"001\",\"CurrencyCode\":\"USD\",\"Description\":\"Age Paws Paws Nothing\",\"ExtendedPrice\":8,\"Account\":\"752001\",\"LineItemNumber\":\"1\",\"SubstanceGroup\":\"82101905 - Static Media Delivery\",\"PONumber\":null,\"Quantity\":1,\"SAPCostObject\":\"9991010\",\"ShipFrom\":null,\"ShipTo\":null,\"TaxCode\":\"G0\",\"UnitPrice\":10}]}"
      },
      "OperationDateTime": "2017-09-26T11:20:52.6735979Z",
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-ARConverter-Create",
        "TenantTelemetry": {}
      }
    }
    ```

##### Update Payload

- Below is the sample update payload:

    ```json
    {
      "DocumentTypeId": "93f9b688-ddc3-43aa-b617-c05a415d1612",
      "Operation": 2,
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
      "DeleteFor": [ "johndoe1" ],
      "ActionDetail": {
        "CorrelationId": "26dd7787-a918-457d-b73a-76eedb71a156",
        "Name": "Approve",
        "Date": "2022-04-13T07:27:44.8945494Z ",
        "Comment": "Within Budget",
        "ActionBy": {
          "Alias": "johndoe1",
          "Name": ""
        },
        "NewApprover": {
          "Alias": "johndoe2",
          "Name": ""
        },
        "Placement": "After",
        "AdditionalData": {
          "ActionByDelegateInMSApprovals": ""
        },
        "UserActionFailureReason": ""
      },
      "NotificationDetail": {
        "SendNotification": true,
        "TemplateKey": "PendingApproval",
        "To": "johndoe2@organization.com,johndoe3@organization.com",
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
        "Attachments": [
          {
            "ID": "2792560",
            "DocumentType": null,
            "Name": "Image1.png",
            "Url": ""
          },
          {
            "ID": "",
            "DocumentType": null,
            "Name": "Image2.png",
            "Url": "https://urltothedocument.com"
          }
        ],
        "ApproverNotes": "Justifications for the request"
      },
      "RefreshDetails": true,
      "DetailsData": {
        "DT1": "{\"CurrencyCode\":\"USD\",\"CurrentApprover\":\"johndoe1\",\"DocumentNumber\":\"Req-118480978\",\"FiscalYear\":\"2022\",\"InvoiceAmount\":\"10\",\"InvoiceDate\":\"2020-10-12T00:00:00\",\"InvoiceNumber\":\"Req-118480978\",\"IsFinalApprover\":true,\"PaymentTerms\":\"2% Discount 10 days, net 60 days\",\"Vendor\":\"VendorName-1232050274\",\"VendorComments\":\"NA\",\"VendorInvoiceNumber\":\"Req-118480978\",\"VendorName\":\"VendorName\",\"VendorNumber\":\"1232050274\",\"ApproverNotes\":\"NA\"}",
        "LINE": "{\"FooterDetail\":{\"CurrencyCode\":\"USD\",\"DutyOrCustoms\":0,\"ShippingAndHandlingOrFreight\":0,\"SubTotal\":2.5,\"Tax\":1.5,\"TotalAmount\":10},\"LineItems\":[{\"AssosiatedLineItemID\":\"001\",\"CurrencyCode\":\"USD\",\"Description\":\"Age Paws Paws Nothing\",\"ExtendedPrice\":8,\"Account\":\"752001\",\"LineItemNumber\":\"1\",\"SubstanceGroup\":\"82101905 - Static Media Delivery\",\"PONumber\":null,\"Quantity\":1,\"SAPCostObject\":\"9991010\",\"ShipFrom\":null,\"ShipTo\":null,\"TaxCode\":\"G0\",\"UnitPrice\":10}]}"
      },
      "OperationDateTime": "2017-09-26T11:20:52.6735979Z",
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-ARConverter-Create",
        "TenantTelemetry": {}
      }
    }
    ```

##### Delete Payload

- Below is the sample delete payload:

    ```json
    {
      "DocumentTypeId": "93f9b688-ddc3-43aa-b617-c05a415d1612",
      "Operation": 3,
      "ApprovalIdentifier": {
        "DisplayDocumentNumber": "REQ-34562-126",
        "DocumentNumber": "REQ-34562-126",
        "FiscalYear": "2020"
      },
      "Approvers": null,
      "DeleteFor": [ "johndoe1" ],
      "ActionDetail": {
        "CorrelationId": "26dd7787-a918-457d-b73a-76eedb71a156",
        "Name": "Approve",
        "Date": "2022-04-13T07:27:44.8945494Z ",
        "Comment": "Within Budget",
        "ActionBy": {
          "Alias": "johndoe1",
          "Name": ""
        },
        "NewApprover": {
          "Alias": "johndoe2",
          "Name": ""
        },
        "Placement": "After",
        "AdditionalData": {
          "ActionByDelegateInMSApprovals": ""
        },
        "UserActionFailureReason": ""
      },
      "NotificationDetail": {
        "SendNotification": true,
        "TemplateKey": "ApprovalComplete",
        "To": "johndoe11@organization.com",
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
        "Attachments": [
          {
            "ID": "2792560",
            "DocumentType": null,
            "Name": "Image1.png",
            "Url": ""
          },
          {
            "ID": "",
            "DocumentType": null,
            "Name": "Image2.png",
            "Url": "https://urltothedocument.com"
          }
        ],
        "ApproverNotes": "Justifications for the request"
      },
      "RefreshDetails": true,
      "DetailsData": {
        "DT1": "{\"CurrencyCode\":\"USD\",\"CurrentApprover\":\"johndoe1\",\"DocumentNumber\":\"Req-118480978\",\"FiscalYear\":\"2022\",\"InvoiceAmount\":\"10\",\"InvoiceDate\":\"2020-10-12T00:00:00\",\"InvoiceNumber\":\"Req-118480978\",\"IsFinalApprover\":true,\"PaymentTerms\":\"2% Discount 10 days, net 60 days\",\"Vendor\":\"VendorName-1232050274\",\"VendorComments\":\"NA\",\"VendorInvoiceNumber\":\"Req-118480978\",\"VendorName\":\"VendorName\",\"VendorNumber\":\"1232050274\",\"ApproverNotes\":\"NA\"}",
        "LINE": "{\"FooterDetail\":{\"CurrencyCode\":\"USD\",\"DutyOrCustoms\":0,\"ShippingAndHandlingOrFreight\":0,\"SubTotal\":2.5,\"Tax\":1.5,\"TotalAmount\":10},\"LineItems\":[{\"AssosiatedLineItemID\":\"001\",\"CurrencyCode\":\"USD\",\"Description\":\"Age Paws Paws Nothing\",\"ExtendedPrice\":8,\"Account\":\"752001\",\"LineItemNumber\":\"1\",\"SubstanceGroup\":\"82101905 - Static Media Delivery\",\"PONumber\":null,\"Quantity\":1,\"SAPCostObject\":\"9991010\",\"ShipFrom\":null,\"ShipTo\":null,\"TaxCode\":\"G0\",\"UnitPrice\":10}]}"
      },
      "OperationDateTime": "2017-09-26T11:20:52.6735979Z",
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-ARConverter-Create",
        "TenantTelemetry": {}
      }
    }
    ```

##### Details of all properties involved in create, update and delete payloads

- Below are the definition of all the properties in create, update and delete payloads

| Property Name | Type | Required/Optional | Description |
| --- | --- | --- | --- |
| DocumentTypeId | string | A required property. | It is a GUID in the form of a string provided by Approvals. It is unique for each tenant used to identify the tenant on Approvals end. |
| Operation | int | A required property. | It indicates the kind of operation for which the request has been received. (*1 = For CREATE operation, 2 = For UPDATE operation, 3 = For DELETE operation and 4 = For specific targeted operations*) |
| ApprovalIdentifier | object | A required property. | It encapsulates following properties within it DisplayDocumentNumber, DocumentNumber, Fiscal Year |
| DisplayDocumentNumber | string | A required property. | It can be same as DocumentNumber or a more readable format of the DocumentNumber as this would be shown to approver in UI as an identifier for the approval request.|
| DocumentNumber | string | A required property. | It serves as a primary document number or an identifier for the approval request.|
| FiscalYear | string | An optional property. | It indicates a fiscal year in which the document number exists or is applicable for. |
| Approvers | list of approver object | A required property if an operation type is CREATE or UPDATE and can be null while operation type is DELETE. | It is a list of approvers for which the approval request would be craeted for. |
| DetailTemplate | string | An optional property. | It is intended for future use, it would have details template, if associated with given approver. |
| Delegation | list of string | An optional property. | It is intended for future use, it will contain delegate user alias and is to be provided only if the current approver has officially delegated responsibilities to another user. |
| OriginalApprovers | list of string | An optional property. | It is intended for future use, it would have list of original approvers. |
| Alias | string | A required property if an operation type is CREATE or UPDATE and can be null while operation type is DELETE. | It contains alias of an approver who should be reviewing the approval request. |
| Name | string | A required property if an operation type is CREATE or UPDATE and can be null while operation type is DELETE. | It contains name of an approver who should be reviewing the approval request. |
| DeleteFor | list of string | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It indicates a list of approver aliases, from whose queue, an approval request needs to be removed from. | 
| ActionDetail | object of key-value pair of action specific additional data | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It indicates the key/value pair for additional action-related information (dynamic data depending on the action). Approvals uses the data here to insert history of approval action performed by previous approver. |
| CorrelationId | string | An optional property. | It is no more used. It was used to contain correlation id in any form like number, guid, string, etc (but stored in string) and was used for all correlation needs. Now Xcv and Tcv are used for correlation purposes. |
| Name | string | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It indicates the name of the action taken by previous approver i.e., APPROVE, REJECT, REASSIGN, ADDCOMMENT etc. |
| Date | datetime | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It indicates date and time when action was taken by previous approved or when action was committed on tenant. |
| Comment | string | An optional property. | It should contain the comments that were added by the previous approver while acting on the approval request. |
| ActionBy | NameAliasEntity object | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It has the details of the user who has taken the action on the approval. |
| Alias | string | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It has the alias of an approver who has taken the action action on the approval. |
| Name | string | A required property if an operation type is UPDATE or DELETE and can be null while operation type is CREATE. | It has the name of an approver who has taken the action action on the approval. |
| NewApprover | NameAliasEntity object | An optional property. | It is used for certain type of action taken by the approvers like *Add approver*, wherein approver may add a new approverto the request. This property would then contain the name and the alias of the approver for whom this approval is intended for in future. |
| Alias | string | An optional property. | It has the alias of an approver for whom this approval is intended for in future. |
| Name | string | An optional property. | It has the name of an approver for whom this approval is intended for in future. |
| Placement | string |An optional property. | It denotes the position (“Before”, ”After”, ”End”) at which the new approver is to be added in case of “add approver” functionality. |

### Details of Approval process

This is when an approver acts on the approval request within Approvals. At this point Approvals would make a call to tenant action API. It is a synchronous POST call and involves:
- ApprovalRequest
- ApprovalResponse

#### Approval Request

This is an outbound call, i.e., sent from Approvals to tenant. Approvals calls tenant when an action is taken within Approvals. PFB the sample JSONs for ApprovalRequest:

##### Single approval scenario

- Below is the sample of the ApprovalRequest body posted by Approvals to tenant for a single approval scenario:

    ```json
    {
      "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
      "Action": "Approve",
      "ActionByAlias": "johndoe1",
      "ActionByDelegateInMSApprovals": "johndoe2",
      "OriginalApproverInTenantSystem": "johndoe3",
      "ActionDetails": {
        "Comment": "Approval comments",
        "ActionDate": "2022-03-12T12:29:09.878Z"
      },
      "ApprovalIdentifier": {
        "DisplayDocumentNumber": "REQ-34562-126",
        "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655a",
        "FiscalYear": "2020"
      },
      "AdditionalData": {
        "Case Number": "REQ-CEH-764748"
      },
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-ApprovalAction-Approve",
        "TenantTelemetry": {}
      }
    }
    ```

##### Bulk approval scenario

- For scenarios wherein tenant supports/needs bulk approvals to be supported, ApprovalRequest body changes to support the list of ApprovalRequests based on configurations. Below is the sample of the ApprovalRequest body posted by Approvals to tenant for bulk approval scenarios:

    ```json
    [
      {
        "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
        "Action": "Approve",
        "ActionByAlias": "johndoe1",
        "ActionByDelegateInMSApprovals": "johndoe2",
        "OriginalApproverInTenantSystem": "johndoe3",
        "ActionDetails": {
          "Comment": "Approval comments",
          "ActionDate": "2022-03-12T12:29:09.878Z"
        },
        "ApprovalIdentifier": {
          "DisplayDocumentNumber": "REQ-34562-126",
          "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655a",
          "FiscalYear": "2020"
        },
        "AdditionalData": {
          "Case Number": "REQ-CEH-764748"
        },
        "Telemetry": {
          "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
          "Xcv": "REQ-34562-126",
          "BusinessProcessName": "Approvals-TenantName-ApprovalAction-Approve",
          "TenantTelemetry": {}
        }
      },
      {
        "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
        "Action": "Approve",
        "ActionByAlias": "johndoe1",
        "ActionByDelegateInMSApprovals": "johndoe2",
        "OriginalApproverInTenantSystem": "johndoe3",
        "ActionDetails": {
          "Comment": "Approval comments",
          "ActionDate": "2022-03-12T12:29:09.878Z"
        },
        "ApprovalIdentifier": {
          "DisplayDocumentNumber": "REQ-34562-127",
          "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655b",
          "FiscalYear": "2020"
        },
        "AdditionalData": {
          "Case Number": "REQ-CEH-764749"
        },
        "Telemetry": {
          "Tcv": "26dd7787-a918-457d-b73a-76eedb71a127",
          "Xcv": "REQ-34562-1267",
          "BusinessProcessName": "Approvals-TenantName-ApprovalAction-Approve",
          "TenantTelemetry": {}
        }
      }
    ]
    ```

#### Approval Response

This is the response format to the synchronous ApprovalRequest which Approvals should be receiving as a response from the tenant. PFB the sample JSONs for ApprovalResponse on success and failure Scenarios:

##### Approval Response for single approval success scenario

- Below is the sample of the ApprovalResponse body which Approvals should be receiving to the synchronous ApprovalRequest call from Approvals to tenant during success scenario:

    ```json
    {
      "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
      "ActionResult": true,
      "ApprovalIdentifier": {
        "DisplayDocumentNumber": "REQ-34562-126",
        "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655a",
        "FiscalYear": "2020"
      },
      "E2EErrorInformation": null,
      "DisplayMessage": "The submitted action was processed successfully.",
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-Action-Success"
      }
    }
    ```

##### Approval Response for single approval failure scenario

- Below is the sample of the ApprovalResponse body which Approvals should be receiving to the synchronous ApprovalRequest call from Approvals to tenant during failure scenario. For example, some business validation failure happened on tenant end and need approver to act in a particular way for next steps.

    ```json
    {
      "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
      "ActionResult": false,
      "ApprovalIdentifier": {
        "DisplayDocumentNumber": "REQ-34562-126",
        "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655a",
        "FiscalYear": "2020"
      },
      "E2EErrorInformation": {
        "ErrorMessages": [
          "Code_SL001: Unable to check safe limits for Safe Service. No active entity present in the database for the given user."
        ],
        "ErrorType": 2,
        "RetryInterval": 3
      },
      "DisplayMessage": "You do not have Safe Limits to approve this request. Please visit <https://somesystem.com > and submit a new Safe Limit request. Once the have valid Authorization limits, you would be able to take action on this request. Please contact mailto:<supportalias mailto:<supportalias <mailto:<supportalias>> for any kind of support.",
      "Telemetry": {
        "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
        "Xcv": "REQ-34562-126",
        "BusinessProcessName": "Approvals-TenantName-Action-Failure"
      }
    }
    ```

##### Approval Response for bulk approvals success and failure scenario

- For scenarios wherein tenant supports/needs bulk approvals to be supported, ApprovalRequest body changes to support the list of ApprovalRequests based on configurations. In such cases Approvals need the Approval Response body to change accordingly. Below is the sample of the ApprovalResponse body which Approvals should be receiving to the synchronous ApprovalRequest call from Approvals to tenant during success or failure scenario during bulk approval scenarios:

    ```json
    [
      {
        "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
        "ActionResult": true,
        "ApprovalIdentifier": {
          "DisplayDocumentNumber": "REQ-34562-126",
          "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655a",
          "FiscalYear": "2020"
        },
        "E2EErrorInformation": null,
        "DisplayMessage": "The submitted action was processed successfully.",
        "Telemetry": {
          "Tcv": "26dd7787-a918-457d-b73a-76eedb71a124",
          "Xcv": "REQ-34562-126",
          "BusinessProcessName": "Approvals-TenantName-Action-Success"
        }
      },
      {
        "DocumentTypeID": "93f9b688-ddc3-43aa-b617-c05a415d1612",
        "ActionResult": false,
        "ApprovalIdentifier": {
          "DisplayDocumentNumber": "REQ-34562-127",
          "DocumentNumber": "23c899a5-0db0-4744-bc94-beecc401655b",
          "FiscalYear": "2020"
        },
        "E2EErrorInformation": {
          "ErrorMessages": [
            "Code_SL001: Unable to check safe limits for Safe Service. No active entity present in the database for the given user."
          ],
          "ErrorType": 2,
          "RetryInterval": 3
        },
        "DisplayMessage": "You do not have Safe Limits to approve this request. Please visit <https://somesystem.com > and submit a new Safe Limit request. Once the have valid Authorization limits, you would be able to take action on this request. Please contact mailto:<supportalias mailto:<supportalias <mailto:<supportalias>> for any kind of support.",
        "Telemetry": {
          "Tcv": "26dd7787-a918-457d-b73a-76eedb71a127",
          "Xcv": "REQ-34562-127",
          "BusinessProcessName": "Approvals-TenantName-Action-Failure"
        }
      }
    ]
    ```

| Property Name | Type | Remarks |
|--|--|--|
| DocumentTypeID | String | Approvals assigned unique identifier for each tenant.Required field and helps to identify the tenant. |
| ActionResult | Boolean | Required field and helps to identify the overall result of the transaction. |
| ApprovalIdentifier | Object | Document Identifier. Contains the - DisplayDocumentNumber (DocumentNumber used for displaying to the user in a user-friendly manner), DocumentNumber, Fiscal Year (Fiscal year in which the document number exists or is applicable) |
| E2EErrorInformation | Object | E2E Error Info which carries additional data about each error that occurred. Applicable for error scenarios only. |
| --	ErrorMessages | List of string | List of technical error messages that occurred on the tenant system for this transaction. The error list can comprise of all the related errors that happened in the tenant's downstream systems as well for enhanced telemetry. |
| --	ErrorType |Enum  | Indicates the type of error that occurred. Default value is NonTransient, but should be overwritten when error type is known by the source system for improved error handling at destination (Approvals).Proper ErrorType should be used (1 – Intended Errors [cannot be retried], 2- Unintended Errors [cannot be retried], 3- Unintended Transient Errors [can be retried and thus RetryInterval is mandatory]) |
| --	RetryInterval | Integer | Transaction retry interval (in mins). Mandatory parameter if ErrorType is known to be UnintendedTransientError. This will be to retry after this interval. Please use this only if a transient error occurred and not when a non-transient or fatal error occurs. |
|  DisplayMessage| String | A display error message when a specific error message needs to be shown to the user .Provide a message for the user even if response is http OK. DisplayMessage shouldn’t have technical error message. It should have user friendly error message while the technical error message should be in the ErrorMessages property. For non-business failure errors, like code exception, a generic message with support link should be sent in the DisplayMessage. Ex. If a business rule fails, user needs to understand the reason for failure and hence should see this message. Ex. In case a SQL Connection fails to establish, ErrorMessage can show SQL connection failed, but DisplayMessage should show something like "Your request could not be processed. Please try again later". |
| Telemetry | Object | Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging. |

### Details of attaching documents while performing actions for Tenant
While performing approval action, users might optionally upload an attachment to the approval request such as receipt or proof of delivery, etc. 
Tenants can control various aspects of this feature such as,
- Are users allowed to upload documents while performing approval action.
- Are users required/mandated to upload documents while perforiming approval action.
- If allowed, what are maximum number of documents that user can attach with the request.
- If allowed, what is the maximum size of documents that user can attach with the request.

While onboarding tenants to the Approval framework, we configure tenants with following properties,

| Property Name | Type | Remarks |
|--|--|--|
| AttachmentContainerName| String | The name of blob container provisioned per tenants where all attahcments are stored. |
| FileAtachmentOptions | Object | Collection of different settings per tenant to control the attachment feature. |
| -- AllowFileUpload | Boolean| Boolean identifier to control if user is allowed to upload the file attachment. |
| -- MaxAttachments | Integer | Maximum number of attachments allowed for user to attach with the approval request. |
| -- AllowedFileTypes | String | The different types of file allowed to be attached with approval request. |
| -- MaxFileSizeInBytes | Integer | Maximum size of files in byte allowed for user per file to attach with the approval request. |

It should be noted that tenant can have some files sent as part of the approval request, seprate from what user is attaching. These are controlled by 'attachmentType'. There are api's exposed to preview and upload the attchments.
For tenant to fetch this attachments, they are configured with access via Managed identity on the container level specified in AttachmentContainerName property while onboarding the tenant. It is responsibility of the tenant to pull the attachments from the storage prior to completion of approval workflow and use them in its approval process. Once the approval request is deleted, all documents related to the request will be cleaned up from approval store.
The approval supports upload failures and displays them gracefully on presentation layer. For example if there were 5 documents there were uploaded by user, 4 of them were successfully processed and 1 got errored out, the presenttion layer will popup message for the failed one and ask user to retry. 
