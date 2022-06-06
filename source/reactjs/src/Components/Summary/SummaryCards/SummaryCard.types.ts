import { ISummaryCardsModel } from './Models/ISummaryCardsModel';
export interface ISummaryCardProps {
    cardInfo: ISummaryCardsModel;
    cardRef: string;
    showApprovalButton: boolean;
    showViewDetailsButton: boolean;
    selectedDocmentNumber: any;
    selectForBulkApproval: boolean;
    allBulkCheckSelected: boolean;
}
