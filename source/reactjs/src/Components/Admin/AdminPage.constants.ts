import { IDropdownOption, IDropdownStyles, ITextFieldStyles } from '@fluentui/react';

export const TENANT_ADMIN_ROLE = 'TenantAdmin';
export const GLOBAL_ADMIN_ROLE = 'GlobalAdmin';
export const TENANT_USER_ROLE = 'TenantUser';

export const SUBMISSION_TYPE_OPTIONS: IDropdownOption[] = [
    { key: 'Single', text: 'Single — one approval at a time', title: 'End users can only act on one approval at a time. The multi-select checkbox is not shown in the approval list.' },
    { key: 'PseudoBulk', text: 'Bulk Select — submitted individually', title: 'End users can select multiple approvals and act on them at once. Behind the scenes, each approval is sent to the tenant API as a separate request. Use this when the tenant API does not support receiving multiple approvals in a single call.' },
    { key: 'Bulk', text: 'Bulk Select — submitted as a batch', title: 'End users can select multiple approvals and act on them at once. All selected approvals are sent to the tenant API in a single batched request. Use this only when the tenant API supports receiving multiple approvals per call.' },
];

export const BULK_EXTERNAL_OPTION: IDropdownOption = {
    key: 'BulkExternal',
    text: 'Bulk Select — tenant-managed batch',
    title: 'End users can select multiple approvals and act on them at once. All selected approvals are sent to the tenant API, and the tenant processes each approval internally, returning individual pass/fail results. Only available for tenants already configured with this mode.',
};

export const dropdownStyles: Partial<IDropdownStyles> = {
    dropdown: { width: 300 },
};

export const textFieldStyles: Partial<ITextFieldStyles> = {
    root: { width: 300 },
};

export const centeredStackStyles = { root: { minHeight: 200 } };
export const dashboardStackStyles = { root: { padding: '20px' } };
export const tabContentStyles = { root: { paddingTop: 16 } };
