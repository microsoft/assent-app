import { IStackTokens, IStackStyles } from '@fluentui/react/lib/Stack';

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