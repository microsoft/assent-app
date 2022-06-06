import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { IconButton, IStyle, Separator, Text } from '@fluentui/react';

export function CollapsibleSection(props: { titleText: string; children: any; style?: IStyle }): React.ReactElement {
    const { titleText, children, style } = props;
    const [isExpanded, setIsExpanded] = React.useState(false);

    const toggleSection = () => {
        setIsExpanded(!isExpanded);
    };
    return (
        <Stack>
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
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            {isExpanded && <Stack.Item>{children}</Stack.Item>}
        </Stack>
    );
}
