import * as React from 'react';
import * as Styled from './DetailsMessageBarsStyling';
import { Stack } from '@fluentui/react/lib/Stack';
import { MessageBar, MessageBarType, Link } from '@fluentui/react';

interface InfoViewProps {
    infoTitle: string;
    infoMessage: string;
    linkHref?: string;
    linkText?: string;
}

const InfoView = ({
    infoTitle = '',
    infoMessage = '',
    linkHref = null,
    linkText = null
}: InfoViewProps): JSX.Element => {
    return (
        <MessageBar messageBarType={MessageBarType.info} isMultiline={true} aria-label={'Informational message'}>
            <Stack tokens={Styled.OtherViewsStackTokensGap}>
                <Stack.Item>
                    <Styled.DetailsMessageBarTitle>{infoTitle}</Styled.DetailsMessageBarTitle>
                </Stack.Item>
                <Stack.Item> {infoMessage} </Stack.Item>
                <Stack.Item>{linkHref && <Link href={linkHref}>{linkText}</Link>}</Stack.Item>
            </Stack>
        </MessageBar>
    );
};

export default InfoView;