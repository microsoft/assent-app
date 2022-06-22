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
import { IconButton, Text } from '@fluentui/react';
import { toggleSettingsPanel } from '../../SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsNotficationsPanelOpen } from '../../../NotificationsPanel/NotificationsPanel.selectors';
import { toggleNotificationPanel } from '../../../NotificationsPanel/NotificationsPanel.actions';
import { updateHelpPanelState } from '../../../HelpPanel/HelpPanel.actions';
import { getIsHelpPanelOpen } from '../../../HelpPanel/HelpPanel.selectors';

export function TopHeader(): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const isHelpPanelOpen = useSelector(getIsHelpPanelOpen);

    const handleSettingsClick = (): void => {
        dispatch(toggleSettingsPanel(true));
    };

    const handleNotificationsClick = (): void => {
        dispatch(toggleNotificationPanel());
    };

    const handleHelpClick = (): void => {
        dispatch(updateHelpPanelState(!isHelpPanelOpen));
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
                            styles={{ icon: { color: 'white' } }}
                            onClick={handleSettingsClick}
                            title="Settings"
                        />
                        <IconButton
                            iconProps={{ iconName: 'Ringer' }}
                            styles={{ icon: { color: 'white' } }}
                            onClick={handleNotificationsClick}
                            title="Notifications"
                        />
                        <IconButton
                            iconProps={{ iconName: 'Help' }}
                            styles={{ icon: { color: 'white' } }}
                            onClick={handleHelpClick}
                            title="Help"
                        />
                    </Stack>
                </Stack.Item>
            </Stack>
        </HeaderStyled.SecondaryHeaderContainer>
    );
}
