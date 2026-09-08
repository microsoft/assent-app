import { detailsInitialState, detailsReducerName } from './Details.reducer';
import { IDetailsAppState } from './Details.types';
import { createSelector } from 'reselect';
import { getIsPullTenantSelected, getSelectedPage } from '../SharedComponents.selectors';
import { IComponentsAppState } from '../SharedComponents.types';

const HIDE_ON_HISTORY_IDS = new Set([
    'UploadPOEDocumentsHeader',
    'UploadPOEAttachmentsImage',
    'UploadPOEDocumentsRadioButtons',
    'DeliveryManagerPOEInstruction',
]);

const hideUploadNodesForHistory = (template: any): any => {
    if (!template) return template;
    const cloned = JSON.parse(JSON.stringify(template));
    const walk = (node: any): void => {
        if (!node || typeof node !== 'object') return;
        if (HIDE_ON_HISTORY_IDS.has(node.id)) node.isVisible = false;
        Object.values(node).forEach((v) => (Array.isArray(v) ? v.forEach(walk) : walk(v)));
    };
    walk(cloned);
    return cloned;
};

export const getReadRequests = (state: IDetailsAppState): string[] => {
    return state.dynamic?.[detailsReducerName]?.readRequests || detailsInitialState.readRequests;
};

export const getFailedRequests = (state: IDetailsAppState): string[] => {
    return state.dynamic?.[detailsReducerName]?.failedRequests || detailsInitialState.failedRequests;
};

export const getBulkFooterHeight = (state: IDetailsAppState): number => {
    return state.dynamic?.[detailsReducerName]?.bulkFooterHeight || detailsInitialState.bulkFooterHeight;
};

export const getBulkMessagebarHeight = (state: IDetailsAppState): number => {
    return state.dynamic?.[detailsReducerName]?.bulkMessagebarHeight || detailsInitialState.bulkMessagebarHeight;
};

export const getAliasMessagebarHeight = (state: IDetailsAppState): number => {
    return state.dynamic?.[detailsReducerName]?.aliasMessagebarHeight || detailsInitialState.aliasMessagebarHeight;
};

export const getTenantId = (state: IDetailsAppState): string => {
    return state.dynamic?.[detailsReducerName]?.tenantId || detailsInitialState.tenantId;
};

export const getAreDetailsEditable = (state: IDetailsAppState): boolean => {
    return state.dynamic?.[detailsReducerName]?.areDetailsEditable || detailsInitialState.areDetailsEditable;
};

export const getIsProcessingAction = (state: IDetailsAppState): boolean => {
    return state.dynamic?.[detailsReducerName]?.isProcessingAction || detailsInitialState.isProcessingAction;
};

export const getDocumentNumber = (state: IDetailsAppState): string => {
    return state.dynamic?.[detailsReducerName]?.documentNumber || detailsInitialState.documentNumber;
};

export const getDisplayDocumentNumber = (state: IDetailsAppState): string => {
    return state.dynamic?.[detailsReducerName]?.displayDocumentNumber || detailsInitialState.displayDocumentNumber;
};

export const getTcv = (state: IDetailsAppState): string => {
    return state.dynamic?.[detailsReducerName]?.tcv || detailsInitialState.tcv;
};

export const getSummaryJSON = (state: IDetailsAppState): object | null => {
    return state.dynamic?.[detailsReducerName]?.summaryJSON || detailsInitialState.summaryJSON;
};

export const getPostActionErrorMessage = (state: IDetailsAppState): string | null => {
    return state.dynamic?.[detailsReducerName]?.postActionErrorMessage || detailsInitialState.postActionErrorMessage;
};

export const getIsDisabled = (state: IComponentsAppState): boolean => {
    const isProcessing = getIsProcessingAction(state);
    const isPullTenantSelected = getIsPullTenantSelected(state);
    const isShowingSuccessStatus =
        state.dynamic?.[detailsReducerName]?.isShowingSuccessStatus || detailsInitialState.isShowingSuccessStatus;
    if (isPullTenantSelected) {
        return isProcessing;
    } else {
        return isProcessing || isShowingSuccessStatus;
    }
};

const getDisplayDocumentNumberForMemo = (_: any, displayDocumentNumber: string): string => displayDocumentNumber;

const getDocumentNumberForMemo = (_: any, documentNumber: string): string => documentNumber;

export const isRequestRead = createSelector(
    getReadRequests,
    getDisplayDocumentNumberForMemo,
    (readRequests, displayDocumentNumber) => {
        return readRequests.includes(displayDocumentNumber);
    }
);

export const isRequestCurrentlySelected = createSelector(
    getDocumentNumber,
    getDocumentNumberForMemo,
    (documentNumberInState, documentNumber) => {
        return documentNumberInState === documentNumber;
    }
);

const getDetailsCommonPropertiesMemo = (
    documentNumber: string,
    displayDocumentNumber: string,
    tenantId: string,
    tcv: string
): any => {
    return {
        MessageId: tcv,
        Xcv: displayDocumentNumber,
        DocumentNumber: documentNumber,
        DisplayDocumentNumber: displayDocumentNumber,
        TenantId: tenantId
    };
};

export const getDetailsCommonPropertiesSelector = createSelector(
    [getDocumentNumber, getDisplayDocumentNumber, getTenantId, getTcv],
    getDetailsCommonPropertiesMemo
);

export const getDetailsTemplateJSON = (state: IDetailsAppState): any => {
    return state.dynamic?.[detailsReducerName]?.detailsTemplateJSON ?? detailsInitialState.detailsTemplateJSON;
};

export const getProcessedDetailsTemplate = createSelector(
    [getDetailsTemplateJSON, getSelectedPage],
    (template, selectedPage) => (selectedPage === 'history' ? hideUploadNodesForHistory(template) : template)
);
