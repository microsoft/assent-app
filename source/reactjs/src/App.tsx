import * as React from 'react';
import { render } from 'react-dom';
import { ThemeProvider, Spinner, Stack, loadTheme } from '@fluentui/react';
import { ShellWithStore } from './ShellWithStore';
import { BrowserRouter } from 'react-router-dom';
import { Main } from './Components/Shared/Components/Main';
import { navConfig } from './navConfig';
import { Routes } from './Routes';
import { usePersistentReducer } from './Components/Shared/Components/PersistentReducer';
import {
    sharedComponentsPersistentReducerName,
    sharedComponentsPersistentReducer,
} from './Components/Shared/SharedComponents.persistent-reducer';
import { sharedComponentsSagas } from './Components/Shared/SharedComponents.sagas';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { useUser } from '@micro-frontend-react/employee-experience/lib/useUser';
import { useGraphPhoto } from '@micro-frontend-react/employee-experience/lib/useGraphPhoto';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { HelpPanel } from './Components/HelpPanel/HelpPanel';
import { SecondaryHeader } from './Components/Shared/Components/SecondaryHeader/SecondaryHeader';
import './App.css';
import { sharedComponentsReducerName } from './Components/Shared/SharedComponents.reducer';
import { sharedComponentsReducer } from './Components/Shared/SharedComponents.reducer';
import { Reducer } from 'redux';
import { UserSettings } from './Components/Shared/Components/UserSettings';
import { getNotifications } from './Components/NotificationsPanel/NotificationsPanel';
import { INotificationsPanelState } from './Components/NotificationsPanel/NotificationsPanel.types';
import {
    notificationsPanelReducerName,
    notificationsPanelInitialState,
} from './Components/NotificationsPanel/NotificationsPanel.reducer';
import { toggleNotificationPanel } from './Components/NotificationsPanel/NotificationsPanel.actions';
import { useLoginOnStartup } from '@micro-frontend-react/employee-experience/lib/useLoginOnStartup';
import * as SharedStyled from './Components/Shared/SharedLayout';
import ErrorResult from './Components/Shared/Components/ErrorResult';
import { NotificationPanelCustom } from './Components/NotificationsPanel/NotificationsPanel';
import { FontIcon } from '@fluentui/react/lib/Icon';
import * as notificationStyled from './Components/NotificationsPanel/NotificationsPanelStyling';
import { getIsPanelOpen, getIsSettingPanelOpen } from './Components/Shared/SharedComponents.selectors';
import { getTeachingBubbleVisibility } from './Components/Shared/SharedComponents.persistent-selectors';
import { TopHeader } from './Components/Shared/Components/SecondaryHeader/TopHeader';
import CoherenceTheme from './Helpers/Theme';
import { SideNav } from './Components/Shared/Components/SideNav';
import { initializeIcons } from '@fluentui/font-icons-mdl2';
import { UserSettingsPanel } from './Components/UserSettingsPanel/UserSettingsPanel';
import { helpPanelReducer, helpPanelReducerName } from './Components/HelpPanel/HelpPanel.reducer';
import { helpPanelSagas } from './Components/HelpPanel/HelpPanel.sagas';
import { ProfilePanel } from './Components/ProfilePanel';
import { registerIcons } from '@fluentui/react/lib/Styling';
import { AccessibilityPanel } from './Components/AccessibilityPanel/AccessibilityPanel';
import { accessibilityReducer, accessibilityReducerName } from './Components/AccessibilityPanel/Accessibility.reducer';

