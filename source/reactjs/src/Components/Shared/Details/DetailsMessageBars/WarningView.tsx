import * as React from 'react';
import * as Styled from './DetailsMessageBarsStyling';
import { Stack } from '@fluentui/react/lib/Stack';
import * as MarkdownIt from 'markdown-it';
import { MessageBar, MessageBarType } from '@fluentui/react';
import { CollapsibleSection } from '../../Components/CollapsibleSection';
import * as sanitizeHtml from 'sanitize-html';

// Applied AFTER markdown.render so markdown-generated tags are constrained to a minimal, safe allowlist.
const BANNER_SANITIZE_CONFIG: sanitizeHtml.IOptions = {
    allowedTags: ['a', 'strong', 'br', 'p', 'em'],
    allowedAttributes: {
        a: ['href', 'target', 'rel'],
    },
    allowedSchemes: ['http', 'https', 'mailto'],
    transformTags: {
        a: sanitizeHtml.simpleTransform('a', { rel: 'noopener noreferrer' }, true),
    },
};

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
    const markdown = MarkdownIt().set({ html: true });
    // Render markdown FIRST, then sanitize the generated HTML against the banner allowlist.
    const messageElements = warningMessages?.map((item: string, index: number) => (
        <Stack.Item styles={warningMessages.length > 1 && Styled.WarningViewStackStylesBottomBorder} key={index}>
            <p dangerouslySetInnerHTML={{ __html: sanitizeHtml(markdown.render(item ?? ''), BANNER_SANITIZE_CONFIG) }} />
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
