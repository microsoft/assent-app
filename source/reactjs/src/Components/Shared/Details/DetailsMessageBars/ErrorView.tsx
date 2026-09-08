import * as React from 'react';
import * as Styled from './DetailsMessageBarsStyling';
import { Stack } from '@fluentui/react/lib/Stack';
import * as MarkdownIt from 'markdown-it';
import { MessageBar, MessageBarType, Link, DefaultButton, SharedColors } from '@fluentui/react';
import * as sanitizeHtml from 'sanitize-html';
import { removeHTMLFromString } from '../../../../Helpers/sharedHelpers';

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

interface ErrorViewProps {
    errorMessage?: string;
    errorMessages?: string[];
    failureType?: string;
    customTitle?: string;
    linkHref?: string;
    linkText?: string;
    dismissHandler?: any;
    isContentCollapsable?: boolean;
}


const ErrorView = ({
    errorMessage = '',
    failureType = '',
    customTitle = '',
    linkHref = null,
    linkText = null,
    dismissHandler = null,
    errorMessages = null,
    isContentCollapsable = false,
}: ErrorViewProps): JSX.Element => {
    const [errorViewRef, setErrorViewRef] = React.useState(null);
    const [dismissed, setDismissed] = React.useState(false);
    const [showMore, setShowMore] = React.useState(true);
    const markdown = MarkdownIt().set({ html: true });

    // Render markdown FIRST, then sanitize the generated HTML against the banner allowlist.
    const renderSafeMarkdown = (text: string): string =>
        sanitizeHtml(markdown.render(text ?? ''), BANNER_SANITIZE_CONFIG);

    React.useEffect(() => {
        if (errorViewRef) {
            errorViewRef.focus();
        }
    }, [errorViewRef]);

    const cleanErrorMessage = errorMessage ? renderSafeMarkdown(errorMessage) : '';

    const messageElements = errorMessages?.map((item: string, index: number) => (
        <Stack.Item styles={errorMessages.length > 1 && Styled.WarningViewStackStylesBottomBorder} key={index}>
            <li>
                <div dangerouslySetInnerHTML={{ __html: renderSafeMarkdown(item) }} />
            </li>
        </Stack.Item>
    ));
    const failureDescription = failureType ? `${failureType} failed.` : customTitle;
    const failureInfo = cleanErrorMessage
        ? failureDescription + ' ' + cleanErrorMessage
        : failureDescription + ' error details available in top error banner';
    return !dismissed ? (
        <MessageBar
            messageBarType={MessageBarType.error}
            isMultiline={true}
            aria-label={'Error message - ' + failureInfo}
            onDismiss={() => {
                if (dismissHandler) {
                    dismissHandler();
                } else {
                    setDismissed(true);
                }
            }}
            dismissButtonAriaLabel={'Dismiss error message'}
        >
            <Stack tokens={Styled.OtherViewsStackTokensGap}>
                {(failureType || customTitle) && (
                    <Stack.Item>
                        <Styled.DetailsMessageBarTitle>{failureDescription}</Styled.DetailsMessageBarTitle>

                    </Stack.Item>
                )}
                {cleanErrorMessage && (
                    <Stack.Item>
                        <div dangerouslySetInnerHTML={{ __html: cleanErrorMessage }} />
                    </Stack.Item>
                )}
                {errorMessages && isContentCollapsable && showMore && (
                    <Stack.Item>
                        <DefaultButton
                            text="Show more"
                            title="Show more"
                            onClick={() => setShowMore(false)}
                            style={{ margin: '10px 0 0 0' }}
                        />
                    </Stack.Item>
                )}
                {errorMessages && (isContentCollapsable ? !showMore : true) && (
                    <Stack.Item>
                        <div
                            ref={(input) => {
                                setErrorViewRef(input);
                            }}
                            role="status"
                            aria-label={
                                (failureType ? `${failureType} failed.` : customTitle) +
                                ' ' +
                                removeHTMLFromString(errorMessages?.join(' ') ?? '')
                            }
                            title={failureType ? `${failureType} failed.` : customTitle}
                            tabIndex={0}
                            style={{ outline: 'none' }}
                        >
                            <Styled.UnorderedList>
                                <Stack>{messageElements}</Stack>
                            </Styled.UnorderedList>
                        </div>
                        {isContentCollapsable && (
                            <DefaultButton
                                text="Show less"
                                title="Show less"
                                onClick={() => setShowMore(true)}
                                style={{ margin: '10px 0 0 0' }}
                            />
                        )}
                    </Stack.Item>
                )}
                <Stack.Item>{linkHref && <Link href={linkHref}>{linkText}</Link>}</Stack.Item>
            </Stack>
        </MessageBar>
    ) : null;
};

export default ErrorView;
