import * as React from 'react';
import {
    CountValue,
    PullTenantSummaryCountCardDiv,
    PullTenantSummaryCountCardHeader,
    FooterRow,
    PullTenantCardLabel
} from '../Shared/SharedLayout';
import { CardBody, StrongHeaderTitle, HeaderTenantIcon, CompanyCode } from './SummaryCards/SummaryCard.styled';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getTenantIcon } from '../Shared/Components/IconMapping';

import {
    requestExternalTenantInto,
    requestMyDelegations,
    requestPullTenantSummary,
    updateBulkSelected,
    updateBulkUploadConcurrentValue,
    updateCardViewType,
    updateFilterValue,
    updateGroupedSummary
} from '../Shared/SharedComponents.actions';
import { getDerivedCountForPullTenant, getTenantInfo } from '../Shared/SharedComponents.selectors';
import { GroupingBy } from '../Shared/Components/GroupingBy';
import { imitateClickOnKeyPressForDiv } from '../../Helpers/sharedHelpers';

export function PullTenantSummaryCountCard(props: {
    AppName: string;
    Count: number;
    TenantId: number;
    userAlias: string;
    isTableView: boolean;
    key?: number;
}): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { AppName, Count, TenantId, userAlias, isTableView } = props;

    const tenantInfo = useSelector(getTenantInfo);
    const derivedCount = useSelector((state: any) => getDerivedCountForPullTenant(state, TenantId, Count));

    const handleClick = (): void => {
        dispatch(updateGroupedSummary(GroupingBy.Tenant));
        dispatch(updateBulkSelected(true));
        dispatch(updateCardViewType(false));
        dispatch(updateFilterValue(AppName));
        dispatch(requestPullTenantSummary(TenantId, userAlias));
        dispatch(requestMyDelegations(null, TenantId, AppName));
        const currentTenant = tenantInfo?.find((tenant: { tenantId: number }) => {
            return tenant.tenantId === TenantId;
        });
        if (currentTenant) {
            const isExternalTenantActionDetails = currentTenant.isExternalTenantActionDetails;
            const bulkActionConcurrentCall = currentTenant.bulkActionConcurrentCall;
            if (isExternalTenantActionDetails) {
                dispatch(requestExternalTenantInto(TenantId, userAlias));
            }
            dispatch(updateBulkUploadConcurrentValue(bulkActionConcurrentCall));
        }
    };

    const cardLabel = 'View requests for ' + AppName;

    return (
        <PullTenantSummaryCountCardDiv
            onClick={handleClick}
            isTableView={isTableView}
            title={cardLabel}
            onKeyPress={imitateClickOnKeyPressForDiv(() => handleClick())}
            role="button"
            tabIndex={0}
        >
            <PullTenantSummaryCountCardHeader>
                <HeaderTenantIcon>{getTenantIcon(AppName, tenantInfo, '24px')}</HeaderTenantIcon>
                <StrongHeaderTitle>{AppName}</StrongHeaderTitle>
            </PullTenantSummaryCountCardHeader>
            <CardBody>
                <CountValue>{derivedCount}</CountValue>
                <PullTenantCardLabel>{'Pending requests'}</PullTenantCardLabel>
                <FooterRow aria-label={`click here to ${cardLabel} in table view`}>{cardLabel}</FooterRow>
            </CardBody>
        </PullTenantSummaryCountCardDiv>
    );
}
