// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// CustomAttribute - contains name and value of the attribute.
    /// </summary>
    [DataContract]
    public class CustomAttribute
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CustomAttribute()
        {
            CustomAttributeName = null;
            CustomAttributeValue = null;
        }

        /// <summary>
        /// Name of the custom attribute.
        /// </summary>
        [DataMember]
        public string CustomAttributeName { get; set; }

        /// <summary>
        /// Value of the custom attribute.
        /// </summary>
        [DataMember]
        public string CustomAttributeValue { get; set; }
    }
}