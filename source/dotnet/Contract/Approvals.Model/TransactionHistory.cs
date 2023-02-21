// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using Newtonsoft.Json;

public partial class TransactionHistory : BaseTableEntity
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    private double? approvalAmount;
    private string unitValue;
    public Nullable<DateTimeOffset> ActionDate { get; set; }
    public string ActionTaken { get; set; }
    public string AmountUnits { get; set; }

    public Nullable<double> ApprovalAmount
    {
        get
        {
            double amount = 0;
            if (double.TryParse(UnitValue, out amount))
                return amount;
            else
                return approvalAmount;
        }
        set
        {
            unitValue = value?.ToString();
            approvalAmount = value;
        }
    }

    public string UnitValue
    {
        get
        {
            if (approvalAmount != null)
            {
                return approvalAmount.ToString();
            }
            else
            {
                return unitValue;
            }
        }
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                unitValue = value;
            }
        }
    }

    public string Approver { get; set; }
    public string ApproversNote { get; set; }
    public string DocumentNumber { get; set; }
    public string DocumentTypeID { get; set; }
    public string JsonData { get; set; }
    public string SubmittedAlias { get; set; }
    public Nullable<DateTimeOffset> SubmittedDate { get; set; }
    public string SubmitterName { get; set; }
    public string TenantId { get; set; }
    public string TenantUrl { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string VendorInvoiceNumber { get; set; }
    public string FiscalYear { get; set; }
    public string CompanyCode { get; set; }
    public string CustomAttribute { get; set; }
    public string AppName { get; set; }
    public string ActionTakenOnClient { get; set; }
    public string MessageId { get; set; }
    public string DelegateUser { get; set; }
    public string Xcv { get; set; }
}