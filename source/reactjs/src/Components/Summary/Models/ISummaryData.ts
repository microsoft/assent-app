export interface ICustomAttribute {
    CustomAttributeName: string;
    CustomAttributeValue: string;
}

export interface IApprovalIdentifier {
    DisplayDocumentNumber: string;
    DocumentNumber: string;
    FiscalYear?: string | null;
    DocumentNumberPrefix: string;
}

export interface IAdditionalData {
    RequestorName: string;
    RequestorAlias: string;
    AmountUSD: string;
    BudgetIndicator: string;
    ApproverList: string;
    RequestActivities: string;
    SapProjectNumber: string;
    SapProjectName: string;
    RequisitionType: string;
    TemplateUri: string;
    DetailsUri: string;
}

export interface ISummaryItem {
    TenantId: number;
    DocumentTypeId: string;
    Approver?: unknown | null;
    SummaryJson?: unknown | null;
    AppName: string;
    CondensedAppName: string;
    Submitter: any;
    Title: string;
    Amount?: string | null;
    CurrencyCode?: string | null;
    SubmittedDate: string;
    DocumentNumber: string;
    CustomAttribute: ICustomAttribute;
    DetailOperations?: unknown | null;
    TemplateName: string;
    CompanyCode: string;
    IsOfflineApprovalSupported: boolean;
    ReadDetails: boolean;
    ApprovalIdentifier: IApprovalIdentifier;
    UnitValue: string;
    UnitOfMeasure: string;
    AdditionalData: IAdditionalData;
    LastFailed: boolean;
    LastFailedExceptionMessage?: string | null;
    Xcv: string;
    BusinessProcessName: string;
    IsRead: boolean;
    IsControlsAndComplianceRequired: boolean;
    IsBackgroundApprovalSupportedUpfront: boolean;
    IsOutOfSyncChallenged: boolean;
    IsOfflineApproval: boolean;
    LobPending: boolean;
    AllowBulkApprovalCondition?: string | null;
}

export interface IDashboardFilters {
    recentSubmissions: boolean;
    hasErrors: boolean;
    highPriorityRequests: boolean;
    // Generic property filters - key is the property name, value is array of selected values
    selectedProperties: Record<string, string[]>;
}

export interface IFilterCard {
    title: string;
    count: number;
    isActive: boolean;
    filterKey: keyof IDashboardFilters;
}

export interface IChartDataPoint {
    x: string;
    y: number;
    color?: string;
}

export interface IInsightsData {
    applicationBreakdown: IChartDataPoint[];
    companyCodeBreakdown: IChartDataPoint[];
    unitValueBreakdown: IChartDataPoint[];
}
