import { IStackStyles, IStackItemStyles } from '@fluentui/react/lib/Stack';
import { IMessageBarStyles } from '@fluentui/react/lib/MessageBar';
import { IDropdownStyles } from '@fluentui/react/lib/Dropdown';
import { maxWidth } from '../../../Shared/Styles/Media';
export const SecondaryHeaderContainer = styled.div`
    height: 48px;
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
        height: '24px',
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
        height: '40px',
        marginLeft: '20px',
        background: '#f2f2f2',
        width: 'calc(100vw - 48px)',
        paddingTop: '10px',
        zIndex: 50,
    },
};

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
        paddingTop: '0.25%',
        paddingBottom: '0.25%',
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
                minHeight: '15px',
                height: '15px',
            },
            '@media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 458px)': {
                paddingLeft: '18px !important;',
                paddingTop: '0px',
                minHeight: '20px',
                height: '20px',
            },
            '@media only screen and (min-device-width: 1023px) and (max-width: 320px)': {
                paddingLeft: '13px !important;',
                paddingTop: '0px',
                minHeight: '16px',
                height: '16px',
            },
        },
    },
};

export const BulkMessageHeight = styled.div``;

export const MessageBarText = styled.div`
    font-size: '4px';
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px) {
        line-height: 7px !important;
        font-size: 2px;
        margin-top: -4px;
    }
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        line-height: 5px !important;
        font-size: 1px;
        margin-top: -4px;
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
        marginTop: '1px',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                marginTop: '0px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                marginTop: '0px',
            },
        },
    },
    dropdownOptionText: { overflow: 'visible', whiteSpace: 'normal' },
    dropdownItem: { height: 'auto' },
    title: { overflow: 'visible', whiteSpace: 'nowrap', height: 'auto' },
    label: {
        paddingRight: '5px',
        marginTop: '1px',
        color: 'rgb(23, 23, 23)',
        selectors: {
            '@media (min-width: 640px) and (max-width: 2048px)': {
                fontWeight: 400,
                color: 'rgb(23, 23, 23)',
            },
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                fontSize: '16px',
                fontWeight: 400,
                marginLeft: '-1px',
                marginTop: '0px',
                color: 'rgb(23, 23, 23)',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                fontSize: '16px',
                fontWeight: 400,
                marginTop: '0px',
                color: 'rgb(23, 23, 23)',
            },
        },
    },
    dropdown: {
        width: isMaximized ? '175px' : 'largestOptionWidth',
        selectors: {
            [maxWidth.xxl]: {
                width: isMaximized ? '100px' : 'largestOptionWidth',
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
