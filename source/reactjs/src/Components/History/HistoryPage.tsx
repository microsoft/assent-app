import * as React from 'react';
import { usePageTracking } from '@micro-frontend-react/employee-experience/lib/usePageTracking';
import { usePageTitle } from '@micro-frontend-react/employee-experience/lib/usePageTitle';
import { Stack } from '@fluentui/react/lib/Stack';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { HistoryTable } from './HistoryTable';
import { updatePanelState, updateSelectedPage } from '../Shared/SharedComponents.actions';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getFeature, getPageLoadFeature } from '@micro-frontend-react/employee-experience/lib/UsageTelemetryHelper';

function HistoryPage(): React.ReactElement {
    usePageTitle(`History Table View - ${__APP_NAME__}`);
    const feature = getFeature('MSApprovalsWeb', 'HistoryPage')
    usePageTracking(getPageLoadFeature(feature));
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    React.useEffect(() => {
        dispatch(updateSelectedPage("history"));
        dispatch(updatePanelState(false));
    }, [])

    return (
        <HistoryTable />
    );
}

const connected = withContext(HistoryPage);
export { connected as HistoryPage };