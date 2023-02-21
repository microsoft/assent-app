import { IGrouping } from '../../Helpers/groupPendingApprovals';
import {
    IActionResponseObject,
    IDelegationObj,
    IFeaturesIntroductionStep,
    IGraphPhoto,
    IProfile,
    IPullTenantSummaryCountObject,
    ITenantDelegationObj
} from './SharedComponents.types';

export enum SharedComponentsActionType {
    REQUEST_MY_PROFILE = 'REQUEST_MY_PROFILE',
    RECEIVE_MY_PROFILE = 'RECEIVE_MY_PROFILE',
    REQUEST_MY_DELEGATIONS = 'REQUEST_MY_DELEGATIONS',
    RECEIVE_MY_DELEGATIONS = 'RECEIVE_MY_DELEGATIONS',
    FAILED_PROFILE = 'FAILED_PROFILE',
    FAILED_DELEGATIONS = 'FAILED_DELEGATIONS',
    REQUEST_MY_SUMMARY = 'REQUEST_MY_SUMMARY',
    RECEIVE_MY_SUMMARY = 'RECEIVE_MY_SUMMARY',
    REQUEST_MY_OUT_OF_SYNC_SUMMARY = 'REQUEST_MY_OUT_OF_SYNC_SUMMARY',
    RECEIVE_MY_OUT_OF_SYNC_SUMMARY = 'RECEIVE_MY_OUT_OF_SYNC_SUMMARY',
    FAILED_OUT_OF_SYNC_SUMMARY = 'FAILED_OUT_OF_SYNC_SUMMARY',
    UPDATE_PANEL_STATE = 'UPDATE_PANEL_STATE',
    UPDATE_GROUPED_SUMMARY = 'UPDATE_GROUPED_SUMMARY',
    FAILED_SUMMARY = 'FAILED_SUMMARY',
    REQUEST_TENANT_INFO = 'REQUEST_TENANT_INFO',
    RECEIVE_TENANT_INFO = 'RECEIVE_TENANT_INFO',
    FAILED_TENANT_INFO = 'FAILED_TENANT_INFO',
    UPDATE_SELECTED_SUMMARY_TO_PENDING = 'UPDATE_SELECTED_SUMMARY_TO_PENDING',
    UPDATE_SELECTED_SUMMARY_TO_OUT_OF_SYNC = 'UPDATE_SELECTED_SUMMARY_TO_OUT_OF_SYNC',
    REQUEST_MY_HISTORY = 'REQUEST_MY_HISTORY',
    RECEIVE_MY_HISTORY = 'RECEIVE_MY_HISTORY',
    FAILED_HISTORY = 'FAILED_HISTORY',
    UPDATE_SELECTED_PAGE = 'UPDATE_SELECTED_PAGE',
    UPDATE_GROUPED_HISTORY = 'UPDATE_GROUPED_HISTORY',
    REQUEST_DOWNLOAD_HISTORY = 'REQUEST_DOWNLOAD_HISTORY',
    RECEIVE_DOWNLOAD_HISTORY = 'RECEIVE_DOWNLOAD_HISTORY',
    UPDATE_FILTER_VALUE = 'UPDATE_FILTER_VALUE',
    UPDATE_BULK_UPLOAD_CONCURRENT_VALUE = 'UPDATE_BULK_UPLOAD_CONCURRENT_VALUE',
    UPDATE_BULK_FAILED_VALUE = 'UPDATE_BULK_FAILED_VALUE',
    UPDATE_BULK_SELECTED = 'UPDATE_BULK_SELECTED',
    UPDATE_BULK_STATUS = 'UPDATE_BULK_STATUS',
    UPDATE_BULK_FAILED = 'UPDATE_BULK_FAILED',
    UPDATE_IS_PROCESSING_BULK_APPROVAL = 'UPDATE_IS_PROCESSING_BULK_APPROVAL',
    UPDATE_CARD_VIEW_TYPE = 'UPDATE_CARD_VIEW_TYPE',
    UPDATE_APPROVAL_RECORDS = 'UPDATE_APPROVAL_RECORDS',
    UPDATE_USER_ALIAS = 'UPDATE_USER_ALIAS',
    TOGGLE_TEACHING_BUBBLE_VISIBILITY = 'TOGGLE_TEACHING_BUBBLE_VISIBILITY',
    UPDATE_TEACHING_STEP = 'UPDATE_TEACHING_STEP',
    UPDATE_HISTORY_DATA = 'UPDATE_HISTORY_DATA',
    TOGGLE_DETAIL_SCREEN = 'TOGGLE_DETAIL_SCREEN',
    REQUEST_FILTERED_USERS = 'REQUEST_FILTERED_USERS',
    RECEIVE_FILTERED_USERS = 'RECEIVE_FILTERED_USERS',
    SET_SELECTED_SUMMARY_TILE_REF = 'SET_SELECTED_SUMMARY_TILE_REF',
    FAILED_DOWNLOAD_HISTORY = 'FAILED_DOWNLOAD_HISTORY',
    TOGGLE_SETTINGS_PANEL = 'TOGGLE_SETTINGS_PANEL',

