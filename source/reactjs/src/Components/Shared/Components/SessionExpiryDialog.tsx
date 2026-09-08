import * as React from 'react';
import { Dialog, DialogType, DialogFooter } from '@fluentui/react/lib/Dialog';
import { PrimaryButton } from '@fluentui/react/lib/Button';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

const modalPropsStyles = { main: { maxWidth: 450 } };
const dialogContentProps = {
    type: DialogType.normal,
    title: 'Session Expired',
    subText: 'Sorry, your session has expired. Please refresh to continue.',
    showCloseButton: false,
};

export const SessionExpiryDialog: React.FunctionComponent = () => {
    const isSessionExpired = window.sessionStorage.getItem('isSessionExpired') == 'true' || false;
    const isHidden = !isSessionExpired;
    const { authClient } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const modalProps = {
        isBlocking: true,
        styles: modalPropsStyles,
    };
    const handleRefresh = () => authClient.login();

    return (
        <Dialog hidden={isHidden} dialogContentProps={dialogContentProps} modalProps={modalProps}>
            <DialogFooter>
                <PrimaryButton onClick={handleRefresh} text="Refresh" />
            </DialogFooter>
        </Dialog>
    );
};
