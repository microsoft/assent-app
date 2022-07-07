import { IGrouping } from '../../Helpers/groupPendingApprovals';
import {
    SharedComponentsActionType,
    IRequestProfileAction,
    IReceiveProfileAction,
    IFailedProfileAction,
    IRequestSummaryAction,
    IReceiveSummaryAction,
    IRequestOutofSyncSummaryAction,
    IReceiveOutofSyncSummaryAction,
    IFailedOutofSyncSummaryAction,
    IUpdatePanelState,
    IUpdateGroupedSummary,
    IFailedSummaryAction,
    IRequestTenantInfoAction,
    IReceiveTenantInfoAction,
    IFailedTenantInfoAction,
    IUpdateSelectedSummarytoPending,
    IUpdateSelectedSummarytoOutOfSync,
    IRequestMyHistoryAction,
    IReceiveMyHistoryAction,
    IFailedHistoryAction,
    IUpdateSelectedPage,
    IUpdateGroupedHistory,
    IUpdateFilterValue,
    IUpdateBulkUploadConcurrentValue,
    IUpdateBulkFailedMsg,
    IUpdateBulkSelected,
    IUpdateBulkApprovalRequest,
    IUpdateBulkvalue,
    IUpdateBulkFailed,
    IUpdateIsProcessingBulkApprovalAction,
    IUpdateCardView,
    IUpdateApprovalRecords,
    IRequestDelegationsAction,
    IReceiveDelegationsAction,
    IFailedDelegationsAction,
    IUpdateUserAlias,
    IToggleDetailScreen,
    IRequestFilteredUsersAction,
    IReceiveFilteredUsersAction,
    ISetSelectedSummaryTileRef,
    IFailedDownloadHistory,
    IToggleSettingsPanel,
    ISaveUserPreferencesRequest,
    ISaveUserPreferencesResponse,
    ISaveUserPreferencesFailed,
    IRequestUserPreferences,
    IReceiveUserPreferences,
    IFailedUserPreferences,
    IClearUserPreferencesAPIMessages,
    IUpdatePeoplePickerSelection,
    IUpdatePeoplePickerHasError,
    IReceiveSubmitterImages,
    IConcatSubmitterImages,
    IRequestPullTenantSummary,
    IReceivePullTenantSummary,
    IRefreshBulkState,
    IUpdateRetainBulkSelection,
    IReceiveExternalTenantInfo,
    IRequestExternalTenantInfo,
    IFailedPullTenantSummary,
    IUpdateFailedPullTenantRequests,
    IUpdatePullTenantSearchCriteria,
    IUpdatePullTenantSearchSelection,
    IUpdateTableRowCount,
    IRequestPullTenantSummaryCount,
    IReceivePullTenantSummaryCount,
    IReceiveTenantDelegations,
    IUpdateSelectedTenantDelegation,
    IFailedExternalTenantInfo,
    IUpdateSuccessfulPullTenantRequests,
    IRequestDownloadHistory,
    IReceiveDownloadHistory,
    IToggleProfilePanel,
} from './SharedComponents.action-types';
import {
    IActionResponseObject,
    IDelegationObj,
    IFeaturesIntroductionStep,
    IGraphPhoto,
    IProfile,
    IPullTenantSummaryCountObject,
    ITenantDelegationObj,
} from './SharedComponents.types';

export function ClearUserPreferencesAPIMessages(): IClearUserPreferencesAPIMessages {
    return {
        type: SharedComponentsActionType.CLEAR_USER_PREFERENCES_API_MESSAGES,
    };
}

export function SaveUserPreferencesRequest(data: any): ISaveUserPreferencesRequest {
    return {
        type: SharedComponentsActionType.SAVE_USER_PREFERENCES_REQUEST,
        data,
    };
}

export function SaveUserPreferencesResponse(message: string): ISaveUserPreferencesResponse {
    return {
        type: SharedComponentsActionType.SAVE_USER_PREFERENCES_RESPONSE,
        message,
    };
}

export function SaveUserPreferencesFailed(message: string): ISaveUserPreferencesFailed {
    return {
        type: SharedComponentsActionType.SAVE_USER_PREFERENCES_FAILED,
        message,
    };
}

