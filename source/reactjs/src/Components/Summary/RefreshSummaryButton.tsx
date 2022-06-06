import * as React from 'react';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IconButton } from '@fluentui/react';
import { getUserAlias } from '../Shared/SharedComponents.persistent-selectors';
import {
    refreshBulkState,
    requestMySummary,
    requestPullTenantSummary,
    requestPullTenantSummaryCount,
    updateFailedPullTenantRequests,
    updatePanelState
} from '../Shared/SharedComponents.actions';
import {
    getFilterValue,
    getIsLoadingPullTenantData,
    getIsLoadingSummary,
    getIsProcessingBulkApproval,
    getIsPullTenantSelected,
    getPullTenantSearchCriteria,
    getPullTenantSearchSelection,
    getTenantIdFromAppName
} from '../Shared/SharedComponents.selectors';

export function RefreshSummaryButton(): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const userAlias = useSelector(getUserAlias);
    const filterValue = useSelector(getFilterValue);
    const isPullTenantSelected = useSelector(getIsPullTenantSelected);
    const searchCriteria: any = useSelector(getPullTenantSearchCriteria);
    const searchSelection = useSelector(getPullTenantSearchSelection);
    const tenantIdforFilterValue = useSelector((state: any) => getTenantIdFromAppName(state, filterValue));
    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const isLoadingPullTenantData = useSelector(getIsLoadingPullTenantData);
    const isProcessingBulkApproval = useSelector(getIsProcessingBulkApproval);

    const refreshSummary = () => {
        if (isPullTenantSelected) {
            let filterCriteria = null;
            if (searchCriteria && searchCriteria.length > 0) {
                filterCriteria = searchCriteria[searchSelection]?.value ?? null;
            }
            dispatch(requestPullTenantSummary(tenantIdforFilterValue, userAlias, filterCriteria));
            dispatch(updateFailedPullTenantRequests([]));
        } else {
            dispatch(requestMySummary(userAlias));
            dispatch(requestPullTenantSummaryCount(userAlias));
        }
        dispatch(refreshBulkState());
        dispatch(updatePanelState(false));
    };

    return (
        <IconButton
            iconProps={{ iconName: 'Refresh' }}
            title="Refresh Summary"
            ariaLabel="Click here to refresh the summary"
            onClick={refreshSummary}
            disabled={isLoadingSummary || isLoadingPullTenantData || isProcessingBulkApproval}
        />
    );
}
