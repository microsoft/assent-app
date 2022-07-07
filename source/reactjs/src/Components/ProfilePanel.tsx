import { Panel, PersonaSize, Separator } from '@fluentui/react';
import * as React from 'react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsProfilePanelOpen, getProfile } from './Shared/SharedComponents.selectors';
import { ActionButton } from '@fluentui/react/lib/Button';
import { toggleProfilePanel } from './Shared/SharedComponents.actions';
import { Persona } from './Shared/Components/Persona';

export function ProfilePanel(): React.ReactElement {
    const { useSelector, dispatch, authClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const isProfilePanelOpen = useSelector(getIsProfilePanelOpen);
    const profile = useSelector(getProfile);

    const handleProfileDismiss = (): void => {
        dispatch(toggleProfilePanel(false));
    };

    const handleLogout = (): void => {
        authClient.logOut();
    };

    return (
        <Panel
            isLightDismiss
            isOpen={isProfilePanelOpen}
            onDismiss={handleProfileDismiss}
            closeButtonAriaLabel="Close profile panel"
            headerText="Profile"
        >
            <Persona
                emailAlias={profile?.userPrincipalName}
                size={PersonaSize.size56}
                text={profile?.displayName}
                secondaryText={profile?.userPrincipalName}
                styles={{ root: { paddingTop: '20px' } }}
            />
            <Separator />
            <ActionButton styles={{ label: { color: '#0078d4' }, root: { marginLeft: '-5px' } }} onClick={handleLogout}>
                Sign out
            </ActionButton>
        </Panel>
    );
}
