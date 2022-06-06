import { ITextFieldStyles } from '@fluentui/react/lib/TextField';
import { Depths } from '@fluentui/theme';
import { IStackTokens, IStackStyles, IStackItemStyles } from '@fluentui/react/lib/Stack';
import { IDropdownStyles } from '@fluentui/react/lib/Dropdown';
import { maxWidth } from '../Styles/Media';

export const SmallSpace = styled.div`
    margin-bottom: 12px;
`;

export const LargeSpace = styled.div`
    margin-bottom: 32px;
`;

export const ExtraLargeSpace = styled.div`
    margin-bottom: 64px;
`;

export const DetailsFilePreviewStackTokens: IStackTokens = { childrenGap: 3, padding: '0 10px' };

export const DetailsDocPreviewHeaderBarStyles = (viewType: string, selectedPage?: string): IStackStyles => ({
    root: {
        display: 'flex',
        justifyContent: 'space-between',
        position: 'absolute',
        width: 'calc(100% - 16px)',
        background: '#ffffff',
        top: '0px',
        paddingBottom: '5px',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 640px) and (max-width: 2048px)': {
                marginTop: `${viewType == 'FLY' || selectedPage === 'history' ? '0px' : '-48px'}`,
            },
        },
    },
});

export const StickyDetailsHeder: IStackStyles = {
    root: {
        position: 'absolute',
        width: '100%',
        background: 'white',
        height: '35px',
        top: '0px',
    },
};

export const DetailsSpacing: IStackItemStyles = {
    root: {
        paddingLeft: '5%',
    },
};

export const Footer: any = styled.footer`
    position: fixed;
    bottom: 0;
    padding-left: 12px;
    bottom: 0;
    width: 100%;
    background: white;
    overflow-y: scroll;
    overflow-x: hidden;
    box-shadow: ${Depths.depth64};
    border-left: 1px solid rgba(0, 0, 0, 0.1);
    @media only screen and (max-width: 780px) {
        max-height: 25% !important;
        width: calc(100% - 8px);
    }
    @media only screen and (max-height: 900px) and (max-width: 680px) {
        height: 8.5%;
    }
    @media only screen and (max-height: 700px) and (max-width: 680px) {
        height: 10%;
    }
`;

export const primaryStyle = styled.div<any>`
    position: fixed;
    bottom: 0;
    padding: 6px 12px;
    width: ${(props) =>
        (props as any).isPanel
            ? (props as any).windowWidth < 1024
                ? '49.7%'
                : props.isMaximized
                ? '32.0%'
                : '47.7%'
            : '100%'};
    background: ${(props) => ((props as any).isPanel ? '#d0e7f8' : 'white')};
    overflow-y: scroll;
    overflow-x: hidden;
    box-shadow: ${Depths.depth64};
    margin: auto;
    border-left: 1px solid rgba(0, 0, 0, 0.1);
    max-height: 55% !important;

    @media only screen and (min-device-width: 1023px) and (min-width: 920px) and (max-width: 2048px) {
        width: ${(props) =>
            (props as any).isPanel
                ? (props as any).windowWidth < 1024
                    ? '49.7%'
                    : props.isMaximized
                    ? '32.0%'
                    : '47.7%'
                : 'calc(100% - 48px)'};
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 640px) and (max-width: 768px) {
        max-height: 25% !important;
        overflow: scroll;
        overflow-x: hidden;
        bottom: 0;
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px) {
        max-height: 25% !important;
        overflow: scroll;
        overflow-x: hidden;
        bottom: 0;
    }

    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        max-height: 25% !important;
        overflow: scroll;
        overflow-x: hidden;
        bottom: 0;
    }

    @media only screen and (min-device-width: 1023px) and (max-width: 653px) {
        display: ${(props) => (props.isPanel ? 'none' : 'fixed')};
    }
`;

export const BulkSmallSpace = styled.div`
    margin-left: 12px;
`;

export const BottomSpacep = styled.div`
    margin-bottom: 12px;
`;

export const BulkSmallTextFieldStyles = (isBulk: boolean): Partial<ITextFieldStyles> => ({
    root: {
        maxWidth: `${isBulk ? '100%' : '60%'}`,
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 571px) and (max-width: 639px)': {
                fontSize: '8px',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 571px)': {
                fontSize: '4px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                fontSize: '14px',
            },
        },
    },
    description: {
        fontSize: '2px',
        '@media (min-device-width: 1023px) and (max-width: 320px)': {
            fontSize: '14px',
        },
    },
});
export const SmallTextFieldStyles: Partial<ITextFieldStyles> = { root: { maxWidth: 500 } };

export const HeaderActionBarMessageStyle = { root: { marginTop: '5px', marginRight: '5px' } };

export const DetailStackStyles: IStackStyles = {
    root: {
        backgroundColor: 'white',
        marginTop: '0!important',
    },
};

export const bulkMessageStyle: IStackStyles = {
    root: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 571px) and (max-width: 639px)': {
                'font-size': '8px',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 571px)': {
                'font-size': '4px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                'font-size': '14px',
            },
        },
    },
};

export const additionalNotesStyle = (isBulk: boolean): IStackStyles => ({
    root: {
        maxWidth: `${isBulk ? '80%' : '40%'}`,
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 571px) and (max-width: 639px)': {
                width: '30%',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 571px)': {
                width: '30%',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                width: '80%',
            },
            [maxWidth.m]: {
                maxWidth: '100%',
            },
        },
    },
});

export const actionWarningStyle = (isBulk: boolean, isMaximized: boolean): IStackStyles => ({
    root: {
        maxWidth: `${isBulk || isMaximized ? '80%' : '60%'}`,
        selectors: {
            [maxWidth.m]: {
                maxWidth: '100%',
            },
        },
    },
});

export const SmallDropdownStyles: Partial<IDropdownStyles> = {
    dropdown: { maxWidth: 500, minWidth: 150 },
    label: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 571px) and (max-width: 639px)': {
                fontSize: '8px',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 571px)': {
                fontSize: '4px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                fontSize: '14px',
            },
        },
    },
};

export const submitAndCancelFormStyle: IStackStyles = {
    root: {
        width: '100%',
    },
};
export const buttonZoomStyle: IStackStyles = {
    root: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                'font-size': '4px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                'margin-top': '8px !important;',
                'max-height': '8px',
                bottom: '-3px',
            },
        },
    },
};

export const StackAdditionDetails = (footerHeight: number): IStackStyles => ({
    root: {
        marginTop: '0!important',
        marginBottom: `${footerHeight + 180}px`,
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 2048px)': {
                marginBottom: `${footerHeight + 10}px`,
            },
        },
    },
});

export const DetailCardBackgroundColor = {
    root: {
        backgroundColor: 'white',
    },
};

export const getModalDimensions = (
    isExpanded: boolean,
    windowWidth: number,
    windowHeight: number
): { width: number; height: number } => {
    if (isExpanded) {
        return {
            width: windowWidth * 0.9,
            height: windowHeight * 0.9,
        };
    } else {
        return {
            width: windowWidth * 0.4,
            height: windowHeight * 0.8,
        };
    }
};
