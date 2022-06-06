import * as React from 'react';
import { Stack, IStackTokens } from '@fluentui/react';
import { Dropdown } from '@fluentui/react/lib/Dropdown';
import { dropdownStyles } from './styles/DropdownControlStyle';

const dropdownTokens: IStackTokens = { childrenGap: 20 };

const DropdownControl = (props: any): any => {
    const privateOnChange = (e: any, option: any): any => {
        if (option.key === 'true') {
            props.dropDownChanged(true);
        } else if (option.key === 'false') {
            props.dropDownChanged(false);
        }
    };
    return (
        <Stack tokens={dropdownTokens}>
            <Dropdown
                ariaLabel={props.ariaLabel}
                options={props.options}
                defaultSelectedKey={`${props.defaultSelectedValue}`}
                styles={dropdownStyles}
                onChange={privateOnChange}
            />
        </Stack>
    );
};

export default DropdownControl;
