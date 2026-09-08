import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import {
    ChoiceGroup,
    Coachmark,
    Dropdown,
    Icon,
    IChoiceGroupOption,
    IChoiceGroupOptionStyles,
    IDropdownOption,
    IStackTokens,
    MessageBar,
    MessageBarType,
    mergeStyleSets,
    PrimaryButton,
    ResponsiveMode,
    Stack,
    TeachingBubble,
    Toggle,
    TooltipHost,
    DirectionalHint,
} from '@fluentui/react';
import { registerIcons } from '@fluentui/react/lib/Styling';
import { CardUi20Regular, Table20Regular } from '@fluentui/react-icons';
import * as React from 'react';
import { Reducer } from 'redux';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../SharedComponents.reducer';
import { sharedComponentsSagas } from '../SharedComponents.sagas';
import { ClearUserPreferencesAPIMessages, postQuickTourInfo, SaveUserPreferencesRequest, updateCardViewType } from '../SharedComponents.actions';
import { GroupingBy } from './GroupingBy';
import { FlightingFeaturesLabs } from './FlightingFeaturesLabs';

import * as Styled from '../SharedLayout';

import * as ButtonStyled from '../Details/DetailsButtons/DetailsButtonsStyled';
import {
    SETTINGS_COACHMARK_ID,
    DETAILS_DEFAULT_VIEW,
    DOCKED_VIEW,
    FLYOUT_VIEW,
    GROUP_BY_FILTER,
    HISTORY_DEFAULT_VIEW,
    DEFAULT_VIEW_TYPE,
    DEFAULT_TENANT,
    TABLE_VIEW,
    CARD_VIEW,
    TIMEZONE_OPTIONS,
    WINDOWS_TO_IANA_TIMEZONE,
    detectWindowsTimezone,
} from '../SharedConstants';
import { useRef } from 'react';
import {
    getDetailsDefaultView,
    getHistoryDefaultView,
    getHasUnreadLabsCoachmark,
    getItemsRead,
    getSelectedPage,
    getUpdatedQuickToursList,
    getUserPreferences,
    getUserPreferencesFailureMessage,
    getUserPreferencesSuccessMessage,
    getTenantInfo,
    getSummary,
    getDefaultTenant,
    getAllBulkTenantOptions,
    getDefaultViewType,
    getDigestPreference,
    getTeamsNotificationsEnabled,
} from '../SharedComponents.selectors';
import { useFlighting } from './FlightingHandler';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../Helpers/telemetryHelpers';

// Register custom icons
registerIcons({ icons: { CardUiCustom: <CardUi20Regular />, TableCustom: <Table20Regular /> } });

export const MOBILE_QUERY = '(max-width: 600px)';

export const SETTINGS_SECTIONS = [
    { key: 'general', name: 'General', icon: 'Settings' },
    { key: 'notifications', name: 'Notifications', icon: 'Ringer' },
    // Approvals Labs is temporarily hidden; restore this entry to bring the tab back.
    // { key: 'labs', name: 'Approvals Labs', icon: 'TestBeaker' },
];

const layout = mergeStyleSets({
    root: {
        display: 'flex',
        flex: 1,
        minWidth: 0,
        minHeight: 0,
        selectors: { [`@media ${MOBILE_QUERY}`]: { flexDirection: 'column' } },
    },
    sidebar: {
        width: 200,
        flexShrink: 0,
        backgroundColor: '#F5F5F5',
        borderRight: '1px solid #EDEBE9',
        padding: '12px 8px',
        overflowY: 'auto',
        selectors: {
            [`@media ${MOBILE_QUERY}`]: {
                width: '100%',
                backgroundColor: '#FFFFFF',
                borderRight: 'none',
                padding: '8px 4px',
                display: 'flex',
                flexDirection: 'column',
                gap: 4,
            },
        },
    },
    navItem: {
        position: 'relative',
        display: 'flex', alignItems: 'center', gap: 10, width: '100%', padding: '8px 12px 8px 16px',
        border: 'none', background: 'transparent', textAlign: 'left', cursor: 'pointer',
        borderRadius: 4, fontSize: 14, color: '#242424',
        selectors: {
            '&:hover': { backgroundColor: '#EBEBEB' },
            [`@media ${MOBILE_QUERY}`]: { padding: '14px 16px 14px 20px', fontSize: 16 },
        },
    },
    navItemActive: {
        backgroundColor: '#F0F0F7',
        fontWeight: 600,
        color: '#242424',
        selectors: {
            '::before': {
                content: '""',
                position: 'absolute',
                left: 4,
                top: 8,
                bottom: 8,
                width: 3,
                borderRadius: 2,
                backgroundColor: '#5B5FC7',
            },
        },
    },
    content: {
        flex: 1,
        minWidth: 0,
        overflowY: 'auto',
        WebkitOverflowScrolling: 'touch',
        overscrollBehavior: 'contain',
        padding: '20px 24px',
        selectors: { [`@media ${MOBILE_QUERY}`]: { padding: '16px' } },
    },
});