    SAVE_USER_PREFERENCES_REQUEST = 'SAVE_USER_PREFERENCES_REQUEST',
    SAVE_USER_PREFERENCES_RESPONSE = 'SAVE_USER_PREFERENCES_RESPONSE',
    SAVE_USER_PREFERENCES_FAILED = 'SAVE_USER_PREFERENCES_FAILED',

    REQUEST_USER_PREFERENCES = 'REQUEST_USER_PREFERENCES',
    RECEIVE_USER_PREFERENCES = 'RECEIVE_USER_PREFERENCES',
    FAILED_USER_PREFERENCES = 'FAILED_USER_PREFERENCES',

    CLEAR_USER_PREFERENCES_API_MESSAGES = 'CLEAR_USER_PREFERENCES_API_MESSAGES',
    SAVE_USER_APPROVAL_REQUEST = 'SAVE_USER_APPROVAL_REQUEST',
    SAVE_BULK_APPROVAL_REQUEST = 'SAVE_BULK_APPROVAL_REQUEST',
    UPDATE_PEOPLEPICKER_SELECTION = 'UPDATE_PEOPLEPICKER_SELECTION',
    UPDATE_PEOPLEPICKER_HASERROR = 'UPDATE_PEOPLEPICKER_HASERROR',
    RECEIVE_SUBMITTER_IMAGES = 'RECEIVE_SUBMITTER_IMAGES',
    CONCAT_SUBMITTER_IMAGES = 'CONCAT_SUBMITTER_IMAGES',
    REQUEST_PULL_TENANT_SUMMARY = 'REQUEST_PULL_TENANT_SUMMARY',
    RECEIVE_PULL_TENANT_SUMMARY = 'RECEIVE_PULL_TENANT_SUMMARY',
    FAILED_PULL_TENANT_SUMMARY = 'FAILED_PULL_TENANT_SUMMARY',
    REFRESH_BULK_STATE = 'REFRESH_BULK_STATE',
    UPDATE_RETAIN_BULK_SELECTION = 'UPDATE_RETAIN_BULK_SELECTION',
    REQUEST_EXTERNAL_TENANT_INFO = 'REQUEST_EXTERNAL_TENANT_INFO',
    RECEIVE_EXTERNAL_TENANT_INFO = 'RECEIVE_EXTERNAL_TENANT_INFO',
    FAILED_EXTERNAL_TENANT_INFO = 'FAILED_EXTERNAL_TENANT_INFO',
    UPDATE_FAILED_PULLTENANT_REQUESTS = 'UPDATE_FAILED_PULLTENANT_REQUESTS',
    UPDATE_PULLTENANT_SEARCH_CRITERIA = 'UPDATE_PULLTENANT_SEARCH_CRITERIA',
    UPDATE_PULLTENANT_SEARCH_SELECTION = 'UPDATE_PULLTENANT_SEARCH_SELECTION',
    UPDATE_TABLE_ROW_COUNT = 'UPDATE_TABLE_ROW_COUNT',
    REQUEST_PULLTENANT_SUMMARY_COUNT = 'REQUEST_PULLTENANT_SUMMARY_COUNT',
    RECEIVE_PULLTENANT_SUMMARY_COUNT = 'RECEIVE_PULLTENANT_SUMMARY_COUNT',
    RECEIVE_TENANT_DELEGATIONS = 'RECEIVE_TENANT_DELEGATIONS',
    UPDATE_SELECTED_TENANT_DELEGATION = 'UPDATE_SELECTED_TENANT_DELEGATION',
    UPDATE_SUCCESSFUL_PULLTENANT_REQUESTS = 'UPDATE_SUCCESSFUL_PULLTENANT_REQUESTS',
    TOGGLE_PROFILE_PANEL = 'TOGGLE_PROFILE_PANEL',
    TOGGLE_ACCESSIBILITY_PANEL = 'TOGGLE_ACCESSIBILITY_PANEL',
}

