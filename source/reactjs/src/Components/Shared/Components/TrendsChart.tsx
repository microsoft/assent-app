import * as React from 'react';
import { VerticalBarChart, IVerticalBarChartProps, IChartProps } from '@fluentui/react-charting';
import { ChartTitle, ChartWrapper } from '../SharedLayout';
import { Stack } from '@fluentui/react';

interface TrendsChartProps {
    values: number[];
    labels: string[];
    containerHeight: number;
    containerWidth: number;
    title?: string;
    intervalLength?: number;
}

interface ChangeIndicatorProps {
    value: number;
}

const ChangeIndicator: React.FC<ChangeIndicatorProps> = ({ value }) => {
    const displayValue = value > 0 ? `+${value}` : `${value}`;
    return (
        <div
            style={{
                backgroundColor: value >= 0 ? 'rgba(16, 124, 16, 0.2)' : 'rgba(232, 17, 35, 0.2)',
                color: value >= 0 ? '#107C10' : '#E81123',
                padding: '4px 10px',
                borderRadius: '15px',
                display: 'inline-flex',
                alignItems: 'center',
                fontSize: '14px',
                minWidth: '30px',
                justifyContent: 'center',
                marginLeft: '10px',
            }}
        >
            {displayValue}
        </div>
    );
};

export const TrendsChart: React.FC<TrendsChartProps> = ({
    values,
    labels,
    containerWidth,
    containerHeight,
    title,
    intervalLength = 30,
}) => {
    const chartData = values.map((point, index) => ({
        x: labels[index],
        y: point,
        color: '#0078d4',
    }));

    const changeValue = values[values.length - 1] - values[values.length - 2]; // Calculate change from last two points

    const chartHeight = containerHeight * 0.6;
    const chartWidth = containerWidth * 0.98;

    return (
        <ChartWrapper>
            <Stack>
                <Stack.Item>
                    <Stack
                        styles={{
                            root: {
                                paddingTop: chartHeight * 0.15,
                                paddingBottom: chartHeight * 0.05,
                                paddingRight: '25px',
                                paddingLeft: '25px',
                            },
                        }}
                        horizontal
                        horizontalAlign="space-between"
                    >
                        <Stack.Item>
                            <ChartTitle>{title}</ChartTitle>
                        </Stack.Item>
                        <Stack.Item>
                            <ChangeIndicator value={changeValue} /> vs last {intervalLength} days
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
                <Stack.Item>
                    <VerticalBarChart
                        styles={{ root: { height: chartHeight, width: chartWidth } }}
                        height={chartHeight}
                        width={chartWidth}
                        data={chartData}
                        hideLegend={true}
                        xAxisInnerPadding={0.9}
                    />
                </Stack.Item>
            </Stack>
        </ChartWrapper>
    );
};

export default TrendsChart;
