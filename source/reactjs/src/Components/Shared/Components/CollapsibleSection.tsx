import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { Icon, IconButton, IStyle, Separator, Text, TooltipHost } from '@fluentui/react';

export function CollapsibleSection(props: {
    titleText: string;
    children: any;
    tooltipText?: string;
    style?: IStyle;
}): React.ReactElement {
    const { titleText, children, tooltipText, style } = props;
    const [isExpanded, setIsExpanded] = React.useState(false);

    const toggleSection = () => {
        setIsExpanded(!isExpanded);
    };
    return (
        <Stack styles={{ root: { marginBottom: '20px' } }}>
            <Stack.Item>
                <Stack horizontal styles={{ root: style }}>
                    <Stack.Item>
                        <IconButton
                            iconProps={{ iconName: isExpanded ? 'ChevronUp' : 'ChevronDown' }}
                            title={isExpanded ? 'Collapse section' : 'Expand section'}
                            onClick={toggleSection}
                        />
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingTop: '5px', paddingLeft: '5px' } }}>
                        <Text styles={{ root: { fontWeight: 600 } }}> {titleText} </Text>
                        {tooltipText && (
                            <TooltipHost content={tooltipText} aria-label="{''}">
                                <Icon
                                    iconName="Info"
                                    tabIndex={0}
                                    aria-label={tooltipText}
                                    styles={{ root: { marginLeft: '5px' } }}
                                />
                            </TooltipHost>
                        )}
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            {isExpanded && <Stack.Item>{children}</Stack.Item>}
        </Stack>
    );
}
