// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    public class ActionAuditLogInfo
    {
        public string DisplayDocumentNumber { get; set; }
        public string ActionDateTime { get; set; }
        public string TenantId { get; set; }
        public string ActionTaken { get; set; }
        public string UnitValue { get; set; }
        public string UnitOfMeasure { get; set; }
        public string ActionStatus { get; set; }
        public string ErrorMessage { get; set; }
        public string ClientType { get; set; }
        public string Approver { get; set; }
        public string ImpersonatedUser { get; set; }
    }
}
