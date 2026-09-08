import * as React from 'react';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import {
    sharedComponentsReducerName,
    sharedComponentsReducer,
    sharedComponentsInitialState,
} from '../Shared/SharedComponents.reducer';
import {
    sharedComponentsPersistentReducerName,
    sharedComponentsPersistentReducer,
    SharedComponentsPersistentInitialState,
} from '../Shared/SharedComponents.persistent-reducer';
import { usePersistentReducer } from '../Shared/Components/PersistentReducer';
import { sharedComponentsSagas } from '../Shared/SharedComponents.sagas';
import { IComponentsAppState } from '../Shared/SharedComponents.types';
import { Reducer } from 'redux';
import { setBulkFooterHeight } from '../Shared/Details/Details.actions';
import { updateSelectedSummarytoPending } from '../Shared/SharedComponents.actions';
import { usePageTracking } from '@micro-frontend-react/employee-experience/lib/usePageTracking';
import { usePageTitle } from '@micro-frontend-react/employee-experience/lib/usePageTitle';
import { Summary } from '../Summary/Summary';
import DetailsFooter from '../Shared/Details/DetailsFooter';
import { getFeature, getPageLoadFeature } from '@micro-frontend-react/employee-experience/lib/UsageTelemetryHelper';
import { isMobile } from 'react-device-detect';
import { updateBulkSelected, updateFilterValue } from '../Shared/SharedComponents.actions';
import {
    getCardViewSelected,
    getIsProcessingBulkApproval,
    getQuickTourData,
    getIsSubmitterView,
} from '../Shared/SharedComponents.selectors';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { QuickTour } from '../Shared/Components/QuickTour/QuickTour';

interface IPendingApprovalProps {
    match?: {
        params: {
            tenantId: string;
            documentNumber: string;
        };
    };
    isDashboardView?: boolean;
}

function PendingApprovalsPage(props: IPendingApprovalProps): React.ReactElement {
    const { isDashboardView } = props;
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const isSubmitterView = useSelector(getIsSubmitterView);
    usePageTitle(`${isDashboardView ? 'Dashboard' : (isSubmitterView ? 'Sent for Approval' : 'Pending Approvals')} - ${__APP_NAME__}`);
    const feature = getFeature('MSApprovalsWeb', isDashboardView ? 'DashboardPage' : 'PendingApprovalsPage');
    usePageTracking(getPageLoadFeature(feature));
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);
    const { isBulkSelected, isPanelOpen, selectedApprovalRecords } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );
    const { userAlias } = useSelector(
        (state: IComponentsAppState) =>
            state.SharedComponentsPersistentReducer || SharedComponentsPersistentInitialState
    );
    const isProcessingBulkApproval = useSelector(getIsProcessingBulkApproval);
    const isCardViewSelected = useSelector(getCardViewSelected);
    const bulkFooterHeight = 0;
    const quickTourData = useSelector(getQuickTourData);
    // Coachmark-only entries (slides: null) live in quickTourData for their own UI gating;
    // only auto-open the QuickTour dialog when there's an unread slide-bearing tour.
    const hasSlideTour = Array.isArray(quickTourData)
        && quickTourData.some((t: any) => Array.isArray(t?.slides) && t.slides.length > 0);

    React.useEffect(() => {
        dispatch(updateSelectedSummarytoPending());
        dispatch(updateBulkSelected(false));
        dispatch(updateFilterValue('All'));
    }, [dispatch]);

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

    React.useEffect(() => {
        function handleResize(): void {
            setDimensions({
                height: window.innerHeight,
                width: window.innerWidth,
            });
        }
        window.addEventListener('resize', handleResize);

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    dispatch(setBulkFooterHeight(0));
    return (
        <>
            {hasSlideTour && <QuickTour hidden={false} unreadOnly={true} />}
            <Summary
                windowHeight={dimensions.height}
                windowWidth={dimensions.width}
                queryTenantId={props.match?.params?.tenantId || ''}
                queryDocNumber={props.match?.params?.documentNumber || ''}
                isDashboardPageView={isDashboardView}
            />

            {isBulkSelected &&
                selectedApprovalRecords.length > 0 &&
                !((isMobile || window.innerWidth < 653) && isPanelOpen) && (
                    <DetailsFooter
                        userAlias={userAlias}
                        windowHeight={dimensions.height}
                        windowWidth={dimensions.width}
                        isBulkApproval={true}
                        setBulkFooterRef={(element: any) => {
                            const bulkFooterReferRef = element;
                            if (bulkFooterReferRef && bulkFooterReferRef.clientHeight) {
                                if (bulkFooterHeight != bulkFooterReferRef.clientHeight) {
                                    bulkFooterHeight = bulkFooterReferRef.clientHeight;
                                    dispatch(setBulkFooterHeight(bulkFooterReferRef.clientHeight));
                                }
                            }
                        }}
                    />
                )}
        </>
    );
}

const connected = withContext(PendingApprovalsPage);
export { connected as PendingApprovalsPage };
