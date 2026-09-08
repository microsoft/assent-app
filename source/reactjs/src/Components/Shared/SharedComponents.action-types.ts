import { IGrouping } from '../../Helpers/groupPendingApprovals';
import { SummaryExportColumnFormat } from '../Summary/SummaryExport.constants';
import {
    IActionResponseObject,
    IDelegationObj,
    IFeaturesIntroductionStep,
    IGraphPhoto,
    IHistoryInsights,
    IProfile,
    IPullTenantSummaryCountObject,
    IQuickTourListItem,
    ISummaryInsights,
    ITenantDelegationObj,
    IUserDelegationEntry,
    ICustomStorageParameters,
    IFeedbackInputUpdate,
    IFeedbackInputDelete,
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
    UPDATE_VISIBLE_COLUMNS = 'UPDATE_VISIBLE_COLUMNS',
    UPDATE_HISTORY_DATA = 'UPDATE_HISTORY_DATA',
    TOGGLE_DETAIL_SCREEN = 'TOGGLE_DETAIL_SCREEN',
    REQUEST_FILTERED_USERS = 'REQUEST_FILTERED_USERS',
    RECEIVE_FILTERED_USERS = 'RECEIVE_FILTERED_USERS',
    SET_SELECTED_SUMMARY_TILE_REF = 'SET_SELECTED_SUMMARY_TILE_REF',
    FAILED_DOWNLOAD_HISTORY = 'FAILED_DOWNLOAD_HISTORY',
    REQUEST_DOWNLOAD_SUMMARY = 'REQUEST_DOWNLOAD_SUMMARY',
    RECEIVE_DOWNLOAD_SUMMARY = 'RECEIVE_DOWNLOAD_SUMMARY',
    FAILED_DOWNLOAD_SUMMARY = 'FAILED_DOWNLOAD_SUMMARY',
    CLEAR_DOWNLOAD_SUMMARY_ERROR = 'CLEAR_DOWNLOAD_SUMMARY_ERROR',
    TOGGLE_SETTINGS_PANEL = 'TOGGLE_SETTINGS_PANEL',

    SAVE_USER_PREFERENCES_REQUEST = 'SAVE_USER_PREFERENCES_REQUEST',
    SAVE_USER_PREFERENCES_RESPONSE = 'SAVE_USER_PREFERENCES_RESPONSE',
    SAVE_USER_PREFERENCES_FAILED = 'SAVE_USER_PREFERENCES_FAILED',

    REQUEST_USER_PREFERENCES = 'REQUEST_USER_PREFERENCES',
    RECEIVE_USER_PREFERENCES = 'RECEIVE_USER_PREFERENCES',
    FAILED_USER_PREFERENCES = 'FAILED_USER_PREFERENCES',
    REQUEST_FLIGHTING_DATA = 'REQUEST_FLIGHTING_DATA',
    RECEIVE_FLIGHTING_DATA = 'RECEIVE_FLIGHTING_DATA',
    SUBSCRIBE_FLIGHTING_FEATURES = 'SUBSCRIBE_FLIGHTING_FEATURES',
    UNSUBSCRIBE_FLIGHTING_FEATURES = 'UNSUBSCRIBE_FLIGHTING_FEATURES',
    SUBSCRIBE_FLIGHTING_FEATURES_SUCCESS = 'SUBSCRIBE_FLIGHTING_FEATURES_SUCCESS',
    SUBSCRIBE_FLIGHTING_FEATURES_FAILED = 'SUBSCRIBE_FLIGHTING_FEATURES_FAILED',
    SUBMIT_FLIGHTING_FEATURE_FEEDBACK = 'SUBMIT_FLIGHTING_FEATURE_FEEDBACK',
    FLIGHTING_FEATURE_FEEDBACK_FAILED = 'FLIGHTING_FEATURE_FEEDBACK_FAILED',

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
    INITIATE_SEARCH = 'INITIATE_SEARCH',
    SAVE_SEARCH_RESULTS = 'SAVE_SEARCH_RESULTS',
    TOGGLE_SEARCH_RESULTS_VIEW = 'TOGGLE_SEARCH_RESULTS_VIEW',
    TOGGLE_QUICKTOUR = 'TOGGLE_QUICKTOUR',
    SET_QUICKTOUR_DATA = 'SET_QUICKTOUR_DATA',
    REQUEST_QUICKTOUR_INFO = 'REQUEST_QUICKTOUR_INFO',
    RECEIVE_QUICKTOUR_INFO = 'RECEIVE_QUICKTOUR_INFO',
    POST_QUICKTOUR_INFO = 'POST_QUICKTOUR_INFO',
    SET_UNREAD_QUICKTOURS = 'SET_UNREAD_QUICKTOURS',
    REQUEST_INSIGHTS = 'REQUEST_INSIGHTS',
    RECEIVE_INSIGHTS = 'RECEIVE_INSIGHTS',
    POST_FEEDBACK = 'POST_FEEDBACK',
    UPDATE_FEEDBACK_INPUT = 'UPDATE_FEEDBACK_INPUT',
    DELETE_FEEDBACK_INPUT = 'DELETE_FEEDBACK_INPUT',
    UPDATE_PROPERTY_FILTERS = 'UPDATE_PROPERTY_FILTERS',
    REQUEST_SUGGEST = 'REQUEST_SUGGEST',
    RECEIVE_SUGGEST = 'RECEIVE_SUGGEST',
    CLEAR_SUGGEST = 'CLEAR_SUGGEST',
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
    | IRequestDownloadSummary
    | IReceiveDownloadSummary
    | IFailedDownloadSummary
    | IClearDownloadSummaryError
    | IToggleSettingsPanel
    | ISaveUserPreferencesRequest
    | ISelectedApprovalRequest
    | IUpdateBulkApprovalRequest
    | ISaveUserPreferencesResponse
    | ISaveUserPreferencesFailed
    | IRequestUserPreferences
    | IReceiveUserPreferences
    | IRequestFlightingData
    | IReceiveFlightingData
    | ISubscribeFlightingFeatures
    | IUnsubscribeFlightingFeatures
    | ISubscribeFlightingFeaturesSuccess
    | ISubscribeFlightingFeaturesFailed
    | ISubmitFlightingFeatureFeedback
    | IFlightingFeatureFeedbackFailed
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
    | IToggleAccessibilityPanel
    | IInitiateSearch
    | ISaveSearchResults
    | IToggleSearchResultsView
    | IToggleAccessibilityPanel
    | IToggleQuickTour
    | ISetQuickTourData
    | IRequestQuickTourInfo
    | IRecieveQuickTourInfo
    | IPostQuickTourInfo
    | IClearUnreadQuickTours
    | IRequestInsights
    | IReceiveInsights
    | IPostFeedback
    | IUpdateFeedbackInput
    | IDeleteFeedbackInput
    | IUpdatePropertyFilters
    | IUpdateVisibleColumns
    | IRequestSuggest
    | IReceiveSuggest
    | IClearSuggest;

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
    preserveSessionState?: boolean;
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
    preserveSessionState?: boolean;
}
export interface IReceiveUserPreferences {
    type: SharedComponentsActionType.RECEIVE_USER_PREFERENCES;
    data: any;
    preserveSessionState?: boolean;
    digestPreference?: any;
    teamsNotificationsEnabled?: boolean | null;
}
export interface IFailedUserPreferences {
    type: SharedComponentsActionType.FAILED_USER_PREFERENCES;
    message: string;
}

