import { IFileUpload } from "./FileUpload/FileUpload";

export enum DetailsActionType {
    REQUEST_USER_IMAGE = 'REQUEST_USER_IMAGE',
    RECEIVE_USER_IMAGE = 'RECEIVE_USER_IMAGE',
    FAILED_USER_IMAGE = 'FAILED_USER_IMAGE',
    RECEIVE_HEADER = 'RECEIVE_HEADER',
    REQUEST_HEADER = 'REQUEST_HEADER',
    REQUEST_MY_DETAILS = 'REQUEST_MY_DETAILS',
    RECEIVE_MY_DETAILS = 'RECEIVE_MY_DETAILS',
    FAILED_DETAILS = 'FAILED_DETAILS',
    REQUEST_CALLBACK_DETAILS = 'REQUEST_CALLBACK_DETAILS',
    RECEIVE_CALLBACK_DETAILS = 'RECEIVE_CALLBACK_DETAILS',
    FAILED_CALLBACK_DETAILS = 'FAILED_CALLBACK_DETAILS',
    REQUEST_CALLBACK_HEADER_DETAILS = 'REQUEST_CALLBACK_HEADER_DETAILS',
    RECEIVE_CALLBACK_HEADER_DETAILS = 'RECEIVE_CALLBACK_HEADER_DETAILS',
    FAILED_CALLBACK_HEADER_DETAILS = 'FAILED_CALLBACK_HEADER_DETAILS',
    REQUEST_DOCUMENT_START = 'REQUEST_DOCUMENT_START',
    REQUEST_DOCUMENT_END = 'REQUEST_DOCUMENT_END',
    REQUEST_DOCUMENT = 'REQUEST_DOCUMENT',
    REQUEST_DOCUMENT_PREVIEW = 'REQUEST_DOCUMENT_PREVIEW',
    REQUEST_ALL_DOCUMENTS = 'REQUEST_ALL_DOCUMENTS',
    RECEIVE_DOCUMENT_PREVIEW = 'RECEIVE_DOCUMENT_PREVIEW',
    CLEAR_DOCUMENT_PREVIEW = 'CLEAR_DOCUMENT_PREVIEW',
    CLOSE_DOCUMENT_PREVIEW = 'CLOSE_DOCUMENT_PREVIEW',
    UPDATE_REQUEST = 'UPDATE_REQUEST',
    POST_ACTION = 'POST_ACTION',
    FAILED_POST_ACTION = 'FAILED_POST_ACTION',
    RECEIVE_ACTION_RESPONSE = 'RECEIVE_ACTION_RESPONSE',
    REINITIALIZE_DETAILS = 'REINITIALIZE_DETAILS',
    REQUEST_FULLY_SCROLLED = 'REQUEST_FULLY_SCROLLED',
    MARK_REQUEST_AS_READ = 'MARK_REQUEST_AS_READ',
    FAILED_DOCUMENT_DOWNLOAD = 'FAILED_DOCUMENT_DOWNLOAD',
    FAILED_DOCUMENT_PREVIEW = 'FAILED_DOCUMENT_PREVIEW',
    FAILED_ALL_DOCUMENTS = 'FAILED_ALL_DOCUMENTS',
    FAILED_HEADER = 'FAILED_HEADER',
    OPEN_MICROFRONTEND = 'OPEN_MICROFRONTEND',
    CLOSE_MICROFRONTEND = 'CLOSE_MICROFRONTEND',
    UPDATE_ADDITIONAL_DATA = 'UPDATE_ADDITIONAL_DATA',
    TOGGLE_HISTORY_DETAIL_PANEL = 'TOGGLE_HISTORY_DETAIL_PANEL',
    SET_FOOTER_HEIGHT = 'SET_FOOTER_HEIGHT',
    REQUEST_FULLY_RENDERED = 'REQUEST_FULLY_RENDERED',
    RECEIVE_ARE_DETAILS_EDITABLE = 'RECEIVE_ARE_DETAILS_EDITABLE',
    POST_EDITABLE_DETAILS = 'POST_EDITABLE_DETAILS',
    FAILED_EDIT_DETAILS = 'FAILED_EDIT_DETAILS',
    SET_BULK_FOOTER_HEIGHT = 'SET_BULK_FOOTER_HEIGHT',
    SET_BULK_MESSAGEBAR_HEIGHT = 'SET_BULK_MESSAGEBAR_HEIGHT',
    SET_ALIAS_MESSAGEBAR_HEIGHT = 'SET_ALIAS_MESSAGEBAR_HEIGHT',
    CONCAT_FAILED_REQUESTS = 'CONCAT_FAILED_REQUESTS',
    RECEIVE_APPROVAL_RESPONSE_DETAILS = 'RECEIVE_APPROVAL_RESPONSE_DETAILS',
    OPEN_FILE_UPLOAD = 'OPEN_FILE_UPLOAD',
    CLOSE_FILE_UPLOAD = 'CLOSE_FILE_UPLOAD',
    UPLOAD_FILE = 'UPLOAD_FILE',
    FAILED_UPLOAD_FILE = 'FAILED_UPLOAD_FILE',
    SUCCESS_UPLOAD_FILE = 'SUCCESS_UPLOAD_FILE',
}