export function App(): React.ReactElement {
    useLoginOnStartup(true, { scopes: ['https://graph.microsoft.com/.default'] });
    initializeIcons();
    registerIcons({
        icons: {
            Accessibility: <SharedStyled.AccessibilityIcon />,
        },
    });
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    useDynamicReducer(helpPanelReducerName, helpPanelReducer as Reducer, [helpPanelSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);
    usePersistentReducer(accessibilityReducerName, accessibilityReducer);
    getNotifications(); // to get notification on initial load
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const teachingBubbleVisibility = useSelector(getTeachingBubbleVisibility);
    const isPanelOpen = useSelector(getIsPanelOpen);
    const isSettingPanelOpen = useSelector(getIsSettingPanelOpen);

    const { isNotificationPanelOpen, unReadNotificationsCount } = useSelector(
        (state: INotificationsPanelState) =>
            state.dynamic?.[notificationsPanelReducerName] || notificationsPanelInitialState
    );
    const [headerConfig, setHeaderConfig] = React.useState<any>(null);
    const [showLoading, setShowLoading] = React.useState(true);

    React.useEffect(() => {
        const timer = setTimeout(() => setShowLoading(false), 12000);
        return () => {
            clearTimeout(timer);
        };
    }, []);

    const user = useUser();
    const userPhoto = useGraphPhoto();
    const isValidUser = !!user;

    React.useEffect(() => {
        setHeaderConfig({
            headerLabel: __APP_NAME__,
            appNameSettings: {
                label: __APP_NAME__,
            },
            farRightSettings: {
                additionalItems: [
                    {
                        key: 'alertNotifications',
                        text: 'Alert Notifications',
                        ariaLabel: 'Alert Notifications',
                        onRenderIcon: () => {
                            return (
                                <div>
                                    {unReadNotificationsCount > 0 && (
                                        <div className={notificationStyled.badgeClass}>{unReadNotificationsCount}</div>
                                    )}
                                    <FontIcon iconName="Ringer" className={notificationStyled.bellIconClass} />
                                </div>
                            );
                        },
                        checked: isNotificationPanelOpen,
                        onClick: () => {
                            dispatch(toggleNotificationPanel());
                        },
                    },
                ],
                profileSettings: {
                    buttonSettings: {
                        title: 'Profile',
                        ariaLabel: 'Profile',
                    },
                    panelSettings: {
                        fullName: user?.name || '',
                        emailAddress: user?.email || '',
                        imageUrl: userPhoto || '',
                        logOutLink: '#',
                    },
                },
                helpSettings: {
                    panelSettings: {
                        body: <HelpPanel />,
                        titleText: 'Help',
                        ...(teachingBubbleVisibility && { isOpen: false }),
                    },
                },
                settingsSettings: {
                    panelSettings: {
                        body: <UserSettings />,
                        titleText: 'Settings',
                        ...(teachingBubbleVisibility && { isOpen: isSettingPanelOpen }),
                    },
                },
            },
        });
    }, [
        dispatch,
        isSettingPanelOpen,
        teachingBubbleVisibility,
        unReadNotificationsCount,
        isNotificationPanelOpen,
        user,
        userPhoto,
    ]);

    const loginMessage = (
        <SharedStyled.HeightBelowShell windowHeight={window.innerHeight} windowWidth={window.innerWidth}>
            {showLoading ? (
                <SharedStyled.SpinnerContainer>
                    <Spinner label="Logging in..." />
                </SharedStyled.SpinnerContainer>
            ) : (
                <ErrorResult message="A login error occured, please login with a valid Microsoft account." />
            )}
        </SharedStyled.HeightBelowShell>
    );

    /* for internal telemetry logging
    initializeOBFeedback(
        __OCV_APP_ID__,
        __OCV_ENVIRONMENT_NAME__,
        '/ocv/scripts/officebrowserfeedback.min.js',
        '/ocv/styles/officebrowserfeedback.min.css',
        '/ocv/intl/',
        'https://eesideas.powerappsportals.com/d365community/forum/aba5cb3a-a022-ec11-b6e6-000d3a339bda',
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        __OCV_TELEMETRY_GROUP__,
        undefined,
        undefined,
        undefined,
        undefined,
        false
    ).then(() => {});
    */

    return (
        <div>
            <AccessibilityPanel />
            <HelpPanel />
            <UserSettingsPanel />
            <NotificationPanelCustom />
            <ProfilePanel />
            <BrowserRouter>
                <TopHeader upn={user?.email} displayName={user?.name} />
                <SideNav links={navConfig} />
                <div>
                    <Stack horizontal className={isPanelOpen ? 'ms-hiddenSm' : ''}>
                        <Stack.Item grow>
                            <SecondaryHeader />
                        </Stack.Item>
                    </Stack>
                    <Main id="main" tabIndex={-1} role="main">
                        {isValidUser ? <Routes /> : loginMessage}
                    </Main>
                </div>
            </BrowserRouter>
        </div>
    );
}

render(
    <ShellWithStore>
        <App />
    </ShellWithStore>,
    document.getElementById('app')
);
