// Constants for the "Export summary to Excel" feature. Kept in a dedicated file so the export
// configuration (output file, excluded columns, header overrides, column formats) stays organized and easy to update.

import { SummaryTableFieldNames } from './SummaryTable/SummaryTableFieldNames';

// Prefix and extension for the file offered to the user when the export completes.
export const SUMMARY_EXPORT_FILE_NAME_PREFIX = 'MSApprovalsDownload';
export const SUMMARY_EXPORT_FILE_EXTENSION = 'csv';

// Builds a unique, filesystem-safe download name of the form:
//   MSApprovalsDownload-<alias>-<yyyyMMdd>-<HHmm>UTC.csv
// The timestamp is UTC and accurate to the minute — enough to differentiate one download from another.
// The trailing "UTC" makes the timezone explicit to the user. Alias handling is defensive: users are not
// guaranteed an @microsoft.com (or any) UPN, so we take the local part before any '@', strip characters
// that are unsafe in file names, and fall back to 'user'.
export const getSummaryExportFileName = (alias?: string | null): string => {
    const localPart = (alias ?? '').split('@')[0];
    const safeAlias = localPart.replace(/[^A-Za-z0-9._-]/g, '').slice(0, 64) || 'user';
    const now = new Date();
    const pad = (value: number): string => String(value).padStart(2, '0');
    const date = `${now.getUTCFullYear()}${pad(now.getUTCMonth() + 1)}${pad(now.getUTCDate())}`;
    const time = `${pad(now.getUTCHours())}${pad(now.getUTCMinutes())}UTC`;
    return `${SUMMARY_EXPORT_FILE_NAME_PREFIX}-${safeAlias}-${date}-${time}.${SUMMARY_EXPORT_FILE_EXTENSION}`;
};

// MIME type used when wrapping the exported bytes in a Blob for download.
export const SUMMARY_EXPORT_CONTENT_TYPE = 'text/csv;charset=utf-8';

// Copy for the one-time teaching coachmark that introduces the summary export button.
// Kept here so the export feature owns its user-facing strings in a single place. The primary
// line is split around the export (Download) icon, which SecondaryHeader renders inline between
// these two fragments so the text mirrors the actual button the user should click.
export const SUMMARY_EXPORT_COACHMARK_HEADLINE = 'Export your approvals';
export const SUMMARY_EXPORT_COACHMARK_CONTENT_BEFORE_ICON = 'Click Export';
export const SUMMARY_EXPORT_COACHMARK_CONTENT_AFTER_ICON = 'to download your pending approvals as a CSV file.';
export const SUMMARY_EXPORT_COACHMARK_CONTENT_SECONDARY =
    'To export approvals for a specific application only, turn on Enable multiple selection and ' +
    'select the application you want, and then click Export.';

// UI-only columns (action buttons / bulk selection) that are shown in the summary table but must not be exported.
export const SUMMARY_EXPORT_EXCLUDED_COLUMNS: string[] = [
    SummaryTableFieldNames.viewDetails,
    SummaryTableFieldNames.actionDetails,
    SummaryTableFieldNames.allowInBulkApproval,
];

// Export-only column header overrides. The on-screen table headers (COLUMN_DISPLAY_NAMES) use short labels like
// "Status"/"Amount"; the export uses these more descriptive headers instead. Falls back to COLUMN_DISPLAY_NAMES.
export const SUMMARY_EXPORT_HEADER_OVERRIDES: Record<string, string> = {
    [SummaryTableFieldNames.IsRead]: 'Is Read',
    [SummaryTableFieldNames.UnitValue]: 'Unit Value',
};

// Describes how a composite export column is combined from one or more summary fields.
export interface SummaryExportColumnFormat {
    // Ordered summary field paths (dot-notation) whose values are combined for the column.
    fields: string[];
    // Separator placed between the non-empty field values. Empty values are skipped.
    separator: string;
}

// Summary field path for the custom attribute name (not a standalone table column, so it is not in
// SummaryTableFieldNames). Paired with SummaryTableFieldNames.CustomAttribute (the value) to build "Additional Info".
const CUSTOM_ATTRIBUTE_NAME_FIELD = 'CustomAttribute.CustomAttributeName';

// Export-only column composition specs. The UI passes these so composite columns in the CSV are combined exactly as
// they are rendered in the table. The backend joins the non-empty field values with the separator; a raw
// "{name}: {value}" template cannot express "omit the separator when the name is empty", so a fields+separator spec
// is used instead. Columns absent from this map export their raw value.
export const SUMMARY_EXPORT_COLUMN_FORMATS: Record<string, SummaryExportColumnFormat> = {
    // "Additional Information" renders as "<name>: <value>" (or just the value when there is no name).
    [SummaryTableFieldNames.CustomAttribute]: {
        fields: [CUSTOM_ATTRIBUTE_NAME_FIELD, SummaryTableFieldNames.CustomAttribute],
        separator: ': ',
    },
};
