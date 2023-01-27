// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System.Runtime.Serialization;

[DataContract(Name = "ApprovalCount", Namespace = "http://www.microsoft.com/document/routing/2010/11")]
public class ApprovalCount
{
    [DataMember]
    public string DocumentTypeId { get; set; }

    [DataMember]
    public int Count { get; set; }

    [DataMember]
    public string AppName { get; set; }
}    
