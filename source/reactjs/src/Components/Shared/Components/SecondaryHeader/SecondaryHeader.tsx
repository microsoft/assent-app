import * as React from 'react';
import * as HeaderStyled from './SecondaryHeaderStyling';
import { useHistory } from 'react-router-dom';
import { Persona } from '../Persona';
import { PersonaSize } from '../Persona/Persona.types';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../../SharedComponents.reducer';
import { IComponentsAppState } from '../../SharedComponents.types';
import { sharedComponentsSagas } from '../../SharedComponents.sagas';
import { Reducer } from 'redux';
import {
    updateCardViewType,
    updateGroupedSummary,
    requestMyProfile,
    updateGroupedHistory,
    updateFilterValue,
    updateBulkSelected,
    requestMyHistory,
    requestMyDelegations,
    updateUserAlias,
    toggleTeachingBubbleVisibility,
    updateTeachingStep,
    updateBulkFailedStatus,
    updateSelectedSummarytoOutOfSync,
    updateSelectedSummarytoPending,
    updatePanelState,
    updateSelectedTenantDelegation,
    requestPullTenantSummary,
    receiveTenantDelegations,
} from '../../SharedComponents.actions';
import { setAliasMessagebarHeight } from '../../Details/Details.actions';
import { detailsInitialState, detailsReducerName } from '../../Details/Details.reducer';
import { IDetailsAppState } from '../../Details/Details.types';
import { Stack } from '@fluentui/react/lib/Stack';
import { IconButton, IButtonProps } from '@fluentui/react/lib/Button';
import { Toggle } from '@fluentui/react/lib/Toggle';
import { TooltipHost } from '@fluentui/react/lib/Tooltip';
import {
    getPanelOpen,
    getIsBulkSelected,
    getCardViewSelected,
    getBulkApproveFailed,
    getFilterValue,
    getHasError,
    getHistoryData,
    getHistoryGroupedBy,
    getHistorySearchCriteria,
    getHistoryTimePeriod,
    getIsLoading,
    getIsLoadingSummary,
    getProfile,
    getSelectedPage,
    getSelectedSummary,
    getSortColumnField,
    getSortDirection,
    getSummary,
    getSummaryCommonPropertiesSelector,
    getSummaryGroupedBy,
    getUserDelegations,
    getSelectedSummaryPage,
    getTenantDelegations,
    getSelectedTenantDelegation,
    getSuccessfulPullTenantCount,
    getDerivedTotalPullTenantCount,
} from '../../SharedComponents.selectors';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../../Helpers/telemetryHelpers';
import { IContextualMenuProps, DirectionalHint } from '@fluentui/react';
import {
    sharedComponentsPersistentReducerName,
    SharedComponentsPersistentInitialState,
    sharedComponentsPersistentReducer,
} from '../../SharedComponents.persistent-reducer';
import { usePersistentReducer } from '../PersistentReducer';
import { GroupingBy } from '../GroupingBy';
import { TeachingBubble } from '@fluentui/react/lib/TeachingBubble';
import { teachingSteps } from '../FeaturesIntroductionSteps';
import { MessageBar } from '@fluentui/react';
import {
    getTeachingBubbleStep,
    getTeachingBubbleVisibility,
    getUserAlias,
    getUserName,
} from '../../SharedComponents.persistent-selectors';
import { DATE_FORMAT_OPTION, DEFAULT_LOCALE } from '../../SharedConstants';

