import * as React from 'react';
import {
    Stack,
    MessageBar,
    MessageBarType,
    Spinner,
    SpinnerSize,
    Text,
    Dropdown,
    PrimaryButton,
    DefaultButton,
    TextField,
    Toggle,
    DetailsList,
    DetailsListLayoutMode,
    SelectionMode,
    IColumn,
    IconButton,
} from '@fluentui/react';
import { IHttpClient } from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { fetchApi, patchApi } from './AdminPage.api';
import { SUBMISSION_TYPE_OPTIONS, BULK_EXTERNAL_OPTION, tabContentStyles } from './AdminPage.constants';
import { FieldLabel } from './FieldLabel';
import {
    ITenantActionSummary,
    ITenantSettingsResponse,
    IActionEdit,
    IOperationResult,
} from './AdminPage.types';
import { extractErrorMessage } from '../Shared/Utils/ErrorUtils';

interface TenantSettingsTabProps {
    selectedTenantId: string | null;
    httpClient: IHttpClient;
}

export function TenantSettingsTab({ selectedTenantId, httpClient }: TenantSettingsTabProps): React.ReactElement {
    const [actionDetailsLoading, setActionDetailsLoading] = React.useState(false);
    const [actionDetailsData, setActionDetailsData] = React.useState<ITenantSettingsResponse | null>(null);
    const [actionDetailsError, setActionDetailsError] = React.useState<string | null>(null);
    const [actionEdits, setActionEdits] = React.useState<Map<string, IActionEdit>>(new Map());
    const [editedAppName, setEditedAppName] = React.useState<string | null>(null);
    const [editedSubmissionType, setEditedSubmissionType] = React.useState<string | null>(null);
    const [editedBulkConcurrentCall, setEditedBulkConcurrentCall] = React.useState<string | null>(null);
    const [actionSaving, setActionSaving] = React.useState(false);
    const [actionSaveResult, setActionSaveResult] = React.useState<IOperationResult | null>(null);

    const requestVersionRef = React.useRef(0);

    React.useEffect(() => {
        if (!selectedTenantId) return;

        const version = ++requestVersionRef.current;

        const load = async () => {
            try {
                setActionDetailsLoading(true);
                setActionDetailsError(null);
                setActionDetailsData(null);
                setActionEdits(new Map());
                setEditedAppName(null);
                setEditedSubmissionType(null);
                setEditedBulkConcurrentCall(null);
                setActionSaveResult(null);

                const response = await fetchApi<ITenantSettingsResponse>(
                    httpClient,
                    `/admin/tenants/${encodeURIComponent(selectedTenantId)}/settings`
                );
                if (version !== requestVersionRef.current) return;
                setActionDetailsData(response);
            } catch (err: any) {
                if (version !== requestVersionRef.current) return;
                setActionDetailsError(extractErrorMessage(err, 'Failed to load tenant settings.'));
            } finally {
                if (version === requestVersionRef.current) {
                    setActionDetailsLoading(false);
                }
            }
        };

        load();
    }, [selectedTenantId, httpClient]);

    const editKey = (section: string, code: string) => `${section}::${code}`;

    const getEditValue = (action: ITenantActionSummary): IActionEdit => {
        const key = editKey(action.section, action.code);
        return actionEdits.get(key) || {
            section: action.section,
            code: action.code,
            isBulkAction: action.isBulkAction,
            commentLength: action.commentLength,
        };
    };

    const setEditValue = (action: ITenantActionSummary, field: 'isBulkAction' | 'commentLength', value: boolean | number) => {
        const key = editKey(action.section, action.code);
        const current = getEditValue(action);
        const updated = { ...current, [field]: value };
        setActionEdits((prev) => {
            const next = new Map(prev);
            next.set(key, updated);
            return next;
        });
        setActionSaveResult(null);
    };

    const isActionDirty = (action: ITenantActionSummary): boolean => {
        const key = editKey(action.section, action.code);
        const edit = actionEdits.get(key);
        if (!edit) return false;
        return edit.isBulkAction !== action.isBulkAction || edit.commentLength !== action.commentLength;
    };

    const isAppNameDirty = editedAppName !== null && editedAppName !== actionDetailsData?.appName;
    const isSubmissionTypeDirty = editedSubmissionType !== null && editedSubmissionType !== actionDetailsData?.actionSubmissionType;
    const parsedBulkConcurrentCall = editedBulkConcurrentCall !== null ? parseInt(editedBulkConcurrentCall, 10) : null;
    const isBulkConcurrentCallDirty = parsedBulkConcurrentCall !== null && !isNaN(parsedBulkConcurrentCall) && parsedBulkConcurrentCall !== actionDetailsData?.bulkActionConcurrentCall;
    const hasDirtyEdits = isAppNameDirty || isSubmissionTypeDirty || isBulkConcurrentCallDirty || (actionDetailsData?.actions?.some(isActionDirty) ?? false);

    const effectiveSubmissionType = editedSubmissionType ?? actionDetailsData?.actionSubmissionType ?? 'Single';
    const isSingleMode = effectiveSubmissionType === 'Single';

    // Include BulkExternal in the dropdown only if the tenant already uses it.
    const submissionTypeOptions = React.useMemo(() => {
        const currentType = actionDetailsData?.actionSubmissionType;
        if (currentType === 'BulkExternal') {
            return [...SUBMISSION_TYPE_OPTIONS, BULK_EXTERNAL_OPTION];
        }
        return SUBMISSION_TYPE_OPTIONS;
    }, [actionDetailsData?.actionSubmissionType]);

    const hasContradiction = React.useMemo(() => {
        if (effectiveSubmissionType !== 'Single' || !actionDetailsData?.actions) return false;
        return actionDetailsData.actions.some((action) => {
            const edit = getEditValue(action);
            return edit.isBulkAction;
        });
    }, [effectiveSubmissionType, actionDetailsData, actionEdits]);

    const handleSaveActionDetails = async () => {
        if (!selectedTenantId || !actionDetailsData) return;

        const changedActions = actionDetailsData.actions
            .filter(isActionDirty)
            .map((action) => {
                const edit = getEditValue(action);
                return {
                    section: action.section,
                    code: action.code,
                    isBulkAction: edit.isBulkAction !== action.isBulkAction ? edit.isBulkAction : null,
                    commentLength: edit.commentLength !== action.commentLength ? edit.commentLength : null,
                };
            });

        const payload: Record<string, unknown> = {};
        payload.eTag = actionDetailsData.eTag;
        if (isAppNameDirty) {
            payload.appName = editedAppName;
        }
        if (isSubmissionTypeDirty) {
            payload.actionSubmissionType = editedSubmissionType;
        }
        if (isBulkConcurrentCallDirty) {
            payload.bulkActionConcurrentCall = parsedBulkConcurrentCall;
        }
        if (changedActions.length > 0) {
            payload.actions = changedActions;
        }

        if (Object.keys(payload).length === 0) return;

        try {
            setActionSaving(true);
            setActionSaveResult(null);

            const response = await patchApi<ITenantSettingsResponse>(
                httpClient,
                `/admin/tenants/${encodeURIComponent(selectedTenantId)}/settings`,
                payload
            );

            setActionDetailsData(response);
            setActionEdits(new Map());
            setEditedAppName(null);
            setEditedSubmissionType(null);
            setEditedBulkConcurrentCall(null);

            const parts: string[] = [];
            if (isAppNameDirty) parts.push('app name');
            if (isSubmissionTypeDirty) parts.push('action submission type');
            if (isBulkConcurrentCallDirty) parts.push('bulk concurrent call limit');
            if (changedActions.length > 0) parts.push(`${changedActions.length} action(s)`);
            setActionSaveResult({
                success: true,
                message: `Successfully updated ${parts.join(' and ')}.`,
            });
        } catch (err: any) {
            const status = err?.status || err?.response?.status;
            let msg: string;
            if (status === 409) {
                msg = 'The tenant settings were modified by another user. Please refresh and try again.';
            } else {
                msg = extractErrorMessage(err, 'Failed to save tenant settings.');
            }
            setActionSaveResult({ success: false, message: msg });
        } finally {
            setActionSaving(false);
        }
    };

    const handleResetEdits = () => {
        setActionEdits(new Map());
        setEditedAppName(null);
        setEditedSubmissionType(null);
        setEditedBulkConcurrentCall(null);
        setActionSaveResult(null);
    };

    const actionDetailsColumns: IColumn[] = [
        {
            key: 'section',
            name: 'Section',
            fieldName: 'section',
            minWidth: 80,
            maxWidth: 100,
            isResizable: true,
        },
        {
            key: 'name',
            name: 'Name',
            fieldName: 'name',
            minWidth: 100,
            maxWidth: 160,
            isResizable: true,
        },
        {
            key: 'isBulkAction',
            name: 'Bulk Action',
            minWidth: 100,
            maxWidth: 140,
            onRenderHeader: () => (
                <FieldLabel
                    text="Bulk Action"
                    tooltip="When enabled, approvers can select multiple requests and perform this action in bulk from the summary view."
                />
            ),
            onRender: (item: ITenantActionSummary) => {
                const edit = getEditValue(item);
                const dirty = edit.isBulkAction !== item.isBulkAction;
                return (
                    <Toggle
                        checked={edit.isBulkAction}
                        onChange={(_e, checked) => setEditValue(item, 'isBulkAction', !!checked)}
                        styles={dirty ? { root: { marginBottom: 0 }, pill: { background: '#fff4ce', borderColor: '#c8a415' } } : { root: { marginBottom: 0 } }}
                    />
                );
            },
        },
        {
            key: 'commentLength',
            name: 'Comment Length',
            minWidth: 140,
            maxWidth: 170,
            onRenderHeader: () => (
                <FieldLabel
                    text="Comment Length"
                    tooltip="Maximum number of characters allowed in the approver's comment for this action. Set to 0 if no comment is required."
                />
            ),
            onRender: (item: ITenantActionSummary) => {
                const edit = getEditValue(item);
                const dirty = edit.commentLength !== item.commentLength;
                return (
                    <TextField
                        type="number"
                        min={0}
                        value={String(edit.commentLength)}
                        onChange={(_e, val) => {
                            const parsed = parseInt(val || '0', 10);
                            setEditValue(item, 'commentLength', isNaN(parsed) ? 0 : Math.max(0, parsed));
                        }}
                        styles={{
                            root: { width: 100 },
                            fieldGroup: dirty ? { borderColor: '#c8a415', background: '#fff4ce' } : undefined,
                        }}
                    />
                );
            },
        },
        {
            key: 'dirty',
            name: '',
            minWidth: 30,
            maxWidth: 30,
            onRender: (item: ITenantActionSummary) => {
                if (!isActionDirty(item)) return null;
                return (
                    <IconButton
                        iconProps={{ iconName: 'Undo' }}
                        title="Reset this action"
                        onClick={() => {
                            const key = editKey(item.section, item.code);
                            setActionEdits((prev) => {
                                const next = new Map(prev);
                                next.delete(key);
                                return next;
                            });
                        }}
                    />
                );
            },
        },
    ];

    return (
        <Stack tokens={{ childrenGap: 16 }} styles={tabContentStyles}>
            <Text variant="small">
                View and modify settings for a tenant. Changes to <strong>App Name</strong>,{' '}
                <strong>Action Submission Type</strong>, <strong>Bulk Action</strong>, and{' '}
                <strong>Comment Length</strong> are saved when you click Save. Modified fields are highlighted.
            </Text>

            {!selectedTenantId ? (
                <MessageBar messageBarType={MessageBarType.info}>
                    Select a tenant above to view settings.
                </MessageBar>
            ) : actionDetailsLoading ? (
                <Spinner size={SpinnerSize.small} label="Loading tenant settings..." />
            ) : actionDetailsError ? (
                <MessageBar
                    messageBarType={MessageBarType.error}
                    onDismiss={() => setActionDetailsError(null)}
                >
                    {actionDetailsError}
                </MessageBar>
            ) : actionDetailsData ? (
                <Stack tokens={{ childrenGap: 12 }}>
                    <TextField
                        label="Update App Name"
                        onRenderLabel={() => (
                            <FieldLabel
                                text="Update App Name"
                                tooltip="The display name of this tenant application shown to approvers throughout the Approvals portal."
                            />
                        )}
                        value={editedAppName !== null ? editedAppName : actionDetailsData.appName}
                        onChange={(_e, val) => {
                            setEditedAppName(val ?? '');
                            setActionSaveResult(null);
                        }}
                        styles={{
                            root: { width: 300 },
                            fieldGroup: isAppNameDirty
                                ? { borderColor: '#c8a415', background: '#fff4ce' }
                                : undefined,
                        }}
                    />

                    <Dropdown
                        label="Action Submission Type"
                        onRenderLabel={() => (
                            <FieldLabel
                                text="Action Submission Type"
                                tooltip="Controls how the backend sends bulk approvals to the tenant API. Single processes one at a time; Semi-Bulk loops per approval; Bulk sends all in one call."
                            />
                        )}
                        selectedKey={editedSubmissionType ?? actionDetailsData.actionSubmissionType}
                        options={submissionTypeOptions}
                        onChange={(_e, option) => {
                            if (option) {
                                setEditedSubmissionType(option.key as string);
                                if (option.key === 'Single') {
                                    setEditedBulkConcurrentCall(null);
                                }
                                setActionSaveResult(null);
                            }
                        }}
                        styles={{
                            dropdown: {
                                width: 300,
                                ...(isSubmissionTypeDirty
                                    ? { borderColor: '#c8a415', background: '#fff4ce' }
                                    : {}),
                            },
                        }}
                    />

                    {!isSingleMode && (
                        <TextField
                            label="Bulk Action Concurrent Calls"
                            onRenderLabel={() => (
                                <FieldLabel
                                    text="Bulk Action Concurrent Calls"
                                    tooltip="Maximum number of approvals a user can process at once in bulk mode. Controls the concurrency limit for batch operations."
                                />
                            )}
                            type="number"
                            min={1}
                            value={editedBulkConcurrentCall ?? String(actionDetailsData.bulkActionConcurrentCall)}
                            onChange={(_e, newValue) => {
                                setEditedBulkConcurrentCall(newValue ?? '');
                                setActionSaveResult(null);
                            }}
                            onBlur={() => {
                                if (editedBulkConcurrentCall !== null) {
                                    const parsed = parseInt(editedBulkConcurrentCall, 10);
                                    if (isNaN(parsed) || parsed < 1) {
                                        setEditedBulkConcurrentCall(String(actionDetailsData.bulkActionConcurrentCall));
                                    }
                                }
                            }}
                            styles={{
                                root: { width: 300 },
                                fieldGroup: {
                                    ...(isBulkConcurrentCallDirty
                                        ? { borderColor: '#c8a415', background: '#fff4ce' }
                                        : {}),
                                },
                            }}
                        />
                    )}

                    {hasContradiction && (
                        <MessageBar messageBarType={MessageBarType.severeWarning}>
                            <strong>Contradictory configuration:</strong> Action Submission Type is set to{' '}
                            <strong>Single</strong> but one or more actions have <strong>Bulk Action</strong>{' '}
                            enabled. Either change the submission type to Semi-Bulk/Bulk or disable Bulk Action
                            on those actions. The backend will reject this combination.
                        </MessageBar>
                    )}

                    {actionDetailsData.isExternalTenantActionDetails && (
                        <MessageBar messageBarType={MessageBarType.warning}>
                            This tenant uses external action details. Editing is disabled.
                        </MessageBar>
                    )}

                    {actionDetailsData.actions.length === 0 ? (
                        <MessageBar messageBarType={MessageBarType.info}>
                            No action details configured for this tenant.
                        </MessageBar>
                    ) : (
                        <>
                            <DetailsList
                                items={actionDetailsData.actions}
                                columns={actionDetailsColumns}
                                layoutMode={DetailsListLayoutMode.fixedColumns}
                                selectionMode={SelectionMode.none}
                                isHeaderVisible={true}
                                compact={true}
                            />

                            {!actionDetailsData.isExternalTenantActionDetails && (
                                <Stack horizontal tokens={{ childrenGap: 8 }}>
                                    <PrimaryButton
                                        text="Save Changes"
                                        onClick={handleSaveActionDetails}
                                        disabled={actionSaving || !hasDirtyEdits || hasContradiction}
                                        styles={{ root: { maxWidth: 150 } }}
                                    />
                                    <DefaultButton
                                        text="Reset"
                                        onClick={handleResetEdits}
                                        disabled={actionSaving || !hasDirtyEdits}
                                        styles={{ root: { maxWidth: 100 } }}
                                    />
                                </Stack>
                            )}

                            {actionSaving && <Spinner size={SpinnerSize.small} label="Saving..." />}

                            {actionSaveResult && (
                                <MessageBar
                                    messageBarType={
                                        actionSaveResult.success
                                            ? MessageBarType.success
                                            : MessageBarType.error
                                    }
                                    onDismiss={() => setActionSaveResult(null)}
                                >
                                    {actionSaveResult.message}
                                </MessageBar>
                            )}
                        </>
                    )}
                </Stack>
            ) : null}
        </Stack>
    );
}
