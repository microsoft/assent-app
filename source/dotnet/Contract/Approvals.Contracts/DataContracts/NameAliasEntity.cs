// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a user and contains user details like alias and name.
    /// This type is extended or contained in other types.
    /// </summary>
    [DataContract]
    public class NameAliasEntity
    {
        private string alias;

        /// <summary>
        /// Alias of the user.
        /// </summary>
        [DataMember]
        public string Alias
        { 
            get { return alias; }
            set 
            {
                if (value == null)
                    alias = null;
                else
                    alias = value.ToLower();
            } 
        }

        /// <summary>
        /// Name of the user.
        /// </summary>
        [DataMember]
        public string Name
        { get; set; }
    }
}
