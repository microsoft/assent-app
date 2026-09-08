/* eslint-disable @typescript-eslint/no-use-before-define */
import { sharedComponentsInitialState, sharedComponentsReducerName } from './SharedComponents.reducer';
import { detailsReducerName } from './Details/Details.reducer';
import {
    IActionResponseObject,
    IComponentsAppState,
    IDelegationObj,
    IGraphPhoto,
    IProfile,
    IPullTenantSuccessfulCountDict,
    IPullTenantSummaryCountObject,
    IQuickTourListItem,
    ITenantDelegationObj,
    IUserDelegationEntry,
    IFeedbackData,
} from './SharedComponents.types';
import { groupByTenant, groupByDate, groupBySubmitter, groupByCategory, IGrouping } from '../../Helpers/groupPendingApprovals';
import { GroupingBy } from './Components/GroupingBy';
import { createSelector } from 'reselect';
import { IDropdownOption } from '@fluentui/react';
import { SETTINGS_COACHMARK_ID, TABLE_COLUMNS_DEFAULT, TABLE_COLUMNS_PULLTENANT, DEFAULT_VISIBLE_COLUMNS, parseColumnPreference } from './SharedConstants';
import { ISuggestRequestItem } from './SharedComponents.action-types';

export const getSelectedSummary = (state: IComponentsAppState) => {
    let selectedSummaryData;
    if (state.dynamic && state.dynamic[sharedComponentsReducerName]) {
        const selected = state.dynamic[sharedComponentsReducerName].selectedSummary;
        if (selected === 'pending') {
            selectedSummaryData = state.dynamic[sharedComponentsReducerName].summary;
        } else if (selected === 'outofsync') {
            selectedSummaryData = state.dynamic[sharedComponentsReducerName].outOfSyncSummary;
        }
    } else {
        selectedSummaryData = sharedComponentsInitialState.summary;
    }
    return selectedSummaryData;
};

export const getProfile = (state: IComponentsAppState): IProfile | null => {
    return state.dynamic?.[sharedComponentsReducerName]?.profile || sharedComponentsInitialState.profile;
};

export const getStateCommonTelemetryProperties = (state: any): any => {
    const logData = {};
    if (state.dynamic && state.dynamic[sharedComponentsReducerName]) {
        const profile = state.dynamic[sharedComponentsReducerName].profile;
        if (profile) {
            Object.assign(logData, {
                UserAlias: profile.userPrincipalName,
                LoggedInUserAlias: profile.userPrincipalName,
            });
        }
    }
    if (state.dynamic && state.dynamic[detailsReducerName]) {
        const { documentNumber, displayDocumentNumber, tenantId, tcv } = state.dynamic[detailsReducerName];
        Object.assign(logData, {
            MessageId: tcv,
            Xcv: displayDocumentNumber,
            DocumentNumber: documentNumber,
            DisplayDocumentNumber: displayDocumentNumber,
            TenantId: tenantId,
        });
    }
    return logData;
};

const getSummaryCommonPropertiesMemo = (profile: any): any => {
    if (profile) {
        return { UserAlias: profile.userPrincipalName, LoggedInUserAlias: profile.userPrincipalName };
    } else return {};
};

export const getSummaryCommonPropertiesSelector = createSelector(getProfile, getSummaryCommonPropertiesMemo);

export const getSummary = (state: IComponentsAppState) => {
    return state.dynamic?.[sharedComponentsReducerName]?.summary || sharedComponentsInitialState.summary;
};

export const getSummaryGroupedBy = (state: IComponentsAppState) => {
    return state.dynamic?.[sharedComponentsReducerName]?.summaryGroupedBy || GroupingBy.Tenant;
};

export const getDefaultTenant = (state: IComponentsAppState) => {
    return state.dynamic?.[sharedComponentsReducerName]?.DefaultTenant || sharedComponentsInitialState.DefaultTenant;
};

export const getIsLoadingSummary = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isLoadingSummary || sharedComponentsInitialState.isLoadingSummary
    );
};

export const getIsLoadingSubmitterImages = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isLoadingSubmitterImages ||
        sharedComponentsInitialState.isLoadingSubmitterImages
    );
};

export const getTenantInfo = (state: IComponentsAppState): any => {
    return state.dynamic?.[sharedComponentsReducerName]?.tenantInfo || sharedComponentsInitialState.tenantInfo;
};

export const getFilterValue = (state: IComponentsAppState): string => {
    return state.dynamic?.[sharedComponentsReducerName]?.filterValue || sharedComponentsInitialState.filterValue;
};

export const getPanelOpen = (state: IComponentsAppState): any => {
    return state.dynamic?.[sharedComponentsReducerName]?.isPanelOpen || sharedComponentsInitialState.isPanelOpen;
};

export const getSelectedApprovalRecords = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.selectedApprovalRecords ||
        sharedComponentsInitialState.selectedApprovalRecords
    );
};

export const getSelectedDisplayDocumentNumbers = (state: IComponentsAppState): string[] => {
    const selectedRecords = getSelectedApprovalRecords(state);
    return selectedRecords.map((record: any) => record.DisplayDocumentNumber).filter(Boolean);
};

