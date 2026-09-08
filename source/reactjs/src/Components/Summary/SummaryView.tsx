/* eslint-disable react/jsx-key */
import * as React from 'react';
import * as SummaryStyled from './SummaryStyling';
import * as SharedStyled from '../Shared/SharedLayout';
import { EmptyResults } from '../Shared/Components/EmptyResults';
import { Stack, IStackTokens } from '@fluentui/react/lib/Stack';
import { Checkbox } from '@fluentui/react/lib/Checkbox';
import { Text } from '@fluentui/react/lib/Text';
import { Spinner } from '@fluentui/react/lib/Spinner';
import { SummaryCards } from './SummaryCards/SummaryCards';
import { getTenantIcon } from '../Shared/Components/IconMapping';
import { DashboardView } from './Dashboard/DashboardView';
import { IGrouping } from '../../Helpers/groupPendingApprovals';
import { GroupingBy } from '../Shared/Components/GroupingBy';
import { Persona } from '../Shared/Components/Persona';
import { PersonaSize } from '../Shared/Components/Persona/Persona.types';
import { IComponentsAppState } from '../Shared/SharedComponents.types';
import {
    getSelectedSummary,
    getGroupedBySummaryMemo,
    getSummaryGroupedBy,
    getTenantInfo,
    getFilterValue,
    getIsLoadingSummary,
    getSelectedApprovalRecords,
    getSelectedDisplayDocumentNumbers,
    getCardViewSelected,
    getIsBulkSelected,
    getBulkActionConcurrentCall,
    getBulkFailedMsg,
    getBulkApproveStatus,
    getBulkApproveFailed,
    getIsProcessingBulkApproval,
    getPullTenantSummaryData,
    getIsLoadingPullTenantData,
    getIsPullTenantSelected,
    getPullTenantSummaryHasError,
    getPullTenantSummaryErrorMessage,
    getPullTenantSearchCriteria,
    getPullTenantSearchSelection,
    getTenantIdFromAppName,
    getFailedPullTenantRequests,
    getPullTenantSummaryCount,
    getIsPanelOpen,
    getTotalPullTenantCount,
    getFilteredTenantInfo,
    getExternalTenantInfoHasError,
    getExternalTenantInfoErrorMessage,
    getIsLoadingPullTenantSummaryCount,
    getPullTenantSummaryMemoized,
    groupedSummaryDataSelector,
    getFilteredSummaryMemoized,
    getSearchResults,
    getIsSearchResultsViewOpen,
    getIsLoadingSearchResults,
    getPropertyFilters,
    applyPropertyFilters,
    getHasError,
    getSummaryErrorMessage,
    getIsSubmitterView,
} from '../Shared/SharedComponents.selectors';
import { sharedComponentsReducerName, sharedComponentsInitialState } from '../Shared/SharedComponents.reducer';
import {
    getBulkFooterHeight,
    getBulkMessagebarHeight,
    getAliasMessagebarHeight,
    getReadRequests,
    getIsDisabled,
} from '../Shared/Details/Details.selectors';
import { getUserAlias } from '../Shared/SharedComponents.persistent-selectors';
import {
    updateApprovalRecords,
    updateFilterValue,
    updateBulkSelected,
    updateBulkFailedStatus,
    requestMySummary,
    updateBulkStatus,
    updateIsProcessingBulkApproval,
    requestPullTenantSummary,
    updatePullTenantSearchSelection,
    toggleSearchResultsView,
    initiateSearch,
    updatePropertyFilters,
} from '../Shared/SharedComponents.actions';
import { isEqual } from 'lodash';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { SummaryTable } from './SummaryTable/SummaryTable';
import ErrorView from '../Shared/Details/DetailsMessageBars/ErrorView';
import ErrorResult from '../Shared/Components/ErrorResult';
import { ContinueMessage } from '../Shared/Components/ContinueMessage';
import { CONTINUE_TIMEOUT } from '../Shared/SharedConstants';
import {
    Dialog,
    Dropdown,
    IDropdownOption,
    IconButton,
    MessageBar,
    MessageBarType,
    ProgressIndicator,
} from '@fluentui/react';

import PullTenantSummaryCountBanner from './PullTenantSummaryCountBanner';
import { breakpointMap } from '../Shared/Styles/Media';
import { useHistory } from 'react-router-dom';
import { DashboardAnalytics } from './Dashboard/DashboardAnalytics';
import { DashboardAnalyticsHeader } from './Dashboard/DashboardAnalyticsHeader';
import { DashboardFilterConfig } from './Dashboard/DashboardFilterConfig';
import { DocumentPreviewModal } from '../Shared/Details/DocumentPreview/DocumentPreviewModal';

interface ISearchPreviewState {
    isOpen: boolean;
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    attachmentId: string;
    attachmentName: string;
}

const defaultPreviewState: ISearchPreviewState = {
    isOpen: false,
    tenantId: '',
    documentNumber: '',
    displayDocumentNumber: '',
    attachmentId: '',
    attachmentName: '',
};
import { PrimaryHeader } from '../Shared/Components/PrimaryHeader/PrimaryHeader';

