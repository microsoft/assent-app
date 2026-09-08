import * as React from 'react';
import * as Styled from './HistoryStyling';
import TrendsChart from '../Shared/Components/TrendsChart';
import { IHistoryInsights } from '../Shared/SharedComponents.types';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getIsPanelOpen } from '../Shared/SharedComponents.selectors';

const MIN_SEGMENT_WIDTH = 80; // Minimum width for each chart segment in pixels

export function HistoryInsights(props: {
    data: IHistoryInsights;
    windowWidth: number;
    windowHeight: number;
    parentHeight: number;
    parentWidth: number;
}): React.ReactElement {
    const { useSelector } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const isPanelOpen = useSelector(getIsPanelOpen);
    const totalCountsBase = props.data.TotalCounts;
    const getDateIntervals = (n: number, intervalDays = 30): string[] => {
        const intervals: string[] = [];
        const today = new Date();

        for (let i = n - 1; i >= 0; i--) {
            const endDate = new Date(today);
            endDate.setDate(today.getDate() - i * intervalDays);

            const startDate = new Date(endDate);
            startDate.setDate(endDate.getDate() - (intervalDays - 1));

            intervals.push(
                `${startDate.toLocaleDateString('en', {
                    month: '2-digit',
                    day: '2-digit',
                })} - ${endDate.toLocaleDateString('en', { month: '2-digit', day: '2-digit' })}`
            );
        }

        return intervals;
    };

    const getSummedValues = (arr: number[], interval = 2): number[] => {
        if (interval <= 1) return arr;
        const result: number[] = [];
        for (let i = 0; i < arr.length; i += interval) {
            let sum = 0;
            for (let j = 0; j < interval && i + j < arr.length; j++) {
                sum += arr[i + j];
            }
            result.push(sum);
        }
        return result;
    };

    const totalCountsBaseLength = totalCountsBase?.length;
    const containerWidth =
        totalCountsBaseLength < 4
            ? props.parentWidth * 0.35
            : totalCountsBaseLength < 7
            ? props.parentWidth * 0.6
            : props.parentWidth;
    const intervalBaseWidth = containerWidth / totalCountsBaseLength;
    const intervalMultiplier = Math.ceil(MIN_SEGMENT_WIDTH / intervalBaseWidth);
    const totalCountsValues = getSummedValues(totalCountsBase, intervalMultiplier);
    const intervalLabels = React.useMemo(
        () => getDateIntervals(totalCountsBaseLength / intervalMultiplier, 30 * intervalMultiplier),
        [totalCountsBaseLength, intervalMultiplier]
    );

    return (
        <Styled.HistoryInsightsContainer height={props.parentHeight} width={containerWidth}>
            <TrendsChart
                values={totalCountsValues}
                labels={intervalLabels}
                containerHeight={props.parentHeight}
                containerWidth={containerWidth}
                title="Action Trends"
                intervalLength={intervalMultiplier * 30}
            />
        </Styled.HistoryInsightsContainer>
    );
}
