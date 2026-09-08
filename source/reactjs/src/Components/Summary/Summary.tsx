import * as React from 'react';
import * as SummaryStyled from './SummaryStyling';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../Shared/SharedComponents.reducer';
import { sharedComponentsSagas } from '../Shared/SharedComponents.sagas';
import { Reducer } from 'redux';
import {
    requestMySummary,
    requestTenantInfo,
    updateSelectedPage,
    RequestUserPreferences,
    updatePanelState,
    setSelectedSumaryTileRef,
    requestPullTenantSummaryCount,
    requestFlightingData,
} from '../Shared/SharedComponents.actions';
import {
    sharedComponentsPersistentReducerName,
    sharedComponentsPersistentReducer,
} from '../Shared/SharedComponents.persistent-reducer';
import { usePersistentReducer } from '../Shared/Components/PersistentReducer';
import { DetailsDockedView } from '../Shared/Details/DetailsDockedView';
import { DetailsPanel } from '../Shared/Details/DetailsPanel';
import { FLYOUT_VIEW } from '../Shared/SharedConstants';
import { SummaryView } from './SummaryView';
import {
    getBulkApproveStatus,
    getBulkApproveFailed,
    getDetailsDefaultView,
    getIsPanelOpen,
    getIsSearchResultsViewOpen,
    getTenantInfo,
    getToggleDetailsScreen,
} from '../Shared/SharedComponents.selectors';
import {
    getBulkMessagebarHeight,
    getAliasMessagebarHeight,
    getDisplayDocumentNumber,
} from '../Shared/Details/Details.selectors';
import { getUserAlias } from '../Shared/SharedComponents.persistent-selectors';
import { updateMyRequest } from '../Shared/Details/Details.actions';
import { RefreshSummaryButton } from './RefreshSummaryButton';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { useHistory, useLocation } from 'react-router-dom';

interface ISummaryProps {
    windowHeight: number;
    windowWidth: number;
    queryDocNumber: string;
    queryTenantId?: string;
    isDashboardPageView?: boolean;
}

