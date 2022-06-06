import * as React from 'react';
import * as Styled from '../SharedLayout';
import { Stack } from '@fluentui/react';

function ErrorResult(props: { message?: string }): React.ReactElement {
    return (
        <div style={{ height: '100%', width: '100%', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
            <Stack horizontalAlign="center">
                <Stack.Item grow>
                    <Styled.ErrorResultIcon title="Error result icon" aria-label="Error result icon" />
                </Stack.Item>
                <Stack.Item>
                    <Styled.MessageBarTitle>
                        {props.message ? props.message : 'An error occured'}
                    </Styled.MessageBarTitle>
                </Stack.Item>
            </Stack>
        </div>
    );
}

export default ErrorResult;