export interface IRequestFlightingData {
    type: SharedComponentsActionType.REQUEST_FLIGHTING_DATA;
}
export interface IReceiveFlightingData {
    type: SharedComponentsActionType.RECEIVE_FLIGHTING_DATA;
    myFlightingData: any;
    allFlightingData: any;
}

export interface ISubscribeFlightingFeatures {
    type: SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES;
    featureNames: string[];
}

export interface IUnsubscribeFlightingFeatures {
    type: SharedComponentsActionType.UNSUBSCRIBE_FLIGHTING_FEATURES;
    featureNames: string[];
}

export interface ISubscribeFlightingFeaturesSuccess {
    type: SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES_SUCCESS;
    message: string;
}

export interface ISubscribeFlightingFeaturesFailed {
    type: SharedComponentsActionType.SUBSCRIBE_FLIGHTING_FEATURES_FAILED;
    message: string;
}

export interface ISubmitFlightingFeatureFeedback {
    type: SharedComponentsActionType.SUBMIT_FLIGHTING_FEATURE_FEEDBACK;
    featureName: string;
    vote: string;
}

export interface IFlightingFeatureFeedbackFailed {
    type: SharedComponentsActionType.FLIGHTING_FEATURE_FEEDBACK_FAILED;
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
    userDelegations: IUserDelegationEntry[];
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
    onBehalfUserUpn: string;
    onBehalfUserId: string;
}

