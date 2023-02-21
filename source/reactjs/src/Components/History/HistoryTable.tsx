import * as React from 'react';
import * as Styled from './HistoryStyling';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';

import { Stack } from '@fluentui/react/lib/Stack';
import { Spinner } from '@fluentui/react/lib/Spinner';
import { ScrollablePane, SearchBox, Dropdown, IDropdownOption, IconButton } from '@fluentui/react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Reducer } from 'redux';
import {
    sharedComponentsReducerName,
    sharedComponentsReducer,
    sharedComponentsInitialState,
} from '../Shared/SharedComponents.reducer';
import { sharedComponentsSagas } from '../Shared/SharedComponents.sagas';
import {
    requestMyHistory,
    requestDownloadHistory,
    requestTenantInfo,
    updateHistoryData,
    RequestUserPreferences,
} from '../Shared/SharedComponents.actions';
import { IComponentsAppState } from '../Shared/SharedComponents.types';
import { Text } from '@fluentui/react/lib/Text';
import { HistoryColumns } from './HistoryTableColumns';
import { DetailsPanel } from '../Shared/Details/DetailsPanel';
import { detailsInitialState, detailsReducer, detailsReducerName } from '../Shared/Details/Details.reducer';
import { detailsSagas } from '../Shared/Details/Details.sagas';
import { EmptyResults } from '../Shared/Components/EmptyResults';
import ErrorView from '../Shared/Details/DetailsMessageBars/ErrorView';

import * as SummaryStyled from '../Summary/SummaryStyling';
import { RequestView } from '../Shared/Details/DetailsAdaptive';
import { requestFullyScrolled } from '../Shared/Details/Details.actions';
import { IDetailsAppState } from '../Shared/Details/Details.types';
import { FLYOUT_VIEW } from '../Shared/SharedConstants';
import { DetailsDockedView } from '../Shared/Details/DetailsDockedView';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { SpinnerContainer } from '../Shared/SharedLayout';
import { Pagination } from '../Shared/Components/Pagination';