export const getBulkActionConcurrentCall = (state: IComponentsAppState): number => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.bulkActionConcurrentCall ||
        sharedComponentsInitialState.bulkActionConcurrentCall
    );
};

export const getSelectedSummaryTileRef = (state: IComponentsAppState): number => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.selectedSummaryTileRef ||
        sharedComponentsInitialState.selectedSummaryTileRef
    );
};

export const getIsBulkSelected = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.isBulkSelected || sharedComponentsInitialState.isBulkSelected;
};

export const getBulkFailedMsg = (state: IComponentsAppState): string[] => {
    return state.dynamic?.[sharedComponentsReducerName]?.bulkFailedMsg || sharedComponentsInitialState.bulkFailedMsg;
};

export const getBulkApproveStatus = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.bulkApproveStatus ||
        sharedComponentsInitialState.bulkApproveStatus
    );
};

export const getToggleDetailsScreen = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.toggleDetailsScreen ||
        sharedComponentsInitialState.toggleDetailsScreen
    );
};

export const getIsPanelOpen = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.isPanelOpen || sharedComponentsInitialState.isPanelOpen;
};

export const getIsSettingPanelOpen = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isSettingPanelOpen ||
        sharedComponentsInitialState.isSettingPanelOpen
    );
};

export const getGroupedBySummaryMemo = (summary: any, groupedBy: string, tenantInfo?: any) => {
    let groupedSummaryData = {};
    if (summary && groupedBy && groupedBy != '') {
        switch (groupedBy) {
            case GroupingBy.Tenant:
                groupedSummaryData = groupByTenant(summary);
                break;
            case GroupingBy.Submitter:
                groupedSummaryData = groupBySubmitter(summary);
                break;
            case GroupingBy.Date:
                groupedSummaryData = groupByDate(summary);
                break;
            case GroupingBy.Category:
                groupedSummaryData = groupByCategory(summary, tenantInfo);
                break;
            // default group by tenant
            default:
                groupedSummaryData = groupByTenant(summary);
                break;
        }
    }
    return groupedSummaryData;
};

export const getGroupedBySummary = (state: IComponentsAppState) => {
    let groupedSummaryData;
    if (
        state.dynamic &&
        state.dynamic[sharedComponentsReducerName] &&
        state.dynamic[sharedComponentsReducerName].summary
    ) {
        const groupedBy = state.dynamic[sharedComponentsReducerName].summaryGroupedBy;
        const selected = state.dynamic[sharedComponentsReducerName].selectedSummary;
        const summaryData =
            selected === 'pending'
                ? state.dynamic[sharedComponentsReducerName].summary
                : state.dynamic[sharedComponentsReducerName].outOfSyncSummary;
        switch (groupedBy) {
            case GroupingBy.Tenant:
                groupedSummaryData = groupByTenant(summaryData);
                break;
            case GroupingBy.Submitter:
                groupedSummaryData = groupBySubmitter(summaryData);
                break;
            case GroupingBy.Date:
                groupedSummaryData = groupByDate(summaryData);
                break;
            // default group by tenant
            default:
                groupedSummaryData = groupByTenant(summaryData);
                break;
        }
    }
    return groupedSummaryData;
};

export const getSubmitterImages = (state: IComponentsAppState): IGraphPhoto[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.submitterImages || sharedComponentsInitialState.submitterImages
    );
};

const getAlias = (_: any, alias: string): string => alias;

export const getImageURLForAlias = createSelector(getSubmitterImages, getAlias, (submitterImages, alias) => {
    if (submitterImages.length > 0 && alias) {
        const matchingElement = submitterImages.find((el) => el.alias === alias);
        return matchingElement?.image || null;
    }
    return null;
});

export const isAliasInSubmitters = createSelector(getSubmitterImages, getAlias, (submitterImages, alias) => {
    if (submitterImages.length > 0 && alias) {
        const matchingElement = submitterImages.find((el) => el.alias === alias);
        return !!matchingElement;
    }
    return null;
});

export const getBulkTenantsFromSummary = (
    summary: any,
    tenantInfo: any,
    formatForDropdown?: boolean
): IDropdownOption[] | string[] => {
    let filterMenuItems: string[] = [];
    for (const key in summary) {
        const tenantId = summary[key]['TenantId'];
        const selectedTenant = tenantInfo ? tenantInfo.find((tenant: any) => tenant.tenantId === tenantId) : null;
        if (validateTenantForBulkAction(selectedTenant)) {
            const appName = summary[key]['AppName'];
            if (!filterMenuItems.includes(appName)) {
                filterMenuItems.push(appName);
            }
        }
    }
    if (formatForDropdown) {
        return getDropdownOptions(filterMenuItems);
    } else {
        return filterMenuItems;
    }
};

export const getBulkFilteredDropDownMenuItems = (summary: any, tenantInfo: any): IDropdownOption[] => {
    let filterMenuItems = getBulkTenantsFromSummary(summary, tenantInfo, false) as string[];
    if (tenantInfo) {
        for (const tenantIndex in tenantInfo) {
            const tenantObj = tenantInfo[tenantIndex];
            const actionSubmissionType = tenantObj.actionSubmissionType;
            const submissionTypeForBulk =
                actionSubmissionType == 1 || actionSubmissionType == 2 || actionSubmissionType == 3;
            if (tenantObj.isPullModelEnabled && submissionTypeForBulk) {
                filterMenuItems.push(tenantObj.appName);
            }
        }
    }
    return getDropdownOptions(filterMenuItems);
};

