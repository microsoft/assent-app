using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    public interface IAdaptiveCardResponseHelper
    {
        /// <summary>
        /// Creates a basic adaptive card with a single text block message.
        /// </summary>
        /// <param name="message">The message text to display.</param>
        /// <returns>Serialized adaptive card JSON string.</returns>
        string CreateTextCard(string message);

        /// <summary>
        /// Constructs a take action card for the finance assistant
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="bodyText"></param>
        /// <param name="askRequest"></param>
        /// <param name="chatRequestContextArgs"></param>
        /// <returns></returns>
        string CreateTakeActionCard(string headerText, string bodyText, AskRequest askRequest, ChatRequestEventArgs chatRequestContextArgs);

        /// <summary>
        /// Creates an adaptive card using a pre-fetched template for parallel fetch scenarios.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="documentNumber">Document number of the approval request.</param>
        /// <param name="details">The request details data as JObject.</param>
        /// <param name="prefetchedTemplate">The pre-fetched adaptive card template.</param>
        /// <param name="userAlias">User alias.</param>
        /// <param name="loggedInAlias">Logged-in alias.</param>
        /// <param name="oauth2UserToken">OAuth 2.0 user token.</param>
        /// <param name="objectId">User's object ID.</param>
        /// <param name="domain">User's domain.</param>
        /// <param name="approverDisplayName">Display name of the approver (optional).</param>
        /// <param name="tcv">Transaction correlation vector for tracking (optional).</param>
        /// <returns>A sanitized Adaptive Card JObject suitable for Copilot/M365.</returns>
        JObject CreateApprovalAssistantRequestCard(
            int tenantId,
            string documentNumber,
            JObject details,
            JObject prefetchedTemplate,
            string userAlias,
            string loggedInAlias,
            string oauth2UserToken,
            string objectId,
            string domain,
            string approverDisplayName = "",
            string tcv = "");
    }
}