export type SharedComponentsAction =
    | IRequestProfileAction
    | IReceiveProfileAction
    | IRequestDelegationsAction
    | IReceiveDelegationsAction
    | IUpdateUserAlias
    | IFailedProfileAction
    | IFailedDelegationsAction
    | IRequestSummaryAction
    | IReceiveSummaryAction
    | IRequestOutofSyncSummaryAction
    | IReceiveOutofSyncSummaryAction
    | IFailedOutofSyncSummaryAction
    | IUpdatePanelState
    | IUpdateGroupedSummary
    | IUpdateFilterValue
    | IUpdateBulkUploadConcurrentValue
    | IUpdateBulkFailedMsg
    | IUpdateBulkSelected
    | IUpdateBulkvalue
    | IUpdateBulkFailed
    | IUpdateIsProcessingBulkApprovalAction
    | IUpdateCardView
    | IUpdateApprovalRecords
    | IFailedSummaryAction
    | IRequestTenantInfoAction
    | IReceiveTenantInfoAction
    | IFailedTenantInfoAction
    | IUpdatePanelState
    | IUpdateSelectedSummarytoPending
    | IUpdateSelectedSummarytoOutOfSync
    | IRequestMyHistoryAction
    | IReceiveMyHistoryAction
    | IFailedHistoryAction
    | IUpdateSelectedPage
    | IUpdateGroupedHistory
    | IRequestDownloadHistory
    | IReceiveDownloadHistory
    | IUpdateHistoryData
    | IToggleTeachingBubbleVisibility
    | IUpdateTeachingStep
    | IToggleDetailScreen
    | IRequestFilteredUsersAction
    | IReceiveFilteredUsersAction
    | ISetSelectedSummaryTileRef
    | IFailedDownloadHistory
    | IToggleSettingsPanel
    | ISaveUserPreferencesRequest
    | ISelectedApprovalRequest
    | IUpdateBulkApprovalRequest
    | ISaveUserPreferencesResponse
    | ISaveUserPreferencesFailed
    | IRequestUserPreferences
    | IReceiveUserPreferences
    | IFailedUserPreferences
    | IClearUserPreferencesAPIMessages
    | IUpdatePeoplePickerSelection
    | IUpdatePeoplePickerHasError
    | IReceiveSubmitterImages
    | IConcatSubmitterImages
    | IRequestPullTenantSummary
    | IReceivePullTenantSummary
    | IFailedPullTenantSummary
    | IRefreshBulkState
    | IUpdateRetainBulkSelection
    | IRequestExternalTenantInfo
    | IReceiveExternalTenantInfo
    | IFailedExternalTenantInfo
    | IUpdateFailedPullTenantRequests
    | IUpdatePullTenantSearchCriteria
    | IUpdatePullTenantSearchSelection
    | IUpdateTableRowCount
    | IRequestPullTenantSummaryCount
    | IReceivePullTenantSummaryCount
    | IReceiveTenantDelegations
    | IUpdateSelectedTenantDelegation
    | IUpdateSuccessfulPullTenantRequests
    | IToggleProfilePanel
    | IToggleAccessibilityPanel;

export interface IClearUserPreferencesAPIMessages {
    type: SharedComponentsActionType.CLEAR_USER_PREFERENCES_API_MESSAGES;
}

export interface ISelectedApprovalRequest {
    type: SharedComponentsActionType.SAVE_USER_APPROVAL_REQUEST;
    data: any;
}

export interface IUpdateBulkApprovalRequest {
    type: SharedComponentsActionType.SAVE_BULK_APPROVAL_REQUEST;
    bulkApproveRequest: boolean;
}