export const getAllBulkTenantOptions = (tenantInfo: any): IDropdownOption[] => {
    let filterMenuItems: string[] = ['All'];
    tenantInfo?.map(function (item: any) {
        if (validateTenantForBulkAction(item)) {
            if (!filterMenuItems.includes(item.appName)) {
                if (validateTenantTypeIsProd(item)) filterMenuItems.push(item.appName);
            }
        }
    });

    if (tenantInfo) {
        for (const tenantIndex in tenantInfo) {
            const tenantObj = tenantInfo[tenantIndex];
            const actionSubmissionType = tenantObj.actionSubmissionType;
            const submissionTypeForBulk =
                actionSubmissionType == 1 || actionSubmissionType == 2 || actionSubmissionType == 3;
            if (tenantObj.isPullModelEnabled && submissionTypeForBulk && validateTenantTypeIsProd(tenantObj)) {
                filterMenuItems.push(tenantObj.appName);
            }
        }
    }

    return getDropdownOptions(filterMenuItems);
};

//checks if a tenant has at least one action enabled for bulk approval
const validateTenantForBulkAction = (tenant: any): boolean => {
    if (tenant) {
        const actionsubmissiontype = tenant.actionSubmissionType;
        const bulkActionIndex = tenant?.actionDetails?.primary?.findIndex(
            (button: { code: string; isBulkAction: boolean }) => button.isBulkAction
        );
        const isBulkAction = bulkActionIndex >= 0;
        return isBulkAction && (actionsubmissiontype == 1 || actionsubmissiontype == 2 || actionsubmissiontype == 3);
    }
    return false;
};

const validateTenantTypeIsProd = (tenant: any): boolean => {
    return tenant.tenantType.toLowerCase() === 'prod';
};

const getDropdownOptions = (filterMenuItems: string[]): IDropdownOption[] => {
    const allIndex = filterMenuItems.indexOf('All');
    if (allIndex > -1) {
        filterMenuItems.splice(allIndex, 1);
    }
    filterMenuItems.sort();
    const filterMenuProps = filterMenuItems.map((x) => ({
        ['key']: x,
        ['text']: x,
    }));
    return filterMenuProps;
};

export const getHasError = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.hasError || sharedComponentsInitialState.hasError;
};

export const getSummaryErrorMessage = (state: IComponentsAppState): string | null => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.summaryErrorMessage ||
        sharedComponentsInitialState.summaryErrorMessage
    );
};

export const getHistoryGroupedBy = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.historyGroupedBy || sharedComponentsInitialState.historyGroupedBy
    );
};

export const getSelectedPage = (state: IComponentsAppState): string => {
    return state.dynamic?.[sharedComponentsReducerName]?.selectedPage || sharedComponentsInitialState.selectedPage;
};

export const getHistoryData = (state: IComponentsAppState): any => {
    return state.dynamic?.[sharedComponentsReducerName]?.historyData || sharedComponentsInitialState.historyData;
};

export const getTenantList = (state: IComponentsAppState): any => {
    return state.dynamic?.[sharedComponentsReducerName]?.tenantList || sharedComponentsInitialState.tenantList;
};

export const getSortColumnField = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.sortColumnField || sharedComponentsInitialState.sortColumnField
    );
};

export const getSortDirection = (state: IComponentsAppState): string => {
    return state.dynamic?.[sharedComponentsReducerName]?.sortDirection || sharedComponentsInitialState.sortDirection;
};

export const getHistorySearchCriteria = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.historySearchCriteria ||
        sharedComponentsInitialState.historySearchCriteria
    );
};

export const getHistoryTimePeriod = (state: IComponentsAppState): number => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.historyTimePeriod ||
        sharedComponentsInitialState.historyTimePeriod
    );
};

export const getUserDelegations = (state: IComponentsAppState): IUserDelegationEntry[] => {
    return (
        (state.dynamic?.[sharedComponentsReducerName]?.userDelegations as IUserDelegationEntry[]) ||
        (sharedComponentsInitialState.userDelegations as IUserDelegationEntry[])
    );
};

// Returns the user-delegation entry that matches the currently selected delegator
// (identified by the persistent `userAlias`). Case-insensitive UPN-prefix match
// because alias casing can drift between URL query params and the backend UPN.
// Returns null when no delegator is selected ("Me" view) or when the selected
// alias is not present in the delegations list.
export const getSelectedDelegationDetails = (state: IComponentsAppState): IUserDelegationEntry | null => {
    const userAlias = state.SharedComponentsPersistentReducer?.userAlias;
    if (!userAlias) return null;
    const normalized = userAlias.toLowerCase();
    const delegations = getUserDelegations(state);
    for (const entry of delegations) {
        const upn = entry?.delegator?.UserPrincipalName;
        if (!upn) continue;
        const alias = upn.split('@')[0]?.toLowerCase();
        if (alias === normalized) return entry;
    }
    return null;
};

export const getIsLoading = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.isLoading || sharedComponentsInitialState.isLoading;
};

export const getDetailsDefaultView = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.detailsDefaultView ||
        sharedComponentsInitialState.detailsDefaultView
    );
};

