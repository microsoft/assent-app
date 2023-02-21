import { IIconProps, IStackTokens, IStyle, mergeStyleSets } from '@fluentui/react';

export const controlStyles = mergeStyleSets({
    mainStack: {
        padding: '0 20px 20px 20px',
    } as IStyle,
    ul: {
        li: {
            listStyleType: 'disc',
        } as IStyle,
    } as IStyle,
    fileIcon: {
        marginRight: '6px',
        verticalAlign: 'middle',
    } as IStyle,
    fileListTextContainer: {
        textAlign: 'end',
    } as IStyle,
    fileListText: {
        verticalAlign: 'middle',
    } as IStyle,
    fileListTextAlreadyExists: {
        margin: '0 0 0 24px',
        verticalAlign: 'middle',
        fontStyle: 'italic',
    } as IStyle,
    fileUploadDetailsListStyle: {
        '.ms-DetailsRow-cell': {
            lineHeight: '28px',
        } as IStyle,
    } as IStyle,
    cannotDeleteNote: {
        lineHeight: '32px',
    } as IStyle,
});

export const deleteIcon: IIconProps = {
    iconName: 'Delete',
    styles: {
        root: {
            ':hover': {
                color: 'darkblue',
            },
        },
    },
};

export const mainStack: IStackTokens = {
    childrenGap: 15,
};

export const buttonStack: IStackTokens = {
    childrenGap: 15,
};
