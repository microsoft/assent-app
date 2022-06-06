import * as React from 'react';
import { DefaultButton } from '@fluentui/react';
import { IContextualMenuProps } from '@fluentui/react/lib/ContextualMenu';

const DropdownButton = ({
    text = '',
    title = '',
    menuItems = [],
    primary = false,
    disabled = false,
    className = ''
}: {
    text: string;
    title: string;
    menuItems?: any;
    primary?: boolean;
    disabled?: boolean;
    className: string;
}): any => {
    const menuProps: IContextualMenuProps = {
        items: menuItems
    };
    return (
        <DefaultButton
            className={className}
            text={text}
            title={title}
            menuProps={menuProps}
            primary={primary}
            disabled={disabled}
        />
    );
};

export default DropdownButton;
