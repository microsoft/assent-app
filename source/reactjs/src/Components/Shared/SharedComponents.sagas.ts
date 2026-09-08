import { SimpleEffect, Effect, getContext, put, all, call, takeLatest, select, delay } from 'redux-saga/effects';
import { IAuthClient } from '@micro-frontend-react/employee-experience/lib/IAuthClient';
import { ITelemetryClient } from '@micro-frontend-react/employee-experience/lib/ITelemetryClient';
import { guid } from '../../Helpers/Guid';
import {
    IRequestDownloadHistory,
    IRequestDownloadSummary,
    IRequestDelegationsAction,
    IRequestExternalTenantInfo,
    IRequestFilteredUsersAction,
    IRequestMyHistoryAction,
    IRequestPullTenantSummary,
    IRequestPullTenantSummaryCount,
    IRequestSummaryAction,
    ISaveUserPreferencesRequest,
    IRequestUserPreferences,
    SharedComponentsActionType,
    IInitiateSearch,
    IPostQuickTourInfo,
    IRequestInsights,
    IPostFeedback,
    IRequestSuggest,
} from './SharedComponents.action-types';
import {
    receiveFriendByEmail,
    receiveMySummary,
    receiveTenantInfo,
    failedSummary,
    failedTenantInfo,
    failedProfile,
    receiveMyOutOfSyncSummary,
    failedOutOfSyncSummary,
    receiveMyHistory,
    failedHistory,
    receiveMyDelegations,
    receiveFilteredUsers,
    receiveDownloadHistory,
    receiveDownloadSummary,
    failedDownloadSummary,
    failedDownloadHistory,
    SaveUserPreferencesResponse,
    ReceiveUserPreferences,
    RequestUserPreferences,
    SaveUserPreferencesFailed,
    receiveSubmitterImages,
    concatSubmitterImages,
    receivePullTenantSummary,
    receiveExternalTenantInfo,
    failedPullTenantSummary,
    updatePullTenantSearchCriteria,
    receivePullTenantSummaryCount,
    receiveTenantDelegations,
    failedExternalTenantInfo,
    requestExternalTenantInto,
    saveSearchResults,
    receiveQuickTourInfo,
    setQuickTourData,
    receiveFlightingData,
    requestFlightingData,
    subscribeFlightingFeaturesSuccess,
    subscribeFlightingFeaturesFailed,
    flightingFeatureFeedbackFailed,
    receiveInsights,
    receiveSuggest,
    clearSuggest,
} from './SharedComponents.actions';
import { saveAs } from 'file-saver';
import {
    getSummaryExportFileName,
    SUMMARY_EXPORT_CONTENT_TYPE,
} from '../Summary/SummaryExport.constants';
import {
    getStateCommonTelemetryProperties,
    getSubmitterImages,
    getSelectedTenantDelegation,
    getPullTenantSearchCriteria,
    getSummary,
    getFeedbackByDocumentNumber,
    getSuggestCache,
    getIsFeatureEnabledForUser,
} from './SharedComponents.selectors';
import {
    trackBusinessProcessEvent,
    trackException,
    trackFeatureUsageEvent,
    TrackingEventId,
} from '../../Helpers/telemetryHelpers';
import { setHeader } from './Components/SagasHelper';
import { IDelegationObj, IGraphPhoto, IQuickTourListItem, IFeedbackData } from './SharedComponents.types';
import {
    IHttpClient,
    IHttpClientResult,
    IHttpClientRequest,
} from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { safeJSONParse } from '../../Helpers/sharedHelpers';
import { normalizeSearchResult } from '../../Helpers/searchHighlightUtils';
import { getOnBehalfUserUpn, getOnBehalfUserId, getUserAlias } from './SharedComponents.persistent-selectors';

