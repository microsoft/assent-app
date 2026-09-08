import { IMatchMetadata } from '../../../Shared/SharedComponents.types';

export interface ISummaryCardsModel {
    AppName: string;
    Title: string;
    SubmittedDate: Date;
    CompanyCode: string;
    UnitValue: string;
    UnitofMeasure: string;
    DisplayDocumentNumber: string;
    DocumentNumber: string;
    FiscalYear: string;
    Submitter: string;
    SubmitterAlias: string;
    TenantId: string;
    isRead: boolean;
    lastFailed: boolean;
    CustomAttributeName: string | null;
    CustomAttributeValue: string | null;
    selectForBulkApproval: boolean;
    allBulkCheckSelected: boolean;
    IsControlsAndComplianceRequired: boolean;
    AllowBulkApprovalCondition: string;
    FlattenedSummary: string;
    _matchMetadata?: IMatchMetadata;
}
