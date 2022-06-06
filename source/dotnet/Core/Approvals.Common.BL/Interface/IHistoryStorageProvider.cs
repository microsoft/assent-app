// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Model;
    public interface IHistoryStorageProvider
    {
        /// <summary>
        ///  Returns history based on alias, time period, and action taken
        /// </summary>
        /// <param name="alias">The approver's alias.</param>
        /// <param name="actionDate">The action date.</param>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="actionTaken">The action taken.</param>
        /// <returns>
        /// List of transaction history
        /// </returns>
        Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string actionDate, string documentNumber, string actionTaken);

        /// <summary>
        /// Saves TransactionHistory data
        /// </summary>
        /// <param name="historyData">The transaction data.</param>
        /// <returns>
        /// </returns>
        Task AddApprovalHistoryAsync(TransactionHistory historyData);

        /// <summary>
        /// Saves list of TransactionHistory data
        /// </summary>
        /// <param name="historyDataList">The list of transaction data</param>
        /// <returns></returns>
        Task AddApprovalHistoryAsync(List<TransactionHistory> historyDataList);

        /// <summary>
        /// Returns history based on tenantid and documentnumber
        /// </summary>
        /// <param name="tenantId">The TenantID.</param>
        /// <param name="documentNumber">The document number.</param>
        /// <returns>
        /// List of transaction history
        /// </returns>
        Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber);

        /// <summary>
        ///  Returns history based on tenantid, documentnumber, and approver
        /// </summary>
        /// <param name="tenantId">The TenantID.</param>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="approver">The approver's alias.</param>
        /// <returns>
        /// List of transaction history
        /// </returns>
        Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber, string approver);

        /// <summary>
        ///  Returns history based on tenantid, documentnumber, and approver
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="timePeriod"></param>
        /// <returns>
        /// List of transaction history
        /// </returns>
        Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, int timePeriod);
    }
}