function* fetchProfile(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: profile }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__GRAPH_BASE_URL__}/me`,
            resource: __GRAPH_RESOURCE_URL__,
        });
        yield put(receiveFriendByEmail(profile));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load Profile - Success',
            'MSApprovals.LoadProfile.Success',
            TrackingEventId.ProfileLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedProfile(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Profile - Failure',
            'MSApprovals.LoadProfile.Failure',
            TrackingEventId.ProfileLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchFilteredUsers(action: IRequestFilteredUsersAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: results }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__GRAPH_BASE_URL__}users?$filter=startswith(userPrincipalName%2C+'${action.filterText}')`,
            resource: __GRAPH_RESOURCE_URL__,
        });
        yield put(receiveFilteredUsers(results.value));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load Graph Users - Success',
            'MSApprovals.LoadGraphUsers.Success',
            TrackingEventId.GraphUsersLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Graph Users - Failure',
            'MSApprovals.LoadGraphUsers.Failure',
            TrackingEventId.GraphUsersLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchDelegations(action: IRequestDelegationsAction): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        let loggedInAlias = action.loggedInAlias;
        if (!loggedInAlias) {
            const email: string = yield call([authClient, authClient.getUserId]);
            loggedInAlias = email.substring(0, email.indexOf('@'));
        }
        const httpClient: IHttpClient = yield getContext('httpClient');
        let url = `${__API_BASE_URL__}${__API_URL_ROOT__}`;
        if (action.tenantId) {
            url = url + `/users/delegations?tenantId=${action.tenantId}`;
        } else if (!action.tenantId) {
            url += '/me/delegations/';
        }
        const { data: delegations }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: url,
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
        });
        if (action.tenantId && action.appName) {
            const delegationList = delegations ? delegations[0]['delegations'] : null;
            let tenantDelegationObj = null;
            if (delegationList && delegationList.length > 0) {
                tenantDelegationObj = {
                    tenantId: action.tenantId,
                    appName: action.appName,
                    delegations: delegationList,
                };
            }
            yield put(receiveTenantDelegations(tenantDelegationObj));
        } else {
            yield put(receiveMyDelegations(delegations));
        }
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load Delegations - Success',
            'MSApprovals.LoadDelegations.Success',
            TrackingEventId.DelegationsLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Delegations - Failure',
            'MSApprovals.LoadDelegations.Failure',
            TrackingEventId.DelegationsLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchGraphData(submitterAlias: string): IterableIterator<Effect<{}, {}>> {
    const httpClient: IHttpClient = yield getContext('httpClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    try {
        const resp: IHttpClientResult<any> = yield call([httpClient, httpClient.request], {
            url: `${__GRAPH_BASE_URL__}users/${submitterAlias}${__UPN_SUFFIX__}/photos/48x48/$value`,
            resource: __GRAPH_RESOURCE_URL__,
            responseType: 'blob',
        });
        if (resp.status === 200) {
            const imageURL = window.URL.createObjectURL(resp.data);
            return { alias: submitterAlias, image: imageURL };
        } else {
            return { alias: submitterAlias, image: null };
        }
    } catch (ex) {
        return { alias: submitterAlias, image: null };
    }
}

function* fetchSummary(action: IRequestSummaryAction): IterableIterator<Effect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const summaryUrl = new URL(`${__API_BASE_URL__}${__API_URL_ROOT__}/summary`);
        if (action.isSubmittedRequest) {
            summaryUrl.searchParams.set('isSubmittedRequest', 'true');
        }
        const { data: summary }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: summaryUrl.toString(),
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias, '', '', onBehalfUserUpn, onBehalfUserId),
        });
        yield put(receiveMySummary(summary));
        if (action.isSubmittedRequest) {
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Submitter View Summary - Success',
                'MSApprovals.SubmitterView.LoadSummary.Success',
                TrackingEventId.SubmitterViewSummaryLoadSuccess,
                stateCommonProperties
            );
        } else {
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Load Summary - Success',
                'MSApprovals.LoadSummary.Success',
                TrackingEventId.SummaryLoadSuccess,
                stateCommonProperties
            );
        }
        let submitters: string[] = [];
        for (let i = 0; i < summary.length; i++) {
            const submitter = summary[i]?.Submitter?.Alias;
            if (!submitters.includes(submitter)) {
                submitters.push(submitter);
            }
        }
        const graphResponses = yield all(submitters.map((sub) => call(fetchGraphData, sub)));
        yield put(receiveSubmitterImages(graphResponses));
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        
        // Create error message with SAW device warning
        const errorMessage = "There was an error loading your requests, please try again later. If you're on a SAW device, this site is not allow-listed in your silo. Please use a corp machine.";
        
        yield put(failedSummary(errorMessage));
        const exception = error.message ? new Error(error.message) : error;
        if (action.isSubmittedRequest) {
            trackException(
                authClient,
                telemetryClient,
                'Load Submitter View Summary - Failure',
                'MSApprovals.SubmitterView.LoadSummary.Failure',
                TrackingEventId.SubmitterViewSummaryLoadFailure,
                stateCommonProperties,
                exception
            );
        } else {
            trackException(
                authClient,
                telemetryClient,
                'Load Summary - Failure',
                'MSApprovals.LoadSummary.Failure',
                TrackingEventId.SummaryLoadFailure,
                stateCommonProperties,
                exception
            );
        }
    }
}

