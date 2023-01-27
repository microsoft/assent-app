import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { IGrouping } from '../../Helpers/groupPendingApprovals';
import { detailsReducerName } from './Details/Details.reducer';
import { IDetailsState } from './Details/Details.types';
import { sharedComponentsReducerName } from './SharedComponents.reducer';

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
    outOfSyncSummary: any;
    isLoading: boolean;
    isLoadingSummary: boolean;
    isLoadingHistory: boolean;
    isDownloadingHistory: boolean;
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
    isCardViewSelected: boolean;
    selectedApprovalRecords: object[];
    historyTenantIdFilter: string;
    userDelegations: object[];
    toggleDetailsScreen: boolean;
    filteredUsers: object[] | null;
    selectedSummaryTileRef: any;
    historyTotalRecords: number;
    historyDownloadHasError: boolean;
    historyDownloadErrorMessage: string;
    isSettingPanelOpen: boolean;
    userPreferences: any;
    userPreferencesSuccessMessage: string;
    userPreferencesFailureMessage: string;
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
}

export interface ISharedComponentsPersistentState {
    userAlias: string;
    userName: string;
    teachingBubbleVisibility: boolean;
    teachingBubbleStep: IFeaturesIntroductionStep;
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
    ApprovalIdentifier: { DocumentNumber: string; DisplayDocumentNumber: string; FiscalYear: string };
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

export interface ITenantDelegationObj {
    tenantId: number;
    appName: string;
    delegations: IDelegationObj[];
}
