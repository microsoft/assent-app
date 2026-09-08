import * as React from 'react';
import { Stack, IStackStyles } from '@fluentui/react/lib/Stack';
import { Text, ITextStyles } from '@fluentui/react/lib/Text';
import { IconButton } from '@fluentui/react/lib/Button';

interface IDashboardAnalyticsHeaderProps {
    isCollapsed: boolean;
    onToggleCollapse: () => void;
    containerStyles?: IStackStyles;
    textVariant?: 'medium' | 'large';
    textStyles?: ITextStyles;
}

export const DashboardAnalyticsHeader: React.FunctionComponent<IDashboardAnalyticsHeaderProps> = ({
    isCollapsed,
    onToggleCollapse,
    containerStyles,
    textVariant = 'medium',
    textStyles,
}) => {
    const defaultContainerStyles: IStackStyles = {
        root: {
            padding: '12px 16px',
            backgroundColor: '#faf9f8',
            borderRadius: '8px',
            border: '1px solid #edebe9',
            minWidth: '150px',
        },
    };

    const defaultTextStyles: ITextStyles = {
        root: {
            fontWeight: '500',
            color: '#323130',
        },
    };

    return (
        <Stack
            horizontal
            horizontalAlign="space-between"
            verticalAlign="center"
            styles={containerStyles || defaultContainerStyles}
        >
            <Text variant={textVariant} styles={textStyles || defaultTextStyles}>
                Analytics
            </Text>
            <IconButton
                iconProps={{ iconName: isCollapsed ? 'ChromeFullScreen' : 'ChromeMinimize' }}
                title={isCollapsed ? 'Maximize Analytics' : 'Minimize Analytics'}
                ariaLabel={isCollapsed ? 'Maximize Analytics' : 'Minimize Analytics'}
                aria-expanded={!isCollapsed}
                onClick={onToggleCollapse}
                styles={{
                    root: {
                        color: '#605e5c',
                        height: '24px',
                        width: '24px',
                    },
                    rootHovered: {
                        color: '#323130',
                    },
                }}
            />
        </Stack>
    );
};
