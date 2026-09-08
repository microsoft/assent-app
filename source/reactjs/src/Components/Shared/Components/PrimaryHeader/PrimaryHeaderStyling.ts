import { IStackStyles, IStackItemStyles } from '@fluentui/react/lib/Stack';
import { IMessageBarStyles } from '@fluentui/react/lib/MessageBar';
import { IDropdownStyles } from '@fluentui/react/lib/Dropdown';
import { maxWidth } from '../../../Shared/Styles/Media';
export const SecondaryHeaderContainer = styled.div`
    min-height: 48px;
    height: auto;
    margin-left: 1% !important;
    .ms-Button-menuIcon {
        color: rgb(0, 13, 23) !important;
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 2048px) and (max-width: 2600px) {
        margin-left: calc(3vw - 10px) !important;
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 1024px) and (max-width: 2048px) {
        margin-left: calc(3vw - 23px) !important;
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 763px) and (max-width: 1023px) {
        margin-left: calc(3vw + 15px) !important;
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 654px) and (max-width: 763px) {
        margin-left: calc(3vw + 15px) !important;
    }

    @media only screen and (min-device-width: 1023px) and (min-width: 640px) and (max-width: 654px) {
        margin-left: calc(5vw + 12px) !important;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 572px) and (max-width: 640px) {
        margin-left: calc(3vw + 2px) !important;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 640px) and (max-width: 572px) {
        margin-left: calc(3vw + 2px) !important;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 458px) and (max-width: 572px) {
        height: 24px !important;
        margin-top: 0px;
        margin-left: calc(6vw - 2px) !important;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 382px) and (max-width: 458px) {
        height: 24px !important;
        margin-top: 0px;
        margin-left: calc(5vw + 6px) !important;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 382px) {
        margin-top: 0px;
        margin-left: calc(5vw + 7px) !important;
    }
    @media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px): {
        height: 24px !important;
        margin-top: 0px;
        fontSize: 24px !important'
        margin-left: calc(5vw + 8px) !important;
    },
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        height: 18px !important;
        margin-top: 0px;
        margin-left: calc(3vw + 15px) !important;
    }
`;

export const GroupAndFilterIconStackItemStyles: IStackItemStyles = {
    root: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 640px) and (max-width: 1023px)': {
                height: 24 + 'px',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                fontSize: 24 + 'px!important',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                fontSize: 24 + 'px',
            },
        },
    },
};

export const SecondaryHeaderStackStyles: IStackStyles = {
    root: {
        minHeight: '24px',
        height: 'auto',
        background: '#f2f2f2',
        width: 'calc(100vw - 48px)',
        paddingTop: '10px',
        zIndex: 50,
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 654px) and (max-width: 1023px)': {
                height: 24 + 'px',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 640px) and (max-width: 654px)': {
                marginLeft: '-10px !important;',
                height: 24 + 'px',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                height: 24 + 'px',
                fontSize: 24 + 'px!important',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                height: 24 + 'px',
            },
        },
    },
};

export const SecondaryHeaderMobileStackStyles: IStackStyles = {
    root: {
        minHeight: '40px',
        height: 'auto',
        marginLeft: '20px',
        background: '#f2f2f2',
        width: 'calc(100vw - 48px)',
        paddingTop: '10px',
        zIndex: 50,
    },
};

export const SecondaryHeaderMobileContainer = styled.div`
    height: auto;
    min-height: 40px;
    margin-left: 1% !important;
    .ms-Button-menuIcon {
        color: rgb(0, 13, 23) !important;
    }
`;

export const SecondaryHeaderIconStyling = {
    zIndex: 10,
    '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
        height: 24 + 'px',
    },
    '@media (min-device-width: 1023px) and (max-width: 320px)': {
        height: 14 + 'px',
    },
};

