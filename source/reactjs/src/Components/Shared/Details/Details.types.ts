import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { detailsReducerName } from './Details.reducer';
import { ISharedComponentsPersistentState, ISharedComponentsState } from '../SharedComponents.types';
import { sharedComponentsReducerName } from '../SharedComponents.reducer';


export interface IDetailsAppState extends IDefaultState {
    dynamic?: {
        [detailsReducerName]: IDetailsState;
        [sharedComponentsReducerName]: ISharedComponentsState;
    };
    SharedComponentsPersistentReducer: ISharedComponentsPersistentState;
}

export enum DetailsType {
    AdaptiveCard,
    Microfrontend
}

export interface IDetailsState {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    isLoadingHeader: boolean;
    headerHasError: boolean;
    isLoadingDetails: boolean;
    isLoadingCallback: boolean;
    isLoadingCallbackHeader: boolean;
    detailsHasError: boolean;
    callbackHasError: boolean;
    headerErrorMessage: string | null;
    detailsErrorMessage: string | null;
    callbackErrorMessage: string | null;
    actionMessage: string | null;
    tenantId: string | null;
    documentNumber: string | null;
    displayDocumentNumber: string | null;
    fiscalYear: string | null;
    isProcessingAction: boolean;
    actionDetails: object;
    documentPreview: string | null;
    isPreviewOpen: boolean;
    postActionHasError: boolean;
    postActionErrorMessage: string | null;
    documentTypeId: string | null;
    businessProcessName: string | null;
    isLoadingPreview: boolean;
    headerDetailsJSON: any;
    headerTemplateJSON: any;
    isRequestFullyScrolled: boolean;
    shouldDetailReRender: boolean;
    detailsJSON: any;
    detailsTemplateJSON: any;
    callbackHeaderJSONs: any;
    callbackJSONs: any;
    showingDetails: boolean;
    selectedPage: string;
    tcv: string | null;
    stateCommonProperties: any;
    readRequests: string[];
    failedRequests: string[];
    panelWidth: number;
    isControlsAndComplianceRequired: boolean;
    documentDownload: boolean;
    docName: string;
    documentDownloadHasError: boolean;
    documentDownloadErrorMessage: string | null;
    isLoadingSummary: boolean;
    userName: string;
    userAlias: string;
    isLoadingUserImage: boolean;
    userImage: any;
    userImageHasError: boolean;
    userImageErrorMessage: string | null;
    documentPreviewHasError: boolean;
    documentPreviewErrorMessage: string | null;
    allDocumentsDownloadHasError: boolean;
    allDocumentsDownloadErrorMessage: string | null;
    isMicrofrontendOpen: boolean;
    additionalData: any;
    fetchedUserAlias: string;
    toggleHistoryDetailPanel: boolean;
    footerHeight: number;
    bulkFooterHeight: number;
    bulkMessagebarHeight: number;
    aliasMessagebarHeight: number;
    isRequestFullyRendered: boolean;
    isPullModelEnabled: boolean;
    detailsComponentType: DetailsType;
    areDetailsEditable: boolean;
    cdnURL: string | null;
    editDetailsErrorMessage: string | null;
    summaryJSON: object | null;
    summaryDataMapping: string | null;
    isModalPreviewOpen: boolean;
    isShowingSuccessStatus: boolean;
    tenantDetailUrl?: string;
    templateName?: string;
    toggleDetailsScreen?: boolean;
    isExternalTenantActionDetails?: boolean;
}

export interface IControlValidation {
    isMandatory: boolean;
    isValid: boolean;
    errorMessage: string;
    controlCode: string;
    expectedValue?: string;
}

export interface IAddditionalInformation {
    Code: string;
    Text: string;
    Type: string;
    Values: any;
    IsValueFromSummaryObject: boolean;
    IsMandatory: boolean;
}