function Summary(props: ISummaryProps): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);
    const { windowHeight, windowWidth, isDashboardPageView } = props;
    const reduxContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { useSelector, dispatch } = reduxContext;
    const isPanelOpen = useSelector(getIsPanelOpen);
    const tenantInfo = useSelector(getTenantInfo);
    const toggleDetailsScreen = useSelector(getToggleDetailsScreen);
    const detailsDefaultView = useSelector(getDetailsDefaultView);
    const userAlias = useSelector(getUserAlias);
    const bulkMessagebarHeight = useSelector(getBulkMessagebarHeight);
    const aliasMessagebarHeight = useSelector(getAliasMessagebarHeight);
    const initialMount = React.useRef(true);
    const isSearchResultsViewOpen = useSelector(getIsSearchResultsViewOpen);
    const bulkApproveStatus = useSelector(getBulkApproveStatus);
    const bulkApproveFailed = useSelector(getBulkApproveFailed);
    // Hide the floating refresh button while a bulk action banner is shown so it does not overlap the message bar (esp. mobile).
    const isBulkActionBannerShown = bulkApproveStatus || bulkApproveFailed;
    const currentDisplayDocumentNumber = useSelector(getDisplayDocumentNumber);
    const history = useHistory();
    const location = useLocation();

    const numberInRoute = props.queryDocNumber;
    const tenantIdInRoute = props.queryTenantId;

    React.useEffect(() => {
        dispatch(updateSelectedPage('summary'));
        dispatch(requestTenantInfo());
        dispatch(RequestUserPreferences());
        return () => {
            initialMount.current = true;
        };
    }, []);

    React.useEffect(() => {
        setTimeout(() => {}, 500); //allow user image to load before summary
        dispatch(requestMySummary(userAlias));
        dispatch(requestPullTenantSummaryCount(userAlias));
        dispatch(updatePanelState(false));
    }, [userAlias]);

    React.useEffect(() => {
        if (
            numberInRoute &&
            numberInRoute !== '' &&
            tenantInfo &&
            numberInRoute !== currentDisplayDocumentNumber
        ) {
            dispatch(
                updateMyRequest(
                    parseInt(tenantIdInRoute),
                    '',
                    numberInRoute
                )
            );
            dispatch(setSelectedSumaryTileRef(numberInRoute));
            dispatch(updatePanelState(true));
        }
    }, [numberInRoute, tenantIdInRoute, tenantInfo, isPanelOpen, currentDisplayDocumentNumber, dispatch]);

    return (
        <SummaryStyled.SummaryContainer
            isPanelOpen={isPanelOpen}
            bulkMessagebarHeight={bulkMessagebarHeight}
            aliasMessagebarHeight={aliasMessagebarHeight}
            windowHeight={windowHeight}
            windowWidth={windowWidth}
            selectedPage={'summary'}
            isDashboardView={isDashboardPageView}
        >
            <div className="ms-Grid" dir="ltr">
                <div className="ms-Grid-row">
                    <div
                        className={
                            'ms-Grid-col' +
                            ' ms-Grid-col-mobile' +
                            (isPanelOpen && detailsDefaultView !== FLYOUT_VIEW
                                ? toggleDetailsScreen
                                    ? ' ms-sm4 ms-xl4 ms-xxl4 ms-xxxl4 ms-hiddenMdDown horizontal-separator'
                                    : ' ms-sm6 ms-hiddenMdDown horizontal-separator'
                                : ' ms-sm12 ')
                        }
                    >
                        <SummaryStyled.RefreshMedia
                            isDetailsExpanded={isPanelOpen && detailsDefaultView !== FLYOUT_VIEW && toggleDetailsScreen}
                            isDashboardView={isDashboardPageView}
                            style={
                                isPanelOpen && detailsDefaultView !== FLYOUT_VIEW
                                    ? { position: 'absolute' }
                                    : { display: 'none', position: 'absolute' }
                            }
                        >
                            {!isSearchResultsViewOpen && !isBulkActionBannerShown && <RefreshSummaryButton />}
                        </SummaryStyled.RefreshMedia>
                        {/*Mobile View*/}
                        <SummaryStyled.RefreshMediaDuplicate
                            isDashboardView={isDashboardPageView}
                            style={
                                isPanelOpen && detailsDefaultView !== FLYOUT_VIEW
                                    ? { display: 'none', position: 'relative' }
                                    : { visibility: 'visible', position: 'absolute' }
                            }
                        >
                            {!isSearchResultsViewOpen && !isBulkActionBannerShown && <RefreshSummaryButton />}
                        </SummaryStyled.RefreshMediaDuplicate>
                        <SummaryStyled.SummaryViewWrapper isDashboardView={isDashboardPageView}>
                            <SummaryView
                                windowHeight={windowHeight}
                                windowWidth={windowWidth}
                                isDashboardPageView={isDashboardPageView}
                            ></SummaryView>
                        </SummaryStyled.SummaryViewWrapper>
                    </div>
                    {isPanelOpen && detailsDefaultView !== FLYOUT_VIEW && tenantInfo && (
                        <DetailsDockedView
                            templateType={'All'}
                            windowHeight={windowHeight}
                            windowWidth={windowWidth}
                            historyRef={history}
                            locationRef={location}
                        ></DetailsDockedView>
                    )}
                    {detailsDefaultView === FLYOUT_VIEW && (
                        <DetailsPanel
                            templateType={'All'}
                            windowHeight={windowHeight}
                            windowWidth={windowWidth}
                            historyRef={history}
                            locationRef={location}
                        />
                    )}
                </div>
            </div>
        </SummaryStyled.SummaryContainer>
    );
}

const connected = React.memo(Summary);
export { connected as Summary };
