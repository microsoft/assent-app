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
} from '../Shared/SharedComponents.actions';
import { isEqual } from 'lodash';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { SummaryTable } from './SummaryTable/SummaryTable';
import ErrorView from '../Shared/Details/DetailsMessageBars/ErrorView';
import { ContinueMessage } from '../Shared/Components/ContinueMessage';
import { CONTINUE_TIMEOUT } from '../Shared/SharedConstants';
import { Dialog, Dropdown, IDropdownOption, MessageBar, MessageBarType } from '@fluentui/react';
import PullTenantSummaryCountBanner from './PullTenantSummaryCountBanner';
import { breakpointMap } from '../Shared/Styles/Media';

interface ISummaryViewProps {
    windowHeight: number;
    windowWidth: number;
}

function SummaryViewBase(props: ISummaryViewProps): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { windowHeight, windowWidth } = props;

    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const tenantInfo = useSelector(getTenantInfo);
    const filterValue = useSelector(getFilterValue);
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const groupedSummaryData: any = useSelector((state: IComponentsAppState) => groupedSummaryDataSelector(state));
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
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

    const stackTokens: IStackTokens =
        windowWidth <= breakpointMap.m
            ? { childrenGap: 0, padding: '0px 5px 10px 0px' }
            : { childrenGap: 5, padding: '0px 5px 10px 0px' };
    const [bulkSelectedRecordsLength, setBulkSelectedRecordsLength] = React.useState(Number);
    const [checked, setChecked] = React.useState(Boolean);
    const [showContinue, setShowContinue] = React.useState(false);

    const tenantIdforFilterValue = useSelector((state: any) => getTenantIdFromAppName(state, filterValue));
    const pullTenantSummaryDataMemo = useSelector(getPullTenantSummaryMemoized);

    const isExternalActionDetails = filteredTenantInfo?.isExternalTenantActionDetails;

    const summaryCount = groupedSummaryData ? groupedSummaryData.length : 0;
    const pullTenantCount = typeof totalPullTenantCount === 'number' ? totalPullTenantCount : 0;
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
        if (isBulkSelected) {
            const organizedSummaryData = filterSummary(groupedSummaryData);
            if (organizedSummaryData && organizedSummaryData.length == 0) {
                dispatch(updateFilterValue('All'));
                dispatch(updateBulkSelected(false));
            }
        }
    }, [groupedSummaryData]);

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

    const onCheckboxChange = (ev: React.FormEvent<HTMLElement>, checked: boolean): void => {
        setChecked(checked);
        if (checked) {
            setBulkSelectedRecordsLength(bulkActionConcurrentCall);
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
        return (
            tenantGroup.grouping.filter(
                (s) => s.IsRead || readRequests.includes(s.DocumentNumber) || !s.IsControlsAndComplianceRequired
            ).length > 0
        );
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
        switch (summaryGroupedBy) {
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
                                                                label=""
                                                                checked={checked}
                                                                title="Select All Bulk Records"
                                                                ariaLabel="Select All Bulk Records"
                                                                disabled={!(tenantInfo?.length > 0)}
                                                                onChange={onCheckboxChange}
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
                                                    <h4>{tenantGroup.displayValue}</h4>
                                                </SummaryStyled.TenantLabel>
                                            </SummaryStyled.CardGroupLabel>
                                        </Text>
                                    </Stack.Item>
                                </Stack>
                            </Stack.Item>
                            <Stack.Item />
                            <Stack.Item>
                                {tenantGroup?.isPullModelEnabled &&
                                    !(tenantGroup?.grouping?.length > 0) &&
                                    !pullTenantSearchCriteria &&
                                    !pullTenantSummaryHasError && (
                                        <SharedStyled.CenterHeightSpace>
                                            <EmptyResults message="There are no pending approvals for you." />
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
                                        isSingleGroupShown={isSingleGroupShown}
                                    />
                                )}
                            </Stack.Item>
                        </Stack>
                    );
                });
            default:
                return (
                    <SummaryCards
                        summary={organizedSummaryData}
                        bulkSelectedRecordsLength={bulkSelectedRecordsLength}
                    ></SummaryCards>
                );
        }
    }

    const onDismissHandler = (): void => {
        dispatch(updateBulkFailedStatus(false));
    };

    const numBulkFailures = bulkApproveFailed && bulkFailedMsg && bulkFailedMsg.length > 0 ? bulkFailedMsg.length : 0;
    const bulkFailureMessageOffset = numBulkFailures > 0 ? numBulkFailures * 20 + 60 : 0;

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
                    <SummaryStyled.SummaryLayoutContainer
                        className="scroll-hidden v-scroll-auto custom-scrollbar"
                        windowHeight={windowHeight}
                        windowWidth={windowWidth}
                        selectedApprovalRecords={selectedApprovalRecords}
                        footerHeight={bulkFooterHeight}
                        bulkMessagebarHeight={bulkMessagebarHeight}
                        aliasMessagebarHeight={aliasMessagebarHeight}
                        bulkFailureMessageOffset={bulkFailureMessageOffset}
                    >
                        {!isLoadingSummary &&
                            groupedSummaryData &&
                            totalRequestCount > 0 &&
                            !(isPullTenantSelected && isLoadingPullTenantData) && (
                                <SummaryStyled.SummaryPageTitle isTableView={!isCardViewSelected}>
                                    Pending Approvals
                                </SummaryStyled.SummaryPageTitle>
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
                            totalRequestCount == 0 && <EmptyResults message="No Pending Requests" />}
                        {!isLoadingSummary &&
                            filterValue === 'All' &&
                            !isPanelOpen &&
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
                            renderSummary()}
                    </SummaryStyled.SummaryLayoutContainer>
                </Stack.Item>
            }
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
