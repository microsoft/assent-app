import {
    IUserDelegationApp,
    IUserDelegationEntry,
    IDelegationExpiryDetails,
    DelegationExpirySeverity,
} from '../Components/Shared/SharedComponents.types';
import { DEFAULT_LOCALE } from '../Components/Shared/SharedConstants';

// Banner turns yellow when a delegation expires within this many calendar days
// (inclusive). Past dates (negative diff) also trigger the warning because the
// delegation is already lapsed and needs renewal.
export const EXPIRY_WARNING_DAYS = 14;

const MS_PER_DAY = 24 * 60 * 60 * 1000;

// Normalize a Date to local midnight so day diffs are calendar-day based
// (matches what the user sees in the formatted date), not elapsed-time based.
function toLocalMidnight(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}

// Parse an ISO-8601 UTC string into a Date; return null for null/undefined/empty
// or any value that produces an invalid date. Defensive against malformed rows.
export function parseEndDate(value: string | null | undefined): Date | null {
    if (!value) return null;
    const d = new Date(value);
    if (isNaN(d.getTime())) return null;
    return d;
}

// Calendar-day delta between two Dates, computed in the user's local timezone.
// Positive when target is in the future, zero when same day, negative when past.
export function getCalendarDaysUntil(target: Date, now: Date): number {
    const targetMidnight = toLocalMidnight(target).getTime();
    const nowMidnight = toLocalMidnight(now).getTime();
    return Math.round((targetMidnight - nowMidnight) / MS_PER_DAY);
}

export interface IEarliestEndDateResult {
    endDate: Date;
    appName: string | null;
}

// A delegator may have multiple per-tenant apps[] entries with different (or
// null) endDates. We surface the earliest non-null + valid endDate (and the
// corresponding appName) because it represents the most pressing renewal.
export function getEarliestEndDate(apps: IUserDelegationApp[] | null | undefined): IEarliestEndDateResult | null {
    if (!apps || apps.length === 0) return null;
    let earliest: IEarliestEndDateResult | null = null;
    for (const app of apps) {
        const parsed = parseEndDate(app?.endDate);
        if (parsed && (earliest === null || parsed.getTime() < earliest.endDate.getTime())) {
            earliest = { endDate: parsed, appName: app?.appName || null };
        }
    }
    return earliest;
}

function formatDate(d: Date, locale: string): string | null {
    try {
        return new Intl.DateTimeFormat(locale, {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
        }).format(d);
    } catch {
        return null;
    }
}

function formatTime(d: Date, locale: string): string | null {
    try {
        return new Intl.DateTimeFormat(locale, {
            hour: 'numeric',
            minute: '2-digit',
        }).format(d);
    } catch {
        return null;
    }
}

// Returns the user's local timezone abbreviation (e.g. "PDT", "GMT-7").
// Falls back to a numeric offset when timeZoneName parts are unavailable.
function getTimeZoneLabel(d: Date, locale: string): string | null {
    try {
        const parts = new Intl.DateTimeFormat(locale, {
            timeZoneName: 'short',
        }).formatToParts(d);
        const tz = parts.find((p) => p.type === 'timeZoneName');
        if (tz?.value) return tz.value;
    } catch {
        // fall through to offset-based label
    }
    const offsetMinutes = -d.getTimezoneOffset();
    const sign = offsetMinutes >= 0 ? '+' : '-';
    const abs = Math.abs(offsetMinutes);
    const hh = Math.floor(abs / 60);
    const mm = abs % 60;
    return mm === 0 ? `GMT${sign}${hh}` : `GMT${sign}${hh}:${String(mm).padStart(2, '0')}`;
}

function getSeverity(daysUntilExpiration: number): DelegationExpirySeverity {
    return daysUntilExpiration <= EXPIRY_WARNING_DAYS ? 'warning' : 'none';
}

// Computes all banner-facing derived fields from a delegation entry.
// `now` is injected for testability; defaults to the current time so the
// production call site can simply pass an entry.
export function computeDelegationExpiryDetails(
    entry: IUserDelegationEntry | null | undefined,
    now: Date = new Date(),
    locale: string = DEFAULT_LOCALE
): IDelegationExpiryDetails | null {
    if (!entry || !entry.delegator) return null;
    const delegatorAlias = (entry.delegator.UserPrincipalName || '').split('@')[0] || '';
    const earliest = getEarliestEndDate(entry.apps);

    if (!earliest) {
        return {
            delegatorAlias,
            daysUntilExpiration: null,
            expirySeverity: 'none',
            expirationTimeFormatted: null,
            expirationTimeZone: null,
            expirationDateTimeFormatted: null,
            expiringAppName: null,
        };
    }

    const { endDate, appName } = earliest;
    const daysUntilExpiration = getCalendarDaysUntil(endDate, now);
    const expirySeverity = getSeverity(daysUntilExpiration);
    const dateFormatted = formatDate(endDate, locale);
    const timeFormatted = formatTime(endDate, locale);
    const tzLabel = getTimeZoneLabel(endDate, locale);
    const dateTimeFormatted =
        dateFormatted && timeFormatted && tzLabel
            ? `${dateFormatted}, ${timeFormatted} ${tzLabel}`
            : null;

    return {
        delegatorAlias,
        daysUntilExpiration,
        expirySeverity,
        expirationTimeFormatted: timeFormatted,
        expirationTimeZone: tzLabel,
        expirationDateTimeFormatted: dateTimeFormatted,
        expiringAppName: appName,
    };
}

// Builds the user-facing expiry sentence from the derived details. The wording
// depends on how soon the delegation expires so the message reads naturally for
// the expired, today, and future cases. Returns null when there is nothing to
// show or when the formatted date/time strings are unavailable.
export function getDelegationExpiryMessage(details: IDelegationExpiryDetails | null): string | null {
    if (!details || details.expirySeverity === 'none') return null;
    const { daysUntilExpiration: days, expirationDateTimeFormatted: dt } = details;
    const { expirationTimeFormatted: time, expirationTimeZone: tz, expiringAppName: app } = details;
    const forApp = app ? ` for ${app}` : '';
    if (days === null) return null;
    if (days < 0) {
        return dt ? `Delegation expired on ${dt}${forApp}` : null;
    }
    if (days === 0) {
        return time && tz ? `Delegation expiring today at ${time} ${tz}${forApp}` : null;
    }
    const dayWord = days === 1 ? 'day' : 'days';
    return dt ? `Delegation expiring in ${days} ${dayWord} on ${dt}${forApp}` : null;
}
