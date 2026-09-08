import * as React from 'react';
import * as HeaderStyled from './SecondaryHeaderStyling';
import { useHistory } from 'react-router-dom';
import { Persona } from '../Persona';
import { PersonaSize } from '../Persona/Persona.types';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { sharedComponentsReducerName, sharedComponentsReducer, sharedComponentsInitialState } from '../../SharedComponents.reducer';
import { NavigationUtils } from '../../Utils/NavigationUtils';
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
    requestMySummary,
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
    initiateSearch,
    requestDownloadSummary,
    clearDownloadSummaryError,
} from '../../SharedComponents.actions';
import { setAliasMessagebarHeight } from '../../Details/Details.actions';
import { detailsInitialState, detailsReducerName } from '../../Details/Details.reducer';
import { IDetailsAppState } from '../../Details/Details.types';
import { Stack } from '@fluentui/react/lib/Stack';
import { IconButton, ActionButton, IButtonProps } from '@fluentui/react/lib/Button';
import { Spinner } from '@fluentui/react/lib/Spinner';
import { TooltipHost } from '@fluentui/react/lib/Tooltip';
import {
    getPanelOpen,
    getIsBulkSelected,
    getCardViewSelected,
    getBulkApproveFailed,
    getFilterValue,
    getHasError,
    getHistoryData,
    getTenantList,
    getTenantInfo,
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
    getIsSearchResultsViewOpen,
    getIsSubmitterView,
    getIsDownloadingSummary,
    getSummaryDownloadHasError,
    getIsPullTenantSelected,
    getTenantIdFromAppName,
    getSelectedDelegationDetails,
} from '../../SharedComponents.selectors';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../../Helpers/telemetryHelpers';
import { computeDelegationExpiryDetails, getDelegationExpiryMessage } from '../../../../Helpers/delegationExpiry';
import {
    IContextualMenuProps,
    IContextualMenuItem,
    DirectionalHint,
    Pivot,
    PivotItem,
    Callout,
    Icon,
} from '@fluentui/react';
import { Text } from '@fluentui/react/lib/Text';
import { useBoolean } from '@fluentui/react-hooks';
import FlightingHandler, { useFlighting, FeatureCoachmark } from '../FlightingHandler';
import ExportSummaryCallout from './ExportSummaryCallout';
import {
    sharedComponentsPersistentReducerName,
    SharedComponentsPersistentInitialState,
    sharedComponentsPersistentReducer,
} from '../../SharedComponents.persistent-reducer';
import { usePersistentReducer } from '../PersistentReducer';
import { GroupingBy } from '../GroupingBy';
import { TeachingBubble } from '@fluentui/react/lib/TeachingBubble';
import { teachingSteps } from '../FeaturesIntroductionSteps';
import { MessageBar, MessageBarType } from '@fluentui/react';
import {
    getTeachingBubbleStep,
    getTeachingBubbleVisibility,
    getUserAlias,
    getUserName,
    getPersistedVisibleColumns,
} from '../../SharedComponents.persistent-selectors';
import {
    DATE_FORMAT_OPTION,
    DEFAULT_LOCALE,
    COLUMN_DISPLAY_NAMES,
    DEFAULT_VISIBLE_COLUMNS,
} from '../../SharedConstants';
import {
    SUMMARY_EXPORT_COACHMARK_CONTENT_AFTER_ICON,
    SUMMARY_EXPORT_COACHMARK_CONTENT_BEFORE_ICON,
    SUMMARY_EXPORT_COACHMARK_CONTENT_SECONDARY,
    SUMMARY_EXPORT_COACHMARK_HEADLINE,
    SUMMARY_EXPORT_COLUMN_FORMATS,
    SUMMARY_EXPORT_EXCLUDED_COLUMNS,
    SUMMARY_EXPORT_HEADER_OVERRIDES,
    SummaryExportColumnFormat,
} from '../../../Summary/SummaryExport.constants';
import { AppName } from '../../../Summary/SummaryCards/SummaryCard.styled';

// Breakpoints (px) at which secondary header elements collapse into the
// overflow "More" dropdown. Lower-priority items collapse first (at wider
// widths), higher-priority items only collapse at narrower widths.
const OVERFLOW_BREAKPOINTS = {
    COLLAPSE_COUNTS: 850,
    COLLAPSE_VIEW_TYPE: 750,
    COLLAPSE_SUBMITTER: 640,
    COLLAPSE_GROUP_FILTER: 320,
};

// Module-level dedup so the telemetry fires at most once per (logged-in user,
// delegator) pair per browser/page session. Module scope survives SecondaryHeader
// remounts (e.g., route changes) and resets only on full page reload. Keying on
// both identities prevents suppressing the event after an in-app account switch
// where a different user is delegated by the same person.
const firedDelegationExpiryWarnings = new Set<string>();

