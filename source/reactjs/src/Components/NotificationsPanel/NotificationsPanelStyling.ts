
import { ITheme, mergeStyleSets, mergeStyles, getTheme, getFocusStyle } from '@fluentui/react/lib/Styling';

const theme: ITheme = getTheme();
const { palette, semanticColors, fonts } = theme;

export const container = mergeStyles({
    overflow: 'auto',
    maxHeight: 500,
});

export const iconClass = mergeStyles({
    fontSize: 18,
    padding: '2px 0',
});

export const classNames = mergeStyleSets({
    itemCellBold: [
        getFocusStyle(theme, { inset: -1 }),
        {
            // minHeight: 54,
            padding: 10,
            boxSizing: 'border-box',
            borderBottom: `1px solid ${semanticColors.bodyDivider}`,
            display: 'flex',
            cursor: 'pointer',
            selectors: {
                '&:hover': { background: palette.neutralLight },
                '::before': {
                    content: "'\\2022'",
                    color: palette.blue,
                    fontWeight: 'bold',
                    display: 'inline-block',
                    fontSize: '1.5em',
                    width: '10px',
                    marginRight: '10px',
                    marginTop: '-10px',
                },
                '&:focus': {
                    backgroundColor: palette.neutralLight,
                    borderTop: '1px solid black',
                    borderBottom: '1px solid black',
                },
            },
        },
    ],
    itemCell: [
        getFocusStyle(theme, { inset: 1 }),
        {
            // minHeight: 54,
            padding: 10,
            boxSizing: 'border-box',
            borderBottom: `1px solid ${semanticColors.bodyDivider}`,
            display: 'flex',
            cursor: 'pointer',
            selectors: {
                '&:hover': { background: palette.neutralLight },
                '&:focus': {
                    backgroundColor: palette.neutralLight,
                    borderTop: '1px solid black',
                    borderBottom: '1px solid black',
                },
            },
        },
    ],
    itemImage: {
        flexShrink: 0,
    },
    itemContent: {
        marginLeft: 10,
        overflow: 'hidden',
        flexGrow: 1,
    },
    itemName: [
        fonts.medium,
    ],
    itemNameBold: [
        fonts.medium,
        {
            fontWeight: 'bold',
        },
    ],
    itemMessageBold: {
        fontSize: fonts.small.fontSize,
        // marginBottom: 10,
        fontWeight: 'bold',
        fontStyle: 'italic',
    },
    itemMessage: {
        fontSize: fonts.small.fontSize,
        // marginBottom: 10,
        fontStyle: 'italic',
    },
    chevron: {
        alignSelf: 'center',
        marginLeft: 10,
        color: palette.neutralTertiary,
        fontSize: fonts.smallPlus.fontSize,
        flexShrink: 0,
    }
});

export const ReportWarning = mergeStyles({
    fontSize: '14px',
    padding: '2px 0',
    color: palette.redDark
});

export const IncidentTriangle = mergeStyles({
    fontSize: '14px',
    padding: '2px 0',
    color:  palette.yellowDark
});

export const InfoSolid = mergeStyles({
    fontSize: '14px',
    padding: '2px 0',
    color:  palette.blue
});

// export const activeItem = mergeStyles({
//     background: palette.neutralLight,
// });

// CSS to render badge
export const badgeClass = mergeStyles({
    fontSize: '10px',
    fontWeight: 600,
    display: 'inline',
    position: 'absolute',
    height: '16px',
    minWidth: '10px',
    borderRadius: '8px',
    lineHeight: '16px',
    textAlign: 'center',
    paddingLeft: '3px',
    paddingRight: '3px',
    backgroundColor: 'rgb(0, 60, 148)',
    color: 'rgb(255, 255, 255)',
    top: '8px',
    right: '7px',
    selectors: {
        '&:hover': {
            color: 'rgb(0, 60, 148)',
            backgroundColor: 'rgb(255, 255, 255)',
        }
    }
});

export const bellIconClass = mergeStyles({
    fontSize: 16,
    paddingTop: '5px',
    color: 'white',
    selectors: {
        '@media only screen and (max-width: 754px)': {
            color: 'black',
            paddingLeft: '17px',
            paddingRight: '05px',
        } 
    }
});

export const italicStyle = mergeStyles({
    fontStyle: 'italic',
    margin: 15, 
    fontSize: 20,
    fontWeight: 400
})

export const emptyNotifications = mergeStyles({
    marginTop: '25%',
})