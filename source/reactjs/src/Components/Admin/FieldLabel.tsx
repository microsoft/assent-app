import * as React from 'react';
import { Stack, TooltipHost, Icon } from '@fluentui/react';

export function FieldLabel(props: { text: string; tooltip: string }): React.ReactElement {
    return (
        <Stack horizontal verticalAlign="center" tokens={{ childrenGap: 4 }}>
            <span>{props.text}</span>
            <TooltipHost content={props.tooltip}>
                <Icon
                    iconName="Info"
                    styles={{
                        root: {
                            fontSize: 14,
                            color: '#605e5c',
                            cursor: 'help',
                        },
                    }}
                />
            </TooltipHost>
        </Stack>
    );
}
