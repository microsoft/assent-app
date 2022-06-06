import { DetailsType, IDetailsState } from './Details.types';
import { DetailsAction, DetailsActionType } from './Details.action-types';
import { guid } from '../../../Helpers/Guid';

export const detailsReducerName = 'DetailsReducer';
export const detailsInitialState: IDetailsState = {
    isLoadingHeader: false,
    headerHasError: false,
    isLoadingDetails: false,
    isLoadingCallback: false,
    isLoadingCallbackHeader: false,
    detailsHasError: false,
    callbackHasError: false,
    headerErrorMessage: null,
    detailsErrorMessage: null,
    callbackErrorMessage: null,
    actionMessage: null,
    tenantId: null,
    documentNumber: null,
    displayDocumentNumber: null,
    fiscalYear: null,
    isProcessingAction: false,
    actionDetails: null,
    documentPreview: null,
    isPreviewOpen: false,
    postActionHasError: false,
    postActionErrorMessage: null,
    documentTypeId: null,
    businessProcessName: null,
    isLoadingPreview: false,
    headerDetailsJSON: null,
    headerTemplateJSON: null,
    detailsJSON: null,
    detailsTemplateJSON: null,
    callbackHeaderJSONs: null,
    callbackJSONs: null,
    showingDetails: false,
    selectedPage: null,
    isRequestFullyScrolled: false,
    shouldDetailReRender: true,
    tcv: null,
    stateCommonProperties: null,
    readRequests: [],
    failedRequests: [],
    toggleHistoryDetailPanel: false,
    isControlsAndComplianceRequired: false,
    documentDownloadErrorMessage: null,
    documentDownloadHasError: false,
    isLoadingSummary: false,
    userName: '',
    userAlias: '',
    isLoadingUserImage: false,
    userImage: null,
    userImageHasError: false,
    userImageErrorMessage: null,
    documentPreviewHasError: false,
    documentPreviewErrorMessage: null,
    documentDownload: false,
    docName: null,
    allDocumentsDownloadHasError: false,
    allDocumentsDownloadErrorMessage: null,
    isMicrofrontendOpen: false,
    additionalData: null,
    fetchedUserAlias: '',
    panelWidth: 600,
    footerHeight: 64,
    isRequestFullyRendered: false,
    detailsComponentType: DetailsType.AdaptiveCard,
    areDetailsEditable: false,
    cdnURL: null,
    editDetailsErrorMessage: null,
    bulkFooterHeight: 0,
    bulkMessagebarHeight: 0,
    aliasMessagebarHeight: 0,
    isPullModelEnabled: false,
    summaryJSON: null,
    summaryDataMapping: null,
    isModalPreviewOpen: false,
    isShowingSuccessStatus: false
};

