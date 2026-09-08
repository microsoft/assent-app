import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { IGrouping } from '../../Helpers/groupPendingApprovals';
import { detailsReducerName } from './Details/Details.reducer';
import { IDetailsState } from './Details/Details.types';
import { sharedComponentsReducerName } from './SharedComponents.reducer';
import { ISuggestRequestItem } from './SharedComponents.action-types';

export interface IComponentsAppState extends IDefaultState {
    SharedComponentsPersistentReducer: ISharedComponentsPersistentState;
    dynamic?: {
        [sharedComponentsReducerName]: ISharedComponentsState;
        [detailsReducerName]: IDetailsState;
    };
}

export type SelectedSummaryType = 'outofsync' | 'pending';

export interface ISharedComponentsState {
    profile: IProfile | null;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    summary: any;
    history: any;
    historyData: any;
    tenantList: any;
    outOfSyncSummary: any;
    isLoading: boolean;
    isLoadingSummary: boolean;
    isLoadingHistory: boolean;
    isDownloadingHistory: boolean;
    isDownloadingSummary: boolean;
    isLoadingOutOfSyncSummary: boolean;
    hasError: boolean;
    outOfSyncHasError: boolean;
    errorMessage: string | null;
    outOfSyncErrorMessage: string | null;
    isPanelOpen: boolean;
    tenantInfo: any;
    historyGroupedBy: string;
    historyHasError: boolean;
    summaryErrorMessage: string | null;
    historyErrorMessage: string | null;
    isLoadingTenantInfo: boolean;
    tenantInfoHasError: boolean;
    tenantInfoErrorMessage: string | null;
    isLoadingProfile: boolean;
    profileHasError: boolean;
    profileErrorMessage: string | null;
    delegationsHasError: boolean;
    delegationsErrorMessage: string | null;
    selectedSummary: SelectedSummaryType;
    selectedPage: string;
    historySelectedPage: number;
    sortColumnField: string;
    sortDirection: string;
    historySearchCriteria: string;
    historyTimePeriod: number;
    summaryGroupedBy: string;
    filterValue: string;
    isBulkSelected: boolean;
    bulkApproveFailed: boolean;
    bulkFailedMsg: string[];
    bulkApproveStatus: boolean;
    isProcessingBulkApproval: boolean;
    selectedApprovalRecords: object[];
    historyTenantIdFilter: string;
    userDelegations: IUserDelegationEntry[];
    toggleDetailsScreen: boolean;
    filteredUsers: object[] | null;
    selectedSummaryTileRef: any;
    historyTotalRecords: number;
    historyDownloadHasError: boolean;
    historyDownloadErrorMessage: string;
    summaryDownloadHasError: boolean;
    summaryDownloadErrorMessage: string;
    isSettingPanelOpen: boolean;
    userPreferences: any;
    userPreferencesSuccessMessage: string | null;
    userPreferencesFailureMessage: string | null;
    detailsDefaultView: string;
    historyDefaultView: string;
    defaultViewType: string;
    DefaultTenant: string;
    bulkActionConcurrentCall: number;
    peoplePickerSelections: object[];
    peoplePickerHasError: boolean;
    submitterImages: IGraphPhoto[];
    isLoadingSubmitterImages: boolean;
    pullTenantSummaryData: object[];
    isLoadingPullTenantData: boolean;
    isPaginationEnabled: boolean;
    isBulkSelectionRetained: boolean;
    externalTenantInfo: object | null;
    externalTenantInfoHasError: boolean;
    externalTenantInfoErrorMessage: string | null;
    pullTenantSummaryHasError: boolean;
    pullTenantSummaryErrorMessage: string | null;
    failedPullTenantRequests: IActionResponseObject[];
    pullTenantSearchCriteria: object[] | null;
    pullTenantSearchSelection: number;
    tableRowCount: number;
    pullTenantSummaryCount: IPullTenantSummaryCountObject[];
    totalPullTenantCount: number;
    tenantDelegations: ITenantDelegationObj | null;
    selectedTenantDelegation: IDelegationObj | null;
    isLoadingPullTenantSummaryCount: boolean;
    successfulPullTenantRequests: string[];
    successfulPullTenantCount: number;
    successfulPullTenantCountDict: IPullTenantSuccessfulCountDict;
    isProfilePanelOpen: boolean;
    isAccessibilityPanelOpen: boolean;
    isLoadingSearchResults: boolean;
    searchResults: any;
    isSearchResultsViewOpen: boolean;
    isQuickTourOpen: boolean;
    quickTourData: IQuickTourListItem[];
    unreadQuickTours: IQuickTourListItem[];
    readQuickTours: IQuickTourListItem[];
    updatedQuickTourList: Array<string>;
    myFlightingData: any;
    allFlightingData: any;
    isLoadingFlightingData: boolean;
    flightingFeatureFeedback: Record<string, string>;
    insightsData: IInsightsData;
    feedback: IFeedbackState;
    isSummaryCollapsed: boolean;
    propertyFilters: Record<string, string[]>;
    isSubmitterView: boolean;
    digestPreference: IDigestPreference | null;
    teamsNotificationsEnabled: boolean | null;
    suggestTerms: string[];
    suggestRequests: ISuggestRequestItem[];
    isLoadingSuggest: boolean;
    suggestCache: { query: string; terms: string[]; requests: ISuggestRequestItem[] } | null;
}

