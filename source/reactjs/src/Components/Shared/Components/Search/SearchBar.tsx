import * as React from 'react';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { SearchBox, ISearchBox } from '@fluentui/react/lib/SearchBox';
import { IconButton } from '@fluentui/react/lib/Button';
import { Stack } from '@fluentui/react/lib/Stack';
import { initiateSearch, toggleSearchResultsView, updatePanelState, requestSuggest, clearSuggest, setSelectedSumaryTileRef } from '../../SharedComponents.actions';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getIsSearchResultsViewOpen, getSummary, getIsLoadingSummary, getFilteredSuggest, getIsLoadingSuggest } from '../../SharedComponents.selectors';
import { getUserAlias } from '../../SharedComponents.persistent-selectors';
import { useHistory } from 'react-router-dom';
import { NavigationUtils } from '../../Utils/NavigationUtils';
import { isMediumResolution } from '../../../../Helpers/sharedHelpers';
import { CoherenceColors } from '../../SharedColors';
import { SearchSuggestionDropdown } from './SearchSuggestionDropdown';
import { ISuggestRequestItem } from '../../SharedComponents.action-types';
import { IComponentsAppState } from '../../SharedComponents.types';
import { updateMyRequest } from '../../Details/Details.actions';
import { getDisplayDocumentNumber } from '../../Details/Details.selectors';
import { trackBusinessProcessEvent, TrackingEventId } from '../../../../Helpers/telemetryHelpers';
import { useFlighting } from '../FlightingHandler';

const headerButtonStyles = {
    icon: { color: 'white' },
    rootHovered: { backgroundColor: CoherenceColors.blueInteractive },
    rootPressed: { backgroundColor: CoherenceColors.blueInteractive },
    root: {
        selectors: {
            ':focus': { border: '1px solid black' },
            ':focus::after': { outline: 'none !important' },
        },
    },
};

const searchBoxIconProps = {
    styles: {
        root: {
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
        },
    },
    imageProps: {
        src: '/icons/ic_fluent_search_sparkle_24_regular.svg',
        alt: 'Search',
        width: 20,
        height: 20,
        style: { display: 'block', margin: '0 auto' },
    },
};

