import * as React from 'react';
import Mark = require('mark.js');
import { ISearchHighlight, IAttachmentMatch } from '../Components/Shared/SharedComponents.types';

/** Extracts unique matched terms (≥2 chars) from backend highlights. */
export function extractMatchedTerms(highlights: ISearchHighlight[] | null | undefined): string[] {
    if (!Array.isArray(highlights)) return [];

    const terms = highlights.flatMap((h) => h.Terms ?? []).filter((t) => t && t.length >= 2);

    return Array.from(new Set(terms));
}

/** Highlights terms in a DOM container using mark.js. Clears prior marks before re-highlighting. */
export function highlightTermsInContainer(container: HTMLElement, terms: string[]): void {
    if (!container) return;

    const instance = new Mark(container);
    instance.unmark({
        done: () => {
            if (terms?.length) {
                instance.mark(terms, {
                    separateWordSearch: false,
                    caseSensitive: false,
                    acrossElements: true,
                    accuracy: 'complementary',
                    exclude: ['.ac-image', 'img', 'button', '.ac-pushButton'],
                });
            }
        },
    });
}

/** Normalizes a raw search result, handling both camelCase and PascalCase property names. */
export function normalizeSearchResult(raw: any): {
    highlights: ISearchHighlight[];
    attachmentMatches: IAttachmentMatch[];
} {
    return {
        highlights: (raw?.highlights || raw?.Highlights || []).map((h: any) => ({
            Source: h.source ?? h.Source ?? '',
            FieldPath: h.fieldPath ?? h.FieldPath ?? null,
            Terms: h.terms ?? h.Terms ?? [],
        })),
        attachmentMatches: (raw?.attachmentMatches || raw?.AttachmentMatches || []).map((m: any) => ({
            AttachmentId: m.attachmentId ?? m.AttachmentId ?? null,
            AttachmentName: m.attachmentName ?? m.AttachmentName ?? '',
            Snippet: m.snippet ?? m.Snippet ?? '',
        })),
    };
}

/** Returns true if any highlight is from field content (not attachments). */
export function hasFieldHighlights(highlights: ISearchHighlight[] | null | undefined): boolean {
    if (!Array.isArray(highlights)) return false;
    return highlights.some((h) => !h.Source?.toLowerCase().startsWith('attachments'));
}

/** Extracts the file extension from a filename (e.g. 'report.pdf' → 'pdf'). */
export function getFileExtension(fileName: string): string {
    const dotIndex = fileName.lastIndexOf('.');
    return dotIndex > 0 ? fileName.substring(dotIndex + 1).toLowerCase() : '';
}

const SNIPPET_CONTEXT_WORDS = 3;

/** Renders a search snippet with <em> highlights into React nodes with context words around each term. */
export function renderSnippet(raw: string | undefined): React.ReactNode | undefined {
    if (!raw) return undefined;
    const sanitized = raw.replace(/<(?!\/?em\b)[^>]*>/gi, '');
    const cleaned = sanitized
        .replace(/\\n/g, ' ')
        .replace(/[\n\r\t]/g, ' ')
        .replace(/\s{2,}/g, ' ')
        .trim();
    if (!cleaned) return undefined;

    const parts = cleaned.split(/(<em>.*?<\/em>)/gi);
    const fragments: React.ReactNode[] = [];
    const seen = new Set<string>();

    parts.forEach((part, i) => {
        if (part.startsWith('<em>')) {
            const term = part.replace(/<\/?em>/g, '').trim();
            if (!term || seen.has(term.toLowerCase())) return;
            seen.add(term.toLowerCase());

            const before = parts[i - 1];
            const after = parts[i + 1];
            const wordsBefore = before ? before.trim().split(/\s+/).slice(-SNIPPET_CONTEXT_WORDS).join(' ') : '';
            const wordsAfter = after ? after.trim().split(/\s+/).slice(0, SNIPPET_CONTEXT_WORDS).join(' ') : '';

            if (fragments.length > 0) fragments.push(' ... ');
            if (wordsBefore) fragments.push(wordsBefore + ' ');
            fragments.push(<strong key={i}>{term}</strong>);
            if (wordsAfter) fragments.push(' ' + wordsAfter);
        }
    });

    if (fragments.length === 0) return undefined;
    return <>{fragments}</>;
}
