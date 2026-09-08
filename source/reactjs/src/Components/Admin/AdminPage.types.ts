export interface ITenantInfo {
    documentTypeId: string;
    appName: string;
    role?: string;
}

export interface IUserRoleAssignment {
    documentTypeId: string;
    appName: string;
    role: string;
}

export interface IRoleAssignmentSummary {
    documentTypeId: string;
    userObjectId: string;
    userPrincipalName: string;
    role: string;
    assignedBy: string;
}

export interface IUpdateRoleAssignmentsRequest {
    adds: string[];
    deletes: string[];
    appName?: string;
    role?: string;
}

export interface IRoleAssignmentOperationResult {
    userPrincipalName: string;
    userObjectId: string;
    operation: string;
    success: boolean;
    error: string;
}

export interface IUpdateRoleAssignmentsResponse {
    results: IRoleAssignmentOperationResult[];
}

export interface ITenantActionSummary {
    section: string;
    code: string;
    name: string;
    isBulkAction: boolean;
    commentLength: number;
}

export interface ITenantSettingsResponse {
    documentTypeId: string;
    appName: string;
    eTag: string;
    actionSubmissionType: string;
    bulkActionConcurrentCall: number;
    isExternalTenantActionDetails: boolean;
    actions: ITenantActionSummary[];
}

export interface IActionEdit {
    section: string;
    code: string;
    isBulkAction: boolean;
    commentLength: number;
}

export interface IOperationResult {
    success: boolean;
    message: string;
}
