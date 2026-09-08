import * as React from 'react';
import * as HeaderStyled from './SecondaryHeaderStyling';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { Stack } from '@fluentui/react/lib/Stack';
import { IconButton, OverflowSet, PersonaSize, Text } from '@fluentui/react';
import {
    toggleAccessibilityPanel,
    toggleDetailsScreen,
    toggleProfilePanel,
    toggleSettingsPanel,
    postQuickTourInfo,
} from '../../SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { toggleNotificationPanel } from '../../../NotificationsPanel/NotificationsPanel.actions';
import { updateHelpPanelState } from '../../../HelpPanel/HelpPanel.actions';
import { getIsHelpPanelOpen } from '../../../HelpPanel/HelpPanel.selectors';
import { Persona } from '../Persona';
import { CommandBarButton } from '@fluentui/react/lib/Button';
import {
    getIsAccessibilityPanelOpen,
    getIsPanelOpen,
    getIsProfilePanelOpen,
    getHasUnreadLabsCoachmark,
    getItemsRead,
    getSelectedPage,
} from '../../SharedComponents.selectors';
import { SETTINGS_COACHMARK_ID } from '../../SharedConstants';
import { CoherenceColors } from '../../SharedColors';
import { IFeedback } from '../../../Feedback/IFeedback';
import { isMobileResolution } from '../../../../Helpers/sharedHelpers';
import { SearchBar } from '../Search/SearchBar';
import FlightingHandler from '../FlightingHandler';

const pulseIndicatorStyle: React.CSSProperties = {
    position: 'absolute',
    top: 6,
    right: 6,
    width: 8,
    height: 8,
    borderRadius: '50%',
    backgroundColor: '#FFB900',
    boxShadow: '0 0 0 2px ' + CoherenceColors.bluePrimary,
    pointerEvents: 'none',
};

const PulseIndicator: React.FC = () => <span aria-hidden className="ms-settings-pulse" style={pulseIndicatorStyle} />;

