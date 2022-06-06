import * as React from 'react';
import * as Styled from './DetailsMessageBarsStyling';
import { Stack } from '@fluentui/react/lib/Stack';

const SuccessView = (): JSX.Element => {
    return (
        <Stack verticalAlign="center" horizontalAlign="center">
            <Stack.Item>
                <Styled.SuccessIcon iconName="SkypeCircleCheck" />
            </Stack.Item>
            <Stack.Item>
                <div
                    ref={input => input && input.focus()}
                    role="status"
                    aria-label="Your action has successfully completed"
                    title="Your action has successfully completed"
                    tabIndex={0}
                    style={{ outline: 'none' }}
                >
                    <Styled.SuccessMessage>Your action has successfully completed!</Styled.SuccessMessage>
                </div>
            </Stack.Item>
        </Stack>
    );
};

export default SuccessView;
