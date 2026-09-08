import { ISharedComponentsState, IFeedbackData } from './SharedComponents.types';
import { SharedComponentsAction, SharedComponentsActionType } from './SharedComponents.action-types';
import { GroupingBy } from './Components/GroupingBy';
import {
    DETAILS_DEFAULT_VIEW,
    GROUP_BY_FILTER,
    HISTORY_DEFAULT_VIEW,
    DOCKED_VIEW,
    FLYOUT_VIEW,
    DEFAULT_TENANT,
    CARD_VIEW,
    DEFAULT_VIEW_TYPE,
} from './SharedConstants';

export const sharedComponentsReducerName = 'SharedComponentsReducer';

export const sharedComponentsInitialState: ISharedComponentsState = {
    profile: null,
    profileErrorMessage: null,
    profileHasError: false,
    summary: [],
    history: null,
    outOfSyncSummary: [],
    isLoading: false,
    isLoadingSummary: false,
    isLoadingOutOfSyncSummary: false,
    isLoadingTenantInfo: false,
    isLoadingProfile: false,
    hasError: false,
    outOfSyncHasError: false,
    errorMessage: null,
    outOfSyncErrorMessage: null,
    isPanelOpen: false,
    tenantInfo: null,
    tenantInfoHasError: false,
    selectedSummary: 'pending',
    historyGroupedBy: GroupingBy.Tenant,
    summaryErrorMessage: null,
    historyErrorMessage: null,
    tenantInfoErrorMessage: null,
    selectedPage: 'summary',
    isLoadingHistory: false,
    isDownloadingHistory: false,
    isDownloadingSummary: false,
    historySelectedPage: 1,
    sortColumnField: 'ActionDate',
    sortDirection: 'DESC',
    historySearchCriteria: '',
    historyTimePeriod: 3,
    historyData: [],
    tenantList: [],
    summaryGroupedBy: GroupingBy.Tenant,
    filterValue: 'All',
    bulkFailedMsg: [],
    isBulkSelected: false,
    bulkApproveFailed: false,
    bulkApproveStatus: false,
    isProcessingBulkApproval: false,
    selectedApprovalRecords: [],
    historyTenantIdFilter: '',
    userDelegations: [],
    delegationsHasError: false,
    delegationsErrorMessage: null,
    historyHasError: false,
    toggleDetailsScreen: false,
    selectedSummaryTileRef: null,
    historyTotalRecords: 0,
    filteredUsers: null,
    historyDownloadHasError: false,
    historyDownloadErrorMessage: null,
    summaryDownloadHasError: false,
    summaryDownloadErrorMessage: null,
    isSettingPanelOpen: false,
    userPreferences: [],
    userPreferencesSuccessMessage: null,
    userPreferencesFailureMessage: null,
    detailsDefaultView: DOCKED_VIEW,
    historyDefaultView: FLYOUT_VIEW,
    defaultViewType: CARD_VIEW,
    DefaultTenant: null,
    bulkActionConcurrentCall: 0,
    peoplePickerSelections: [],
    peoplePickerHasError: false,
    submitterImages: [],
    isLoadingSubmitterImages: false,
    pullTenantSummaryData: [],
    isPaginationEnabled: false,
    isBulkSelectionRetained: false,
    externalTenantInfo: null,
    externalTenantInfoHasError: false,
    externalTenantInfoErrorMessage: null,
    isLoadingPullTenantData: false,
    pullTenantSummaryHasError: false,
    pullTenantSummaryErrorMessage: null,
    failedPullTenantRequests: [],
    pullTenantSearchCriteria: null,
    pullTenantSearchSelection: 0,
    tableRowCount: 0,
    pullTenantSummaryCount: [],
    totalPullTenantCount: 0,
    tenantDelegations: null,
    selectedTenantDelegation: null,
    isLoadingPullTenantSummaryCount: false,
    successfulPullTenantRequests: [],
    successfulPullTenantCount: 0,
    successfulPullTenantCountDict: {},
    isProfilePanelOpen: false,
    isAccessibilityPanelOpen: false,
    isLoadingSearchResults: false,
    searchResults: null,
    isSearchResultsViewOpen: false,
    isQuickTourOpen: false,
    quickTourData: [],
    unreadQuickTours: [],
    readQuickTours: [],
    updatedQuickTourList: [],
    myFlightingData: null,
    allFlightingData: null,
    isLoadingFlightingData: false,
    flightingFeatureFeedback: {},
    insightsData: { summaryInsights: null, historyInsights: null },
    feedback: {
        feedbackByFeature: {},
        isLoading: false,
        hasError: false,
        errorMessage: null,
    },
    isSummaryCollapsed: false,
    propertyFilters: {},
    isSubmitterView: false,
    digestPreference: null,
    teamsNotificationsEnabled: null,
    suggestTerms: [],
    suggestRequests: [],
    isLoadingSuggest: false,
    suggestCache: null,
};

