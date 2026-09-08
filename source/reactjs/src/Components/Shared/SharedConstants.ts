import { IDropdownOption } from '@fluentui/react';

export const GROUP_BY_FILTER = 'Group By Filter';
export const DETAILS_DEFAULT_VIEW = 'Details ViewV2';
export const HISTORY_DEFAULT_VIEW = 'History ViewV2';
export const DEFAULT_VIEW_TYPE = 'View TypeV2';
export const DOCKED_VIEW = 'Docked';
export const FLYOUT_VIEW = 'Flyout';
export const CARD_VIEW = 'Card';
export const TABLE_VIEW = 'Table';
export const DEFAULT_TENANT = 'Default Tenant';
export const TABLE_COLUMNS_DEFAULT = 'Table Columns Default';
export const TABLE_COLUMNS_PULLTENANT = 'Table Columns PullTenant';
export const CONTINUE_TIMEOUT = 4000;
export const DATE_FORMAT_OPTION: any = { year: 'numeric', day: 'numeric', month: 'short' }; //// MMM DD,YYYY
export const DEFAULT_LOCALE = 'en-US';

// RowKey of the FlightingFeature row (TeachingCoach=true, no slides) that drives the Settings icon coachmark/pulse.
export const SETTINGS_COACHMARK_ID = '48';

// Column display names for the settings UI
export const COLUMN_DISPLAY_NAMES: Record<string, string> = {
    IsRead: 'Status',
    'ApprovalIdentifier.DisplayDocumentNumber': 'Document Number',
    SubmittedDate: 'Submitted Date',
    'Submitter.Name': 'Submitter',
    Title: 'Title',
    UnitValue: 'Amount',
    CompanyCode: 'Company Code',
    'CustomAttribute.CustomAttributeValue': 'Additional Info',
    viewDetails: 'Details',
    actionDetails: 'Approval Status',
    submittedForFullName: 'Submitted For',
    assignmentName: 'Assignment',
    laborDate: 'Date',
    laborHours: 'Hours',
    laborCategoryName: 'Category',
    submittedByFullName: 'Submitted By',
    isBillable: 'Billable',
    laborNotes: 'Notes',
    allowInBulkApproval: 'Anomaly',
};

// Columns that cannot be hidden (functional, not informational)
export const MANDATORY_COLUMNS: Record<string, string[]> = {
    Default: ['ApprovalIdentifier.DisplayDocumentNumber'],
    PullTenant: ['viewDetails', 'actionDetails'],
};

// Default visible columns per tenant type (matches current hardcoded behavior)
export const DEFAULT_VISIBLE_COLUMNS: Record<string, string[]> = {
    Default: [
        'IsRead',
        'ApprovalIdentifier.DisplayDocumentNumber',
        'SubmittedDate',
        'Submitter.Name',
        'Title',
        'UnitValue',
        'CompanyCode',
        'CustomAttribute.CustomAttributeValue',
    ],
    PullTenant: [
        'allowInBulkApproval',
        'viewDetails',
        'actionDetails',
        'submittedForFullName',
        'assignmentName',
        'laborDate',
        'laborHours',
        'laborCategoryName',
        'submittedByFullName',
        'isBillable',
        'laborNotes',
    ],
};

export function parseColumnPreference(json: string): string[] | null {
    try {
        const parsed = JSON.parse(json);
        return Array.isArray(parsed) && parsed.length > 0 ? parsed : null;
    } catch {
        return null;
    }
}

