import { SummaryTableFieldNames } from './SummaryTableFieldNames';

export interface ITableColumnConfig {
    field: string;
    displayName: string;
    isFilterable: boolean;
}

export const tenantColumns: Record<string, ITableColumnConfig> = {
    [SummaryTableFieldNames.IsRead]: {
        field: SummaryTableFieldNames.IsRead,
        displayName: 'Read',
        isFilterable: true,
    },
    [SummaryTableFieldNames.ApprovalIdentifier]: {
        field: SummaryTableFieldNames.ApprovalIdentifier,
        displayName: 'Document Number',
        isFilterable: true,
    },
    [SummaryTableFieldNames.SubmittedDate]: {
        field: SummaryTableFieldNames.SubmittedDate,
        displayName: 'Submitted Date',
        isFilterable: false,
    },
    [SummaryTableFieldNames.Submitter]: {
        field: SummaryTableFieldNames.Submitter,
        displayName: 'Submitter',
        isFilterable: true,
    },
    [SummaryTableFieldNames.Title]: {
        field: SummaryTableFieldNames.Title,
        displayName: 'Title',
        isFilterable: true,
    },
    [SummaryTableFieldNames.UnitValue]: {
        field: SummaryTableFieldNames.UnitValue,
        displayName: 'Unit Value',
        isFilterable: true,
    },
    [SummaryTableFieldNames.CompanyCode]: {
        field: SummaryTableFieldNames.CompanyCode,
        displayName: 'Company Code',
        isFilterable: true,
    },
    [SummaryTableFieldNames.CustomAttribute]: {
        field: SummaryTableFieldNames.CustomAttribute,
        displayName: 'Additional Information',
        isFilterable: true,
    },
};

export const defaultTableColumns = Object.values(tenantColumns).map(config => ({
    field: config.field,
    title: config.displayName,
    isFilterable: config.isFilterable,
}));
