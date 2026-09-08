import * as React from 'react';
import { Dialog, DialogType, DialogFooter } from '@fluentui/react/lib/Dialog';
import { PrimaryButton, DefaultButton } from '@fluentui/react/lib/Button';
import { Checkbox } from '@fluentui/react/lib/Checkbox';
import { Dropdown, IDropdownOption } from '@fluentui/react/lib/Dropdown';
import { Spinner, SpinnerSize } from '@fluentui/react/lib/Spinner';
import { Stack } from '@fluentui/react/lib/Stack';

interface IConsentDialogProps {
    isOpen: boolean;
    onDismiss: () => void;
    onSuccess: (rememberFor?: number) => void;
    onPopupFailed?: (rememberFor?: number) => void;
    actionId: string;
    actionLabel: string;
    authClient: any;
    rememberForOptions?: IDropdownOption[];
}

const defaultRememberForOptions: IDropdownOption[] = [
    { key: 120, text: '2 minutes' },
    { key: 300, text: '5 minutes' },
    { key: 900, text: '15 minutes' },
    { key: 0, text: 'Only this action' },
];

export const ConsentDialog = (props: IConsentDialogProps): JSX.Element => {
    const { isOpen, onDismiss, onSuccess, actionId, actionLabel, authClient } = props;
    const options = props.rememberForOptions?.length > 0 ? props.rememberForOptions : defaultRememberForOptions;
    const [agree, setAgree] = React.useState(false);
    const [rememberFor, setRememberFor] = React.useState<number>(options[0]?.key as number ?? 900);
    const [loading, setLoading] = React.useState(false);

    React.useEffect(() => {
        if (isOpen) {
            setRememberFor(options[0]?.key as number ?? 900);
            setAgree(false);
            setLoading(false);
        }
    }, [isOpen, props.rememberForOptions]);

    const onConfirm = async () => {
        if (!agree) return;
        setLoading(true);
        // Mobile browsers block or hang the MSAL popup — skip straight to the redirect fallback.
        const isMobileBrowser = typeof navigator !== 'undefined' && /Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent);
        if (isMobileBrowser && props.onPopupFailed) {
            props.onPopupFailed(rememberFor);
            return;
        }
        // call loginPopup in authclientv2.js
        try {
            // prompt: 'login' forces the user to enter credentials, ignoring cached tokens
            await authClient.loginPopup({ prompt: 'login', scopes: [`${__RESOURCE_URL__}/.default`] });
            if (onSuccess) {
                onSuccess(rememberFor);
            }
            setLoading(false);
            onDismiss();
        } catch (error) {
            console.error('Consent grant failed:', error);
            // Only fall back to redirect when the popup itself is blocked/unsupported.
            const errorCode = (error as any)?.errorCode ?? (error as any)?.code;
            const message = String((error as any)?.message ?? '').toLowerCase();
            const isPopupBlocked =
                errorCode === 'popup_window_error' ||
                errorCode === 'empty_window_error' ||
                (message.includes('popup') && (message.includes('blocked') || message.includes('not supported')));
            if (isPopupBlocked && props.onPopupFailed) {
                props.onPopupFailed(rememberFor);
                return;
            }
            setLoading(false);
        }
    }

    const dialogContentProps = {
        type: DialogType.normal,
        title: `Confirm identity for: ${actionLabel}`,
        subText: 'This action requires confirming your presence. We will record a minimal proof (proofId, actionId, timestamp, method). We will NOT store your authentication tokens or private keys.',
    };

    return (
        <Dialog
            hidden={!isOpen}
            onDismiss={onDismiss}
            dialogContentProps={dialogContentProps}
            minWidth={400}
        >
            <Stack tokens={{ childrenGap: 15 }}>
                <Checkbox
                    label="I consent to provide proof of presence for this action"
                    checked={agree}
                    onChange={(e, checked) => setAgree(!!checked)}
                />
                <Dropdown
                    label="Remember for:"
                    selectedKey={rememberFor}
                    options={options}
                    onChange={(e, option) => setRememberFor(option?.key as number)}
                />
                {loading && <Spinner size={SpinnerSize.medium} label="Processing..." />}
            </Stack>
            <DialogFooter>
                <PrimaryButton onClick={onConfirm} disabled={!agree || loading} text="Confirm & Continue" />
                <DefaultButton onClick={onDismiss} disabled={loading} text="Cancel" />
            </DialogFooter>
        </Dialog>
    );
};
