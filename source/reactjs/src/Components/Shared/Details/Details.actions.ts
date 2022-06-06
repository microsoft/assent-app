import {
    DetailsActionType,
    IRequestUserImageAction,
    IReceiveUserImageAction,
    IFailedUserImageAction,
    IRequestDetailsAction,
    IReceiveHeaderAction,
    IFailedHeaderAction,
    IReceiveDetailsAction,
    IFailedDetailsAction,
    IRequestCallbackDetailsAction,
    IReceiveCallbackDetailsAction,
    IFailedCallbackDetailsAction,
    IUpdateDetailsRequest,
    IPostAction,
    IReceiveActionResponse,
    IRequestDocumentAction,
    IReceiveDocumentPreview,
    IClearDocumentPreview,
    ICloseDocumentPreview,
    IFailedPostAction,
    IRequestDocumentPreviewAction,
    IRequestAllDocumentsAction,
    IReinitializeDetailsAction,
    IRequestHeaderAction,
    IRequestFullyScrolledAction,
    IMarkRequestAsReadAction,
    IFailedDocumentDownloadAction,
    IFailedDocumentPreviewAction,
    IFailedAllDocumentsAction,
    IOpenMicrofrontendAction,
    ICloseMicrofrontendAction,
    IUpdateAdditionalDataAction,
    IToggleHistoryDetailPanel,
    ISetFooterHeight,
    IRequestFullyRenderedAction,
    IReceiveAreDetailsEditable,
    IPostEditableDetailsAction,
    IFailedEditDetailsAction,
    ISetBulkFooterHeight,
    ISetBulkMessagebarHeight,
    ISetAliasMessagebarHeight,
    IRequestDocumentStart,
    IRequestDocumentEnd
} from './Details.action-types';

export function setFooterHeight(height: number): ISetFooterHeight {
    return {
        type: DetailsActionType.SET_FOOTER_HEIGHT,
        height
    };
}

export function setBulkFooterHeight(height: number): ISetBulkFooterHeight {
    return {
        type: DetailsActionType.SET_BULK_FOOTER_HEIGHT,
        height
    };
}

export function setBulkMessagebarHeight(height: number): ISetBulkMessagebarHeight {
    return {
        type: DetailsActionType.SET_BULK_MESSAGEBAR_HEIGHT,
        height
    };
}

export function setAliasMessagebarHeight(height: number): ISetAliasMessagebarHeight {
    return {
        type: DetailsActionType.SET_ALIAS_MESSAGEBAR_HEIGHT,
        height
    };
}

export function requestMyDetails(
    tenantId: string,
    documentNumber: string,
    displayDocumentNumber: string,
    userAlias: string,
    requiresTemplate: boolean,
    isPullModelEnabled: boolean
): IRequestDetailsAction {
    return {
        type: DetailsActionType.REQUEST_MY_DETAILS,
        tenantId,
        documentNumber,
        displayDocumentNumber,
        userAlias,
        requiresTemplate,
        isPullModelEnabled
    };
}
export function toggleHistoryDetailPanelAction(): IToggleHistoryDetailPanel {
    return {
        type: DetailsActionType.TOGGLE_HISTORY_DETAIL_PANEL
    };
}

export function requestUserImage(userAlias: string): IRequestUserImageAction {
    return {
        type: DetailsActionType.REQUEST_USER_IMAGE,
        userAlias
    };
}

export function receiveUserImage(userImage: any): IReceiveUserImageAction {
    return {
        type: DetailsActionType.RECEIVE_USER_IMAGE,
        userImage
    };
}

export function failedUserImage(userImageErrorMessage: string): IFailedUserImageAction {
    return {
        type: DetailsActionType.FAILED_USER_IMAGE,
        userImageErrorMessage
    };
}

export function requestHeader(
    tenantId: string,
    documentNumber: string,
    userAlias: string,
    isPullModelEnabled: boolean,
    summaryJSON?: object,
    summaryDataMapping?: string
): IRequestHeaderAction {
    return {
        type: DetailsActionType.REQUEST_HEADER,
        tenantId,
        documentNumber,
        userAlias,
        isPullModelEnabled,
        summaryJSON,
        summaryDataMapping
    };
}