export function TopHeader(props: { upn: string; displayName: string; feedback: IFeedback }): React.ReactElement {
    const { upn, displayName, feedback } = props;
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const isHelpPanelOpen = useSelector(getIsHelpPanelOpen);
    const isProfilePanelOpen = useSelector(getIsProfilePanelOpen);
    const isAccessibilityPanelOpen = useSelector(getIsAccessibilityPanelOpen);
    const selectedPage = useSelector(getSelectedPage);
    const showSettingsPulse = useSelector(getHasUnreadLabsCoachmark);
    const readQuickTours = useSelector(getItemsRead);

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
    const [isSearchExpanded, setIsSearchExpanded] = React.useState(false);

    const handleSettingsClick = (): void => {
        dispatch(toggleSettingsPanel(true));
        // Merge labs id into the server-truth read list (InsertOrReplace) so the pulse clears permanently.
        if (showSettingsPulse && readQuickTours) {
            dispatch(postQuickTourInfo([...readQuickTours.map((i) => i.id.toString()), SETTINGS_COACHMARK_ID]));
        }
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
        try {
            feedback.launchFeedback();
        } catch (ex: any) {
            feedback?.handleFeedbackException(ex);
        }
    };

    const handleAccessibilityClick = (): void => {
        dispatch(toggleAccessibilityPanel(!isAccessibilityPanelOpen));
    };

    const headerItems = [
        {
            key: 'Accessibility',
            name: 'Accessibility',
            label: 'Accessibility',
            iconName: 'Accessibility',
            condition: true,
            onClick: handleAccessibilityClick,
            id: 'AcessibilityTopHeaderButton',
        },
        {
            key: 'Settings',
            name: 'Settings',
            label: 'Settings',
            iconName: 'Settings',
            condition: true,
            onClick: handleSettingsClick,
            id: 'SettingsTopHeaderButton',
        },
        {
            key: 'Notifications',
            name: 'Notifications',
            label: 'Notifications',
            iconName: 'Ringer',
            condition: true,
            onClick: handleNotificationsClick,
            id: 'NotificationsTopHeaderButton',
        },
        {
            key: 'Help',
            name: 'Help',
            label: 'Help',
            iconName: 'Help',
            condition: true,
            onClick: handleHelpClick,
            id: 'HelpTopHeaderButton',
        },
    ];

    const interactiveStyles = {
        rootHovered: { backgroundColor: CoherenceColors.blueInteractive },
        rootPressed: { backgroundColor: CoherenceColors.blueInteractive },
        rootDisabled: { backgroundColor: CoherenceColors.blueInteractive },
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
        const menuItems = (overflowItems ?? []).map((item) =>
            item.key === 'Settings' && showSettingsPulse
                ? {
                      ...item,
                      onRenderIcon: (props: any, defaultRender: any): React.ReactNode => (
                          <span style={{ position: 'relative', display: 'inline-flex' }}>
                              {defaultRender?.(props)}
                              <span
                                  aria-hidden
                                  style={{
                                      position: 'absolute',
                                      top: -2,
                                      right: -4,
                                      width: 8,
                                      height: 8,
                                      borderRadius: '50%',
                                      backgroundColor: '#FFB900',
                                  }}
                              />
                          </span>
                      ),
                  }
                : item,
        );
        const overflowButton = (
            <IconButton
                title="More options"
                styles={{
                    menuIcon: { color: 'white' },
                    rootExpanded: { backgroundColor: CoherenceColors.blueInteractive },
                    ...interactiveStyles,
                }}
                menuIconProps={{ iconName: 'More' }}
                menuProps={{ items: menuItems }}
            />
        );
        return showSettingsPulse ? (
            <span style={{ position: 'relative', display: 'inline-block' }}>
                {overflowButton}
                <PulseIndicator />
            </span>
        ) : (
            overflowButton
        );
    };

    const renderOverflowSet = () => {
        return (
            <OverflowSet
                aria-label="Menu"
                overflowItems={headerItems.filter((item) => item.condition)}
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
                {headerItems.map((item) => {
                    const buttonComponent = (
                        <IconButton
                            iconProps={item.iconSrc ? null : { iconName: item.iconName }}
                            styles={{ icon: { color: 'white' }, ...interactiveStyles }}
                            onClick={item.onClick}
                            title={item.name}
                            id={item.id}
                            aria-label={item.label}
                        >
                            {item.iconSrc && <img title={item.iconAlt} src={item?.iconSrc}></img>}
                        </IconButton>
                    );
                    const withPulse =
                        item.key === 'Settings' && showSettingsPulse ? (
                            <span style={{ position: 'relative', display: 'inline-block' }}>
                                {buttonComponent}
                                <PulseIndicator />
                            </span>
                        ) : (
                            buttonComponent
                        );
                    const flightingCheckedComponent = item.flightingName ? (
                        <FlightingHandler featureName={item.flightingName}>{withPulse}</FlightingHandler>
                    ) : (
                        withPulse
                    );
                    return item.condition && flightingCheckedComponent;
                })}
            </Stack>
        );
    };

    return (
        <HeaderStyled.SecondaryHeaderContainer isTopHeader role="banner">
            <Stack
                horizontal
                horizontalAlign="space-between"
                styles={HeaderStyled.SecondaryHeaderStackStyles(false, true)}
            >
                {!isSearchExpanded && (
                    <Stack.Item styles={{ root: { paddingLeft: '1%' } }}>
                        <HeaderStyled.topHeaderTitleLink href="/"> MSApprovals </HeaderStyled.topHeaderTitleLink>
                    </Stack.Item>
                )}
                {selectedPage === 'summary' && (
                    <Stack.Item
                        align="center"
                        grow={isSearchExpanded ? 1 : undefined}
                        styles={{
                            root: isSearchExpanded ? { padding: '0 8px' } : { paddingLeft: dimensions.width * 0.15 },
                        }}
                    >
                        <SearchBar screenWidth={dimensions.width} onExpandedChange={setIsSearchExpanded} />
                    </Stack.Item>
                )}
                {!isSearchExpanded && (
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
                            <CommandBarButton
                                onClick={handleProfileClick}
                                title="Profile"
                                styles={{
                                    ...interactiveStyles,
                                    root: {
                                        backgroundColor: CoherenceColors.bluePrimary,
                                        ...interactiveStyles.root,
                                    },
                                }}
                            >
                                <Persona
                                    emailAlias={upn}
                                    size={PersonaSize.size32}
                                    styles={{ root: { width: '32px' } }}
                                />
                            </CommandBarButton>
                        </Stack>
                    </Stack.Item>
                )}
            </Stack>
        </HeaderStyled.SecondaryHeaderContainer>
    );
}
