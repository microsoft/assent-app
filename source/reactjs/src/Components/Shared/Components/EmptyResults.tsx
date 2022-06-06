import * as React from 'react';
import * as Styled from '../SharedLayout';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Stack } from '@fluentui/react';

function EmptyResults(props: { message?: string }): React.ReactElement {
    const displayMessage = props.message? props.message : "No Results Found";

    const [emptyResultsRef, setEmptyResultsRef] = React.useState(null);

    React.useEffect(() => {
        if (emptyResultsRef) {
            emptyResultsRef.focus();
        }
    }, [emptyResultsRef]);


    return (
        <div style={{height: '100%', width: '100%', display: 'flex', justifyContent: 'center', alignItems: 'center'}}>
            <Stack horizontalAlign='center'>
                <Stack.Item grow>
                    <Styled.EmptySearchIcon title="Empty Search Icon" aria-label="Empty Search Icon" role="text" />
                </Stack.Item>
                <Stack.Item>
                <div
                    ref={(input: any): void => {
                            setEmptyResultsRef(input);
                        }}
                    role="status"
                    aria-label={displayMessage}
                    title={displayMessage}
                    tabIndex={0}
                    style={{ outline: 'none' }}
                >
                    <Styled.MessageBarTitle> {displayMessage} </Styled.MessageBarTitle>
                    </div>
                </Stack.Item>
            </Stack>
        </div>
    );
}

const connected = withContext(EmptyResults);
export { connected as EmptyResults };