export function receiveHeader(details: any, template: any, summaryObj?: any): IReceiveHeaderAction {
    return {
        type: DetailsActionType.RECEIVE_HEADER,
        details,
        template,
        summaryObj
    };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function receiveMyDetails(details: any, template?: any): IReceiveDetailsAction {
    return {
        type: DetailsActionType.RECEIVE_MY_DETAILS,
        details,
        template: template ?? null
    };
}

export function failedDetails(errorMessage: string): IFailedDetailsAction {
    return {
        type: DetailsActionType.FAILED_DETAILS,
        errorMessage
    };
}

export function requestCallbackDetails(urls: string[], userAlias: string): IRequestCallbackDetailsAction {
    return {
        type: DetailsActionType.REQUEST_CALLBACK_DETAILS,
        urls,
        userAlias
    };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function receiveCallbackDetails(callbackJSONs: any): IReceiveCallbackDetailsAction {
    return {
        type: DetailsActionType.RECEIVE_CALLBACK_DETAILS,
        callbackJSONs
    };
}

export function failedCallbackDetails(errorMessage: string): IFailedCallbackDetailsAction {
    return {
        type: DetailsActionType.FAILED_CALLBACK_DETAILS,
        errorMessage
    };
}

export function receiveDocumentPreview(filePreview: string): IReceiveDocumentPreview {
    return {
        type: DetailsActionType.RECEIVE_DOCUMENT_PREVIEW,
        filePreview
    };
}

export function clearDocumentPreview(): IClearDocumentPreview {
    return {
        type: DetailsActionType.CLEAR_DOCUMENT_PREVIEW
    };
}

export function closeDocumentPreview(): ICloseDocumentPreview {
    return {
        type: DetailsActionType.CLOSE_DOCUMENT_PREVIEW
    };
}

export function updateMyRequest(
    tenantId: number,
    documentNumber: string,
    displayDocumentNumber: string,
    fiscalYear?: string,
    summaryJSON?: object
): IUpdateDetailsRequest {
    return {
        type: DetailsActionType.UPDATE_REQUEST,
        tenantId,
        documentNumber,
        displayDocumentNumber,
        fiscalYear,
        summaryJSON
    };
}

export function requestDocument(
    url: string,
    actionId: string,
    attachmentName: string,
    userAlias: string,
    tenantId?: string,
    documentNumber?: string,
    displayDocumentNumber?: string,
    attachmentId?: string
): IRequestDocumentAction {
    return {
        type: DetailsActionType.REQUEST_DOCUMENT,
        url,
        actionId,
        attachmentName,
        userAlias,
        tenantId,
        documentNumber,
        displayDocumentNumber,
        attachmentId
    };
}

export function requestDocumentStart(docName: string): IRequestDocumentStart {
    return { type: DetailsActionType.REQUEST_DOCUMENT_START, docName };
}

export function requestDocumentEnd(): IRequestDocumentEnd {
    return { type: DetailsActionType.REQUEST_DOCUMENT_END };
}

export function requestDocumentPreview(
    tenantId: string,
    documentNumber: string,
    displayDocumentNumber: string,
    attachmentId: string,
    userAlias: string,
    isModal: boolean
): IRequestDocumentPreviewAction {
    return {
        type: DetailsActionType.REQUEST_DOCUMENT_PREVIEW,
        tenantId,
        documentNumber,
        displayDocumentNumber,
        attachmentId,
        userAlias,
        isModal
    };
}

export function requestAllDocuments(
    tenantId: string,
    documentNumber: string,
    displayDocumentNumber: string,
    attachmentArray: any,
    userAlias: string
): IRequestAllDocumentsAction {
    return {
        type: DetailsActionType.REQUEST_ALL_DOCUMENTS,
        tenantId,
        documentNumber,
        displayDocumentNumber,
        attachmentArray,
        userAlias
    };
}

export function postAction(
    submission: object,
    userAlias: string,
    isBulkAction?: boolean,
    isPullModelEnabled?: boolean
): IPostAction {
    const {
        documentNumber,
        displayDocumentNumber,
        fiscalYear,
        tenantId,
        code,
        reasonCode,
        reasonText,
        comments,
        documentTypeId,
        businessProcessName,
        receiptsCheck,
        corruptionCheck,
        sequenceID,
        nextApprover,
        peoplePickerSelections,
        additionalActionDetails,
        digitalSignature
    } = submission as any;
    return {
        type: DetailsActionType.POST_ACTION,
        documentNumber,
        displayDocumentNumber,
        fiscalYear,
        tenantId,
        code,
        reasonCode,
        reasonText,
        comments,
        documentTypeId,
        businessProcessName,
        receiptsCheck,
        corruptionCheck,
        sequenceID,
        nextApprover,
        userAlias,
        peoplePickerSelections,
        digitalSignature,
        additionalActionDetails,
        isBulkAction,
        isPullModelEnabled
    };
}

export function receiveActionResponse( actionResponse: any, showSuccess: boolean): IReceiveActionResponse {
    return {
        type: DetailsActionType.RECEIVE_ACTION_RESPONSE,
        showSuccess,
        actionResponse
    };
}
export function failedPostAction(
    postActionErrorMessage: string,
    displayDocumentNumber: string,
    isPullModelEnabled: boolean
): IFailedPostAction {
    return {
        type: DetailsActionType.FAILED_POST_ACTION,
        postActionErrorMessage,
        displayDocumentNumber,
        isPullModelEnabled
    };
}

export function reinitializeDetails(): IReinitializeDetailsAction {
    return {
        type: DetailsActionType.REINITIALIZE_DETAILS
    };
}

export function requestFullyScrolled(toggle: boolean, shouldDetailReRender: boolean): IRequestFullyScrolledAction {
    return {
        type: DetailsActionType.REQUEST_FULLY_SCROLLED,
        toggle,
        shouldDetailReRender
    };
}

export function markRequestAsRead(
    tenantId: string,
    displayDocumentNumber: string,
    userAlias: string
): IMarkRequestAsReadAction {
    return {
        type: DetailsActionType.MARK_REQUEST_AS_READ,
        tenantId,
        displayDocumentNumber,
        userAlias
    };
}

export function failedDocumentDownload(documentDownloadErrorMessage: string): IFailedDocumentDownloadAction {
    return {
        type: DetailsActionType.FAILED_DOCUMENT_DOWNLOAD,
        documentDownloadErrorMessage
    };
}

export function failedDocumentPreview(documentPreviewErrorMessage: string): IFailedDocumentPreviewAction {
    return {
        type: DetailsActionType.FAILED_DOCUMENT_PREVIEW,
        documentPreviewErrorMessage
    };
}

export function failedAllDocumentsDownload(allDocumentsDownloadErrorMessage: string): IFailedAllDocumentsAction {
    return {
        type: DetailsActionType.FAILED_ALL_DOCUMENTS,
        allDocumentsDownloadErrorMessage
    };
}

export function failedHeader(errorMessage: string): IFailedHeaderAction {
    return {
        type: DetailsActionType.FAILED_HEADER,
        errorMessage
    };
}
export function openMicrofrontend(): IOpenMicrofrontendAction {
    return {
        type: DetailsActionType.OPEN_MICROFRONTEND
    };
}

export function closeMicrofrontend(): ICloseMicrofrontendAction {
    return {
        type: DetailsActionType.CLOSE_MICROFRONTEND
    };
}

export function updateAdditionalData(newAdditionalData: any): IUpdateAdditionalDataAction {
    return {
        type: DetailsActionType.UPDATE_ADDITIONAL_DATA,
        newAdditionalData: newAdditionalData
    };
}

export function requestFullyRendered(isRequestFullyRendered: boolean): IRequestFullyRenderedAction {
    return {
        type: DetailsActionType.REQUEST_FULLY_RENDERED,
        isRequestFullyRendered: isRequestFullyRendered
    };
}

export function receiveAreDetailsEditable(areDetailsEditable: boolean): IReceiveAreDetailsEditable {
    return {
        type: DetailsActionType.RECEIVE_ARE_DETAILS_EDITABLE,
        areDetailsEditable: areDetailsEditable
    };
}

export function postEditableDetails(tenantId: string, userAlias: string): IPostEditableDetailsAction {
    return {
        type: DetailsActionType.POST_EDITABLE_DETAILS,
        tenantId: tenantId,
        userAlias: userAlias
    };
}

export function failedEditDetails(errorMessage: string): IFailedEditDetailsAction {
    return {
        type: DetailsActionType.FAILED_EDIT_DETAILS,
        errorMessage
    };
}
