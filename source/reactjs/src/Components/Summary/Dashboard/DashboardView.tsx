import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { ISummaryItem, IDashboardFilters } from '../Models/ISummaryData';
import { DashboardFilterCards } from './DashboardFilterCards';
import { SummaryTable } from '../SummaryTable/SummaryTable';
import { DashboardAnalytics } from './DashboardAnalytics';
import { DashboardAnalyticsHeader } from './DashboardAnalyticsHeader';
import { FilterTags, IFilterTag } from './FilterTags';
import { DashboardFilterUtils } from './DashboardFilterUtils';
import { DashboardFilterConfig } from './DashboardFilterConfig';

const DashboardTableWrapper = styled.div`
    margin-left: -2%;
    margin-right: -2%;
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
`;

const FilterTagsRightAligned = styled.div`
    display: flex;
    justify-content: flex-end;
    align-items: flex-end;
    margin-right: -2%; /* Align with table right edge */
`;

interface IDashboardViewProps {
    summaryData: ISummaryItem[];
    windowWidth: number;
    windowHeight: number;
    isSearchResultsViewOpen?: boolean;
    isPanelOpen?: boolean;
}

export const DashboardView: React.FunctionComponent<IDashboardViewProps> = ({
    summaryData,
    windowWidth,
    windowHeight,
    isSearchResultsViewOpen = false,
    isPanelOpen = false,
}) => {
    const [filters, setFilters] = React.useState<IDashboardFilters>({
        recentSubmissions: false,
        hasErrors: false,
        highPriorityRequests: false,
        selectedProperties: {},
    });

    const [isAnalyticsCollapsed, setIsAnalyticsCollapsed] = React.useState(isPanelOpen);

    // Collapse analytics when panel opens
    React.useEffect(() => {
        if (isPanelOpen) {
            setIsAnalyticsCollapsed(true);
        }
    }, [isPanelOpen]);

    // Filter the data based on active filters
    const filteredData = React.useMemo(() => {
        return DashboardFilterUtils.applyFilters(summaryData, filters);
    }, [summaryData, filters]);

    // Calculate counts for filter cards directly from filtered data (more efficient)
    const filterCounts = React.useMemo(() => {
        const threeDaysAgo = new Date();
        threeDaysAgo.setDate(threeDaysAgo.getDate() - 3);

        return {
            recentSubmissions: filteredData.filter((item) => {
                const submittedDate = new Date(item.SubmittedDate);
                return submittedDate >= threeDaysAgo;
            }).length,
            errors: filteredData.filter((item) => item.LastFailed === true).length,
            highPriority: undefined as number | undefined, // Placeholder until AI logic is implemented
        };
    }, [filteredData]);

    const handleFilterToggle = React.useCallback((filterKey: keyof IDashboardFilters) => {
        if (filterKey === 'selectedProperties') {
            return; // Properties are handled separately
        }
        setFilters((prev) => ({
            ...prev,
            [filterKey]: !prev[filterKey],
        }));
    }, []);

    const handlePropertyFilter = React.useCallback((propertyKey: string, value: string) => {
        setFilters((prev) => DashboardFilterUtils.togglePropertyFilter(prev, propertyKey, value));
    }, []);

    const handleToggleAnalytics = React.useCallback(() => {
        setIsAnalyticsCollapsed((prev) => !prev);
    }, []);

    const handleRemoveFilter = React.useCallback(
        (filterKey: string) => {
            // Handle basic filters
            if (
                filterKey === 'recentSubmissions' ||
                filterKey === 'hasErrors' ||
                filterKey === 'highPriorityRequests'
            ) {
                handleFilterToggle(filterKey as keyof IDashboardFilters);
                return;
            }
            // Handle property filters
            const propertyFilter = DashboardFilterUtils.parseFilterKey(filterKey);
            if (propertyFilter) {
                setFilters((prev) =>
                    DashboardFilterUtils.removePropertyFilter(prev, propertyFilter.propertyKey, propertyFilter.value)
                );
            }
        },
        [handleFilterToggle]
    );

    // Generate active filter tags
    const activeFilterTags = React.useMemo(() => {
        const tags: IFilterTag[] = [];

        // Add basic filter tags
        if (filters.recentSubmissions) {
            tags.push({ key: 'recentSubmissions', label: 'Recent Submissions', type: 'filter' });
        }
        if (filters.hasErrors) {
            tags.push({ key: 'hasErrors', label: 'Failed Requests', type: 'filter' });
        }
        if (filters.highPriorityRequests) {
            tags.push({ key: 'highPriorityRequests', label: 'High Priority Requests', type: 'filter' });
        }

        // Add property filter tags
        Object.entries(filters.selectedProperties).forEach(([propertyKey, values]) => {
            const config = DashboardFilterConfig.getPropertyConfig(propertyKey);
            if (config) {
                values.forEach((value) => {
                    tags.push({
                        key: DashboardFilterUtils.createPropertyFilterKey(propertyKey, value),
                        label: `${config.displayName}: ${value}`,
                        type: 'category',
                    });
                });
            }
        });

        return tags;
    }, [filters.recentSubmissions, filters.hasErrors, filters.highPriorityRequests, filters.selectedProperties]);

    const isDesktop = windowWidth > 1024;

    // Calculate available height to fill parent container efficiently
    // Parent container height is calculated in SummaryLayoutContainer with various offsets
    const searchResultsHeaderHeight = isSearchResultsViewOpen ? 40 : 0; // Account for search results header
    const parentContainerHeight =
        windowWidth < 572
            ? windowWidth < 320
                ? windowHeight - 110 - 40 - searchResultsHeaderHeight
                : windowHeight - 110 - 40 - searchResultsHeaderHeight
            : windowHeight - 145 - 40 - searchResultsHeaderHeight;

    const availableHeight = Math.max(450, parentContainerHeight - 20);
    const mobileAnalyticsHeight = !isDesktop ? 200 : 0;

    const analyticsHeight = availableHeight; // Analytics gets full height to align with table bottom edge

    return (
        <Stack
            horizontal={isDesktop}
            tokens={{ childrenGap: 20, padding: '8px 20px 20px 20px' }}
            styles={{
                root: {
                    height: '100%',
                    maxHeight: '100%',
                    overflow: 'hidden',
                },
            }}
        >
            {/* Main Content Area */}
            <Stack.Item
                grow
                styles={{
                    root: {
                        minWidth: 0,
                        overflow: 'hidden',
                        display: 'flex',
                        flexDirection: 'column',
                        height: '100%',
                    },
                }}
            >
                <Stack
                    tokens={{ childrenGap: 20 }}
                    styles={{
                        root: {
                            height: '100%',
                            display: 'flex',
                            flexDirection: 'column',
                        },
                    }}
                >
                    {/* Mobile analytics - top on mobile */}
                    {!isDesktop && (
                        <Stack.Item styles={{ root: { flexShrink: 0 } }}>
                            <DashboardAnalytics
                                summaryData={filteredData}
                                height={Math.max(180, mobileAnalyticsHeight - 20)}
                                width={windowWidth - 80}
                                onPropertyFilter={handlePropertyFilter}
                                isPanelOpen={isPanelOpen}
                                isCollapsed={isAnalyticsCollapsed}
                                onToggleCollapse={handleToggleAnalytics}
                            />
                        </Stack.Item>
                    )}

                    {/* Filter Cards and Tags Row */}
                    <Stack.Item styles={{ root: { flexShrink: 0 } }}>
                        <Stack
                            horizontal
                            horizontalAlign="space-between"
                            verticalAlign="end"
                            tokens={{ childrenGap: 16 }}
                            styles={{
                                root: {
                                    alignItems: 'flex-end',
                                },
                            }}
                        >
                            {/* Filter Cards - Left side */}
                            <Stack.Item>
                                <DashboardFilterCards
                                    recentSubmissionsCount={filterCounts.recentSubmissions}
                                    errorCount={filterCounts.errors}
                                    highPriorityRequestsCount={filterCounts.highPriority}
                                    activeFilters={filters}
                                    onFilterToggle={handleFilterToggle}
                                    isPanelOpen={isPanelOpen}
                                    isSearchResultsViewOpen={isSearchResultsViewOpen}
                                />
                            </Stack.Item>

                            {/* Right side: Filter Tags and/or Collapsed Analytics Header */}
                            <Stack horizontal tokens={{ childrenGap: 16 }} verticalAlign="end">
                                {/* Filter Tags */}
                                {activeFilterTags.length > 0 && (
                                    <Stack.Item
                                        styles={{
                                            root: {
                                                alignSelf: 'flex-end',
                                            },
                                        }}
                                    >
                                        <FilterTagsRightAligned>
                                            <FilterTags
                                                tags={activeFilterTags}
                                                onRemoveTag={handleRemoveFilter}
                                                showLabel={false}
                                            />
                                        </FilterTagsRightAligned>
                                    </Stack.Item>
                                )}

                                {/* Collapsed Analytics Header - Desktop only */}
                                {isDesktop && isAnalyticsCollapsed && (
                                    <Stack.Item>
                                        <DashboardAnalyticsHeader
                                            isCollapsed={isAnalyticsCollapsed}
                                            onToggleCollapse={handleToggleAnalytics}
                                        />
                                    </Stack.Item>
                                )}
                            </Stack>
                        </Stack>
                    </Stack.Item>

                    {/* Data Table */}
                    <Stack.Item
                        grow
                        styles={{
                            root: {
                                minHeight: 0,
                                flex: 1,
                                display: 'flex',
                                flexDirection: 'column',
                                marginBottom: `${30 + searchResultsHeaderHeight}px`, // Add bottom margin to reduce table height and account for search results header
                            },
                        }}
                    >
                        <DashboardTableWrapper>
                            <SummaryTable
                                tenantGroup={filteredData}
                                tenantName="Dashboard Summary"
                                isSingleGroupShown={true}
                                isDashboardView={true}
                            />
                        </DashboardTableWrapper>
                    </Stack.Item>
                </Stack>
            </Stack.Item>

            {/* Analytics Panel - Right side on desktop only when expanded */}
            {isDesktop && !isAnalyticsCollapsed && (
                <Stack.Item
                    styles={{
                        root: {
                            width: '300px',
                            minWidth: '300px',
                            flexShrink: 0,
                            height: `${analyticsHeight}px`,
                        },
                    }}
                >
                    <DashboardAnalytics
                        summaryData={filteredData}
                        height={analyticsHeight}
                        width={280}
                        onPropertyFilter={handlePropertyFilter}
                        isPanelOpen={isPanelOpen}
                        isCollapsed={isAnalyticsCollapsed}
                        onToggleCollapse={handleToggleAnalytics}
                    />
                </Stack.Item>
            )}
        </Stack>
    );
};
