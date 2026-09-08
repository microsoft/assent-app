//uncomment styled import for performance testing
//import styled from "styled-components";

export const CompactIndicator = styled.button`
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 4px 10px;
    margin-top: 6px;
    background: #f5f5f5;
    border: 1px solid #e1dfdd;
    border-radius: 12px;
    cursor: pointer;
    white-space: nowrap;
    overflow: hidden;
    max-width: 100%;
    font-family: inherit;
    font-size: inherit;
    text-align: left;
    &:hover {
        background: #e8e8e8;
    }
`;

export const TrailingIcon = styled.span`
    color: #0072c9;
    flex-shrink: 0;
    display: inline-flex;
    align-items: center;
    margin-left: auto;
`;

export const IndicatorText = styled.span`
    font-size: 11px;
    font-weight: 600;
    color: #323130;
    overflow: hidden;
    text-overflow: ellipsis;
`;

export const AttachmentLink = styled.span`
    color: #0072c9;
    text-decoration: none;
    &:hover {
        text-decoration: underline;
    }
`;

export const CalloutItem = styled.button`
    display: flex;
    align-items: flex-start;
    gap: 10px;
    width: 100%;
    padding: 8px 16px;
    border: none;
    background: none;
    cursor: pointer;
    font-family: inherit;
    text-align: left;
    &:hover {
        background: #f3f2f1;
    }
`;

export const CalloutItemIcon = styled.span`
    font-size: 16px;
    flex-shrink: 0;
    display: inline-flex;
    align-items: center;
    margin-top: 1px;
`;

export const CalloutItemContent = styled.div`
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
`;

export const CalloutItemName = styled.span`
    font-size: 13px;
    font-weight: 600;
    color: #323130;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
`;

export const CalloutItemSnippet = styled.span`
    font-size: 11px;
    color: #605e5c;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    line-height: 1.4;
`;
