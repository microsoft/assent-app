import { ISharedComponentsState } from './SharedComponents.types';
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
    historySelectedPage: 1,
    sortColumnField: 'ActionDate',
    sortDirection: 'DESC',
    historySearchCriteria: '',
    historyTimePeriod: 3,
    historyData: [],
    summaryGroupedBy: GroupingBy.Tenant,
    filterValue: 'All',
    bulkFailedMsg: [],
    isBulkSelected: false,
    bulkApproveFailed: false,
    bulkApproveStatus: false,
    isProcessingBulkApproval: false,
    isCardViewSelected: true,
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
            };
        case SharedComponentsActionType.RECEIVE_MY_SUMMARY:
            return {
                ...prev,
                isLoadingSummary: false,
                hasError: false,
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
                toggleDetailsScreen: action.isOpen ? prev.toggleDetailsScreen : false,
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
        case SharedComponentsActionType.UPDATE_CARD_VIEW_TYPE:
            return {
                ...prev,
                isPanelOpen: false,
                isCardViewSelected: action.isCardViewSelected,
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
            };
        case SharedComponentsActionType.TOGGLE_DETAIL_SCREEN:
            return {
                ...prev,
                toggleDetailsScreen: !prev.toggleDetailsScreen,
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
        case SharedComponentsActionType.TOGGLE_SETTINGS_PANEL:
            return {
                ...prev,
                isSettingPanelOpen: action.toggle,
            };
        case SharedComponentsActionType.RECEIVE_USER_PREFERENCES:
            let summaryGroupedBy = GroupingBy.Tenant;
            let detailsDefaultView = DOCKED_VIEW;
            let historyDefaultView = FLYOUT_VIEW;
            let defaultViewType = CARD_VIEW;
            let DefaultTenant = '';
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
                isCardViewSelected: defaultViewType == CARD_VIEW ? true : false,
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
                isSettingPanelOpen: false,
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
        default:
            return prev;
    }
}