export interface ISaveUserPreferencesRequest {
    type: SharedComponentsActionType.SAVE_USER_PREFERENCES_REQUEST;
    data: any;
}
export interface ISaveUserPreferencesResponse {
    type: SharedComponentsActionType.SAVE_USER_PREFERENCES_RESPONSE;
    message: string;
}
export interface ISaveUserPreferencesFailed {
    type: SharedComponentsActionType.SAVE_USER_PREFERENCES_FAILED;
    message: string;
}

export interface IRequestUserPreferences {
    type: SharedComponentsActionType.REQUEST_USER_PREFERENCES;
}
export interface IReceiveUserPreferences {
    type: SharedComponentsActionType.RECEIVE_USER_PREFERENCES;
    data: any;
}
export interface IFailedUserPreferences {
    type: SharedComponentsActionType.FAILED_USER_PREFERENCES;
    message: string;
}

export interface IToggleSettingsPanel {
    type: SharedComponentsActionType.TOGGLE_SETTINGS_PANEL;
    toggle: boolean;
}

export interface IRequestProfileAction {
    type: SharedComponentsActionType.REQUEST_MY_PROFILE;
}

export interface IReceiveProfileAction {
    type: SharedComponentsActionType.RECEIVE_MY_PROFILE;
    profile: IProfile;
}

export interface IRequestDelegationsAction {
    type: SharedComponentsActionType.REQUEST_MY_DELEGATIONS;
    loggedInAlias: string;
    tenantId?: number;
    appName?: string;
}

export interface IReceiveDelegationsAction {
    type: SharedComponentsActionType.RECEIVE_MY_DELEGATIONS;
    userDelegations: Record<string, any>[];
}

export interface IReceiveTenantDelegations {
    type: SharedComponentsActionType.RECEIVE_TENANT_DELEGATIONS;
    tenantDelations: ITenantDelegationObj;
}

export interface IFailedProfileAction {
    type: SharedComponentsActionType.FAILED_PROFILE;
    profileErrorMessage: string;
}

export interface IUpdateUserAlias {
    type: SharedComponentsActionType.UPDATE_USER_ALIAS;
    userAlias: string;
    userName: string;
}

export interface IFailedDelegationsAction {
    type: SharedComponentsActionType.FAILED_DELEGATIONS;
    delegationsErrorMessage: string;
}

export interface IRequestSummaryAction {
    type: SharedComponentsActionType.REQUEST_MY_SUMMARY;
    userAlias: string;
}

export interface IReceiveSummaryAction {
    type: SharedComponentsActionType.RECEIVE_MY_SUMMARY;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    summary: any;
}

export interface IFailedSummaryAction {
    type: SharedComponentsActionType.FAILED_SUMMARY;
    summaryErrorMessage: string;
}

export interface IFailedHistoryAction {
    type: SharedComponentsActionType.FAILED_HISTORY;
    historyErrorMessage: string;
}

export interface IRequestTenantInfoAction {
    type: SharedComponentsActionType.REQUEST_TENANT_INFO;
}

export interface IReceiveTenantInfoAction {
    type: SharedComponentsActionType.RECEIVE_TENANT_INFO;
    tenantInfo: any;
}

export interface IRequestOutofSyncSummaryAction {
    type: SharedComponentsActionType.REQUEST_MY_OUT_OF_SYNC_SUMMARY;
}

export interface IReceiveOutofSyncSummaryAction {
    type: SharedComponentsActionType.RECEIVE_MY_OUT_OF_SYNC_SUMMARY;
    outOfSyncSummary: any;
}

export interface IFailedOutofSyncSummaryAction {
    type: SharedComponentsActionType.FAILED_OUT_OF_SYNC_SUMMARY;
    outOfSyncErrorMessage: any;
}

export interface IFailedTenantInfoAction {
    type: SharedComponentsActionType.FAILED_TENANT_INFO;
    tenantInfoErrorMessage: string;
}

export interface IUpdatePanelState {
    type: SharedComponentsActionType.UPDATE_PANEL_STATE;
    isOpen: boolean;
}

export interface IUpdateSelectedSummarytoPending {
    type: SharedComponentsActionType.UPDATE_SELECTED_SUMMARY_TO_PENDING;
}