export type DetailsAction =
    | IRequestUserImageAction
    | IReceiveUserImageAction
    | IFailedUserImageAction
    | IRequestHeaderAction
    | IReceiveHeaderAction
    | IRequestDetailsAction
    | IReceiveDetailsAction
    | IFailedDetailsAction
    | IUpdateDetailsRequest
    | IRequestCallbackDetailsAction
    | IReceiveCallbackDetailsAction
    | IFailedCallbackDetailsAction
    | IPostAction
    | IReceiveActionResponse
    | IReceiveDocumentPreview
    | IClearDocumentPreview
    | ICloseDocumentPreview
    | IRequestDocumentAction
    | IRequestDocumentPreviewAction
    | IFailedPostAction
    | IReinitializeDetailsAction
    | IRequestFullyScrolledAction
    | IMarkRequestAsReadAction
    | IFailedDocumentDownloadAction
    | IFailedDocumentPreviewAction
    | IFailedAllDocumentsAction
    | IFailedHeaderAction
    | IOpenMicrofrontendAction
    | ICloseMicrofrontendAction
    | IUpdateAdditionalDataAction
    | IToggleHistoryDetailPanel
    | ISetFooterHeight
    | IRequestFullyRenderedAction
    | IReceiveAreDetailsEditable
    | IPostEditableDetailsAction
    | IFailedEditDetailsAction
    | ISetBulkFooterHeight
    | ISetBulkMessagebarHeight
    | ISetAliasMessagebarHeight
    | IRequestFullyRenderedAction
    | IConcatFailedRequests
    | IRequestDocumentStart
    | IRequestAllDocumentsAction
    | IRequestDocumentEnd
    | IOpenFileUploadAction
    | ICloseFileUploadAction
    | IUploadFileAction
    | IFailedUploadFilesAction
    | ISuccessUploadFilesAction;

export interface ISetFooterHeight {
    type: DetailsActionType.SET_FOOTER_HEIGHT;
    height: number;
}

export interface ISetBulkFooterHeight {
    type: DetailsActionType.SET_BULK_FOOTER_HEIGHT;
    height: number;
}

export interface ISetBulkMessagebarHeight {
    type: DetailsActionType.SET_BULK_MESSAGEBAR_HEIGHT;
    height: number;
}

export interface ISetAliasMessagebarHeight {
    type: DetailsActionType.SET_ALIAS_MESSAGEBAR_HEIGHT;
    height: number;
}

