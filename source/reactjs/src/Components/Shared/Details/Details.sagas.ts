import { SimpleEffect, Effect, getContext, put, all, call, takeLatest, select } from 'redux-saga/effects';
import { IAuthClient } from '@micro-frontend-react/employee-experience/lib/IAuthClient';
import { ITelemetryClient } from '@micro-frontend-react/employee-experience/lib/ITelemetryClient';
import { guid } from '../../../Helpers/Guid';
import {
    DetailsActionType,
    IPostAction,
    IRequestUserImageAction,
    IRequestDetailsAction,
    IRequestDocumentAction,
    IRequestDocumentPreviewAction,
    IRequestAllDocumentsAction,
    IRequestCallbackDetailsAction,
    IMarkRequestAsReadAction,
    IRequestHeaderAction,
    IPostEditableDetailsAction,
    IUploadFileAction,
} from './Details.action-types';
import {
    receiveUserImage,
    failedUserImage,
    receiveMyDetails,
    receiveActionResponse,
    failedDetails,
    failedPostAction,
    receiveDocumentPreview,
    receiveCallbackDetails,
    failedCallbackDetails,
    receiveHeader,
    failedHeader,
    markRequestAsRead,
    failedDocumentDownload,
    failedDocumentPreview,
    failedAllDocumentsDownload,
    receiveAreDetailsEditable,
    failedEditDetails,
    requestDocumentEnd,
    failedFileUpload,
    successFileUpload,
    requestMyDetails,
} from './Details.actions';
import {
    updateBulkStatus,
    updateBulkFailedStatus,
    updateBulkFailedValue,
    updateApprovalRecords,
    updatePanelState,
    updateIsProcessingBulkApproval,
    requestMySummary,
    updateRetainBulkSelection,
    requestPullTenantSummary,
    updateFailedPullTenantRequests,
    updateSuccessfulPullTenantRequests,
} from '../SharedComponents.actions';

import {
    getStateCommonTelemetryProperties,
    getTenantBusinessProcessName,
    getTenantDataModelMapping,
    getTenantDocumentTypeId,
    getTenantInfo,
    getSelectedTenantDelegation,
    getSelectedPage,
} from '../SharedComponents.selectors';
import { trackBusinessProcessEvent, trackException, TrackingEventId } from '../../../Helpers/telemetryHelpers';
import { setHeader } from '../Components/SagasHelper';
import {
    IHttpClient,
    IHttpClientResult,
    IHttpClientRequest,
} from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import {
    convertKeysToLowercase,
    flattenObj,
    formatBusinessProcessName,
    generatePullModelSummary,
    generatePullTenantAdditionalData,
    generateSummaryObjForPullTenant,
    safeJSONParse,
} from '../../../Helpers/sharedHelpers';
import { getDisplayDocumentNumber, getSummaryJSON, getTcv, getIsProcessingAction } from './Details.selectors';
import { IDelegationObj } from '../SharedComponents.types';
import { IFileUploadResponse } from './Details.types';

