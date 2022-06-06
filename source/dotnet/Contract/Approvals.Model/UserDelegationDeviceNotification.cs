// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using System.Runtime.Serialization;
    public class UserDelegationDeviceNotification
    {
        #region Notification Info
        [DataMember]
        public String To { get; set; }

        [DataMember]
        public String CC { get; set; }

        [DataMember]
        public String NotificationTemplateKey { get; set; }

        [DataMember]
        public DateTime DateTo { get; set; }

        [DataMember]
        public DateTime DateFrom { get; set; }

        [DataMember]
        public String ActionTaken { get; set; }

        [DataMember]
        public String Xcv { get; set; }

        [DataMember]
        public String Tcv { get; set; }

        #endregion
    }
}