export interface IUpdateSelectedSummarytoOutOfSync {
    type: SharedComponentsActionType.UPDATE_SELECTED_SUMMARY_TO_OUT_OF_SYNC;
}

export interface IUpdateGroupedSummary {
    type: SharedComponentsActionType.UPDATE_GROUPED_SUMMARY;
    summaryGroupedBy: string;
}

export interface IUpdateFilterValue {
    type: SharedComponentsActionType.UPDATE_FILTER_VALUE;
    filterValue: string;
}

export interface IUpdateBulkUploadConcurrentValue {
    type: SharedComponentsActionType.UPDATE_BULK_UPLOAD_CONCURRENT_VALUE;
    bulkUploadConcurrentValue: number;
}

export interface IUpdateBulkFailedMsg {
    type: SharedComponentsActionType.UPDATE_BULK_FAILED_VALUE;
    bulkFailedMessage: string[];
}

export interface IUpdateCardView {
    type: SharedComponentsActionType.UPDATE_CARD_VIEW_TYPE;
    isCardViewSelected: boolean;
}

export interface IUpdateBulkSelected {
    type: SharedComponentsActionType.UPDATE_BULK_SELECTED;
    isBulkSelected: boolean;
}

export interface IUpdateBulkvalue {
    type: SharedComponentsActionType.UPDATE_BULK_STATUS;
    bulkStatus: boolean;
}

export interface IUpdateBulkFailed {
    type: SharedComponentsActionType.UPDATE_BULK_FAILED;
    bulkApproveFailed: boolean;
}

export interface IUpdateIsProcessingBulkApprovalAction {
    type: SharedComponentsActionType.UPDATE_IS_PROCESSING_BULK_APPROVAL;
    isProcessingBulkApproval: boolean;
}

export interface IUpdateApprovalRecords {
    type: SharedComponentsActionType.UPDATE_APPROVAL_RECORDS;
    approveRecords: Array<any>;
    subAction: string;
}

export interface IRequestMyHistoryAction {
    type: SharedComponentsActionType.REQUEST_MY_HISTORY;
    page: number;
    sortColumn: string;
    sortDirection: string;
    searchCriteria: string;
    timePeriod: number;
    tenantId: string;
}

export interface IReceiveMyHistoryAction {
    type: SharedComponentsActionType.RECEIVE_MY_HISTORY;
    history: any;
}

export interface IUpdateSelectedPage {
    type: SharedComponentsActionType.UPDATE_SELECTED_PAGE;
    currentPage: string;
}

export interface IUpdateGroupedHistory {
    type: SharedComponentsActionType.UPDATE_GROUPED_HISTORY;
    historyGroupedBy: string;
}

export interface IRequestDownloadHistory {
    type: SharedComponentsActionType.REQUEST_DOWNLOAD_HISTORY;
    monthsOfData: number;
    searchCriteria: string;
    sortField: string;
    sortDirection: string;
    tenantId: string;
}

export interface IReceiveDownloadHistory {
    type: SharedComponentsActionType.RECEIVE_DOWNLOAD_HISTORY;
}

export interface IUpdateHistoryData {
    type: SharedComponentsActionType.UPDATE_HISTORY_DATA;
    historyData: any;
    totalRecords: number;
}

export interface IToggleTeachingBubbleVisibility {
    type: SharedComponentsActionType.TOGGLE_TEACHING_BUBBLE_VISIBILITY;
}

export interface IUpdateTeachingStep {
    type: SharedComponentsActionType.UPDATE_TEACHING_STEP;
    newStep: IFeaturesIntroductionStep;
}

export interface IToggleDetailScreen {
    type: SharedComponentsActionType.TOGGLE_DETAIL_SCREEN;
}

export interface IRequestFilteredUsersAction {
    type: SharedComponentsActionType.REQUEST_FILTERED_USERS;
    filterText: string;
}

export interface IReceiveFilteredUsersAction {
    type: SharedComponentsActionType.RECEIVE_FILTERED_USERS;
    filteredUsers: object[];
}

export interface ISetSelectedSummaryTileRef {
    type: SharedComponentsActionType.SET_SELECTED_SUMMARY_TILE_REF;
    tileRef: any;
}

export interface IFailedDownloadHistory {
    type: SharedComponentsActionType.FAILED_DOWNLOAD_HISTORY;
    downloadErrorMessage: string;
}

