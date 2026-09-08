import * as React from 'react';
import * as MarkdownIt from 'markdown-it';
import * as sanitizeHtml from 'sanitize-html';

export interface MarkdownRendererProps {
    content?: string | null | undefined;
    className?: string;
    // When true, allow a safe subset of HTML after sanitization (links/strong/br etc.)
    allowHtml?: boolean;
}

// Singleton markdown-it instance to avoid re-creating on every render
const md = MarkdownIt().set({ html: true, linkify: true, breaks: false });

// Centralized sanitizer config to keep output safe and consistent
const sanitizeConfig: sanitizeHtml.IOptions = {
    allowedTags: [
        'p',
        'br',
        'strong',
        'em',
        'ul',
        'ol',
        'li',
        'blockquote',
        'code',
        'pre',
        'h1',
        'h2',
        'h3',
        'h4',
        'h5',
        'h6',
        'a',
        'span',
    ],
    allowedAttributes: {
        a: ['href', 'title', 'target', 'rel'],
        '*': ['class'],
    },
    // Disallow JS/data URLs and add rel for safety
    allowedSchemes: ['http', 'https', 'mailto'],
    transformTags: {
        a: sanitizeHtml.simpleTransform('a', { rel: 'noopener noreferrer' }, true),
    },
};

export const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({ content, className, allowHtml = true }) => {
    if (!content) return null;

    const safeHtml = React.useMemo(() => {
        const rawHtml = md.render(content);
        return allowHtml
            ? sanitizeHtml(rawHtml, sanitizeConfig)
            : sanitizeHtml(rawHtml, { allowedTags: [], allowedAttributes: {} });
    }, [content, allowHtml]);

    return <div className={className} dangerouslySetInnerHTML={{ __html: safeHtml }} />;
};

export default MarkdownRenderer;