function SecondaryHeader(): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);

    const { useSelector, dispatch, authClient, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const isCardViewSelected = useSelector(getCardViewSelected);
    const bulkApproveFailed = useSelector(getBulkApproveFailed);
    const isBulkSelected = useSelector(getIsBulkSelected);
    const isPanelOpen = useSelector(getPanelOpen);
    const summary = useSelector(getSummary);
    const profile = useSelector(getProfile);
    const isLoading = useSelector(getIsLoading);
    const hasError = useSelector(getHasError);
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const historyGroupedBy = useSelector(getHistoryGroupedBy);
    const selectedPage = useSelector(getSelectedPage);
    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const historyData = useSelector(getHistoryData);
    const sortColumnField = useSelector(getSortColumnField);
    const sortDirection = useSelector(getSortDirection);
    const historySearchCriteria = useSelector(getHistorySearchCriteria);
    const historyTimePeriod = useSelector(getHistoryTimePeriod);
    const filterValue = useSelector(getFilterValue);
    const userDelegations = useSelector(getUserDelegations);
    const userAlias = useSelector(getUserAlias);
    const userName = useSelector(getUserName);
    const teachingBubbleVisibility = useSelector(getTeachingBubbleVisibility);
    const teachingBubbleStep = useSelector(getTeachingBubbleStep);
    const selectedSummary = useSelector(getSelectedSummaryPage);
    const selectedSummaryData = useSelector((state: IComponentsAppState) => getSelectedSummary(state));
    const derivedPullTenantCount = useSelector(getDerivedTotalPullTenantCount);
    const tenantDelegations = useSelector(getTenantDelegations);
    const selectedTenantDelegation = useSelector(getSelectedTenantDelegation);

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

    const { aliasMessagebarHeight } = useSelector(
        (state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState
    );

    const [filterMenuProps, setFilterMenuProps] = React.useState<IContextualMenuProps>(null);
    const [delegationMenuProps, setDelegationsMenuProps] = React.useState<IContextualMenuProps>(null);
    const [allHistory, setAllHistory] = React.useState<any>(null);
    const summaryCommonProperties = useSelector(getSummaryCommonPropertiesSelector);

    React.useEffect(() => {
        dispatch(requestMyProfile());
    }, [dispatch]);

    React.useEffect(() => {
        if (profile) {
            dispatch(requestMyDelegations(profile.userPrincipalName));
        }
    }, [profile]);

    React.useEffect(() => {
        if (historyData.length > 0) {
            setAllHistory(historyData);
        }
    }, [historyData]);

    React.useEffect(() => {
        getFilteredMenuItems();
    }, [allHistory, filterValue, historyGroupedBy, summaryGroupedBy, summary]);

    React.useEffect(() => {
        getDelegationMenuItems();
    }, [userDelegations, tenantDelegations, isCardViewSelected]);

    React.useEffect(() => {
        if (selectedPage !== 'summary' && profile) {
            // leave alias as null to know we are on default user
            dispatch(updateUserAlias('', profile.displayName));
        }

        if (selectedPage !== 'faq') {
            // reset grouping items
            getDelegationMenuItems();
            // reset filter menu items
            getFilteredMenuItems();
        }
    }, [selectedPage]);

    const failedCount = (data: any) => {
        let count = 0;
        for (const key in data) {
            if (data[key].LastFailed === true) {
                count++;
            }
        }
        return count;
    };

    const summaryGroupingTelemetry = (eventName: string, appAction: string, eventId: number): void => {
        const fullEventName = 'Group summary by ' + eventName;
        trackFeatureUsageEvent(authClient, telemetryClient, fullEventName, appAction, eventId, summaryCommonProperties);
    };

    const handleSummaryGrouping = (groupType: string): void => {
        dispatch(updateGroupedSummary(groupType));
        if (groupType === GroupingBy.Tenant) {
            summaryGroupingTelemetry('tenant', 'MSApprovals.GroupingbyTenant', TrackingEventId.GroupByTenant);
        } else if (groupType === GroupingBy.Submitter) {
            summaryGroupingTelemetry('submitter', 'MSApprovals.GroupingbySubmitter', TrackingEventId.GroupBySubmitter);
        } else if (groupType === GroupingBy.Date) {
            summaryGroupingTelemetry('date', 'MSApprovals.GroupingbyDate', TrackingEventId.GroupByDate);
        }
    };

    const viewTypeMenuProps: IContextualMenuProps = {
        items: [
            {
                key: 'card',
                text: 'Card',
                canCheck: true,
                isChecked: isCardViewSelected,
                onClick: (): void => {
                    if (!isCardViewSelected) {
                        setViewType(null, false);
                    }
                },
            },
            {
                key: 'table',
                text: 'Table',
                canCheck: true,
                isChecked: !isCardViewSelected,
                onClick: (): void => {
                    if (isCardViewSelected) {
                        setViewType(null, true);
                    }
                },
            },
        ],
    };

    const historyMenuProps: IContextualMenuProps = {
        items: [
            {
                key: 'Application',
                text: 'Application',
                canCheck: true,
                isChecked: true,
                onClick: () => {
                    dispatch(updateGroupedHistory(GroupingBy.Tenant));
                },
            },
        ],
        directionalHint: DirectionalHint.bottomRightEdge,
        directionalHintFixed: true,
    };

    const getFilteredMenuItems = (): void => {
        let filterMenuItems: string[] = ['All'];
        let appNameToTenantIdMapping: any = { All: '' };
        let allItems: any;
        let filterKey: string;

        // Select which Items we are filtering. Ex: Summary (Approval Requests) or History Records
        if (selectedPage === 'history') {
            filterKey = historyGroupedBy;
            allItems = allHistory;
            for (const key in allHistory) {
                appNameToTenantIdMapping[allHistory[key][filterKey]] = allHistory[key]['TenantId'];
            }
        } else {
            filterKey = summaryGroupedBy;
            allItems = summary;
        }

        let secondaryIndex: string;
        if (summaryGroupedBy === GroupingBy.Submitter && selectedPage == 'summary') {
            secondaryIndex = 'Name';
        }
        for (const key in allItems) {
            let filterValue = allItems[key][filterKey];
            if (secondaryIndex) {
                filterValue = filterValue[secondaryIndex];
            }
            if (!filterMenuItems.includes(filterValue)) {
                filterMenuItems.push(filterValue);
            }
        }

        // Sort Menu Items
        const allIndex = filterMenuItems.indexOf('All');
        if (allIndex > -1) {
            filterMenuItems.splice(allIndex, 1);
        }
        filterMenuItems.sort();
        if (filterKey === GroupingBy.Date) {
            // flip the sorting (descending)
            filterMenuItems.reverse();
            // format the dates
            filterMenuItems.forEach((date, index, dateArray) => {
                dateArray[index] = new Date(date).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION);
            });
        }
        filterMenuItems.unshift('All'); //have All always on top

        // Set Items
        let filterMenuProps: IContextualMenuProps = {
            items: [],
            directionalHint: DirectionalHint.bottomRightEdge,
            directionalHintFixed: true,
        };
        switch (selectedPage) {
            case 'history':
                filterMenuProps.items = filterMenuItems.map((x) => ({
                    ['key']: x,
                    ['text']: x,
                    ['title']: x,
                    ['canCheck']: true,
                    // can add the tenant icon using this
                    // ["iconProps"]: { iconName: 'Add' },
                    ['isChecked']: x === filterValue ? true : false,
                    ['onClick']: () => {
                        dispatch(
                            requestMyHistory(
                                1,
                                sortColumnField,
                                sortDirection,
                                historySearchCriteria,
                                historyTimePeriod,
                                appNameToTenantIdMapping[x]
                            )
                        );
                    },
                }));
            default:
                filterMenuProps.items = filterMenuItems.map((x) => ({
                    ['key']: x,
                    ['text']: x,
                    ['title']: x,
                    ['canCheck']: true,
                    // can add the tenant icon using this
                    // ["iconProps"]: { iconName: 'Add' },
                    ['isChecked']: x === filterValue ? true : false,
                    ['onClick']: () => {
                        dispatch(updateFilterValue(x));
                    },
                }));
        }
        setFilterMenuProps(filterMenuProps);
    };

    const summaryMenuProps: IContextualMenuProps = {
        items: [
            {
                key: 'tenant',
                text: 'Application',
                canCheck: true,
                isChecked: summaryGroupedBy === GroupingBy.Tenant ? true : false,
                onClick: () => handleSummaryGrouping(GroupingBy.Tenant),
            },
            {
                key: 'submitter',
                text: 'Submitter',
                canCheck: true,
                isChecked: summaryGroupedBy === GroupingBy.Submitter ? true : false,
                onClick: () => handleSummaryGrouping(GroupingBy.Submitter),
            },
            {
                key: 'date',
                text: 'Date',
                canCheck: true,
                isChecked: summaryGroupedBy === GroupingBy.Date ? true : false,
                onClick: () => handleSummaryGrouping(GroupingBy.Date),
            },
        ],
        directionalHint: DirectionalHint.bottomRightEdge,
        directionalHintFixed: true,
    };

    const mobileMenuProps: IContextualMenuProps = {
        items: [
            {
                key: 'Pending Approvals',
                text: 'Pending Approvals',
                canCheck: true,
                isChecked: selectedSummary == 'pending',
                onClick: () => getSummaryRecords('pending'),
            },
            {
                key: 'Out Of Sync',
                text: 'Out Of Sync',
                canCheck: true,
                isChecked: selectedSummary == 'outofsync',
                onClick: () => getSummaryRecords('outofsync'),
            },
        ],
        directionalHintFixed: true,
    };

    const failedRecords = failedCount(selectedSummaryData);
    const summaryCount = selectedSummaryData ? selectedSummaryData.length : 0;
    const requestedRecords = summaryCount + derivedPullTenantCount;

    const failedRecordsText = 'failed actions';
    const requestedRecordsText = 'requests pending';

    const items = [];

    if (userAlias && userName && selectedPage === 'summary') {
        items.push({
            key: `Working on behalf of ${userName}`,
            text: `Working on behalf of ${userName}`,
            title: `Working on behalf of ${userName}`,
        });
    } else {
        items.push({
            key: `${userAlias && userName ? userName : profile?.displayName} `,
            text: `${userAlias && userName ? userName : profile?.displayName} `,
            title: `${userAlias && userName ? userName : profile?.displayName} `,
        });
    }

    if (requestedRecords > 0) {
        items.push({
            key: `${requestedRecords} ${requestedRecordsText}`,
            text: `${requestedRecords} ${requestedRecordsText}`,
            title: `${requestedRecords} ${requestedRecordsText}`,
        });
    }
    if (failedRecords > 0) {
        items.push({
            key: `${failedRecords} ${failedRecordsText}`,
            text: `${failedRecords} ${failedRecordsText}`,
            title: `${failedRecords} ${failedRecordsText}`,
        });
    }

    const displayPendingItemsButton = requestedRecords > 0 || failedRecords > 0;
    const pendingMenuProps: IContextualMenuProps = {
        items: [
            {
                key: 'requestedRecordsText',
                text: requestedRecords + ' ' + requestedRecordsText,
            },
            {
                key: 'failedRecordsText',
                text: failedRecords + ' ' + failedRecordsText,
            },
        ],
        directionalHint: DirectionalHint.bottomRightEdge,
        directionalHintFixed: true,
    };

    const mobileMenus: IContextualMenuProps = {
        items,
        directionalHintFixed: true,
    };

    let menuProps: IContextualMenuProps;
    switch (selectedPage) {
        case 'history':
            menuProps = historyMenuProps;
            break;
        default:
            menuProps = summaryMenuProps;
    }

    const setViewType = (ev: React.FormEvent<HTMLElement>, checked: boolean) => {
        dispatch(updateCardViewType(!checked));
        if (bulkApproveFailed) {
            dispatch(updateBulkFailedStatus(false));
        }
        if (tenantDelegations && !checked && filterValue === tenantDelegations.appName && selectedTenantDelegation) {
            dispatch(updateSelectedTenantDelegation(null));
        }

        if (checked) {
            trackFeatureUsageEvent(
                authClient,
                telemetryClient,
                'ViewType',
                'MSApprovals.TableView',
                TrackingEventId.TableView,
                summaryCommonProperties
            );
        } else {
            trackFeatureUsageEvent(
                authClient,
                telemetryClient,
                'ViewType',
                'MSApprovals.CardView',
                TrackingEventId.CardView,
                summaryCommonProperties
            );
        }
    };

    const getSummaryRecords = (summaryType: string) => {
        if (summaryType === 'pending') {
            dispatch(updateSelectedSummarytoPending());
        } else if (summaryType === 'outofsync') {
            dispatch(updateSelectedSummarytoOutOfSync());
        }
    };
    let history = useHistory();

    const getPendingRecords = (ev: React.FormEvent<HTMLElement>, checked: boolean): void => {
        if (history.push) {
            var pendingApprovalPagePath = checked ? `/outofsync` : `/`;
            history.push(pendingApprovalPagePath);
            dispatch(updatePanelState(false));
        }
        if (checked) {
            dispatch(updateSelectedSummarytoOutOfSync());
        } else if (!checked) {
            dispatch(updateSelectedSummarytoPending());
        }
    };

    function getDelegationMenuItems(): void {
        if ((!userDelegations || userDelegations.length === 0) && !tenantDelegations) {
            const delegationMenuProps: IContextualMenuProps = {
                items: [],
                directionalHintFixed: true,
            };
            setDelegationsMenuProps(delegationMenuProps);
            return;
        }
        if (userDelegations.filter((u: any) => u.name === 'Me').length <= 0) {
            userDelegations.unshift({ name: 'Me', alias: '' });
        }
        const delegationMenuProps: IContextualMenuProps = {
            items: userDelegations.map((x: any) => ({
                ['key']: x.name,
                ['text']: x.name,
                ['title']: x.name,
                ['onClick']: () => {
                    dispatch(updateUserAlias(x['alias'], x['name']));
                    dispatch(updateFilterValue('All'));
                    dispatch(updateBulkSelected(false));
                    dispatch(setAliasMessagebarHeight(0));
                    dispatch(receiveTenantDelegations(null));
                },
            })),
            directionalHintFixed: true,
        };
        const allMenuProps = delegationMenuProps;
        if (tenantDelegations && !isCardViewSelected && filterValue === tenantDelegations.appName) {
            const tenantDelegationMenuItems = tenantDelegations.delegations?.map((x: any) => ({
                ['key']: x.name,
                ['text']: x.name,
                ['title']: x.name,
                ['onClick']: () => {
                    dispatch(updateSelectedTenantDelegation(x));
                    dispatch(requestPullTenantSummary(tenantDelegations.tenantId, x.alias, null, true));
                },
            }));
            allMenuProps.items = allMenuProps.items.concat(tenantDelegationMenuItems);
        }
        setDelegationsMenuProps(allMenuProps);
    }

    function updateTeachingBubbleStep(nextKey: number) {
        if (nextKey < 0) {
            dispatch(toggleTeachingBubbleVisibility());
            return;
        }
        const newStep = teachingSteps.find((teachingStep) => teachingStep?.step === nextKey);
        if (!newStep) {
            dispatch(toggleTeachingBubbleVisibility());
            return;
        }
        dispatch(updateTeachingStep(newStep));
    }

    const teachingStepSuccessButtonProps: IButtonProps = {
        children: teachingBubbleStep?.successButtonLabel,
        onClick: () => updateTeachingBubbleStep(teachingBubbleStep.successNextStep),
    };

    const teachingStepDeclineButtonProps: IButtonProps = React.useMemo(
        () => ({
            children: teachingBubbleStep?.declineButtonLabel,
            onClick: () => updateTeachingBubbleStep(teachingBubbleStep.declineNextStep),
        }),
        []
    );

    function renderPersona(): any {
        const emailAlias =
            tenantDelegations?.appName === filterValue && selectedTenantDelegation && selectedPage === 'summary'
                ? selectedTenantDelegation.alias + `${__UPN_SUFFIX__}`
                : userAlias && userName && selectedPage === 'summary'
                ? userAlias + `${__UPN_SUFFIX__}`
                : profile?.userPrincipalName;
        const personaText =
            tenantDelegations?.appName === filterValue && selectedTenantDelegation && selectedPage === 'summary'
                ? selectedTenantDelegation.name
                : userAlias && userName && selectedPage === 'summary'
                ? userName
                : profile?.displayName;
        return (
            <div>
                <Stack.Item>
                    <Persona
                        // use dyanamic domain in the future
                        emailAlias={emailAlias}
                        size={dimensions.width > 639 ? PersonaSize.size40 : PersonaSize.size16}
                        text={personaText}
                        optionalText={profile?.officeLocation}
                        className="loggedin-user-name"
                        styles={HeaderStyled.PersonaMobileStyling}
                    />
                </Stack.Item>
                {/* <Stack.Item align="center" className={'ms-hiddenLgUp'}>
                    <Persona
                        // use dyanamic domain in the future
                        emailAlias={emailAlias}
                        size={PersonaSize.size40}
                        styles={HeaderStyled.PersonaMobileStyling}
                    />
                </Stack.Item> */}
            </div>
        );
    }

    const setAliasMessageBarRef = (element: any): void => {
        const aliasMessageElement = element;
        if (summary && summary.length > 0 && aliasMessageElement && aliasMessageElement.clientHeight) {
            const aliasMessageElementHeight = aliasMessageElement.clientHeight - 3;
            if (aliasMessagebarHeight != aliasMessageElementHeight) {
                dispatch(setAliasMessagebarHeight(aliasMessageElementHeight));
            }
        }
    };

    const onBehalfOfText =
        tenantDelegations?.appName === filterValue && selectedTenantDelegation && selectedPage === 'summary'
            ? selectedTenantDelegation.name
            : userAlias && userName && selectedPage === 'summary'
            ? userName
            : null;

    return (
        <div>
            <HeaderStyled.SecondaryHeaderContainer>
                <Stack horizontal styles={HeaderStyled.SecondaryHeaderStackStyles(isPanelOpen)}>
                    <Stack.Item align="center" grow={1} styles={HeaderStyled.PersonaDropdownStyles}>
                        {/* user persona/name including delegation */}
                        <Stack horizontal style={{ backgroundColor: 'inherit' }}>
                            {!isLoading &&
                            !hasError &&
                            profile &&
                            selectedPage === 'summary' &&
                            delegationMenuProps?.items.length > 1 ? (
                                // load delegation dropdown
                                <TooltipHost content={userName}>
                                    <Stack.Item styles={HeaderStyled.SecondaryHeaderDelegationIconStyling}>
                                        <IconButton
                                            title={userName}
                                            ariaLabel={userName}
                                            style={HeaderStyled.SecondaryHeaderIconStyling}
                                            menuProps={delegationMenuProps}
                                        >
                                            {renderPersona()}
                                        </IconButton>
                                    </Stack.Item>
                                </TooltipHost>
                            ) : (
                                // just load the persona with their name
                                <Stack>{renderPersona()}</Stack>
                            )}
                        </Stack>
                    </Stack.Item>
                    {selectedPage === 'summary' && !isLoadingSummary && (
                        // requested records and failed records
                        <>
                            <Stack.Item align="center" grow={1}>
                                <Stack>
                                    {displayPendingItemsButton && (
                                        // hidden when medium and down - "requested count"
                                        <Stack.Item align="center" className={'hidden-LgUpCustom'}>
                                            <Stack horizontalAlign="end" horizontal>
                                                <TooltipHost content="Summary">
                                                    <IconButton
                                                        menuIconProps={{ iconName: 'More' }}
                                                        title="Summary"
                                                        ariaLabel="Status"
                                                        style={HeaderStyled.SecondaryHeaderIconStyling}
                                                        menuProps={pendingMenuProps}
                                                    />
                                                </TooltipHost>
                                            </Stack>
                                        </Stack.Item>
                                    )}
                                </Stack>
                            </Stack.Item>
                        </>
                    )}
                    {selectedPage === 'summary' && !isLoadingSummary && (
                        // requested records and failed records
                        <>
                            <Stack.Item align="center" grow={1}>
                                <Stack>
                                    {requestedRecords > 0 && (
                                        // hidden when medium and down - "requested count"
                                        <Stack.Item
                                            grow={1}
                                            verticalFill
                                            align="center"
                                            styles={HeaderStyled.SummaryCountStyling}
                                            className={' ms-hiddenMdDown'}
                                        >
                                            <Stack horizontal>
                                                <HeaderStyled.SummaryCountText>
                                                    {requestedRecords}
                                                </HeaderStyled.SummaryCountText>{' '}
                                                <HeaderStyled.SummaryCountLabelText>
                                                    {requestedRecordsText}
                                                </HeaderStyled.SummaryCountLabelText>
                                            </Stack>
                                        </Stack.Item>
                                    )}
                                    {failedRecords > 0 && (
                                        // hidden when medium and down - "failed count"
                                        <Stack.Item
                                            grow={1}
                                            align="center"
                                            styles={HeaderStyled.SummaryCountStyling}
                                            className={' ms-hiddenMdDown'}
                                        >
                                            <Stack horizontal>
                                                <HeaderStyled.FailedCountText>
                                                    {failedCount(selectedSummaryData)}
                                                </HeaderStyled.FailedCountText>{' '}
                                                <HeaderStyled.FailedCountLabelText>
                                                    {failedRecordsText}
                                                </HeaderStyled.FailedCountLabelText>
                                            </Stack>
                                        </Stack.Item>
                                    )}
                                </Stack>
                            </Stack.Item>
                        </>
                    )}
                    {/* hide toggle for now*/}
                    {false && (
                        <Stack.Item grow={20} align="center" className={' ms-hiddenMdDown'}>
                            <Stack horizontalAlign="end" horizontal>
                                <Toggle
                                    inlineLabel
                                    checked={selectedSummary == 'outofsync'}
                                    label="Pending Approvals"
                                    onText="Out Of Sync"
                                    offText="Out Of Sync"
                                    onChange={getPendingRecords}
                                />
                            </Stack>
                        </Stack.Item>
                    )}
                    {/* hidden when large and up - "more" button*/}
                    {false && (
                        <Stack.Item align="center" className={'ms-hiddenLgUp'}>
                            <Stack horizontalAlign="end" horizontal>
                                <TooltipHost content="Summary">
                                    <IconButton
                                        menuIconProps={{ iconName: 'More' }}
                                        title="Summary"
                                        ariaLabel="Status"
                                        style={HeaderStyled.SecondaryHeaderIconStyling}
                                        menuProps={mobileMenuProps}
                                    />
                                </TooltipHost>
                            </Stack>
                        </Stack.Item>
                    )}
                    {/* stack item for the group by and filtering icons */}
                    <Stack.Item grow={20} align="center" styles={HeaderStyled.GroupAndFilterIconStackItemStyles}>
                        <Stack horizontalAlign="end" horizontal>
                            {selectedPage == 'summary' && (
                                <>
                                    <Toggle
                                        inlineLabel
                                        label="View Type"
                                        styles={HeaderStyled.ToggleStackStyles}
                                        offText="Card"
                                        onText="Table"
                                        role="switch"
                                        checked={!isCardViewSelected}
                                        onChange={setViewType}
                                        className={'ms-hiddenMdDown'}
                                    />
                                    <IconButton
                                        iconProps={{ iconName: 'View' }}
                                        className={'hidden-LgUpCustom'}
                                        ariaLabel="Change view type"
                                        menuProps={viewTypeMenuProps}
                                        style={{
                                            ...HeaderStyled.SecondaryHeaderIconStyling,
                                            ...{ backgroundColor: 'inherit' },
                                        }}
                                    />
                                </>
                            )}
                            {selectedPage != 'faq' && (
                                <>
                                    <TooltipHost
                                        style={HeaderStyled.SecondaryHeaderIconStyling}
                                        className={isBulkSelected ? 'ms-hiddenMdDown' : null}
                                        content="Group By"
                                    >
                                        {/* Always display Group by icon(even when disabled) */}
                                        <IconButton
                                            id="groupBy"
                                            iconProps={{ iconName: 'GroupList' }}
                                            ariaLabel="Group By"
                                            style={HeaderStyled.SecondaryHeaderIconStyling}
                                            menuProps={menuProps}
                                            disabled={isBulkSelected && selectedPage === 'summary'}
                                        />
                                    </TooltipHost>
                                    <TooltipHost content="Filter" className={isBulkSelected ? 'ms-hiddenMdDown' : null}>
                                        {/* Always display Filter icon(even when disabled) */}
                                        <IconButton
                                            id="filter"
                                            iconProps={{ iconName: filterValue === 'All' ? 'Filter' : 'FilterSolid' }}
                                            ariaLabel="Filter"
                                            style={HeaderStyled.SecondaryHeaderIconStyling}
                                            menuProps={filterMenuProps}
                                            disabled={isBulkSelected && selectedPage === 'summary'}
                                        />
                                    </TooltipHost>
                                </>
                            )}
                        </Stack>
                    </Stack.Item>
                    {teachingBubbleVisibility && teachingBubbleStep && (
                        <TeachingBubble
                            target={teachingBubbleStep.target}
                            hasCondensedHeadline={true}
                            primaryButtonProps={
                                teachingBubbleStep.successButtonLabel ? teachingStepSuccessButtonProps : null
                            }
                            secondaryButtonProps={
                                teachingBubbleStep.declineButtonLabel ? teachingStepDeclineButtonProps : null
                            }
                            onDismiss={() => {}}
                            headline={teachingBubbleStep.headline}
                        />
                    )}
                </Stack>
            </HeaderStyled.SecondaryHeaderContainer>
            {/* delegation available info banner */}
            {selectedPage === 'summary' &&
                !userAlias &&
                !isLoadingSummary &&
                !userDelegations &&
                userDelegations?.length === 0 && (
                    <TooltipHost
                        content={'Delegation is available. Select user from dropdown to switch.'}
                        aria-label="Delegation available message"
                    >
                        <MessageBar
                            styles={HeaderStyled.DelegationBarStyles}
                            isMultiline={false}
                            aria-label={'Informational message'}
                        >
                            Delegation is available. Select user from dropdown to switch.
                        </MessageBar>
                    </TooltipHost>
                )}
            {/* working on behalf of banner */}
            {onBehalfOfText && (
                <HeaderStyled.BulkMessageHeight ref={(element) => setAliasMessageBarRef(element)}>
                    <MessageBar
                        styles={HeaderStyled.DelegationBarStyles}
                        isMultiline={false}
                        aria-label={'Informational message'}
                    >
                        <Stack.Item>
                            <HeaderStyled.MessageBarText>
                                Working on behalf of {onBehalfOfText}
                            </HeaderStyled.MessageBarText>
                        </Stack.Item>
                    </MessageBar>
                </HeaderStyled.BulkMessageHeight>
            )}
        </div>
    );
}

const connected = React.memo(SecondaryHeader);
export { connected as SecondaryHeader };