export function RequestUserPreferences(): IRequestUserPreferences {
    return {
        type: SharedComponentsActionType.REQUEST_USER_PREFERENCES,
    };
}

export function ReceiveUserPreferences(data: any): IReceiveUserPreferences {
    return {
        type: SharedComponentsActionType.RECEIVE_USER_PREFERENCES,
        data,
    };
}

export function FailedUserPreferences(message: string): IFailedUserPreferences {
    return {
        type: SharedComponentsActionType.FAILED_USER_PREFERENCES,
        message,
    };
}

export function toggleSettingsPanel(toggle: boolean): IToggleSettingsPanel {
    return {
        type: SharedComponentsActionType.TOGGLE_SETTINGS_PANEL,
        toggle,
    };
}

export function toggleDetailsScreen(): IToggleDetailScreen {
    return {
        type: SharedComponentsActionType.TOGGLE_DETAIL_SCREEN,
    };
}

export function requestMyProfile(): IRequestProfileAction {
    return {
        type: SharedComponentsActionType.REQUEST_MY_PROFILE,
    };
}

export function failedProfile(profileErrorMessage: string): IFailedProfileAction {
    return {
        type: SharedComponentsActionType.FAILED_PROFILE,
        profileErrorMessage,
    };
}

export function receiveFriendByEmail(profile: IProfile): IReceiveProfileAction {
    return {
        type: SharedComponentsActionType.RECEIVE_MY_PROFILE,
        profile,
    };
}

export function updateUserAlias(userAlias: string, userName: string): IUpdateUserAlias {
    return {
        type: SharedComponentsActionType.UPDATE_USER_ALIAS,
        userAlias,
        userName,
    };
}

export function requestMyDelegations(
    loggedInAlias: string | null,
    tenantId?: number,
    appName?: string
): IRequestDelegationsAction {
    return {
        type: SharedComponentsActionType.REQUEST_MY_DELEGATIONS,
        loggedInAlias,
        tenantId: tenantId ?? null,
        appName: appName ?? null,
    };
}

export function receiveMyDelegations(userDelegations: Object[]): IReceiveDelegationsAction {
    return {
        type: SharedComponentsActionType.RECEIVE_MY_DELEGATIONS,
        userDelegations,
    };
}

export function receiveTenantDelegations(tenantDelations: ITenantDelegationObj): IReceiveTenantDelegations {
    return {
        type: SharedComponentsActionType.RECEIVE_TENANT_DELEGATIONS,
        tenantDelations,
    };
}

export function failedDelegations(delegationsErrorMessage: string): IFailedDelegationsAction {
    return {
        type: SharedComponentsActionType.FAILED_DELEGATIONS,
        delegationsErrorMessage,
    };
}

export function requestMySummary(userAlias: string): IRequestSummaryAction {
    return {
        type: SharedComponentsActionType.REQUEST_MY_SUMMARY,
        userAlias,
    };
}
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function receiveMySummary(summary: any): IReceiveSummaryAction {
    return {
        type: SharedComponentsActionType.RECEIVE_MY_SUMMARY,
        summary,
    };
}

export function failedSummary(summaryErrorMessage: string): IFailedSummaryAction {
    return {
        type: SharedComponentsActionType.FAILED_SUMMARY,
        summaryErrorMessage,
    };
}

export function failedHistory(historyErrorMessage: string): IFailedHistoryAction {
    return {
        type: SharedComponentsActionType.FAILED_HISTORY,
        historyErrorMessage,
    };
}

