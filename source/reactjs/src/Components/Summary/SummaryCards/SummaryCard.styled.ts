import { NeutralColors } from '@fluentui/theme';
import { Depths } from '@fluentui/theme';
import { FontWeights, FontSizes } from '@fluentui/react';
import { mergeStyleSets } from '@fluentui/react';
import { SurfaceColors } from '../../Shared/SharedColors';
import { minWidth } from '../../Shared/Styles/Media';
//uncomment styled import for performance testing
//import styled from "styled-components";

export const TextContainer = styled.div`
    height: 100%;
    margin-left: 12px;
    margin-right: 12px;
    padding-bottom: 1%;
    position: relative;
`;

export const Submitter = styled.div`
    font-size: ${FontSizes.size16};
    font-weight: ${FontWeights.semibold};
`;

export const Date = styled.div`
    font-size: ${FontSizes.size12};
`;

export const CompanyCode = styled.div`
    font-size: ${FontSizes.size12};
`;

export const DateRow = styled.div`
    height: 22.5%;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
`;
export const UnitValueRow = styled.div`
    height: 37.5%;
    padding-top: 0.5%;
    display: flex;
    flex-direction: row;
    align-items: top;
`;

export const UnitValue: any = styled.div`
    font-size: 26px;
    font-weight: ${(props: any) => (!props.isRead ? FontWeights.semibold : FontWeights.regular)};
    padding-top: 0.5%;
    padding-bottom: 0.5%;
    margin-right: 5px;
    max-width: 70%;
`;

export const UnitofMeasure: any = styled.div`
    font-size: ${FontSizes.size14};
    padding-top: 3%;
    max-width: 30%;
`;

export const Title = styled.div`
    height: 15%;
    font-weight: ${(props: any) => (!props.isRead ? FontWeights.semibold : FontWeights.regular)};
    padding-Bottom: 0.5%;
`;

export const SecondaryTitleContainer = styled.div`
    height: 10%;
    font-weight: ${(props: any) => (!props.isRead ? FontWeights.regular : FontWeights.semilight)};
    font-size: ${FontSizes.size12};
    vertical-align: text-top;
`;

export const DisplayDocumentNumber: any = styled.div`
    height: 15%;
    font-size: ${FontSizes.size14};
    font-weight: ${(props: any) => (!props.isRead ? FontWeights.semibold : FontWeights.regular)};
`;

export const Desc = styled.div`
    color: ${NeutralColors.gray200};
    font-size: ${FontSizes.size18};
    line-height: 1.5;
    padding-right: 16px;
`;

export const Header = styled.div`
    padding-top: 2%;
    padding-bottom: 0%;
    flex-direction: row;
    display: flex;
    align-items: center;
    width: 100%;
`;

export const StrongHeaderTitle = styled.strong`
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
`;

export const HeaderTitleContainer: any = styled.div`
    align-items: center;
    display: flex;
    flex-direction: row;
    justify-content: flex-start;
    width: 85%;
    font-weight: ${(props: any) => (!props.isRead ? 'bold' : 'normal')};
`;

export const HeaderTenantIcon = styled.div`
    padding-right: 6px;
    padding-top: 6px;
`;

export const HeaderIcons = styled.div`
    align-items: center;
    display: flex;
    flex-direction: row;
    justify-content: flex-end;
    width: 15%;
`;

export const MailReadIcon = styled.div`
    width: 18px;
    height: 18px;
    background-image: url('/icons/read-blk.png');
    background-size: 18px 18px;

    @media screen and (forced-colors: active){
        background-image: url('/icons/read-wht.png');
    }
`;

export const MailUnreadIcon = styled.div`
    width: 18px;
    height: 18px;
    background-image: url('/icons/unread-blk.png');
    background-size: 18px 18px;

    @media screen and (forced-colors: active){
        background-image: url('/icons/unread-wht.png');
    }
`;

export const Styles = mergeStyleSets({
    text: {
        fontWeight: 'bold',
        textOverflow: 'ellipsis',
        width: '100%',
        whiteSpace: 'nowrap',
        overflow: 'hidden'
    }
});

export const failedIconStyle = {
    color: '#A80000',
    height: '18px',
    width: '18px'
};

export const emptyFailedIconStyle = {
    height: '18px',
    width: '18px'
};

export const Footer = styled.div`
    padding-top: 2%;
    margin-bottom: 2%;
    flex-direction: row;
    display: flex;
    align-items: center;
    width: 100%;
    border-top: 1px solid rgba(0, 0, 0, 0.1);
`;

export const Spacer = styled.div`
    align-items: center;
    display: flex;
    flex-direction: row;
    justify-content: flex-start;
    width: 15%;
    padding-left: 16px;
`;

export const AppName = styled.div`
    align-items: center;
    display: flex;
    flex-direction: row;
    justify-content: flex-end;
    width: 85%;
    color: #646464;
    padding-top: 1%;
    padding-right: 16px;
`;

// Originally from Card.ts
export const CardContainer = styled.div<any>`
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    margin-left: ${props => (props.isTableView ? 'calc(2vw - 8px)' : 'calc(3vw)')};
    margin-right: calc(3vw);
    margin-bottom: ${props => (props.selectedApprovalRecords?.length > 0 ? 'calc(45vw)' : 'calc(0vw)')};

    ${minWidth.xl} {
        margin-left: ${props => (props.isTableView ? 'calc(2vw - 8px)' : 'calc(4vw - 32px)')};
        margin-right: calc(4vw - 32px);
    }
`;

export const Card: any = styled.div`
    background-color: ${SurfaceColors.primary};
    border-radius: 2px;
    box-shadow: ${Depths.depth4};
    margin-bottom: 16px;
    margin-left: 16px;
    margin-right: 16px;
    padding-right: 20px;
    width: 100%;
    height: 20%;
    transition: box-shadow 0.3s;
    &:hover {
        box-shadow: ${Depths.depth64};
    }
    max-width: 300px;
    min-width: 250px;
    cursor: ${(props: any) => (props.role === 'button' ? 'pointer' : 'default')};
    box-shadow: ${(props: any) => (props.isSelected ? Depths.depth64 : 'none')};

    ${minWidth.xl} {
        margin-bottom: 24px;
        margin-left: 12px;
        margin-right: 12px;
        padding-right: 0px !important;
    }

    border-left: ${(props: any) =>
        props.lastFailed
            ? '5px solid rgb(168, 0, 0)'
            : props.isRead
            ? '5px solid rgb(255, 255, 255)'
            : '5px solid rgb(0, 120, 212)'};
    height: ${(props: any) => (props.footer ? '150px' : '190px')};
`;

export const CardBody = styled.div`
    height: 75%;
    cursor: ${(props: any) => (props.role === 'button' ? 'pointer' : 'default')};
`;

export const CardHeader = styled.div`
    display: flex;
    flex-direction: row;
    height: 25%;
    justify-content: space-between;
    padding-left: 12px;
    padding-right: 12px;
`;
