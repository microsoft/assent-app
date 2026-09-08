import { IDashboardFilters, ISummaryItem } from '../Models/ISummaryData';
import { DashboardFilterConfig } from './DashboardFilterConfig';

export interface IPropertyFilterPayload {
    propertyKey: string;
    value: string;
}

/**
 * Type for filter predicate functions
 */
export type FilterPredicate = (item: ISummaryItem) => boolean;

/**
 * Utility class for managing dashboard filters using a generic, configuration-driven approach
 */
export class DashboardFilterUtils {
    /**
     * Toggle a property filter value on/off
     */
    static togglePropertyFilter(
        currentFilters: IDashboardFilters,
        propertyKey: string,
        value: string
    ): IDashboardFilters {
        // Validate the property key
        if (!DashboardFilterConfig.isValidPropertyKey(propertyKey)) {
            return currentFilters;
        }

        const currentValues = currentFilters.selectedProperties[propertyKey] || [];
        const isActive = currentValues.includes(value);

        return {
            ...currentFilters,
            selectedProperties: {
                ...currentFilters.selectedProperties,
                [propertyKey]: isActive ? currentValues.filter((v: string) => v !== value) : [...currentValues, value],
            },
        };
    }

    /**
     * Remove a specific property filter
     */
    static removePropertyFilter(
        currentFilters: IDashboardFilters,
        propertyKey: string,
        value: string
    ): IDashboardFilters {
        const currentValues = currentFilters.selectedProperties[propertyKey] || [];
        
        return {
            ...currentFilters,
            selectedProperties: {
                ...currentFilters.selectedProperties,
                [propertyKey]: currentValues.filter((v: string) => v !== value),
            },
        };
    }

    /**
     * Clear all filters for a specific property
     */
    static clearPropertyFilters(currentFilters: IDashboardFilters, propertyKey: string): IDashboardFilters {
        return {
            ...currentFilters,
            selectedProperties: {
                ...currentFilters.selectedProperties,
                [propertyKey]: [],
            },
        };
    }

    /**
     * Parse filter key to extract property and value information
     */
    static parseFilterKey(filterKey: string): IPropertyFilterPayload | null {
        const result = DashboardFilterConfig.parseFilterKey(filterKey);
        if (!result) {
            return null;
        }

        return {
            propertyKey: result.propertyKey,
            value: result.value,
        };
    }

    /**
     * Create a filter key for any property filter
     */
    static createPropertyFilterKey(propertyKey: string, value: string): string {
        return DashboardFilterConfig.createFilterKey(propertyKey, value);
    }

    /**
     * Apply all active filters to the data
     */
    static applyFilters(data: ISummaryItem[], filters: IDashboardFilters): ISummaryItem[] {
        let filtered = [...data];

        // Apply basic filters
        if (filters.recentSubmissions) {
            filtered = this.applyRecentSubmissionsFilter(filtered);
        }

        if (filters.hasErrors) {
            filtered = this.applyErrorsFilter(filtered);
        }

        if (filters.highPriorityRequests) {
            filtered = this.applyHighPriorityFilter(filtered);
        }

        // Apply property filters
        filtered = this.applyPropertyFilters(filtered, filters.selectedProperties);

        return filtered;
    }

    /**
     * Filter for recent submissions (last 3 days)
     */
    private static applyRecentSubmissionsFilter(data: ISummaryItem[]): ISummaryItem[] {
        const threeDaysAgo = new Date();
        threeDaysAgo.setDate(threeDaysAgo.getDate() - 3);

        return data.filter((item) => {
            const submittedDate = new Date(item.SubmittedDate);
            return submittedDate >= threeDaysAgo;
        });
    }

    /**
     * Filter for items with errors
     */
    private static applyErrorsFilter(data: ISummaryItem[]): ISummaryItem[] {
        return data.filter((item) => item.LastFailed === true);
    }

    /**
     * Filter for high priority requests (placeholder for future AI implementation)
     */
    private static applyHighPriorityFilter(data: ISummaryItem[]): ISummaryItem[] {
        // TODO: Implement high priority requests filtering logic based on AI criteria
        // For now, this filter won't actually filter the data until the AI logic is implemented
        return data;
    }

    /**
     * Apply property filters using the configuration-driven approach
     */
    private static applyPropertyFilters(
        data: ISummaryItem[],
        propertyFilters: Record<string, string[]>
    ): ISummaryItem[] {
        let filtered = data;

        // Apply each property filter
        Object.entries(propertyFilters).forEach(([propertyKey, values]) => {
            if (values.length > 0) {
                const config = DashboardFilterConfig.getPropertyConfig(propertyKey);
                if (config) {
                    filtered = filtered.filter((item) => values.includes(config.extractor(item)));
                }
            }
        });

        return filtered;
    }
}
