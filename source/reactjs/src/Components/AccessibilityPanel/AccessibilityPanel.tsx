import { Panel, Toggle } from '@fluentui/react';
import * as React from 'react';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsAccessibilityPanelOpen } from '../Shared/SharedComponents.selectors';
import { toggleAccessibilityPanel } from '../Shared/SharedComponents.actions';
import { toggleKeyboardColumnResizing } from './Accessibility.actions';
import { getIsKeyboardColumnResizingOn } from './Accessibility.selectors';

export function AccessibilityPanel(): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const isAccessibilityPanelOpen = useSelector(getIsAccessibilityPanelOpen);
    const isKeyboardColumnResizingOn = useSelector(getIsKeyboardColumnResizingOn);

    const handleColumnResizingToggle = (ev: React.MouseEvent<HTMLElement>, checked?: boolean) => {
        dispatch(toggleKeyboardColumnResizing(checked));
    };

    return (
        <Panel
            isLightDismiss
            isOpen={isAccessibilityPanelOpen}
            onDismiss={() => {
                dispatch(toggleAccessibilityPanel(false));
            }}
            closeButtonAriaLabel="Close accessibility panel"
            headerText="Accessibility"
        >
            <h4 style={{ margin: '10px 0px' }}>{'Table view'}</h4>
            <Toggle
                label="Keyboard column resizing"
                inlineLabel
                onText="On"
                offText="Off"
                checked={isKeyboardColumnResizingOn}
                onChange={handleColumnResizingToggle}
            />
        </Panel>
    );
}
