import * as React from 'react';
import { TextField, ITextFieldStyles } from '@fluentui/react/lib/TextField';


const textFiledStyles: Partial<ITextFieldStyles> = {
    root: { width: 160 },
};

const TextFieldCustom = (props: any): any => {
    return (
        <TextField
            onChange={props.onChange}
            errorMessage={props.errorMessage}
            ariaLabel={props.ariaLabel}
            value={props.value}
            iconProps={props.iconProps}
            styles={textFiledStyles}
        />
    )
}

export default TextFieldCustom