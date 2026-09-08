/**
 * Extracts a human-readable error message from an HTTP error response or exception.
 * Checks common response shapes (err.data.message, err.data, err.message) and
 * falls back to the provided default when none match.
 */
export function extractErrorMessage(err: any, fallback: string): string {
    const msg = err?.data?.message || err?.data || err?.message || fallback;
    return typeof msg === 'string' ? msg : JSON.stringify(msg);
}
