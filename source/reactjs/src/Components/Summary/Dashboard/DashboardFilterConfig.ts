import { ISummaryItem } from '../Models/ISummaryData';
import { SummaryTableFieldNames } from '../SummaryTable/SummaryTableFieldNames';

/**
 * Aggregation type for chart data
 */
export type AggregationType = 'count' | 'sum';

/**
 * Configuration for a filterable/chartable property
 */
export interface IFilterablePropertyConfig {
    /** The key used internally (matches ISummaryItem property) */
    key: string;
    /** Human-readable display name */
    displayName: string;
    /** Function to extract the value from ISummaryItem */
    extractor: (item: ISummaryItem) => string;
    /** How to aggregate the data for charts */
    aggregationType: AggregationType;
    /** Function to extract numeric value for sum aggregation (optional) */
    valueExtractor?: (item: ISummaryItem) => number;
    /** Chart colors for this property */
    colors: string[];
    /** Alternative key to use for filtering (optional) */
    altKey?: string;
}

/**
 * Configuration for all chart properties
 */
export class DashboardFilterConfig {
    private static readonly PROPERTY_CONFIGS: Record<string, IFilterablePropertyConfig> = {
        [SummaryTableFieldNames.UnitValue]: {
            key: SummaryTableFieldNames.UnitValue,
            displayName: 'Unit Value',
            extractor: (item: ISummaryItem) => item.UnitOfMeasure || 'Unknown',
            aggregationType: 'sum',
            valueExtractor: (item: ISummaryItem) => {
                const value = parseFloat(item.UnitValue || '0');
                return isNaN(value) ? 0 : value;
            },
            colors: ['#d83b01', '#0078d4', '#107c10', '#b146c2', '#00bcf2', '#ca5010', '#8764b8'],
            altKey: 'UnitOfMeasure',
        },
        AppName: {
            key: 'AppName',
            displayName: 'Application',
            extractor: (item: ISummaryItem) => item.AppName || 'Unknown',
            aggregationType: 'count',
            colors: ['#0078d4', '#107c10', '#d83b01', '#b146c2', '#00bcf2', '#ca5010', '#8764b8'],
        },
        [SummaryTableFieldNames.Submitter]: {
            key: SummaryTableFieldNames.Submitter,
            displayName: 'Submitter',
            extractor: (item: ISummaryItem) => item.Submitter?.name || item.Submitter?.Name || 'Unknown',
            aggregationType: 'count',
            colors: ['#107c10', '#0078d4', '#d83b01', '#b146c2', '#00bcf2', '#ca5010', '#8764b8'],
        },
        [SummaryTableFieldNames.CompanyCode]: {
            key: SummaryTableFieldNames.CompanyCode,
            displayName: 'Company Code',
            extractor: (item: ISummaryItem) => (item.CompanyCode?.trim() || 'Unknown'),
            aggregationType: 'count',
            colors: ['#8764b8', '#ca5010', '#00bcf2', '#b146c2', '#d83b01', '#107c10', '#0078d4'],
        },
    };

    /**
     * Get configuration for a specific property
     */
    static getPropertyConfig(key: string): IFilterablePropertyConfig | undefined {
        return this.PROPERTY_CONFIGS[key];
    }

    /**
     * Get all configured properties
     */
    static getAllProperties(): IFilterablePropertyConfig[] {
        return Object.values(this.PROPERTY_CONFIGS);
    }

    /**
     * Get all property keys
     */
    static getAllPropertyKeys(): string[] {
        return Object.keys(this.PROPERTY_CONFIGS);
    }

    /**
     * Get display name for a property key
     */
    static getDisplayName(key: string): string {
        const config = this.getPropertyConfig(key);
        return config ? config.displayName : key;
    }

    /**
     * Create a filter key for any property
     */
    static createFilterKey(propertyKey: string, value: string): string {
        return `${propertyKey}:${value}`;
    }

    /**
     * Parse a filter key to extract property and value
     */
    static parseFilterKey(filterKey: string): { propertyKey: string; value: string } | null {
        const parts = filterKey.split(':');
        if (parts.length !== 2) {
            return null;
        }

        const [propertyKey, value] = parts;
        
        // Validate that the property key exists in our configuration
        if (!this.PROPERTY_CONFIGS[propertyKey]) {
            return null;
        }

        return { propertyKey, value };
    }

    /**
     * Check if a property key is valid
     */
    static isValidPropertyKey(key: string): boolean {
        return key in this.PROPERTY_CONFIGS;
    }
}