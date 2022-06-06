import * as React from 'react';
import { Label, PrimaryButton, Stack } from '@fluentui/react';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getUserAlias } from '../SharedComponents.persistent-selectors';
import {
    refreshBulkState,
    requestMySummary,
    updatePanelState,
    updateIsProcessingBulkApproval,
} from '../SharedComponents.actions';
import { useHistory, useLocation } from 'react-router-dom';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

export function ContinueMessage(props: { isBulkAction: boolean; customLabel?: string }): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const isBulkAction = props.isBulkAction;
    const customLabel = props.customLabel;
    const userAlias = useSelector(getUserAlias);
    const history = useHistory();
    const location = useLocation();

    function handleContinueClick(): void {
        if (history && location) {
            if (location.pathname.length > 1) {
                history.push('/');
            }
        }
        if (isBulkAction) {
            dispatch(updateIsProcessingBulkApproval(false));
            dispatch(refreshBulkState());
            dispatch(requestMySummary(userAlias));
        } else {
            dispatch(updatePanelState(false));
            dispatch(requestMySummary(userAlias));
        }
    }

    return (
        <div
            style={{
                height: '100%',
                width: '100%',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                outline: 'none',
            }}
            ref={(input) => input && input.focus()}
            tabIndex={0}
        >
            <Stack tokens={{ childrenGap: '10' }}>
                <Stack.Item>
                    <Label>
                        {customLabel ?? 'Please wait for confirmation or click continue to work on other requests.'}
                    </Label>
                </Stack.Item>
                <Stack.Item align={isBulkAction ? 'end' : 'center'}>
                    <PrimaryButton ariaLabel="Continue" text="Continue" onClick={handleContinueClick} />
                </Stack.Item>
            </Stack>
        </div>
    );
}