export const DelegationBarStyles: IMessageBarStyles = {
    root: {
        paddingLeft: '1.5%',
        paddingTop: '8px',
        paddingBottom: '8px',
        backgroundColor: '#D0E7F8', // Coherence DefaultThemeColors.blue20
        selectors: {
            '@media only screen and (min-device-width: 1023px) and  (min-width: 1024px) and (max-width: 2048px)': {
                marginLeft: '-16px !important;',
                width: 'calc(100vw + 16px)',
            },

            '@media only screen and (min-device-width: 1023px) and  (min-width: 763px) and (max-width: 1023px)': {
                paddingLeft: '33px !important;',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 654px) and (max-width: 763px)': {
                paddingLeft: '30px !important;',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 640px) and (max-width: 654px)': {
                paddingLeft: '25px !important;',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 458px) and (max-width: 640px)': {
                paddingLeft: '25px !important;',
            },
            '@media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 458px)': {
                paddingLeft: '18px !important;',
                paddingTop: '8px',
                paddingBottom: '8px',
            },
            '@media only screen and (min-device-width: 1023px) and (max-width: 320px)': {
                paddingLeft: '13px !important;',
                paddingTop: '8px',
                paddingBottom: '8px',
            },
        },
    },
};

export const BulkMessageHeight = styled.div``;

export const MessageBarText = styled.div`
    font-size: 14px;
    line-height: 1.5;
    word-wrap: break-word;
    overflow-wrap: break-word;
    white-space: normal;

    @media (max-width: 640px) {
        font-size: 13px;
        line-height: 1.6;
    }
`;

export const DropDownStyle = (
    largestOptionWidth: string,
    isMaximized: boolean,
    isPanelOpen: boolean
): Partial<IDropdownStyles> => ({
    root: {
        display: 'flex',
        marginRight: '20px',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                marginTop: '0px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                marginTop: '0px',
            },
        },
    },
    dropdownOptionText: {
        overflow: 'visible', whiteSpace: 'normal',
        selectors: {
            '@media (max-width: 1280px)': {
                fontSize: isMaximized ? "12px" : "16px"
            },
            '@media (max-width: 1152px)': {
                fontSize: isMaximized ? "9px" : "16px"
            },
        }
    },
    dropdownItem: {
        height: 'auto',
    },
    title: {
        overflow: 'hidden', whiteSpace: 'nowrap', height: 'auto',
    },
    label: {
        paddingRight: '5px',
        marginTop: '1px',
        color: 'rgb(50, 49, 48)',
        selectors: {
            '@media (min-width: 640px) and (max-width: 2048px)': {
                fontWeight: 400,
                color: 'rgb(50, 49, 48)',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                fontSize: '16px',
                fontWeight: 400,
                marginLeft: '-1px',
                marginTop: '0px',
                color: 'rgb(50, 49, 48)',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                fontSize: '16px',
                fontWeight: 400,
                marginTop: '0px',
                color: 'rgb(50, 49, 48)',
            },
        },
    },
    dropdown: {
        width: '200px',
        selectors: {
            '@media (max-width: 1440px)': {
                width: isMaximized ? '160px' : '215px',
                fontSize: '16px',
            },
            '@media (max-width: 1400px)': {
                width: isMaximized ? '152px' : '215px',
                fontSize: '16px',
            },
            '@media (max-width: 1366px)': {
                width: isMaximized ? '145px' : '215px',
                fontSize: '16px',
            },
            '@media (max-width: 1280px)': {
                width: isMaximized ? '118px' : '215px',
                fontSize: '16px',
            },
            '@media (min-device-width: 1023px) and (max-width: 1152px)': {
                width: isMaximized ? '90px' : '215px',
                fontSize: '16px',
            },
            [maxWidth.m]: {
                width: isMaximized ? '100px' : isPanelOpen ? '150px' : '175px',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                width: parseInt(largestOptionWidth, 10) / 1.5 + 'px',
                fontSize: '16px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                width: Math.min(parseInt(largestOptionWidth) / 1.5, 120) + 'px',
                fontSize: '16px',
            },
        },
    },
});

