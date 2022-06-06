import * as React from 'react';
import * as Styled from './DetailsMessageBarsStyling';
import { Stack } from '@fluentui/react/lib/Stack';
import { MessageBar, MessageBarType } from '@fluentui/react';
import { CollapsibleSection } from '../../Components/CollapsibleSection';

interface WarningViewProps {
    warningTitle?: string;
    warningMessages: string[] | null;
    onDismiss?: VoidFunction | null;
    isCollapsible?: boolean;
    messageBarStyles?: any;
}

const WarningView = ({
    warningTitle = '',
    warningMessages = null,
    onDismiss = null,
    isCollapsible = false,
    messageBarStyles = null
}: WarningViewProps): JSX.Element => {
    const messageElements = warningMessages?.map((item: string, index: number) => (
        <Stack.Item styles={warningMessages.length > 1 && Styled.WarningViewStackStylesBottomBorder} key={index}>
            <p>{item}</p>
        </Stack.Item>
    ));

    return isCollapsible ? (
        <CollapsibleSection
            defaultIsExpanded={false}
            titleText={warningTitle ? warningTitle : ''}
            renderHeaderAs="div"
            style={Styled.CollapsibleStyle}
        >
            <Stack tokens={Styled.WarningViewStackTokensLargeGap} style={Styled.CollapsibleStyle}>
                {messageElements}
            </Stack>
        </CollapsibleSection>
    ) : (
        <MessageBar
            messageBarType={MessageBarType.warning}
            isMultiline={true}
            aria-label={'Warning message'}
            onDismiss={onDismiss}
            styles={messageBarStyles}
        >
            <Stack tokens={Styled.WarningViewStackTokensLargeGap}>
                <>
                    {warningTitle && (
                        <Stack.Item>
                            <Styled.DetailsMessageBarTitle>{warningTitle}</Styled.DetailsMessageBarTitle>
                        </Stack.Item>
                    )}
                    {messageElements}
                </>
            </Stack>
        </MessageBar>
    );
};

export default WarningView;
