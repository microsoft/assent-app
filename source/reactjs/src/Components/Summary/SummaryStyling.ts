import { maxWidth, minWidth } from '../Shared/Styles/Media';
import { FontIcon } from '@fluentui/react/lib/Icon';
import { FontSizes } from '@fluentui/react';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const SummaryContainer = styled.div<any>`
    padding-top: 0;
    margin: 0;
    min-width: ${(props) => (props.windowWidth <= 640 ? '100%' : '95%')};
    margin-bottom: 20px;
    ${maxWidth.m} {
        min-width: 100%;
        margin-top: ${(props) => (props.isPanelOpen && props.selectedPage === 'summary' ? '-12.5%' : '0px')};
    }
    @media (max-width: 640px) {
        min-width: 100%;
        margin-top: ${(props) => (props.isPanelOpen ? '-24px' : '0px')};
    }
`;

export const SummaryViewWrapper = styled.div<any>`
    margin-top: ${(props) => (props.isDashboardView ? '-30px' : '0px')};
`;

export const SummaryTablesContainer = styled.div<any>`
    padding-top: 0;
    margin: 0;
    margin-bottom: 40px;
    min-width: ${(props) => (props.windowWidth <= 640 ? '100%' : '95%')};
    margin-left: ${(props) => (props.isBulkSelected ? (props.windowWidth <= 640 ? '0px' : '-20px') : '0px')};
    margin-top: ${(props) => (props.isPanelOpen && props.isMobile ? '-12.5%' : '0px')};
    ${maxWidth.m} {
        margin-left: 0px;
    }
    @media (max-width: 640px) {
        min-width: 100%;
        margin-left: 0px;
        margin-top: ${(props) => (props.isPanelOpen ? '-12px' : '0px')};
    }
`;

export const TenantLabel = styled.div`
    height: 100%;
    flex-grow: 1;
    margin-left: 6px;

    h2 {
        font-size: 1em;
        font-weight: bold;
        margin: 0;
    }
`;

export const TenantDescription = styled.div`
    font-style: italic;
    font-size: 0.85em;
    color: #605e5c;
    margin-top: 2px;
    margin-left: calc(3vw + 8px);
    margin-right: calc(3vw + 8px);

    ${minWidth.xl} {
        margin-left: calc(4vw - 22px);
        margin-right: calc(4vw - 22px);
    }
`;

export const CardTenantImage = styled.div<any>`
    padding-top: 6px;
    height: 100%;
    float: left;
    margin-left: ${(props) => (!props.isCardViewSelected && props.isBulkSelected ? '-19px' : '0px')};
    @media (-ms-high-contrast: active), (forced-colors: active) {
        img {
            forced-color-adjust: none;
            background-color: #ffffff;
        }
    }
`;
export const SelectAllCheckStyle = styled.div`
    margin-left: 0px;
    ${minWidth.xl} {
        margin-left: -20px;
    }
`;
export const CardGroupLabel = styled.div`
    display: flex;
    flex-direction: row;
    margin-left: calc(2vw + 4px);
    margin-right: calc(2vw + 4px);
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;

    ${minWidth.xl} {
        margin-left: calc(4vw - 22px);
        margin-right: calc(4vw - 22px);
    }
`;
export const RefreshMedia: any = styled.div<any>`
    visibility: visible;
    left: 95.9%;
    ${minWidth.s} {
        left: 86.4%;
        visibility: hidden;
    }
    ${minWidth.m} {
        visibility: visible;
        margin-top: -40px;
    }

    ${minWidth.l} {
        left: 93.4%;
        visibility: hidden;
        margin-top: -40px;
    }

    ${minWidth.xl} {
        left: ${(props: any) => (props.isDetailsExpanded ? '93.4%' : '94.9%')};
        visibility: visible;
        margin-top: -45px;
    }

    ${minWidth.xxl} {
        visibility: visible;
        margin-top: -45px;
    }

    ${minWidth.xxxl} {
        visibility: visible;
        margin-top: -45px;
    }

    /* At 200% zoom (≤640px), avoid overlap with analytics header */
    @media (max-width: 640px) {
        margin-top: 0px;
    }

    @media only screen and (orientation: portrait) {
        left: 94.9%;
        visibility: visible;
        margin-top: -45px;
    }
`;