export function detailsReducer(prev: IDetailsState = detailsInitialState, action: DetailsAction): IDetailsState {
    switch (action.type) {
        case DetailsActionType.REQUEST_DOCUMENT_START:
            return {
                ...prev,
                docName: action.docName,
                documentDownload: true,
                documentDownloadHasError: false,
                documentDownloadErrorMessage: null,
                allDocumentsDownloadHasError: false,
                allDocumentsDownloadErrorMessage: null
            };
        case DetailsActionType.REQUEST_DOCUMENT:
            return {
                ...prev
            };
        case DetailsActionType.REQUEST_ALL_DOCUMENTS:
            return {
                ...prev
            };
        case DetailsActionType.REQUEST_DOCUMENT_END:
            return {
                ...prev,
                documentDownload: false
            };
        case DetailsActionType.SET_FOOTER_HEIGHT:
            return {
                ...prev,
                footerHeight: action.height
            };
        case DetailsActionType.SET_BULK_FOOTER_HEIGHT:
            return {
                ...prev,
                bulkFooterHeight: action.height
            };
        case DetailsActionType.SET_BULK_MESSAGEBAR_HEIGHT:
            return {
                ...prev,
                bulkMessagebarHeight: action.height
            };
        case DetailsActionType.SET_ALIAS_MESSAGEBAR_HEIGHT:
            return {
                ...prev,
                aliasMessagebarHeight: action.height
            };

        case DetailsActionType.TOGGLE_HISTORY_DETAIL_PANEL:
            return {
                ...prev,
                toggleHistoryDetailPanel: !prev.toggleHistoryDetailPanel
            };
        case DetailsActionType.REQUEST_USER_IMAGE:
            return {
                ...prev,
                isLoadingUserImage: true,
                userImageHasError: false,
                fetchedUserAlias: action.userAlias
            };
        case DetailsActionType.RECEIVE_USER_IMAGE:
            return {
                ...prev,
                isLoadingUserImage: false,
                userImageHasError: false,
                userImage: action.userImage
            };
        case DetailsActionType.FAILED_USER_IMAGE:
            return {
                ...prev,
                isLoadingUserImage: false,
                userImageHasError: true,
                userImageErrorMessage: action.userImageErrorMessage
            };
        case DetailsActionType.REQUEST_HEADER:
            return {
                ...prev,
                isLoadingHeader: true,
                headerHasError: false,
                isProcessingAction: false,
                headerDetailsJSON: null,
                headerTemplateJSON: null,
                isPreviewOpen: false,
                isRequestFullyScrolled: false,
                isRequestFullyRendered: false,
                shouldDetailReRender: true,
                detailsHasError: false,
                detailsErrorMessage: null,
                callbackErrorMessage: null
            };
        case DetailsActionType.RECEIVE_HEADER:
            return {
                ...prev,
                isLoadingHeader: false,
                headerHasError: false,
                headerDetailsJSON: action.details,
                headerTemplateJSON: action.template,
                isPreviewOpen: false,
                shouldDetailReRender: true,
                summaryJSON: action.summaryObj ?? prev.summaryJSON
            };
        case DetailsActionType.FAILED_HEADER:
            return {
                ...prev,
                isLoadingHeader: false,
                headerHasError: true,
                headerErrorMessage: action.errorMessage
            };
        case DetailsActionType.REQUEST_MY_DETAILS:
            return {
                ...prev,
                isLoadingDetails: true,
                detailsHasError: false,
                detailsErrorMessage: null,
                callbackHasError: false,
                callbackErrorMessage: null,
                isProcessingAction: false,
                detailsJSON: null,
                detailsTemplateJSON: null,
                callbackJSONs: null,
                isRequestFullyScrolled: false,
                isRequestFullyRendered: false,
                shouldDetailReRender: true
            };
        case DetailsActionType.RECEIVE_MY_DETAILS:
            return {
                ...prev,
                isLoadingDetails: false,
                detailsHasError: false,
                detailsJSON: action.details,
                detailsTemplateJSON: action.template,
                showingDetails: true,
                shouldDetailReRender: true
            };
        case DetailsActionType.FAILED_DETAILS:
            return {
                ...prev,
                isLoadingDetails: false,
                detailsHasError: true,
                detailsErrorMessage: action.errorMessage
            };
        case DetailsActionType.REQUEST_CALLBACK_DETAILS:
            return {
                ...prev,
                isLoadingCallback: true,
                callbackHasError: false,
                isProcessingAction: false,
                callbackJSONs: null
            };
        case DetailsActionType.RECEIVE_CALLBACK_DETAILS:
            return {
                ...prev,
                isLoadingCallback: false,
                callbackHasError: false,
                callbackJSONs: action.callbackJSONs
            };
        case DetailsActionType.FAILED_CALLBACK_DETAILS:
            return {
                ...prev,
                isLoadingCallback: false,
                callbackHasError: true,
                callbackErrorMessage: action.errorMessage
            };
        case DetailsActionType.UPDATE_REQUEST:
            return {
                ...detailsInitialState,
                readRequests: prev.readRequests,
                failedRequests: prev.failedRequests,
                tenantId: action.tenantId.toString(),
                documentNumber: action.documentNumber,
                displayDocumentNumber: action.displayDocumentNumber,
                summaryJSON: action.summaryJSON ?? null,
                fiscalYear: action.fiscalYear,
                isRequestFullyScrolled: false,
                shouldDetailReRender: true,
                tcv: guid()
            };
        case DetailsActionType.POST_ACTION:
            return {
                ...prev,
                isProcessingAction: !action.isBulkAction,
                postActionHasError: false,
                postActionErrorMessage: null
            };
        case DetailsActionType.FAILED_POST_ACTION:
            return {
                ...prev,
                isProcessingAction: false,
                postActionHasError: true,
                postActionErrorMessage: action.postActionErrorMessage,
                failedRequests: action.isPullModelEnabled
                    ? prev.failedRequests
                    : prev.failedRequests.concat([action.displayDocumentNumber])
            };
        case DetailsActionType.REQUEST_DOCUMENT_PREVIEW:
            return {
                ...prev,
                isLoadingPreview: true,
                isPreviewOpen: !action.isModal,
                isModalPreviewOpen: action.isModal,
                documentPreviewHasError: false,
                documentDownloadHasError: false,
                documentDownloadErrorMessage: null,
                allDocumentsDownloadHasError: false,
                allDocumentsDownloadErrorMessage: null
            };
        case DetailsActionType.RECEIVE_ACTION_RESPONSE:
            return {
                ...prev,
                isProcessingAction: false,
                actionMessage: action.actionResponse,
                isShowingSuccessStatus: action.showSuccess
            };
        case DetailsActionType.RECEIVE_DOCUMENT_PREVIEW:
            return {
                ...prev,
                documentPreview: action.filePreview,
                isLoadingPreview: false
            };
        case DetailsActionType.CLEAR_DOCUMENT_PREVIEW:
            return {
                ...prev,
                documentPreview: null
            };

        case DetailsActionType.CLOSE_DOCUMENT_PREVIEW:
            return {
                ...prev,
                isPreviewOpen: false,
                isModalPreviewOpen: false
            };
        case DetailsActionType.REINITIALIZE_DETAILS:
            return {
                ...detailsInitialState,
                readRequests: prev.readRequests,
                failedRequests: prev.failedRequests,
                bulkMessagebarHeight: prev.bulkMessagebarHeight
            };
        case DetailsActionType.REQUEST_FULLY_SCROLLED:
            return {
                ...prev,
                isRequestFullyScrolled: action.toggle,
                shouldDetailReRender: action.shouldDetailReRender
            };
        case DetailsActionType.MARK_REQUEST_AS_READ:
            return {
                ...prev,
                readRequests: prev.readRequests.concat([action.displayDocumentNumber])
            };
        case DetailsActionType.FAILED_DOCUMENT_DOWNLOAD:
            return {
                ...prev,
                documentDownload: false,
                documentDownloadHasError: true,
                documentDownloadErrorMessage: action.documentDownloadErrorMessage
            };
        case DetailsActionType.FAILED_DOCUMENT_PREVIEW:
            return {
                ...prev,
                isLoadingPreview: false,
                documentPreviewHasError: true,
                documentPreviewErrorMessage: action.documentPreviewErrorMessage
            };
        case DetailsActionType.FAILED_ALL_DOCUMENTS:
            return {
                ...prev,
                documentDownload: false,
                allDocumentsDownloadHasError: true,
                allDocumentsDownloadErrorMessage: action.allDocumentsDownloadErrorMessage
            };
        case DetailsActionType.OPEN_MICROFRONTEND:
            return {
                ...prev,
                isMicrofrontendOpen: true
            };
        case DetailsActionType.CLOSE_MICROFRONTEND:
            return {
                ...prev,
                isMicrofrontendOpen: false
            };
        case DetailsActionType.UPDATE_ADDITIONAL_DATA:
            return {
                ...prev,
                additionalData: action.newAdditionalData
            };
        case DetailsActionType.REQUEST_FULLY_RENDERED:
            return {
                ...prev,
                isRequestFullyRendered: action.isRequestFullyRendered
            };
        case DetailsActionType.RECEIVE_ARE_DETAILS_EDITABLE:
            return {
                ...prev,
                areDetailsEditable: action.areDetailsEditable
            };
        case DetailsActionType.FAILED_EDIT_DETAILS:
            return {
                ...prev,
                editDetailsErrorMessage: action.errorMessage
            };
        default:
            return prev;
    }
}
