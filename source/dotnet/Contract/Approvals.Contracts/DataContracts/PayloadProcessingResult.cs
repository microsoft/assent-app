// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class PayloadProcessingResult
    {
        /// <summary>
        /// Constructor with ID and Result params
        /// </summary>
        /// <param name="payloadId"></param>
        /// <param name="payloadValidationResults"></param>
        public PayloadProcessingResult(Guid payloadId, List<ValidationResult> payloadValidationResults)
        {
            PayloadId = payloadId;
            PayloadValidationResults = payloadValidationResults;
            ReconciliationAwaitTime = 180;
        }

        /// <summary>
        /// Constructor with ID, Result and ReconciliationTime params
        /// </summary>
        /// <param name="payloadId"></param>
        /// <param name="payloadValidationResults"></param>
        /// <param name="ReconciliationAwaitTime"></param>
        internal PayloadProcessingResult(Guid payloadId, List<ValidationResult> payloadValidationResults, int reconciliationAwaitTime)
        {
            PayloadId = payloadId;
            PayloadValidationResults = payloadValidationResults;
            ReconciliationAwaitTime = reconciliationAwaitTime;
        }

        /// <summary>
        /// Identifier for payload. The same value is also assigned to the brokered message
        /// </summary>
        public Guid PayloadId { get; set; }

        /// <summary>
        /// Payload validation results
        /// </summary>
        public List<ValidationResult> PayloadValidationResults { get; set; }

        /// <summary>
        /// Time in SECONDS for which the tenant should wait before checking back with Approvals for reconciling the status of the payload
        /// </summary>
        public int ReconciliationAwaitTime { get; set; }
    }
}