import * as React from 'react';
import { ISuggestRequestItem } from '../../SharedComponents.action-types';
import { formatUnitValue } from '../../../../Helpers/sharedHelpers';
import {
    DropdownContainer,
    SectionHeader,
    Separator,
    TermItem,
    RequestItem,
    RequestTitle,
    RequestSubtitle,
    FooterHint,
    HighlightMatch,
    ShimmerBar,
    ScreenReaderOnly,
} from './SearchSuggestionDropdown.styled';

export interface SearchSuggestionDropdownProps {
    terms: string[];
    requests: ISuggestRequestItem[];
    isVisible: boolean;
    isLoading: boolean;
    activeIndex: number;
    query: string;
    onTermClick: (term: string) => void;
    onRequestClick: (request: ISuggestRequestItem) => void;
}

const SearchIcon = (): React.ReactElement => (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#616161" strokeWidth="2"
        style={{ marginRight: 12, flexShrink: 0 }}>
        <circle cx="11" cy="11" r="8" />
        <line x1="21" y1="21" x2="16.65" y2="16.65" />
    </svg>
);

const DocumentIcon = (): React.ReactElement => (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#616161" strokeWidth="2"
        style={{ marginRight: 12, marginTop: 2, flexShrink: 0 }}>
        <rect x="3" y="3" width="18" height="18" rx="2" />
        <line x1="9" y1="9" x2="15" y2="9" />
        <line x1="9" y1="13" x2="15" y2="13" />
    </svg>
);

function renderHighlighted(text: string, query: string): React.ReactNode {
    if (!query || query.length < 2) return text;
    const lowerText = text.toLowerCase();
    const lowerQuery = query.toLowerCase();
    const matchIndex = lowerText.indexOf(lowerQuery);
    if (matchIndex === -1) return text;

    const before = text.slice(0, matchIndex);
    const match = text.slice(matchIndex, matchIndex + query.length);
    const after = text.slice(matchIndex + query.length);

    return (
        <>
            {before}
            <HighlightMatch>{match}</HighlightMatch>
            {after}
        </>
    );
}

export function SearchSuggestionDropdown(props: SearchSuggestionDropdownProps): React.ReactElement | null {
    const { terms, requests, isVisible, isLoading, activeIndex, query, onTermClick, onRequestClick } = props;

    // Keep the active item visible when navigating with arrow keys.
    // Declared before any early returns to satisfy the Rules of Hooks.
    React.useEffect(() => {
        if (activeIndex < 0) return;
        const el = document.getElementById(`suggest-item-${activeIndex}`);
        if (el && typeof el.scrollIntoView === 'function') {
            el.scrollIntoView({ block: 'nearest' });
        }
    }, [activeIndex]);

    if (!isVisible) {
        return null;
    }

    const hasResults = terms.length > 0 || requests.length > 0;
    const showShimmer = isLoading && !hasResults;
    const totalTerms = terms.length;
    const totalItems = terms.length + requests.length;

    if (!hasResults && !showShimmer) {
        return null;
    }

    return (
        <>
            <DropdownContainer
                role="listbox"
                id="search-suggestions"
                aria-label="Search suggestions"
                tabIndex={-1}
            >
                {showShimmer && (
                    <>
                        <ShimmerBar />
                        <ShimmerBar />
                        <ShimmerBar />
                    </>
                )}

                {terms.length > 0 && (
                <>
                    <SectionHeader>Suggested terms</SectionHeader>
                    {terms.map((term, i) => (
                        <TermItem
                            key={`term-${i}`}
                            id={`suggest-item-${i}`}
                            role="option"
                            aria-selected={activeIndex === i}
                            isActive={activeIndex === i}
                            onMouseDown={(e) => {
                                e.preventDefault();
                                onTermClick(term);
                            }}
                        >
                            <SearchIcon />
                            <span>{renderHighlighted(term, query)}</span>
                        </TermItem>
                    ))}
                </>
            )}

            {terms.length > 0 && requests.length > 0 && <Separator />}

            {requests.length > 0 && (
                <>
                    <SectionHeader>Matching requests</SectionHeader>
                    {requests.map((request, i) => {
                        const flatIndex = totalTerms + i;
                        return (
                            <RequestItem
                                key={`request-${i}`}
                                id={`suggest-item-${flatIndex}`}
                                role="option"
                                aria-selected={activeIndex === flatIndex}
                                isActive={activeIndex === flatIndex}
                                onMouseDown={(e) => {
                                    e.preventDefault();
                                    onRequestClick(request);
                                }}
                            >
                                <DocumentIcon />
                                <div style={{ minWidth: 0, flex: 1 }}>
                                    <RequestTitle>
                                        {renderHighlighted(request.title || request.displayDocumentNumber, query)}
                                        {request.unitValueText && <> · {renderHighlighted(formatUnitValue(request.unitValueText), query)}</>}
                                    </RequestTitle>
                                    <RequestSubtitle>
                                        {renderHighlighted(request.submitterName || '', query)}
                                        {request.displayDocumentNumber && <> · {renderHighlighted(request.displayDocumentNumber, query)}</>}
                                    </RequestSubtitle>
                                </div>
                            </RequestItem>
                        );
                    })}
                </>
            )}

            {hasResults && (
                <FooterHint>↑↓ navigate · Enter select · Esc dismiss</FooterHint>
            )}
            </DropdownContainer>

            <ScreenReaderOnly role="status" aria-live="polite" aria-atomic="true">
                {totalItems} suggestions available
            </ScreenReaderOnly>
        </>
    );
}