export const getHistoryDefaultView = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.historyDefaultView ||
        sharedComponentsInitialState.historyDefaultView
    );
};

export const getDefaultViewType = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.defaultViewType || sharedComponentsInitialState.defaultViewType
    );
};

export const getUserPreferencesFailureMessage = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.userPreferencesFailureMessage ||
        sharedComponentsInitialState.userPreferencesFailureMessage
    );
};

export const getUserPreferencesSuccessMessage = (state: IComponentsAppState): string => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.userPreferencesSuccessMessage ||
        sharedComponentsInitialState.userPreferencesSuccessMessage
    );
};

export const getUserPreferences = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.userPreferences || sharedComponentsInitialState.userPreferences
    );
};

export const getDigestPreference = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.digestPreference ?? sharedComponentsInitialState.digestPreference
    );
};

export const getTeamsNotificationsEnabled = (state: IComponentsAppState): boolean | null => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.teamsNotificationsEnabled ??
        sharedComponentsInitialState.teamsNotificationsEnabled
    );
};

export const getVisibleColumns = (state: IComponentsAppState, tenantType: string): string[] => {
    const prefs = getUserPreferences(state);
    const prefKey = tenantType === 'pullTenant' ? TABLE_COLUMNS_PULLTENANT : TABLE_COLUMNS_DEFAULT;
    const defaultKey = tenantType === 'pullTenant' ? 'PullTenant' : 'Default';
    if (prefs && Array.isArray(prefs)) {
        const columnPref = prefs.find((a: any) => a.UserPreferenceText === prefKey);
        if (columnPref?.UserPreferenceStatus) {
            const parsed = parseColumnPreference(columnPref.UserPreferenceStatus);
            if (parsed) {
                return parsed;
            }
        }
    }
    return DEFAULT_VISIBLE_COLUMNS[defaultKey];
};

export const getCardViewSelected = (state: IComponentsAppState): boolean => {
    return state.SharedComponentsPersistentReducer?.isCardViewSelected ?? false;
};

export const getIsProcessingBulkApproval = (state: IComponentsAppState): boolean => {
    if (state.dynamic) {
        return state.dynamic?.[sharedComponentsReducerName]?.isProcessingBulkApproval;
    } else {
        return sharedComponentsInitialState.isProcessingBulkApproval;
    }
};

export const getBulkApproveFailed = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.bulkApproveFailed ||
        sharedComponentsInitialState.bulkApproveFailed
    );
};

export const getSelectedSummaryPage = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.selectedSummary || sharedComponentsInitialState.selectedSummary
    );
};

export const getIsPaginationEnabled = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isPaginationEnabled ||
        sharedComponentsInitialState.isPaginationEnabled
    );
};

export const getPullTenantSummaryData = (state: IComponentsAppState): object[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.pullTenantSummaryData ||
        sharedComponentsInitialState.pullTenantSummaryData
    );
};

export const getIsLoadingPullTenantData = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isLoadingPullTenantData ||
        sharedComponentsInitialState.isLoadingPullTenantData
    );
};

export const getFilteredTenantInfo = (state: IComponentsAppState): any => {
    const tenantName = state.dynamic?.[sharedComponentsReducerName]?.filterValue;
    const tenantInfo = state.dynamic?.[sharedComponentsReducerName]?.tenantInfo;
    if (tenantName && tenantInfo && tenantName.length > 0 && tenantName !== 'All') {
        const currentTenant = tenantInfo.find((tenant: { appName: string }) => {
            return tenant.appName === tenantName;
        });
        return currentTenant ?? null;
    }
    return null;
};

export const getIsPullTenantSelected = (state: IComponentsAppState): boolean => {
    const tenantName = state.dynamic?.[sharedComponentsReducerName]?.filterValue;
    const tenantInfo = state.dynamic?.[sharedComponentsReducerName]?.tenantInfo;
    if (tenantName && tenantInfo && tenantName.length > 0 && tenantName !== 'All') {
        const currentTenant = tenantInfo.find((tenant: { appName: string }) => {
            return tenant.appName === tenantName;
        });
        return currentTenant?.isPullModelEnabled ?? false;
    }
    return false;
};

export const getTenantAdditionalNotes = (state: IComponentsAppState): string => {
    const tenantName = state.dynamic?.[sharedComponentsReducerName]?.filterValue;
    const tenantInfo = state.dynamic?.[sharedComponentsReducerName]?.tenantInfo;
    if (tenantName && tenantInfo && tenantName.length > 0 && tenantName !== 'All') {
        const currentTenant = tenantInfo.find((tenant: { appName: string }) => {
            return tenant.appName === tenantName;
        });
        return currentTenant?.additionalNotes ?? null;
    }
    return null;
};

export const getTenantDataModelMapping = (state: IComponentsAppState): string => {
    const tenantName = state.dynamic?.[sharedComponentsReducerName]?.filterValue;
    const tenantInfo = state.dynamic?.[sharedComponentsReducerName]?.tenantInfo;
    if (tenantName && tenantInfo && tenantName.length > 0 && tenantName !== 'All') {
        const currentTenant = tenantInfo.find((tenant: { appName: string }) => {
            return tenant.appName === tenantName;
        });
        return currentTenant?.dataModelMapping ?? null;
    }
    return null;
};

