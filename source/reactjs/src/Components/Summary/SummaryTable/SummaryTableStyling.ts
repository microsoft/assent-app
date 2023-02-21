import { IStackStyles } from '@fluentui/react/lib/Stack';
import { mergeStyles } from '@fluentui/react/lib/Styling';
import { TextColors } from '../../Shared/SharedColors';
import { IDropdownStyles } from '@fluentui/react/lib/Dropdown';
import { maxWidth, minWidth } from '../../Shared/Styles/Media';

export const SummaryTableContainer = styled.div`
    padding: 0 2% 0 2%;

    @media only screen and (max-device-width: 480px) {
        margin-left: 0;
        padding-right: 0;
    }
`;

export const SummaryTableMainContainer = styled.div`
    position: relative;
    padding-right: 0.5%;
    padding-bottom: 0.5%;
    overflow-y: hidden;
    background-color: white;
`;

export const failedIconStyle = {
    color: '#A80000',
    fontSize: 12,
    cursor: 'default'
};

export const pendingIconStyle = {
    fontSize: 11,
    cursor: 'default'
};

export const fileIconCell = mergeStyles({
    textOverflow: 'unset!important'
});

export const fileIconImg = {
    verticalAlign: 'middle',
    maxHeight: '16px',
    maxWidth: '16px'
};

export const MailReadIcon = styled.div`
    width: 18px;
    height: 18px;
    background-image: url('/icons/read-blk.png');
    background-size: 18px 18px;

    @media screen and (-ms-high-contrast: white-on-black) {
        background-image: url('/icons/read-wht.png');
    }
`;

export const MailUnreadIcon = styled.div`
    width: 18px;
    height: 18px;
    background-image: url('/icons/unread-blk.png');
    background-size: 18px 18px;

    @media screen and (-ms-high-contrast: white-on-black) {
        background-image: url('/icons/unread-wht.png');
    }
`;

export const paginationWidth = {
    root: {
        width: '50%',
        marginBottom: '0px',
        selectors: {
            '@media (min-device-width: 320px) and (max-width: 320px)': {
                transform: ' scale(0.6)'
            }
        }
    }
};

export const paginationAlign = { marginLeft: -132 };

export const DataGridContainer = styled.div<any>`
    display: ${props => (props.isFilterPanelOpen ? 'flex' : 'block')};
`;

export const StackStylesBottomBorder: IStackStyles = {
    root: { borderBottom: `1px solid #C6C6C6`, paddingBottom: '10px' }
};

export const StackStylesRowCount = (isMaximized: boolean, isPanelOpen: boolean): IStackStyles => ({
    root: {
        marginLeft: `${isMaximized ? '3%' : isPanelOpen ? '1%' : '0.5%'}`,
        selectors: {
            [minWidth.xxxl]: {
                marginLeft: `${isMaximized ? '2%' : isPanelOpen ? '1%' : '0.5%'}`
            },
            [maxWidth.xxl]: {
                marginLeft: `${isMaximized ? '4%' : '1%'}`
            },
            [maxWidth.xl]: {
                marginLeft: `${isMaximized ? '4%' : '2%'}`
            },
            [maxWidth.m]: {
                marginLeft: '4%'
            }
        }
    }
});

export const truncateTextWithEllipsis = {
    fontSize: '14px',
    color: TextColors.lightPrimary,
    textOverflow: 'ellipsis',
    overflow: 'hidden'
};

export const searchCriteraDropdownStyles: Partial<IDropdownStyles> = {
    root: { display: 'flex' },
    dropdown: { maxWidth: 500, minWidth: 200 },
    label: { paddingRight: '5px' }
};
