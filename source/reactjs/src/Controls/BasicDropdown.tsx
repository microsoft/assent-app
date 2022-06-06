import * as React from 'react';
import { Dropdown } from '@fluentui/react/lib/Dropdown';

const BasicDropdown = ({
    options = [],
    placeholder = '',
    selectedKey = '',
    label = '',
    onChange = (): void => {},
    required = false,
    styles = {},
    componentRef = null,
    errorMessage = null,
    ariaLabel = null,
}: {
    options: any;
    placeholder?: string;
    selectedKey: string;
    label: string;
    onChange: any;
    required?: boolean;
    styles?: object;
    componentRef?: any;
    errorMessage?: any;
    ariaLabel?: string;
}): any => {
    return (
        <Dropdown
            options={options}
            placeholder={placeholder}
            selectedKey={selectedKey}
            label={label}
            onChange={onChange}
            required={required}
            styles={styles}
            componentRef={componentRef}
            errorMessage={errorMessage}
            ariaLabel={ariaLabel}
        />
    );
};
export default BasicDropdown;
