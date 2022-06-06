import { Text } from '@fluentui/react/lib/Text';
import { IDropdownStyles } from '@fluentui/react/lib/Dropdown';
import { IStackStyles } from '@fluentui/react/lib/Stack';
import { FontSizes, FontWeights, IMessageBarStyles } from '@fluentui/react';
import { Depths } from '@fluentui/theme';
import { SurfaceColors } from '../Shared/SharedColors';
import { minWidth } from '../Shared/Styles/Media';
//uncomment styled import for performance testing
//import styled from "styled-components";

export interface IAdditionalHTMLAttribute {
    windowWidth: number;
    windowHeight: number;
}

export const Container = styled.div`
    margin-top: 35px;
    margin-bottom: 35px;
    margin-left: 2%;
    margin-right: 5%;
`;

export const Space = styled.div`
    margin-bottom: 24px;
`;

export const CenterHeightSpace = styled.div`
    margin-top: 10%;
`;

export const HeightBelowShell = styled.div`
    height: ${(props: IAdditionalHTMLAttribute) =>
        props.windowWidth < 480 ? props.windowHeight - 96 : props.windowHeight - 150}px;
`;

export const PageHeading = styled(Text).attrs({
    as: 'h1',
    variant: 'xLarge',
    block: true
})`
    margin-bottom: 6px;
`;

export const PageDescription = styled(Text).attrs({
    as: 'p',
    block: true
})`
    margin-bottom: 24px;
`;

export const SectionTitle = styled(Text).attrs({
    as: 'h2',
    variant: 'large',
    block: true
})`
    margin-bottom: 24px;
`;

export const SpinnerContainer = styled.div`
    height: 100%;
    width: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
`;

// need to move to separate file
export const EmptySearchIcon = styled.div`
    width: 68px;
    height: 68px;
    background-image: url('/icons/empty-search.svg');
    background-size: 68px 68px;
    font-size: 68;
    margin: 0 50px;
`;

export const ErrorResultIcon = styled.div`
    width: 68px;
    height: 68px;
    background-image: url('/icons/error-result.svg');
    background-size: 68px 68px;
    font-size: 68;
    margin: 0 50px;
`;

export const ErrorText = styled.p`
    color: #d73b02;
`;

// move this too
export const MessageBarTitle = styled(Text).attrs({
    as: 'p'
})`
    font-size: ${FontSizes.size16};
    font-weight: ${FontWeights.semibold};
`;

export const ErrorMessage = styled.div`
    padding: 0.75rem 1.25rem;
    border: 1px solid transparent;
    margin-bottom: 1rem;
    border-radius: 0.25rem;

    color: #721c24;
    background-color: #f8d7da;
    border-color: #f5c6cb;
`;

export const SuccessMessage = styled.div`
    padding: 0.75rem 1.25rem;
    border: 1px solid transparent;
    margin-bottom: 1rem;
    border-radius: 0.25rem;

    color: #155724;
    background-color: #d4edda;
    border-color: #c3e6cb;
`;

export const SmallDropdownStyles: Partial<IDropdownStyles> = { dropdown: { maxWidth: 500, minWidth: 150 } };

export const MediumDropdownStyles: Partial<IDropdownStyles> = { dropdown: { maxWidth: 500, minWidth: 200 } };

export const StackStylesBottomBorder: IStackStyles = {
    root: { borderBottom: `1px solid #C6C6C6`, paddingBottom: '10px' }
};

export const StackStylesOverflowWithEllipsis: IStackStyles = { root: { overflow: 'hidden', textOverflow: 'ellipsis' } };

export const bulkTableViewBottomOffset = 350;

export const FilterContainer = styled.div<any>`
    overflow-y: auto;
    background: #fafafa;
    width: 400px;
    margin-left: 10px;
    padding-right: 10px;
    padding-top: 1%;
    height: 100vh;
     ${minWidth.xl} {      
        height: calc(100vh - ${bulkTableViewBottomOffset}px);       
    }   
    @media (min-device-width: 1023px) and (min-width: 639px) and (max-width: 916px){ 
        width: 900px;
    }
`;

export const LargeMessageStyles: IMessageBarStyles = {
    root: {
        paddingLeft: '1.5%',
        paddingTop: '0.5%',
        paddingBottom: '0.5%',
        backgroundColor: '#D0E7F8' // Coherence DefaultThemeColors.blue20
    },
    text: {
        fontSize: '14px'
    }
};

export const PullTenantSummaryCountDiv = styled.div`
    width: 80%;
    display: flex;
    flex-direction: column;
    border: 1px rgb(0, 120, 212) solid;
    margin-left: calc(3vw + 8px);
    ${minWidth.xl} {
        margin-left: calc(4vw - 22px);
    }
    margin-bottom: 30px;
`;

export const PullTenantSummaryBannerHeader = styled.div`
    height: 30%;
    background-color: gray;
    font-size: 14px;
    margin-bottom: 10px;
`;

export const PullTenantSummaryBannerContainer = styled.div`
    padding: 2px 16px;
`;

export const PullTenantSummaryCountCardDiv = styled.div<any>`
    background-color: ${SurfaceColors.primary};
    border-radius: 2px;
    box-shadow: ${Depths.depth4};
    margin-bottom: 16px;
    margin-left: 16px;
    margin-right: 16px;
    padding-right: 20px;
    width: 100%;
    transition: box-shadow 0.3s;
    &:hover {
        box-shadow: ${Depths.depth64};
    }
    max-width: 300px;
    min-width: 250px;
    cursor: pointer;
    box-shadow: 'none';

    ${minWidth.l} {
        margin-bottom: 24px;
        margin-left: ${props => (props.isTableView ? '0px' : '12px')};
        margin-right: 12px;
        padding-right: 0px !important;
    }

    ${minWidth.xl} {
        margin-bottom: 24px;
        margin-left: ${props => (props.isTableView ? '0px' : '12px')};
        margin-right: 12px;
        padding-right: 0px !important;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px) {
        margin-top: 30px;
    }
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        margin-top: 30px;
    }
    height: 150px;
`;

export const PullTenantSummaryCountCardHeader = styled.div`
    height: 25%;
    padding-top: 8px;
    padding-left: 12px;
    display: flex;
    flex-direction: row;
    align-items: center;
`;

export const CountValue = styled.div`
    font-size: ${FontSizes.size24};
    font-weight: ${FontWeights.semibold};
    margin-top: 18px;
    text-align: left;
    padding-left: 12px;
`;

export const FooterRow = styled.div`
    height: 22.5%;
    font-size: ${FontSizes.size12};
    padding-left: 12px;
    padding-top: 24px;
`;

export const PullTenantCardLabel = styled.div`
    height: 15%;
    font-size: ${FontSizes.size14};
    font-weight: ${FontWeights.regular};
    padding-left: 12px;
`;

export const dropdownStyles: Partial<IDropdownStyles> = {
    dropdownOptionText: { overflow: 'visible', whiteSpace: 'normal' },
    dropdownItem: { height: 'auto' },
    title: {overflow: 'visible', whiteSpace: 'normal' , height:'auto'}
};
