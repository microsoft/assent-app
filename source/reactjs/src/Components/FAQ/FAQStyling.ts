import { IButtonStyles, IStackStyles } from '@fluentui/react';
import { CoherenceColors } from '../Shared/SharedColors';

export interface IAdditionalHTMLAttribute {
    windowHeight?: number;
    windowWidth?: number;
}

export const FAQContainer = styled.div`
    padding: 1% 2% 0px;
    height: ${props =>
        props.windowWidth < 572
            ? props.windowWidth < 320
                ? props.windowHeight - 44
                : props.windowHeight - 70
            : props.windowHeight - 145}px;
    scroll-behavior: smooth;
    overflow-y: auto;
    overflow-x: hidden;
    margin-left: 25px;

    @media only screen and (max-width: 320px) {
        height: ${(props: IAdditionalHTMLAttribute) => props.windowHeight - 50}px;
    }
`;

export const FAQTitle = styled.h1`
    padding-bottom: 4px;
`;

export const QuickLink = styled.div`
    margin-bottom: 16px;
`;

export const interactiveStyles = {
    rootHovered: { backgroundColor: CoherenceColors.InputBorder },
    rootPressed: { backgroundColor: CoherenceColors.InputBorder },
};

export const interactiveStackStyles: IStackStyles = {
    root: {
        width: "210px",
        padding: "1px 1px 1px 1px",
        borderRadius: "12px",
        border: "2px solid #b178780f",
        selectors: {
            ':hover': {
                backgroundColor: CoherenceColors.InputBorder,
                cursor: "pointer"
            }
        }
    }
};

export const QuickTourButtons: IButtonStyles = {
    root: {
        width: 100,
    }
}

export const FAQInnerContentStyles: IStackStyles = {
    root: {
        selectors: {
            ['@media (min-width: 706px)']: {
                marginLeft: "40px"
            },
            ['@media (min-width: 374px) and (max-width: 705px)']: {
                marginLeft: "40px"
            },
        }
    }
}
