import { Panel } from '@fluentui/react';
import * as React from 'react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsSettingPanelOpen } from '../Shared/SharedComponents.selectors';
import { UserSettings } from '../Shared/Components/UserSettings';
import { toggleSettingsPanel } from '../Shared/SharedComponents.actions';

export function UserSettingsPanel(): React.ReactElement {
    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const isSettingPanelOpen = useSelector(getIsSettingPanelOpen);

    const handleSettingsDismiss = (): void => {
        dispatch(toggleSettingsPanel(false));
    };

    return (
        <Panel
            isLightDismiss
            isOpen={isSettingPanelOpen}
            onDismiss={handleSettingsDismiss}
            closeButtonAriaLabel="Close settings panel"
            headerText="Settings"
        >
            <UserSettings />
        </Panel>
    );
}