export function sharedComponentsReducer(
    prev: ISharedComponentsState = sharedComponentsInitialState,
    action: SharedComponentsAction
): ISharedComponentsState {
    switch (action.type) {
        case SharedComponentsActionType.REQUEST_MY_PROFILE:
            return {
                ...prev,
                isLoading: true,
                hasError: false,
            };
        case SharedComponentsActionType.RECEIVE_MY_PROFILE:
            return {
                ...prev,
                isLoading: false,
                hasError: false,
                profile: action.profile,
            };
        case SharedComponentsActionType.FAILED_PROFILE:
            return {
                ...prev,
                isLoadingProfile: false,
                profileHasError: true,
                profileErrorMessage: action.profileErrorMessage,
            };
        case SharedComponentsActionType.REQUEST_MY_DELEGATIONS:
            return {
                ...prev,
                delegationsHasError: false,
            };
        case SharedComponentsActionType.RECEIVE_MY_DELEGATIONS:
            return {
                ...prev,
                userDelegations: action.userDelegations,
            };
        case SharedComponentsActionType.REQUEST_MY_SUMMARY:
            return {
                ...prev,
                isLoadingSummary: true,
                hasError: false,
                summaryErrorMessage: null,
                propertyFilters: {},
                isSubmitterView: action.isSubmittedRequest ?? false,
            };
        case SharedComponentsActionType.RECEIVE_MY_SUMMARY:
            return {
                ...prev,
                isLoadingSummary: false,
                hasError: false,
                summaryErrorMessage: null,
                summary: action.summary,
                isLoadingSubmitterImages: true,
            };
        case SharedComponentsActionType.REQUEST_MY_OUT_OF_SYNC_SUMMARY:
            return {
                ...prev,
                isLoadingOutOfSyncSummary: true,
                outOfSyncHasError: false,
            };
        case SharedComponentsActionType.RECEIVE_MY_OUT_OF_SYNC_SUMMARY:
            return {
                ...prev,
                isLoadingOutOfSyncSummary: false,
                outOfSyncHasError: false,
                outOfSyncSummary: action.outOfSyncSummary,
            };
        case SharedComponentsActionType.FAILED_OUT_OF_SYNC_SUMMARY:
            return {
                ...prev,
                isLoadingOutOfSyncSummary: false,
                outOfSyncHasError: true,
                outOfSyncErrorMessage: action.outOfSyncErrorMessage,
            };
        case SharedComponentsActionType.FAILED_SUMMARY:
            return {
                ...prev,
                isLoadingSummary: false,
                hasError: true,
                summaryErrorMessage: action.summaryErrorMessage,
            };
        case SharedComponentsActionType.REQUEST_TENANT_INFO:
            return {
                ...prev,
                isLoadingTenantInfo: true,
                tenantInfoHasError: false,
            };
        case SharedComponentsActionType.RECEIVE_TENANT_INFO:
            return {
                ...prev,
                isLoadingTenantInfo: false,
                tenantInfoHasError: false,
                tenantInfo: action.tenantInfo,
            };
        case SharedComponentsActionType.FAILED_TENANT_INFO:
            return {
                ...prev,
                isLoadingTenantInfo: false,
                tenantInfoHasError: true,
                tenantInfoErrorMessage: action.tenantInfoErrorMessage,
            };
        case SharedComponentsActionType.UPDATE_PANEL_STATE:
            return {
                ...prev,
                isPanelOpen: action.isOpen,
            };
        case SharedComponentsActionType.UPDATE_SELECTED_SUMMARY_TO_PENDING:
            return {
                ...prev,
                selectedSummary: 'pending',
            };
        case SharedComponentsActionType.UPDATE_SELECTED_SUMMARY_TO_OUT_OF_SYNC:
            return {
                ...prev,
                selectedSummary: 'outofsync',
            };
        case SharedComponentsActionType.UPDATE_GROUPED_SUMMARY:
            return {
                ...prev,
                summaryGroupedBy: action.summaryGroupedBy,
                isPanelOpen: false,
                filterValue: 'All',
            };
        case SharedComponentsActionType.UPDATE_FILTER_VALUE:
            return {
                ...prev,
                isPanelOpen: false,
                filterValue: action.filterValue,
                pullTenantSummaryData: null,
                pullTenantSearchCriteria: null,
                pullTenantSearchSelection: 0,
                pullTenantSummaryHasError: false,
                pullTenantSummaryErrorMessage: null,
                failedPullTenantRequests: [],
                tenantDelegations: null,
                selectedTenantDelegation: null,
                externalTenantInfoErrorMessage: null,
                externalTenantInfoHasError: false,
                externalTenantInfo: null,
                bulkApproveStatus: false,
                propertyFilters: {},
            };
        case SharedComponentsActionType.UPDATE_BULK_UPLOAD_CONCURRENT_VALUE:
            return {
                ...prev,
                bulkActionConcurrentCall: action.bulkUploadConcurrentValue,
            };
        case SharedComponentsActionType.UPDATE_BULK_FAILED_VALUE:
            return {
                ...prev,
                bulkFailedMsg: action.bulkFailedMessage,
            };
        case SharedComponentsActionType.UPDATE_BULK_SELECTED:
            return {
                ...prev,
                isPanelOpen: false,
                isBulkSelected: action.isBulkSelected,
            };
        case SharedComponentsActionType.SAVE_BULK_APPROVAL_REQUEST:
            return {
                ...prev,
                isPanelOpen: false,
            };
        case SharedComponentsActionType.UPDATE_BULK_FAILED:
            return {
                ...prev,
                bulkApproveFailed: action.bulkApproveFailed,
            };
        case SharedComponentsActionType.UPDATE_BULK_STATUS:
            return {
                ...prev,
                isPanelOpen: false,
                bulkApproveStatus: action.bulkStatus,
            };
        case SharedComponentsActionType.UPDATE_IS_PROCESSING_BULK_APPROVAL:
            return {
                ...prev,
                isProcessingBulkApproval: action.isProcessingBulkApproval,
                bulkApproveFailed: action.isProcessingBulkApproval ? false : prev.bulkApproveFailed,
            };
        case SharedComponentsActionType.UPDATE_APPROVAL_RECORDS:
            switch (action.subAction) {
                case 'Push':
                    return {
                        ...prev,
                        selectedApprovalRecords: [...prev.selectedApprovalRecords.concat(action.approveRecords)],
                    };
                default:
                    return {
                        ...prev,
                        selectedApprovalRecords: action.approveRecords,
                    };
            }
        case SharedComponentsActionType.REQUEST_MY_HISTORY:
            return {
                ...prev,
                historyHasError: false,
                isLoadingHistory: true,
                historySelectedPage: action.page,
                sortColumnField: action.sortColumn,
                sortDirection: action.sortDirection,
                historySearchCriteria: action.searchCriteria,
                historyTimePeriod: action.timePeriod,
                historyTenantIdFilter: action.tenantId,
                filterValue: 'All',
            };
        case SharedComponentsActionType.RECEIVE_MY_HISTORY:
            return {
                ...prev,
                historyHasError: false,
                history: action.history,
                isLoadingHistory: false,
                isLoadingSubmitterImages: true,
            };
        case SharedComponentsActionType.FAILED_HISTORY:
            return {
                ...prev,
                historyHasError: true,
                historyErrorMessage: action.historyErrorMessage,
            };
        case SharedComponentsActionType.UPDATE_SELECTED_PAGE:
            return {
                ...prev,
                selectedPage: action.currentPage,
                pullTenantSummaryData: null,
                pullTenantSearchCriteria: null,
                pullTenantSearchSelection: 0,
                pullTenantSummaryHasError: false,
                pullTenantSummaryErrorMessage: null,
                failedPullTenantRequests: [],
                tenantDelegations: null,
                selectedTenantDelegation: null,
                externalTenantInfoErrorMessage: null,
                externalTenantInfoHasError: false,
                externalTenantInfo: null,
            };
        case SharedComponentsActionType.UPDATE_GROUPED_HISTORY:
            return {
                ...prev,
                historyGroupedBy: action.historyGroupedBy,
            };
        case SharedComponentsActionType.UPDATE_HISTORY_DATA:
            return {
                ...prev,
                historyData: action.historyData,
                historyTotalRecords: action.totalRecords,
                tenantList: action.tenantList,
            };
        case SharedComponentsActionType.TOGGLE_DETAIL_SCREEN:
            return {
                ...prev,
                toggleDetailsScreen: action.isOpen != null ? action.isOpen : !prev.toggleDetailsScreen,
            };
        case SharedComponentsActionType.RECEIVE_FILTERED_USERS:
            return {
                ...prev,
                filteredUsers: action.filteredUsers,
            };
        case SharedComponentsActionType.SET_SELECTED_SUMMARY_TILE_REF:
            return {
                ...prev,
                selectedSummaryTileRef: action.tileRef,
            };
        case SharedComponentsActionType.REQUEST_DOWNLOAD_HISTORY:
            return {
                ...prev,
                isDownloadingHistory: true,
                historyDownloadHasError: false,
                historyDownloadErrorMessage: null,
            };
        case SharedComponentsActionType.RECEIVE_DOWNLOAD_HISTORY:
            return {
                ...prev,
                isDownloadingHistory: false,
                historyDownloadHasError: false,
                historyDownloadErrorMessage: null,
            };
        case SharedComponentsActionType.FAILED_DOWNLOAD_HISTORY:
            return {
                ...prev,
                historyDownloadHasError: true,
                historyDownloadErrorMessage: action.downloadErrorMessage,
                isDownloadingHistory: false,
            };
        case SharedComponentsActionType.REQUEST_DOWNLOAD_SUMMARY:
            return {
                ...prev,
                isDownloadingSummary: true,
                summaryDownloadHasError: false,
                summaryDownloadErrorMessage: null,
            };
        case SharedComponentsActionType.RECEIVE_DOWNLOAD_SUMMARY:
            return {
                ...prev,
                isDownloadingSummary: false,
                summaryDownloadHasError: false,
                summaryDownloadErrorMessage: null,
            };
        case SharedComponentsActionType.FAILED_DOWNLOAD_SUMMARY:
            return {
                ...prev,
                summaryDownloadHasError: true,
                summaryDownloadErrorMessage: action.downloadErrorMessage,
                isDownloadingSummary: false,
            };
        case SharedComponentsActionType.CLEAR_DOWNLOAD_SUMMARY_ERROR:
            return {
                ...prev,
                summaryDownloadHasError: false,
                summaryDownloadErrorMessage: null,
            };
        case SharedComponentsActionType.TOGGLE_SETTINGS_PANEL:
            return {
                ...prev,
                isSettingPanelOpen: action.toggle,
            };
        case SharedComponentsActionType.RECEIVE_USER_PREFERENCES:
            if (action.preserveSessionState) {
                return {
                    ...prev,
                    userPreferences: action.data,
                };
            }
            let summaryGroupedBy = prev.summaryGroupedBy || GroupingBy.Tenant;
            let detailsDefaultView = prev.detailsDefaultView || DOCKED_VIEW;
            let historyDefaultView = prev.historyDefaultView || FLYOUT_VIEW;
            let defaultViewType = prev.defaultViewType || CARD_VIEW;
            let DefaultTenant = prev.DefaultTenant || '';
            if (action.data && action.data.length > 0) {
                const _groupByFilter = action.data.find((u: any) => u.UserPreferenceText === GROUP_BY_FILTER);
                if (_groupByFilter) {
                    summaryGroupedBy = _groupByFilter.UserPreferenceStatus;
                }

                const detailView = action.data.find((u: any) => u.UserPreferenceText === DETAILS_DEFAULT_VIEW);
                if (detailView) {
                    detailsDefaultView = detailView.UserPreferenceStatus;
                }

                const historyView = action.data.find((u: any) => u.UserPreferenceText === HISTORY_DEFAULT_VIEW);
                if (historyView) {
                    historyDefaultView = historyView.UserPreferenceStatus;
                }

                const viewType = action.data.find((u: any) => u.UserPreferenceText === DEFAULT_VIEW_TYPE);
                if (viewType) {
                    defaultViewType = viewType.UserPreferenceStatus;
                }

                const defaultTenant = action.data.find((u: any) => u.UserPreferenceText === DEFAULT_TENANT);
                if (defaultTenant) {
                    DefaultTenant = defaultTenant.UserPreferenceStatus;
                }
            }
            return {
                ...prev,
                userPreferences: action.data,
                summaryGroupedBy: summaryGroupedBy,
                isPanelOpen: false,
                detailsDefaultView: detailsDefaultView,
                historyDefaultView: historyDefaultView,
                DefaultTenant: DefaultTenant,
                defaultViewType: defaultViewType,
                digestPreference: action.digestPreference || prev.digestPreference,
                teamsNotificationsEnabled: action.teamsNotificationsEnabled ?? prev.teamsNotificationsEnabled,
            };
        case SharedComponentsActionType.SAVE_USER_PREFERENCES_REQUEST:
            return {
                ...prev,
                userPreferencesSuccessMessage: null,
                userPreferencesFailureMessage: null,
            };
        case SharedComponentsActionType.SAVE_USER_PREFERENCES_RESPONSE:
            return {
                ...prev,
                userPreferencesSuccessMessage: action.message,
            };
        case SharedComponentsActionType.SAVE_USER_PREFERENCES_FAILED:
            return {
                ...prev,
                userPreferencesFailureMessage: action.message,
            };
        case SharedComponentsActionType.CLEAR_USER_PREFERENCES_API_MESSAGES:
            return {
                ...prev,
                userPreferencesFailureMessage: null,
                userPreferencesSuccessMessage: null,
            };
        case SharedComponentsActionType.UPDATE_PEOPLEPICKER_SELECTION:
            return {
                ...prev,
                peoplePickerSelections: action.peoplePickerSelections,
            };
        case SharedComponentsActionType.UPDATE_PEOPLEPICKER_HASERROR:
            return {
                ...prev,
                peoplePickerHasError: action.peoplePickerHasError,
            };
        case SharedComponentsActionType.RECEIVE_SUBMITTER_IMAGES:
            return {
                ...prev,
                submitterImages: action.submitterImages,
                isLoadingSubmitterImages: false,
            };
        case SharedComponentsActionType.CONCAT_SUBMITTER_IMAGES:
            return {
                ...prev,
                submitterImages: prev.submitterImages?.concat(action.newSubmitterImages) ?? action.newSubmitterImages,
                isLoadingSubmitterImages: false,
            };
        case SharedComponentsActionType.REQUEST_PULL_TENANT_SUMMARY:
            return {
                ...prev,
                isLoadingPullTenantData: true,
                pullTenantSummaryHasError: false,
                isPanelOpen: false,
                bulkApproveStatus: false,
            };
        case SharedComponentsActionType.RECEIVE_PULL_TENANT_SUMMARY:
            return {
                ...prev,
                isLoadingPullTenantData: false,
                pullTenantSummaryData: action.pullTenantSummary,
                successfulPullTenantRequests: [],
            };
        case SharedComponentsActionType.FAILED_PULL_TENANT_SUMMARY:
            return {
                ...prev,
                isLoadingPullTenantData: false,
                pullTenantSummaryHasError: true,
                pullTenantSummaryErrorMessage: action.errorMessage,
            };
        case SharedComponentsActionType.REFRESH_BULK_STATE:
            return {
                ...prev,
                bulkApproveStatus: false,
                selectedApprovalRecords: [],
                bulkApproveFailed: false,
            };
        case SharedComponentsActionType.UPDATE_RETAIN_BULK_SELECTION:
            return {
                ...prev,
                isBulkSelectionRetained: action.isBulkSelectionRetained,
            };
        case SharedComponentsActionType.REQUEST_EXTERNAL_TENANT_INFO:
            return {
                ...prev,
                externalTenantInfoHasError: false,
                externalTenantInfoErrorMessage: null,
            };
        case SharedComponentsActionType.RECEIVE_EXTERNAL_TENANT_INFO:
            return {
                ...prev,
                externalTenantInfo: action.externalTenantInfo,
            };
        case SharedComponentsActionType.UPDATE_FAILED_PULLTENANT_REQUESTS:
            return {
                ...prev,
                failedPullTenantRequests: action.failedRequests,
            };
        case SharedComponentsActionType.UPDATE_PULLTENANT_SEARCH_CRITERIA:
            return {
                ...prev,
                pullTenantSearchCriteria: action.searchCriteria,
            };
        case SharedComponentsActionType.UPDATE_PULLTENANT_SEARCH_SELECTION:
            return {
                ...prev,
                pullTenantSearchSelection: action.searchSelection,
            };
        case SharedComponentsActionType.UPDATE_TABLE_ROW_COUNT:
            return {
                ...prev,
                tableRowCount: action.tableRowCount,
            };
        case SharedComponentsActionType.REQUEST_PULLTENANT_SUMMARY_COUNT:
            return {
                ...prev,
                totalPullTenantCount: 0,
                successfulPullTenantCount: 0,
                successfulPullTenantCountDict: {},
                isLoadingPullTenantSummaryCount: true,
            };
        case SharedComponentsActionType.RECEIVE_PULLTENANT_SUMMARY_COUNT:
            return {
                ...prev,
                pullTenantSummaryCount: action.pullTenantSummaryCount,
                totalPullTenantCount: action.totalPullTenantCount,
                isLoadingPullTenantSummaryCount: false,
            };
        case SharedComponentsActionType.RECEIVE_TENANT_DELEGATIONS:
            return {
                ...prev,
                tenantDelegations: action.tenantDelations,
                selectedTenantDelegation: null,
            };
        case SharedComponentsActionType.UPDATE_SELECTED_TENANT_DELEGATION:
            return {
                ...prev,
                selectedTenantDelegation: action.selectedTenantDelegation,
                failedPullTenantRequests: [],
                pullTenantSearchCriteria: null,
                pullTenantSearchSelection: 0,
            };
        case SharedComponentsActionType.FAILED_EXTERNAL_TENANT_INFO:
            return {
                ...prev,
                externalTenantInfoErrorMessage: action.errorMessage,
                externalTenantInfoHasError: true,
            };
        case SharedComponentsActionType.UPDATE_SUCCESSFUL_PULLTENANT_REQUESTS:
            return {
                ...prev,
                successfulPullTenantRequests: prev.successfulPullTenantRequests.concat(action.requests),
                successfulPullTenantCount: prev.successfulPullTenantCount + action.requests?.length,
                successfulPullTenantCountDict: {
                    ...prev.successfulPullTenantCountDict,
                    [action.tenantId]:
                        (prev.successfulPullTenantCountDict[action.tenantId] ?? 0) + action.requests?.length,
                },
            };
        case SharedComponentsActionType.TOGGLE_PROFILE_PANEL:
            return {
                ...prev,
                isProfilePanelOpen: action.isOpen,
            };
        case SharedComponentsActionType.TOGGLE_ACCESSIBILITY_PANEL:
            return {
                ...prev,
                isAccessibilityPanelOpen: action.isOpen,
            };
        case SharedComponentsActionType.INITIATE_SEARCH:
            return {
                ...prev,
                isLoadingSearchResults: true,
                isSearchResultsViewOpen: true,
            };
        case SharedComponentsActionType.SAVE_SEARCH_RESULTS:
            return {
                ...prev,
                searchResults: action.searchResults,
                isLoadingSearchResults: false,
            };
        case SharedComponentsActionType.TOGGLE_SEARCH_RESULTS_VIEW:
            return {
                ...prev,
                isSearchResultsViewOpen: action.isOn,
                searchResults: action.isOn ? prev.searchResults : null,
            };
        case SharedComponentsActionType.REQUEST_SUGGEST:
            return {
                ...prev,
                isLoadingSuggest: true,
            };
        case SharedComponentsActionType.RECEIVE_SUGGEST:
            return {
                ...prev,
                suggestTerms: action.suggestTerms,
                suggestRequests: action.suggestRequests,
                isLoadingSuggest: false,
                suggestCache: {
                    query: action.query,
                    terms: action.suggestTerms,
                    requests: action.suggestRequests,
                },
            };
        case SharedComponentsActionType.CLEAR_SUGGEST:
            return {
                ...prev,
                suggestTerms: [],
                suggestRequests: [],
                isLoadingSuggest: false,
                suggestCache: null,
            };
        case SharedComponentsActionType.TOGGLE_QUICKTOUR:
            return {
                ...prev,
                isQuickTourOpen: !prev.isQuickTourOpen,
            };
        case SharedComponentsActionType.SET_QUICKTOUR_DATA:
            return {
                ...prev,
                quickTourData: action.quickTourData,
            };
        case SharedComponentsActionType.REQUEST_QUICKTOUR_INFO:
            return {
                ...prev,
            };
        case SharedComponentsActionType.RECEIVE_QUICKTOUR_INFO:
            return {
                ...prev,
                unreadQuickTours: action?.unreadQuickTours,
                readQuickTours: action?.readQuickTours,
                updatedQuickTourList: action?.updatedQuickTourList,
            };
        case SharedComponentsActionType.POST_QUICKTOUR_INFO: {
            // Server replaces QuickTourFeatureList wholesale with action.unReadQuickTours,
            // so sync local read/unread lists optimistically; otherwise back-to-back dismisses
            // (e.g. Submitter View then Approvals Labs) read a stale readQuickTours and drop IDs.
            const postedIds = new Set((action.unReadQuickTours || []).map(String));
            const allItems = [...(prev.unreadQuickTours || []), ...(prev.readQuickTours || [])];
            const seen = new Set<string>();
            const dedupedItems = allItems.filter(it => {
                const k = it?.id?.toString();
                if (!k || seen.has(k)) return false;
                seen.add(k);
                return true;
            });
            return {
                ...prev,
                readQuickTours: dedupedItems.filter(it => postedIds.has(it.id.toString())).map(it => ({ ...it, isViewed: true })),
                unreadQuickTours: dedupedItems.filter(it => !postedIds.has(it.id.toString())).map(it => ({ ...it, isViewed: false })),
                updatedQuickTourList: [],
            };
        }
        case SharedComponentsActionType.SET_UNREAD_QUICKTOURS:
            return {
                ...prev,
                updatedQuickTourList: [],
            };
        case SharedComponentsActionType.REQUEST_FLIGHTING_DATA:
            return {
                ...prev,
                isLoadingFlightingData: true,
            };
        case SharedComponentsActionType.RECEIVE_FLIGHTING_DATA: {
            const feedbackMap: Record<string, string> = {};
            if (Array.isArray(action.myFlightingData)) {
                action.myFlightingData.forEach((f: any) => {
                    const name = f?.featureName ?? f?.FeatureName;
                    const vote = f?.vote ?? f?.Vote;
                    if (name && vote) {
                        feedbackMap[name] = vote;
                    }
                });
            }
            return {
                ...prev,
                myFlightingData: action.myFlightingData,
                allFlightingData: action.allFlightingData,
                isLoadingFlightingData: false,
                flightingFeatureFeedback: feedbackMap,
            };
        }
        case SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES:
        case SharedComponentsActionType.UNSUBSCRIBE_FLIGHTING_FEATURES:
            return {
                ...prev,
                isLoadingFlightingData: true,
                userPreferencesSuccessMessage: null,
                userPreferencesFailureMessage: null,
            };
        case SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES_SUCCESS:
            return {
                ...prev,
                userPreferencesSuccessMessage: action.message,
                userPreferencesFailureMessage: null,
                isLoadingFlightingData: false,
            };
        case SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES_FAILED:
            return {
                ...prev,
                userPreferencesSuccessMessage: null,
                userPreferencesFailureMessage: action.message,
                isLoadingFlightingData: false,
            };
        case SharedComponentsActionType.SUBMIT_FLIGHTING_FEATURE_FEEDBACK: {
            const nextFeedback = { ...(prev.flightingFeatureFeedback || {}) };
            if (!action.vote || action.vote === 'None') {
                delete nextFeedback[action.featureName];
            } else {
                nextFeedback[action.featureName] = action.vote;
            }
            return {
                ...prev,
                flightingFeatureFeedback: nextFeedback,
            };
        }
        case SharedComponentsActionType.RECEIVE_INSIGHTS:
            return {
                ...prev,
                insightsData: {
                    summaryInsights: action.summaryInsights ?? prev.insightsData.summaryInsights,
                    historyInsights: action.historyInsights ?? prev.insightsData.historyInsights,
                },
            };
        case SharedComponentsActionType.UPDATE_FEEDBACK_INPUT: {
            const { id, featureName, inputType, inputValue, documentNumber, fiscalYear } = action.payload;
            const existingFeedback = prev.feedback.feedbackByFeature[featureName];

            if (!existingFeedback) {
                // Initialize new feedback if it doesn't exist
                const newFeedback: IFeedbackData = {
                    FeatureName: featureName,
                    DocumentNumber: documentNumber || '',
                    FiscalYear: fiscalYear || '',
                    Inputs: [
                        {
                            id: id,
                            InputType: inputType,
                            InputValue: inputValue,
                        },
                    ],
                };

                return {
                    ...prev,
                    feedback: {
                        ...prev.feedback,
                        feedbackByFeature: {
                            ...prev.feedback.feedbackByFeature,
                            [featureName]: newFeedback,
                        },
                    },
                };
            }

            // Update existing feedback
            const updatedInputs = existingFeedback.Inputs.map((input) =>
                input.id === id ? { ...input, InputValue: inputValue } : input
            );

            // Add new input if it doesn't exist
            if (!existingFeedback.Inputs.some((input) => input.id === id)) {
                updatedInputs.push({
                    id: id,
                    InputType: inputType,
                    InputValue: inputValue,
                });
            }

            return {
                ...prev,
                feedback: {
                    ...prev.feedback,
                    feedbackByFeature: {
                        ...prev.feedback.feedbackByFeature,
                        [featureName]: {
                            ...existingFeedback,
                            Inputs: updatedInputs,
                        },
                    },
                },
            };
        }
        case SharedComponentsActionType.DELETE_FEEDBACK_INPUT: {
            const { id, featureName } = action.payload;
            const existingFeedback = prev.feedback.feedbackByFeature[featureName];

            // If feedback doesn't exist, nothing to delete
            if (!existingFeedback) {
                return prev;
            }

            // Filter out the input with the matching id
            const filteredInputs = existingFeedback.Inputs.filter((input) => input.id !== id);

            // If no inputs remain, remove the entire feedback
            if (filteredInputs.length === 0) {
                // eslint-disable-next-line @typescript-eslint/no-unused-vars
                const { [featureName]: _, ...remainingFeedback } = prev.feedback.feedbackByFeature;
                return {
                    ...prev,
                    feedback: {
                        ...prev.feedback,
                        feedbackByFeature: remainingFeedback,
                    },
                };
            }

            // Update feedback with filtered inputs
            return {
                ...prev,
                feedback: {
                    ...prev.feedback,
                    feedbackByFeature: {
                        ...prev.feedback.feedbackByFeature,
                        [featureName]: {
                            ...existingFeedback,
                            Inputs: filteredInputs,
                        },
                    },
                },
            };
        }
        case SharedComponentsActionType.UPDATE_PROPERTY_FILTERS:
            return {
                ...prev,
                propertyFilters: action.propertyFilters,
            };
        default:
            return prev;
    }
}