export const getTenantDocumentTypeId = (state: IComponentsAppState): string => {
    const tenantName = state.dynamic?.[sharedComponentsReducerName]?.filterValue;
    const tenantInfo = state.dynamic?.[sharedComponentsReducerName]?.tenantInfo;
    if (tenantName && tenantInfo && tenantName.length > 0 && tenantName !== 'All') {
        const currentTenant = tenantInfo.find((tenant: { appName: string }) => {
            return tenant.appName === tenantName;
        });
        return currentTenant?.docTypeId ?? null;
    }
    return null;
};

export const getTenantBusinessProcessName = (state: IComponentsAppState): string => {
    const tenantName = state.dynamic?.[sharedComponentsReducerName]?.filterValue;
    const tenantInfo = state.dynamic?.[sharedComponentsReducerName]?.tenantInfo;
    if (tenantName && tenantInfo && tenantName.length > 0 && tenantName !== 'All') {
        const currentTenant = tenantInfo.find((tenant: { appName: string }) => {
            return tenant.appName === tenantName;
        });
        return currentTenant?.businessProcessName ?? null;
    }
    return null;
};

export const getIsBulkSelectionRetained = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isBulkSelectionRetained ||
        sharedComponentsInitialState.isBulkSelectionRetained
    );
};

export const getExternalTenantInfo = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.externalTenantInfo ||
        sharedComponentsInitialState.externalTenantInfo
    );
};

const getTenantId = (_: any, tenantId: number): number => tenantId;

export const getAppNameFromTenantId = createSelector(getTenantInfo, getTenantId, (tenantInfo, tenantId) => {
    const currentTenant = tenantInfo?.find((tenant: { tenantId: number }) => {
        return tenant.tenantId === tenantId;
    });
    return currentTenant ? currentTenant.appName : null;
});

const getAppName = (_: any, appName: string): string => appName;

export const getTenantIdFromAppName = createSelector(getTenantInfo, getAppName, (tenantInfo, appName) => {
    const currentTenant = tenantInfo?.find((tenant: { appName: string }) => {
        return tenant.appName === appName;
    });
    return currentTenant ? currentTenant.tenantId : null;
});

export const getPullTenantSummaryHasError = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.pullTenantSummaryHasError ||
        sharedComponentsInitialState.pullTenantSummaryHasError
    );
};

export const getPullTenantSummaryErrorMessage = (state: IComponentsAppState): string | null => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.pullTenantSummaryErrorMessage ||
        sharedComponentsInitialState.pullTenantSummaryErrorMessage
    );
};

export const getFailedPullTenantRequests = (state: IComponentsAppState): IActionResponseObject[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.failedPullTenantRequests ||
        sharedComponentsInitialState.failedPullTenantRequests
    );
};

export const getPullTenantSearchCriteria = (state: IComponentsAppState): object[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.pullTenantSearchCriteria ||
        sharedComponentsInitialState.pullTenantSearchCriteria
    );
};

export const getPullTenantSearchSelection = (state: IComponentsAppState): number => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.pullTenantSearchSelection ||
        sharedComponentsInitialState.pullTenantSearchSelection
    );
};

export const getTableRowCount = (state: IComponentsAppState): number => {
    return state.dynamic?.[sharedComponentsReducerName]?.tableRowCount || sharedComponentsInitialState.tableRowCount;
};

export const getPullTenantSummaryCount = (state: IComponentsAppState): IPullTenantSummaryCountObject[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.pullTenantSummaryCount ||
        sharedComponentsInitialState.pullTenantSummaryCount
    );
};

export const getTotalPullTenantCount = (state: IComponentsAppState): number => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.totalPullTenantCount ||
        sharedComponentsInitialState.totalPullTenantCount
    );
};

export const getTenantDelegations = (state: IComponentsAppState): ITenantDelegationObj => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.tenantDelegations ||
        sharedComponentsInitialState.tenantDelegations
    );
};

export const getSelectedTenantDelegation = (state: IComponentsAppState): IDelegationObj => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.selectedTenantDelegation ||
        sharedComponentsInitialState.selectedTenantDelegation
    );
};

export const getExternalTenantInfoHasError = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.externalTenantInfoHasError ||
        sharedComponentsInitialState.externalTenantInfoHasError
    );
};

export const getExternalTenantInfoErrorMessage = (state: IComponentsAppState): string | null => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.externalTenantInfoErrorMessage ||
        sharedComponentsInitialState.externalTenantInfoErrorMessage
    );
};

export const getIsLoadingPullTenantSummaryCount = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isLoadingPullTenantSummaryCount ||
        sharedComponentsInitialState.isLoadingPullTenantSummaryCount
    );
};

const getSuccessfulPullTenantRequests = (state: IComponentsAppState): string[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.successfulPullTenantRequests ||
        sharedComponentsInitialState.successfulPullTenantRequests
    );
};

export const getSuccessfulPullTenantCount = (state: IComponentsAppState): number => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.successfulPullTenantCount ||
        sharedComponentsInitialState.successfulPullTenantCount
    );
};

