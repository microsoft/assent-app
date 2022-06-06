import { SimpleEffect, Effect, getContext, put, all, call, takeLatest, select } from 'redux-saga/effects';
import { IAuthClient } from '@micro-frontend-react/employee-experience/lib/IAuthClient';
import { ITelemetryClient } from '@micro-frontend-react/employee-experience/lib/ITelemetryClient';
import { guid } from '../../Helpers/Guid';
import {
    IRequestDownloadHistory,
    IRequestDelegationsAction,
    IRequestExternalTenantInfo,
    IRequestFilteredUsersAction,
    IRequestMyHistoryAction,
    IRequestPullTenantSummary,
    IRequestPullTenantSummaryCount,
    IRequestSummaryAction,
    ISaveUserPreferencesRequest,
    SharedComponentsActionType
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
    requestExternalTenantInto
} from './SharedComponents.actions';
import { saveAs } from 'file-saver';
import {
    getStateCommonTelemetryProperties,
    getSubmitterImages,
    getSelectedTenantDelegation,
    getPullTenantSearchCriteria,
} from './SharedComponents.selectors';
import { trackBusinessProcessEvent, trackException, TrackingEventId } from '../../Helpers/telemetryHelpers';
import { setHeader } from './Components/SagasHelper';
import { IDelegationObj, IGraphPhoto } from './SharedComponents.types';
import {
    IHttpClient,
    IHttpClientResult,
    IHttpClientRequest,
} from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { safeJSONParse } from '../../Helpers/sharedHelpers';

function* fetchProfile(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: profile }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__GRAPH_BASE_URL__}/me`,
            resource: __GRAPH_RESOURCE_URL__
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
            resource: __GRAPH_RESOURCE_URL__
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
        let url = `${__API_BASE_URL__}${__API_URL_ROOT__}/userDelegationSettings`;
        if (action.tenantId) {
            url = url + `?tenantMapId=${action.tenantId}&`;
        } else if (!action.tenantId) {
            url += '?';
        }
        url = url + `loggedInAlias=${loggedInAlias}`;
        const { data: delegations }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: url,
            resource: __RESOURCE_URL__,
            headers: setHeader(null)
        });
        const delegationList = delegations ? delegations[0]['delegations'] : null;
        if (action.tenantId && action.appName) {
            let tenantDelegationObj = null;
            if (delegationList && delegationList.length > 0) {
                tenantDelegationObj = {
                    tenantId: action.tenantId,
                    appName: action.appName,
                    delegations: delegationList
                };
            }
            yield put(receiveTenantDelegations(tenantDelegationObj));
        } else {
            yield put(receiveMyDelegations(delegationList));
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
            responseType: 'blob'
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
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: summary }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/summary`,
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias)
        });
        yield put(receiveMySummary(summary));
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Load Summary - Success',
            'MSApprovals.LoadSummary.Success',
            TrackingEventId.SummaryLoadSuccess,
            stateCommonProperties
        );
        let submitters: string[] = [];
        for (let i = 0; i < summary.length; i++) {
            const submitter = summary[i]?.Submitter?.Alias;
            if (!submitters.includes(submitter)) {
                submitters.push(submitter);
            }
        }
        const graphResponses = yield all(submitters.map(sub => call(fetchGraphData, sub)));
        yield put(receiveSubmitterImages(graphResponses));
    } catch (errorResponse: any) {
        const error = errorResponse.data ?? errorResponse;
        yield put(failedSummary(error.message ? error.message : error));
        const exception = error.message ? new Error(error.message) : error;
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
            headers: setHeader(null)
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
            const matchingElement = currentImages?.find(el => el.alias === submitter);
            if (!matchingElement && !newSubmitters.includes(submitter)) {
                newSubmitters.push(submitter);
            }
        }
        const newGraphResponses = yield all(newSubmitters.map(sub => call(fetchGraphData, sub)));
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
            headers: setHeader(null)
        });
        const blob = new Blob([historyDownload], { type: 'text/csv;charset=utf-8' });
        saveAs(blob, 'history.csv');
        yield put(receiveDownloadHistory())
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

function* fetchTenantInfo(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data: tenantInfo }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/TenantInfo`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null)
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
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const { data: outOfSyncSummary }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/outofsyncsummary`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null)
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
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const result: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/userpreference?SessionId=${telemetryClient.getCorrelationId()}`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
            data: action.data
        });
        yield put(SaveUserPreferencesResponse('Feature Preferences has been submitted successfully.'));
        yield put(RequestUserPreferences());
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

function* fetchUserPreferences(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/userpreference?SessionId=${telemetryClient.getCorrelationId()}`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null)
        });

        const featurePreferenceJson = safeJSONParse(data?.featurePreferenceJson);

        if (featurePreferenceJson) {
            yield put(ReceiveUserPreferences(featurePreferenceJson));
        }
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
            'weekRange.endDate': action.filterCriteria.endDate
        };
        const filterObjectString = filterObject && JSON.stringify(filterObject);
        const filterCriteriaHeader = action.filterCriteria && {
            FilterParameters: filterObjectString
        };
        const tenantSummaryResponse: any = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/PullTenant/${action.tenantId}`,
            resource: __RESOURCE_URL__,
            headers: {
                ...setHeader(action.userAlias),
                ...filterCriteriaHeader,
                ...{ Xcv: newGuid },
                ...{ TenantId: action.tenantId },
                ...(delegatedUserAlias && { DelegatedUserAlias: delegatedUserAlias })
            }
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
            headers: { ...setHeader(null), ...{ tcv: newGuid } }
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
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const { data }: IHttpClientRequest = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/pulltenantsummarycount`,
            resource: __RESOURCE_URL__,
            headers: setHeader(action.userAlias)
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

export function* sharedComponentsSagas(): IterableIterator<{}> {
    yield all([
        takeLatest(SharedComponentsActionType.REQUEST_MY_PROFILE, fetchProfile),
        takeLatest(SharedComponentsActionType.REQUEST_MY_DELEGATIONS, fetchDelegations),
        takeLatest(SharedComponentsActionType.REQUEST_MY_SUMMARY, fetchSummary),
        takeLatest(SharedComponentsActionType.REQUEST_MY_HISTORY, fetchHistory),
        takeLatest(SharedComponentsActionType.REQUEST_DOWNLOAD_HISTORY, downloadHistory),
        takeLatest(SharedComponentsActionType.REQUEST_MY_OUT_OF_SYNC_SUMMARY, fetchOutOfSyncSummary),
        takeLatest(SharedComponentsActionType.REQUEST_TENANT_INFO, fetchTenantInfo),
        takeLatest(SharedComponentsActionType.REQUEST_FILTERED_USERS, fetchFilteredUsers),
        takeLatest(SharedComponentsActionType.SAVE_USER_PREFERENCES_REQUEST, saveUserPreferences),
        takeLatest(SharedComponentsActionType.REQUEST_USER_PREFERENCES, fetchUserPreferences),
        takeLatest(SharedComponentsActionType.REQUEST_PULL_TENANT_SUMMARY, fetchPullTenantSummary),
        takeLatest(SharedComponentsActionType.REQUEST_EXTERNAL_TENANT_INFO, fetchExternalTenantInfo),
        takeLatest(SharedComponentsActionType.REQUEST_PULLTENANT_SUMMARY_COUNT, fetchPullTenantSummaryCount)
    ]);
}