export function requestTenantInfo(): IRequestTenantInfoAction {
    return {
        type: SharedComponentsActionType.REQUEST_TENANT_INFO,
    };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function receiveTenantInfo(tenantInfo: any): IReceiveTenantInfoAction {
    return {
        type: SharedComponentsActionType.RECEIVE_TENANT_INFO,
        tenantInfo,
    };
}

export function requestMyOutOfSyncSummary(): IRequestOutofSyncSummaryAction {
    return {
        type: SharedComponentsActionType.REQUEST_MY_OUT_OF_SYNC_SUMMARY,
    };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function receiveMyOutOfSyncSummary(outOfSyncSummary: any): IReceiveOutofSyncSummaryAction {
    return {
        type: SharedComponentsActionType.RECEIVE_MY_OUT_OF_SYNC_SUMMARY,
        outOfSyncSummary,
    };
}

export function failedOutOfSyncSummary(outOfSyncErrorMessage: any): IFailedOutofSyncSummaryAction {
    return {
        type: SharedComponentsActionType.FAILED_OUT_OF_SYNC_SUMMARY,
        outOfSyncErrorMessage,
    };
}

export function failedTenantInfo(tenantInfoErrorMessage: string): IFailedTenantInfoAction {
    return {
        type: SharedComponentsActionType.FAILED_TENANT_INFO,
        tenantInfoErrorMessage,
    };
}

export function updatePanelState(isOpen: boolean): IUpdatePanelState {
    return {
        type: SharedComponentsActionType.UPDATE_PANEL_STATE,
        isOpen,
    };
}

export function setSelectedSumaryTileRef(tileRef: any): ISetSelectedSummaryTileRef {
    return {
        type: SharedComponentsActionType.SET_SELECTED_SUMMARY_TILE_REF,
        tileRef,
    };
}

export function updateSelectedSummarytoPending(): IUpdateSelectedSummarytoPending {
    return {
        type: SharedComponentsActionType.UPDATE_SELECTED_SUMMARY_TO_PENDING,
    };
}

export function updateSelectedSummarytoOutOfSync(): IUpdateSelectedSummarytoOutOfSync {
    return {
        type: SharedComponentsActionType.UPDATE_SELECTED_SUMMARY_TO_OUT_OF_SYNC,
    };
}

export function updateGroupedSummary(summaryGroupedBy: string): IUpdateGroupedSummary {
    return {
        type: SharedComponentsActionType.UPDATE_GROUPED_SUMMARY,
        summaryGroupedBy,
    };
}

export function updateFilterValue(filterValue: string): IUpdateFilterValue {
    return {
        type: SharedComponentsActionType.UPDATE_FILTER_VALUE,
        filterValue,
    };
}

export function updateBulkUploadConcurrentValue(bulkUploadConcurrentValue: number): IUpdateBulkUploadConcurrentValue {
    return {
        type: SharedComponentsActionType.UPDATE_BULK_UPLOAD_CONCURRENT_VALUE,
        bulkUploadConcurrentValue,
    };
}

export function updateBulkFailedValue(bulkFailedMessage: string[]): IUpdateBulkFailedMsg {
    return {
        type: SharedComponentsActionType.UPDATE_BULK_FAILED_VALUE,
        bulkFailedMessage,
    };
}

export function updateIsProcessingBulkApproval(
    isProcessingBulkApproval: boolean
): IUpdateIsProcessingBulkApprovalAction {
    return {
        type: SharedComponentsActionType.UPDATE_IS_PROCESSING_BULK_APPROVAL,
        isProcessingBulkApproval,
    };
}

export function updateBulkSelected(isBulkSelected: boolean): IUpdateBulkSelected {
    return {
        type: SharedComponentsActionType.UPDATE_BULK_SELECTED,
        isBulkSelected,
    };
}

export function updateBulkApproveRequest(bulkApproveRequest: boolean): IUpdateBulkApprovalRequest {
    return {
        type: SharedComponentsActionType.SAVE_BULK_APPROVAL_REQUEST,
        bulkApproveRequest,
    };
}

export function updateBulkStatus(bulkStatus: boolean): IUpdateBulkvalue {
    return {
        type: SharedComponentsActionType.UPDATE_BULK_STATUS,
        bulkStatus,
    };
}

export function updateBulkFailedStatus(bulkApproveFailed: boolean): IUpdateBulkFailed {
    return {
        type: SharedComponentsActionType.UPDATE_BULK_FAILED,
        bulkApproveFailed,
    };
}

export function updateCardViewType(isCardViewSelected: boolean): IUpdateCardView {
    return {
        type: SharedComponentsActionType.UPDATE_CARD_VIEW_TYPE,
        isCardViewSelected,
    };
}

export function updateApprovalRecords(approveRecords: Array<any>, subAction?: string): IUpdateApprovalRecords {
    return {
        type: SharedComponentsActionType.UPDATE_APPROVAL_RECORDS,
        approveRecords,
        subAction,
    };
}

export function requestMyHistory(
    page: number,
    sortColumn: string,
    sortDirection: string,
    searchCriteria: string,
    timePeriod: number,
    tenantId: string
): IRequestMyHistoryAction {
    return {
        type: SharedComponentsActionType.REQUEST_MY_HISTORY,
        page,
        sortColumn,
        sortDirection,
        searchCriteria,
        timePeriod,
        tenantId,
    };
}

export function receiveMyHistory(history: any): IReceiveMyHistoryAction {
    return {
        type: SharedComponentsActionType.RECEIVE_MY_HISTORY,
        history,
    };
}

export function updateSelectedPage(currentPage: string): IUpdateSelectedPage {
    return {
        type: SharedComponentsActionType.UPDATE_SELECTED_PAGE,
        currentPage,
    };
}

export function updateGroupedHistory(historyGroupedBy: string): IUpdateGroupedHistory {
    return {
        type: SharedComponentsActionType.UPDATE_GROUPED_HISTORY,
        historyGroupedBy,
    };
}

export function requestDownloadHistory(
    monthsOfData: number,
    searchCriteria: string,
    sortField: string,
    sortDirection: string,
    tenantId: string
): IRequestDownloadHistory {
    return {
        type: SharedComponentsActionType.REQUEST_DOWNLOAD_HISTORY,
        monthsOfData,
        searchCriteria,
        sortField,
        sortDirection,
        tenantId,
    };
}

export function receiveDownloadHistory(): IReceiveDownloadHistory {
    return {
        type: SharedComponentsActionType.RECEIVE_DOWNLOAD_HISTORY,
    };
}

export function updateHistoryData(historyData: any, totalRecords: number) {
    return {
        type: SharedComponentsActionType.UPDATE_HISTORY_DATA,
        historyData,
        totalRecords,
    };
}

export function toggleTeachingBubbleVisibility() {
    return {
        type: SharedComponentsActionType.TOGGLE_TEACHING_BUBBLE_VISIBILITY,
    };
}

export function updateTeachingStep(newStep: IFeaturesIntroductionStep) {
    return {
        type: SharedComponentsActionType.UPDATE_TEACHING_STEP,
        newStep,
    };
}

export function requestFilteredUsers(filterText: string): IRequestFilteredUsersAction {
    return {
        type: SharedComponentsActionType.REQUEST_FILTERED_USERS,
        filterText,
    };
}

export function receiveFilteredUsers(filteredUsers: object[]): IReceiveFilteredUsersAction {
    return {
        type: SharedComponentsActionType.RECEIVE_FILTERED_USERS,
        filteredUsers,
    };
}

export function failedDownloadHistory(downloadErrorMessage: string): IFailedDownloadHistory {
    return {
        type: SharedComponentsActionType.FAILED_DOWNLOAD_HISTORY,
        downloadErrorMessage,
    };
}

export function updatePeoplePickerSelection(peoplePickerSelections: object[]): IUpdatePeoplePickerSelection {
    return {
        type: SharedComponentsActionType.UPDATE_PEOPLEPICKER_SELECTION,
        peoplePickerSelections,
    };
}

export function updatePeoplePickerHasError(peoplePickerHasError: boolean): IUpdatePeoplePickerHasError {
    return {
        type: SharedComponentsActionType.UPDATE_PEOPLEPICKER_HASERROR,
        peoplePickerHasError,
    };
}

export function receiveSubmitterImages(submitterImages: IGraphPhoto[]): IReceiveSubmitterImages {
    return {
        type: SharedComponentsActionType.RECEIVE_SUBMITTER_IMAGES,
        submitterImages,
    };
}

export function concatSubmitterImages(newSubmitterImages: IGraphPhoto[]): IConcatSubmitterImages {
    return {
        type: SharedComponentsActionType.CONCAT_SUBMITTER_IMAGES,
        newSubmitterImages,
    };
}

export function requestPullTenantSummary(
    tenantId: number,
    userAlias: string,
    filterCriteria?: object,
    isExternalTenantInfoRequired?: boolean
): IRequestPullTenantSummary {
    return {
        type: SharedComponentsActionType.REQUEST_PULL_TENANT_SUMMARY,
        tenantId,
        userAlias,
        filterCriteria,
        isExternalTenantInfoRequired,
    };
}

export function receivePullTenantSummary(pullTenantSummary: object[]): IReceivePullTenantSummary {
    return {
        type: SharedComponentsActionType.RECEIVE_PULL_TENANT_SUMMARY,
        pullTenantSummary,
    };
}

export function failedPullTenantSummary(errorMessage: string): IFailedPullTenantSummary {
    return {
        type: SharedComponentsActionType.FAILED_PULL_TENANT_SUMMARY,
        errorMessage,
    };
}

export function refreshBulkState(): IRefreshBulkState {
    return {
        type: SharedComponentsActionType.REFRESH_BULK_STATE,
    };
}

export function updateRetainBulkSelection(isBulkSelectionRetained: boolean): IUpdateRetainBulkSelection {
    return {
        type: SharedComponentsActionType.UPDATE_RETAIN_BULK_SELECTION,
        isBulkSelectionRetained,
    };
}

export function requestExternalTenantInto(tenantId: number, userAlias: string): IRequestExternalTenantInfo {
    return {
        type: SharedComponentsActionType.REQUEST_EXTERNAL_TENANT_INFO,
        tenantId,
        userAlias,
    };
}

export function receiveExternalTenantInfo(externalTenantInfo: object): IReceiveExternalTenantInfo {
    return {
        type: SharedComponentsActionType.RECEIVE_EXTERNAL_TENANT_INFO,
        externalTenantInfo,
    };
}

export function failedExternalTenantInfo(errorMessage: string): IFailedExternalTenantInfo {
    return {
        type: SharedComponentsActionType.FAILED_EXTERNAL_TENANT_INFO,
        errorMessage,
    };
}

export function updateFailedPullTenantRequests(
    failedRequests: IActionResponseObject[]
): IUpdateFailedPullTenantRequests {
    return {
        type: SharedComponentsActionType.UPDATE_FAILED_PULLTENANT_REQUESTS,
        failedRequests,
    };
}

export function updatePullTenantSearchCriteria(searchCriteria: object[]): IUpdatePullTenantSearchCriteria {
    return {
        type: SharedComponentsActionType.UPDATE_PULLTENANT_SEARCH_CRITERIA,
        searchCriteria,
    };
}

export function updatePullTenantSearchSelection(searchSelection: number): IUpdatePullTenantSearchSelection {
    return {
        type: SharedComponentsActionType.UPDATE_PULLTENANT_SEARCH_SELECTION,
        searchSelection,
    };
}

export function updateTableRowCount(tableRowCount: number): IUpdateTableRowCount {
    return {
        type: SharedComponentsActionType.UPDATE_TABLE_ROW_COUNT,
        tableRowCount,
    };
}

export function requestPullTenantSummaryCount(userAlias: string): IRequestPullTenantSummaryCount {
    return {
        type: SharedComponentsActionType.REQUEST_PULLTENANT_SUMMARY_COUNT,
        userAlias,
    };
}

export function receivePullTenantSummaryCount(
    pullTenantSummaryCount: IPullTenantSummaryCountObject[],
    totalPullTenantCount: number
): IReceivePullTenantSummaryCount {
    return {
        type: SharedComponentsActionType.RECEIVE_PULLTENANT_SUMMARY_COUNT,
        pullTenantSummaryCount,
        totalPullTenantCount,
    };
}

export function updateSelectedTenantDelegation(
    selectedTenantDelegation: IDelegationObj
): IUpdateSelectedTenantDelegation {
    return {
        type: SharedComponentsActionType.UPDATE_SELECTED_TENANT_DELEGATION,
        selectedTenantDelegation,
    };
}

export function updateSuccessfulPullTenantRequests(
    tenantId: number,
    requests: string[]
): IUpdateSuccessfulPullTenantRequests {
    return {
        type: SharedComponentsActionType.UPDATE_SUCCESSFUL_PULLTENANT_REQUESTS,
        tenantId,
        requests,
    };
}

export function toggleProfilePanel(isOpen: boolean): IToggleProfilePanel {
    return {
        type: SharedComponentsActionType.TOGGLE_PROFILE_PANEL,
        isOpen,
    };
}
