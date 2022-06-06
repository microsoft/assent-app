import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { IconButton, ITextFieldStyles, TextField } from '@fluentui/react';
import { Text } from '@fluentui/react/lib/Text';

interface IPaginationProps {
    pageCount: number;
    selectedPage: number;
    onPageChange: (pageNumber: number) => void;
}

export function Pagination(props: IPaginationProps): React.ReactElement {
    const { pageCount, selectedPage, onPageChange } = props;
    const ERROR_MESSAGE = 'Please enter a number between 1 and ' + pageCount;
    const pageInputWidth = pageCount?.toString()?.length * 12 + 15;
    const narrowTextFieldStyles: Partial<ITextFieldStyles> = { fieldGroup: { width: pageInputWidth } };
    const [inputNumber, setInputNumber] = React.useState(selectedPage);

    const onChangeSecondTextFieldValue = (
        event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>,
        newValue?: string
    ) => {
        if (!newValue) {
            setInputNumber(-1);
        } else {
            const stringToInt = parseInt(newValue);
            if (!isNaN(stringToInt)) {
                setInputNumber(stringToInt);
                if (stringToInt > 0 && stringToInt <= pageCount) {
                    onPageChange(stringToInt);
                }
            }
        }
    };
    const getErrorMessage = (): string => {
        if (inputNumber > 0 && inputNumber <= pageCount) {
            return '';
        } else {
            return ERROR_MESSAGE;
        }
    };

    const errorValue = getErrorMessage();

    return (
        <Stack>
            <Stack.Item>
                <Stack horizontal verticalAlign="center" tokens={{ childrenGap: 10 }}>
                    <Stack.Item>
                        <IconButton
                            iconProps={{ iconName: 'Back' }}
                            styles={null}
                            title="Previous page"
                            disabled={selectedPage <= 1}
                            onClick={() => {
                                onPageChange(selectedPage - 1);
                            }}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <Text>Page </Text>
                    </Stack.Item>
                    <Stack.Item>
                        <TextField
                            value={inputNumber > 0 ? inputNumber.toString() : ''}
                            onChange={onChangeSecondTextFieldValue}
                            styles={narrowTextFieldStyles}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <Text>{'of ' + pageCount} </Text>
                    </Stack.Item>
                    <Stack.Item>
                        <IconButton
                            iconProps={{ iconName: 'Forward' }}
                            styles={null}
                            title="Next page"
                            disabled={selectedPage >= pageCount}
                            onClick={() => {
                                onPageChange(selectedPage + 1);
                            }}
                        />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            {errorValue && (
                <Stack.Item>
                    <p role="alert" className="custom-input-error">
                        {errorValue}
                    </p>
                </Stack.Item>
            )}
        </Stack>
    );
}
