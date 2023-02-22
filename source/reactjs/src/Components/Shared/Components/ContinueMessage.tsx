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

const BUTTON_MESSAGE = 'click continue to work on other requests';

export function ContinueMessage(props: { isBulkAction: boolean; isActionCompleted?: boolean }): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const isBulkAction = props.isBulkAction;
    const isActionCompleted = props.isActionCompleted;
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

    const continueLabel = 'Please ' + (isActionCompleted ? '' : 'wait for confirmation or ') + BUTTON_MESSAGE;

    const continueAriaLabel = isActionCompleted
        ? 'Your action has successfully completed. ' + continueLabel
        : continueLabel;

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
        >
            <Stack tokens={{ childrenGap: '10' }}>
                <Stack.Item>
                    <Label>{continueLabel}</Label>
                </Stack.Item>
                <Stack.Item align={isBulkAction ? 'end' : 'center'}>
                    <PrimaryButton
                        ariaLabel={continueAriaLabel}
                        text="Continue"
                        onClick={handleContinueClick}
                        componentRef={(input: { focus: () => any }) => {
                            input && input.focus();
                        }}
                    />
                </Stack.Item>
            </Stack>
        </div>
    );
}