export function SearchBar(props: {
    screenWidth: number;
    onExpandedChange?: (expanded: boolean) => void;
}): React.ReactElement {
    const { useSelector, dispatch, telemetryClient, authClient } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { screenWidth, onExpandedChange } = props;
    const [searchValue, setSearchValue] = React.useState<string>('');
    const [userClearedSearch, setUserClearedSearch] = React.useState<boolean>(false);
    const [hasInitiatedSearchFromUrl, setHasInitiatedSearchFromUrl] = React.useState<boolean>(false);
    const [isExpanded, setIsExpanded] = React.useState(false);
    const isSearchResultsViewOpen = useSelector(getIsSearchResultsViewOpen);
    const userAlias = useSelector(getUserAlias);
    const summary = useSelector(getSummary);
    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const isLoadingSuggest = useSelector(getIsLoadingSuggest);
    const filtered = useSelector((state: IComponentsAppState) => getFilteredSuggest(state, searchValue));
    const suggestTerms = filtered.terms;
    const suggestRequests = filtered.requests;
    const selectedDisplayDocumentNumber = useSelector(getDisplayDocumentNumber as any);
    const [isDropdownVisible, setIsDropdownVisible] = React.useState(false);
    const [activeIndex, setActiveIndex] = React.useState(-1);

    const wrapperRef = React.useRef<HTMLDivElement>(null);
    const hasTrackedShownRef = React.useRef<string | null>(null);
    const searchBoxRef = React.useRef<ISearchBox>(null);
    const triggerButtonRef = React.useRef<HTMLDivElement>(null);
    const prevExpandedRef = React.useRef(false);
    const history = useHistory();

    const isCompact = isMediumResolution(screenWidth);
    const isDeepSearchFlighted = useFlighting('DeepSearch');

    // Auto-collapse when viewport becomes non-compact
    React.useEffect(() => {
        if (!isCompact) {
            setIsExpanded(false);
        }
    }, [isCompact]);

    // Notify parent of expanded state changes; reset on unmount
    React.useEffect(() => {
        onExpandedChange?.(isCompact && isExpanded);
        return () => {
            onExpandedChange?.(false);
        };
    }, [isCompact, isExpanded, onExpandedChange]);

    // Manage focus when toggling between expanded and collapsed
    React.useEffect(() => {
        const wasExpanded = prevExpandedRef.current;
        prevExpandedRef.current = isExpanded;

        if (isExpanded && !wasExpanded) {
            searchBoxRef.current?.focus();
        } else if (!isExpanded && wasExpanded && isCompact) {
            const button = triggerButtonRef.current?.querySelector('button');
            button?.focus();
        }
    }, [isExpanded, isCompact]);

    React.useEffect(() => {
        if (!isSearchResultsViewOpen) {
            setSearchValue('');
        }
    }, [isSearchResultsViewOpen]);

    const trackSuggestEvent = (
        businessProcessName: string,
        appAction: string,
        eventId: number,
        additionalProperties: any = {}
    ): void => {
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            businessProcessName,
            appAction,
            eventId,
            {},
            additionalProperties
        );
    };

    const handleSearch = (input: string): void => {
        if (input) {
            NavigationUtils.performSearch(history, input);
        } else {
            NavigationUtils.clearSearch(history);
        }

        setUserClearedSearch(false);
        dispatch(updatePanelState(false));
        dispatch(initiateSearch(userAlias, input));
    };

    const handleCollapse = (): void => {
        setIsExpanded(false);
    };

    const handleTermClick = (term: string, selectionMethod: 'mouse' | 'keyboard' = 'mouse'): void => {
        trackSuggestEvent(
            'Search suggest - Term selected',
            'MSApprovals.SearchSuggest.TermSelected',
            TrackingEventId.SearchSuggestTermSelected,
            {
                Query: searchValue,
                SelectedTerm: term,
                SelectedIndex: suggestTerms.indexOf(term),
                SelectionMethod: selectionMethod,
                TermCount: suggestTerms.length,
                RequestCount: suggestRequests.length,
            }
        );
        setSearchValue(term);
        setIsDropdownVisible(false);
        handleSearch(term);
    };

    const handleRequestClick = (request: ISuggestRequestItem, selectionMethod: 'mouse' | 'keyboard' = 'mouse'): void => {
        trackSuggestEvent(
            'Search suggest - Request selected',
            'MSApprovals.SearchSuggest.RequestSelected',
            TrackingEventId.SearchSuggestRequestSelected,
            {
                Query: searchValue,
                DocumentNumber: request.documentNumber,
                DisplayDocumentNumber: request.displayDocumentNumber,
                TenantId: request.tenantId,
                SelectedIndex: suggestRequests.findIndex((r) => r.documentNumber === request.documentNumber),
                SelectionMethod: selectionMethod,
                TermCount: suggestTerms.length,
                RequestCount: suggestRequests.length,
            }
        );
        setIsDropdownVisible(false);
        setActiveIndex(-1);

        const summaryItem = summary?.find(
            (s: any) => s.ApprovalIdentifier?.DocumentNumber === request.documentNumber
        );
        if (summaryItem) {
            const cardRef = `${request.displayDocumentNumber}_${summaryItem.TenantId}`;

            const isAlreadySelected = selectedDisplayDocumentNumber === request.displayDocumentNumber;

            if (!isAlreadySelected) {
                dispatch(
                    updateMyRequest(
                        Number(summaryItem.TenantId),
                        request.documentNumber,
                        request.displayDocumentNumber,
                        summaryItem.ApprovalIdentifier?.FiscalYear || '',
                        summaryItem
                    )
                );
                dispatch(setSelectedSumaryTileRef(cardRef));
            }
            dispatch(updatePanelState(true));
            NavigationUtils.navigateToDetails(
                history,
                Number(summaryItem.TenantId),
                request.displayDocumentNumber
            );

            // Collapse compact-expanded mode after selecting a request
            if (isCompact && isExpanded) {
                setIsExpanded(false);
            }

            setTimeout(() => {
                const cardEl = document.getElementById(cardRef);
                if (cardEl) {
                    cardEl.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
            }, 100);
        }
    };

    const handleSuggestKeyDown = (e: React.KeyboardEvent): void => {
        const totalItems = suggestTerms.length + suggestRequests.length;
        if (totalItems === 0 || !isDropdownVisible) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setActiveIndex((prev) => (prev + 1) % totalItems);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setActiveIndex((prev) => (prev - 1 + totalItems) % totalItems);
        } else if (e.key === 'Enter' && activeIndex >= 0) {
            e.preventDefault();
            if (activeIndex < suggestTerms.length) {
                handleTermClick(suggestTerms[activeIndex], 'keyboard');
            } else {
                handleRequestClick(suggestRequests[activeIndex - suggestTerms.length], 'keyboard');
            }
        } else if (e.key === 'Escape' && !(isCompact && isExpanded)) {
            // In compact expanded mode, the SearchBox's onEscape collapses the search instead.
            trackSuggestEvent(
                'Search suggest - Dismissed',
                'MSApprovals.SearchSuggest.Dismissed',
                TrackingEventId.SearchSuggestDismissed,
                {
                    Query: searchValue,
                    DismissMethod: 'escape',
                    TermCount: suggestTerms.length,
                    RequestCount: suggestRequests.length,
                }
            );
            setIsDropdownVisible(false);
            setActiveIndex(-1);
        }
    };

    // Handle search from URL parameter
    React.useEffect(() => {
        const searchParams = new URLSearchParams(history.location.search);
        const filterFromUrl = searchParams.get('filter');

        if (filterFromUrl) {
            setSearchValue(filterFromUrl);

            if (!hasInitiatedSearchFromUrl && !isLoadingSummary && summary && summary.length > 0) {
                dispatch(updatePanelState(false));
                dispatch(initiateSearch(userAlias, filterFromUrl));
                setHasInitiatedSearchFromUrl(true);
            }
        } else {
            setSearchValue('');
            setHasInitiatedSearchFromUrl(false);
        }
    }, [history.location.search, userAlias, summary, hasInitiatedSearchFromUrl, isLoadingSummary]);

    // Reset search flag when user changes
    React.useEffect(() => {
        setHasInitiatedSearchFromUrl(false);
    }, [userAlias]);

    React.useEffect(() => {
        if (!searchValue && userClearedSearch && history.location.search.includes('filter=')) {
            NavigationUtils.clearSearch(history);
            dispatch(toggleSearchResultsView(false));
        }
    }, [searchValue, userClearedSearch, history.location.search, history, dispatch]);

    React.useEffect(() => {
        if (isSearchResultsViewOpen === false && searchValue) {
            setSearchValue('');
        }
    }, [isSearchResultsViewOpen]);

    React.useEffect(() => {
        if (!isDeepSearchFlighted) {
            return;
        }
        if (searchValue.length >= 3) {
            dispatch(requestSuggest(searchValue));
        } else {
            dispatch(clearSuggest());
        }
    }, [searchValue, dispatch, isDeepSearchFlighted]);

    // Show dropdown immediately when query is long enough (shimmer appears while loading)
    React.useEffect(() => {
        if (isDeepSearchFlighted && searchValue.length >= 3) {
            setIsDropdownVisible(true);
            setActiveIndex(-1);
        } else {
            setIsDropdownVisible(false);
            hasTrackedShownRef.current = null;
        }
    }, [searchValue, isDeepSearchFlighted]);

    // Track "dropdown shown with results" — only once per query change
    React.useEffect(() => {
        const hasResults = suggestTerms.length > 0 || suggestRequests.length > 0;
        if (isDropdownVisible && hasResults && hasTrackedShownRef.current !== searchValue) {
            hasTrackedShownRef.current = searchValue;
            trackSuggestEvent(
                'Search suggest - Dropdown shown',
                'MSApprovals.SearchSuggest.DropdownShown',
                TrackingEventId.SearchSuggestDropdownShown,
                {
                    Query: searchValue,
                    TermCount: suggestTerms.length,
                    RequestCount: suggestRequests.length,
                }
            );
        }
    }, [isDropdownVisible, suggestTerms.length, suggestRequests.length, searchValue]);

    // Click-outside dismiss
    React.useEffect(() => {
        if (!isDropdownVisible) return;

        function handleClickOutside(e: MouseEvent): void {
            if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
                trackSuggestEvent(
                    'Search suggest - Dismissed',
                    'MSApprovals.SearchSuggest.Dismissed',
                    TrackingEventId.SearchSuggestDismissed,
                    {
                        Query: searchValue,
                        DismissMethod: 'click-outside',
                        TermCount: suggestTerms.length,
                        RequestCount: suggestRequests.length,
                    }
                );
                setIsDropdownVisible(false);
                setActiveIndex(-1);
            }
        }

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, [isDropdownVisible]);

    // Re-show dropdown when search box is focused and has a valid query
    const handleWrapperFocus = (): void => {
        if (isDeepSearchFlighted && searchValue.length >= 3 && !isDropdownVisible) {
            setIsDropdownVisible(true);
        }
    };

    const placeholderText = 'Search pending requests';

    const activeItemId = activeIndex >= 0 ? `suggest-item-${activeIndex}` : undefined;
    const dropdownAriaProps = isDropdownVisible && (suggestTerms.length > 0 || suggestRequests.length > 0)
        ? {
            'aria-controls': 'search-suggestions',
            'aria-activedescendant': activeItemId,
            'aria-expanded': true as boolean,
        }
        : { 'aria-expanded': false as boolean };

    const dropdown = (
        <SearchSuggestionDropdown
            terms={suggestTerms}
            requests={suggestRequests}
            isVisible={isDropdownVisible}
            isLoading={isLoadingSuggest}
            activeIndex={activeIndex}
            query={searchValue}
            onTermClick={handleTermClick}
            onRequestClick={handleRequestClick}
        />
    );

    // Inline onSearch handler used by both expanded compact and normal inline modes.
    // Fluent SearchBox fires onSearch on Enter and does NOT forward Enter to our onKeyDown,
    // so we must handle keyboard-selected suggestions here.
    const handleSearchBoxEnter = (newValue: string): void => {
        if (isDropdownVisible && activeIndex >= 0 && (suggestTerms.length + suggestRequests.length) > 0) {
            if (activeIndex < suggestTerms.length) {
                handleTermClick(suggestTerms[activeIndex], 'keyboard');
            } else {
                handleRequestClick(suggestRequests[activeIndex - suggestTerms.length], 'keyboard');
            }
            return;
        }
        handleSearch(newValue);
        setIsDropdownVisible(false);
    };

    // Compact collapsed: show search icon button
    if (isCompact && !isExpanded) {
        return (
            <div ref={triggerButtonRef as React.RefObject<HTMLDivElement>}>
                <IconButton
                    iconProps={{ iconName: 'Search' }}
                    styles={headerButtonStyles}
                    onClick={() => setIsExpanded(true)}
                    title="Search"
                    aria-label="Search pending requests"
                    id="SearchTopHeaderButton"
                />
            </div>
        );
    }

    // Compact expanded: back button + full-width search with suggest dropdown
    if (isCompact && isExpanded) {
        return (
            <Stack horizontal verticalAlign="center" styles={{ root: { width: '100%' } }}>
                <IconButton
                    iconProps={{ iconName: 'ChromeBack' }}
                    styles={headerButtonStyles}
                    onClick={handleCollapse}
                    title="Close search"
                    aria-label="Close search"
                />
                <Stack.Item grow styles={{ root: { minWidth: 0 } }}>
                    <div
                        ref={wrapperRef}
                        style={{ position: 'relative' }}
                        onFocus={handleWrapperFocus}
                        role="combobox"
                        aria-haspopup="listbox"
                        aria-owns="search-suggestions"
                        {...dropdownAriaProps}
                    >
                        <SearchBox
                            componentRef={searchBoxRef}
                            styles={{ root: { width: '100%' } }}
                            placeholder={placeholderText}
                            ariaLabel={placeholderText}
                            onSearch={handleSearchBoxEnter}
                            onEscape={handleCollapse}
                            iconProps={searchBoxIconProps}
                            onClear={() => {
                                setUserClearedSearch(true);
                                setSearchValue('');
                                setIsDropdownVisible(false);
                            }}
                            onChange={(_, newValue): void => {
                                setSearchValue(newValue ?? '');
                                setUserClearedSearch(!newValue);
                            }}
                            onKeyDown={handleSuggestKeyDown}
                            value={searchValue}
                            aria-autocomplete="list"
                        />
                        {dropdown}
                    </div>
                </Stack.Item>
            </Stack>
        );
    }

    // Normal inline mode (desktop)
    return (
        <div
            ref={wrapperRef}
            style={{ position: 'relative' }}
            onFocus={handleWrapperFocus}
            role="combobox"
            aria-haspopup="listbox"
            aria-owns="search-suggestions"
            {...dropdownAriaProps}
        >
            <SearchBox
                styles={{ root: { width: screenWidth * 0.3 } }}
                placeholder={placeholderText}
                ariaLabel={placeholderText}
                onSearch={handleSearchBoxEnter}
                iconProps={searchBoxIconProps}
                onClear={() => {
                    setUserClearedSearch(true);
                    setSearchValue('');
                    setIsDropdownVisible(false);
                }}
                onChange={(_, newValue): void => {
                    setSearchValue(newValue ?? '');
                    setUserClearedSearch(!newValue);
                }}
                onKeyDown={handleSuggestKeyDown}
                value={searchValue}
                aria-autocomplete="list"
            />
            {dropdown}
        </div>
    );
}
