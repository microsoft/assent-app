import * as React from 'react';
import * as Styled from '../SharedLayout';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Stack } from '@fluentui/react';

function EmptyResults(props: { message?: string }): React.ReactElement {
    const displayMessage = props.message? props.message : "No Results Found";
    const [announcement, setAnnouncement] = React.useState<string>('');

    React.useEffect(() => {
        const timerId = setTimeout(() => {
            setAnnouncement(displayMessage);
        }, 100);
        return () => clearTimeout(timerId);
    }, [displayMessage]);

    return (
        <div style={{height: '100%', width: '100%', display: 'flex', justifyContent: 'center', alignItems: 'center'}}>
            <Stack horizontalAlign='center'>
                <Stack.Item grow>
                    <Styled.EmptySearchIcon title="Empty Search Icon" aria-label="Empty Search Icon" role="text" />
                </Stack.Item>
                <Stack.Item>
                    <Styled.NoResults
                        role="status"
                        aria-label={displayMessage}
                        title={displayMessage}
                    >
                        <Styled.MessageBarTitle> {displayMessage} </Styled.MessageBarTitle>
                    </Styled.NoResults>
                </Stack.Item>
            </Stack>
            <div
                aria-live="assertive"
                aria-atomic="true"
                style={{
                    position: 'absolute',
                    width: '1px',
                    height: '1px',
                    overflow: 'hidden',
                    clip: 'rect(0, 0, 0, 0)',
                }}
            >
                {announcement}
            </div>
        </div>
    );
}

const connected = withContext(EmptyResults);
export { connected as EmptyResults };