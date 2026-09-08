/** Pure helper functions for DocumentPreviewModal. Extracted for testability. */

export function arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

export function getImageMimeType(base64FirstChar: string): string {
    switch (base64FirstChar) {
        case '/':
            return 'image/jpeg';
        case 'R':
            return 'image/gif';
        case 'i':
        default:
            return 'image/png';
    }
}

export function getModalSize(isExpanded: boolean, windowWidth: number, windowHeight: number) {
    if (isExpanded) {
        return { width: windowWidth * 0.9, height: windowHeight * 0.9 };
    }
    return { width: windowWidth * 0.6, height: windowHeight * 0.85 };
}

export type PreviewContentType = 'image' | 'pdf' | 'unsupported';

/** Determines how to render a preview based on Content-Type header and base64 heuristic fallback. */
export function detectPreviewContentType(
    contentType: string | null | undefined,
    base64FirstChar: string | undefined
): PreviewContentType {
    const ct = contentType?.toLowerCase();
    // Use header when it carries a recognized type
    if (ct && ct !== 'application/octet-stream') {
        if (ct.startsWith('image/')) return 'image';
        if (ct === 'application/pdf') return 'pdf';
        // Unrecognized Content-Type — fall through to heuristic rather than
        // returning unsupported, since the backend may mislabel content.
    }
    // Fallback: heuristic from base64 first character
    if (base64FirstChar && ['/', 'i', 'R'].includes(base64FirstChar)) return 'image';
    if (base64FirstChar === 'J') return 'pdf';
    return 'unsupported';
}

/** Builds the document preview API URL, omitting displayDocumentNumber when undefined. */
export function buildPreviewUrl(
    apiBaseUrl: string,
    apiUrlRoot: string,
    tenantId: string,
    documentNumber: string,
    attachmentId: string,
    displayDocumentNumber?: string
): string {
    const displayDocParam = displayDocumentNumber
        ? `&displayDocumentNumber=${encodeURIComponent(displayDocumentNumber)}`
        : '';
    return `${apiBaseUrl}${apiUrlRoot}/documentpreview/${encodeURIComponent(
        tenantId
    )}/${encodeURIComponent(documentNumber)}/?attachmentId=${encodeURIComponent(
        attachmentId
    )}&isPreAttached=true${displayDocParam}`;
}
