import { IStackStyles, IStackItemStyles } from '@fluentui/react/lib/Stack';
import { Text } from '@fluentui/react/lib/Text';
import { TextColors, MessagingColors } from '../../SharedColors';
import { IMessageBarStyles } from '@fluentui/react/lib/MessageBar';
import { IToggleStyles } from '@fluentui/react';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const SecondaryHeaderContainer = styled.div<{ isTopHeader: boolean }>`
    height: 48px;
    padding-left: ${(props: { isTopHeader: boolean }) => (props.isTopHeader ? '0px' : '48px')};

    .ms-Button-menuIcon {
        color: ${(props: { isTopHeader: boolean }) => (props.isTopHeader ? 'white' : 'rgb(0, 13, 23) !important')};
    }

    @media only screen and (max-width: 639px) {
        height: 24px !important;
    }

    @media only screen and (min-device-width: 1023px) and (max-width: 1024px) {
        margin-left: 0px !important;
    }
`;

export const topHeaderTitleLink = styled.a`
    font-family: 'Segoe UI', 'Segoe UI Web (West European)', 'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto,
        'Helvetica Neue', sans-serif;
    -webkit-font-smoothing: antialiased;
    font-size: 16px;
    font-weight: 600;
    outline: none;
    text-decoration: none;
    height: 46px;
    line-height: 46px;
    display: block;
    overflow: hidden;
    max-width: 100%;
    white-space: nowrap;
    text-overflow: ellipsis;
    border: 1px solid transparent;
    color: rgb(255, 255, 255) !important;

    @media only screen and (max-width: 639px) {
        font-size: 10px;
        height: 24px;
        line-height: 24px;
    }
    &:focus {
        border: 1px solid black !important;
    }
`;

export const SummaryCountText = styled(Text).attrs({
    as: 'p',
})`
    color: ${TextColors.lightPrimary};
    font-weight: bold;
`;

export const SummaryCountLabelText = styled(Text).attrs({
    as: 'p',
})`
    color: ${TextColors.lightPrimary};
    font-weight: regular;
    padding-left: 5px;
`;

export const FailedCountText = styled(Text).attrs({
    as: 'p',
})`
    color: ${MessagingColors.errorBlockIcon};
    font-weight: bold;
`;

export const FailedCountLabelText = styled(Text).attrs({
    as: 'p',
})`
    color: ${MessagingColors.errorBlockIcon};
    padding-left: 5px;
`;

export const SummaryCountStyling: IStackItemStyles = {
    root: {
        width: 'max-content',
    },
};

export const PersonaDropdownStyles: IStackItemStyles = {
    root: {
        marginLeft: '2%',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                height: 20 + 'px',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                height: 20 + 'px',
            },
        },
    },
};

export const GroupAndFilterIconStackItemStyles: IStackItemStyles = {
    root: {
        width: 'max-content',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                height: 24 + 'px !important',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                height: 10 + 'px !important',
                'margin-top': '-8px',
                'font-size': '5px',
            },
        },
    },
};

export const DelegationBarStyles: IMessageBarStyles = {
    root: {
        paddingLeft: '1.5%',
        paddingTop: '0.25%',
        paddingBottom: '0.25%',
        backgroundColor: '#D0E7F8', // Coherence DefaultThemeColors.blue20
        selectors: {
            '@media only screen and (min-device-width: 1023px) and  (min-width: 1024px) and (max-width: 1024px)': {
                marginLeft: '0px',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 1024px) and (max-width: 2048px)': {
                marginLeft: '36px',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 763px) and (max-width: 1023px)': {
                paddingLeft: '50px !important;',
                marginLeft: -48 + 'px !important;',
                marginBottom: -5 + 'px !important;',
                width: 'calc(100vw + 48px)',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 654px) and (max-width: 763px)': {
                marginLeft: -48 + 'px !important;',
                marginBottom: -5 + 'px !important;',
                paddingLeft: '30px !important;',
                width: 'calc(100vw + 48px)',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 640px) and (max-width: 654px)': {
                marginLeft: -48 + 'px !important;',
                marginBottom: -5 + 'px !important;',
                paddingLeft: '25px !important;',
                width: 'calc(100vw + 48px)',
            },
            '@media only screen and (min-device-width: 1023px) and  (min-width: 458px) and (max-width: 640px)': {
                paddingLeft: '25px !important;',
                marginLeft: -48 + 'px !important;',
                marginBottom: -5 + 'px !important;',
                width: 'calc(100vw + 48px)',
            },
            '@media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 458px)': {
                paddingLeft: '18px !important;',
                paddingTop: '0px',
                marginLeft: -48 + 'px !important;',
                marginBottom: -5 + 'px !important;',
                width: 'calc(100vw + 48px)',
            },
            '@media only screen and (min-device-width: 1023px) and (max-width: 320px)': {
                paddingLeft: '13px !important;',
                paddingTop: '0px',
                marginLeft: -48 + 'px !important;',
                marginBottom: -5 + 'px !important;',
                width: 'calc(100vw + 48px)',
            },
        },
    },
    icon: {
        selectors: {
            '@media only screen and (min-device-width: 1023px) and  (min-width: 320px) and (max-width: 639px)': {
                marginTop: '5px',
                fontSize: 14,
            },
        },
    },
};
export const SecondaryHeaderStackStyles = (isPanelOpen: boolean, isTopHeader?: boolean): IStackStyles => ({
    root: {
        height: '48px',
        background: `${isTopHeader ? '#0078D4' : '#e5e5e5'}`,
        width: `${isTopHeader ? '100vw' : 'calc(100vw - 48px)'}`,
        zIndex: 50,
        position: 'fixed',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                height: 24 + 'px !important',
                display: `${isPanelOpen ? 'none' : 'flex'}`,
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                height: 14 + 'px',
                display: `${isPanelOpen ? 'none' : 'flex'}`,
            },

        },
    },
});

export const SecondaryHeaderIconStyling = {
    height: '24px',
    paddingTop: '4px',
    selectors: {
        '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
            padding: 0,
            height: '20px',
        },
    },
};

export const PersonaMobileStyling = {
    root: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                maxWidth: '60px',
            },
        },
    },
    primaryText: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                maxWidth: '35px',
            },
        },
    },
};

export const SecondaryHeaderDelegationIconStyling: IStackItemStyles = {
    root: {
        height: '24px',
        display: 'flex',
        backgroundColor: 'inherit',
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                height: 24 + 'px !important',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                height: 14 + 'px',
            },
        },
    },
};

export const ToggleStackStyles: any = {
    container: {
        // border: 'solid black 2px',
        selectors: {
            '@media (min-width: 1280px)': {
                fontSize: 24 + 'px !important',
            },
        },
    },
    label: {
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px)': {
                display: 'none',
            },
            '@media (min-device-width: 1023px) and (max-width: 320px)': {
                display: 'none',
            },
        },
    },
};

export const BulkMessageHeight = styled.div``;

export const MessageBarText = styled.div`
    font-size: '4px';
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px) {
        line-height: 20px !important;
        margin-top: -4px;
    }
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        line-height: 20px !important;
        margin-top: -4px;
    }
`;