export interface IToggleHistoryDetailPanel {
    type: DetailsActionType.TOGGLE_HISTORY_DETAIL_PANEL;
}
export interface IRequestUserImageAction {
    type: DetailsActionType.REQUEST_USER_IMAGE;
    userAlias: string;
}

export interface IReceiveUserImageAction {
    type: DetailsActionType.RECEIVE_USER_IMAGE;
    userImage: any;
}

export interface IFailedUserImageAction {
    type: DetailsActionType.FAILED_USER_IMAGE;
    userImageErrorMessage: string;
}

export interface IRequestHeaderAction {
    type: DetailsActionType.REQUEST_HEADER;
    tenantId: string;
    documentNumber: string;
    userAlias: string;
    isPullModelEnabled: boolean;
    summaryJSON?: object;
    summaryDataMapping?: string;
}
export interface IReceiveHeaderAction {
    type: DetailsActionType.RECEIVE_HEADER;
    details: any;
    template: any;
    summaryObj?: any;
}

export interface IRequestDetailsAction {
    type: DetailsActionType.REQUEST_MY_DETAILS;
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    userAlias: string;
    requiresTemplate: boolean;
    isPullModelEnabled: boolean;
}

export interface IReceiveDetailsAction {
    type: DetailsActionType.RECEIVE_MY_DETAILS;
    details: any;
    template?: any;
}

export interface IFailedDetailsAction {
    type: DetailsActionType.FAILED_DETAILS;
    errorMessage: string;
}

export interface IRequestCallbackDetailsAction {
    type: DetailsActionType.REQUEST_CALLBACK_DETAILS;
    urls: string[];
    userAlias: string;
}

export interface IReceiveCallbackDetailsAction {
    type: DetailsActionType.RECEIVE_CALLBACK_DETAILS;
    callbackJSONs: any;
}

export interface IFailedCallbackDetailsAction {
    type: DetailsActionType.FAILED_CALLBACK_DETAILS;
    errorMessage: string;
}

export interface IRequestDocumentAction {
    type: DetailsActionType.REQUEST_DOCUMENT;
    actionId: string;
    url: string;
    attachmentName: string;
    userAlias: string;
    tenantId?: string;
    documentNumber?: string;
    displayDocumentNumber?: string;
    attachmentId?: string;
}

export interface IRequestDocumentPreviewAction {
    type: DetailsActionType.REQUEST_DOCUMENT_PREVIEW;
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    attachmentId: string;
    isPreAttached: boolean;
    userAlias: string;
    isModal: boolean;
}

export interface IOpenFileUploadAction {
    type: DetailsActionType.OPEN_FILE_UPLOAD;
    isModal: boolean;
}

export interface ICloseFileUploadAction {
    type: DetailsActionType.CLOSE_FILE_UPLOAD;
}

export interface IUploadFileAction {
    type: DetailsActionType.UPLOAD_FILE;
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    userAlias: string;
    requiresTemplate: boolean;
    isPullModelEnabled: boolean;
    files: IFileUpload[];
}

export interface IFailedUploadFilesAction {
    type: DetailsActionType.FAILED_UPLOAD_FILE;
    uploadFilesErrorMessage: string;
    uploadFilesErrorMessageList: string[];
}

export interface ISuccessUploadFilesAction {
    type: DetailsActionType.SUCCESS_UPLOAD_FILE;
}

export interface IRequestAllDocumentsAction {
    type: DetailsActionType.REQUEST_ALL_DOCUMENTS;
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    attachmentArray: any;
    userAlias: string;
}

export interface IReceiveDocumentPreview {
    type: DetailsActionType.RECEIVE_DOCUMENT_PREVIEW;
    filePreview: string;
}

export interface IClearDocumentPreview {
    type: DetailsActionType.CLEAR_DOCUMENT_PREVIEW;
}

export interface ICloseDocumentPreview {
    type: DetailsActionType.CLOSE_DOCUMENT_PREVIEW;
}

