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
import { IconButton, PersonaSize, Text } from '@fluentui/react';
import { toggleProfilePanel, toggleSettingsPanel } from '../../SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsNotficationsPanelOpen } from '../../../NotificationsPanel/NotificationsPanel.selectors';
import { toggleNotificationPanel } from '../../../NotificationsPanel/NotificationsPanel.actions';
import { updateHelpPanelState } from '../../../HelpPanel/HelpPanel.actions';
import { getIsHelpPanelOpen } from '../../../HelpPanel/HelpPanel.selectors';
import { Persona } from '../Persona';
import { CommandBarButton } from '@fluentui/react/lib/Button';
import { getIsProfilePanelOpen } from '../../SharedComponents.selectors';
import { CoherenceColors } from '../../SharedColors';

export function TopHeader(props: { upn: string; displayName: string }): React.ReactElement {
    const { upn, displayName } = props;
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const isHelpPanelOpen = useSelector(getIsHelpPanelOpen);
    const isProfilePanelOpen = useSelector(getIsProfilePanelOpen);

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

    const interactiveStyles = {
        rootHovered: { backgroundColor: CoherenceColors.blueInteractive },
        rootPressed: { backgroundColor: CoherenceColors.blueInteractive },
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
                            styles={{
                                root: { backgroundColor: CoherenceColors.bluePrimary },
                                ...interactiveStyles,
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
