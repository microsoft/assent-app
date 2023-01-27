// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Constant;
public enum ServiceBusMessageType
{
    Active,
    DeadLetter
}
public enum FlightingFeatureStatus : int
{
    Disabled = 1, // The feature is disabled
    EnabledForAll = 2, // Not a flighting feature; feature in production
    InFlighting = 3 // The feature is a flighting feature
}

public enum LogDataKey
{
    //TestHarness keys
    _TrackingEvent = 1,
    EventId = 2,
    EventName = 3,
    Xcv = 4,
    Tcv = 5,
    ComponentName = 6,
    MSAComponentName = 7,
    Environment = 8,
    Attachment = 9,
    Operation = 10,
    TenantName = 11,
    UserAlias = 12,
    DocumentNumber = 13,
    DaysToDelete = 14,
    BlobName = 15,
    InvalidDouments = 16,
    SuccessDocuments = 17,
    FailureDocuments = 18,
    PayloadResult = 19,
    PayloadType = 20,
    Submitter = 21,
    TenantId = 22,
    Load = 23,
    BatchSize = 24,
    DetailOperation = 25,
    ActionOrComponentUri = 26,
    LoggingDateTimeUtc = 27
}

public enum TrackingEvent
{
    //TestHarness events
    MissingAttachmentName = 1,
    AttachmentFetchFailure = 2,
    BulkDeleteStarted = 3,
    BulkDeleteFailure = 4,
    TenantAndApproverFetchFailure = 5,
    LoadGenerationFailure = 6,
    GetSchemaFileFailure = 7,
    GetBlobFailure = 8,
    GetBlobSuccessful = 9,
    UploadToBlobSuccessful = 10,
    UploadToBlobFailure = 11,
    SampleDataFetchForMasterPayload = 12,
    SampleDataFetchForTenantPayload = 13,
    MasterPayloadSampleDataFetchFailure = 14,
    GetUISchemaFileFailure = 15,
    UploadDataToBlobFailure = 16,
    GetSchemaFromSamplePayloadFailure = 17,
    UploadPayloadValueFailure = 18,
    InsertSyntheticDetailFailure = 19,
    GenerateFormSuccessful = 20,
    GenerateFormFailure = 21,
    SendPayloadSuccessful = 22,
    SavePayloadSuccessful = 23,
    SendPayloadFailure = 24,
    TokenGenerationFailure = 25,
    BulkDeleteCompleted = 26,
    ErrorFetchingDeleteRequests = 27,
    SendPayloadStarted = 28,
    SendPayloadCompleted = 29,
    SendDeletePayloadFailure = 30,
    DocumentStatusUpdateCompleted = 31,
    DocumentStatusUpdateFailure = 32,
    ActionSuccessful = 33,
    ActionFailure = 34,
    BulkActionSuccessful = 35,
    DetailFetchSuccessful = 36,
    GetAttachmentFailure = 37,
    NoPendingSummaryData = 38,
    PullTenantSummaryDataSuccess = 39,
    PullTenantSummaryFailure = 40,
    PullTenantDetailsSuccess = 41,
    PullTenantDetailsFailure = 42,
    DetailFetchFailure = 43
}

public enum ConfigurationKey
{
    TestHarnessApproverAlias
}