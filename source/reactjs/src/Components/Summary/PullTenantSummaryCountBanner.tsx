import { MessageBar, Stack, IStackTokens } from '@fluentui/react';
import * as React from 'react';
import { IPullTenantSummaryCountObject } from '../Shared/SharedComponents.types';
import { PullTenantSummaryBannerHeader, PullTenantSummaryCountDiv } from '../Shared/SharedLayout';
import { PullTenantSummaryCountCard } from './PullTenantSummaryCountCard';
import { CardContainer } from './SummaryCards/SummaryCard.styled';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getUserAlias } from '../Shared/SharedComponents.persistent-selectors';
import * as SummaryStyled from './SummaryStyling';
import { getTenantIcon } from '../Shared/Components/IconMapping';
import { Text } from '@fluentui/react/lib/Text';
import { getTenantInfo } from '../Shared/SharedComponents.selectors';

const PullTenantSummaryCountBanner = (props: {
    pullTenantSummaryCount: IPullTenantSummaryCountObject[];
    isTableView: boolean;
    filterValue?: string;
}): React.ReactElement => {
    const { useSelector } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const userAlias = useSelector(getUserAlias);

    const { pullTenantSummaryCount, isTableView, filterValue } = props;

    const countValues =
        filterValue && filterValue !== 'All'
            ? pullTenantSummaryCount?.filter(item => item.AppName === filterValue)
            : pullTenantSummaryCount;

    return (
        <CardContainer isTableView={isTableView}>
            {countValues &&
                countValues.map((item, index) => (
                    <PullTenantSummaryCountCard
                        AppName={item.AppName}
                        Count={item.Count}
                        TenantId={item.TenantId}
                        userAlias={userAlias}
                        isTableView={isTableView}
                        key={index}
                    />
                ))}
        </CardContainer>
    );
};

export default PullTenantSummaryCountBanner;
