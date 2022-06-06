import * as React from 'react';
import { DefaultButton, PrimaryButton } from '@fluentui/react';

const BasicButton = ({
    text = '',
    title = '',
    onClick = () => {},
    primary = false,
    componentRef = null,
    disabled = false,
    className = '',
    styles = {},
    aiEventName = ''
}: {
    text: string;
    title: string;
    onClick: any;
    primary?: boolean;
    componentRef?: any;
    disabled?: boolean;
    className?: string;
    styles?: object;
    aiEventName?: any
}): any => {
    return primary ? (
        <PrimaryButton
            text={text}
            title={title}
            onClick={onClick}
            componentRef={componentRef}
            disabled={disabled}
            className={className}
            styles={styles}
        />
    ) : (
        <DefaultButton
            text={text}
            title={title}
            onClick={onClick}
            componentRef={componentRef}
            disabled={disabled}
            className={className}
            styles={styles}
        />
    );
};

export default BasicButton;
