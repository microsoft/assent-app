import { maxWidth, minWidth } from '../Shared/Styles/Media';
import { FontIcon } from '@fluentui/react/lib/Icon';
import { FontSizes } from '@fluentui/react';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const SummaryContainer = styled.div<any>`
    padding-top: 0;
    margin: 0;
    min-width: 95%;
    margin-bottom: 20px;
    margin-top: 0px;
    ${maxWidth.m} {
        min-width: 100%;
        margin-top: ${props => (props.isPanelOpen && props.selectedPage === 'summary' ? '-12.5%' : '0px')};
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 571px) and (max-width: 639px) {
        margin-top: ${props => (props.isPanelOpen ? '-48px' : '0px')};
        min-width: 100%;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 571px) {
        margin-top: ${props =>
            props.isPanelOpen ? -(props.bulkMessagebarHeight + props.aliasMessagebarHeight + 24) + 'px' : '0px'};
        min-width: 100%;
    }
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        margin-top: ${props =>
            props.isPanelOpen ? -(props.bulkMessagebarHeight + props.aliasMessagebarHeight + 18) + 'px' : '0px'};
        width: 100%;
    }
`;

export const SummaryTablesContainer = styled.div<any>`
    padding-top: 0;
    margin: 0;
    margin-bottom: 40px;
    min-width: 95%;
    margin-left: ${props => (props.isBulkSelected ? '-20px' : '0px')};
    margin-top: ${props => (props.isPanelOpen && props.isMobile ? '-12.5%' : '0px')};
    @media only screen and (min-device-width: 1023px) and (min-width: 571px) and (max-width: 639px) {
        margin-top: ${props => (props.isPanelOpen ? '-48px' : '0px')};
        min-width: 100%;
    }
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 571px) {
        margin-top: ${props => (props.isPanelOpen ? '0px' : '0px')};
        min-width: 100%;
    }
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {
        margin-top: ${props => (props.isPanelOpen ? '-18px' : '0px')};
        width: 100%;
    }
`;

export const TenantLabel = styled.div`
    height: 100%;
    flex-grow: 1;
    margin-left: 6px;
`;

export const CardTenantImage = styled.div<any>`
    padding-top: 6px;
    height: 100%;
    float: left;
    margin-left: ${props => (!props.isCardViewSelected && props.isBulkSelected ? '-19px' : '0px')};
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
    margin-left: calc(3vw + 8px);
    margin-right: calc(3vw + 8px);
    align-items: center;

    ${minWidth.xl} {
        margin-left: calc(4vw - 22px);
        margin-right: calc(4vw - 22px);
    }
`;
export const RefreshMedia = styled.div`
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
        left: 94.9%;
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

    @media only screen and  (orientation: portrait) {
        left: 94.9%;
        visibility: visible;
        margin-top: -45px;
    }
`;

export const RefreshMediaDuplicate = styled.div`
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
    height: ${props =>
        props.windowWidth < 572
            ? props.windowWidth < 320
                ? props.windowHeight - (50 + props.footerHeight)
                : props.windowHeight - (60 + props.footerHeight)
            : props.windowHeight -
              (140 + props.bulkMessagebarHeight + props.aliasMessagebarHeight + props.footerHeight)}px;
    overflow-y: auto;
    background: white;
    margin-top: 35px;
    @media only screen and (min-device-width: 1023px) and (min-width: 640px) and (max-width: 2048px) {
        margin-top: ${props => (props.selectedPage === 'summary' ? '-14px' : '35px')};
    }
`;

export const SummaryLayoutContainer = styled.div<any>`
    height: ${props =>
        props.windowWidth < 572
            ? props.windowWidth < 320
                ? props.windowHeight -
                  (44 + props.bulkMessagebarHeight + props.aliasMessagebarHeight + props.footerHeight)
                : props.windowHeight -
                  (70 +
                      props.bulkMessagebarHeight +
                      props.aliasMessagebarHeight +
                      props.footerHeight +
                      props.bulkFailureMessageOffset / 2)
            : props.windowHeight -
              (145 +
                  props.bulkMessagebarHeight +
                  props.aliasMessagebarHeight +
                  props.footerHeight +
                  props.bulkFailureMessageOffset)}px;
    overflow-y: auto;
    overflow-x: hidden;
    @media only screen and (min-device-width: 1023px) and (min-width: 320px) and (max-width: 639px) {     
        margin-top: 30px;
    }
    @media only screen and (min-device-width: 1023px) and (max-width: 320px) {        
        margin-top: 30px;
    }    
`;

export const ErrorWrapStyle = styled.div`
    white-space: pre-wrap;
`;

export const disableInteractionStyle = { root: { pointerEvents: 'none', opacity: '0.5' } };

export const SummaryPageTitle = styled.h1<any>`
    margin-left: ${(props) => (props.isTableView ? 'calc(2vw - 8px)' : 'calc(3vw)')};
    font-size: 20px;
    margin-bottom: 5px;
`;
