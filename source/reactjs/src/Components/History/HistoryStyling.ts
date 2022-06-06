import { IStackTokens, IStackStyles } from '@fluentui/react/lib/Stack';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const HistoryContainer: any = styled.div`
    padding: 1% 2% 0 2%;
    height: ${(props: any) => props.windowHeight - 150}px;

    @media only screen and (max-device-width: 480px) {
        margin-left: 0;
    }
    @media only screen and (max-width: 320px) {
        height: ${(props: any) => props.windowHeight - 50}px;
    }
`;

export const HistoryTitle = styled.h1`
    padding-bottom: 4px;
`;

export const HistoryNavStackStyles: IStackStyles = {
    root: {
        marginBottom: '1%',
        marginTop: '1%'
    }
};

export const HistoryNavStackTokens: IStackTokens = {
    childrenGap: 15
};

export const HistoryTableContainer = styled.div<any>`
    position: relative;
    padding-right: 1%;
    padding-bottom: 5%;
    overflow-y: hidden;
    background-color: white;
    height: ${props =>
        props.windowWidth < 1024
            ? props.windowWidth >= 640 && props.isPanelOpen
                ? props.windowHeight * 0.65
                : props.windowHeight - 10
            : props.windowHeight - 250}px;

    @media only screen and (max-width: 320px) {
        overflow-y: initial;
    }
`;

export const HistoryColumnTenantImage = styled.div`
    width: 16px;
    height: auto;
    padding-right: 36px;
`;

export const HistoryIconStyling = {
    height: '100%'
};