export interface IUpdatePeoplePickerSelection {
    type: SharedComponentsActionType.UPDATE_PEOPLEPICKER_SELECTION;
    peoplePickerSelections: object[];
}

export interface IUpdatePeoplePickerHasError {
    type: SharedComponentsActionType.UPDATE_PEOPLEPICKER_HASERROR;
    peoplePickerHasError: boolean;
}

export interface IReceiveSubmitterImages {
    type: SharedComponentsActionType.RECEIVE_SUBMITTER_IMAGES;
    submitterImages: IGraphPhoto[];
}

export interface IConcatSubmitterImages {
    type: SharedComponentsActionType.CONCAT_SUBMITTER_IMAGES;
    newSubmitterImages: IGraphPhoto[];
}

export interface IRequestPullTenantSummary {
    type: SharedComponentsActionType.REQUEST_PULL_TENANT_SUMMARY;
    tenantId: number;
    userAlias: string;
    filterCriteria?: any;
    isExternalTenantInfoRequired?: boolean;
}

export interface IReceivePullTenantSummary {
    type: SharedComponentsActionType.RECEIVE_PULL_TENANT_SUMMARY;
    pullTenantSummary: object[];
}

export interface IFailedPullTenantSummary {
    type: SharedComponentsActionType.FAILED_PULL_TENANT_SUMMARY;
    errorMessage: string;
}

export interface IRefreshBulkState {
    type: SharedComponentsActionType.REFRESH_BULK_STATE;
}

export interface IUpdateRetainBulkSelection {
    type: SharedComponentsActionType.UPDATE_RETAIN_BULK_SELECTION;
    isBulkSelectionRetained: boolean;
}

export interface IRequestExternalTenantInfo {
    type: SharedComponentsActionType.REQUEST_EXTERNAL_TENANT_INFO;
    tenantId: number;
    userAlias: string;
}

export interface IReceiveExternalTenantInfo {
    type: SharedComponentsActionType.RECEIVE_EXTERNAL_TENANT_INFO;
    externalTenantInfo: object;
}

export interface IFailedExternalTenantInfo {
    type: SharedComponentsActionType.FAILED_EXTERNAL_TENANT_INFO;
    errorMessage: string;
}

export interface IUpdateFailedPullTenantRequests {
    type: SharedComponentsActionType.UPDATE_FAILED_PULLTENANT_REQUESTS;
    failedRequests: IActionResponseObject[];
}

export interface IUpdatePullTenantSearchCriteria {
    type: SharedComponentsActionType.UPDATE_PULLTENANT_SEARCH_CRITERIA;
    searchCriteria: object[];
}

export interface IUpdatePullTenantSearchSelection {
    type: SharedComponentsActionType.UPDATE_PULLTENANT_SEARCH_SELECTION;
    searchSelection: number;
}

export interface IUpdateTableRowCount {
    type: SharedComponentsActionType.UPDATE_TABLE_ROW_COUNT;
    tableRowCount: number;
}

export interface IRequestPullTenantSummaryCount {
    type: SharedComponentsActionType.REQUEST_PULLTENANT_SUMMARY_COUNT;
    userAlias: string;
}

export interface IReceivePullTenantSummaryCount {
    type: SharedComponentsActionType.RECEIVE_PULLTENANT_SUMMARY_COUNT;
    pullTenantSummaryCount: IPullTenantSummaryCountObject[];
    totalPullTenantCount: number;
}

export interface IUpdateSelectedTenantDelegation {
    type: SharedComponentsActionType.UPDATE_SELECTED_TENANT_DELEGATION;
    selectedTenantDelegation: IDelegationObj;
}

export interface IUpdateSuccessfulPullTenantRequests {
    type: SharedComponentsActionType.UPDATE_SUCCESSFUL_PULLTENANT_REQUESTS;
    tenantId: number;
    requests: string[];
}

export interface IToggleProfilePanel {
    type: SharedComponentsActionType.TOGGLE_PROFILE_PANEL;
    isOpen: boolean;
}

export interface IToggleAccessibilityPanel {
    type: SharedComponentsActionType.TOGGLE_ACCESSIBILITY_PANEL;
    isOpen: boolean;
}
