import { ISummaryCardsModel } from './Models/ISummaryCardsModel';

export type SearchPreviewClickHandler = (
    tenantId: string,
    documentNumber: string,
    displayDocumentNumber: string,
    attachmentId: string,
    attachmentName: string
) => void;

export interface ISummaryCardProps {
    cardInfo: ISummaryCardsModel;
    cardRef: string;
    showApprovalButton: boolean;
    showViewDetailsButton: boolean;
    selectedDocmentNumber: any;
    selectForBulkApproval: boolean;
    allBulkCheckSelected: boolean;
    isCardAvailableForBulk: boolean;
    onSearchPreviewClick?: SearchPreviewClickHandler;
}