function* fetchHistory(action: IRequestMyHistoryAction): IterableIterator<Effect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const currentImages: IGraphPhoto[] = yield select(getSubmitterImages);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: history }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/history?page=${action.page}&sortColumn=${action.sortColumn}&sortDirection=${action.sortDirection}&searchCriteria=${action.searchCriteria}&timePeriod=${action.timePeriod}&tenantId=${action.tenantId}`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
        });
        yield put(receiveMyHistory(history));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load History - Success',
            'MSApprovals.LoadHistory.Success',
            TrackingEventId.HistoryLoadSuccess,
            stateCommonProperties
        );
        let newSubmitters: string[] = [];
        for (let i = 0; i < history?.Records?.length; i++) {
            const submitter = history.Records[i]['SubmittedAlias'];
            const matchingElement = currentImages?.find((el) => el.alias === submitter);
            if (!matchingElement && !newSubmitters.includes(submitter)) {
                newSubmitters.push(submitter);
            }
        }
        const newGraphResponses = yield all(newSubmitters.map((sub) => call(fetchGraphData, sub)));
        yield put(concatSubmitterImages(newGraphResponses));
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedHistory(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load History - Failure',
            'MSApprovals.LoadHistory.Failure',
            TrackingEventId.HistoryLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* downloadHistory(action: IRequestDownloadHistory): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: historyDownload }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/history/download?monthsOfData=${action.monthsOfData}&searchCriteria=${action.searchCriteria}&sortField=${action.sortField}&sortDirection=${action.sortDirection}&tenantId=${action.tenantId}`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
        });
        const blob = new Blob([historyDownload], { type: 'text/csv;charset=utf-8' });
        saveAs(blob, 'history.csv');
        yield put(receiveDownloadHistory());
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Download History - Success',
            'MSApprovals.DownloadHistory.Success',
            TrackingEventId.HistoryDownloadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedDownloadHistory(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Download History - Failure',
            'MSApprovals.DownloadHistory.Failure',
            TrackingEventId.HistoryDownloadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* downloadSummary(action: IRequestDownloadSummary): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: summaryDownload }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/summary/download`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias, '', '', onBehalfUserUpn, onBehalfUserId),
            data: {
                columns: action.columns,
                columnHeaders: action.columnHeaders,
                filters: action.filters ?? {},
                columnFormats: action.columnFormats ?? {},
            },
        });
        const blob = new Blob([summaryDownload], { type: SUMMARY_EXPORT_CONTENT_TYPE });
        // Resolve whose summary this file represents: an explicit alias (a delegation) wins; otherwise fall
        // back to the logged-in user's identity, mirroring how the SecondaryHeader persona resolves the
        // displayed user (delegator alias -> logged-in UPN). getSummaryExportFileName strips the domain, so
        // a full UPN is fine here. Guarded so a failure to read the identity never blocks a completed export.
        let fileNameAlias: string = action.userAlias;
        if (!fileNameAlias) {
            try {
                fileNameAlias = yield call([authClient, authClient.getUserId]);
            } catch {
                fileNameAlias = '';
            }
        }
        saveAs(blob, getSummaryExportFileName(fileNameAlias));
        yield put(receiveDownloadSummary());
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Download Summary - Success',
            'MSApprovals.DownloadSummary.Success',
            TrackingEventId.SummaryDownloadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedDownloadSummary(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Download Summary - Failure',
            'MSApprovals.DownloadSummary.Failure',
            TrackingEventId.SummaryDownloadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchTenantInfo(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: tenantInfo }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/TenantInfo`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
        });
        yield put(receiveTenantInfo(tenantInfo));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load Tenant Info - Success',
            'MSApprovals.LoadTenantInfo.Success',
            TrackingEventId.TenantInfoLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedTenantInfo(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Tenant Info - Failure',
            'MSApprovals.LoadTenantInfo.Failure',
            TrackingEventId.TenantInfoLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchOutOfSyncSummary(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const { data: outOfSyncSummary }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/outofsyncsummary`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
        });
        yield put(receiveMyOutOfSyncSummary(outOfSyncSummary));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load Out of Sync Summary - Success',
            'MSApprovals.LoadOOSSummary.Success',
            TrackingEventId.OOSSummaryLoadSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedOutOfSyncSummary(error.message));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Load Out of Sync Summary - Failure',
            'MSApprovals.LoadOOSSummary.Failure',
            TrackingEventId.OOSSummaryLoadFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* saveUserPreferences(action: ISaveUserPreferencesRequest): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const result: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/preferences?SessionId=${telemetryClient.getCorrelationId()}`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
            data: action.data,
        });
        yield put(SaveUserPreferencesResponse('Preferences submitted successfully.'));
        yield put(RequestUserPreferences(action.preserveSessionState));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Save preferences - Success',
            'MSApprovals.userpreference.Success',
            TrackingEventId.SaveUserPreferenceSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        yield put(SaveUserPreferencesFailed(exception.message));
        trackException(
            authClient,
            telemetryClient,
            'Save preferences - Failure',
            'MSApprovals.userpreference.Failure',
            TrackingEventId.SaveUserPreferenceFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchFlightingData(): IterableIterator<Effect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const [myFlightingDataRes, allFlightingDataRes]: any = yield all([
            call([httpClient, httpClient.request], {
                url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/flightingFeatures`,
                resource: __RESOURCE_URL__,
                headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
            }),
            call([httpClient, httpClient.request], {
                url: `${__API_BASE_URL__}${__API_URL_ROOT__}/metadata/flightingFeatures`,
                resource: __RESOURCE_URL__,
                headers: setHeader(null),
            }),
        ]);

        const myFlightingData = myFlightingDataRes?.data;
        const allFlightingData = allFlightingDataRes?.data;

        if (myFlightingData != null && myFlightingData != undefined && allFlightingData) {
            yield put(receiveFlightingData(myFlightingData, allFlightingData));
        }
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Get flighting data - Success',
            'MSApprovals.GetFlightingData.Success',
            TrackingEventId.GetFlightingDataSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        yield put(receiveFlightingData(null, null));
        const error = errorResponse.data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Get flighting data - Failure',
            'MSApprovals.GetFlightingData.Failure',
            TrackingEventId.GetFlightingDataFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* subscribeFlightingFeaturesSaga(action: any): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/flightingFeatures`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
            data: { FeatureNames: action.featureNames },
        });
        
        yield put(subscribeFlightingFeaturesSuccess('Successfully subscribed to features'));
        yield put(requestFlightingData());
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Subscribe to flighting features - Success',
            'MSApprovals.SubscribeFlightingFeatures.Success',
            TrackingEventId.SubscribeFlightingFeaturesSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        const exception = error.message ? new Error(error.message) : error;
        yield put(subscribeFlightingFeaturesFailed(error.message || 'Failed to subscribe to features'));
        trackException(
            authClient,
            telemetryClient,
            'Subscribe to flighting features - Failure',
            'MSApprovals.SubscribeFlightingFeatures.Failure',
            TrackingEventId.SubscribeFlightingFeaturesFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* unsubscribeFlightingFeaturesSaga(action: any): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/flightingFeatures`,
            method: 'DELETE',
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
            data: { FeatureNames: action.featureNames },
        });
        
        yield put(subscribeFlightingFeaturesSuccess('Successfully unsubscribed from features'));
        yield put(requestFlightingData());
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Unsubscribe from flighting features - Success',
            'MSApprovals.UnsubscribeFlightingFeatures.Success',
            TrackingEventId.UnsubscribeFlightingFeaturesSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        const exception = error.message ? new Error(error.message) : error;
        yield put(subscribeFlightingFeaturesFailed(error.message || 'Failed to unsubscribe from features'));
        trackException(
            authClient,
            telemetryClient,
            'Unsubscribe from flighting features - Failure',
            'MSApprovals.UnsubscribeFlightingFeatures.Failure',
            TrackingEventId.UnsubscribeFlightingFeaturesFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* submitFlightingFeatureFeedbackSaga(action: any): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/flightingFeatures/feedback`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
            data: { FeatureName: action.featureName, Vote: action.vote },
        });
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Submit flighting feature feedback - Success',
            'MSApprovals.SubmitFlightingFeatureFeedback.Success',
            TrackingEventId.SubmitFlightingFeatureFeedbackSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        const exception = error.message ? new Error(error.message) : error;
        yield put(flightingFeatureFeedbackFailed(error.message || 'Failed to submit feature feedback'));
        trackException(
            authClient,
            telemetryClient,
            'Submit flighting feature feedback - Failure',
            'MSApprovals.SubmitFlightingFeatureFeedback.Failure',
            TrackingEventId.SubmitFlightingFeatureFeedbackFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchUserPreferences(action: IRequestUserPreferences): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/preferences?SessionId=${telemetryClient.getCorrelationId()}`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
        });

        const featurePreferenceJson = safeJSONParse(data?.featurePreferenceJson);
        const digestPreference = safeJSONParse(data?.digestPreferenceJson);
        const teamsNotificationsEnabled = data?.teamsNotificationsEnabled ?? null;

        yield put(
            ReceiveUserPreferences(
                featurePreferenceJson || [],
                action.preserveSessionState,
                digestPreference,
                teamsNotificationsEnabled
            )
        );
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Get preferences - Success',
            'MSApprovals.userpreference.Success',
            TrackingEventId.GetUserPreferenceSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Get preferences - Failure',
            'MSApprovals.userpreference.Failure',
            TrackingEventId.GetUserPreferenceFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchPullTenantSummary(action: IRequestPullTenantSummary): IterableIterator<Effect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const newGuid = guid();
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const selectedTenantDelegation: IDelegationObj = yield select(getSelectedTenantDelegation);
        const pullTenantSearchCriteria: any = yield select(getPullTenantSearchCriteria);
        const delegatedUserAlias = selectedTenantDelegation ? selectedTenantDelegation.alias : action.userAlias;
        const filterObject = action.filterCriteria && {
            'weekRange.startDate': action.filterCriteria.startDate,
            'weekRange.endDate': action.filterCriteria.endDate,
        };
        const filterObjectString = filterObject && JSON.stringify(filterObject);
        const filterCriteriaHeader = action.filterCriteria && {
            FilterParameters: filterObjectString,
        };
        const tenantSummaryResponse: any = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/PullTenant/${action.tenantId}`,
            resource: __RESOURCE_URL__,
            headers: {
                ...setHeader(action.userAlias),
                ...filterCriteriaHeader,
                ...{ Xcv: newGuid },
                ...{ TenantId: action.tenantId },
                ...(delegatedUserAlias && { DelegatedUserAlias: delegatedUserAlias }),
            },
        });
        const summaryData =
            tenantSummaryResponse.data?.response?.['ApprovalSummaryData'] ??
            tenantSummaryResponse.data?.response?.['approvalSummaryData'] ??
            [];
        const filterCriteria = tenantSummaryResponse.data?.response?.['filterCriteria']?.form?.[0]?.titleMap;
        yield put(receivePullTenantSummary(summaryData));
        if (filterCriteria) {
            yield put(updatePullTenantSearchCriteria(filterCriteria));
        }
        if (!tenantSummaryResponse.data?.response && pullTenantSearchCriteria) {
            yield put(updatePullTenantSearchCriteria(null));
        }
        if (action.isExternalTenantInfoRequired && summaryData?.length > 0) {
            yield put(requestExternalTenantInto(action.tenantId, action.userAlias));
        }
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'PullTenantSummary - Success',
            'PullTenantSummary.Success',
            TrackingEventId.GetPullTenantSummarySuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const emptyResult: any = [];
        const error = errorResponse.data ?? errorResponse;
        const errorMessage =
            'Unable to fetch pending approvals at this time. If this issue persists, please contact support team with Tracking Id: ' +
            newGuid;
        yield put(receivePullTenantSummary(emptyResult));
        yield put(failedPullTenantSummary(errorMessage));
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'PullTenantSummary - Failure',
            'PullTenantSummary.Failure',
            TrackingEventId.GetPullTenantSummaryFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchExternalTenantInfo(action: IRequestExternalTenantInfo): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const newGuid = guid();
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data }: IHttpClientResult<any> = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/tenantInfo/${action.tenantId}`,
            resource: __RESOURCE_URL__,
            headers: { ...setHeader(null), ...{ tcv: newGuid } },
        });

        yield put(receiveExternalTenantInfo(data));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load external tenant info - Success',
            'MSApprovals.ExternalTenantInfo.Success',
            TrackingEventId.GetExternalTenantInfoSuccess,
            stateCommonProperties
        );
    } catch (errorResponse) {
        const error = (errorResponse as any).data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        const failureMessage =
            'Unable to fetch data at this time. If this issue persists, please contact support team with Tracking Id: ' +
            newGuid;
        yield put(failedExternalTenantInfo(failureMessage));
        trackException(
            authClient,
            telemetryClient,
            'Load external tenant info - Failure',
            'MSApprovals.ExternalTenantInfo.Failure',
            TrackingEventId.GetExternalTenantInfoFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchPullTenantSummaryCount(action: IRequestPullTenantSummaryCount): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/pulltenantsummarycount`,
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias, '', '', onBehalfUserUpn, onBehalfUserId),
        });
        let sum = 0;
        for (let i = 0; i < data.length; i++) {
            sum = sum + data[i].Count;
        }
        yield put(receivePullTenantSummaryCount(data, sum));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Get pull tenant summary count - Success',
            'MSApprovals.GetPullTenantSummaryCount.Success',
            TrackingEventId.GetPullTenantSummaryCountSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        yield put(receivePullTenantSummaryCount([], 0));
        const error = errorResponse.data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Get pull tenant summary count - Failure',
            'MSApprovals.GetPullTenantSummaryCount.Failure',
            TrackingEventId.GetPullTenantSummaryCountFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchQuickTourInfo(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
    const onBehalfUserId = yield select(getOnBehalfUserId);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const quickTourResponse: any = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/metadata/quicktour`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null, null, null, onBehalfUserUpn, onBehalfUserId),
        });
        let unreadQuickToursList: IQuickTourListItem[] = [];
        let readQuickToursList: IQuickTourListItem[] = [];
        const newQuickTours: Array<string> = [];

        quickTourResponse.data.quickTour.forEach((item: any) => {
            if (item.isEnabled) {
                // Only slide-bearing tours participate in the bulk-acknowledge list dismissed via the QuickTour dialog;
                // coachmark-only rows are acknowledged explicitly by their own UI.
                if (Array.isArray(item.slides) && item.slides.length > 0) {
                    newQuickTours.push(item.id.toString());
                }
                if (item.isViewed === false) {
                    unreadQuickToursList.push({
                        id: item.id,
                        name: item.name,
                        isViewed: item.isViewed ? true : false,
                        isEnabled: item.isEnabled,
                        slides: item.slides,
                        summary: item.summary,
                        summaryImage: item.summaryImage,
                    });
                } else {
                    readQuickToursList.push({
                        id: item.id,
                        name: item.name,
                        isViewed: item.isViewed ? true : false,
                        isEnabled: item.isEnabled,
                        slides: item.slides,
                        summary: item.summary,
                        summaryImage: item.summaryImage,
                    });
                }
            }
        });
        yield put(receiveQuickTourInfo(unreadQuickToursList, readQuickToursList, newQuickTours));
        yield put(setQuickTourData(unreadQuickToursList));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Fetch Quick Tour Info - Success',
            'MSApprovals.FetchQuickTourInfo.Success',
            TrackingEventId.QuickTourSuccess,
            stateCommonProperties
        );
    } catch (error: any) {
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Fetch Quick Tour Info - Failure',
            'MSApprovals.FetchQuickTourInfo.Failure',
            TrackingEventId.QuickTourFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* updateQuickTourInfo(action: IPostQuickTourInfo): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/user/preferences?SessionId=${telemetryClient.getCorrelationId()}`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
            data: { QuickTourFeatureList: `${JSON.stringify(action.unReadQuickTours)}` },
        });
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Update QuickTours - Success',
            'MSApprovals.UpdateQuickTourInfo.Success',
            TrackingEventId.UpdateQuickToursSuccess,
            stateCommonProperties
        );
    } catch (error: any) {
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Update QuickTours - Failure',
            'MSApprovals.UpdateQuickTourInfo.Failure',
            TrackingEventId.UpdateQuickToursFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* postFeedback(action: IPostFeedback): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');

        // Get feedback data from state using selector
        const feedbackData: IFeedbackData | undefined = yield select(
            getFeedbackByDocumentNumber,
            action.documentNumber
        );

        if (!feedbackData) {
            throw new Error(`No feedback data found for document number: ${action.documentNumber}`);
        }

        const requestData = {
            ...feedbackData,
            CustomStorageParameters: action.customStorageParameters,
        };
        yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/Feedback`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
            data: requestData,
        });
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Post Feedback - Success',
            'MSApprovals.PostFeedback.Success',
            TrackingEventId.PostFeedbackSuccess,
            stateCommonProperties
        );
    } catch (error: any) {
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Post Feedback - Failure',
            'MSApprovals.PostFeedback.Failure',
            TrackingEventId.PostFeedbackFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchSearchResults(action: IInitiateSearch): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);

    trackFeatureUsageEvent(
        authClient,
        telemetryClient,
        'Search - Initiated',
        'MSApprovals.Search.Initiated',
        TrackingEventId.SearchInitiated,
        stateCommonProperties
    );

    const summary = (yield select(getSummary)) || [];
    const isFlighted: boolean = yield select(getIsFeatureEnabledForUser, 'DeepSearch');

    // Local-search helper used by both the unflighted path and the API-failure fallback.
    const runLocalSearch = (): any[] => {
        const convertObjToValuesList = (obj: any): any[] => {
            const getValue = (key: any) => {
                const val = obj[key];
                if (val && typeof val === 'object') {
                    return convertObjToValuesList(val);
                }
                return val;
            };
            const vals = Object.keys(obj).map((key) => getValue(key));
            return vals;
        };
        const summaryVals = summary.map(convertObjToValuesList);
        const arrayHasItem = (arr: any[], searchInput: string): boolean => {
            const checkElement = (element: any) => {
                if (Array.isArray(element)) {
                    return arrayHasItem(element, searchInput);
                } else {
                    if (typeof element === 'string' || typeof element === 'number') {
                        const strElement = element.toString().toLowerCase();
                        return strElement.includes(searchInput.toLowerCase());
                    }
                    return false;
                }
            };
            const isInputInElement = (element: any) => checkElement(element);
            return arr.some(isInputInElement);
        };
        return summary.filter((item: any, index: string | number) =>
            arrayHasItem(summaryVals[index], action.userInput)
        );
    };

    // Unflighted users: legacy behavior — local first, fall back to DeepSearch API on 0 local results
    if (!isFlighted) {
        const filteredSum = runLocalSearch();
        if (filteredSum && filteredSum.length > 0) {
            yield put(saveSearchResults(filteredSum));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Search (local, unflighted) - Success',
                'MSApprovals.Search.LocalFirst.Success',
                TrackingEventId.DeepSearchSuccess,
                stateCommonProperties
            );
            return;
        }
        // Local returned nothing — fall through to the API call below
    }

    try {
        // DeepSearch API first
        const httpClient: IHttpClient = yield getContext('httpClient');
        const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
        const onBehalfUserId = yield select(getOnBehalfUserId);
        let url = `${__API_BASE_URL__}${__API_URL_ROOT__}/search?query=${encodeURIComponent(action.userInput)}`;
        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: url,
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias, '', '', onBehalfUserUpn, onBehalfUserId),
        });

        let filteredSum = data;
        // If data is an array of document numbers (legacy), filter summary accordingly
        if (Array.isArray(data) && data.length > 0 && typeof data[0] === 'string') {
            filteredSum = summary.filter(
                (item: { ApprovalIdentifier: any; DisplayDocumentNumber: { toString: () => any } }) =>
                    item.ApprovalIdentifier?.DisplayDocumentNumber != null &&
                    data.includes(item.ApprovalIdentifier.DisplayDocumentNumber.toString())
            );
        } else if (Array.isArray(data) && data.length > 0 && typeof data[0] === 'object' && data[0] !== null) {
            // Enriched SearchResultDto[] — merge match metadata onto summary items
            // Preserve backend result order (ranked by relevance).
            // Backend may use either camelCase or PascalCase for DocumentNumber depending on
            // serializer config; fall back explicitly rather than assuming one casing.
            const summaryMap = new Map(
                summary
                    .filter((item: any) => item.ApprovalIdentifier?.DisplayDocumentNumber != null)
                    .map((item: any) => [item.ApprovalIdentifier.DisplayDocumentNumber.toString(), item])
            );
            filteredSum = data
                .filter((r: any) => summaryMap.has(r.documentNumber ?? r.DocumentNumber))
                .map((r: any) => {
                    const item = summaryMap.get(r.documentNumber ?? r.DocumentNumber);
                    return {
                        ...item,
                        _matchMetadata: normalizeSearchResult(r),
                    };
                });
        }

        yield put(saveSearchResults(filteredSum));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Deep search (api) - Success',
            'MSApprovals.DeepSearch.Api.Success',
            TrackingEventId.DeepSearchSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        // DeepSearch API failed — fall back to local search
        try {
            const filteredSum = runLocalSearch();

            yield put(saveSearchResults(filteredSum && filteredSum.length > 0 ? filteredSum : null));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Deep search (local fallback) - Success',
                'MSApprovals.DeepSearch.LocalFallback.Success',
                TrackingEventId.DeepSearchSuccess,
                stateCommonProperties
            );
        } catch (fallbackError: any) {
            yield put(saveSearchResults(null));
        }

        const error = errorResponse.data ?? errorResponse;
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Deep search - Failure',
            'MSApprovals.DeepSearch.Failure',
            TrackingEventId.DeepSearchFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchInsights(action: IRequestInsights): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        let url = `${__API_BASE_URL__}${__API_URL_ROOT__}/me/insights`;
        if (action.pageType === 'history') {
            url += `?page=${action.pageType}&timePeriod=${action.timePeriod ?? 6}`;
        }
        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: url,
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
        });
        const summaryInsightsResults = action?.pageType === 'history' ? null : data;
        const historyInsightsResults = action?.pageType === 'history' ? data : null;
        yield put(receiveInsights(summaryInsightsResults, historyInsightsResults));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Get insights - Success',
            'MSApprovals.GetInsights.Success',
            TrackingEventId.GetInsightsSuccess,
            stateCommonProperties
        );
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse; // handling cases where error occurs outside of httpclient call
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Get insights - Failure',
            'MSApprovals.GetInsights.Failure',
            TrackingEventId.GetInsightsFailure,
            stateCommonProperties,
            exception
        );
    }
}

function* fetchSuggestResults(action: IRequestSuggest): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);

    // Debounce: wait before fetching so rapid keystrokes don't flood the API
    yield delay(150);

    if (!action.query || action.query.length < 3) {
        yield put(clearSuggest());
        return;
    }

    // Skip API call if cache covers this query — selector filters client-side
    // But re-fetch if filtered results are too thin (< 2 total items)
    const cache: ReturnType<typeof getSuggestCache> = yield select(getSuggestCache);
    if (cache && action.query.toLowerCase().startsWith(cache.query.toLowerCase())
        && action.query.toLowerCase() !== cache.query.toLowerCase()) {
        // If cache already had 0 results, extending the query won't help — stop re-fetching
        if (cache.terms.length === 0 && cache.requests.length === 0) {
            yield put(receiveSuggest(action.query, [], []));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Search suggest - Cache skipped (empty)',
                'MSApprovals.SearchSuggest.CacheSkippedEmpty',
                TrackingEventId.SearchSuggestCacheSkippedEmpty,
                stateCommonProperties,
                { Query: action.query, CachedQuery: cache.query }
            );
            return;
        }
        const lq = action.query.toLowerCase();
        // Defensive: coerce to string before lowercasing in case backend returns non-string field
        const fieldIncludes = (value: unknown): boolean =>
            String(value ?? '').toLowerCase().includes(lq);
        const filteredTerms = cache.terms.filter((t: string) => fieldIncludes(t));
        const filteredRequests = cache.requests.filter(
            (r: any) =>
                fieldIncludes(r.documentNumber) ||
                fieldIncludes(r.displayDocumentNumber) ||
                fieldIncludes(r.title) ||
                fieldIncludes(r.submitterName) ||
                fieldIncludes(r.appName)
        );
        if (filteredTerms.length + filteredRequests.length >= 2) {
            // Enough cached results — skip API call
            yield put(receiveSuggest(cache.query, cache.terms, cache.requests));
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Search suggest - Cache hit',
                'MSApprovals.SearchSuggest.CacheHit',
                TrackingEventId.SearchSuggestCacheHit,
                stateCommonProperties,
                {
                    Query: action.query,
                    CachedQuery: cache.query,
                    FilteredTermCount: filteredTerms.length,
                    FilteredRequestCount: filteredRequests.length,
                }
            );
            return;
        }
        // Thin results — fall through to make a fresh API call
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Search suggest - Cache refetch (thin results)',
            'MSApprovals.SearchSuggest.CacheRefetch',
            TrackingEventId.SearchSuggestCacheRefetch,
            stateCommonProperties,
            {
                Query: action.query,
                CachedQuery: cache.query,
                FilteredTermCount: filteredTerms.length,
                FilteredRequestCount: filteredRequests.length,
            }
        );
    }

    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const userAlias: string = yield select(getUserAlias);
        const onBehalfUserUpn = yield select(getOnBehalfUserUpn);
        const onBehalfUserId = yield select(getOnBehalfUserId);
        const tcv = guid();

        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/search/suggest?query=${encodeURIComponent(action.query)}`,
            resource: __RESOURCE_URL__,
            headers: setHeader(userAlias, tcv, '', onBehalfUserUpn, onBehalfUserId),
        });

        const terms = data?.terms || data?.Terms || [];
        const requests = data?.requests || data?.Requests || [];
        yield put(receiveSuggest(action.query, terms, requests));

        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Search suggest - Success',
            'MSApprovals.SearchSuggest.Success',
            TrackingEventId.SearchInitiated,
            stateCommonProperties,
            { ResultCount: terms.length + requests.length, Query: action.query }
        );
    } catch (error: any) {
        yield put(clearSuggest());
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Search suggest - Failure',
            'MSApprovals.SearchSuggest.Failure',
            TrackingEventId.DeepSearchFailure,
            stateCommonProperties,
            exception
        );
    }
}