interface UserSettingsProps {
    isMobile?: boolean;
    drilledIn?: boolean;
    section?: string;
    onSelectSection?: (key: string) => void;
}

function UserSettings(props: UserSettingsProps = {}): React.ReactElement {
    const stackTokens: IStackTokens = { childrenGap: 12 };

    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);

    const { useSelector, dispatch, authClient, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const tenantInfo = useSelector(getTenantInfo);
    const summary = useSelector(getSummary);
    const DefaultTenant = useSelector(getDefaultTenant);
    const selectedPage = useSelector(getSelectedPage);
    const userPreferences = useSelector(getUserPreferences);
    const detailsDefaultView = useSelector(getDetailsDefaultView);
    const historyDefaultView = useSelector(getHistoryDefaultView);
    const defaultViewType = useSelector(getDefaultViewType);
    const userPreferencesSuccessMessage = useSelector(getUserPreferencesSuccessMessage);
    const userPreferencesFailureMessage = useSelector(getUserPreferencesFailureMessage);
    const digestPreference = useSelector(getDigestPreference);
    const teamsNotificationsEnabledFromStore = useSelector(getTeamsNotificationsEnabled);
    const digestNotificationEnabled = useFlighting('DigestNotification');
    const isLabsCoachmarkUnread = useSelector(getHasUnreadLabsCoachmark);
    const readQuickTours = useSelector(getItemsRead);
    const updatedQuickTourList = useSelector(getUpdatedQuickToursList);

    const [localDetailsDefaultView, setLocalDetailsDefaultView] = React.useState(false);
    const [localHistoryDefaultView, setLocalHistoryDefaultView] = React.useState(false);
    const [localDefaultViewType, setLocalDefaultViewType] = React.useState(false);
    const [localDefaultTenantView, setLocalDefaultTenantView] = React.useState(null);
    const [dropdownOptions, setDropdownOptions] = React.useState<IDropdownOption[]>([]);
    const [groupBy, setGroupBy] = React.useState(null);
    const [digestEmailEnabled, setDigestEmailEnabled] = React.useState(false);
    const [digestEmailCadence, setDigestEmailCadence] = React.useState<string>('daily');
    const [teamsNotificationsEnabled, setTeamsNotificationsEnabled] = React.useState(true);
    const [preferredTimezone, setPreferredTimezone] = React.useState<string>(detectWindowsTimezone);
    const [isSaving, setIsSaving] = React.useState(false);
    const [internalSection, setInternalSection] = React.useState('general');

    const selectedSection = props.section ?? internalSection;
    const setSelectedSection = props.onSelectSection ?? setInternalSection;
    const isMobile = props.isMobile ?? false;
    const mobileDrilledIn = props.drilledIn ?? false;

    const sections = SETTINGS_SECTIONS.filter((s) => s.key !== 'notifications' || digestNotificationEnabled);
    const showSidebar = !isMobile || !mobileDrilledIn;
    const showContent = !isMobile || mobileDrilledIn;

    const labsTabRef = React.useRef<HTMLButtonElement>(null);
    const labsInlineAnchorRef = React.useRef<HTMLSpanElement>(null);
    const [coachmarkDismissed, setCoachmarkDismissed] = React.useState(false);
    // Suppress the coachmark whenever the Approvals Labs tab is not registered in SETTINGS_SECTIONS.
    const isLabsTabHidden = !SETTINGS_SECTIONS.some((s) => s.key === 'labs');
    const showLabsCoachmark =
        !isLabsTabHidden && isLabsCoachmarkUnread && !coachmarkDismissed && selectedSection !== 'labs';
    const [teachingBubbleVisible, setTeachingBubbleVisible] = React.useState(false);

    const dismissLabsCoachmark = React.useCallback(() => {
        setCoachmarkDismissed(true);
        setTeachingBubbleVisible(false);
        // Backend's QuickTourFeatureList is the full read set (InsertOrReplace),
        // so merge labs id into the existing server-truth read list rather than
        // posting just [labsId] which would wipe other acknowledged tours.
        if (!readQuickTours) return;
        const readIds = readQuickTours.map((i) => i.id.toString());
        if (readIds.includes(SETTINGS_COACHMARK_ID)) return;
        readIds.push(SETTINGS_COACHMARK_ID);
        dispatch(postQuickTourInfo(readIds));
    }, [dispatch, readQuickTours]);

    const cadenceToHours: Record<string, number> = {
        every2hours: 2,
        every4hours: 4,
        every8hours: 8,
        every12hours: 12,
        daily: 24,
    };

    const hoursToCadence: Record<number, string> = Object.fromEntries(
        Object.entries(cadenceToHours).map(([k, v]) => [v, k])
    );

    const storedDigestEnabled = digestPreference?.DigestEnabled ?? false;
    const storedDigestCadence = hoursToCadence[digestPreference?.Cadence] || 'daily';
    const storedTimezone = digestPreference?.TimeZone || detectWindowsTimezone();
    const storedTeamsEnabled = teamsNotificationsEnabledFromStore ?? true;

    const notificationPrefsChanged =
        digestEmailEnabled !== storedDigestEnabled ||
        digestEmailCadence !== storedDigestCadence ||
        teamsNotificationsEnabled !== storedTeamsEnabled ||
        preferredTimezone !== storedTimezone;

    // Emit a feature-usage event whenever the user opens the Approvals Labs tab so
    // developers and managers can measure how many people engage with Approvals Labs.
    React.useEffect(() => {
        if (selectedSection === 'labs') {
            trackFeatureUsageEvent(
                authClient,
                telemetryClient,
                'ApprovalsLabsTabOpen',
                'MSApprovals.ApprovalsLabs.Open',
                TrackingEventId.ApprovalsLabsTabOpen,
                null
            );
        }
    }, [selectedSection, authClient, telemetryClient]);

    React.useEffect(() => {
        if (userPreferences && userPreferences.some((a: any) => a.UserPreferenceText === GROUP_BY_FILTER)) {
            const groupByStatus = userPreferences.find(
                (a: any) => a.UserPreferenceText === GROUP_BY_FILTER
            ).UserPreferenceStatus;
            setGroupBy(groupByStatus);
        }
    }, [userPreferences]);

    React.useEffect(() => {
        setLocalDetailsDefaultView(detailsDefaultView == DOCKED_VIEW);
    }, [detailsDefaultView]);

    React.useEffect(() => {
        setLocalHistoryDefaultView(historyDefaultView == DOCKED_VIEW);
    }, [historyDefaultView]);

    React.useEffect(() => {
        setLocalDefaultViewType(defaultViewType == TABLE_VIEW);
    }, [defaultViewType]);

    React.useEffect(() => {
        setLocalDefaultTenantView(DefaultTenant);
    }, [DefaultTenant]);

    React.useEffect(() => {
        if (digestPreference) {
            setDigestEmailEnabled(digestPreference.DigestEnabled ?? false);
            setDigestEmailCadence(hoursToCadence[digestPreference.Cadence] || 'daily');
            setPreferredTimezone(digestPreference.TimeZone || detectWindowsTimezone());
            setIsSaving(false);
        }
    }, [digestPreference]);

    React.useEffect(() => {
        if (teamsNotificationsEnabledFromStore !== null && teamsNotificationsEnabledFromStore !== undefined) {
            setTeamsNotificationsEnabled(teamsNotificationsEnabledFromStore);
        } else {
            setTeamsNotificationsEnabled(true);
        }
        setIsSaving(false);
    }, [teamsNotificationsEnabledFromStore]);

    React.useEffect(() => {
        if (userPreferencesFailureMessage) {
            setIsSaving(false);
        }
    }, [userPreferencesFailureMessage]);

    React.useEffect(() => {
        populateDropDownOptions();
    }, [summary, selectedPage]);

    const populateDropDownOptions = (): void => {
        getFilteredMenuItems(GroupingBy.Tenant);
    };

    const getFilteredMenuItems = (filterKey: any): void => {
        const filterMenuProps = getAllBulkTenantOptions(tenantInfo);
        if (filterMenuProps) {
            setDropdownOptions(filterMenuProps);
        }
    };

    const menuProps: IDropdownOption[] = [
        {
            key: GroupingBy.Tenant,
            text: 'Application',
        },
        {
            key: GroupingBy.Submitter,
            text: 'Submitter',
        },
        {
            key: GroupingBy.Date,
            text: 'Date',
        },
        {
            key: GroupingBy.Category,
            text: 'Category',
        },
    ];

    const saveUserPreferences = () => {
        const existingFeaturePreferences = userPreferences || [];

        if (existingFeaturePreferences) {
            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === DETAILS_DEFAULT_VIEW)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === DETAILS_DEFAULT_VIEW
                ).UserPreferenceStatus = localDetailsDefaultView ? DOCKED_VIEW : FLYOUT_VIEW;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: DETAILS_DEFAULT_VIEW,
                    UserPreferenceStatus: localDetailsDefaultView ? DOCKED_VIEW : FLYOUT_VIEW,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === HISTORY_DEFAULT_VIEW)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === HISTORY_DEFAULT_VIEW
                ).UserPreferenceStatus = localHistoryDefaultView ? DOCKED_VIEW : FLYOUT_VIEW;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: HISTORY_DEFAULT_VIEW,
                    UserPreferenceStatus: localHistoryDefaultView ? DOCKED_VIEW : FLYOUT_VIEW,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === DEFAULT_VIEW_TYPE)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === DEFAULT_VIEW_TYPE
                ).UserPreferenceStatus = localDefaultViewType ? TABLE_VIEW : CARD_VIEW;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: DEFAULT_VIEW_TYPE,
                    UserPreferenceStatus: localDefaultViewType ? TABLE_VIEW : CARD_VIEW,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === GROUP_BY_FILTER)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === GROUP_BY_FILTER
                ).UserPreferenceStatus = groupBy;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: GROUP_BY_FILTER,
                    UserPreferenceStatus: groupBy,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === DEFAULT_TENANT)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === DEFAULT_TENANT
                ).UserPreferenceStatus = localDefaultTenantView;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: DEFAULT_TENANT,
                    UserPreferenceStatus: localDefaultTenantView,
                });
            }
        }

        const request: any = {
            FeaturePreferenceJson: JSON.stringify(existingFeaturePreferences),
        };

        dispatch(SaveUserPreferencesRequest(request));
    };

    const isMounted = useRef(false);

    React.useEffect(() => {
        dispatch(ClearUserPreferencesAPIMessages());
    }, []);

    React.useEffect(() => {
        if (isMounted.current) {
            saveUserPreferences();
        }
    }, [localDetailsDefaultView, localHistoryDefaultView, localDefaultViewType, groupBy, localDefaultTenantView]);

    const detailsDefaultViewChange = (
        ev?: React.FormEvent<HTMLElement | HTMLInputElement>,
        option?: IChoiceGroupOption
    ) => {
        if (!option) return;
        const checked = option.key === 'docked';
        isMounted.current = true;
        setLocalDetailsDefaultView(checked);
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'DetailsDefaultViewToggle',
            checked ? 'MSApprovals.DetailsDefaultView.Docked' : 'MSApprovals.DetailsDefaultView.Flyout',
            checked ? TrackingEventId.DockedView : TrackingEventId.FlyOutView,
            null
        );
    };

    const historyDefaultViewChange = (
        ev?: React.FormEvent<HTMLElement | HTMLInputElement>,
        option?: IChoiceGroupOption
    ) => {
        if (!option) return;
        const checked = option.key === 'docked';
        isMounted.current = true;
        setLocalHistoryDefaultView(checked);
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'HistoryDefaultViewToggle',
            checked ? 'MSApprovals.HistoryDefaultView.Docked' : 'MSApprovals.HistoryDefaultView.Flyout',
            checked ? TrackingEventId.DockedView : TrackingEventId.FlyOutView,
            null
        );
    };

    const defaultViewTypeChange = (
        ev?: React.FormEvent<HTMLElement | HTMLInputElement>,
        option?: IChoiceGroupOption
    ) => {
        if (!option) return;
        const checked = option.key === 'table';
        isMounted.current = true;
        setLocalDefaultViewType(checked);
        // Persistent reducer applies RECEIVE_USER_PREFERENCES only once per session,
        // so we must dispatch UPDATE_CARD_VIEW_TYPE here to flip list rendering immediately.
        dispatch(updateCardViewType(!checked));
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'DefaultViewTypeToggle',
            checked ? 'MSApprovals.DefaultViewType.Table' : 'MSApprovals.DefaultViewType.Card',
            checked ? TrackingEventId.TableView : TrackingEventId.CardView,
            null
        );
    };

    const pictorialChoiceStyles: IChoiceGroupOptionStyles = {
        root: { marginTop: 0, marginRight: 8 },
        choiceFieldWrapper: { width: 72 },
        field: {
            padding: '6px 6px 4px',
            borderRadius: 4,
            border: '1px solid #D1D1D1',
            backgroundColor: '#FFFFFF',
            selectors: {
                '::before': { display: 'none' },
                '::after': { display: 'none' },
                '&:hover': { backgroundColor: '#F3F2F1' },
                '&.is-checked': {
                    backgroundColor: '#EFF6FC',
                    borderColor: '#0078D4',
                    borderWidth: 2,
                    padding: '5px 5px 3px',
                },
                '&.is-checked .ms-ChoiceFieldLabel': { fontWeight: 600, color: '#106EBE' },
                '&.is-checked .ms-ChoiceField-iconWrapper': { color: '#0078D4' },
                '.ms-ChoiceFieldLabel-wrapper': { paddingTop: 0 },
            },
        },
        iconWrapper: { fontSize: 20, height: 20, lineHeight: 20, marginBottom: 0 },
        labelWrapper: { height: 'auto', maxHeight: 'none' },
    };

    const dockedFlyoutOptions = (groupName: string): IChoiceGroupOption[] => [
        {
            key: 'docked',
            text: 'Docked',
            iconProps: { iconName: 'DockRight' },
            ariaLabel: `${groupName}: Docked`,
            styles: pictorialChoiceStyles,
        },
        {
            key: 'flyout',
            text: 'Flyout',
            iconProps: { iconName: 'OpenPane' },
            ariaLabel: `${groupName}: Flyout`,
            styles: pictorialChoiceStyles,
        },
    ];

    const viewTypeOptions: IChoiceGroupOption[] = [
        {
            key: 'table',
            text: 'Table',
            iconProps: { iconName: 'Table' },
            ariaLabel: 'View type: Table',
            styles: pictorialChoiceStyles,
        },
        {
            key: 'card',
            text: 'Card',
            iconProps: { iconName: 'CardUiCustom' },
            ariaLabel: 'View type: Card',
            styles: pictorialChoiceStyles,
        },
    ];

    const defaultTenantChange = (ev: React.FormEvent<HTMLElement>, value: IDropdownOption) => {
        isMounted.current = true;
        setLocalDefaultTenantView(value.key);
    };

    const groupByChange = (ev: React.FormEvent<HTMLElement>, value: IDropdownOption) => {
        isMounted.current = true;
        setGroupBy(value.key);
    };

    const digestEmailCadenceOptions: IDropdownOption[] = [
        { key: 'every2hours', text: 'Every 2 hours' },
        { key: 'every4hours', text: 'Every 4 hours' },
        { key: 'every8hours', text: 'Every 8 hours' },
        { key: 'every12hours', text: 'Every 12 hours' },
        { key: 'daily', text: 'Once daily' },
    ];

    const saveNotificationPreferences = () => {
        const digestPreference = {
            DigestEnabled: digestEmailEnabled,
            Cadence: cadenceToHours[digestEmailCadence] || 24,
            TimeZone: preferredTimezone,
        };

        const request: any = {
            DigestPreferenceJson: JSON.stringify(digestPreference),
            TeamsNotificationsEnabled: teamsNotificationsEnabled,
        };

        setIsSaving(true);
        dispatch(SaveUserPreferencesRequest(request));
    };

    const digestEmailToggleChange = (ev: React.MouseEvent<HTMLElement>, checked?: boolean) => {
        if (checked !== undefined) {
            setDigestEmailEnabled(checked);
            if (checked) {
                setTeamsNotificationsEnabled(false);
            }
        }
    };

    const teamsNotificationsToggleChange = (ev: React.MouseEvent<HTMLElement>, checked?: boolean) => {
        if (checked !== undefined) {
            setTeamsNotificationsEnabled(checked);
        }
    };

    const digestEmailCadenceChange = (ev: React.FormEvent<HTMLElement>, value?: IDropdownOption) => {
        if (value) {
            setDigestEmailCadence(value.key as string);
        }
    };

    const timezoneChange = (ev: React.FormEvent<HTMLElement>, value?: IDropdownOption) => {
        if (value) {
            setPreferredTimezone(value.key as string);
        }
    };

    const getNextDigestTime = (cadenceHours: number, windowsTimeZone: string): Date => {
        const ianaZone = WINDOWS_TO_IANA_TIMEZONE[windowsTimeZone] || 'UTC';
        const now = new Date();
        const nowLocal = new Date(now.toLocaleString('en-US', { timeZone: ianaZone }));

        const anchor = new Date(nowLocal);
        anchor.setHours(8, 0, 0, 0);

        const elapsedMs = nowLocal.getTime() - anchor.getTime();
        const elapsedHours = elapsedMs / (1000 * 60 * 60);
        const intervalsElapsed = Math.floor(elapsedHours / cadenceHours);

        const nextLocal = new Date(anchor);
        nextLocal.setHours(8 + (intervalsElapsed + 1) * cadenceHours);

        const offsetMs = nowLocal.getTime() - now.getTime();
        return new Date(nextLocal.getTime() - offsetMs);
    };

    const nextDigestTime = React.useMemo(() => {
        if (!digestEmailEnabled) return null;
        const cadenceHours = cadenceToHours[digestEmailCadence] || 24;
        const nextUtc = getNextDigestTime(cadenceHours, preferredTimezone);
        const ianaZone = WINDOWS_TO_IANA_TIMEZONE[preferredTimezone] || 'UTC';
        return nextUtc.toLocaleString('en-US', {
            timeZone: ianaZone,
            weekday: 'short',
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    }, [digestEmailEnabled, digestEmailCadence, preferredTimezone]);

    return (
        <div className={layout.root}>
            {showSidebar && (
            <div className={layout.sidebar} role="tablist" aria-label="Settings">
                {sections.map((s) => (
                    <button
                        key={s.key}
                        type="button"
                        role="tab"
                        ref={s.key === 'labs' ? labsTabRef : undefined}
                        aria-selected={selectedSection === s.key}
                        className={`${layout.navItem} ${selectedSection === s.key && !isMobile ? layout.navItemActive : ''}`}
                        onClick={() => {
                            setSelectedSection(s.key);
                            dispatch(ClearUserPreferencesAPIMessages());
                            if (s.key === 'labs' && isLabsCoachmarkUnread && !coachmarkDismissed) {
                                dismissLabsCoachmark();
                            }
                        }}
                    >
                        <Icon iconName={s.icon} aria-hidden />
                        <span>{s.name}</span>
                        {s.key === 'labs' && (
                            <span
                                ref={labsInlineAnchorRef}
                                aria-hidden
                                style={{ display: 'inline-block', width: 1, height: 1, marginLeft: 4 }}
                            />
                        )}
                    </button>
                ))}
            </div>
            )}
            {showLabsCoachmark && labsTabRef.current && (
                <Coachmark
                    target={isMobile && labsInlineAnchorRef.current ? labsInlineAnchorRef.current : labsTabRef.current}
                    positioningContainerProps={{ directionalHint: DirectionalHint.rightCenter }}
                    ariaAlertText="A coachmark is available for Approvals Labs"
                    ariaDescribedBy="approvals-labs-coachmark-desc"
                    ariaLabelledBy="approvals-labs-coachmark-label"
                    onAnimationOpenEnd={() => setTeachingBubbleVisible(true)}
                />
            )}
            {showLabsCoachmark && teachingBubbleVisible && labsTabRef.current && (
                <TeachingBubble
                    target={labsTabRef.current}
                    calloutProps={{
                        directionalHint: isMobile ? DirectionalHint.bottomCenter : DirectionalHint.rightCenter,
                    }}
                    headline="Try Approvals Labs"
                    hasCloseButton
                    closeButtonAriaLabel="Dismiss"
                    primaryButtonProps={{ children: 'Got it', onClick: dismissLabsCoachmark }}
                    onDismiss={dismissLabsCoachmark}
                    ariaDescribedBy="approvals-labs-coachmark-desc"
                    ariaLabelledBy="approvals-labs-coachmark-label"
                >
                    Preview new approval experiences before they ship. Opt in to features here, give feedback,
                    and opt back out any time.
                </TeachingBubble>
            )}
            {showContent && (
            <div className={layout.content} role="tabpanel">
                {selectedSection === 'general' && (
                <Stack tokens={stackTokens}>
                    <Stack.Item>
                        {userPreferencesFailureMessage && (
                            <Styled.ErrorMessage>{userPreferencesFailureMessage}</Styled.ErrorMessage>
                        )}
                    </Stack.Item>
                    <Stack.Item>
                        <label>
                            Choose default view
                            <TooltipHost
                                content="Select between viewing the details page as a flyout or docked."
                                calloutProps={ButtonStyled.tooltipCalloutProps}
                                styles={ButtonStyled.tooltipHostContainer}
                            >
                                <Icon
                                    role="img"
                                    aria-label="Select between viewing the details page as a flyout or docked."
                                    iconName="info"
                                    tabIndex={0}
                                />
                            </TooltipHost>
                        </label>
                    </Stack.Item>
                    <Stack horizontal tokens={{ childrenGap: 24 }} wrap>
                        <ChoiceGroup
                            label="Details view"
                            selectedKey={localDetailsDefaultView ? 'docked' : 'flyout'}
                            options={dockedFlyoutOptions('Details view')}
                            onChange={detailsDefaultViewChange}
                            styles={{ flexContainer: { display: 'flex', flexDirection: 'row' } }}
                        />
                        <ChoiceGroup
                            label="History view"
                            selectedKey={localHistoryDefaultView ? 'docked' : 'flyout'}
                            options={dockedFlyoutOptions('History view')}
                            onChange={historyDefaultViewChange}
                            styles={{ flexContainer: { display: 'flex', flexDirection: 'row' } }}
                        />
                        <ChoiceGroup
                            label="View Type"
                            selectedKey={localDefaultViewType ? 'table' : 'card'}
                            options={viewTypeOptions}
                            onChange={defaultViewTypeChange}
                            styles={{ flexContainer: { display: 'flex', flexDirection: 'row' } }}
                        />
                    </Stack>
                    <Stack horizontal tokens={{ childrenGap: 16 }} wrap>
                        <Stack.Item grow={1} styles={{ root: { minWidth: 240 } }}>
                            <Dropdown
                                placeholder="Select an option"
                                label="Default grouping for pending approvals"
                                options={menuProps}
                                onChange={groupByChange}
                                selectedKey={groupBy}
                                responsiveMode={ResponsiveMode.large}
                            />
                        </Stack.Item>
                        <Stack.Item grow={1} styles={{ root: { minWidth: 240 } }}>
                            <Dropdown
                                placeholder="Select an option"
                                label="Default Application for Bulk Approval"
                                options={dropdownOptions}
                                onChange={defaultTenantChange}
                                selectedKey={localDefaultTenantView}
                                styles={Styled.dropdownStyles}
                                responsiveMode={ResponsiveMode.large}
                                calloutProps={{
                                    directionalHint: DirectionalHint.bottomLeftEdge,
                                    directionalHintFixed: true,
                                }}
                            />
                        </Stack.Item>
                    </Stack>
                </Stack>
                )}
                {selectedSection === 'labs' && <FlightingFeaturesLabs />}
                {selectedSection === 'notifications' && (
                <Stack tokens={stackTokens}>
                    {userPreferencesFailureMessage && !isSaving && (
                        <Stack.Item>
                            <Styled.ErrorMessage>{userPreferencesFailureMessage}</Styled.ErrorMessage>
                        </Stack.Item>
                    )}
                    {userPreferencesSuccessMessage && !notificationPrefsChanged && (
                        <Stack.Item>
                            <Styled.SuccessMessage>Notification preferences saved.</Styled.SuccessMessage>
                        </Stack.Item>
                    )}
                    <Stack.Item>
                        <label>
                            Email notifications
                            <TooltipHost
                                content="Enable digest to receive a scheduled email listing all pending approvals at your chosen cadence, replacing immediate notification emails and daily reminder emails."
                                calloutProps={ButtonStyled.tooltipCalloutProps}
                                styles={ButtonStyled.tooltipHostContainer}
                            >
                                <Icon
                                    role="img"
                                    aria-label="Enable digest to receive a scheduled email listing all pending approvals at your chosen cadence, replacing immediate notification emails and daily reminder emails."
                                    iconName="info"
                                    tabIndex={0}
                                />
                            </TooltipHost>
                        </label>
                    </Stack.Item>
                    <Stack.Item>
                        <label>Digest email (replaces individual notifications)</label>
                    </Stack.Item>
                    <Stack.Item>
                        <Toggle
                            inlineLabel
                            onText="On"
                            offText="Off"
                            checked={digestEmailEnabled}
                            onChange={digestEmailToggleChange}
                        />
                    </Stack.Item>
                    {digestEmailEnabled && (
                        <Stack.Item>
                            <Dropdown
                                label="Digest email cadence"
                                placeholder="Select cadence"
                                options={digestEmailCadenceOptions}
                                selectedKey={digestEmailCadence}
                                onChange={digestEmailCadenceChange}
                                responsiveMode={ResponsiveMode.large}
                            />
                        </Stack.Item>
                    )}
                    {digestEmailEnabled && (
                        <Stack.Item>
                            <Dropdown
                                label="Preferred time zone"
                                placeholder="Select time zone"
                                options={TIMEZONE_OPTIONS}
                                selectedKey={preferredTimezone}
                                onChange={timezoneChange}
                                responsiveMode={ResponsiveMode.large}
                            />
                        </Stack.Item>
                    )}
                    {digestEmailEnabled && nextDigestTime && notificationPrefsChanged && (
                        <Stack.Item>
                            <p style={{ fontSize: '12px', color: '#605e5c', margin: '4px 0 0 0' }}>
                                Next digest at approximately <strong>{nextDigestTime}</strong>
                            </p>
                        </Stack.Item>
                    )}
                    <Stack.Item>
                        <div style={{ marginTop: '16px' }}>
                        <label>
                            Teams notifications
                            <TooltipHost
                                content="Digest is not available for Teams notifications. Teams notifications can only be turned on or off."
                                calloutProps={ButtonStyled.tooltipCalloutProps}
                                styles={ButtonStyled.tooltipHostContainer}
                            >
                                <Icon
                                    role="img"
                                    aria-label="Digest is not available for Teams notifications. Teams notifications can only be turned on or off."
                                    iconName="info"
                                    tabIndex={0}
                                />
                            </TooltipHost>
                        </label>
                        </div>
                    </Stack.Item>
                    <Stack.Item>
                        <Toggle
                            inlineLabel
                            onText="On"
                            offText="Off"
                            checked={teamsNotificationsEnabled}
                            onChange={teamsNotificationsToggleChange}
                        />
                    </Stack.Item>
                    {teamsNotificationsEnabled && digestEmailEnabled && (
                        <Stack.Item>
                            <MessageBar messageBarType={MessageBarType.info}>
                                Keeping Teams notifications on will send immediate notifications for every approval on Teams. If your goal is to reduce notification noise, we recommend turning Teams notifications off and relying on digest emails instead.
                            </MessageBar>
                        </Stack.Item>
                    )}
                    <Stack.Item>
                        <PrimaryButton
                            text={isSaving ? 'Saving...' : 'Save'}
                            onClick={saveNotificationPreferences}
                            disabled={!notificationPrefsChanged || isSaving}
                            style={{ marginTop: '20px' }}
                        />
                    </Stack.Item>
                </Stack>
                )}
            </div>
            )}
        </div>
    );
}

const connected = withContext(UserSettings as React.ComponentType<UserSettingsProps>) as React.ComponentType<UserSettingsProps>;
export { connected as UserSettings };