function* fetchUserImage(action: IRequestUserImageAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: userImage }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__GRAPH_BASE_URL__}users/${action.userAlias}${__UPN_SUFFIX__}/photos/96x96/$value`,
            resource: __GRAPH_RESOURCE_URL__,
            responseType: 'arraybuffer',
        });
        // convert to base64
        let charConversion = '';
        const byteArrayCopy = new Uint8Array(userImage);
        for (let i = 0; i < byteArrayCopy.byteLength; i++) {
            charConversion += String.fromCharCode(byteArrayCopy[i]);
        }
        const base64Version = btoa(charConversion);

        yield put(receiveUserImage(base64Version));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load User Image - Success',
            'MSApprovals.LoadUserImage.Success',
            TrackingEventId.UserImageLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedUserImage(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load User Image - Failure',
            'MSApprovals.LoadUserImage.Failure',
            TrackingEventId.UserImageLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchHeader(action: IRequestHeaderAction): IterableIterator<Effect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const selectedPage = yield select(getSelectedPage);
    const tcv = yield select(getTcv);
    let updatedSummaryJSON = null;
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const headerTemplateRequest = {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/adaptivedetail/${action.tenantId}?TemplateType=Summary`,
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias, tcv, action.documentNumber),
        };
        let headerDetailsJSON;
        let headerTemplateJSON;
        if (!action.isPullModelEnabled) {
            const [headerTemplateResponse, headerDetailsResponse]: any = yield all([
                call([httpClient, httpClient.request], headerTemplateRequest),
                call([httpClient, httpClient.request], {
                    url: `${__API_BASE_URL__}${__API_URL_ROOT__}/detail/${action.tenantId}/${action.documentNumber}?CallType=Summary`,
                    resource: __RESOURCE_URL__,
                    headers: setHeader(action.userAlias),
                }),
            ]);
            headerDetailsJSON = headerDetailsResponse?.data;
            headerTemplateJSON = headerTemplateResponse?.data;
        } else {
            const headerTemplateResponse: IHttpClientResult<object> = yield call(
                [httpClient, httpClient.request],
                headerTemplateRequest
            );
            headerTemplateJSON = headerTemplateResponse?.data;
            let summaryObj = action.summaryJSON;
            if (selectedPage === 'history') {
                const lowerCaseObj = convertKeysToLowercase(action.summaryJSON);
                const filterObjectString = action?.summaryJSON && JSON.stringify(lowerCaseObj);
                const encoded = encodeURIComponent(filterObjectString);
                const filterHeader = { FilterParameters: encoded, TenantId: action.tenantId };
                const summaryRequestObj = {
                    url: `${__API_BASE_URL__}${__API_URL_ROOT__}/pulltenant/${action.tenantId}/${action.documentNumber}?operationType=HISTSUM`,
                    resource: __RESOURCE_URL__,
                    headers: { ...setHeader(null), ...filterHeader },
                };
                const { data: responseSummaryObj }: IHttpClientResult<any> = yield call(
                    [httpClient, httpClient.request],
                    summaryRequestObj
                );
                summaryObj = responseSummaryObj?.response?.['approvalSummaryData'];
                updatedSummaryJSON = summaryObj;
            }
            headerDetailsJSON = generatePullModelSummary(action.summaryDataMapping, summaryObj, true);
        }
        // If a Message is returned, display the message as an error
        if (headerDetailsJSON?.Message) {
            yield put(failedHeader(headerDetailsJSON.Message));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Header - Failure',
                'MSApprovals.LoadHeader.AuthFailure',
                TrackingEventId.HeaderLoadSuccessWithAuthFailure,
                stateCommonProperties
            );
        } else {
            try {
                const userAlias = headerDetailsJSON?.Submitter?.Alias;
                const { data: userImage }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
                    url: `${__GRAPH_BASE_URL__}users/${userAlias}${__UPN_SUFFIX__}/photos/96x96/$value`,
                    resource: __GRAPH_RESOURCE_URL__,
                    responseType: 'arraybuffer',
                });
                // convert to base64
                let charConversion = '';
                const byteArrayCopy = new Uint8Array(userImage);
                for (let i = 0; i < byteArrayCopy.byteLength; i++) {
                    charConversion += String.fromCharCode(byteArrayCopy[i]);
                }
                const base64Version = btoa(charConversion);

                yield put(receiveUserImage(base64Version));
                trackBusinessProcessEvent(
                    authClient,
                    telemetryClient,
                    'Load User Image - Success',
                    'MSApprovals.LoadUserImage.Success',
                    TrackingEventId.UserImageLoadSuccess,
                    stateCommonProperties
                );
            } catch (errorResponse: any) {
                const error = errorResponse.data ?? errorResponse;
                yield put(failedUserImage(error.message ? error.message : error));
                const exception = error.message ? new Error(error.message) : error;
                trackException(
                    authClient,
                    telemetryClient,
                    'Load User Image - Failure',
                    'MSApprovals.LoadUserImage.Failure',
                    TrackingEventId.UserImageLoadFailure,
                    stateCommonProperties,
                    exception
                );
            }
            yield put(receiveHeader(headerDetailsJSON, headerTemplateJSON.sum, updatedSummaryJSON));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Header - Success',
                'MSApprovals.LoadHeader.Success',
                TrackingEventId.HeaderLoadSuccess,
                stateCommonProperties,
                { SelectedPage: selectedPage }
            );
        }
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedHeader(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Header - Failure',
            'MSApprovals.LoadHeader.Failure',
            TrackingEventId.HeaderLoadFailure,
            stateCommonProperties,
            exception,
            {
                ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }),
                SelectedPage: selectedPage,
            }
        );
    }
}

