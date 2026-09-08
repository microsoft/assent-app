import * as React from 'react';
import { Stack, Text, IconButton, mergeStyles } from '@fluentui/react';
import { MarkdownRenderer } from '../../Markdown/MarkdownRenderer';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../../Helpers/telemetryHelpers';
import { getSummaryCommonPropertiesSelector } from '../../SharedComponents.selectors';

export const SummarizeSection: React.FC<{ summary: string }> = ({ summary }) => {
    const [isCollapsed, setIsCollapsed] = React.useState(true);
    const { useSelector, authClient, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const summaryCommonProperties = useSelector(getSummaryCommonPropertiesSelector);

    const summarizeIconClass = React.useMemo(
        () =>
            mergeStyles({
                width: 16,
                height: 16,
                display: 'block',
            }),
        []
    );

    const handleToggle = (): void => {
        const nextIsCollapsed = !isCollapsed;
        const isExpand = !nextIsCollapsed;
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'SummarySection',
            isExpand ? 'MSApprovals.SummarySectionExpand' : 'MSApprovals.SummarySectionCollapse',
            isExpand ? TrackingEventId.SummarySectionExpand : TrackingEventId.SummarySectionCollapse,
            summaryCommonProperties,
            { IsCollapsed: nextIsCollapsed }
        );
        setIsCollapsed(nextIsCollapsed);
    };

    return (
        <Stack
            tokens={{ padding: '12px 16px' }}
            styles={{
                root: {
                    backgroundColor: '#f9f9f9',
                    border: '1px solid #e1e1e1',
                    borderRadius: 4,
                    margin: '8px 0 10px',
                },
            }}
        >
            <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
                <Stack horizontal verticalAlign="center" tokens={{ childrenGap: 6 }}>
                    <img src="/icons/summarize-wht.svg" alt="Summarize" className={summarizeIconClass} />
                    <Text variant="smallPlus" styles={{ root: { fontWeight: 600, color: '#0072c9' } }}>
                        AI-Generated Summary
                    </Text>
                </Stack>
                <IconButton
                    iconProps={{ iconName: isCollapsed ? 'ChevronDown' : 'ChevronUp' }}
                    title={isCollapsed ? 'Expand summary' : 'Collapse summary'}
                    ariaLabel={isCollapsed ? 'Expand summary' : 'Collapse summary'}
                    onClick={handleToggle}
                    styles={{ root: { color: '#0078d4', height: 24, width: 24 } }}
                />
            </Stack>

            {!isCollapsed && (
                <>
                    <MarkdownRenderer content={summary} />
                    <Text variant="tiny" styles={{ root: { color: '#605e5c', fontStyle: 'italic', marginTop: 4 } }}>
                        AI-generated content may not be accurate. Please verify information before making decisions.
                    </Text>
                </>
            )}
        </Stack>
    );
};