export interface IUpdateDetailsRequest {
    type: DetailsActionType.UPDATE_REQUEST;
    tenantId: number;
    documentNumber: string;
    displayDocumentNumber: string;
    fiscalYear: string;
    summaryJSON?: object;
}

export interface IPostAction {
    type: DetailsActionType.POST_ACTION;
    documentNumber: string;
    displayDocumentNumber: string;
    fiscalYear: string;
    tenantId: string;
    code: string;
    reasonCode: string;
    reasonText: string;
    comments: string;
    documentTypeId: string;
    businessProcessName: string;
    receiptsCheck: boolean;
    corruptionCheck: boolean;
    sequenceID: string;
    nextApprover: string;
    userAlias: string;
    peoplePickerSelections: any;
    additionalActionDetails: object | null;
    digitalSignature: string;
    isBulkAction?: boolean;
    isPullModelEnabled?: boolean;
}

export interface IFailedPostAction {
    type: DetailsActionType.FAILED_POST_ACTION;
    postActionErrorMessage: string;
    displayDocumentNumber: string;
    isPullModelEnabled: boolean;
}

export interface IReceiveActionResponse {
    type: DetailsActionType.RECEIVE_ACTION_RESPONSE;
    showSuccess: boolean;
    actionResponse: any;
}

export interface IReinitializeDetailsAction {
    type: DetailsActionType.REINITIALIZE_DETAILS;
}

export interface IRequestFullyScrolledAction {
    type: DetailsActionType.REQUEST_FULLY_SCROLLED;
    toggle: boolean;
    shouldDetailReRender: boolean;
}

export interface IMarkRequestAsReadAction {
    type: DetailsActionType.MARK_REQUEST_AS_READ;
    tenantId: string;
    displayDocumentNumber: string;
    userAlias: string;
}

export interface IFailedDocumentDownloadAction {
    type: DetailsActionType.FAILED_DOCUMENT_DOWNLOAD;
    documentDownloadErrorMessage: string;
}

export interface IFailedDocumentPreviewAction {
    type: DetailsActionType.FAILED_DOCUMENT_PREVIEW;
    documentPreviewErrorMessage: string;
}

export interface IFailedAllDocumentsAction {
    type: DetailsActionType.FAILED_ALL_DOCUMENTS;
    allDocumentsDownloadErrorMessage: string;
}

export interface IFailedHeaderAction {
    type: DetailsActionType.FAILED_HEADER;
    errorMessage: string;
}
export interface IOpenMicrofrontendAction {
    type: DetailsActionType.OPEN_MICROFRONTEND;
}

export interface ICloseMicrofrontendAction {
    type: DetailsActionType.CLOSE_MICROFRONTEND;
}

export interface IUpdateAdditionalDataAction {
    type: DetailsActionType.UPDATE_ADDITIONAL_DATA;
    newAdditionalData: any;
}

export interface IRequestFullyRenderedAction {
    type: DetailsActionType.REQUEST_FULLY_RENDERED;
    isRequestFullyRendered: boolean;
}

export interface IReceiveAreDetailsEditable {
    type: DetailsActionType.RECEIVE_ARE_DETAILS_EDITABLE;
    areDetailsEditable: boolean;
}

export interface IPostEditableDetailsAction {
    type: DetailsActionType.POST_EDITABLE_DETAILS;
    tenantId: string;
    userAlias: string;
}

export interface IFailedEditDetailsAction {
    type: DetailsActionType.FAILED_EDIT_DETAILS;
    errorMessage: string;
}

export interface IConcatFailedRequests {
    type: DetailsActionType.CONCAT_FAILED_REQUESTS;
}

export interface IRequestDocumentStart {
    type: DetailsActionType.REQUEST_DOCUMENT_START;
    docName: string;
}

export interface IRequestDocumentEnd {
    type: DetailsActionType.REQUEST_DOCUMENT_END;
}