function* fetchDetails(action: IRequestDetailsAction): IterableIterator<Effect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const detailsURL = action.isPullModelEnabled
            ? `pulltenant/${action.tenantId}/${action.displayDocumentNumber}?operationType=DTL`
            : `detail/${action.tenantId}/${action.displayDocumentNumber}?CallType=Details`;
        const urlBase = `${__API_BASE_URL__}${__API_URL_ROOT__}/`;
        let reqHeaders = setHeader(action.userAlias, tcv, action.displayDocumentNumber);
        if (action.isPullModelEnabled) {
            const summaryJSON = yield select(getSummaryJSON);
            const flatSummary = flattenObj(summaryJSON, false);
            const flatSummaryString = JSON.stringify(flatSummary);
            const encoded = encodeURIComponent(flatSummaryString);
            const filterHeader = { FilterParameters: encoded, TenantId: action.tenantId };
            reqHeaders = { ...reqHeaders, ...filterHeader };
        }
        const detailsJSONRequest = {
            url: urlBase + detailsURL,
            resource: __RESOURCE_URL__,
            headers: reqHeaders,
        };
        let detailsJSON;
        let templateJSON;
        if (action.requiresTemplate) {
            const [detailsJSONResponse, templateJSONResponse]: any = yield all([
                call([httpClient, httpClient.request], detailsJSONRequest),
                call([httpClient, httpClient.request], {
                    url: `${__API_BASE_URL__}${__API_URL_ROOT__}/adaptivedetail/${action.tenantId}?TemplateType=Details`,
                    resource: __RESOURCE_URL__,
                    headers: setHeader(action.userAlias),
                }),
            ]);
            detailsJSON = detailsJSONResponse?.data;
            templateJSON = templateJSONResponse?.data;
        } else {
            const detailsJSONResponse: IHttpClientResult<object> = yield call(
                [httpClient, httpClient.request],
                detailsJSONRequest
            );
            detailsJSON = detailsJSONResponse?.data;
        }
        if (detailsJSON?.Message) {
            yield put(failedDetails(detailsJSON.Message));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Details - Failure',
                'MSApprovals.LoadDetails.AuthFailure',
                TrackingEventId.DetailsLoadSuccessWithAuthFailure,
                stateCommonProperties
            );
        } else {
            try {
                const tenantInfo: any = yield select(getTenantInfo);
                const tenantInfoForId: { isDetailsEditable: boolean } = tenantInfo?.find(
                    (item: object) => (item as any).tenantId === Number(action.tenantId)
                );
                const editableInTenantInfo = tenantInfoForId && tenantInfoForId.isDetailsEditable;
                if (editableInTenantInfo) {
                    const { data: areDetailsEditable }: IHttpClientResult<boolean> = yield call(
                        [httpClient, httpClient.request],
                        {
                            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/saveeditabledetails?tenantId=${action.tenantId}&documentNumber=${action.displayDocumentNumber}`,
                            resource: __RESOURCE_URL__,
                            headers: setHeader(action.userAlias),
                        }
                    );
                    yield put(receiveAreDetailsEditable(areDetailsEditable));
                } else {
                    put(receiveAreDetailsEditable(false));
                }
            } catch {
                yield put(receiveAreDetailsEditable(false));
            }
            yield all([
                put(receiveMyDetails(detailsJSON, templateJSON?.full)),
                put(markRequestAsRead(action.tenantId, action.displayDocumentNumber, action.userAlias)),
            ]);
            yield put(receiveMyDetails(detailsJSON, templateJSON?.dtl));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Details - Success',
                'MSApprovals.LoadDetails.Success',
                TrackingEventId.DetailsLoadSuccess,
                stateCommonProperties
            );
        }
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedDetails(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Details - Failure',
            'MSApprovals.LoadDetails.Failure',
            TrackingEventId.DetailsLoadFailure,
            stateCommonProperties,
            exception,
            { ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }) }
        );
    }
}

function* postRequestasRead(action: IMarkRequestAsReadAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const url = `${__API_BASE_URL__}${__API_URL_ROOT__}/readdetails/${action.tenantId}`;
        const readDetailsRequest = {
            DocumentKeys: action.displayDocumentNumber,
            ActionDetails: { Action: 'Read Details' },
        };
        const actionRequest = {
            url: url,
            resource: __RESOURCE_URL__,
            data: readDetailsRequest,
            headers: setHeader(action.userAlias, tcv, action.displayDocumentNumber),
        };
        yield call([httpClient, httpClient.post], url, actionRequest);
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Post Request as Read - Success',
            'MSApprovals.PostReadRequest.Success',
            TrackingEventId.PostRequestAsReadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Post Request as Read - Failure',
            'MSApprovals.PostReadRequest.Failure',
            TrackingEventId.PostRequestAsReadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchCallbackDetails(action: IRequestCallbackDetailsAction): IterableIterator<SimpleEffect<{}, {}>> {
    // go through each callback url and make a call
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    const displayDocumentNumber = yield select(getDisplayDocumentNumber);
    let isErrorMessagePresentInResponse = false;
    let callbackJSONs = [];
    try {
        for (let i = 0; i < action.urls.length; i++) {
            const httpClient: IHttpClient = yield getContext('httpClient');
            const { data: callbackDetails }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
                url: `${__API_BASE_URL__}${action.urls[i]}`,
                resource: __RESOURCE_URL__,
                headers: setHeader(action.userAlias, tcv, displayDocumentNumber),
            });
            callbackJSONs.push(callbackDetails);
            if (callbackDetails?.Message) {
                isErrorMessagePresentInResponse = true;
                yield put(failedCallbackDetails(callbackDetails.Message));
                trackBusinessProcessEvent(
                    authClient,
                    telemetryClient,
                    'Load Details - Failure',
                    'MSApprovals.LoadDetails.LobFailure',
                    TrackingEventId.DetailsLoadSuccessWithLobFailure,
                    stateCommonProperties
                );
            }
        }
        if (!isErrorMessagePresentInResponse) {
            yield put(receiveCallbackDetails(callbackJSONs));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Callbacks - Success',
                'MSApprovals.LoadCallbacks.Success',
                TrackingEventId.CallbackLoadSuccess,
                stateCommonProperties
            );
        }
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedCallbackDetails(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Callbacks - Failure',
            'MSApprovals.LoadCallbacks.Failure',
            TrackingEventId.CallbackLoadFailure,
            stateCommonProperties,
            exception,
            { ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }) }
        );
    }
}

function* postAction(action: IPostAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    let loggedInAlias: string;
    const newguid = guid();
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const email: string = yield call([authClient, authClient.getUserId]);
        loggedInAlias = email.substring(0, email.indexOf('@'));
        const additionalData = yield select((state) => state.dynamic?.DetailsReducer?.additionalData);
        const selectedApproval: any = yield select(
            (state) => state.dynamic?.SharedComponentsReducer?.selectedApprovalRecords
        );
        const newApproverUpn = action.peoplePickerSelections?.length > 0 ? action.peoplePickerSelections[0].upn : null;
        const newApproverAlias = newApproverUpn && newApproverUpn.substring(0, newApproverUpn.indexOf('@'));
        let approvalRequest: any;
        let url;
        const selectedTenantDelegation: IDelegationObj = yield select(getSelectedTenantDelegation);
        const originalApprover = selectedTenantDelegation
            ? selectedTenantDelegation.alias
            : action.userAlias
            ? action.userAlias
            : loggedInAlias;
        if (!action.isBulkAction) {
            const formattedBusinessProcessName = formatBusinessProcessName(action.businessProcessName, [
                'ApprovalAction',
                action.code,
            ]);
            url = `${__API_BASE_URL__}${__API_URL_ROOT__}/documentaction/${action.tenantId}`;
            approvalRequest = {
                documentTypeID: action.documentTypeId,
                Action: action.code,
                ActionByAlias: action.userAlias ? action.userAlias : loggedInAlias,
                //ActionByDelegateInMSApprovals: action.userAlias ? action.userAlias : loggedInAlias,
                //OriginalApproverInTenantSystem: loggedInAlias,
                AdditionalData: additionalData ? additionalData : {},
                telemetry: {
                    tcv: newguid,
                    xcv: action.documentNumber,
                    businessProcessName: formattedBusinessProcessName,
                    TenantTelemetry: null as any,
                },
            };
            if (!action.isPullModelEnabled) {
                const actionDetails = {
                    Comment: action.comments,
                    ReasonCode: action.reasonCode,
                    ReasonText: action.reasonText,
                    ActionDate: new Date(),
                    RequestVersion: null as any,
                    ReceiptsAcknowledged: action.receiptsCheck,
                    AntiCorruptionAcknowledged: action.corruptionCheck,
                    DigitalSignature: action.digitalSignature,
                };
                approvalRequest.ActionDetails = actionDetails;
                approvalRequest.DocumentKeys = [
                    {
                        displayDocumentNumber: action.displayDocumentNumber,
                        documentNumber: action.documentNumber,
                        FiscalYear: action.fiscalYear ?? '',
                    },
                ];
            } else if (action.isPullModelEnabled) {
                approvalRequest.ApprovalIdentifier = {
                    displayDocumentNumber: action.displayDocumentNumber,
                    documentNumber: action.documentNumber,
                };
                const pullTenantActionDetails = { actionDate: new Date() };
                if (action.additionalActionDetails !== null) {
                    const newActionDetails = { ...pullTenantActionDetails, ...action.additionalActionDetails };
                    approvalRequest.ActionDetails = newActionDetails;
                }
                approvalRequest.ActionByDelegateInMSApprovals = loggedInAlias;
                approvalRequest.OriginalApproverInTenantSystem = originalApprover;
                approvalRequest.ActionByAlias = originalApprover;
                approvalRequest = [approvalRequest];
            }
        } else {
            // bulk approvals
            yield put(updatePanelState(false));
            const tvcNum = action.documentTypeId;
            url = `${__API_BASE_URL__}${__API_URL_ROOT__}/documentaction/${
                action.tenantId
            }?sessionId=${telemetryClient.getCorrelationId()}`;
            if (!action.isPullModelEnabled) {
                const documentKeys = [...(selectedApproval as any)];
                approvalRequest = {
                    ActionDetails: {
                        Comment: action.comments,
                        ReasonCode: action.reasonCode,
                        ReasonText: action.reasonText,
                        ActionDate: new Date(),
                    },
                    ActionByAlias: action.userAlias ? action.userAlias : loggedInAlias,
                    Action: action.code,
                    DocumentKeys: documentKeys,
                    Tcv: tvcNum,
                };
            } else if (action.isPullModelEnabled) {
                approvalRequest = [];
                const tenantDataModel = yield select(getTenantDataModelMapping);
                const documentTypeId = yield select(getTenantDocumentTypeId);
                const businessProcessName = yield select(getTenantBusinessProcessName);
                const formattedBusinessProcessName = formatBusinessProcessName(businessProcessName, [
                    'ApprovalAction',
                    action.code,
                ]);
                const pullTenantActionDetails = { actionDate: new Date() };
                let newActionDetails = {};
                if (action.additionalActionDetails !== null) {
                    newActionDetails = { ...pullTenantActionDetails, ...action.additionalActionDetails };
                }
                for (let i = 0; i < selectedApproval.length; i++) {
                    const summaryJSONForItem = selectedApproval[i];
                    let requestForItem: any = {
                        documentTypeID: documentTypeId,
                        Action: action.code,
                        ActionByAlias: originalApprover,
                        telemetry: {
                            tcv: newguid,
                            xcv: summaryJSONForItem.laborId,
                            businessProcessName: formattedBusinessProcessName,
                        },
                        ActionByDelegateInMSApprovals: loggedInAlias,
                        OriginalApproverInTenantSystem: originalApprover,
                    };
                    requestForItem.ActionDetails = newActionDetails;
                    requestForItem.ApprovalIdentifier = {
                        displayDocumentNumber: summaryJSONForItem.laborId,
                        documentNumber: summaryJSONForItem.laborId,
                    };

                    const summaryObj = generatePullModelSummary(tenantDataModel, summaryJSONForItem);
                    const summaryJSONObj = generateSummaryObjForPullTenant(
                        summaryObj,
                        originalApprover,
                        summaryJSONForItem
                    );
                    const additionalData = generatePullTenantAdditionalData(summaryJSONForItem, summaryJSONObj);
                    requestForItem.AdditionalData = additionalData;
                    approvalRequest.push(requestForItem);
                }
            }
        }
        if (approvalRequest.ActionDetails) {
            if (action.code?.toLowerCase() === 'addapprover'.toLowerCase()) {
                approvalRequest.ActionDetails.ApproverType = 'Interim';
            }
            if (
                (action.code?.toLowerCase() === 'addapprover'.toLowerCase() ||
                    action.code?.toLowerCase() === 'reassign'.toLowerCase()) &&
                newApproverAlias &&
                newApproverAlias !== ''
            ) {
                approvalRequest.ActionDetails.NewApproverAlias = newApproverAlias;
            }

            if (action.sequenceID && action.sequenceID !== '') {
                approvalRequest.ActionDetails.SequenceID = action.sequenceID;
            }

            if (action.reasonCode && action.reasonCode !== '') {
                approvalRequest.ActionDetails.ReasonCode = action.reasonCode;
                approvalRequest.ActionDetails.ReasonText = action.reasonText;
            }

            //for adding next level approver in hierarchy
            if (action.nextApprover && action.nextApprover != '') {
                approvalRequest.ActionDetails.NextApprover = action.nextApprover;
                approvalRequest.ActionDetails.SequenceID = 'AddApprover';
            }
        }

        const headerObject = {
            ClientDevice: 'React',
            Xcv: action.documentNumber ?? newguid,
            Tcv: newguid,
            TenantId: action.tenantId,
            ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }),
        };

        const actionRequest = {
            url: url,
            resource: __RESOURCE_URL__,
            headers: headerObject,
            data: approvalRequest,
        };
        const { data: actionResponse }: IHttpClientRequest = yield call(
            [httpClient, httpClient.post],
            url,
            actionRequest
        );
        const isProcessingBulkApproval = yield select(
            (state) => state.dynamic?.SharedComponentsReducer?.isProcessingBulkApproval
        );

        if (!action.isBulkAction) {
            //successful single action
            const isProcessingAction = yield select(getIsProcessingAction);
            const showSuccess = !!isProcessingAction;
            yield put(receiveActionResponse(actionResponse, showSuccess));
            if (action.isPullModelEnabled) {
                yield put(updateSuccessfulPullTenantRequests(Number(action.tenantId), [action.displayDocumentNumber]));
            }
            const filteredApprovalRecords = selectedApproval?.filter(
                (item: { DocumentNumber: string }) => item.DocumentNumber != action.documentNumber
            );
            if (selectedApproval?.length != filteredApprovalRecords.length) {
                yield put(updateApprovalRecords(filteredApprovalRecords));
            }
            if (filteredApprovalRecords?.length > 0) {
                yield put(updateRetainBulkSelection(true));
            }
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Post action - Success',
                'MSApprovals.PostAction.Success',
                TrackingEventId.PostActionSuccess,
                stateCommonProperties
            );
        } else {
            // successful bulk action
            if (isProcessingBulkApproval) {
                let requestsToRemove = [];
                for (let i = 0; i < selectedApproval?.length; i++) {
                    requestsToRemove.push(selectedApproval?.[i]?.laborId);
                }
                yield put(updateBulkStatus(true));
                yield put(updateIsProcessingBulkApproval(false));
                yield put(receiveActionResponse(actionResponse, false));
                yield put(updateApprovalRecords([]));
                yield put(updateBulkFailedStatus(false));
                if (action.isPullModelEnabled) {
                    yield put(updateSuccessfulPullTenantRequests(Number(action.tenantId), requestsToRemove));
                }
            }
        }
    } catch (errorResponse: any) {
        let errorMessage = errorResponse.data ?? errorResponse;
        errorMessage = errorMessage?.ErrorMessage ?? errorMessage;
        if (!action.isBulkAction) {
            //failed single action
            yield put(failedPostAction(errorMessage, action.displayDocumentNumber, action.isPullModelEnabled));
            if (action.isPullModelEnabled) {
                yield put(updateFailedPullTenantRequests([{ Key: action.displayDocumentNumber, Value: errorMessage }]));
            }
            yield put(updateBulkFailedValue(['']));
            yield put(updateBulkFailedStatus(false));
            const exception = errorMessage?.ErrorMessage ? new Error(errorMessage?.ErrorMessage) : errorResponse;
            trackException(
                authClient,
                telemetryClient,
                'Post action - Failure',
                'MSApprovals.PostAction.Failure',
                TrackingEventId.PostActionFailure,
                stateCommonProperties,
                exception
            );
        } else {
            //failed bulk action
            const errorMessage = errorResponse.data ?? errorResponse;
            let errorList = [errorMessage];
            const isProcessingBulkApproval = yield select(
                (state) => state.dynamic?.SharedComponentsReducer?.isProcessingBulkApproval
            );
            if (errorMessage?.ApprovalResponseDetails) {
                const successCount = errorMessage.ApprovalResponseDetails?.True?.length;
                const failureCount = errorMessage.ApprovalResponseDetails?.False?.length;
                const successfulRequests = errorMessage.ApprovalResponseDetails?.True?.map((item: any) => item.Key);
                let failureMessage =
                    successCount > 0 ? '<strong>' + successCount + '</strong> request(s) passed, ' : '';
                const failureCountMessage =
                    failureCount > 0
                        ? '<strong>' + failureCount + '</strong> request(s) failed.'
                        : 'Unable to complete action at this time. If this issue persists, please contact support team';
                if (action.isPullModelEnabled) {
                    const hoverMessage =
                        'Please hover on the "Failure" status of the corresponding request(s) to find more information.';
                    const countWithHoverMessage =
                        failureCount > 0 ? failureCountMessage + ' ' + hoverMessage : failureCountMessage;
                    failureMessage += 'Tracking Id: ' + newguid + ':: ' + countWithHoverMessage;
                    errorList = [failureMessage];
                    yield put(updateFailedPullTenantRequests(errorMessage?.ApprovalResponseDetails?.False));
                    if (successfulRequests?.length > 0) {
                        yield put(updateSuccessfulPullTenantRequests(Number(action.tenantId), successfulRequests));
                    }
                    if (successCount > 0) {
                        yield put(updateApprovalRecords([]));
                    }
                } else {
                    const failureList = errorMessage.ApprovalResponseDetails?.False;
                    const errorMessageList = failureList?.map((item: any) => item.Key + ' - ' + item.Value);
                    if (errorMessageList) {
                        errorList = [failureCountMessage].concat(errorMessageList);
                    }
                }
            }
            if (isProcessingBulkApproval) {
                //refresh summary if user has not clicked continue
                if (!action.isPullModelEnabled) {
                    yield put(requestMySummary(action.userAlias));
                }
                yield put(updateBulkFailedStatus(true));
                yield put(updateIsProcessingBulkApproval(false));
                yield put(updateBulkFailedValue(errorList));
            }
        }
    }
}

function* fetchDocument(action: IRequestDocumentAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        let selectedURL = action.url;
        if (
            action.url.length <= 0 &&
            action.tenantId &&
            action.documentNumber &&
            action.displayDocumentNumber &&
            action.attachmentId
        ) {
            selectedURL = `${__API_BASE_URL__}${__API_URL_ROOT__}/documentdownload/${action.tenantId}/${action.documentNumber}/?displayDocumentNumber=${action.displayDocumentNumber}&attachmentId=${action.attachmentId}`;
        }

        if (selectedURL.indexOf('${IsPreAttached}') > -1) {
            selectedURL = selectedURL.replace('${IsPreAttached}', 'true');
        }
        const { data: fileBytes }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: selectedURL,
            resource: __RESOURCE_URL__,
            responseType: 'arraybuffer',
            headers: setHeader(action.userAlias, tcv, action.displayDocumentNumber),
        });
        yield put(requestDocumentEnd());
        const id = action.actionId;
        if (id.includes('download')) {
            const blob = new Blob([fileBytes], { type: 'application/octet-stream' });
            const link = document.createElement('a');
            const objectUrl = URL.createObjectURL(blob);
            link.setAttribute('target', '_blank');
            link.setAttribute('href', objectUrl);
            link.setAttribute('download', action.attachmentName);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Attachment download - Success',
                'MSApprovals.AttachmentDownload.Success',
                TrackingEventId.AttachmentLoadSuccess,
                stateCommonProperties
            );
        }
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedDocumentDownload(error.message ? error.message : 'Unable to download attachment'));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Attachment download - Failure',
            'MSApprovals.AttachmentDownload.Failure',
            TrackingEventId.AttachmentLoadFailure,
            stateCommonProperties,
            exception,
            { ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }) }
        );
    }
}

function* fetchDocumentPreview(action: IRequestDocumentPreviewAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        action.isPreAttached = action.isPreAttached ?? true;
        const { data: fileBytes }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/documentpreview/${action.tenantId}/${action.documentNumber}/?displayDocumentNumber=${action.displayDocumentNumber}&attachmentId=${action.attachmentId}&isPreAttached=${action.isPreAttached}`,
            resource: __RESOURCE_URL__,
            responseType: 'arraybuffer',
            headers: setHeader(action.userAlias, tcv, action.displayDocumentNumber),
        });
        let charConversion = '';
        const byteArrayCopy = new Uint8Array(fileBytes);
        for (let i = 0; i < byteArrayCopy.byteLength; i++) {
            charConversion += String.fromCharCode(byteArrayCopy[i]);
        }
        const base64Version = btoa(charConversion);
        yield put(receiveDocumentPreview(base64Version));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Attachment preview - Success',
            'MSApprovals.AttachmentPreview.Success',
            TrackingEventId.AttachmentPreviewLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedDocumentPreview(error.message ? error.message : 'Unable to load attachment preview'));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Attachment preview - Failure',
            'MSApprovals.AttachmentPreview.Failure',
            TrackingEventId.AttachmentPreviewLoadSuccess,
            stateCommonProperties,
            exception,
            { ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }) }
        );
    }
}

