import { FontWeights, FontSizes } from '@fluentui/react';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const Divider = styled.div`
    position: relative;
    left: 0%;
    right: 0%;
    top: 29.14%;
    bottom: 70.64%;
    margin-bottom: 24px;

    border: 1px solid #E8E8E8;
`;

// Type hierarchy - B
export const Subheader = styled.h2`
    font: SegoeUI;
    color: black;
    font-weight: ${FontWeights.semibold};
    font-size: ${FontSizes.size18};
    margin-bottom: 8px;
`;

// Type hierarchy - C
export const BaseCTitle = styled.h3` 
    font: SegoeUI;
    color: black;
    font-weight: ${FontWeights.semibold};
    font-size: ${FontSizes.size14};
    margin-bottom: 16px;
`;

// Type hierarchy - D
export const MiniTopicHeader = styled.p`
    font: SegoeUI;
    color: black;
    font-weight: ${FontWeights.bold};
    font-size: ${FontSizes.size14};
`;

// Type hierarchy - E
export const Body = styled.p`
    font: SegoeUI;
    color: black;
    font-weight: ${FontWeights.regular};
    font-size: ${FontSizes.size14};
    margin-bottom: 24px;
`;

export const QuickLink = styled.div`
    margin-bottom: 16px;
`;