interface ISummaryViewProps {
    windowHeight: number;
    windowWidth: number;
    isDashboardPageView?: boolean;
}

function SummaryViewBase(props: ISummaryViewProps): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { windowHeight, windowWidth, isDashboardPageView } = props;
    const history = useHistory();

    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const tenantInfo = useSelector(getTenantInfo);
    const filterValue = useSelector(getFilterValue);
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const groupedSummaryData: any = useSelector((state: IComponentsAppState) => groupedSummaryDataSelector(state));
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const selectedDisplayDocumentNumbers = useSelector(getSelectedDisplayDocumentNumbers);
    const isProcessingBulkApproval = useSelector(getIsProcessingBulkApproval);
    const isCardViewSelected = useSelector(getCardViewSelected);
    const isBulkSelected = useSelector(getIsBulkSelected);
    const bulkActionConcurrentCall = useSelector(getBulkActionConcurrentCall);
    const bulkFailedMsg = useSelector(getBulkFailedMsg);
    const bulkApproveStatus = useSelector(getBulkApproveStatus);
    const bulkApproveFailed = useSelector(getBulkApproveFailed);
    const bulkFooterHeight = useSelector(getBulkFooterHeight);
    const bulkMessagebarHeight = useSelector(getBulkMessagebarHeight);
    const aliasMessagebarHeight = useSelector(getAliasMessagebarHeight);
    const readRequests = useSelector(getReadRequests);
    const userAlias = useSelector(getUserAlias);
    const isLoadingPullTenantData = useSelector(getIsLoadingPullTenantData);
    const isPullTenantSelected = useSelector(getIsPullTenantSelected);
    const pullTenantSummaryHasError = useSelector(getPullTenantSummaryHasError);
    const pullTenantSummaryErrorMessage = useSelector(getPullTenantSummaryErrorMessage);
    const pullTenantSummaryCount = useSelector(getPullTenantSummaryCount);
    const isPanelOpen = useSelector(getIsPanelOpen);
    const totalPullTenantCount = useSelector(getTotalPullTenantCount);
    const filteredTenantInfo = useSelector(getFilteredTenantInfo);
    const externalTenantInfoHasError = useSelector(getExternalTenantInfoHasError);
    const externalTenantInfoErrorMessage = useSelector(getExternalTenantInfoErrorMessage);
    const isLoadingPullTenantSummaryCount = useSelector(getIsLoadingPullTenantSummaryCount);
    const pullTenantSearchCriteria = useSelector(getPullTenantSearchCriteria);
    const disabled = useSelector(getIsDisabled);
    const filteredSummaryData = useSelector(getFilteredSummaryMemoized);
    const searchResults = useSelector(getSearchResults);
    const isSearchResultsViewOpen = useSelector(getIsSearchResultsViewOpen);
    const isLoadingSearchResults = useSelector(getIsLoadingSearchResults);
    const propertyFilters = useSelector(getPropertyFilters);
    const hasError = useSelector(getHasError);
    const summaryErrorMessage = useSelector(getSummaryErrorMessage);
    const isSubmitterView = useSelector(getIsSubmitterView);

    const [bulkSelectedRecordsLength, setBulkSelectedRecordsLength] = React.useState<number>(0);
    const [checked, setChecked] = React.useState<boolean>(false);
    const [bulkCheckboxRenderKey, setBulkCheckboxRenderKey] = React.useState<number>(0);
    const bulkCheckboxRef = React.useRef<any>(null);
    const [showContinue, setShowContinue] = React.useState(false);
    const [isAnalyticsCollapsed, setIsAnalyticsCollapsed] = React.useState(false);
    const isDashboardView = isDashboardPageView || false;
    // Treat 200% browser zoom (≤640px effective width) the same as mobile for responsive layout
    const isMobileOrHighZoom = windowWidth <= 640;

    const stackTokens: IStackTokens = { childrenGap: 5, padding: '0px 5px 10px 0px' };
    const [previewState, setPreviewState] = React.useState<ISearchPreviewState>(defaultPreviewState);

    const handleSearchPreviewClick = (
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        attachmentId: string,
        attachmentName: string
    ) => {
        // Telemetry is emitted inside MatchCitation so all click paths (field, attachment-preview,
        // attachment-open, callout-toggle) are tracked consistently — not just attachment previews.
        setPreviewState({
            isOpen: true,
            tenantId,
            documentNumber,
            displayDocumentNumber,
            attachmentId,
            attachmentName,
        });
    };

    const tenantIdforFilterValue = useSelector((state: any) => getTenantIdFromAppName(state, filterValue));
    const pullTenantSummaryDataMemo = useSelector(getPullTenantSummaryMemoized);

    const isExternalActionDetails = filteredTenantInfo?.isExternalTenantActionDetails;

    const summaryCount = groupedSummaryData ? groupedSummaryData.length : 0;
    const pullTenantCount = !isSubmitterView && typeof totalPullTenantCount === 'number' ? totalPullTenantCount : 0;
    const totalRequestCount = summaryCount + pullTenantCount;

    const isSingleGroupShown =
        isBulkSelected || filterValue !== 'All' || (groupedSummaryData?.length <= 1 && pullTenantCount === 0);

    React.useEffect(() => {
        setChecked(false);
        setBulkSelectedRecordsLength(0);
        dispatch(updateApprovalRecords([]));
    }, [filterValue, isCardViewSelected]);

    React.useEffect(() => {
        if (selectedApprovalRecords.length == 0) {
            setChecked(false);
            setBulkSelectedRecordsLength(0);
        }
    }, [selectedApprovalRecords]);

    React.useEffect(() => {
        let timeoutId: number;
        if (bulkCheckboxRenderKey > 0) {
            timeoutId = window.setTimeout(() => {
                bulkCheckboxRef.current?.focus?.();
            }, 0);
        }
        return () => {
            if (timeoutId) clearTimeout(timeoutId);
        };
    }, [bulkCheckboxRenderKey]);

    React.useEffect(() => {
        let continueTimer: any;
        if (isProcessingBulkApproval) {
            continueTimer = setTimeout(() => setShowContinue(true), CONTINUE_TIMEOUT);
        } else {
            clearTimeout(continueTimer);
            setShowContinue(false);
        }
        return () => {
            clearTimeout(continueTimer);
            setShowContinue(false);
        };
    }, [isProcessingBulkApproval]);

    React.useEffect(() => {
        if (bulkApproveStatus && !isPullTenantSelected) {
            dispatch(requestMySummary(userAlias));
        }
    }, [bulkApproveStatus]);

    React.useEffect(() => {
        if (isBulkSelected && !isPullTenantSelected) {
            const organizedSummaryData = filterSummary(groupedSummaryData);
            if (organizedSummaryData && organizedSummaryData.length == 0) {
                dispatch(updateFilterValue('All'));
                dispatch(updateBulkSelected(false));
            }
        }
    }, [groupedSummaryData]);

    React.useEffect(() => {
        // Collapse analytics when isBulkSelected is true and isPanelOpen goes from false to true
        if (isBulkSelected && isPanelOpen) {
            setIsAnalyticsCollapsed(true);
        }
    }, [isPanelOpen]);

    const [progress, setProgress] = React.useState(0);

    React.useEffect(() => {
        let interval: NodeJS.Timeout | undefined;
        if (isLoadingSearchResults) {
            setProgress(0);
            interval = setInterval(() => {
                setProgress((prev) => {
                    // Increment up to 95% while loading
                    if (prev < 0.95) return prev + 0.05;
                    return prev;
                });
            }, 400);
        }
        return () => {
            if (interval) clearInterval(interval);
        };
    }, [isLoadingSearchResults]);

    const handleSearchFromUrl = React.useCallback((): void => {
        const urlSearch = new URLSearchParams(history.location.search);
        const filterParam = urlSearch.get('filter');

        if (userAlias && isSearchResultsViewOpen && !isLoadingSearchResults) {
            if (filterParam && totalRequestCount > 0) {
                dispatch(initiateSearch(userAlias, filterParam));
            }
        }
    }, [userAlias, isSearchResultsViewOpen, isLoadingSearchResults, totalRequestCount, history, dispatch]);

    React.useEffect(() => {
        handleSearchFromUrl();
    }, []);

    function filterSummary(organizedSummary: any): IGrouping[] {
        if (filterValue === 'All') {
            return organizedSummary;
        } else if (isPullTenantSelected) {
            return pullTenantSummaryDataMemo && pullTenantSummaryDataMemo.length > 0
                ? [
                      {
                          key: tenantIdforFilterValue,
                          displayValue: filterValue,
                          isPullModelEnabled: true,
                          grouping: pullTenantSummaryDataMemo,
                      },
                  ]
                : [
                      {
                          key: tenantIdforFilterValue,
                          displayValue: filterValue,
                          isPullModelEnabled: true,
                          grouping: [],
                      },
                  ];
        }
        return filteredSummaryData;
    }

    const getBulkSelectableCount = (tenantGroup?: IGrouping): number => {
        return Math.min(
            tenantGroup?.grouping?.filter(
                (summaryItem) =>
                    summaryItem.IsRead ||
                    readRequests.includes(summaryItem.DocumentNumber) ||
                    !summaryItem.IsControlsAndComplianceRequired
            ).length ?? 0,
            bulkActionConcurrentCall
        );
    };

    const getBulkCheckboxAriaLabel = (tenantGroup: IGrouping, isChecked: boolean): string => {
        const selectedCount = isChecked ? getBulkSelectableCount(tenantGroup) : 0;
        const checkState = isChecked ? 'checked' : 'unchecked';

        return `${checkState} Select all Pending Approvals bulk records for ${tenantGroup.displayValue} and You have selected ${selectedCount} out of max allowed ${bulkActionConcurrentCall} record(s) to take bulk action.`;
    };

    const onCheckboxChange = (ev: React.FormEvent<HTMLElement>, checked: boolean, tenantGroup?: IGrouping): void => {
        const selectedCount = checked ? getBulkSelectableCount(tenantGroup) : 0;

        setChecked(checked);
        setBulkCheckboxRenderKey((prevValue) => prevValue + 1);
        if (checked) {
            setBulkSelectedRecordsLength(selectedCount);
        } else {
            for (let i = 0; i < selectedApprovalRecords.length; i++) {
                updateCardStyle(false, selectedApprovalRecords[i].DocumentNumber);
            }
            dispatch(updateApprovalRecords([]));
            setBulkSelectedRecordsLength(0);
        }
    };

    function updateCardStyle(selected: boolean, elementID: any): void {
        const element = document.getElementById(elementID);
        if (element) {
            const removeClass = selected ? 'defaultCardStyle' : 'bulkSelectedCardStyle';
            const addClass = selected ? 'bulkSelectedCardStyle' : 'defaultCardStyle';
            element.classList.remove(removeClass);
            element.classList.add(addClass);
        }
    }

    function isBulkCheckRequired(tenantGroup: IGrouping): boolean {
        if (tenantGroup.isPullModelEnabled) {
            return false;
        }
        return getBulkSelectableCount(tenantGroup) > 0;
    }

    function renderTenantGroup(tenantGroup: IGrouping): any {
        return isCardViewSelected ? (
            tenantGroup?.isPullModelEnabled ? (
                <PullTenantSummaryCountBanner
                    pullTenantSummaryCount={pullTenantSummaryCount}
                    isTableView={!isCardViewSelected}
                    filterValue={tenantGroup.displayValue}
                />
            ) : (
                <SummaryCards
                    summary={tenantGroup.grouping}
                    bulkSelectedRecordsLength={bulkSelectedRecordsLength}
                ></SummaryCards>
            )
        ) : (
            <SummaryTable
                key={tenantGroup.key}
                tenantGroup={tenantGroup.grouping}
                tenantName={tenantGroup.displayValue}
                isPullTenant={tenantGroup.isPullModelEnabled}
                isSingleGroupShown={isSingleGroupShown}
            />
        );
    }

    function renderSummary(): any {
        const organizedSummaryData = filterSummary(groupedSummaryData);
        const validGrouping = isSearchResultsViewOpen ? null : isBulkSelected ? GroupingBy.Tenant : summaryGroupedBy;
        switch (validGrouping) {
            case GroupingBy.Tenant:
                return organizedSummaryData.map((tenantGroup: IGrouping) => {
                    // if no approvals for a specific tenant are found
                    if (tenantGroup.grouping === null) {
                        return (
                            <Stack tokens={stackTokens}>
                                <Stack.Item>
                                    <Text variant="xLarge">{'Unable to load card'}</Text>
                                </Stack.Item>
                            </Stack>
                        );
                    }
                    const tenantDescription = tenantInfo?.find(
                        (tenant: { appName: string }) => tenant.appName === tenantGroup.displayValue
                    )?.tenantDescription;
                    return (
                        <Stack tokens={stackTokens}>
                            <Stack.Item>
                                <Stack horizontal horizontalAlign="space-between">
                                    <Stack.Item>
                                        <Text variant="xLarge">
                                            <SummaryStyled.CardGroupLabel>
                                                {isCardViewSelected &&
                                                    isBulkSelected &&
                                                    isBulkCheckRequired(tenantGroup) && (
                                                        <SummaryStyled.SelectAllCheckStyle>
                                                            {' '}
                                                            <Checkbox
                                                                key={`${tenantGroup.key}-${bulkCheckboxRenderKey}`}
                                                                componentRef={bulkCheckboxRef}
                                                                label=""
                                                                checked={checked}
                                                                title="Select All Bulk Records"
                                                                ariaLabel={getBulkCheckboxAriaLabel(tenantGroup, checked)}
                                                                disabled={!(tenantInfo?.length > 0)}
                                                                onChange={(ev, isChecked): void => {
                                                                    onCheckboxChange(ev, !!isChecked, tenantGroup);
                                                                }}
                                                            />
                                                        </SummaryStyled.SelectAllCheckStyle>
                                                    )}
                                                <SummaryStyled.CardTenantImage
                                                    isCardViewSelected={isCardViewSelected}
                                                    isBulkSelected={isBulkSelected}
                                                >
                                                    {getTenantIcon(tenantGroup.displayValue, tenantInfo, '26px')}
                                                </SummaryStyled.CardTenantImage>
                                                <SummaryStyled.TenantLabel>
                                                    <h2>{tenantGroup.displayValue}</h2>
                                                </SummaryStyled.TenantLabel>
                                            </SummaryStyled.CardGroupLabel>
                                        </Text>
                                    </Stack.Item>
                                </Stack>
                                {tenantDescription && (
                                    <SummaryStyled.TenantDescription>
                                        {tenantDescription}
                                    </SummaryStyled.TenantDescription>
                                )}
                            </Stack.Item>
                            <Stack.Item />
                            <Stack.Item>
                                {tenantGroup?.isPullModelEnabled &&
                                    !(tenantGroup?.grouping?.length > 0) &&
                                    !pullTenantSearchCriteria &&
                                    !pullTenantSummaryHasError && (
                                        <SharedStyled.CenterHeightSpace>
                                            <EmptyResults message={isSubmitterView ? "You have no requests sent for approval." : "There are no pending approvals for you."} />
                                        </SharedStyled.CenterHeightSpace>
                                    )}
                                {(!tenantGroup?.isPullModelEnabled ||
                                    tenantGroup?.grouping?.length > 0 ||
                                    pullTenantSearchCriteria ||
                                    pullTenantSummaryHasError) &&
                                    renderTenantGroup(tenantGroup)}
                            </Stack.Item>
                        </Stack>
                    );
                });
            case GroupingBy.Submitter:
                return organizedSummaryData.map((submitterGroup: IGrouping) => {
                    // if no approvals for a specific alias are found
                    if (submitterGroup.grouping === null) {
                        return (
                            <Stack tokens={stackTokens}>
                                <Stack.Item>
                                    <Text variant="xLarge">{'Unable to load card'}</Text>
                                </Stack.Item>
                            </Stack>
                        );
                    }
                    return (
                        <Stack tokens={stackTokens}>
                            <Stack.Item>
                                <Text variant="xLarge">
                                    <SummaryStyled.CardGroupLabel>
                                        <SummaryStyled.PersonaContainer>
                                            <Persona
                                                // Handle scenarios with multiple domains in future. Needs to be updated to upn
                                                emailAlias={submitterGroup.key + `${__UPN_SUFFIX__}`}
                                                size={PersonaSize.size32}
                                            />
                                        </SummaryStyled.PersonaContainer>
                                        <SummaryStyled.SubmitterLabel>
                                            {submitterGroup.displayValue}
                                        </SummaryStyled.SubmitterLabel>
                                    </SummaryStyled.CardGroupLabel>
                                </Text>
                            </Stack.Item>
                            <Stack.Item />
                            <Stack.Item>
                                {isCardViewSelected ? (
                                    <SummaryCards
                                        summary={submitterGroup.grouping}
                                        bulkSelectedRecordsLength={bulkSelectedRecordsLength}
                                    ></SummaryCards>
                                ) : (
                                    <SummaryTable
                                        key={submitterGroup.key}
                                        tenantGroup={submitterGroup.grouping}
                                        tenantName={submitterGroup.displayValue}
                                        isPullTenant={submitterGroup.isPullModelEnabled}
                                        isSingleGroupShown={isSingleGroupShown}
                                    />
                                )}
                            </Stack.Item>
                        </Stack>
                    );
                });
            case GroupingBy.Date:
                return organizedSummaryData.map((dateGroup: IGrouping) => {
                    // if no approvals for a specific date are found
                    if (dateGroup.grouping === null) {
                        return (
                            <Stack tokens={stackTokens}>
                                <Stack.Item>
                                    <Text variant="xLarge">{'Unable to load card'}</Text>
                                </Stack.Item>
                            </Stack>
                        );
                    }
                    return (
                        <Stack tokens={stackTokens}>
                            <Stack.Item>
                                <Text variant="xLarge">
                                    <SummaryStyled.CardGroupLabel>
                                        <SummaryStyled.CalendarIcon iconName="Calendar" />
                                        <SummaryStyled.SubmitterLabel>
                                            <strong>{dateGroup.displayValue} </strong>
                                        </SummaryStyled.SubmitterLabel>
                                    </SummaryStyled.CardGroupLabel>
                                </Text>
                            </Stack.Item>
                            <Stack.Item />
                            <Stack.Item>
                                {isCardViewSelected ? (
                                    <SummaryCards
                                        summary={dateGroup.grouping}
                                        bulkSelectedRecordsLength={bulkSelectedRecordsLength}
                                    ></SummaryCards>
                                ) : (
                                    <SummaryTable
                                        key={dateGroup.key}
                                        tenantGroup={dateGroup.grouping}
                                        tenantName={dateGroup.displayValue}
                                        isPullTenant={dateGroup.isPullModelEnabled}
                                        isSingleGroupShown={isSingleGroupShown}
                                    />
                                )}
                            </Stack.Item>
                        </Stack>
                    );
                });
            case GroupingBy.Category:
                return organizedSummaryData.map((categoryGroup: IGrouping) => {
                    // if no approvals for a specific category are found
                    if (categoryGroup.grouping === null) {
                        return (
                            <Stack tokens={stackTokens}>
                                <Stack.Item>
                                    <Text variant="xLarge">{'Unable to load card'}</Text>
                                </Stack.Item>
                            </Stack>
                        );
                    }
                    return (
                        <Stack tokens={stackTokens}>
                            <Stack.Item>
                                <Text variant="xLarge">
                                    <SummaryStyled.CardGroupLabel>
                                        <SummaryStyled.TenantLabel>
                                            {categoryGroup.displayValue}
                                        </SummaryStyled.TenantLabel>
                                    </SummaryStyled.CardGroupLabel>
                                </Text>
                            </Stack.Item>
                            <Stack.Item />
                            <Stack.Item>{renderTenantGroup(categoryGroup)}</Stack.Item>
                        </Stack>
                    );
                });
            default:
                return (
                    <>
                        {isLoadingSearchResults ? (
                            progress >= 0.95 ? (
                                <SharedStyled.SpinnerContainer>
                                    <Spinner label="Finalizing your results" />
                                </SharedStyled.SpinnerContainer>
                            ) : (
                                <SharedStyled.SpinnerContainer>
                                    <ProgressIndicator label="Searching your approvals..." percentComplete={progress} />
                                </SharedStyled.SpinnerContainer>
                            )
                        ) : searchResults && searchResults.length > 0 ? (
                            <SummaryCards
                                summary={searchResults ?? []}
                                bulkSelectedRecordsLength={bulkSelectedRecordsLength}
                                onSearchPreviewClick={handleSearchPreviewClick}
                            ></SummaryCards>
                        ) : (
                            <SharedStyled.CenterHeightSpace>
                                <EmptyResults message="No results found for your search" />
                            </SharedStyled.CenterHeightSpace>
                        )}
                    </>
                );
        }
    }

    const onDismissHandler = (): void => {
        dispatch(updateBulkFailedStatus(false));
    };

    const numBulkFailures = bulkApproveFailed && bulkFailedMsg && bulkFailedMsg.length > 0 ? bulkFailedMsg.length : 0;
    const bulkFailureMessageOffset = numBulkFailures > 0 ? numBulkFailures * 20 + 60 : 0;

    // Memoize filtered summary data for analytics - uses shared applyPropertyFilters function
    const analyticsData = React.useMemo(() => {
        if (!isBulkSelected || isCardViewSelected || isPullTenantSelected) return [];
        const filtered = filterSummary(groupedSummaryData);
        const baseData = filtered.length > 0 ? filtered[0].grouping || [] : [];

        return applyPropertyFilters(baseData, propertyFilters);
    }, [isBulkSelected, isCardViewSelected, isPullTenantSelected, groupedSummaryData, filterValue, propertyFilters]);

    // Handler for when a property filter is clicked in DashboardAnalytics
    const handlePropertyFilter = React.useCallback(
        (propertyKey: string, value: string) => {
            // Check if there's an altKey for this property
            const config = DashboardFilterConfig.getPropertyConfig(propertyKey);
            const filterKey = config?.altKey || propertyKey;

            const currentValues = propertyFilters[filterKey] || [];
            const newFilters = { ...propertyFilters };

            if (currentValues.includes(value)) {
                // Remove the filter
                newFilters[filterKey] = currentValues.filter((v: string) => v !== value);
            } else {
                // Add the filter
                newFilters[filterKey] = [...currentValues, value];
            }

            dispatch(updatePropertyFilters(newFilters));
        },
        [propertyFilters, dispatch]
    );

    return (
        <Stack styles={disabled ? SummaryStyled.disableInteractionStyle : null}>
            {!bulkApproveStatus && bulkApproveFailed && (
                <Stack.Item>
                    <ErrorView
                        errorMessages={isPullTenantSelected ? null : bulkFailedMsg}
                        errorMessage={isPullTenantSelected ? bulkFailedMsg?.[0] : null}
                        failureType={'Submit'}
                        dismissHandler={onDismissHandler}
                    />
                </Stack.Item>
            )}
            {isPullTenantSelected && pullTenantSummaryHasError && pullTenantSummaryErrorMessage && (
                <Stack.Item>
                    <ErrorView errorMessage={pullTenantSummaryErrorMessage} failureType={'Loading requests'} />
                </Stack.Item>
            )}
            {filteredTenantInfo &&
                isExternalActionDetails &&
                externalTenantInfoHasError &&
                externalTenantInfoErrorMessage && (
                    <Stack.Item>
                        <ErrorView
                            errorMessage={externalTenantInfoErrorMessage}
                            failureType={'Loading application data'}
                        />
                    </Stack.Item>
                )}
            {bulkApproveStatus && !bulkApproveFailed && (
                <Stack.Item>
                    <MessageBar
                        messageBarType={MessageBarType.success}
                        isMultiline={false}
                        onDismiss={(): void => {
                            dispatch(updateBulkStatus(false));
                        }}
                    >
                        Your action has successfully completed!
                    </MessageBar>
                </Stack.Item>
            )}
            {
                <Stack.Item>
                    {!isLoadingSummary &&
                        groupedSummaryData &&
                        !isSearchResultsViewOpen &&
                        totalRequestCount > 0 &&
                        !(isPullTenantSelected && isLoadingPullTenantData) && (
                            <Stack
                                horizontal
                                horizontalAlign="space-between"
                                verticalAlign="center"
                                styles={{
                                        root: {
                                            marginTop: '10px', marginBottom: '8px',
                                            paddingRight: isMobileOrHighZoom ? '8vw' : '2vw',
                                        },
                                    }}
                            >
                                <SummaryStyled.SummaryPageTitle isTableView={!isCardViewSelected}>
                                    {isDashboardPageView
                                            ? 'Dashboard'
                                            : isSubmitterView
                                            ? 'Sent for Approval'
                                            : 'Pending Approvals'}
                                </SummaryStyled.SummaryPageTitle>

                                {/* Collapsed Analytics Header - when collapsed on desktop */}
                                {isBulkSelected &&
                                    isAnalyticsCollapsed &&
                                    !isCardViewSelected &&
                                    !isPullTenantSelected && (
                                        <Stack.Item>
                                            <DashboardAnalyticsHeader
                                                isCollapsed={isAnalyticsCollapsed}
                                                onToggleCollapse={() =>
                                                    setIsAnalyticsCollapsed(!isAnalyticsCollapsed)
                                                }
                                            />
                                        </Stack.Item>
                                    )}
                            </Stack>
                        )}
                    <SummaryStyled.SummaryLayoutContainer
                        className="scroll-hidden v-scroll-auto custom-scrollbar"
                        windowHeight={windowHeight}
                        windowWidth={windowWidth}
                        selectedApprovalRecords={selectedApprovalRecords}
                        footerHeight={bulkFooterHeight}
                        bulkMessagebarHeight={bulkMessagebarHeight}
                        aliasMessagebarHeight={aliasMessagebarHeight}
                        bulkFailureMessageOffset={bulkFailureMessageOffset}
                        isDashboardView={isDashboardView}
                    >
                        {!isLoadingSummary &&
                            groupedSummaryData &&
                            !isSearchResultsViewOpen &&
                            totalRequestCount > 0 && (
                                <PrimaryHeader />
                            )}
                        {!isLoadingSummary &&
                            isSearchResultsViewOpen &&
                            !(isPullTenantSelected && isLoadingPullTenantData) && (
                                <Stack
                                    horizontal
                                    styles={{ root: { marginLeft: 'calc(2vw)', marginBottom: '10px' } }}
                                    tokens={{ childrenGap: 5 }}
                                >
                                    <Stack.Item>
                                        <IconButton
                                            iconProps={{ iconName: 'Back' }}
                                            title="Clear search"
                                            ariaLabel="Clear search"
                                            onClick={(): void => {
                                                const urlSearch = new URLSearchParams(history.location.search);
                                                urlSearch.delete('filter');
                                                history.push({
                                                    pathname: history.location.pathname,
                                                    search: urlSearch.toString(),
                                                });
                                                dispatch(toggleSearchResultsView(false));
                                            }}
                                        />
                                    </Stack.Item>
                                    <Stack.Item>
                                        <SummaryStyled.SearchResultsTitle>
                                            Search Results{' '}
                                            {!isLoadingSearchResults && searchResults && searchResults.length > 0
                                                ? `(${searchResults.length})`
                                                : ''}
                                        </SummaryStyled.SearchResultsTitle>
                                        <Stack horizontal verticalAlign="center">
                                            <SummaryStyled.SearchResultsSubtext>
                                                Results are found with the help of AI. Please verify important
                                                information.
                                                <img
                                                    src="/icons/sparkle24bluedark.png"
                                                    alt="Sparkle"
                                                    style={{
                                                        width: 20,
                                                        height: 20,
                                                        verticalAlign: 'middle',
                                                        marginRight: 6,
                                                    }}
                                                />
                                            </SummaryStyled.SearchResultsSubtext>
                                        </Stack>
                                    </Stack.Item>
                                </Stack>
                            )}
                        {(isLoadingSummary ||
                            (!isLoadingSummary &&
                                groupedSummaryData?.length === 0 &&
                                isLoadingPullTenantSummaryCount)) && (
                            <SharedStyled.SpinnerContainer>
                                <Spinner label="Loading requests..." />
                            </SharedStyled.SpinnerContainer>
                        )}
                        {isLoadingPullTenantData && (
                            <SharedStyled.SpinnerContainer>
                                <Spinner label="Loading requests..." />
                            </SharedStyled.SpinnerContainer>
                        )}
                        {isProcessingBulkApproval && (
                            <Dialog
                                hidden={false}
                                modalProps={{ isBlocking: true }}
                                dialogContentProps={{ showCloseButton: false }}
                            >
                                <Stack tokens={{ childrenGap: '12' }}>
                                    <Stack.Item
                                        styles={
                                            !isPullTenantSelected && showContinue
                                                ? null
                                                : { root: { paddingTop: '10%' } }
                                        }
                                    >
                                        <Spinner label="Processing..." ariaLabel="Processing action" />
                                    </Stack.Item>
                                    {!isPullTenantSelected && showContinue && (
                                        <Stack.Item>
                                            <ContinueMessage isBulkAction={true} />
                                        </Stack.Item>
                                    )}
                                </Stack>
                            </Dialog>
                        )}
                        {!isLoadingSummary &&
                            !isProcessingBulkApproval &&
                            groupedSummaryData &&
                            totalRequestCount == 0 && !hasError && <EmptyResults message={isSubmitterView ? "No Submitted Requests" : "No Pending Requests"} />}
                        {!isLoadingSummary && hasError && summaryErrorMessage && totalRequestCount == 0 && (
                            <ErrorResult message={summaryErrorMessage} />
                        )}
                        {!isLoadingSummary &&
                            !isSubmitterView &&
                            filterValue === 'All' &&
                            !isPanelOpen &&
                            !isSearchResultsViewOpen &&
                            !isDashboardView &&
                            pullTenantSummaryCount &&
                            pullTenantSummaryCount.length > 0 &&
                            totalPullTenantCount > 0 && (
                                <PullTenantSummaryCountBanner
                                    pullTenantSummaryCount={pullTenantSummaryCount}
                                    isTableView={!isCardViewSelected}
                                />
                            )}
                        {!isLoadingSummary &&
                            groupedSummaryData &&
                            totalRequestCount > 0 &&
                            !(isPullTenantSelected && isLoadingPullTenantData) &&
                            /* Dashboard-specific search loading indicator */
                            (isDashboardView && isSearchResultsViewOpen && isLoadingSearchResults ? (
                                <SharedStyled.SpinnerContainer>
                                    {progress >= 0.95 ? (
                                        <Spinner label="Finalizing your results" />
                                    ) : (
                                        <ProgressIndicator
                                            label="Searching your approvals..."
                                            percentComplete={progress}
                                        />
                                    )}
                                </SharedStyled.SpinnerContainer>
                            ) : isDashboardView ? (
                                <DashboardView
                                    summaryData={
                                        isSearchResultsViewOpen
                                            ? searchResults ?? []
                                            : [
                                                  ...groupedSummaryData.flatMap(
                                                      (group: IGrouping) => group.grouping || []
                                                  ),
                                                  ...(pullTenantSummaryDataMemo || []),
                                              ]
                                    }
                                    windowWidth={windowWidth}
                                    windowHeight={windowHeight}
                                    isSearchResultsViewOpen={isSearchResultsViewOpen}
                                    isPanelOpen={isPanelOpen}
                                />
                            ) : (
                                <>
                                    <Stack
                                        horizontal={!isMobileOrHighZoom}
                                        tokens={{ childrenGap: 0 }}
                                        styles={{ root: { alignItems: 'stretch' } }}
                                    >
                                        <Stack.Item grow styles={{ root: { minWidth: 0 } }}>
                                            {renderSummary()}
                                        </Stack.Item>
                                        {/* Show analytics below the table when mobile / high zoom and not collapsed and bulk is selected */}

                                        {isMobileOrHighZoom &&
                                            isBulkSelected &&
                                            !isAnalyticsCollapsed &&
                                            !isCardViewSelected &&
                                            !isPullTenantSelected && (
                                                <Stack.Item
                                                    styles={{
                                                        root: {
                                                            width: '100%',
                                                            transform: 'translateY(-20px)',
                                                        },
                                                    }}
                                                >
                                                    <DashboardAnalytics
                                                        windowWidth={windowWidth}
                                                        summaryData={analyticsData}
                                                        height={200}
                                                        width={windowWidth - 40}
                                                        isCollapsed={isAnalyticsCollapsed}
                                                        onToggleCollapse={() =>
                                                            setIsAnalyticsCollapsed(!isAnalyticsCollapsed)
                                                        }
                                                        isBulkSelected={isBulkSelected}
                                                        onPropertyFilter={handlePropertyFilter}
                                                        selectedDisplayDocumentNumbers={selectedDisplayDocumentNumbers}
                                                    />
                                                </Stack.Item>
                                            )}

                                        {/* Show analytics to the right when desktop (not mobile / not high zoom) and not collapsed and bulk is selected */}
                                        {!isMobileOrHighZoom &&
                                            isBulkSelected &&
                                            !isAnalyticsCollapsed &&
                                            !isCardViewSelected &&
                                            !isPullTenantSelected && (
                                                <Stack.Item styles={{ root: { width: '300px', flexShrink: 0 } }}>
                                                    <DashboardAnalytics
                                                        windowWidth={windowWidth}
                                                        summaryData={analyticsData}
                                                        height={windowHeight - 250}
                                                        width={280}
                                                        isCollapsed={isAnalyticsCollapsed}
                                                        onToggleCollapse={() =>
                                                            setIsAnalyticsCollapsed(!isAnalyticsCollapsed)
                                                        }
                                                        isBulkSelected={isBulkSelected}
                                                        onPropertyFilter={handlePropertyFilter}
                                                        selectedDisplayDocumentNumbers={selectedDisplayDocumentNumbers}
                                                    />
                                                </Stack.Item>
                                            )}
                                    </Stack>
                                </>
                            ))}
                    </SummaryStyled.SummaryLayoutContainer>
                </Stack.Item>
            }
            {previewState.isOpen && (
                <DocumentPreviewModal
                    isOpen={previewState.isOpen}
                    onDismiss={() => setPreviewState((prev) => ({ ...prev, isOpen: false }))}
                    tenantId={previewState.tenantId}
                    documentNumber={previewState.documentNumber}
                    displayDocumentNumber={previewState.displayDocumentNumber}
                    attachmentId={previewState.attachmentId}
                    attachmentName={previewState.attachmentName}
                    userAlias={userAlias}
                />
            )}
        </Stack>
    );
}

function areEqual(prevProps: ISummaryViewProps, nextProps: ISummaryViewProps): boolean {
    /*
    return true if passing nextProps to render would return
    the same result as passing prevProps to render,
    otherwise return false
    */
    const res =
        isEqual(prevProps.windowHeight, nextProps.windowHeight) &&
        isEqual(prevProps.windowWidth, nextProps.windowWidth);
    return res;
}
const memoizedSummaryView = React.memo(SummaryViewBase, areEqual);
export { memoizedSummaryView as SummaryView };