function SecondaryHeader(): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);
    const sentPivotRef = React.useRef<HTMLSpanElement | null>(null);

    let history = useHistory();

    // Check if we're in dashboard view
    const isDashboardView = history?.location?.pathname === '/dashboard';

    const { useSelector, dispatch, authClient, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const isCardViewSelected = useSelector(getCardViewSelected);
    const bulkApproveFailed = useSelector(getBulkApproveFailed);
    const isBulkSelected = useSelector(getIsBulkSelected);
    const isPanelOpen = useSelector(getPanelOpen);
    const summary = useSelector(getSummary);
    const tenantInfo = useSelector(getTenantInfo);
    const profile = useSelector(getProfile);
    const isLoading = useSelector(getIsLoading);
    const hasError = useSelector(getHasError);
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const historyGroupedBy = useSelector(getHistoryGroupedBy);
    const selectedPage = useSelector(getSelectedPage);
    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const historyData = useSelector(getHistoryData);
    const tenantList = useSelector(getTenantList);
    const sortColumnField = useSelector(getSortColumnField);
    const sortDirection = useSelector(getSortDirection);
    const historySearchCriteria = useSelector(getHistorySearchCriteria);
    const historyTimePeriod = useSelector(getHistoryTimePeriod);
    const filterValue = useSelector(getFilterValue);
    const userDelegations = useSelector(getUserDelegations);
    const selectedDelegationEntry = useSelector(getSelectedDelegationDetails);
    const userAlias = useSelector(getUserAlias);
    const visibleColumns = useSelector((state: any) => getPersistedVisibleColumns(state, 'all'));
    const userName = useSelector(getUserName);
    const teachingBubbleVisibility = useSelector(getTeachingBubbleVisibility);
    const teachingBubbleStep = useSelector(getTeachingBubbleStep);
    const selectedSummary = useSelector(getSelectedSummaryPage);
    const selectedSummaryData = useSelector((state: IComponentsAppState) => getSelectedSummary(state));
    const derivedPullTenantCount = useSelector(getDerivedTotalPullTenantCount);
    const tenantDelegations = useSelector(getTenantDelegations);
    const selectedTenantDelegation = useSelector(getSelectedTenantDelegation);
    const isSearchResultsViewOpen = useSelector(getIsSearchResultsViewOpen);
    const isSubmitterView = useSelector(getIsSubmitterView);
    const isDownloadingSummary = useSelector(getIsDownloadingSummary);
    const summaryDownloadHasError = useSelector(getSummaryDownloadHasError);
    const isFilterPullTenant = useSelector(getIsPullTenantSelected);
    const filterTenantId = useSelector((state: any) => getTenantIdFromAppName(state, filterValue));

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

    // Determine if submitter view is flighted on for this user
    const isSubmitterViewFlighted = useFlighting('Submitter View');
    // Determine if the summary export feature is flighted on for this user
    const isSummaryExportFlighted = useFlighting('SummaryExport');

    // Overflow state: determine which secondary header items to show inline
    // vs. collapse into the overflow "More" dropdown based on viewport width.
    const showCountsInline = dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_COUNTS;
    // When counts are the only collapsed element (750-849px), show them as compact
    // number badges with a callout instead of putting them in a "More" dropdown.
    const showCountsCompactInline = !showCountsInline && dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_VIEW_TYPE;
    const showSubmitterPivotInline = dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_COUNTS;
    const showSubmitterCompactInline = !showSubmitterPivotInline && dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_SUBMITTER;
    const showViewTypeInline = dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_VIEW_TYPE;
    const showGroupFilterInline = dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_GROUP_FILTER;

    const { aliasMessagebarHeight } = useSelector(
        (state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState
    );

    const [filterMenuProps, setFilterMenuProps] = React.useState<IContextualMenuProps>(null);
    const [delegationMenuProps, setDelegationsMenuProps] = React.useState<IContextualMenuProps>(null);
    const [allHistory, setAllHistory] = React.useState<any>(null);
    const [isExportDialogOpen, { setTrue: openExportDialog, setFalse: closeExportDialog }] = useBoolean(false);
    const [exportAll, setExportAll] = React.useState(false);
    const summaryCommonProperties = useSelector(getSummaryCommonPropertiesSelector);
    
    // Reference to track the last requested count to detect significant changes
    const lastRequestedCountRef = React.useRef(0);
    // Reference to track the last summary data for change detection
    const lastSummaryDataRef = React.useRef<string>(JSON.stringify(selectedSummaryData || []));

    React.useEffect(() => {
        dispatch(requestMyProfile());
    }, [dispatch]);

    // Auto-dismiss the export failure toast after a short delay so it behaves like a transient notification.
    React.useEffect(() => {
        if (summaryDownloadHasError) {
            const timer = setTimeout(() => dispatch(clearDownloadSummaryError()), 5000);
            return () => clearTimeout(timer);
        }
    }, [summaryDownloadHasError, dispatch]);

    // While a download is in progress the export button is swapped for a spinner, which drops keyboard focus to
    // <body>. Once the download settles, restore focus to the export button so keyboard/screen-reader users keep place.
    const wasDownloadingSummaryRef = React.useRef(false);
    React.useEffect(() => {
        if (wasDownloadingSummaryRef.current && !isDownloadingSummary) {
            document.getElementById('exportPendingApprovals')?.focus();
        }
        wasDownloadingSummaryRef.current = isDownloadingSummary;
    }, [isDownloadingSummary]);

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

        // Deep-link (?alias=) UX allowlist only — NOT a security boundary; backend must re-validate the delegation grant.
        const urlSearch = new URLSearchParams(history.location.search);
        const requestedAlias = (urlSearch.get('alias') || '').trim().toLowerCase();

        // Auto-apply only after delegations load and the alias matches a server-granted delegation.
        if (requestedAlias && userDelegations && userDelegations.length > 0) {
            const matchedDelegation = userDelegations.find((delegation: any) => {
                const delegationAlias = (delegation?.delegator?.UserPrincipalName?.split('@')[0] || '')
                    .trim()
                    .toLowerCase();
                return delegationAlias !== '' && delegationAlias === requestedAlias;
            });

            if (matchedDelegation) {
                delegationOnClick(
                    matchedDelegation.delegator.UserPrincipalName.split('@')[0],
                    matchedDelegation.delegator.DisplayName,
                    matchedDelegation.delegator.UserPrincipalName,
                    matchedDelegation.delegator.Id
                );
            }
        }
    }, [userDelegations, tenantDelegations, isCardViewSelected]);

    React.useEffect(() => {
        if (selectedPage !== 'summary' && profile) {
            // leave alias as null to know we are on default user
            dispatch(updateUserAlias('', profile.displayName, '', ''));
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
            allItems = tenantList;
            for (const key in tenantList) {
                appNameToTenantIdMapping[tenantList[key][filterKey]] = tenantList[key]['TenantId'];
            }
        } else {
            filterKey = summaryGroupedBy;
            allItems = summary;
        }

        let secondaryIndex: string;
        if (summaryGroupedBy === GroupingBy.Submitter && selectedPage == 'summary') {
            secondaryIndex = 'Name';
        }

        // When grouping by category, resolve each request's category from tenant metadata
        const tenantCategoryMap = new Map<number, string>();
        if (summaryGroupedBy === GroupingBy.Category && selectedPage === 'summary' && tenantInfo) {
            tenantInfo.forEach((tenant: any) => {
                tenantCategoryMap.set(tenant.tenantId, tenant.tenantCategory || 'Others');
            });
        }
        for (const key in allItems) {
            let filterValue = allItems[key][filterKey];
            if (secondaryIndex) {
                filterValue = filterValue[secondaryIndex];
            }
            if (summaryGroupedBy === GroupingBy.Category && selectedPage === 'summary') {
                filterValue = tenantCategoryMap.get(allItems[key]['TenantId']) || 'Others';
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
        filterMenuProps.items = filterMenuItems.map((x) => ({
            ['key']: x,
            ['text']: x,
            ['title']: x,
            ['canCheck']: true,
            // can add the tenant icon using this
            // ["iconProps"]: { iconName: 'Add' },
            ['isChecked']: x === filterValue ? true : false,
            ['onClick']: () => {
            switch (selectedPage) {
                case 'history':
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
                default:
                    dispatch(updateFilterValue(x));
            }
        },
        }));
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
            {
                key: 'category',
                text: 'Category',
                canCheck: true,
                isChecked: summaryGroupedBy === GroupingBy.Category ? true : false,
                onClick: () => handleSummaryGrouping(GroupingBy.Category),
            },
        ].filter((item) => !(isSubmitterView && item.key === 'submitter')),
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
    // Exclude approver-only pull-tenant requests in submitter view to match SummaryView's empty list.
    const requestedRecords = summaryCount + (isSubmitterView ? 0 : derivedPullTenantCount);
    
    React.useEffect(() => {
        if (isSearchResultsViewOpen) {
            const urlSearch = new URLSearchParams(history.location.search);
            const filterParam = urlSearch.get('filter');
            if (filterParam && selectedSummaryData) {
                dispatch(initiateSearch(userAlias, filterParam));
            }
        }
    }, [summaryCount, derivedPullTenantCount, requestedRecords, userAlias, isSearchResultsViewOpen, dispatch, selectedSummaryData, history]);

    const failedRecordsText = 'failed action(s)';
    const requestedRecordsText = isSubmitterView ? 'request(s) submitted' : 'request(s) pending';

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

    const getPendingRecords = (ev: React.FormEvent<HTMLElement>, checked: boolean): void => {
        if (history.push) {
            const currentParams = NavigationUtils.getCurrentUrlParams(history);
            const filterParam = currentParams.get('filter');
            const isDashboardRoute = NavigationUtils.isDashboardRoute(history);

            const customPath = checked 
                ? isDashboardRoute
                    ? `/dashboard/outofsync`
                    : `/outofsync`
                : isDashboardRoute
                    ? `/dashboard`
                    : `/`;

            const finalPath = filterParam 
                ? `${customPath}?filter=${encodeURIComponent(filterParam)}`
                : customPath;
            
            history.push(finalPath);
            dispatch(updatePanelState(false));
        }
        if (checked) {
            dispatch(updateSelectedSummarytoOutOfSync());
        } else if (!checked) {
            dispatch(updateSelectedSummarytoPending());
        }
    };

    const delegationOnClick = (alias:string, name:string, upn:string, id:string): void => {
        dispatch(updateUserAlias(alias, name, upn, id));
        dispatch(updateFilterValue('All'));
        dispatch(updateBulkSelected(false));
        dispatch(setAliasMessagebarHeight(0));
        dispatch(receiveTenantDelegations(null));
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
        if (userDelegations.filter((u: any) => u.delegator.DisplayName === 'Me').length <= 0) {
            userDelegations.unshift({
                upn: '',
                apps: [],
                delegator: { DisplayName: 'Me', UserPrincipalName: '', Id: '' },
            });
        }
        const delegationMenuProps: IContextualMenuProps = {
            items: userDelegations.map((x: any) => ({
                ['key']: x.delegator.DisplayName,
                ['text']: x.delegator.DisplayName,
                ['title']: x.delegator.DisplayName,
                ['onClick']: () => {
                    let alias = x['delegator']['UserPrincipalName'].split('@')[0];
                    
                    delegationOnClick(
                        alias,
                        x['delegator']['DisplayName'],
                        x['delegator']['UserPrincipalName'],
                        x['delegator']['Id']
                    );
                    
                    // Use NavigationUtils for consistent delegation navigation
                    NavigationUtils.navigateWithDelegation(history, alias);
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
                        size={dimensions.width > 639 ? PersonaSize.size40 : PersonaSize.size24}
                        text={dimensions.width > 374 ? personaText : ""}
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

    const isTenantDelegationBanner = !!(
        tenantDelegations?.appName === filterValue &&
        selectedTenantDelegation &&
        selectedPage === 'summary'
    );

    const expiryDetails = React.useMemo(
        () => (isTenantDelegationBanner ? null : computeDelegationExpiryDetails(selectedDelegationEntry)),
        [isTenantDelegationBanner, selectedDelegationEntry]
    );

    const expirationText = React.useMemo(() => getDelegationExpiryMessage(expiryDetails), [expiryDetails]);

    const shouldShowExpiryBanner =
        !!onBehalfOfText && !isTenantDelegationBanner && expiryDetails?.expirySeverity !== 'none' && !!expiryDetails;
    const expirySeverity = expiryDetails?.expirySeverity ?? 'none';

    React.useEffect(() => {
        if (!shouldShowExpiryBanner || !expiryDetails) return;
        const delegatorAlias = expiryDetails.delegatorAlias?.toLowerCase();
        const loggedInUpn = profile?.userPrincipalName?.toLowerCase();
        if (!delegatorAlias || !loggedInUpn) return;
        const dedupKey = `${loggedInUpn}:${delegatorAlias}`;
        if (firedDelegationExpiryWarnings.has(dedupKey)) return;
        firedDelegationExpiryWarnings.add(dedupKey);
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'DelegationExpiryWarningBannerShown',
            'MSApprovals.DelegationExpiryWarningBannerShown',
            TrackingEventId.DelegationExpiryWarningBannerShown,
            summaryCommonProperties,
            {
                delegatorAlias: expiryDetails.delegatorAlias,
                daysUntilExpiration: expiryDetails.daysUntilExpiration,
                expirySeverity: expiryDetails.expirySeverity,
                expiringAppName: expiryDetails.expiringAppName,
            }
        );
    }, [shouldShowExpiryBanner, expiryDetails, authClient, telemetryClient, summaryCommonProperties, profile]);

    const switchSubmitterView = (toSent: boolean): void => {
        if (toSent) {
            trackFeatureUsageEvent(
                authClient,
                telemetryClient,
                'SubmitterViewPivotClick',
                'MSApprovals.SubmitterView.PivotClick',
                TrackingEventId.SubmitterViewPivotClick,
                summaryCommonProperties
            );
        }

        // Close details panel: clear doc number from URL (prevents Summary useEffect from re-opening) then close
        if (isPanelOpen && history?.location?.pathname?.length > 1) {
            NavigationUtils.navigateWithSearch(history, {}, { preserveAlias: true, preserveDashboardRoute: true });
        }
        dispatch(updatePanelState(false));

        dispatch(requestMySummary(userAlias, toSent));
    };

    const onPivotItemClick = (item?: PivotItem) => {
        const isSwitching =
            (item?.props.itemKey === 'sent' && !isSubmitterView) ||
            (item?.props.itemKey === 'received' && isSubmitterView);

        if (isSwitching) {
            switchSubmitterView(item?.props.itemKey === 'sent');
        } else {
            dispatch(requestMySummary(userAlias, item?.props.itemKey === 'sent'));
        }
    };

    // Build the overflow "More" menu containing collapsed items.
    // Items are only included when they are both active (page/data conditions met)
    // and collapsed (below their respective breakpoint).
    const overflowItems = React.useMemo((): IContextualMenuItem[] => {
        const items: IContextualMenuItem[] = [];

        // Counts (collapse first) — rendered as section headers (non-interactive).
        // When showCountsCompactInline is true, counts are shown as an inline info
        // icon with badges instead of being placed in this overflow menu.
        if (!showCountsInline && !showCountsCompactInline && selectedPage === 'summary' && !isLoadingSummary) {
            if (requestedRecords > 0) {
                items.push({
                    key: 'overflow-pending-count',
                    text: `${requestedRecords} ${requestedRecordsText}`,
                    iconProps: { iconName: 'Clock', style: { color: '#605e5c' } },
                    disabled: true,
                    itemProps: {
                        styles: {
                            root: { opacity: 1 },
                            label: { color: '#323130' },
                        },
                    },
                });
            }
            if (failedRecords > 0) {
                items.push({
                    key: 'overflow-failed-count',
                    text: `${failedRecords} ${failedRecordsText}`,
                    iconProps: { iconName: 'Warning', style: { color: '#605e5c' } },
                    disabled: true,
                    itemProps: {
                        styles: {
                            root: { opacity: 1 },
                            label: { color: '#323130' },
                        },
                    },
                });
            }
        }

        // View Type (collapse second)
        if (!showViewTypeInline && selectedPage === 'summary' && !isDashboardView) {
            if (items.length > 0) {
                items.push({ key: 'divider-viewtype', itemType: 1 });
            }
            items.push({
                key: 'overflow-view-type',
                text: isCardViewSelected ? 'Switch to Table' : 'Switch to Card',
                ariaLabel: isCardViewSelected ? 'Switch to table view' : 'Switch to card view',
                iconProps: { iconName: isCardViewSelected ? 'Table' : 'ContactCard' },
                disabled: isSearchResultsViewOpen,
                onClick: (): void => {
                    setViewType(null, isCardViewSelected);
                },
            });
        }

        // Submitter View (collapses before group by/filter)
        if (!showSubmitterPivotInline && !showSubmitterCompactInline && isSubmitterViewFlighted && selectedPage === 'summary' && !selectedTenantDelegation && !userAlias) {
            if (items.length > 0) {
                items.push({ key: 'divider-submitter', itemType: 1 });
            }
            items.push({
                key: 'overflow-submitter-toggle',
                text: isSubmitterView ? 'Switch to Received' : 'Switch to Sent',
                ariaLabel: isSubmitterView ? 'Switch to received requests' : 'Switch to sent requests',
                iconProps: { iconName: isSubmitterView ? 'Inbox' : 'Send' },
                onClick: (): void => {
                    switchSubmitterView(!isSubmitterView);
                },
            });
        }

        // Group By (collapse last)
        if (!showGroupFilterInline && selectedPage !== 'faq' && selectedPage !== 'delegation' && !isDashboardView) {
            if (items.length > 0) {
                items.push({ key: 'divider-groupfilter', itemType: 1 });
            }
            items.push({
                key: 'overflow-group-by',
                text: 'Group By',
                iconProps: { iconName: 'GroupList' },
                disabled: isSearchResultsViewOpen || (isBulkSelected && selectedPage === 'summary'),
                subMenuProps: menuProps,
            });
        }

        // Filter (collapse last)
        if (!showGroupFilterInline && selectedPage !== 'faq' && selectedPage !== 'delegation' && !isDashboardView) {
            items.push({
                key: 'overflow-filter',
                text: 'Filter',
                iconProps: { iconName: filterValue === 'All' ? 'Filter' : 'FilterSolid' },
                disabled: isSearchResultsViewOpen || (isBulkSelected && selectedPage === 'summary'),
                subMenuProps: filterMenuProps,
            });
        }

        return items;
    }, [
        showCountsInline, showCountsCompactInline, showSubmitterPivotInline, showSubmitterCompactInline, showViewTypeInline, showGroupFilterInline,
        selectedPage, isLoadingSummary, requestedRecords, failedRecords, requestedRecordsText, failedRecordsText,
        isSubmitterViewFlighted, selectedTenantDelegation, userAlias, isSubmitterView,
        isDashboardView, isCardViewSelected, isSearchResultsViewOpen, isBulkSelected,
        menuProps, filterMenuProps, filterValue, summaryCommonProperties,
    ]);

    const hasOverflowItems = overflowItems.length > 0;

    const overflowMenuProps: IContextualMenuProps = React.useMemo(() => ({
        items: overflowItems,
        ariaLabel: 'More actions',
        directionalHint: DirectionalHint.bottomRightEdge,
        directionalHintFixed: true,
    }), [overflowItems]);

    // The secondary-header filter targets whatever "Group By" dimension is active, so label the active filter by its
    // dimension rather than assuming an application.
    const filterTypeLabel =
        summaryGroupedBy === GroupingBy.Submitter
            ? 'Submitter'
            : summaryGroupedBy === GroupingBy.Date
            ? 'Date'
            : summaryGroupedBy === GroupingBy.Category
            ? 'Category'
            : 'Application';

    // A pull tenant filter can never be applied to the export, and the user can choose to override the active filter via
    // the "export all" checkbox; in both cases the pill renders as "ignored" and all records are exported.
    const filterIgnored = exportAll || isFilterPullTenant;

    let exportScopeNote: React.ReactNode;
    if (isFilterPullTenant) {
        exportScopeNote = (
            <>
                Data for <strong>{filterValue}</strong> is stored externally and can&apos;t be exported by MSApprovals.
                The current filter is ignored and all other records are exported.
            </>
        );
    } else if (exportAll) {
        exportScopeNote = 'The current filter is ignored — all your approval records will be exported.';
    } else if (filterValue === 'All') {
        exportScopeNote = 'All approval records will be exported (no filter active).';
    } else {
        exportScopeNote = (
            <>
                Only records matching the {filterTypeLabel.toLowerCase()} <strong>{filterValue}</strong> will be exported.
            </>
        );
    }

    // Translates the active secondary-header filter (a single value over whatever "Group By" dimension is active) into
    // the raw field predicates the export endpoint matches on. Mirrors the client-side row filtering so the CSV
    // contains exactly the rows shown for the current filter. Returns an empty map (export all) when there is no
    // active filter, the user chose "export all", or a pull tenant is filtered (which can never be exported here).
    const buildExportFilters = (forceExportAll: boolean): Record<string, string[]> => {
        if (forceExportAll || filterValue === 'All' || !filterValue || isFilterPullTenant) {
            return {};
        }
        switch (summaryGroupedBy) {
            case GroupingBy.Submitter:
                // Submitter groups are labelled by name; match rows on the same field.
                return { 'Submitter.Name': [filterValue] };
            case GroupingBy.Date: {
                // The date group label is a locale-formatted day; collect the raw (yyyy-MM-dd) dates it covers so the
                // backend matches the same rows using its normalised date value.
                const dates = new Set<string>();
                (summary ?? []).forEach((item: any) => {
                    if (
                        item?.SubmittedDate &&
                        new Date(item.SubmittedDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION) === filterValue
                    ) {
                        dates.add(String(item.SubmittedDate).substring(0, 10));
                    }
                });
                return dates.size > 0 ? { SubmittedDate: Array.from(dates) } : {};
            }
            case GroupingBy.Category: {
                // Category is derived from tenant metadata; expand it to the TenantIds it contains.
                const tenantIds = (tenantInfo ?? [])
                    .filter((tenant: any) => (tenant?.tenantCategory || 'Others') === filterValue)
                    .map((tenant: any) => String(tenant.tenantId));
                return tenantIds.length > 0 ? { TenantId: tenantIds } : {};
            }
            case GroupingBy.Tenant:
            default:
                return filterTenantId != null ? { TenantId: [String(filterTenantId)] } : {};
        }
    };

    const handleExportSummary = (forceExportAll = false) => {
        const exportColumns = (
            visibleColumns && visibleColumns.length > 0 ? visibleColumns : DEFAULT_VISIBLE_COLUMNS.Default
        ).filter((column: string) => !SUMMARY_EXPORT_EXCLUDED_COLUMNS.includes(column));
        const exportHeaders = exportColumns.map(
            (column: string) => SUMMARY_EXPORT_HEADER_OVERRIDES[column] ?? COLUMN_DISPLAY_NAMES[column] ?? column
        );
        // Composition specs for any exported columns that are combined from multiple summary fields (e.g. the
        // "Additional Information" column). Only the specs for exported columns are sent to the backend.
        const exportColumnFormats = exportColumns.reduce(
            (formats: Record<string, SummaryExportColumnFormat>, column: string) => {
                if (SUMMARY_EXPORT_COLUMN_FORMATS[column]) {
                    formats[column] = SUMMARY_EXPORT_COLUMN_FORMATS[column];
                }
                return formats;
            },
            {} as Record<string, SummaryExportColumnFormat>
        );
        const exportFilters = buildExportFilters(forceExportAll);
        dispatch(requestDownloadSummary(userAlias, exportColumns, exportHeaders, exportFilters, exportColumnFormats));
        closeExportDialog();
    };

    const showExportButton =
        isSummaryExportFlighted &&
        showGroupFilterInline &&
        dimensions.width >= OVERFLOW_BREAKPOINTS.COLLAPSE_SUBMITTER &&
        selectedPage === 'summary' &&
        !isDashboardView &&
        !isSubmitterView;

    return (
        <div>
            <HeaderStyled.SecondaryHeaderContainer role="complementary">
                <Stack horizontal styles={HeaderStyled.SecondaryHeaderStackStyles(isPanelOpen)}>
                    <Stack.Item align="center" grow={1} styles={HeaderStyled.PersonaDropdownStyles}>
                        {/* user persona/name including delegation */}
                        <Stack horizontal style={{ backgroundColor: 'inherit' }} verticalAlign="center">
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
                            {showSubmitterPivotInline && selectedPage === 'summary' && !selectedTenantDelegation && !userAlias && (
                                <FlightingHandler
                                    featureName="Submitter View"
                                    coachmark={{
                                        target: sentPivotRef,
                                        headline: 'Sent for Approval',
                                        content: "Click here to see requests you've sent for approval.",
                                        directionalHint: DirectionalHint.bottomCenter,
                                    }}
                                >
                                    <Stack.Item styles={{ root: { paddingLeft: '20px' } }}>
                                        <Pivot aria-label="Filters" onLinkClick={onPivotItemClick}>
                                            <PivotItem headerText="Received" itemKey="received" />
                                            <PivotItem
                                                headerText="Sent"
                                                itemKey="sent"
                                                onRenderItemLink={(link, defaultRender) => {
                                                    return (
                                                        <span ref={sentPivotRef}>
                                                            {defaultRender(link)}
                                                        </span>
                                                    );
                                                }}
                                            />
                                        </Pivot>
                                    </Stack.Item>
                                </FlightingHandler>
                            )}
                            {/* Compact submitter view toggle — shown when Pivot is collapsed but above submitter threshold */}
                            {showSubmitterCompactInline && selectedPage === 'summary' && !selectedTenantDelegation && !userAlias && (
                                <FlightingHandler featureName="Submitter View">
                                    <Stack.Item styles={{ root: { paddingLeft: '8px' } }}>
                                        <TooltipHost content={isSubmitterView ? 'Switch to Received' : 'Switch to Sent'}>
                                            <ActionButton
                                                iconProps={{ iconName: isSubmitterView ? 'Inbox' : 'Send' }}
                                                text={isSubmitterView ? 'Switch to Received' : 'Switch to Sent'}
                                                ariaLabel={isSubmitterView ? 'Switch to received requests' : 'Switch to sent requests'}
                                                style={{
                                                    ...HeaderStyled.SecondaryHeaderIconStyling,
                                                    backgroundColor: 'inherit',
                                                }}
                                                onClick={() => {
                                                    switchSubmitterView(!isSubmitterView);
                                                }}
                                            />
                                        </TooltipHost>
                                    </Stack.Item>
                                </FlightingHandler>
                            )}
                        </Stack>
                    </Stack.Item>
                    {/* Inline counts: only shown when width is above collapse threshold */}
                    {showCountsInline && selectedPage === 'summary' && !isLoadingSummary && (
                        <Stack.Item align="center" grow={1}>
                            <Stack>
                                {requestedRecords > 0 && (
                                    <Stack.Item
                                        grow={1}
                                        verticalFill
                                        align="center"
                                        styles={HeaderStyled.SummaryCountStyling}
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
                                    <Stack.Item
                                        grow={1}
                                        align="center"
                                        styles={HeaderStyled.SummaryCountStyling}
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
                    )}
                    {/* Compact counts: info icon with number badges and tooltip (750-849px) */}
                    {showCountsCompactInline && selectedPage === 'summary' && !isLoadingSummary && (requestedRecords > 0 || failedRecords > 0) && (
                        <Stack.Item align="center">
                            <TooltipHost
                                content={
                                    [
                                        requestedRecords > 0 ? `${requestedRecords} ${requestedRecordsText}` : '',
                                        failedRecords > 0 ? `${failedRecords} ${failedRecordsText}` : '',
                                    ]
                                        .filter(Boolean)
                                        .join(', ')
                                }
                            >
                                <Stack
                                    horizontal
                                    verticalAlign="center"
                                    tokens={{ childrenGap: 4 }}
                                    style={{ cursor: 'default', padding: '0 4px' }}
                                    tabIndex={0}
                                    role="status"
                                    aria-atomic="true"
                                    aria-label={
                                        [
                                            requestedRecords > 0 ? `${requestedRecords} ${requestedRecordsText}` : '',
                                            failedRecords > 0 ? `${failedRecords} ${failedRecordsText}` : '',
                                        ]
                                            .filter(Boolean)
                                            .join(', ')
                                    }
                                >
                                    {requestedRecords > 0 && (
                                        <span
                                            style={{
                                                backgroundColor: '#0078d4',
                                                color: '#fff',
                                                borderRadius: '10px',
                                                padding: '1px 6px',
                                                fontSize: '11px',
                                                fontWeight: 600,
                                                lineHeight: '16px',
                                            }}
                                            aria-hidden="true"
                                        >
                                            {requestedRecords}
                                        </span>
                                    )}
                                    {failedRecords > 0 && (
                                        <span
                                            style={{
                                                backgroundColor: '#d13438',
                                                color: '#fff',
                                                borderRadius: '10px',
                                                padding: '1px 6px',
                                                fontSize: '11px',
                                                fontWeight: 600,
                                                lineHeight: '16px',
                                            }}
                                            aria-hidden="true"
                                        >
                                            {failedRecords}
                                        </span>
                                    )}
                                </Stack>
                            </TooltipHost>
                        </Stack.Item>
                    )}
                    {/* stack item for the group by, filtering, and view type icons */}
                    <Stack.Item grow={20} align="center" styles={HeaderStyled.GroupAndFilterIconStackItemStyles}>
                        <Stack horizontalAlign="end" horizontal verticalAlign="center">
                            {/* View Type — segmented toggle buttons when inline */}
                            {showViewTypeInline && selectedPage == 'summary' && !isDashboardView && (
                                <Stack
                                    horizontal
                                    verticalAlign="center"
                                    role="group"
                                    aria-label="View type"
                                    styles={HeaderStyled.ViewTypeToggleContainerStyles}
                                >
                                    <IconButton
                                        toggle
                                        checked={isCardViewSelected}
                                        iconProps={{ iconName: 'CardUiCustom' }}
                                        title="Card view"
                                        ariaLabel="Card view"
                                        disabled={isSearchResultsViewOpen}
                                        onClick={(): void => {
                                            if (!isCardViewSelected) {
                                                setViewType(null, false);
                                            }
                                        }}
                                        styles={HeaderStyled.ViewTypeToggleButtonStyles(false)}
                                    />
                                    <IconButton
                                        toggle
                                        checked={!isCardViewSelected}
                                        iconProps={{ iconName: 'TableCustom' }}
                                        title="Table view"
                                        ariaLabel="Table view"
                                        disabled={isSearchResultsViewOpen}
                                        onClick={(): void => {
                                            if (isCardViewSelected) {
                                                setViewType(null, true);
                                            }
                                        }}
                                        styles={HeaderStyled.ViewTypeToggleButtonStyles(true)}
                                    />
                                </Stack>
                            )}
                            {/* Group By — shown inline only above collapse threshold */}
                            {showGroupFilterInline && selectedPage != 'faq' && selectedPage != 'delegation' && !isDashboardView && (
                                <TooltipHost
                                    style={HeaderStyled.SecondaryHeaderIconStyling}
                                    className={isBulkSelected ? 'ms-hiddenMdDown' : null}
                                    content="Group By"
                                >
                                    <IconButton
                                        id="groupBy"
                                        iconProps={{ iconName: 'GroupList' }}
                                        ariaLabel="Group By"
                                        style={HeaderStyled.SecondaryHeaderIconStyling}
                                        menuProps={menuProps}
                                        disabled={
                                            isSearchResultsViewOpen ||
                                            (isBulkSelected && selectedPage === 'summary')
                                        }
                                    />
                                </TooltipHost>
                            )}
                            {/* Filter — shown inline only above collapse threshold */}
                            {showGroupFilterInline && selectedPage != 'faq' && selectedPage != 'delegation' && !isDashboardView && (
                                <TooltipHost content="Filter" className={isBulkSelected ? 'ms-hiddenMdDown' : null}>
                                    <IconButton
                                        id="filter"
                                        iconProps={{ iconName: filterValue === 'All' ? 'Filter' : 'FilterSolid' }}
                                        ariaLabel="Filter"
                                        style={HeaderStyled.SecondaryHeaderIconStyling}
                                        menuProps={filterMenuProps}
                                        disabled={
                                            isSearchResultsViewOpen ||
                                            (isBulkSelected && selectedPage === 'summary')
                                        }
                                    />
                                </TooltipHost>
                            )}
                            {/* Export pending approvals */}
                            {showExportButton && (
                                <TooltipHost
                                    content="Export pending approvals"
                                    className={isBulkSelected ? 'ms-hiddenMdDown' : null}
                                >
                                    {isDownloadingSummary ? (
                                        <Spinner
                                            label="Downloading"
                                            labelPosition="right"
                                            style={{ margin: '5px' }}
                                            ariaLabel="Downloading summary data"
                                        ></Spinner>
                                    ) : (
                                        <IconButton
                                            id="exportPendingApprovals"
                                            iconProps={{ iconName: 'Download' }}
                                            ariaLabel="Export pending approvals"
                                            style={HeaderStyled.SecondaryHeaderIconStyling}
                                            onClick={() => {
                                                // One-click export when there is no active filter; otherwise open the
                                                // dialog so the user can choose filtered vs. all data.
                                                if (!filterValue || filterValue === 'All') {
                                                    handleExportSummary(false);
                                                } else {
                                                    setExportAll(false);
                                                    openExportDialog();
                                                }
                                            }}
                                            disabled={isSearchResultsViewOpen}
                                        />
                                    )}
                                </TooltipHost>
                            )}
                            {/* Transient failure toast anchored to the export button */}
                            {showExportButton && summaryDownloadHasError && !isDownloadingSummary && (
                                <Callout
                                    target="#exportPendingApprovals"
                                    onDismiss={() => dispatch(clearDownloadSummaryError())}
                                    directionalHint={DirectionalHint.bottomCenter}
                                    role="alert"
                                    styles={{ root: { padding: '10px 14px', maxWidth: 260 } }}
                                >
                                    <Stack horizontal verticalAlign="center" tokens={{ childrenGap: 8 }}>
                                        <Icon
                                            iconName="ErrorBadge"
                                            styles={{ root: { color: '#A80000', fontSize: 16 } }}
                                        />
                                        <Text variant="small" styles={{ root: { color: '#323130' } }}>
                                            Download failed. Please try again later.
                                        </Text>
                                    </Stack>
                                </Callout>
                            )}
                            {/* One-time teaching coachmark introducing the export button.
                                SummaryExport flights via the useFlighting hook (it doesn't wrap
                                its UI in FlightingHandler), so the coachmark is declared standalone.
                                Eligibility (flighted + TeachingCoach + unread) and one-time
                                persistence live inside FeatureCoachmark; we only mount it while the
                                button is on screen so the anchor element exists. */}
                            {showExportButton && !isDownloadingSummary && !isSearchResultsViewOpen && (
                                <FeatureCoachmark
                                    featureName="SummaryExport"
                                    target="#exportPendingApprovals"
                                    headline={SUMMARY_EXPORT_COACHMARK_HEADLINE}
                                    content={
                                        <>
                                            <span>
                                                {SUMMARY_EXPORT_COACHMARK_CONTENT_BEFORE_ICON}{' '}
                                                <Icon
                                                    iconName="Download"
                                                    aria-hidden="true"
                                                    styles={{ root: { verticalAlign: 'middle' } }}
                                                />{' '}
                                                {SUMMARY_EXPORT_COACHMARK_CONTENT_AFTER_ICON}
                                            </span>
                                            <p style={{ marginTop: 12, marginBottom: 0 }}>
                                                {SUMMARY_EXPORT_COACHMARK_CONTENT_SECONDARY}
                                            </p>
                                        </>
                                    }
                                    directionalHint={DirectionalHint.bottomRightEdge}
                                    delayMs={300}
                                />
                            )}
                            {/* Overflow "More" button — contains collapsed items */}
                            {hasOverflowItems && (
                                <TooltipHost content="More">
                                    <IconButton
                                        iconProps={{ iconName: 'More' }}
                                        ariaLabel="More"
                                        title="More"
                                        style={HeaderStyled.SecondaryHeaderIconStyling}
                                        menuProps={overflowMenuProps}
                                    />
                                </TooltipHost>
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
                            onDismiss={() => { }}
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
                        styles={
                            expirySeverity === 'warning'
                                ? HeaderStyled.DelegationWarningBarStyles
                                : HeaderStyled.DelegationBarStyles
                        }
                        messageBarType={expirySeverity === 'warning' ? MessageBarType.warning : undefined}
                        isMultiline={expirySeverity === 'warning'}
                        aria-label={expirySeverity === 'warning' ? 'Warning message' : 'Informational message'}
                    >
                        <Stack.Item>
                            <HeaderStyled.MessageBarText>
                                Working on behalf of {onBehalfOfText}
                                {shouldShowExpiryBanner && expirationText ? ` — ${expirationText}` : ''}
                            </HeaderStyled.MessageBarText>
                        </Stack.Item>
                    </MessageBar>
                </HeaderStyled.BulkMessageHeight>
            )}
            {/* Export pending approvals callout */}
            {isExportDialogOpen && (
                <ExportSummaryCallout
                    filterIgnored={filterIgnored}
                    isFilterPullTenant={isFilterPullTenant}
                    filterTypeLabel={filterTypeLabel}
                    filterValue={filterValue}
                    exportAll={exportAll}
                    exportScopeNote={exportScopeNote}
                    onExportAllChange={setExportAll}
                    onExport={() => handleExportSummary(exportAll)}
                    onDismiss={closeExportDialog}
                />
            )}
        </div>
    );
}

const connected = React.memo(SecondaryHeader);
export { connected as SecondaryHeader };
