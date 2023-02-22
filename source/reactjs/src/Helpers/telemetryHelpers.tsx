import { IAuthClient } from '@micro-frontend-react/employee-experience/lib/IAuthClient';
import { ITelemetryClient } from '@micro-frontend-react/employee-experience/lib/ITelemetryClient';

export enum TrackingEventId {
    SummaryLoadSuccess = 100000,
    SummaryLoadFailure,
    ProfileLoadSuccess,
    ProfileLoadFailure,
    DelegationsLoadSuccess,
    DelegationsLoadFailure,
    HistoryLoadSuccess,
    HistoryLoadFailure,
    HistoryDownloadSuccess,
    HistoryDownloadFailure,
    TenantInfoLoadSuccess,
    TenantInfoLoadFailure,
    OOSSummaryLoadSuccess,
    OOSSummaryLoadFailure,
    PostRequestAsReadSuccess,
    PostRequestAsReadFailure,
    CallbackLoadSuccess,
    CallbackLoadFailure,
    PostActionSuccess,
    PostActionFailure,
    AttachmentLoadSuccess,
    AttachmentLoadFailure,
    AttachmentPreviewLoadSuccess,
    AttachmentPreviewLoadFailure,
    AttachmentZipLoadSuccess,
    AttachmentZipLoadFailure,
    DetailsLoadInitiate,
    DetailsLoadSuccess,
    DetailsLoadSuccessWithAuthFailure,
    DetailsLoadSuccessWithLobFailure,
    DetailsLoadFailure,
    HeaderLoadSuccess,
    HeaderLoadSuccessWithAuthFailure,
    HeaderLoadSuccessWithLobFailure,
    HeaderLoadFailure,
    UserImageLoadSuccess,
    UserImageLoadFailure,
    DownloadAttachment,
    DownloadAllAttachments,
    PreviewAttachments,
    CloseAttachmentPreview,
    GroupByTenant,
    GroupBySubmitter,
    GroupByDate,
    GroupByApprovalAmount,
    SummaryCardClicked,
    ParseActionErrorResponseFailure,
    RenderPrimaryActionButtonsFailure,
    RenderSecondaryActionButtonsFailure,
    MicrofrontendSubmitEventFailure,
    MicrofrontendActionEventFailure,
    MicrofrontendCancelEventFailure,
    GraphUsersLoadSuccess,
    GraphUsersLoadFailure,
    SaveUserPreferenceSuccess,
    SaveUserPreferenceFailure,
    GetUserPreferenceSuccess,
    GetUserPreferenceFailure,
    NotificationsSuccess,
    NotificationsFailure,
    UpdateNotificationsSuccess,
    UpdateNotificationsFailure,
    SaveEditableDetailsSuccess,
    SaveEditableDetailsFailure,
    GetPullTenantSummarySuccess,
    GetPullTenantSummaryFailure,
    GetExternalTenantInfoSuccess,
    GetExternalTenantInfoFailure,
    GetPullTenantSummaryCountSuccess,
    GetPullTenantSummaryCountFailure,
    CardView,
    TableView,
    DockedView,
    FlyOutView,
    BulkApproval,
    FileAttachmentUploadFailure,
    FileAttachmentUploadSuccess,
    ParseFileAttachmentUploadFailure,
    FeedbackLaunchFailure,
}

export const getContextCommonTelemetryProperties = (
    authClient: IAuthClient,
    telemetryClient: ITelemetryClient,
    appAction: string,
    eventName: string,
    eventId: number
): any => {
    const occurenceTime = new Date();
    const correlationId = telemetryClient.getCorrelationId();
    const logData = {
        AppAction: appAction,
        EventOccurenceTime: occurenceTime,
        SessionId: correlationId,
        EventId: eventId,
        EventName: eventName,
        ComponentType: 'Web',
    };
    return logData;
};

export const trackBusinessProcessEvent = (
    authClient: IAuthClient,
    telemetryClient: ITelemetryClient,
    businessProcessName: string,
    appAction: string,
    eventId: number,
    stateCommonProperties: any,
    additionalProperties: any = {}
): void => {
    try {
        const businessProcessProperties = {
            BusinessProcessName: businessProcessName,
            EventType: 'BusinessProcessEvent',
        };
        const contextCommonProperties = getContextCommonTelemetryProperties(
            authClient,
            telemetryClient,
            appAction,
            businessProcessName,
            eventId
        );
        const telemetryProperties = Object.assign(
            businessProcessProperties,
            stateCommonProperties,
            contextCommonProperties,
            additionalProperties
        );
        telemetryClient.trackEvent({ name: businessProcessName }, telemetryProperties);
    } catch (error) {}
};

export const trackFeatureUsageEvent = (
    authClient: IAuthClient,
    telemetryClient: ITelemetryClient,
    eventName: string,
    appAction: string,
    eventId: number,
    stateCommonProperties: any,
    additionalProperties: any = {}
): void => {
    try {
        const featureUsageProperties = {
            EventType: 'FeatureUsageEvent',
        };
        const contextCommonProperties = getContextCommonTelemetryProperties(
            authClient,
            telemetryClient,
            appAction,
            eventName,
            eventId
        );
        const telemetryProperties = Object.assign(
            featureUsageProperties,
            stateCommonProperties,
            contextCommonProperties,
            additionalProperties
        );
        telemetryClient.trackEvent({ name: eventName }, telemetryProperties);
    } catch (error) {}
};

export const trackException = (
    authClient: IAuthClient,
    telemetryClient: ITelemetryClient,
    eventName: string,
    appAction: string,
    eventId: number,
    stateCommonProperties: any,
    exception: Error,
    additionalProperties: any = {}
): void => {
    try {
        const contextCommonProperties = getContextCommonTelemetryProperties(
            authClient,
            telemetryClient,
            appAction,
            eventName,
            eventId
        );
        const telemetryProperties = Object.assign(stateCommonProperties, contextCommonProperties, additionalProperties);
        telemetryClient.trackException({ exception: exception, properties: telemetryProperties });
    } catch (error) {}
};
