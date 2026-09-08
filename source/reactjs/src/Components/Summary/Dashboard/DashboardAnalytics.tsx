import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { Text } from '@fluentui/react/lib/Text';
import { Toggle } from '@fluentui/react/lib/Toggle';
import { DonutChart, IChartProps, IChartDataPoint } from '@fluentui/react-charting';
import { ISummaryItem } from '../Models/ISummaryData';
import { DashboardFilterConfig } from './DashboardFilterConfig';
import { DashboardAnalyticsHeader } from './DashboardAnalyticsHeader';
import { breakpointMap } from '../../Shared/Styles/Media';

interface IDashboardAnalyticsProps {
    summaryData: ISummaryItem[];
    height: number;
    width?: number;
    windowWidth: number;
    onPropertyFilter?: (propertyKey: string, value: string) => void;
    isCollapsed: boolean;
    onToggleCollapse: () => void;
    isBulkSelected?: boolean;
    selectedDisplayDocumentNumbers?: string[];
}

export const DashboardAnalytics: React.FunctionComponent<IDashboardAnalyticsProps> = ({
    summaryData,
    windowWidth,
    height,
    width,
    onPropertyFilter,
    isCollapsed,
    onToggleCollapse,
    isBulkSelected = false,
    selectedDisplayDocumentNumbers = [],
}) => {
    const [reflectSelection, setReflectSelection] = React.useState(false);
    const hasSelection = selectedDisplayDocumentNumbers.length > 0;

    // Filter summary data based on reflectSelection toggle
    const filteredSummaryData = React.useMemo(() => {
        if (!reflectSelection || !hasSelection) {
            return summaryData;
        }
        return summaryData.filter(item => 
            selectedDisplayDocumentNumbers.includes(item.ApprovalIdentifier?.DisplayDocumentNumber)
        );
    }, [summaryData, reflectSelection, hasSelection, selectedDisplayDocumentNumbers]);

    // Determine which properties to show based on context
    const propertiesToShow = React.useMemo(() => {
        const allProperties = DashboardFilterConfig.getAllProperties();
        if (isBulkSelected) {
            // In bulk selection mode, show Submitter instead of AppName
            return allProperties.filter(prop => prop.key !== 'AppName');
        } else {
            // In normal mode, show AppName instead of Submitter
            return allProperties.filter(prop => prop.key !== 'Submitter');
        }
    }, [isBulkSelected]);

    // Generic function to generate chart data for any property
    const generateChartData = React.useCallback(
        (propertyKey: string): IChartDataPoint[] => {
            const config = DashboardFilterConfig.getPropertyConfig(propertyKey);
            if (!config) {
                return [];
            }

            const aggregatedData = filteredSummaryData.reduce((acc, item) => {
                const key = config.extractor(item) || 'Unknown';
                const value =
                    config.aggregationType === 'sum' && config.valueExtractor ? config.valueExtractor(item) : 1;
                acc[key] = (acc[key] || 0) + value;
                return acc;
            }, {} as Record<string, number>);

            return Object.entries(aggregatedData).map(
                ([name, value], index): IChartDataPoint => ({
                    legend: config.aggregationType === 'sum' ? `${name}: ${value.toFixed(2)}` : name,
                    data: value,
                    color: config.colors.length > 0 ? config.colors[index % config.colors.length] : '#000000', // default to black if colors array is empty
                    onClick: () => {
                        if (onPropertyFilter) {
                            onPropertyFilter(propertyKey, name);
                        }
                    },
                })
            );
        },
        [filteredSummaryData, onPropertyFilter]
    );

    // Generate chart data for each configured property
    const chartData = React.useMemo(() => {
        return propertiesToShow.reduce((acc, config) => {
            acc[config.key] = generateChartData(config.key);
            return acc;
        }, {} as Record<string, IChartDataPoint[]>);
    }, [generateChartData, propertiesToShow]);

    const chartHeight = Math.max(150, (height - 120) / 3);
    const chartWidth = width ? Math.max(260, width - 20) : 280;

    // Create chart props for each configured property
    const getChartProps = (propertyKey: string): IChartProps => ({
        chartData: chartData[propertyKey] || [],
    });

    return (
        <Stack
            styles={{
                root: {
                    height: isCollapsed ? 'auto' : windowWidth <= breakpointMap.m ? '16rem' : `${height}px`,
                    backgroundColor: '#faf9f8',
                    borderRadius: '8px',
                    border: '1px solid #edebe9',
                    display: 'flex',
                    flexDirection: 'column',
                },
            }}
        >
            {/* Fixed Title Header with Collapse Button */}
            <DashboardAnalyticsHeader
                isCollapsed={isCollapsed}
                onToggleCollapse={onToggleCollapse}
                textVariant="medium"
                containerStyles={{
                    root: {
                        padding: '8px 16px 6px 16px',
                        borderBottom: isCollapsed ? 'none' : '1px solid #edebe9',
                        backgroundColor: '#faf9f8',
                        borderRadius: '8px 8px 0 0',
                        flexShrink: 0,
                    },
                }}
                textStyles={{
                    root: {
                        fontWeight: '500',
                        color: '#323130',
                        fontSize: '15px',
                    },
                }}
            />

            {/* Reflect Selection Toggle */}
            {!isCollapsed && hasSelection && (
                <Stack
                    styles={{
                        root: {
                            padding: '8px 16px',
                            borderBottom: '1px solid #edebe9',
                            backgroundColor: '#faf9f8',
                            flexShrink: 0,
                        },
                    }}
                >
                    <Toggle
                        label="Reflect selection"
                        checked={reflectSelection}
                        onChange={(_, checked) => setReflectSelection(!!checked)}
                        inlineLabel
                        styles={{
                            root: { marginBottom: 0 },
                            label: {
                                fontSize: '13px',
                                fontWeight: '500',
                                color: '#605e5c',
                            },
                        }}
                    />
                </Stack>
            )}

            {/* Scrollable Content */}
            {!isCollapsed && (
                <Stack
                    tokens={{ childrenGap: 20, padding: '20px' }}
                    styles={{
                        root: {
                            flex: '1 1 auto',
                            overflowY: 'auto',
                            minHeight: 0,
                        },
                    }}
                >
                    {/* Render charts for each configured property */}
                    {propertiesToShow.map((config) => {
                        const data = chartData[config.key] || [];
                        const chartProps = getChartProps(config.key);

                        return (
                            <Stack key={config.key} tokens={{ childrenGap: 8 }}>
                                <Text
                                    variant="medium"
                                    styles={{
                                        root: {
                                            fontWeight: '500',
                                            color: '#605e5c',
                                            fontSize: '14px',
                                        },
                                    }}
                                >
                                    {config.displayName} {config.aggregationType === 'sum' ? 'Summation' : 'Breakdown'}
                                </Text>
                                {data.length > 0 ? (
                                    <DonutChart
                                        data={chartProps}
                                        innerRadius={40}
                                        width={chartWidth}
                                        height={chartHeight}
                                        hideLabels
                                        showLabelsInPercent
                                    />
                                ) : (
                                    <Text variant="medium" styles={{ root: { color: '#8a8886' } }}>
                                        No data available
                                    </Text>
                                )}
                            </Stack>
                        );
                    })}
                </Stack>
            )}
        </Stack>
    );
};
