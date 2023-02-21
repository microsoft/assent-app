// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Details of any given (user) action.
    /// </summary>
    public class ActionDetail
    {
        /// <summary>
        /// Contains Correlation Id in any form like number, guid, string, etc (but stored in string) and should be used for all correlation needs
        /// </summary>
        public string CorrelationId { get; set; }


        /// <summary>
        /// A simple string representing the Action taken (APPROVE, REJECT, REASSIGN, ADDCOMMENTs etc.).
        /// For Update and Delete operations, ActionDetail is mandatory field, hence Name property should have the Actionname value.
        /// For CREATE operation, ActionDetail must be null, hence Name property does not exist
        /// </summary>
        // Validation for Name null or empty
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ActionDetailNameMessage)]
        public string Name
        { get; set; }

        /// <summary>
        /// Date time when Action was taken or committed on tenant.
        /// For Update and Delete operations, ActionDetail is mandatory field, hence Date property should have the timestamp when the action was taken.
        /// For CREATE operation, ActionDetail must be null, hence Date property does not exist
        /// </summary>
        // Validation for Date null or empty
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ActionDetailDateMessage)]
        //[Range(typeof(DateTime), DateTime.MinValue.ToString(), DateTime.MaxValue.ToString(), ErrorMessage = Constants.ActionDetailDateMessage)]
        public DateTime Date
        { get; set; }

        /// <summary>
        /// This is a string containing the comments that were added by the user while taking any particular action on the request pending in his queue
        /// Comment field is optional for all Delete and Update operations. For Create operation Comment field does not exists
        /// </summary>
        public string Comment
        { get; set; }

        /// <summary>
        /// Details of user who has taken the action on the approval.
        /// For Update and Delete operations, ActionDetail is mandatory field, hence ActionBy  property should have the Name and Alias of the user who has taken action.
        /// For CREATE operation, ActionDetail must be null, hence Name property does not exist
        /// </summary>
        // Validation for ActionBy null
        [Required(ErrorMessage = Constants.ActionDetailActionByMessage)]
        public NameAliasEntity ActionBy
        { get; set; }

        /// <summary>
        /// This property contains the name and the alias of the approver for whom this approval is intended for in future.
        /// </summary>
        public NameAliasEntity NewApprover
        { get; set; }

        /// <summary>
        /// This string (“Before”,”After”,”End”) denotes the position at which the new approver is to be added in case of “add approver” functionality.
        /// </summary>
        public string Placement
        { get; set; }

        /// <summary>
        /// Any additional info (Not for display, but for emails ex. ReasonCode, ReasonText etc.).
        /// Comment field is optional for all Delete and Update operations. 
        /// For Create operation Comment field does not exists
        /// </summary>        
        public Dictionary<string, string> AdditionalData
        { get; set; }

        /// <summary>
        /// Display exact reason of action failure
        /// It may be from Approvals side or Tenant side
        /// </summary>
        public string UserActionFailureReason { get; set; }
    }
}