export function* sharedComponentsSagas(): IterableIterator<{}> {
    yield all([
        takeLatest(SharedComponentsActionType.REQUEST_MY_PROFILE, fetchProfile),
        takeLatest(SharedComponentsActionType.REQUEST_MY_DELEGATIONS, fetchDelegations),
        takeLatest(SharedComponentsActionType.REQUEST_MY_SUMMARY, fetchSummary),
        takeLatest(SharedComponentsActionType.REQUEST_MY_HISTORY, fetchHistory),
        takeLatest(SharedComponentsActionType.REQUEST_DOWNLOAD_HISTORY, downloadHistory),
        takeLatest(SharedComponentsActionType.REQUEST_DOWNLOAD_SUMMARY, downloadSummary),
        takeLatest(SharedComponentsActionType.REQUEST_MY_OUT_OF_SYNC_SUMMARY, fetchOutOfSyncSummary),
        takeLatest(SharedComponentsActionType.REQUEST_TENANT_INFO, fetchTenantInfo),
        takeLatest(SharedComponentsActionType.REQUEST_FILTERED_USERS, fetchFilteredUsers),
        takeLatest(SharedComponentsActionType.SAVE_USER_PREFERENCES_REQUEST, saveUserPreferences),
        takeLatest(SharedComponentsActionType.REQUEST_USER_PREFERENCES, fetchUserPreferences),
        takeLatest(SharedComponentsActionType.REQUEST_PULL_TENANT_SUMMARY, fetchPullTenantSummary),
        takeLatest(SharedComponentsActionType.REQUEST_EXTERNAL_TENANT_INFO, fetchExternalTenantInfo),
        takeLatest(SharedComponentsActionType.REQUEST_PULLTENANT_SUMMARY_COUNT, fetchPullTenantSummaryCount),
        takeLatest(SharedComponentsActionType.INITIATE_SEARCH, fetchSearchResults),
        takeLatest(SharedComponentsActionType.REQUEST_QUICKTOUR_INFO, fetchQuickTourInfo),
        takeLatest(SharedComponentsActionType.POST_QUICKTOUR_INFO, updateQuickTourInfo),
        takeLatest(SharedComponentsActionType.REQUEST_FLIGHTING_DATA, fetchFlightingData),
        takeLatest(SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES, subscribeFlightingFeaturesSaga),
        takeLatest(SharedComponentsActionType.UNSUBSCRIBE_FLIGHTING_FEATURES, unsubscribeFlightingFeaturesSaga),
        takeLatest(SharedComponentsActionType.SUBMIT_FLIGHTING_FEATURE_FEEDBACK, submitFlightingFeatureFeedbackSaga),
        takeLatest(SharedComponentsActionType.REQUEST_INSIGHTS, fetchInsights),
        takeLatest(SharedComponentsActionType.POST_FEEDBACK, postFeedback),
        takeLatest(SharedComponentsActionType.REQUEST_SUGGEST, fetchSuggestResults),
    ]);
}