export const getDerivedTotalPullTenantCount = (state: IComponentsAppState): number => {
    const summaryTotal = getTotalPullTenantCount(state);
    const removedTotal = getSuccessfulPullTenantCount(state);
    const derivedCount = summaryTotal - removedTotal;
    if (derivedCount >= 0) {
        return derivedCount;
    } else {
        return summaryTotal;
    }
};

export const getSuccessfulPullTenantCountDict = (state: IComponentsAppState): IPullTenantSuccessfulCountDict => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.successfulPullTenantCountDict ||
        sharedComponentsInitialState.successfulPullTenantCountDict
    );
};

export const getPullTenantSummaryMemoized = createSelector(
    getPullTenantSummaryData,
    getSuccessfulPullTenantRequests,
    (pullTenantSummaryData, successfulPullTenantRequests) => {
        if (pullTenantSummaryData.length > 0 && successfulPullTenantRequests.length > 0) {
            return pullTenantSummaryData.filter((item: any) => !successfulPullTenantRequests.includes(item.laborId));
        } else {
            return pullTenantSummaryData;
        }
    }
);

export const groupedSummaryDataSelector = createSelector(
    [getSelectedSummary, getSummaryGroupedBy, getTenantInfo],
    getGroupedBySummaryMemo
);

export const getFilteredSummaryMemoized = createSelector(
    getFilterValue,
    groupedSummaryDataSelector,
    (filterValue, groupedData: any) => {
        return groupedData.filter((summaryItem: { displayValue: any }) => summaryItem.displayValue === filterValue);
    }
);

export const getDerivedValueFromSummaryCount = createSelector(
    getPullTenantSummaryCount,
    getSuccessfulPullTenantCountDict,
    getTenantId,
    (summaryCountObj, successfulCountObj, tenantId) => {
        const summaryObj = summaryCountObj?.find((item) => item.TenantId === tenantId);
        const summaryValue = summaryObj?.Count ?? 0;
        const removedCount = successfulCountObj?.[tenantId] ?? 0;
        const derivedCount = summaryValue - removedCount;
        if (typeof removedCount === 'number' && removedCount > 0 && derivedCount >= 0) {
            return derivedCount;
        } else {
            return summaryValue;
        }
    }
);

const getSummaryCount = (_: any, __: any, summaryCount: number): number => summaryCount;

export const getDerivedCountForPullTenant = createSelector(
    getSuccessfulPullTenantCountDict,
    getTenantId,
    getSummaryCount,
    (successfulCountObj, tenantId, summaryCount) => {
        const removedCount = successfulCountObj?.[tenantId] ?? 0;
        const derivedCount = summaryCount - removedCount;
        if (typeof removedCount === 'number' && removedCount > 0 && derivedCount >= 0) {
            return derivedCount;
        } else {
            return summaryCount;
        }
    }
);

export const getIsProfilePanelOpen = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isProfilePanelOpen ||
        sharedComponentsInitialState.isProfilePanelOpen
    );
};

export const getIsAccessibilityPanelOpen = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isAccessibilityPanelOpen ||
        sharedComponentsInitialState.isAccessibilityPanelOpen
    );
};

export const getSearchResults = (state: IComponentsAppState): any => {
    return state.dynamic?.[sharedComponentsReducerName]?.searchResults || sharedComponentsInitialState.searchResults;
};

export const getIsSearchResultsViewOpen = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isSearchResultsViewOpen ||
        sharedComponentsInitialState.isSearchResultsViewOpen
    );
};

export const getIsLoadingSearchResults = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isLoadingSearchResults ||
        sharedComponentsInitialState.isLoadingSearchResults
    );
};

export const getIsQuickTourOpen = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isQuickTourOpen || sharedComponentsInitialState.isQuickTourOpen
    );
};

export const getQuickTourData = (state: IComponentsAppState): IQuickTourListItem[] => {
    return state.dynamic?.[sharedComponentsReducerName]?.quickTourData || sharedComponentsInitialState.quickTourData;
};

export const getItemsUnread = (state: IComponentsAppState): IQuickTourListItem[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.unreadQuickTours || sharedComponentsInitialState.unreadQuickTours
    );
};

export const getHasUnreadLabsCoachmark = (state: IComponentsAppState): boolean => {
    const unread =
        state.dynamic?.[sharedComponentsReducerName]?.unreadQuickTours ||
        sharedComponentsInitialState.unreadQuickTours;
    return (unread || []).some((t: IQuickTourListItem) => String(t.id) === SETTINGS_COACHMARK_ID);
};

export const getItemsRead = (state: IComponentsAppState): IQuickTourListItem[] => {
    return state.dynamic?.[sharedComponentsReducerName]?.readQuickTours || sharedComponentsInitialState.readQuickTours;
};

export const getUpdatedQuickToursList = (state: IComponentsAppState): Array<string> => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.updatedQuickTourList ||
        sharedComponentsInitialState.updatedQuickTourList
    );
};

export const getMyFlightingData = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.myFlightingData || sharedComponentsInitialState.myFlightingData
    );
};

export const getAllFlightingData = (state: IComponentsAppState): any => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.allFlightingData || sharedComponentsInitialState.allFlightingData
    );
};

const getFeatureName = (_: any, featureName: string): string => featureName;

