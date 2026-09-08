import * as React from 'react';
import {
    Stack,
    MessageBar,
    MessageBarType,
    Spinner,
    SpinnerSize,
    Text,
    PrimaryButton,
    DefaultButton,
    TextField,
    DetailsList,
    DetailsListLayoutMode,
    SelectionMode,
    IColumn,
    IconButton,
    Dropdown,
    IDropdownOption,
} from '@fluentui/react';
import { IHttpClient } from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { fetchApi, patchApi } from './AdminPage.api';
import { GLOBAL_ADMIN_ROLE, TENANT_USER_ROLE, textFieldStyles, dropdownStyles, tabContentStyles } from './AdminPage.constants';
import {
    ITenantInfo,
    IRoleAssignmentSummary,
    IUpdateRoleAssignmentsRequest,
    IUpdateRoleAssignmentsResponse,
    IOperationResult,
} from './AdminPage.types';
import { extractErrorMessage } from '../Shared/Utils/ErrorUtils';

interface ManageUsersTabProps {
    selectedTenantId: string | null;
    httpClient: IHttpClient;
    tenants: ITenantInfo[];
    selectedTenantRole?: string;
}

export function ManageUsersTab({ selectedTenantId, httpClient, tenants, selectedTenantRole }: ManageUsersTabProps): React.ReactElement {
    const [roleAssignments, setRoleAssignments] = React.useState<IRoleAssignmentSummary[]>([]);
    const [roleAssignmentsLoading, setRoleAssignmentsLoading] = React.useState(false);
    const [pendingAdds, setPendingAdds] = React.useState<string[]>([]);
    const [pendingDeletes, setPendingDeletes] = React.useState<Set<string>>(new Set());
    const [newUpnInput, setNewUpnInput] = React.useState('');
    const [assignSaving, setAssignSaving] = React.useState(false);
    const [assignResult, setAssignResult] = React.useState<IOperationResult | null>(null);
    const [selectedRole, setSelectedRole] = React.useState<string>(TENANT_USER_ROLE);

    const isGlobalAdmin = selectedTenantRole === GLOBAL_ADMIN_ROLE;
    const roleOptions: IDropdownOption[] = [
        { key: 'TenantUser', text: 'TenantUser' },
        { key: 'TenantAdmin', text: 'TenantAdmin' },
    ];

    const requestVersionRef = React.useRef(0);
    const canManageUsers = selectedTenantRole !== TENANT_USER_ROLE;

    React.useEffect(() => {
        if (!selectedTenantId) return;

        const version = ++requestVersionRef.current;

        const load = async () => {
            try {
                setRoleAssignmentsLoading(true);
                setPendingAdds([]);
                setPendingDeletes(new Set());
                setNewUpnInput('');
                setAssignResult(null);

                const data = await fetchApi<IRoleAssignmentSummary[]>(
                    httpClient,
                    `/admin/tenants/${encodeURIComponent(selectedTenantId)}/role-assignments`
                );
                if (version !== requestVersionRef.current) return;
                setRoleAssignments(data);
            } catch (err: any) {
                if (version !== requestVersionRef.current) return;
                setRoleAssignments([]);
                setAssignResult({ success: false, message: extractErrorMessage(err, 'Failed to load role assignments.') });
            } finally {
                if (version === requestVersionRef.current) setRoleAssignmentsLoading(false);
            }
        };

        load();
    }, [selectedTenantId, httpClient]);

    const refreshRoleAssignments = async () => {
        if (!selectedTenantId) return;
        const version = ++requestVersionRef.current;

        try {
            setRoleAssignmentsLoading(true);
            setPendingAdds([]);
            setPendingDeletes(new Set());
            setNewUpnInput('');

            const data = await fetchApi<IRoleAssignmentSummary[]>(
                httpClient,
                `/admin/tenants/${encodeURIComponent(selectedTenantId)}/role-assignments`
            );
            if (version !== requestVersionRef.current) return;
            setRoleAssignments(data);
        } catch (err: any) {
            if (version !== requestVersionRef.current) return;
            setRoleAssignments([]);
            setAssignResult({ success: false, message: extractErrorMessage(err, 'Failed to load role assignments.') });
        } finally {
            if (version === requestVersionRef.current) setRoleAssignmentsLoading(false);
        }
    };

    const handleAddPendingRow = () => {
        const upn = newUpnInput.trim();
        if (!upn) return;
        if (pendingAdds.includes(upn)) return;
        setPendingAdds((prev) => [...prev, upn]);
        setNewUpnInput('');
        setAssignResult(null);
    };

    const handleRemovePendingAdd = (upn: string) => {
        setPendingAdds((prev) => prev.filter((u) => u !== upn));
    };

    const handleToggleDelete = (userObjectId: string) => {
        setPendingDeletes((prev) => {
            const next = new Set(prev);
            if (next.has(userObjectId)) {
                next.delete(userObjectId);
            } else {
                next.add(userObjectId);
            }
            return next;
        });
        setAssignResult(null);
    };

    const hasRoleChanges = pendingAdds.length > 0 || pendingDeletes.size > 0;

    const handleUpdateRoleAssignments = async () => {
        if (!selectedTenantId || !hasRoleChanges) return;

        const allowedRoles = roleOptions.map((option) => option.key as string);
        if (!allowedRoles.includes(selectedRole)) {
            setAssignResult({
                success: false,
                message: 'You are not authorized to assign the selected role.',
            });
            return;
        }

        try {
            setAssignSaving(true);
            setAssignResult(null);

            const selectedTenant = tenants.find((t) => t.documentTypeId === selectedTenantId);
            const response = await patchApi<IUpdateRoleAssignmentsResponse>(
                httpClient,
                `/admin/tenants/${encodeURIComponent(selectedTenantId)}/role-assignments`,
                {
                    adds: pendingAdds,
                    deletes: Array.from(pendingDeletes),
                    appName: selectedTenant?.appName,
                    role: selectedRole,
                } as IUpdateRoleAssignmentsRequest
            );

            const failures = response.results.filter((r) => !r.success);
            if (failures.length === 0) {
                setAssignResult({ success: true, message: 'All changes saved successfully.' });
            } else {
                const failMsgs = failures.map((f) => `${f.operation} ${f.userPrincipalName || f.userObjectId}: ${f.error}`).join('; ');
                setAssignResult({ success: false, message: `Some operations failed: ${failMsgs}` });
            }

            await refreshRoleAssignments();
        } catch (err: any) {
            setAssignResult({ success: false, message: extractErrorMessage(err, 'Failed to update role assignments.') });
        } finally {
            setAssignSaving(false);
        }
    };

    const handleResetRoleChanges = () => {
        setPendingAdds([]);
        setPendingDeletes(new Set());
        setNewUpnInput('');
        setAssignResult(null);
    };

    const roleAssignmentColumns: IColumn[] = [
        {
            key: 'upn',
            name: 'User Principal Name',
            fieldName: 'upn',
            minWidth: 200,
            maxWidth: 300,
            isResizable: true,
            onRender: (item: any) => (
                <span
                    style={{
                        textDecoration: item.isPendingDelete ? 'line-through' : 'none',
                        opacity: item.isPendingDelete ? 0.5 : 1,
                        fontStyle: !item.isExisting ? 'italic' : 'normal',
                        color: !item.isExisting ? '#0078d4' : undefined,
                    }}
                >
                    {item.upn || '—'}
                </span>
            ),
        },
        {
            key: 'role',
            name: 'Role',
            fieldName: 'role',
            minWidth: 100,
            maxWidth: 120,
            onRender: (item: any) => (
                <span style={{ opacity: item.isPendingDelete ? 0.5 : 1 }}>{item.role}</span>
            ),
        },
        {
            key: 'assignedBy',
            name: 'Assigned By',
            fieldName: 'assignedBy',
            minWidth: 150,
            maxWidth: 250,
            isResizable: true,
            onRender: (item: any) => (
                <span style={{ opacity: item.isPendingDelete ? 0.5 : 1 }}>{item.assignedBy || '—'}</span>
            ),
        },
        ...(canManageUsers ? [{
            key: 'actions',
            name: '',
            minWidth: 40,
            maxWidth: 40,
            onRender: (item: any) => {
                if (item.isExisting) {
                    return (
                        <IconButton
                            iconProps={{ iconName: item.isPendingDelete ? 'Undo' : 'Delete' }}
                            title={item.isPendingDelete ? 'Undo removal' : 'Remove admin'}
                            onClick={() => handleToggleDelete(item.userObjectId)}
                        />
                    );
                }
                return (
                    <IconButton
                        iconProps={{ iconName: 'Cancel' }}
                        title="Remove pending add"
                        onClick={() => handleRemovePendingAdd(item.upn)}
                    />
                );
            },
        }] : []),
    ];

    const roleAssignmentItems = React.useMemo(() => [
        ...roleAssignments.map((r) => ({
            key: r.userObjectId,
            upn: r.userPrincipalName,
            role: r.role,
            assignedBy: r.assignedBy,
            isExisting: true,
            isPendingDelete: pendingDeletes.has(r.userObjectId),
            userObjectId: r.userObjectId,
        })),
        ...pendingAdds.map((upn) => ({
            key: `pending-${upn}`,
            upn,
            role: selectedRole,
            assignedBy: '(pending)',
            isExisting: false,
            isPendingDelete: false,
            userObjectId: '',
        })),
    ], [roleAssignments, pendingDeletes, pendingAdds, selectedRole]);

    return (
        <Stack tokens={{ childrenGap: 16 }} styles={tabContentStyles}>
            <Text variant="small">
                {canManageUsers
                    ? 'View, add, and remove role assignments. Pending changes are highlighted — click Update to commit all changes at once.'
                    : 'You have read-only access to the user list for this tenant.'}
            </Text>

            {!canManageUsers && (
                <MessageBar messageBarType={MessageBarType.info}>
                    TenantUser role grants view-only access to user assignments.
                </MessageBar>
            )}

            {!selectedTenantId ? (
                <MessageBar messageBarType={MessageBarType.info}>
                    Select a tenant above to manage users.
                </MessageBar>
            ) : roleAssignmentsLoading ? (
                <Spinner size={SpinnerSize.small} label="Loading role assignments..." />
            ) : (
                <Stack tokens={{ childrenGap: 8 }}>
                    <DetailsList
                        items={roleAssignmentItems}
                        columns={roleAssignmentColumns}
                        layoutMode={DetailsListLayoutMode.fixedColumns}
                        selectionMode={SelectionMode.none}
                        isHeaderVisible={true}
                        compact={true}
                    />

                    {canManageUsers && (
                        <>
                            <Stack horizontal tokens={{ childrenGap: 8 }} verticalAlign="end">
                                <TextField
                                    label="Add new user"
                                    placeholder="e.g. johndoe@microsoft.com"
                                    value={newUpnInput}
                                    onChange={(_e, val) => setNewUpnInput(val || '')}
                                    onKeyDown={(e) => {
                                        if (e.key === 'Enter') handleAddPendingRow();
                                    }}
                                    styles={textFieldStyles}
                                />
                                {isGlobalAdmin && (
                                    <Dropdown
                                        label="Role"
                                        selectedKey={selectedRole}
                                        options={roleOptions}
                                        onChange={(_e, option) => option && setSelectedRole(option.key as string)}
                                        styles={dropdownStyles}
                                    />
                                )}
                                <IconButton
                                    iconProps={{ iconName: 'Add' }}
                                    title="Add to pending list"
                                    onClick={handleAddPendingRow}
                                    disabled={!newUpnInput.trim()}
                                    styles={{ root: { marginBottom: 2 } }}
                                />
                            </Stack>

                            <Stack horizontal tokens={{ childrenGap: 8 }}>
                                <PrimaryButton
                                    text="Update"
                                    onClick={handleUpdateRoleAssignments}
                                    disabled={assignSaving || !hasRoleChanges}
                                    styles={{ root: { maxWidth: 120 } }}
                                />
                                <DefaultButton
                                    text="Reset"
                                    onClick={handleResetRoleChanges}
                                    disabled={assignSaving || !hasRoleChanges}
                                    styles={{ root: { maxWidth: 100 } }}
                                />
                            </Stack>
                        </>
                    )}

                    {assignSaving && <Spinner size={SpinnerSize.small} label="Saving changes..." />}

                    {assignResult && (
                        <MessageBar
                            messageBarType={assignResult.success ? MessageBarType.success : MessageBarType.error}
                            onDismiss={() => setAssignResult(null)}
                        >
                            {assignResult.message}
                        </MessageBar>
                    )}
                </Stack>
            )}
        </Stack>
    );
}
