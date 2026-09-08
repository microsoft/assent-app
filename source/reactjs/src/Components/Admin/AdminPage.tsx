import * as React from 'react';
import { usePageTracking } from '@micro-frontend-react/employee-experience/lib/usePageTracking';
import { usePageTitle } from '@micro-frontend-react/employee-experience/lib/usePageTitle';
import { getFeature, getPageLoadFeature } from '@micro-frontend-react/employee-experience/lib/UsageTelemetryHelper';
import { WhiteContainer } from '../Shared/SharedLayout';
import { updateSelectedPage } from '../Shared/SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IHttpClient } from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import {
    Stack,
    MessageBar,
    MessageBarType,
    Spinner,
    SpinnerSize,
    Text,
    Dropdown,
    IDropdownOption,
    Pivot,
    PivotItem,
} from '@fluentui/react';
import { ITenantInfo, IUserRoleAssignment } from './AdminPage.types';
import {
    GLOBAL_ADMIN_ROLE,
    dropdownStyles,
    centeredStackStyles,
    dashboardStackStyles,
} from './AdminPage.constants';
import { fetchApi } from './AdminPage.api';
import { ManageUsersTab } from './ManageUsersTab';
import { TenantSettingsTab } from './TenantSettingsTab';

export function AdminPage(): React.ReactElement {
    usePageTitle(`Admin - ${__APP_NAME__}`);
    const feature = getFeature('MSApprovalsWeb', 'Admin');
    usePageTracking(getPageLoadFeature(feature));
    const { dispatch, httpClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );

    const [loading, setLoading] = React.useState(true);
    const [authResult, setAuthResult] = React.useState<{ effectiveRole: string } | null>(null);
    const [error, setError] = React.useState<string | null>(null);

    const [tenants, setTenants] = React.useState<ITenantInfo[]>([]);
    const [tenantsLoading, setTenantsLoading] = React.useState(false);
    const [selectedTenantId, setSelectedTenantId] = React.useState<string | null>(null);

    const [windowHeight, setWindowHeight] = React.useState(window.innerHeight);

    const isMounted = React.useRef(true);

    React.useEffect(() => {
        dispatch(updateSelectedPage('admin'));

        function handleResize() {
            setWindowHeight(window.innerHeight);
        }
        window.addEventListener('resize', handleResize);

        const loadAdminAccess = async () => {
            try {
                setLoading(true);
                setTenantsLoading(true);
                setError(null);

                const roleList = await fetchApi<IUserRoleAssignment[]>(httpClient as IHttpClient, '/admin/me/roles');

                if (!isMounted.current) return;

                if (Array.isArray(roleList) && roleList.length > 0) {
                    // Use the first entry's role as the overall effective role.
                    // Per-tenant role is derived separately via selectedTenantRole.
                    setAuthResult({ effectiveRole: roleList[0].role });
                    setTenants(roleList.map((r) => ({
                        documentTypeId: r.documentTypeId,
                        appName: r.appName,
                        role: r.role,
                    })));
                } else {
                    setAuthResult({ effectiveRole: '' });
                    setTenants([]);
                }
            } catch {
                if (!isMounted.current) return;
                setError('An error occurred while checking admin access.');
            } finally {
                if (isMounted.current) {
                    setTenantsLoading(false);
                    setLoading(false);
                }
            }
        };

        loadAdminAccess();

        return () => {
            isMounted.current = false;
            window.removeEventListener('resize', handleResize);
        };
    }, [dispatch, httpClient]);

    const onTenantChange = (_event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        if (option) {
            setSelectedTenantId(option.key as string);
        }
    };

    const isGlobalAdmin = authResult?.effectiveRole === GLOBAL_ADMIN_ROLE;

    // Derive role for the currently selected tenant
    const selectedTenantRole = React.useMemo(() => {
        if (isGlobalAdmin) return GLOBAL_ADMIN_ROLE;
        if (!selectedTenantId) return authResult?.effectiveRole || '';
        const tenant = tenants.find(t => t.documentTypeId === selectedTenantId);
        return tenant?.role || '';
    }, [isGlobalAdmin, selectedTenantId, tenants, authResult]);

    const tenantDropdownOptions = React.useMemo(
        () => tenants
            .map((t) => ({ key: t.documentTypeId, text: t.appName }))
            .sort((a, b) => a.text.localeCompare(b.text)),
        [tenants]
    );

    // Auto-select when only one tenant is available
    React.useEffect(() => {
        if (tenants.length === 1 && !selectedTenantId) {
            setSelectedTenantId(tenants[0].documentTypeId);
        }
    }, [tenants, selectedTenantId]);

    if (loading) {
        return (
            <WhiteContainer windowHeight={windowHeight}>
                <Stack horizontalAlign="center" verticalAlign="center" styles={centeredStackStyles}>
                    <Spinner size={SpinnerSize.large} label="Checking admin access..." />
                </Stack>
            </WhiteContainer>
        );
    }

    if (error) {
        return (
            <WhiteContainer windowHeight={windowHeight}>
                <MessageBar messageBarType={MessageBarType.error}>{error}</MessageBar>
            </WhiteContainer>
        );
    }

    if (!authResult?.effectiveRole) {
        return (
            <WhiteContainer windowHeight={windowHeight}>
                <Stack horizontalAlign="center" verticalAlign="center" styles={centeredStackStyles}>
                    <MessageBar messageBarType={MessageBarType.blocked}>
                        401 Unauthorized — You do not have admin access to this resource.
                    </MessageBar>
                </Stack>
            </WhiteContainer>
        );
    }

    return (
        <WhiteContainer windowHeight={windowHeight}>
            <Stack tokens={{ childrenGap: 16 }} styles={dashboardStackStyles}>
                <Text variant="xLarge">Admin Dashboard</Text>

                <Stack tokens={{ childrenGap: 12 }}>
                    {tenantsLoading ? (
                        <Spinner size={SpinnerSize.small} label="Loading tenants..." />
                    ) : tenants.length > 0 ? (
                        <Dropdown
                            label="Select a tenant"
                            placeholder="Choose a tenant"
                            options={tenantDropdownOptions}
                            selectedKey={selectedTenantId}
                            onChange={onTenantChange}
                            styles={dropdownStyles}
                        />
                    ) : (
                        <MessageBar messageBarType={MessageBarType.info}>
                            No tenants available.
                        </MessageBar>
                    )}
                </Stack>

                <Pivot aria-label="Admin sections" styles={{ root: { marginTop: 8 } }}>
                    <PivotItem headerText="Manage Users" itemIcon="People">
                        <ManageUsersTab
                            selectedTenantId={selectedTenantId}
                            httpClient={httpClient as IHttpClient}
                            tenants={tenants}
                            selectedTenantRole={selectedTenantRole}
                        />
                    </PivotItem>
                    <PivotItem headerText="Tenant Settings" itemIcon="Settings">
                        <TenantSettingsTab
                            selectedTenantId={selectedTenantId}
                            httpClient={httpClient as IHttpClient}
                        />
                    </PivotItem>
                </Pivot>
            </Stack>
        </WhiteContainer>
    );
}