// Single source of truth for feature enablement, consumed by both the useFlighting
// hook and the sagas: enabled when the tenant status is "Enable All" (everyone), or
// "Flighting" and the user is in their personal list.
export const getIsFeatureEnabledForUser = createSelector(
    getAllFlightingData,
    getMyFlightingData,
    getFeatureName,
    (allFlightingData, myFlightingData, featureName) => {
        if (!Array.isArray(allFlightingData)) {
            return false;
        }
        const feature = allFlightingData.find((element: any) => element?.['featureName'] === featureName);
        const status = feature?.flightingStatus || 'Enable All';
        if (status === 'Enable All') {
            return true;
        }
        if (status === 'Flighting') {
            return (
                Array.isArray(myFlightingData) &&
                myFlightingData.some((element: any) => element?.['featureName'] === featureName)
            );
        }
        return false;
    }
);

// Whether the given feature carries a one-time TeachingCoach coachmark, per the
// flighting data. TeachingCoach is a per-feature boolean projected into
// allFlightingData (FlightingDataProvider) alongside FlightingStatus, so the
// flighting feed is the authoritative owner of the "does this feature want a
// coachmark" decision. This deliberately distinguishes coachmark features from
// slide-based quick-tour features (which have QuickTourSlidesJson but
// TeachingCoach=false). The one thing the flighting feed lacks — the per-user
// "already seen" state — comes from the quick-tour feed (getQuickTourData).
export const getIsTeachingCoachEnabledForFeature = createSelector(
    getAllFlightingData,
    getFeatureName,
    (allFlightingData, featureName): boolean => {
        if (!Array.isArray(allFlightingData)) {
            return false;
        }
        const feature = allFlightingData.find((element: any) => element?.['featureName'] === featureName);
        // Client payload is camelCase; tolerate PascalCase defensively.
        return feature?.teachingCoach === true || feature?.TeachingCoach === true;
    }
);

// Feedback selectors
export const getFeedbackByFeature = (state: IComponentsAppState, featureName: string): IFeedbackData | null => {
    return state.dynamic?.[sharedComponentsReducerName]?.feedback?.feedbackByFeature?.[featureName] || null;
};
export const getAllFeedback = (state: IComponentsAppState): { [featureName: string]: IFeedbackData } => {
    return state.dynamic?.[sharedComponentsReducerName]?.feedback?.feedbackByFeature || {};
};

export const getFeedbackByDocumentNumber = (
    state: IComponentsAppState,
    documentNumber: string
): IFeedbackData | undefined => {
    const allFeedback = getAllFeedback(state);
    return Object.values(allFeedback).find((feedback: IFeedbackData) => feedback.DocumentNumber === documentNumber);
};

export const getFeedbackInput = (state: IComponentsAppState, featureName: string, inputType: string): string => {
    const feedback = getFeedbackByFeature(state, featureName);
    const input = feedback?.Inputs.find((i) => i.InputType === inputType);
    return input?.InputValue || '';
};

export const getFeedbackState = (state: IComponentsAppState) => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.feedback || {
            feedbackByFeature: {},
            isLoading: false,
            hasError: false,
            errorMessage: null,
        }
    );
};
export const getIsSummaryCollapsed = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isSummaryCollapsed ??
        sharedComponentsInitialState.isSummaryCollapsed
    );
};

export const getPropertyFilters = (state: IComponentsAppState): Record<string, string[]> => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.propertyFilters ?? sharedComponentsInitialState.propertyFilters
    );
};

export const getIsSubmitterView = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.isSubmitterView ?? sharedComponentsInitialState.isSubmitterView;
};

export const getIsDownloadingSummary = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.isDownloadingSummary ??
        sharedComponentsInitialState.isDownloadingSummary
    );
};

export const getSummaryDownloadHasError = (state: IComponentsAppState): boolean => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.summaryDownloadHasError ??
        sharedComponentsInitialState.summaryDownloadHasError
    );
};

// Shared utility function for applying property filters to data
export function applyPropertyFilters(
    data: any[],
    propertyFilters: Record<string, string[]>,
    tableColumns?: any[]
): any[] {
    // If no filters are applied, return original data
    if (!propertyFilters || Object.keys(propertyFilters).length === 0) {
        return data;
    }

    // Filter the data based on property filters
    return data.filter((item: any) => {
        // Check each property filter with short-circuit evaluation
        for (const [key, values] of Object.entries(propertyFilters)) {
            if (values && values.length > 0) {
                // Get the value from the item (supports nested properties with dot notation)
                let itemValue = key.includes('.')
                    ? key.split('.').reduce((obj: any, prop: string) => obj?.[prop], item)
                    : item[key];

                // Handle boolean values
                if (typeof itemValue === 'boolean') {
                    itemValue = itemValue ? 'Yes' : 'No';
                }

                // Handle default values if table columns are provided
                if (tableColumns) {
                    const columnInfo = tableColumns.find((col: any) => col.field === key);
                    if (columnInfo?.defaultValue && !itemValue) {
                        itemValue = columnInfo.defaultValue;
                    }
                }

                // Check if the item's value is in the filter values
                if (!values.includes(itemValue)) {
                    return false;
                }
            }
        }

        return true;
    });
}

