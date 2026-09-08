import * as React from 'react';

export enum DocumentPreviewMode {
    Fetch = 'fetch',
    Provided = 'provided',
}

export interface IBaseModalProps {
    isOpen: boolean;
    onDismiss: () => void;
    attachmentName?: string;
}

export interface ISelfFetchProps extends IBaseModalProps {
    mode?: DocumentPreviewMode.Fetch;
    tenantId: string;
    documentNumber: string;
    displayDocumentNumber: string;
    attachmentId: string;
    userAlias: string;
}

export interface IPreFetchedProps extends IBaseModalProps {
    mode: DocumentPreviewMode.Provided;
    isLoading: boolean;
    hasError: boolean;
    errorMessage?: string;
    windowWidth?: number;
    windowHeight?: number;
    renderContent: (modalWidth: number, modalHeight: number, isExpanded: boolean) => React.ReactNode;
}

export type IDocumentPreviewModalProps = ISelfFetchProps | IPreFetchedProps;