function* fetchAllDocuments(action: IRequestAllDocumentsAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const allDocumentsRequestData = {
            Attachments: action.attachmentArray,
        };
        const allDocumentsRequestURL = `${__API_BASE_URL__}${__API_URL_ROOT__}/alldocumentdownload?tenantid=${action.tenantId}&documentNumber=${action.documentNumber}&displayDocumentNumber=${action.displayDocumentNumber}`;
        const allDocumentsRequest = {
            url: allDocumentsRequestURL,
            resource: __RESOURCE_URL__,
            responseType: 'arraybuffer',
            headers: setHeader(action.userAlias, tcv, action.displayDocumentNumber),
            data: allDocumentsRequestData,
        };
        const { data: zipBytes }: IHttpClientRequest = yield call(
            [httpClient, httpClient.post],
            allDocumentsRequestURL,
            allDocumentsRequest
        );
        yield put(requestDocumentEnd());
        const blob = new Blob([zipBytes], { type: 'application/zip' });
        const link = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        const attachmentName = 'MSA-' + action.documentNumber + '.zip';
        link.setAttribute('target', '_blank');
        link.setAttribute('href', objectUrl);
        link.setAttribute('download', attachmentName);
        const divBanner = document.createElement('div');
        divBanner.setAttribute('style', 'width:100px;height:100px;background:black;');
        document.body.appendChild(divBanner);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Attachment download - Success',
            'MSApprovals.AttachmentDownloadZip.Success',
            TrackingEventId.AttachmentZipLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedAllDocumentsDownload(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Attachment download zip - Failure',
            'MSApprovals.AttachmentDownloadZip.Failure',
            TrackingEventId.AttachmentZipLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* postEditableDetails(action: IPostEditableDetailsAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const tcv = yield select(getTcv);
    const displayDocumentNumber = yield select(getDisplayDocumentNumber);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const url = `${__API_BASE_URL__}${__API_URL_ROOT__}/saveeditabledetails/${action.tenantId}`;
        const additionalData: { editDetailsData: object } = yield select(
            (state) => state.dynamic?.DetailsReducer?.additionalData
        );

        //using details data passed from child component
        const editDetailsData = additionalData.editDetailsData;
        const actionRequest = {
            url: url,
            resource: __RESOURCE_URL__,
            data: editDetailsData,
            headers: setHeader(action.userAlias, tcv, displayDocumentNumber),
        };
        yield call([httpClient, httpClient.post], url, actionRequest);
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Save editable details - Success',
            'MSApprovals.SaveEditableDetails.Success',
            TrackingEventId.SaveEditableDetailsSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedEditDetails(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Save editable details - Failure',
            'MSApprovals.SaveEditableDetails.Failure',
            TrackingEventId.SaveEditableDetailsFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* uploadFiles(action: IUploadFileAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const newguid = guid();
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const url = `${__API_BASE_URL__}${__API_URL_ROOT__}/AttachmentUpload/${action.tenantId}/${action.documentNumber}`;

        const headerObject = {
            ClientDevice: 'React',
            Xcv: newguid,
            Tcv: newguid,
            TenantId: action.tenantId,
            ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }),
        };
        const actionRequest = {
            url: url,
            resource: __RESOURCE_URL__,
            headers: headerObject,
            data: action.files,
        };
        const { data: actionResponse }: IHttpClientRequest = yield call(
            [httpClient, httpClient.post],
            url,
            actionRequest
        );
        const successCount = actionResponse.filter((x: IFileUploadResponse) => x.actionResult === true).length;
        const failureCount = actionResponse.filter((x: IFileUploadResponse) => x.actionResult === false).length;
        if (failureCount === 0) {
            yield put(successFileUpload());
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Attachment upload - Success',
                'MSApprovals.FileAttachmentUpload.Success',
                TrackingEventId.FileAttachmentUploadSuccess,
                stateCommonProperties
            );
        } else {
            const successCountMessage =
                successCount > 0 ? '<strong>' + successCount + '</strong> file(s) uploaded. <br>' : '';
            const failureCountMessage =
                failureCount > 0
                    ? '<strong>' + failureCount + '</strong> file(s) upload failed. <br>'
                    : 'Unable to complete file upload at this time. If this issue persists, please contact support team <br>';
            const errorMessageList: string[] = actionResponse
                ?.filter((item: IFileUploadResponse) => item.e2EErrorInformation)
                .map((item: IFileUploadResponse) => item.name + ' - ' + item.e2EErrorInformation.errorMessages);
            const errorList = errorMessageList;
            yield put(failedFileUpload(successCountMessage + failureCountMessage, errorList));
        }

        yield put(
            requestMyDetails(
                action.tenantId,
                action.documentNumber,
                action.displayDocumentNumber,
                action.userAlias,
                action.requiresTemplate,
                action.isPullModelEnabled
            )
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedFileUpload(error.message ? error.message : 'Unable to upload file attachments', null));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'File attachment upload - Failure',
            'MSApprovals.FileAttachmentUpload.Failure',
            TrackingEventId.FileAttachmentUploadFailure,
            stateCommonProperties,
            exception,
            { ...(action.userAlias && { UserAlias: `${action.userAlias ? action.userAlias : undefined}` }) }
        );
    }
}

export function* detailsSagas(): IterableIterator<{}> {
    yield all([
        takeLatest(DetailsActionType.REQUEST_USER_IMAGE, fetchUserImage),
        takeLatest(DetailsActionType.REQUEST_HEADER, fetchHeader),
        takeLatest(DetailsActionType.REQUEST_MY_DETAILS, fetchDetails),
        takeLatest(DetailsActionType.REQUEST_CALLBACK_DETAILS, fetchCallbackDetails),
        takeLatest(DetailsActionType.POST_ACTION, postAction),
        takeLatest(DetailsActionType.REQUEST_DOCUMENT, fetchDocument),
        takeLatest(DetailsActionType.REQUEST_DOCUMENT_PREVIEW, fetchDocumentPreview),
        takeLatest(DetailsActionType.REQUEST_ALL_DOCUMENTS, fetchAllDocuments),
        takeLatest(DetailsActionType.MARK_REQUEST_AS_READ, postRequestasRead),
        takeLatest(DetailsActionType.POST_EDITABLE_DETAILS, postEditableDetails),
        takeLatest(DetailsActionType.UPLOAD_FILE, uploadFiles),
    ]);
}
