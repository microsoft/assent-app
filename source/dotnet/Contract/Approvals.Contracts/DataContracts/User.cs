// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    /// <summary>
    /// Represents a user and contains user details like alias and name.
    /// This type is extended or contained in other types.
    /// </summary>
    public class User : Microsoft.Graph.Models.User
    {
        private string alias;

        /// <summary>
        /// Alias of the user.
        /// </summary>
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
        public string Name
        { get; set; }
    }
}