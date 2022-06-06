import { Panel, PanelType } from '@fluentui/react/lib/Panel';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Stack } from '@fluentui/react';
import * as React from 'react';
import { Reducer } from 'redux';
import { requestFullyScrolled, toggleHistoryDetailPanelAction } from './Details.actions';
import { detailsReducerName, detailsInitialState } from './Details.reducer';
import { IDetailsAppState } from './Details.types';
import { RequestView } from './DetailsAdaptive';
import { updatePanelState } from '../SharedComponents.actions';
import {
    sharedComponentsReducerName,
    sharedComponentsInitialState,
    sharedComponentsReducer,
} from '../SharedComponents.reducer';
import { sharedComponentsSagas } from '../SharedComponents.sagas';
import { IComponentsAppState } from '../SharedComponents.types';
import { MaximizeButton } from './DetailsButtons/MaximizeButton';
import { BackButton } from './DetailsButtons/BackButton';
import { CloseButton } from './DetailsButtons/CloseButton';
import * as Styled from './DetailsStyling';
import { RefreshButton } from './DetailsButtons/RefreshButton';

function DetailsPanel(props: {
    templateType?: string;
    windowWidth: number;
    windowHeight: number;
    historyRef: any;
    locationRef: any;
}): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);

    const componentContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const reduxContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { useSelector, dispatch, telemetryClient } = reduxContext;

    const { isPanelOpen, tenantInfo } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );
    const {
        isPreviewOpen,
        isMicrofrontendOpen,
        toggleHistoryDetailPanel,
        isRequestFullyScrolled,
        isRequestFullyRendered,
    } = useSelector((state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState);

    function calculateWidth() {
        const widthXL8 = (window.innerWidth * 66.66) / 100;
        const widthXL5 = (window.innerWidth * 41.66) / 100;

        return {
            widthXL8,
            widthXL5,
        };
    }

    const _pWidth =
        window.innerWidth >= 768
            ? toggleHistoryDetailPanel
                ? calculateWidth().widthXL8
                : calculateWidth().widthXL5
            : window.innerWidth;

    const [detailPanelWidth, setDetailPanelWidth] = React.useState(_pWidth);

    function handleResize() {
        const _pWidth =
            window.innerWidth >= 768
                ? toggleHistoryDetailPanel
                    ? calculateWidth().widthXL8
                    : calculateWidth().widthXL5
                : window.innerWidth;

        setDetailPanelWidth(_pWidth);
    }

    React.useEffect(() => {
        handleResize();
        window.addEventListener('resize', handleResize);

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    React.useEffect(() => {
        handleResize();
    }, [toggleHistoryDetailPanel]);

    function adjsutPanelWidth() {
        dispatch(toggleHistoryDetailPanelAction());
    }

    function handleScroll(e: any): void {
        const threshold = 5;
        const bottom = Math.round(e.target.scrollHeight - e.target.scrollTop) - e.target.clientHeight <= threshold;
        if (!isPreviewOpen && !isMicrofrontendOpen && !isRequestFullyScrolled && isRequestFullyRendered && bottom) {
            dispatch(requestFullyScrolled(true, false)); // Added this code to fix the re-render issue
        }
    }

    return (
        <Stack>
            {tenantInfo && (
                <Panel
                    isOpen={isPanelOpen}
                    onDismiss={() => dispatch(updatePanelState(false))}
                    customWidth={`${detailPanelWidth}px`}
                    type={PanelType.custom}
                    className={props.templateType == 'All' ? 'detail-panel-with-sticky-footer' : ''}
                    isLightDismiss={true}
                    onScroll={handleScroll}
                    overlayProps={{
                        isDarkThemed: true,
                    }}
                    focusTrapZoneProps={{
                        firstFocusableSelector: 'close-button',
                    }}
                    styles={{ content: { paddingTop: '36px' } }}
                    onRenderNavigation={() => {
                        return (
                            <Stack.Item styles={Styled.DetailsDocPreviewHeaderBarStyles('FLY')}>
                                {(isPreviewOpen || isMicrofrontendOpen) && <BackButton />}
                                {!(isPreviewOpen || isMicrofrontendOpen) && <div></div>}
                                <Stack style={{ flexFlow: 'row' }}>
                                    <Stack.Item>
                                        <MaximizeButton
                                            callbackOnMaximizeToggle={() => {
                                                adjsutPanelWidth();
                                            }}
                                        />
                                    </Stack.Item>
                                    <Stack.Item>
                                        <RefreshButton />
                                    </Stack.Item>
                                    <Stack.Item>
                                        <CloseButton />
                                    </Stack.Item>
                                </Stack>
                            </Stack.Item>
                        );
                    }}
                >
                    <RequestView
                        componentContext={componentContext}
                        hideHeaderActionBar={true}
                        reduxContext={reduxContext}
                        viewType="FLY"
                        templateType={props.templateType}
                        handleContainerScrolling={handleScroll}
                        windowWidth={props.windowWidth}
                        windowHeight={props.windowHeight}
                        historyRef={props.historyRef}
                        locationRef={props.locationRef}
                    ></RequestView>
                </Panel>
            )}
        </Stack>
    );
}

const connected = withContext(DetailsPanel);
export { connected as DetailsPanel };
