// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface;

public interface IRequestAttachment
{
    string Name { get; set; }
    string ID { get; set; }
    string Url { get; set; }
    System.Net.Http.HttpContent Content { get; set; }
    bool IsPreAttached { get; set; }
}

public class RequestAttachment : IRequestAttachment
{
    public string Name { get; set; }
    public string ID { get; set; }
    public string Url { get; set; }
    public System.Net.Http.HttpContent Content { get; set; }
    public bool IsPreAttached { get; set; } = true;
}