function HistoryTable(): React.ReactElement {
    //intializing both reducers without persistance
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    useDynamicReducer(detailsReducerName, detailsReducer as Reducer, [detailsSagas], false);

    const reduxContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const {
        historySelectedPage,
        sortColumnField,
        sortDirection,
        historyTimePeriod,
        historySearchCriteria,
        isLoadingHistory,
        isDownloadingHistory,
        historyData,
        historyHasError,
        historyErrorMessage,
        historyTenantIdFilter,
        historyTotalRecords,
        historyDownloadHasError,
        historyDownloadErrorMessage,
        isPanelOpen,
        historyDefaultView,
        filterValue,
    } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    const { isPreviewOpen, isMicrofrontendOpen, isRequestFullyScrolled, isRequestFullyRendered } = useSelector(
        (state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState
    );
    const [pageCount, setPageCount] = React.useState<number>(1);
    const [dynamicPageSize, setDynamicPageSize] = React.useState<number>(50);
    const [searchLabel, setSearchLabel] = React.useState<string>('');

    const { history, toggleDetailsScreen } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );
    const navWrapperRef = React.useRef<HTMLDivElement>(null);
    const [filterHistoryData, setFilterHistoryData] = React.useState([]);

    React.useEffect(() => {
        function handleResize() {
            setDimensions({
                height: window.innerHeight,
                width: window.innerWidth,
            });
        }
        window.addEventListener('resize', handleResize);
        document.body.classList.add('scrollable-area');
        return (): void => {
            window.removeEventListener('resize', handleResize);
            document.body.classList.remove('scrollable-area');
        };
    }, []);

    React.useEffect(() => {
        dispatch(
            requestMyHistory(
                historySelectedPage,
                sortColumnField,
                sortDirection,
                '',
                historyTimePeriod,
                historyTenantIdFilter
            )
        );
    }, []);

    React.useEffect(() => {
        if (!history) {
            return;
        }
        fetchData();
    }, [history]);

    React.useEffect(() => {
        updateHistoryState();
        filteredHistoryData();
    }, [historyData, historyTotalRecords, filterValue]);

    React.useEffect(() => {
        dispatch(RequestUserPreferences());
        dispatch(requestTenantInfo());
    }, [dispatch]);

    React.useEffect(() => {
        if (historySearchCriteria) {
            setSearchLabel('Search Bar. Filtered Results by keyword ' + historySearchCriteria);
        } else {
            setSearchLabel('');
        }
    }, [historySearchCriteria]);

    const dropdownOptions: IDropdownOption[] = [
        { key: '3', text: 'Last 3 months' },
        { key: '6', text: 'Last 6 months' },
        { key: '9', text: 'Last 9 months' },
        { key: '12', text: 'Last year' },
        { key: '24', text: 'Last 2 years' },
    ];

    function convertRecordsAmountToFloat(historyRecords: any) {
        for (var i = 0; i < historyRecords.length; i++) {
            historyRecords[i]['UnitValue'] = isNaN(parseInt(historyRecords[i]['UnitValue']))
                ? historyRecords[i]['UnitValue']
                : parseFloat(historyRecords[i]['UnitValue']).toFixed(2);
            historyRecords[i]['TenantId'] = parseInt(historyRecords[i]['TenantId']);
        }
    }

    const updateHistoryState = (): void => {
        if (historyData && historyTotalRecords) {
            // if returned records is less than page size -> set this page to last
            if (historyData.length < dynamicPageSize) {
                setPageCount(historySelectedPage);
            } else {
                setPageCount(Math.ceil(historyTotalRecords / dynamicPageSize));
            }
        }
    };

    const fetchData = (): void => {
        const historyRecords = history['Records'];
        if (historyRecords.length > dynamicPageSize) {
            setDynamicPageSize(historyRecords.length);
        }
        convertRecordsAmountToFloat(historyRecords);
        dispatch(updateHistoryData(historyRecords, history.TotalRecords));
        return;
    };

    //TODO: pagination component
    const paginationProps = {
        pageCount: pageCount,
        selectedPage: historySelectedPage,
        onPageChange: (pageNumber: number) => {
            if (pageNumber !== historySelectedPage) {
                dispatch(
                    requestMyHistory(
                        pageNumber,
                        sortColumnField,
                        sortDirection,
                        historySearchCriteria,
                        historyTimePeriod,
                        historyTenantIdFilter
                    )
                );
            }
        },
    };

    const [selectedTimePeriod, setSelectedKey] = React.useState<string>('3');

    const handleHistoryTimePeriodChanged = (event: React.FormEvent<HTMLDivElement>, item: IDropdownOption): void => {
        setSelectedKey(item.key as string);
        const pageNumber = 1;
        dispatch(
            requestMyHistory(
                pageNumber,
                // historySelectedPage,
                sortColumnField,
                sortDirection,
                historySearchCriteria,
                parseInt(item.key.toString()),
                historyTenantIdFilter
            )
        );
    };

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

    const fetchTimeRangeLabel = (): string => {
        return 'Selected time period range: Last ' + historyTimePeriod + ' months';
    };

    const filteredHistoryData = () => {
        if (filterValue === 'All') {
            setFilterHistoryData(historyData);
        } else {
            const historyRecord = historyData.filter((item: any) => item.AppName == filterValue);
            setFilterHistoryData(historyRecord);
        }
    };
    return (
        <SummaryStyled.SummaryContainer
            isPanelOpen={isPanelOpen}
            bulkMessagebarHeight={0}
            aliasMessagebarHeight={0}
            windowHeight={dimensions.height}
            windowWidth={dimensions.width}
        >
            <div className="ms-Grid" dir="ltr">
                <div className="ms-Grid-row">
                    <div
                        className={
                            'ms-Grid-col ' +
                            (isPanelOpen && historyDefaultView !== FLYOUT_VIEW
                                ? toggleDetailsScreen
                                    ? ' ms-sm4 ms-xl4 ms-xxl4 ms-xxxl4 ms-hiddenLgDown horizontal-separator'
                                    : ' ms-sm6 ms-hiddenMdDown horizontal-separator'
                                : ' ms-sm12 ')
                        }
                    >
                        <Styled.HistoryContainer windowHeight={dimensions.height}>
                            <Styled.HistoryTitle>History</Styled.HistoryTitle>
                            <Stack
                                horizontal
                                horizontalAlign="space-between"
                                wrap
                                styles={{ root: { paddingBottom: '2%', width: '100%' } }}
                            >
                                <Stack
                                    horizontal
                                    tokens={Styled.HistoryNavStackTokens}
                                    styles={{ root: { width: '100%' } }}
                                >
                                    <Stack.Item align="baseline">
                                        <Dropdown
                                            style={{ width: '150px' }}
                                            ariaLabel={fetchTimeRangeLabel()}
                                            label="Time range"
                                            placeholder="Select an option"
                                            options={dropdownOptions}
                                            onChange={handleHistoryTimePeriodChanged}
                                            selectedKey={selectedTimePeriod}
                                        />
                                    </Stack.Item>

                                    <Stack.Item
                                        align="end"
                                        styles={{ root: { marginLeft: 'auto !important', color: 'red' } }}
                                    >
                                        <Stack
                                            horizontal
                                            horizontalAlign="space-between"
                                            grow
                                            tokens={Styled.HistoryNavStackTokens}
                                        >
                                            {!isLoadingHistory && historyData.length > 0 && (
                                                <Stack.Item align="start" className={isPanelOpen ? 'alignCenter' : ''}>
                                                    {isDownloadingHistory ? (
                                                        <Spinner
                                                            label="Downloading"
                                                            labelPosition="right"
                                                            style={{ margin: '5px' }}
                                                            ariaLabel="Downloading history data"
                                                        ></Spinner>
                                                    ) : (
                                                        <a
                                                            onClick={() => {
                                                                dispatch(
                                                                    requestDownloadHistory(
                                                                        historyTimePeriod,
                                                                        historySearchCriteria,
                                                                        sortColumnField,
                                                                        sortDirection,
                                                                        historyTenantIdFilter
                                                                    )
                                                                );
                                                            }}
                                                        >
                                                            <Stack horizontal>
                                                                <Stack.Item align="center">
                                                                    <div
                                                                        style={{
                                                                            height: '100%',
                                                                            paddingTop: 'calc(50% - 8px)',
                                                                        }}
                                                                    >
                                                                        <IconButton
                                                                            iconProps={{ iconName: 'Download' }}
                                                                            title="Download to Excel"
                                                                            style={Styled.HistoryIconStyling}
                                                                        />
                                                                    </div>
                                                                </Stack.Item>
                                                                <Stack.Item align="center">
                                                                    <Text className="ms-hiddenMdDown">
                                                                        Download To Excel
                                                                    </Text>
                                                                    <Text className="ms-hiddenLgUp"> Download </Text>
                                                                </Stack.Item>
                                                            </Stack>
                                                        </a>
                                                    )}
                                                </Stack.Item>
                                            )}
                                            {(historyTotalRecords !== 0 || historySearchCriteria !== '') && (
                                                <Stack.Item align="end">
                                                    <SearchBox
                                                        placeholder={'Search'}
                                                        ariaLabel={searchLabel}
                                                        onSearch={(searchCriteria) => {
                                                            dispatch(
                                                                requestMyHistory(
                                                                    1,
                                                                    'ActionDate',
                                                                    'DESC',
                                                                    searchCriteria,
                                                                    historyTimePeriod,
                                                                    historyTenantIdFilter
                                                                )
                                                            );
                                                        }}
                                                        onClear={() => {
                                                            dispatch(
                                                                requestMyHistory(
                                                                    1,
                                                                    sortColumnField,
                                                                    sortDirection,
                                                                    '',
                                                                    historyTimePeriod,
                                                                    historyTenantIdFilter
                                                                )
                                                            );
                                                        }}
                                                        style={{ border: 0, width: '100%' }}
                                                    />
                                                </Stack.Item>
                                            )}
                                        </Stack>
                                    </Stack.Item>
                                </Stack>
                            </Stack>
                            {!isLoadingHistory && (
                                <Stack padding="0 0 8px 0">
                                    {historyTotalRecords > 0 && (
                                        <Text className="ms-hiddenMdDown">
                                            Displaying requests{' '}
                                            <b>
                                                {dynamicPageSize * (historySelectedPage - 1) +
                                                    1 +
                                                    ' to ' +
                                                    (dynamicPageSize * (historySelectedPage - 1) +
                                                        filterHistoryData.length)}
                                            </b>{' '}
                                            out of <b>{historyTotalRecords}</b> from the Last{' '}
                                            {selectedTimePeriod === '3' ? selectedTimePeriod : historyTimePeriod} Months
                                        </Text>
                                    )}
                                    <Text className="ms-hiddenLgUp">Showing Last {historyTimePeriod} Months </Text>
                                </Stack>
                            )}
                            {isLoadingHistory && (
                                <SpinnerContainer>
                                    <Spinner label="Loading history..." />
                                </SpinnerContainer>
                            )}
                            {!isLoadingHistory && !historyErrorMessage && historyData.length === 0 && (
                                <EmptyResults message="No Historical Records Found" />
                            )}
                            {historyDownloadHasError && historyDownloadErrorMessage && (
                                <ErrorView errorMessage={historyDownloadErrorMessage} failureType={'Download'} />
                            )}
                            {!isLoadingHistory && !historyHasError && !historyErrorMessage && historyData.length !== 0 && (
                                <Stack grow>
                                    <Styled.HistoryTableContainer
                                        windowHeight={dimensions.height}
                                        windowWidth={dimensions.width}
                                        isPanelOpen={isPanelOpen}
                                    >
                                        <Stack
                                            style={{
                                                height:
                                                    dimensions.width < 1024 ? '100%' : `${dimensions.height - 250}px`,
                                            }}
                                        >
                                            <div
                                                style={{
                                                    position: 'relative',
                                                    height:
                                                        dimensions.width < 1024
                                                            ? '90%'
                                                            : `${dimensions.height - 350}px`,
                                                    overflowY: 'hidden',
                                                    marginBottom: '1%',
                                                }}
                                            >
                                                <ScrollablePane>
                                                    <HistoryColumns />
                                                </ScrollablePane>
                                            </div>
                                            <Stack horizontal horizontalAlign="center">
                                                <Pagination {...paginationProps} />
                                            </Stack>
                                        </Stack>
                                    </Styled.HistoryTableContainer>
                                </Stack>
                            )}
                            {historyHasError && historyErrorMessage && (
                                <Stack.Item>
                                    <Text> {historyErrorMessage} </Text>
                                </Stack.Item>
                            )}
                        </Styled.HistoryContainer>
                    </div>

                    {isPanelOpen && historyDefaultView !== FLYOUT_VIEW && (
                        <DetailsDockedView
                            templateType={'Summary'}
                            windowHeight={dimensions.height}
                            windowWidth={dimensions.width}
                        ></DetailsDockedView>
                    )}
                    {historyDefaultView === FLYOUT_VIEW && (
                        <div ref={navWrapperRef}>
                            <DetailsPanel
                                templateType={'Summary'}
                                windowHeight={dimensions.height}
                                windowWidth={dimensions.width}
                            />
                        </div>
                    )}
                </div>
            </div>
        </SummaryStyled.SummaryContainer>
    );
}

const connected = withContext(HistoryTable);
export { connected as HistoryTable };