export interface IDigestPreference {
    DigestEnabled: boolean;
    Cadence: number;
    TimeZone: string;
}

export interface ISharedComponentsPersistentState {
    userAlias: string;
    userName: string;
    onBehalfUserUpn: string;
    teachingBubbleVisibility: boolean;
    teachingBubbleStep: IFeaturesIntroductionStep;
    onBehalfUserId: string;
    visibleColumnsDefault: string[];
    visibleColumnsPullTenant: string[];
    isCardViewSelected: boolean;
    isViewTypeInitialized: boolean;
    isColumnsInitialized: boolean;
}
export interface IProfile {
    userPrincipalName: string;
    displayName: string;
    jobTitle: string;
    officeLocation: string;
    givenName?: string;
    surname?: string;
}

export interface ITileSummary {
    submitter: string;
    unitValue: string;
    displayDocNumber: string;
}

export interface IFeaturesIntroductionStep {
    step: number;
    headline: string;
    target: string;
    successButtonLabel: string;
    declineButtonLabel: string;
    successNextStep: number; //-1 if done
    declineNextStep: number; //-1 if done
}

export interface IGraphPhoto {
    alias: string;
    image: string | null;
}

export interface ISummaryObject {
    ApprovalIdentifier: IApprovalIdentifier;
}

export interface IApprovalIdentifier {
    DocumentNumber: string;
    DisplayDocumentNumber: string;
    FiscalYear: string;
}

export interface IActionResponseObject {
    Key: string;
    Value: string;
}

export interface IPullTenantSummaryCountObject {
    TenantId: number;
    AppName: string;
    CondensedAppName: string;
    Count: number;
}

export interface IPullTenantSuccessfulCountDict {
    [key: number]: number;
}

export interface IDelegationObj {
    alias: string;
    name: string;
}

export interface IUserDelegator {
    Id: string;
    DisplayName: string;
    UserPrincipalName: string;
}

export interface IUserDelegationApp {
    appId: string;
    appName: string;
    isDelegationPlatform: boolean;
    // ISO-8601 UTC. Nullable: legacy table rows may have no DateTo, and Delegation
    // Platform entries may not set an end date.
    endDate?: string | null;
}

export interface IUserDelegationEntry {
    upn: string;
    delegator: IUserDelegator;
    apps: IUserDelegationApp[];
}

// Derived client-side from the active delegation entry's apps[].endDate.
// The backend only exposes a raw endDate; everything else is computed in the
// frontend by computeDelegationExpiryDetails (see Helpers/delegationExpiry.ts).
export type DelegationExpirySeverity = 'none' | 'warning';

export interface IDelegationExpiryDetails {
    delegatorAlias: string;
    daysUntilExpiration: number | null;
    expirySeverity: DelegationExpirySeverity;
    expirationTimeFormatted: string | null;
    expirationTimeZone: string | null;
    expirationDateTimeFormatted: string | null;
    expiringAppName: string | null;
}

export interface ITenantDelegationObj {
    tenantId: number;
    appName: string;
    delegations: IDelegationObj[];
}

export interface IQuickTourSlide {
    title: string;
    subtext: string;
    image: string;
}

export interface IQuickTourListItem {
    id: number;
    name: string;
    isEnabled: boolean;
    isViewed: boolean;
    slides: IQuickTourSlide[];
    summary: string;
    summaryImage: string;
}

export interface ISummaryInsights {
    HighPriority: { requests: string[]; reason?: string };
}

export interface IHistoryInsights {
    TotalCounts: number[];
}

export interface IInsightsData {
    summaryInsights?: ISummaryInsights;
    historyInsights?: IHistoryInsights;
}

export interface ICustomStorageParameters {
    PartitionKeyPath: string;
    CollectionName: string;
}

export interface IFeedbackData {
    FeatureName: string;
    DocumentNumber: string;
    FiscalYear: string;
    Inputs: Array<{
        id: string;
        InputType: string;
        InputValue: string;
    }>;
}

export interface IFeedbackState {
    feedbackByFeature: { [featureName: string]: IFeedbackData };
    isLoading: boolean;
    hasError: boolean;
    errorMessage: string | null;
}

export interface IFeedbackInputUpdate {
    id: string;
    featureName: string;
    inputType: string;
    inputValue: string;
    documentNumber?: string;
    fiscalYear?: string;
}

export interface IFeedbackInputDelete {
    id: string;
    featureName: string;
}

// DeepSearch — Enriched search result types

export interface IAttachmentMatch {
    AttachmentId: string | null;
    AttachmentName: string;
    Snippet: string;
}

export interface ISearchHighlight {
    Source: string;
    FieldPath: string | null;
    Terms: string[];
}

export interface ISearchResultDto {
    DocumentNumber: string;
    Highlights: ISearchHighlight[] | null;
    AttachmentMatches: IAttachmentMatch[] | null;
}

export interface IMatchMetadata {
    highlights?: ISearchHighlight[];
    attachmentMatches?: IAttachmentMatch[];
}