export interface IFailedDelegationsAction {
    type: SharedComponentsActionType.FAILED_DELEGATIONS;
    delegationsErrorMessage: string;
}

export interface IRequestSummaryAction {
    type: SharedComponentsActionType.REQUEST_MY_SUMMARY;
    userAlias: string;
    isSubmittedRequest?: boolean;
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
    tenantList: any;
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
    isOpen: boolean;
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

export interface IRequestDownloadSummary {
    type: SharedComponentsActionType.REQUEST_DOWNLOAD_SUMMARY;
    userAlias: string;
    columns: string[];
    columnHeaders: string[];
    filters: Record<string, string[]>;
    columnFormats: Record<string, SummaryExportColumnFormat>;
}

export interface IReceiveDownloadSummary {
    type: SharedComponentsActionType.RECEIVE_DOWNLOAD_SUMMARY;
}

export interface IFailedDownloadSummary {
    type: SharedComponentsActionType.FAILED_DOWNLOAD_SUMMARY;
    downloadErrorMessage: string;
}

export interface IClearDownloadSummaryError {
    type: SharedComponentsActionType.CLEAR_DOWNLOAD_SUMMARY_ERROR;
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

export interface IInitiateSearch {
    type: SharedComponentsActionType.INITIATE_SEARCH;
    userAlias: string;
    userInput: string;
}

export interface ISaveSearchResults {
    type: SharedComponentsActionType.SAVE_SEARCH_RESULTS;
    searchResults: any;
}

export interface IToggleSearchResultsView {
    type: SharedComponentsActionType.TOGGLE_SEARCH_RESULTS_VIEW;
    isOn: boolean;
}

export interface IToggleQuickTour {
    type: SharedComponentsActionType.TOGGLE_QUICKTOUR;
}

export interface ISetQuickTourData {
    type: SharedComponentsActionType.SET_QUICKTOUR_DATA;
    quickTourData: IQuickTourListItem[];
}

export interface IRequestQuickTourInfo {
    type: SharedComponentsActionType.REQUEST_QUICKTOUR_INFO;
}

export interface IRecieveQuickTourInfo {
    type: SharedComponentsActionType.RECEIVE_QUICKTOUR_INFO;
    unreadQuickTours: IQuickTourListItem[];
    readQuickTours: IQuickTourListItem[];
    updatedQuickTourList: Array<string>;
}

export interface IPostQuickTourInfo {
    type: SharedComponentsActionType.POST_QUICKTOUR_INFO;
    unReadQuickTours: Array<string>;
}

export interface IClearUnreadQuickTours {
    type: SharedComponentsActionType.SET_UNREAD_QUICKTOURS;
}

export interface IRequestInsights {
    type: SharedComponentsActionType.REQUEST_INSIGHTS;
    pageType?: string;
    timePeriod?: number;
}
export interface IReceiveInsights {
    type: SharedComponentsActionType.RECEIVE_INSIGHTS;
    summaryInsights?: ISummaryInsights;
    historyInsights?: IHistoryInsights;
}

export interface IPostFeedback {
    type: SharedComponentsActionType.POST_FEEDBACK;
    customStorageParameters: ICustomStorageParameters;
    documentNumber: string;
}

export interface IUpdateFeedbackInput {
    type: SharedComponentsActionType.UPDATE_FEEDBACK_INPUT;
    payload: IFeedbackInputUpdate;
}

export interface IDeleteFeedbackInput {
    type: SharedComponentsActionType.DELETE_FEEDBACK_INPUT;
    payload: IFeedbackInputDelete;
}

export interface IUpdatePropertyFilters {
    type: SharedComponentsActionType.UPDATE_PROPERTY_FILTERS;
    propertyFilters: Record<string, string[]>;
}

export interface IUpdateVisibleColumns {
    type: SharedComponentsActionType.UPDATE_VISIBLE_COLUMNS;
    tenantType: string;
    columns: string[];
}

export interface IRequestSuggest {
    type: SharedComponentsActionType.REQUEST_SUGGEST;
    query: string;
}

export interface IReceiveSuggest {
    type: SharedComponentsActionType.RECEIVE_SUGGEST;
    query: string;
    suggestTerms: string[];
    suggestRequests: ISuggestRequestItem[];
}

export interface IClearSuggest {
    type: SharedComponentsActionType.CLEAR_SUGGEST;
}

export interface ISuggestRequestItem {
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    title: string;
    appName: string;
    unitValueText: string;
    submitterName: string;
    businessProcessName: string | null;
}
