import * as React from 'react';
import * as Styled from './DetailsMessageBarsStyling';
import { Stack } from '@fluentui/react/lib/Stack';
import { MessageBar, MessageBarType, Link } from '@fluentui/react';
import * as sanitizeHtml from 'sanitize-html';
import { removeHTMLFromString } from '../../../../Helpers/sharedHelpers';

interface ErrorViewProps {
    errorMessage?: string;
    errorMessages?: string[];
    failureType?: string;
    customTitle?: string;
    linkHref?: string;
    linkText?: string;
    dismissHandler?: any;
}

const ErrorView = ({
    errorMessage = '',
    failureType = '',
    customTitle = '',
    linkHref = null,
    linkText = null,
    dismissHandler = null,
    errorMessages = null,
}: ErrorViewProps): JSX.Element => {
    const [errorViewRef, setErrorViewRef] = React.useState(null);
    const [dismissed, setDismissed] = React.useState(false);

    React.useEffect(() => {
        if (errorViewRef) {
            errorViewRef.focus();
        }
    }, [errorViewRef]);

    const cleanErrorMessage = errorMessage
        ? sanitizeHtml(errorMessage, {
              allowedTags: ['a', 'strong'],
              allowedAttributes: {
                  a: ['href', 'target'],
              },
          })
        : '';
    const messageElements = errorMessages?.map((item: string, index: number) => (
        <Stack.Item styles={errorMessages.length > 1 && Styled.WarningViewStackStylesBottomBorder} key={index}>
            <li>
                <div dangerouslySetInnerHTML={{ __html: item }} />
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
                {errorMessages && (
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
                    </Stack.Item>
                )}
                <Stack.Item>{linkHref && <Link href={linkHref}>{linkText}</Link>}</Stack.Item>
            </Stack>
        </MessageBar>
    ) : null;
};

export default ErrorView;
