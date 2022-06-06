import { Panel } from '@fluentui/react';
import * as React from 'react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsHelpPanelOpen } from './HelpPanel.selectors';
import HelpPanelContent from './HelpPanelContent';
import { updateHelpPanelState } from './HelpPanel.actions';

export function HelpPanel(): React.ReactElement {
    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const isHelpPanelOpen = useSelector(getIsHelpPanelOpen);

    const handleHelpDismiss = (): void => {
        dispatch(updateHelpPanelState(false));
    };

    return (
        <Panel
            isLightDismiss
            isOpen={isHelpPanelOpen}
            onDismiss={handleHelpDismiss}
            closeButtonAriaLabel="Close help panel"
            headerText="Help"
        >
            <HelpPanelContent />
        </Panel>
    );
}
