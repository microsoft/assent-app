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
    getDetailsDefaultView,
    getIsPanelOpen,
    getSummary,
    getTenantInfo,
    getToggleDetailsScreen,
} from '../Shared/SharedComponents.selectors';
import { getBulkMessagebarHeight, getAliasMessagebarHeight } from '../Shared/Details/Details.selectors';
import { getUserAlias } from '../Shared/SharedComponents.persistent-selectors';
import { updateMyRequest } from '../Shared/Details/Details.actions';
import { ISummaryObject } from '../Shared/SharedComponents.types';
import { RefreshSummaryButton } from './RefreshSummaryButton';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { useHistory, useLocation } from 'react-router-dom';

interface ISummaryProps {
    windowHeight: number;
    windowWidth: number;
    queryDocNumber: string;
    queryTenantId?: string;
}

function Summary(props: ISummaryProps): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);
    const { windowHeight, windowWidth } = props;
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
    const summary = useSelector(getSummary);
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
        if (numberInRoute && numberInRoute !== '' && tenantInfo && summary && !isPanelOpen) {
            const requestFromSummary: ISummaryObject = summary.find(
                (request: ISummaryObject) => request.ApprovalIdentifier.DisplayDocumentNumber === numberInRoute
            );
            if (requestFromSummary) {
                dispatch(
                    updateMyRequest(
                        parseInt(tenantIdInRoute),
                        requestFromSummary.ApprovalIdentifier.DocumentNumber,
                        numberInRoute,
                        requestFromSummary.ApprovalIdentifier.FiscalYear
                    )
                );
                dispatch(setSelectedSumaryTileRef(numberInRoute));
                dispatch(updatePanelState(true));
            }
        }
    }, [numberInRoute, tenantIdInRoute, tenantInfo, isPanelOpen, summary, dispatch]);

    return (
        <SummaryStyled.SummaryContainer
            isPanelOpen={isPanelOpen}
            bulkMessagebarHeight={bulkMessagebarHeight}
            aliasMessagebarHeight={aliasMessagebarHeight}
            windowHeight={windowHeight}
            windowWidth={windowWidth}
            selectedPage={'summary'}
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
                            style={
                                isPanelOpen && detailsDefaultView !== FLYOUT_VIEW
                                    ? { position: 'relative' }
                                    : { display: 'none', position: 'absolute' }
                            }
                        >
                            <RefreshSummaryButton />
                        </SummaryStyled.RefreshMedia>
                        {/*Mobile View*/}
                        <SummaryStyled.RefreshMediaDuplicate
                            style={
                                isPanelOpen && detailsDefaultView !== FLYOUT_VIEW
                                    ? { display: 'none', position: 'relative' }
                                    : { visibility: 'visible', position: 'absolute' }
                            }
                        >
                            <RefreshSummaryButton />
                        </SummaryStyled.RefreshMediaDuplicate>
                        <SummaryView windowHeight={windowHeight} windowWidth={windowWidth}></SummaryView>
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
