import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { TextField } from '@fluentui/react/lib/TextField';

const MultilineTextField = ({
    label = '',
    name = '',
    ariaLabel = '',
    value = null,
    onChange = (): any => {},
    componentRef = null,
    rows = 1,
    required = false,
    inputErrorMessage = '',
    styles = {},
    ariaLabelledby = '',
    ariaRequired = false,
    inputStyle: {}
}: {
    label: string;
    name?: string;
    ariaLabel?: string;
    value: any;
    onChange: any;
    componentRef?: any;
    rows: number;
    required: boolean;
    inputErrorMessage: string;
    styles: object;
    ariaLabelledby?: string;
    ariaRequired?: boolean;
    inputStyle?: any
}): any => {
    return (
        <Stack.Item>
            <TextField
                label={label}
                ariaLabel={ariaLabel}
                name={name}
                value={value}
                onChange={onChange}
                multiline
                rows={rows}
                required={required}
                errorMessage={inputErrorMessage}
                componentRef={componentRef}
                styles={styles}
                aria-labelledby={ariaLabelledby}
                aria-required={ariaRequired}
            />
        </Stack.Item>
    );
};

export default MultilineTextField;
