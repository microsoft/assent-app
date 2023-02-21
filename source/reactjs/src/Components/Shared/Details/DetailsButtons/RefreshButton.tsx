import * as React from 'react';
import { IconButton } from '@fluentui/react';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';

import * as Styled from './DetailsButtonsStyled';
import { requestHeader, requestMyDetails } from '../../Details/Details.actions';
import { detailsReducerName, detailsInitialState } from '../../Details/Details.reducer';
import { DetailsType, IDetailsAppState } from '../../Details/Details.types';
import { IComponentsAppState } from '../../SharedComponents.types';
import { SharedComponentsPersistentInitialState } from '../../SharedComponents.persistent-reducer';
import { TooltipHost } from '@fluentui/react/lib/Tooltip';
import { getSelectedPage, getTenantInfo } from '../../SharedComponents.selectors';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

function RefreshButton(): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const selectedPage = useSelector(getSelectedPage);
    const { userAlias } = useSelector(
        (state: IComponentsAppState) =>
            state.SharedComponentsPersistentReducer || SharedComponentsPersistentInitialState
    );
    const { tenantId, documentNumber, displayDocumentNumber, summaryJSON } = useSelector(
        (state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState
    );
    const tenantInfo = useSelector(getTenantInfo);
    let currentTenant;
    if (tenantInfo) {
        currentTenant = tenantInfo.find((tenant: { tenantId: any }) => {
            return Number(tenant.tenantId) === Number(tenantId);
        });
    }
    const detailsType = currentTenant && currentTenant?.detailsComponentInfo?.detailsComponentType;
    const summaryDataMapping = currentTenant?.summaryDataMapping;
    const isPullModelEnabled = currentTenant?.isPullModelEnabled;
    const requiresTemplate = detailsType === DetailsType.AdaptiveCard;
    return (
        <TooltipHost
            content="Refresh Details"
            calloutProps={Styled.tooltipCalloutProps}
            styles={Styled.tooltipHostContainer}
        >
            <IconButton
                iconProps={{ iconName: 'Refresh' }}
                title="Refresh"
                ariaLabel="Click here to refresh the details screen"
                style={Styled.DetailActionIcon}
                onClick={(): void => {
                    dispatch(
                        requestHeader(
                            tenantId,
                            displayDocumentNumber,
                            selectedPage === 'history' ? '' : userAlias,
                            isPullModelEnabled,
                            summaryJSON,
                            summaryDataMapping
                        )
                    );
                    dispatch(
                        requestMyDetails(
                            tenantId,
                            documentNumber,
                            displayDocumentNumber,
                            selectedPage === 'history' ? '' : userAlias,
                            requiresTemplate,
                            isPullModelEnabled
                        )
                    );
                }}
            />
        </TooltipHost>
    );
}

const connected = withContext(RefreshButton);
export { connected as RefreshButton };
