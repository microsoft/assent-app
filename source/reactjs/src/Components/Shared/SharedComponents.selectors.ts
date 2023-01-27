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
    ITenantDelegationObj
} from './SharedComponents.types';
import { groupByTenant, groupByDate, groupBySubmitter, IGrouping } from '../../Helpers/groupPendingApprovals';
import { GroupingBy } from './Components/GroupingBy';
import { createSelector } from 'reselect';
import { IDropdownOption } from '@fluentui/react';

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
                LoggedInUserAlias: profile.userPrincipalName
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
            TenantId: tenantId
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

export const getGroupedBySummaryMemo = (summary: any, groupedBy: string) => {
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
        const matchingElement = submitterImages.find(el => el.alias === alias);
        return matchingElement?.image || null;
    }
    return null;
});

export const isAliasInSubmitters = createSelector(getSubmitterImages, getAlias, (submitterImages, alias) => {
    if (submitterImages.length > 0 && alias) {
        const matchingElement = submitterImages.find(el => el.alias === alias);
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
    tenantInfo?.map(function(item: any) {
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
    const filterMenuProps = filterMenuItems.map(x => ({
        ['key']: x,
        ['text']: x
    }));
    return filterMenuProps;
};

export const getHasError = (state: IComponentsAppState): boolean => {
    return state.dynamic?.[sharedComponentsReducerName]?.hasError || sharedComponentsInitialState.hasError;
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

export const getUserDelegations = (state: IComponentsAppState): object[] => {
    return (
        state.dynamic?.[sharedComponentsReducerName]?.userDelegations || sharedComponentsInitialState.userDelegations
    );
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

export const getCardViewSelected = (state: IComponentsAppState): boolean => {
    if (state.dynamic) {
        return state.dynamic?.[sharedComponentsReducerName]?.isCardViewSelected;
    } else {
        return sharedComponentsInitialState.isCardViewSelected;
    }
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
    [getSelectedSummary, getSummaryGroupedBy],
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
        const summaryObj = summaryCountObj?.find(item => item.TenantId === tenantId);
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
    )
};