export const IANA_TO_WINDOWS_TIMEZONE: Record<string, string> = {
    'Africa/Abidjan': 'Greenwich Standard Time',
    'Africa/Accra': 'Greenwich Standard Time',
    'Africa/Addis_Ababa': 'E. Africa Standard Time',
    'Africa/Algiers': 'W. Central Africa Standard Time',
    'Africa/Cairo': 'Egypt Standard Time',
    'Africa/Casablanca': 'Morocco Standard Time',
    'Africa/Johannesburg': 'South Africa Standard Time',
    'Africa/Lagos': 'W. Central Africa Standard Time',
    'Africa/Nairobi': 'E. Africa Standard Time',
    'Africa/Tripoli': 'Libya Standard Time',
    'America/Anchorage': 'Alaskan Standard Time',
    'America/Argentina/Buenos_Aires': 'Argentina Standard Time',
    'America/Bogota': 'SA Pacific Standard Time',
    'America/Caracas': 'Venezuela Standard Time',
    'America/Chicago': 'Central Standard Time',
    'America/Denver': 'Mountain Standard Time',
    'America/Halifax': 'Atlantic Standard Time',
    'America/Los_Angeles': 'Pacific Standard Time',
    'America/Manaus': 'SA Western Standard Time',
    'America/Mexico_City': 'Central Standard Time (Mexico)',
    'America/New_York': 'Eastern Standard Time',
    'America/Phoenix': 'US Mountain Standard Time',
    'America/Regina': 'Canada Central Standard Time',
    'America/Santiago': 'Pacific SA Standard Time',
    'America/Sao_Paulo': 'E. South America Standard Time',
    'America/St_Johns': 'Newfoundland Standard Time',
    'America/Toronto': 'Eastern Standard Time',
    'America/Vancouver': 'Pacific Standard Time',
    'America/Winnipeg': 'Central Standard Time',
    'Asia/Almaty': 'Central Asia Standard Time',
    'Asia/Amman': 'Jordan Standard Time',
    'Asia/Baghdad': 'Arabic Standard Time',
    'Asia/Baku': 'Azerbaijan Standard Time',
    'Asia/Bangkok': 'SE Asia Standard Time',
    'Asia/Beirut': 'Middle East Standard Time',
    'Asia/Calcutta': 'India Standard Time',
    'Asia/Colombo': 'Sri Lanka Standard Time',
    'Asia/Damascus': 'Syria Standard Time',
    'Asia/Dhaka': 'Bangladesh Standard Time',
    'Asia/Dubai': 'Arabian Standard Time',
    'Asia/Hong_Kong': 'China Standard Time',
    'Asia/Irkutsk': 'North Asia East Standard Time',
    'Asia/Jakarta': 'SE Asia Standard Time',
    'Asia/Jerusalem': 'Israel Standard Time',
    'Asia/Kabul': 'Afghanistan Standard Time',
    'Asia/Karachi': 'Pakistan Standard Time',
    'Asia/Kathmandu': 'Nepal Standard Time',
    'Asia/Kolkata': 'India Standard Time',
    'Asia/Krasnoyarsk': 'North Asia Standard Time',
    'Asia/Kuala_Lumpur': 'Singapore Standard Time',
    'Asia/Kuwait': 'Arab Standard Time',
    'Asia/Magadan': 'Magadan Standard Time',
    'Asia/Muscat': 'Arabian Standard Time',
    'Asia/Novosibirsk': 'N. Central Asia Standard Time',
    'Asia/Omsk': 'Omsk Standard Time',
    'Asia/Rangoon': 'Myanmar Standard Time',
    'Asia/Riyadh': 'Arab Standard Time',
    'Asia/Seoul': 'Korea Standard Time',
    'Asia/Shanghai': 'China Standard Time',
    'Asia/Singapore': 'Singapore Standard Time',
    'Asia/Taipei': 'Taipei Standard Time',
    'Asia/Tashkent': 'West Asia Standard Time',
    'Asia/Tbilisi': 'Georgian Standard Time',
    'Asia/Tehran': 'Iran Standard Time',
    'Asia/Tokyo': 'Tokyo Standard Time',
    'Asia/Ulaanbaatar': 'Ulaanbaatar Standard Time',
    'Asia/Vladivostok': 'Vladivostok Standard Time',
    'Asia/Yakutsk': 'Yakutsk Standard Time',
    'Asia/Yekaterinburg': 'Ekaterinburg Standard Time',
    'Asia/Yerevan': 'Caucasus Standard Time',
    'Atlantic/Azores': 'Azores Standard Time',
    'Atlantic/Cape_Verde': 'Cape Verde Standard Time',
    'Atlantic/Reykjavik': 'Greenwich Standard Time',
    'Australia/Adelaide': 'Cen. Australia Standard Time',
    'Australia/Brisbane': 'E. Australia Standard Time',
    'Australia/Darwin': 'AUS Central Standard Time',
    'Australia/Hobart': 'Tasmania Standard Time',
    'Australia/Perth': 'W. Australia Standard Time',
    'Australia/Sydney': 'AUS Eastern Standard Time',
    'Europe/Amsterdam': 'W. Europe Standard Time',
    'Europe/Athens': 'GTB Standard Time',
    'Europe/Belgrade': 'Central Europe Standard Time',
    'Europe/Berlin': 'W. Europe Standard Time',
    'Europe/Brussels': 'Romance Standard Time',
    'Europe/Bucharest': 'GTB Standard Time',
    'Europe/Budapest': 'Central Europe Standard Time',
    'Europe/Dublin': 'GMT Standard Time',
    'Europe/Helsinki': 'FLE Standard Time',
    'Europe/Istanbul': 'Turkey Standard Time',
    'Europe/Kiev': 'FLE Standard Time',
    'Europe/Lisbon': 'GMT Standard Time',
    'Europe/London': 'GMT Standard Time',
    'Europe/Madrid': 'Romance Standard Time',
    'Europe/Minsk': 'Belarus Standard Time',
    'Europe/Moscow': 'Russian Standard Time',
    'Europe/Paris': 'Romance Standard Time',
    'Europe/Prague': 'Central Europe Standard Time',
    'Europe/Rome': 'W. Europe Standard Time',
    'Europe/Sarajevo': 'Central European Standard Time',
    'Europe/Sofia': 'FLE Standard Time',
    'Europe/Stockholm': 'W. Europe Standard Time',
    'Europe/Vienna': 'W. Europe Standard Time',
    'Europe/Warsaw': 'Central European Standard Time',
    'Europe/Zurich': 'W. Europe Standard Time',
    'Pacific/Auckland': 'New Zealand Standard Time',
    'Pacific/Bougainville': 'Central Pacific Standard Time',
    'Pacific/Fiji': 'Fiji Standard Time',
    'Pacific/Guam': 'West Pacific Standard Time',
    'Pacific/Honolulu': 'Hawaiian Standard Time',
    'Pacific/Kwajalein': 'UTC+12',
    'Pacific/Midway': 'UTC-11',
    'Pacific/Pago_Pago': 'UTC-11',
    'Pacific/Port_Moresby': 'West Pacific Standard Time',
    'Pacific/Tongatapu': 'Tonga Standard Time',
    'America/Belize': 'Central America Standard Time',
    'America/Chihuahua': 'Mountain Standard Time (Mexico)',
    'America/Cayenne': 'SA Eastern Standard Time',
    'America/Godthab': 'Greenland Standard Time',
    'America/Indianapolis': 'US Eastern Standard Time',
    'America/Montevideo': 'Montevideo Standard Time',
    'America/Noronha': 'UTC-02',
    'America/Tijuana': 'Pacific Standard Time (Mexico)',
    'Africa/Windhoek': 'Namibia Standard Time',
    'Etc/GMT+12': 'Dateline Standard Time',
    'UTC': 'UTC',
};

