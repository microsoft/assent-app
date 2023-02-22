import * as React from 'react';
import * as HeaderStyled from './SecondaryHeaderStyling';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../../SharedComponents.reducer';
import { IComponentsAppState } from '../../SharedComponents.types';
import { sharedComponentsSagas } from '../../SharedComponents.sagas';
import { Reducer } from 'redux';
import { Stack } from '@fluentui/react/lib/Stack';
import { IconButton, OverflowSet, PersonaSize, Text } from '@fluentui/react';
import { toggleAccessibilityPanel, toggleProfilePanel, toggleSettingsPanel } from '../../SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsNotficationsPanelOpen } from '../../../NotificationsPanel/NotificationsPanel.selectors';
import { toggleNotificationPanel } from '../../../NotificationsPanel/NotificationsPanel.actions';
import { updateHelpPanelState } from '../../../HelpPanel/HelpPanel.actions';
import { getIsHelpPanelOpen } from '../../../HelpPanel/HelpPanel.selectors';
import { Persona } from '../Persona';
import { CommandBarButton } from '@fluentui/react/lib/Button';
import { getIsAccessibilityPanelOpen, getIsProfilePanelOpen } from '../../SharedComponents.selectors';
import { CoherenceColors } from '../../SharedColors';
import { IFeedback } from '../../../Feedback/IFeedback';
import { isMobileResolution } from '../../../../Helpers/sharedHelpers';

export function TopHeader(props: { upn: string; displayName: string; feedback: IFeedback }): React.ReactElement {
    const { upn, displayName, feedback } = props;
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const isHelpPanelOpen = useSelector(getIsHelpPanelOpen);
    const isProfilePanelOpen = useSelector(getIsProfilePanelOpen);
    const isAccessibilityPanelOpen = useSelector(getIsAccessibilityPanelOpen);

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

    const isMobile = isMobileResolution(dimensions.width);

    const handleSettingsClick = (): void => {
        dispatch(toggleSettingsPanel(true));
    };

    const handleProfileClick = (): void => {
        dispatch(toggleProfilePanel(!isProfilePanelOpen));
    };

    const handleNotificationsClick = (): void => {
        dispatch(toggleNotificationPanel());
    };

    const handleHelpClick = (): void => {
        dispatch(updateHelpPanelState(!isHelpPanelOpen));
    };

    const handleFeedbackClick = (): void => {
        feedback.launchFeedback();
    };

    const handleAccessibilityClick = (): void => {
        dispatch(toggleAccessibilityPanel(!isAccessibilityPanelOpen));
    };

    const headerItems = [
        {
            key: 'Accessibility',
            name: 'Accessibility',
            iconName: 'Accessibility',
            onClick: handleAccessibilityClick,
        },
        {
            key: 'Settings',
            name: 'Settings',
            iconName: 'Settings',
            onClick: handleSettingsClick,
        },
        {
            key: 'Notifications',
            name: 'Notifications',
            iconName: 'Ringer',
            onClick: handleNotificationsClick,
        },
        {
            key: 'Help',
            name: 'Help',
            iconName: 'Help',
            onClick: handleHelpClick,
        },
    ];

    const interactiveStyles = {
        rootHovered: { backgroundColor: CoherenceColors.blueInteractive },
        rootPressed: { backgroundColor: CoherenceColors.blueInteractive },
        root: {
            selectors: {
                ':focus': {
                    border: '1px solid black',
                },
                ':focus::after': { outline: 'none !important' },
            },
        },
    };

    const onRenderOverflowButton = (overflowItems: any[] | undefined): JSX.Element => {
        return (
            <IconButton
                title="More options"
                styles={{
                    menuIcon: { color: 'white' },
                    rootExpanded: { backgroundColor: CoherenceColors.blueInteractive },
                    ...interactiveStyles,
                }}
                menuIconProps={{ iconName: 'More' }}
                menuProps={{ items: overflowItems }}
            />
        );
    };

    const renderOverflowSet = () => {
        return (
            <OverflowSet
                aria-label="Menu"
                overflowItems={headerItems}
                onRenderOverflowButton={onRenderOverflowButton}
                onRenderItem={() => {
                    return null;
                }}
            />
        );
    };

    const renderAllButtons = () => {
        return (
            <Stack horizontal tokens={{ childrenGap: 'm' }}>
                {headerItems.map((item) => (
                    <IconButton
                        iconProps={{ iconName: item.iconName }}
                        styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                        onClick={item.onClick}
                        title={item.name}
                    />
                ))}
            </Stack>
        );
    };

    return (
        <HeaderStyled.SecondaryHeaderContainer isTopHeader>
            <Stack
                horizontal
                horizontalAlign="space-between"
                styles={HeaderStyled.SecondaryHeaderStackStyles(false, true)}
            >
                <Stack.Item styles={{ root: { paddingLeft: '1%' } }}>
                    <HeaderStyled.topHeaderTitleLink href="/"> MSApprovals </HeaderStyled.topHeaderTitleLink>
                </Stack.Item>
                <Stack.Item align="center" styles={{ root: { paddingRight: '0.5%' } }}>
                    <Stack horizontal tokens={{ childrenGap: 'm' }}>
                        {isMobile ? renderOverflowSet() : renderAllButtons()}
                        {feedback && (
                            <IconButton
                                iconProps={{ iconName: 'Emoji2' }}
                                styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                                onClick={handleFeedbackClick}
                                title="Feedback"
                            />
                        )}
                        <IconButton
                            iconProps={{ iconName: 'Accessibility' }}
                            styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                            onClick={handleAccessibilityClick}
                            title="Accessibility"
                        />
                        <IconButton
                            iconProps={{ iconName: 'Settings' }}
                            styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                            onClick={handleSettingsClick}
                            title="Settings"
                        />
                        <IconButton
                            iconProps={{ iconName: 'Ringer' }}
                            styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                            onClick={handleNotificationsClick}
                            title="Notifications"
                        />
                        <IconButton
                            iconProps={{ iconName: 'Help' }}
                            styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                            onClick={handleHelpClick}
                            title="Help"
                        />
                        <CommandBarButton
                            onClick={handleProfileClick}
                            title= "Profile"
                            styles={{
                                ...interactiveStyles,
                                root: { backgroundColor: CoherenceColors.bluePrimary, ...interactiveStyles.root },
                            }}
                        >
                            <Persona emailAlias={upn} size={PersonaSize.size32} styles={{ root: { width: '32px' } }} />
                        </CommandBarButton>
                    </Stack>
                </Stack.Item>
            </Stack>
        </HeaderStyled.SecondaryHeaderContainer>
    );
}