// Selector for property filtered data that works with both filterTable and DashboardFilterUtils
export const getPropertyFilteredData = createSelector(
    [
        (state: IComponentsAppState) => state,
        (_: IComponentsAppState, data: any[]) => data,
        (_: IComponentsAppState, __: any[], tableColumns?: any[]) => tableColumns,
    ],
    (state, data, tableColumns) => {
        const propertyFilters = getPropertyFilters(state);
        return applyPropertyFilters(data, propertyFilters, tableColumns);
    }
);

export const getSuggestTerms = (state: IComponentsAppState): string[] => {
    return state.dynamic?.[sharedComponentsReducerName]?.suggestTerms || sharedComponentsInitialState.suggestTerms;
};

export const getSuggestRequests = (state: IComponentsAppState): any[] => {
    return state.dynamic?.[sharedComponentsReducerName]?.suggestRequests || sharedComponentsInitialState.suggestRequests;
};

export const getIsLoadingSuggest = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.isLoadingSuggest || sharedComponentsInitialState.isLoadingSuggest;
};

type ISuggestCache = { query: string; terms: string[]; requests: ISuggestRequestItem[] };

export const getSuggestCache = (state: IComponentsAppState): ISuggestCache | null => {
    return state.dynamic?.[sharedComponentsReducerName]?.suggestCache || null;
};

// Minimum query length before local summary matching kicks in (mirrors the API suggest gate).
const SUGGEST_MIN_QUERY_LENGTH = 3;

const mapSummaryToSuggestRequest = (item: any): ISuggestRequestItem => ({
    tenantId: String(item?.TenantId ?? ''),
    documentNumber: String(item?.ApprovalIdentifier?.DocumentNumber ?? ''),
    displayDocumentNumber: String(item?.ApprovalIdentifier?.DisplayDocumentNumber ?? ''),
    title: item?.Title ?? '',
    appName: item?.AppName ?? '',
    unitValueText: item?.UnitValue != null ? String(item.UnitValue) : '',
    submitterName: item?.Submitter?.Name ?? '',
    businessProcessName: item?.BusinessProcessName ?? null,
});

// Build request suggestions from the locally loaded summary so a slow or failed
// search index still surfaces requests the user already has in view.
const getLocalSuggestRequests = (summary: any[], lowerQuery: string): ISuggestRequestItem[] => {
    if (!Array.isArray(summary) || lowerQuery.length < SUGGEST_MIN_QUERY_LENGTH) {
        return [];
    }
    const matches = (value: unknown): boolean => String(value ?? '').toLowerCase().includes(lowerQuery);
    return summary
        .filter(
            (item) =>
                matches(item?.ApprovalIdentifier?.DocumentNumber) ||
                matches(item?.ApprovalIdentifier?.DisplayDocumentNumber) ||
                matches(item?.Title) ||
                matches(item?.Submitter?.Name) ||
                matches(item?.AppName)
        )
        .map(mapSummaryToSuggestRequest);
};

// Union API/cache requests with local summary matches. API results win on
// duplicate documentNumber (they carry ranking/metadata); local-only matches
// are appended after them.
const mergeSuggestRequests = (
    apiRequests: ISuggestRequestItem[],
    localRequests: ISuggestRequestItem[]
): ISuggestRequestItem[] => {
    if (localRequests.length === 0) {
        return apiRequests;
    }
    const seen = new Set(apiRequests.map((r) => r.documentNumber));
    const localOnly = localRequests.filter((r) => r.documentNumber && !seen.has(r.documentNumber));
    return localOnly.length === 0 ? apiRequests : [...apiRequests, ...localOnly];
};

export const getFilteredSuggest = (
    state: IComponentsAppState,
    query: string
): { terms: string[]; requests: ISuggestRequestItem[] } => {
    const cache = getSuggestCache(state);
    const lowerQuery = query.toLowerCase();
    // Hold local matches until the API settles so the list doesn't flash local
    // results and then jump when the response lands. On failure/empty the flag
    // is cleared too, so local still surfaces as the resilience fallback.
    const localRequests = getIsLoadingSuggest(state)
        ? []
        : getLocalSuggestRequests(getSummary(state), lowerQuery);

    if (cache && query.length >= 3 && lowerQuery.startsWith(cache.query.toLowerCase())) {
        // Exact match — fresh results from API, show everything unfiltered
        if (lowerQuery === cache.query.toLowerCase()) {
            return { terms: cache.terms, requests: mergeSuggestRequests(cache.requests, localRequests) };
        }

        // Query extends cached query — filter client-side for instant results.
        const filteredRequests = cache.requests.filter(
            (r) =>
                r.documentNumber?.toLowerCase().includes(lowerQuery) ||
                r.displayDocumentNumber?.toLowerCase().includes(lowerQuery) ||
                r.title?.toLowerCase().includes(lowerQuery) ||
                r.submitterName?.toLowerCase().includes(lowerQuery) ||
                r.appName?.toLowerCase().includes(lowerQuery)
        );
        return {
            terms: cache.terms.filter((t) => t.toLowerCase().includes(lowerQuery)),
            requests: mergeSuggestRequests(filteredRequests, localRequests),
        };
    }

    // No cache hit — surface local summary matches while the API responds (or if it fails)
    return {
        terms: getSuggestTerms(state),
        requests: mergeSuggestRequests(getSuggestRequests(state), localRequests),
    };
};