export const TIMEZONE_OPTIONS: IDropdownOption[] = [
    { key: 'Dateline Standard Time', text: '(UTC-12:00) International Date Line West' },
    { key: 'UTC-11', text: '(UTC-11:00) Coordinated Universal Time -11' },
    { key: 'Hawaiian Standard Time', text: '(UTC-10:00) Hawaii' },
    { key: 'Alaskan Standard Time', text: '(UTC-09:00) Alaska' },
    { key: 'Pacific Standard Time (Mexico)', text: '(UTC-08:00) Baja California' },
    { key: 'Pacific Standard Time', text: '(UTC-08:00) Pacific Time (US & Canada)' },
    { key: 'US Mountain Standard Time', text: '(UTC-07:00) Arizona' },
    { key: 'Mountain Standard Time (Mexico)', text: '(UTC-07:00) Chihuahua, La Paz, Mazatlan' },
    { key: 'Mountain Standard Time', text: '(UTC-07:00) Mountain Time (US & Canada)' },
    { key: 'Central America Standard Time', text: '(UTC-06:00) Central America' },
    { key: 'Central Standard Time', text: '(UTC-06:00) Central Time (US & Canada)' },
    { key: 'Central Standard Time (Mexico)', text: '(UTC-06:00) Guadalajara, Mexico City, Monterrey' },
    { key: 'Canada Central Standard Time', text: '(UTC-06:00) Saskatchewan' },
    { key: 'SA Pacific Standard Time', text: '(UTC-05:00) Bogota, Lima, Quito, Rio Branco' },
    { key: 'Eastern Standard Time', text: '(UTC-05:00) Eastern Time (US & Canada)' },
    { key: 'US Eastern Standard Time', text: '(UTC-05:00) Indiana (East)' },
    { key: 'Venezuela Standard Time', text: '(UTC-04:30) Caracas' },
    { key: 'Atlantic Standard Time', text: '(UTC-04:00) Atlantic Time (Canada)' },
    { key: 'SA Western Standard Time', text: '(UTC-04:00) Georgetown, La Paz, Manaus, San Juan' },
    { key: 'Pacific SA Standard Time', text: '(UTC-04:00) Santiago' },
    { key: 'Newfoundland Standard Time', text: '(UTC-03:30) Newfoundland' },
    { key: 'E. South America Standard Time', text: '(UTC-03:00) Brasilia' },
    { key: 'Argentina Standard Time', text: '(UTC-03:00) Buenos Aires' },
    { key: 'SA Eastern Standard Time', text: '(UTC-03:00) Cayenne, Fortaleza' },
    { key: 'Greenland Standard Time', text: '(UTC-03:00) Greenland' },
    { key: 'Montevideo Standard Time', text: '(UTC-03:00) Montevideo' },
    { key: 'UTC-02', text: '(UTC-02:00) Coordinated Universal Time -02' },
    { key: 'Azores Standard Time', text: '(UTC-01:00) Azores' },
    { key: 'Cape Verde Standard Time', text: '(UTC-01:00) Cape Verde Is.' },
    { key: 'Morocco Standard Time', text: '(UTC) Casablanca' },
    { key: 'UTC', text: '(UTC) Coordinated Universal Time' },
    { key: 'GMT Standard Time', text: '(UTC) Dublin, Edinburgh, Lisbon, London' },
    { key: 'Greenwich Standard Time', text: '(UTC) Monrovia, Reykjavik' },
    { key: 'W. Europe Standard Time', text: '(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna' },
    { key: 'Central Europe Standard Time', text: '(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague' },
    { key: 'Romance Standard Time', text: '(UTC+01:00) Brussels, Copenhagen, Madrid, Paris' },
    { key: 'Central European Standard Time', text: '(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb' },
    { key: 'W. Central Africa Standard Time', text: '(UTC+01:00) West Central Africa' },
    { key: 'Namibia Standard Time', text: '(UTC+01:00) Windhoek' },
    { key: 'Jordan Standard Time', text: '(UTC+02:00) Amman' },
    { key: 'GTB Standard Time', text: '(UTC+02:00) Athens, Bucharest' },
    { key: 'Middle East Standard Time', text: '(UTC+02:00) Beirut' },
    { key: 'Egypt Standard Time', text: '(UTC+02:00) Cairo' },
    { key: 'Syria Standard Time', text: '(UTC+02:00) Damascus' },
    { key: 'FLE Standard Time', text: '(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius' },
    { key: 'Israel Standard Time', text: '(UTC+02:00) Jerusalem' },
    { key: 'Libya Standard Time', text: '(UTC+02:00) Tripoli' },
    { key: 'South Africa Standard Time', text: '(UTC+02:00) Harare, Pretoria' },
    { key: 'Turkey Standard Time', text: '(UTC+02:00) Istanbul' },
    { key: 'Arabic Standard Time', text: '(UTC+03:00) Baghdad' },
    { key: 'Arab Standard Time', text: '(UTC+03:00) Kuwait, Riyadh' },
    { key: 'Belarus Standard Time', text: '(UTC+03:00) Minsk' },
    { key: 'Russian Standard Time', text: '(UTC+03:00) Moscow, St. Petersburg, Volgograd' },
    { key: 'E. Africa Standard Time', text: '(UTC+03:00) Nairobi' },
    { key: 'Iran Standard Time', text: '(UTC+03:30) Tehran' },
    { key: 'Arabian Standard Time', text: '(UTC+04:00) Abu Dhabi, Muscat' },
    { key: 'Azerbaijan Standard Time', text: '(UTC+04:00) Baku' },
    { key: 'Georgian Standard Time', text: '(UTC+04:00) Tbilisi' },
    { key: 'Caucasus Standard Time', text: '(UTC+04:00) Yerevan' },
    { key: 'Afghanistan Standard Time', text: '(UTC+04:30) Kabul' },
    { key: 'West Asia Standard Time', text: '(UTC+05:00) Ashgabat, Tashkent' },
    { key: 'Pakistan Standard Time', text: '(UTC+05:00) Islamabad, Karachi' },
    { key: 'Ekaterinburg Standard Time', text: '(UTC+05:00) Ekaterinburg' },
    { key: 'India Standard Time', text: '(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi' },
    { key: 'Sri Lanka Standard Time', text: '(UTC+05:30) Sri Jayawardenepura' },
    { key: 'Nepal Standard Time', text: '(UTC+05:45) Kathmandu' },
    { key: 'Central Asia Standard Time', text: '(UTC+06:00) Astana' },
    { key: 'Bangladesh Standard Time', text: '(UTC+06:00) Dhaka' },
    { key: 'N. Central Asia Standard Time', text: '(UTC+06:00) Novosibirsk' },
    { key: 'Myanmar Standard Time', text: '(UTC+06:30) Yangon (Rangoon)' },
    { key: 'SE Asia Standard Time', text: '(UTC+07:00) Bangkok, Hanoi, Jakarta' },
    { key: 'North Asia Standard Time', text: '(UTC+07:00) Krasnoyarsk' },
    { key: 'China Standard Time', text: '(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi' },
    { key: 'North Asia East Standard Time', text: '(UTC+08:00) Irkutsk' },
    { key: 'Singapore Standard Time', text: '(UTC+08:00) Kuala Lumpur, Singapore' },
    { key: 'W. Australia Standard Time', text: '(UTC+08:00) Perth' },
    { key: 'Taipei Standard Time', text: '(UTC+08:00) Taipei' },
    { key: 'Ulaanbaatar Standard Time', text: '(UTC+08:00) Ulaanbaatar' },
    { key: 'Korea Standard Time', text: '(UTC+09:00) Seoul' },
    { key: 'Tokyo Standard Time', text: '(UTC+09:00) Osaka, Sapporo, Tokyo' },
    { key: 'Yakutsk Standard Time', text: '(UTC+09:00) Yakutsk' },
    { key: 'AUS Central Standard Time', text: '(UTC+09:30) Darwin' },
    { key: 'Cen. Australia Standard Time', text: '(UTC+09:30) Adelaide' },
    { key: 'AUS Eastern Standard Time', text: '(UTC+10:00) Canberra, Melbourne, Sydney' },
    { key: 'E. Australia Standard Time', text: '(UTC+10:00) Brisbane' },
    { key: 'West Pacific Standard Time', text: '(UTC+10:00) Guam, Port Moresby' },
    { key: 'Tasmania Standard Time', text: '(UTC+10:00) Hobart' },
    { key: 'Vladivostok Standard Time', text: '(UTC+10:00) Vladivostok' },
    { key: 'Magadan Standard Time', text: '(UTC+11:00) Magadan' },
    { key: 'Central Pacific Standard Time', text: '(UTC+11:00) Solomon Is., New Caledonia' },
    { key: 'New Zealand Standard Time', text: '(UTC+12:00) Auckland, Wellington' },
    { key: 'UTC+12', text: '(UTC+12:00) Coordinated Universal Time +12' },
    { key: 'Fiji Standard Time', text: '(UTC+12:00) Fiji' },
    { key: 'Tonga Standard Time', text: '(UTC+13:00) Nuku\'alofa' },
];

export function detectWindowsTimezone(): string {
    try {
        const ianaZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        return IANA_TO_WINDOWS_TIMEZONE[ianaZone] || 'UTC';
    } catch {
        return 'UTC';
    }
}

export const WINDOWS_TO_IANA_TIMEZONE: Record<string, string> = Object.entries(IANA_TO_WINDOWS_TIMEZONE).reduce(
    (acc, [iana, windows]) => {
        if (!acc[windows]) {
            acc[windows] = iana;
        }
        return acc;
    },
    {} as Record<string, string>
);