export const RefreshMediaDuplicate = styled.div<any>`
    visibility: visible;
    left: 80.4%;
    margin-top: 0px;

    ${minWidth.s} {
        left: 86.4%;
        visibility: hidden;
    }

    ${minWidth.m} {
        visibility: hidden;
        margin-top: -20px;
        left: 93.4%;
    }
    ${minWidth.l} {
        left: 93.4%;
        visibility: hidden;
        margin-top: -45px;
    }
    ${minWidth.xl} {
        visibility: hidden;
        margin-top: -45px;
        left: 94.9%;
    }
    ${minWidth.xxl} {
        visibility: hidden;
        margin-top: -45px;
        left: 94.9%;
    }
    ${minWidth.xxxl} {
        visibility: hidden;
        margin-top: -45px;
        left: 94.9%;
    }
`;
export const PersonaContainer = styled.div`
    height: 100%;
    width: 28px;
    float: left'
`;

export const SubmitterLabel = styled.div`
    height: 100%;
    flex-grow: 1;
    margin-left: 6px;
`;

export const CalendarIcon = styled(FontIcon)`
    font-size: 26px;
`;

export const DetailCardContainer = styled.div<any>`
    height: ${(props) =>
        props.windowWidth < 572
            ? props.windowWidth < 320
                ? props.windowHeight - (50 + props.footerHeight)
                : props.windowHeight - (60 + props.footerHeight)
            : props.windowHeight -
              (90 + props.bulkMessagebarHeight + props.aliasMessagebarHeight + props.footerHeight)}px;
    overflow-y: auto;
    background: white;
    margin-top: 5px;
    @media only screen and (min-device-width: 1023px) and (min-width: 640px) and (max-width: 2048px) {
        margin-top: ${(props) => (props.selectedPage === 'summary' ? '0px' : '10px')};
    }
    @media only screen and (max-width: 640px) {
        position: fixed;
        top: 48px;
        left: 0;
        right: 0;
        bottom: 0;
        height: auto;
        margin: 0;
        padding: 0;
        z-index: 40;
        overflow: hidden auto;
        box-sizing: border-box;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px) {
        top: 24px;
    }
`;

export const SummaryLayoutContainer = styled.div<any>`
    height: ${(props) => {
        // At 200% zoom, let content flow naturally in the scrollable Main container
        if (props.windowWidth <= 640) {
            return 'auto';
        }
        // Desktop: fixed height for internal scrolling
        return (
            props.windowHeight -
            (145 +
                props.bulkMessagebarHeight +
                props.aliasMessagebarHeight +
                props.footerHeight +
                props.bulkFailureMessageOffset) +
            'px'
        );
    }};
    overflow-y: ${(props) => (props.isDashboardView ? 'hidden' : props.windowWidth <= 640 ? 'visible' : 'auto')};
    overflow-x: hidden;
    padding: 0 20px;
    box-sizing: border-box;
    ${maxWidth.m} {
        padding-left: 10px;
        padding-right: 10px;
    }
`;

export const ErrorWrapStyle = styled.div`
    white-space: pre-wrap;
`;

export const disableInteractionStyle = { root: { pointerEvents: 'none', opacity: '0.5' } };

export const SummaryPageTitle = styled.h1<any>`
    padding-left: 0px;
    font-size: 20px;
    margin: 0 0 5px 0;
    word-wrap: break-word;
    overflow-wrap: break-word;

    @media (max-width: 640px) {
        font-size: 18px;
    }
`;

export const SearchResultsTitle = styled.h1<any>`
    font-size: 20px;
`;

export const SearchResultsSubtext = styled.div`
    font-size: 14px;
    color: #d6d6d;
    font-weight: 400;
    margin-top: 2px;
    margin-left: 2px;
`;
