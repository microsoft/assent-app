import { ITextFieldStyles } from '@fluentui/react/lib/TextField';
import { Depths } from '@fluentui/theme';
import { IStackTokens, IStackStyles, IStackItemStyles } from '@fluentui/react/lib/Stack';
import { IDropdownStyles } from '@fluentui/react/lib/Dropdown';
import { maxWidth, breakpointMap } from '../Styles/Media';
import { makeStyles, shorthands, tokens } from "@fluentui/react-components";

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
        position: viewType === 'FLY' ? 'relative' : 'sticky',
        width: 'calc(100% - 16px)',
        background: '#ffffff',
        top: '0px',
        paddingBottom: '5px',
        zIndex: 10,
        selectors: {
            '@media (min-device-width: 1023px) and (min-width: 640px) and (max-width: 2048px)': {
                marginTop: '0px',
            },
            '@media (max-width: 640px)': {
                position: 'static',
                top: 'auto',
                width: '100%',
                height: 'auto',
            },
        },
    },
});

export const StickyDetailsHeder: IStackStyles = {
    root: {
        position: 'sticky',
        width: '100%',
        background: 'white',
        height: '35px',
        top: '0px',
        zIndex: 10,
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
    @media only screen and (max-width: 640px) {
        position: static;
        box-shadow: none;
        border-left: none;
        overflow: visible;
        max-height: none !important;
        height: auto !important;
        width: 100%;
        padding-bottom: 16px;
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
        marginBottom: `10px`,
        selectors: {
            '@media (max-width: 319px)': {
                marginBottom: `${footerHeight + 10}px`,
            },
            '@media (max-width: 640px)': {
                marginBottom: '0px',
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
    if (windowWidth <= breakpointMap.l) {
        // On mobile, size the modal to most of the viewport (not full-screen) so it reads as a
        // popup overlaying the page while still leaving a visible margin around it. The maximize/
        // restore button toggles the height so it stays functional on small devices.
        return {
            width: windowWidth * 0.95,
            height: isExpanded ? windowHeight * 0.85 : windowHeight * 0.5,
        };
    }
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

makeStyles({
    provider: {
      maxWidth: "320px",
      backgroundColor: tokens.colorNeutralBackground3,
      ...shorthands.padding("16px"),
      ...shorthands.borderRadius("12px"),
      display: "flex",
      columnGap: "24px",
      flexDirection: "column",
      height: "600px",
    },
    latencyWrapper: {
      paddingTop: "16px",
    },
    chat: {
      ...shorthands.padding(0, "16px", "16px"),
      overflowY: "scroll",
      height: "100%",
      marginLeft: `calc(${tokens.spacingHorizontalL} * -1)`,
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: tokens.colorNeutralForeground4,
        ...shorthands.border("2px", "solid", tokens.colorNeutralBackground3),
        ...shorthands.borderRadius(tokens.borderRadiusMedium),
      },
      "&::-webkit-scrollbar-track": {
        backgroundColor: tokens.colorNeutralBackground3,
      },
      "&::-webkit-scrollbar": {
        width: tokens.spacingHorizontalS,
      },
    },
    chatMessage: {
      display: "block",
      marginLeft: 0,
    },
    chatMessageBody: {
      backgroundColor: tokens.colorNeutralBackground1,
      boxShadow: tokens.shadow4,
      boxSizing: "content-box",
      display: "block",
    },
    chatMyMessage: {
      gridTemplateAreas: "unset",
      marginLeft: 0,
    },
    chatMyMessageBody: {
      backgroundColor: "#E0E7FF",
    },
    inputArea: {
      paddingTop: "16px",
    },
  });
