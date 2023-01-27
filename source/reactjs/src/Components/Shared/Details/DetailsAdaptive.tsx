import * as React from 'react';
import { Stack, IStackTokens } from '@fluentui/react/lib/Stack';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { detailsReducerName, detailsReducer, detailsInitialState } from './Details.reducer';
import { DetailsType, IDetailsAppState, IDetailsState } from './Details.types';
import { detailsSagas } from './Details.sagas';
import { Dispatch } from 'redux';
import {
    requestUserImage,
    requestMyDetails,
    requestCallbackDetails,
    updateMyRequest,
    postAction,
    requestDocument,
    clearDocumentPreview,
    closeDocumentPreview,
    requestDocumentPreview,
    requestAllDocuments,
    reinitializeDetails,
    openMicrofrontend,
    updateAdditionalData,
    requestHeader,
    requestFullyScrolled,
    closeMicrofrontend,
    setFooterHeight,
    requestFullyRendered,
    requestDocumentStart,
    requestDocumentEnd,
} from './Details.actions';
import {
    requestMySummary,
    toggleDetailsScreen,
    updatePanelState,
    updatePeoplePickerHasError,
    updatePeoplePickerSelection,
    requestPullTenantSummary,
} from '../SharedComponents.actions';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { connect } from 'react-redux';
import { Adaptive } from './Adaptive';
import * as Styled from './DetailsStyling';
import * as SharedStyled from '../SharedLayout';
import BasicButton from '../../../Controls/BasicButton';
import { IDropdownOption } from '@fluentui/react/lib/Dropdown';
import DocumentPreview from './DocumentPreview/DocumentPreview';
import SuccessView from './DetailsMessageBars/SuccessView';
import ErrorView from './DetailsMessageBars/ErrorView';
import WarningView from './DetailsMessageBars/WarningView';
import InfoView from './DetailsMessageBars/InfoView';
import Microfrontend from './Microfrontend';
import { Spinner } from '@fluentui/react/lib/Spinner';
import { trackBusinessProcessEvent, trackException, TrackingEventId } from '../../../Helpers/telemetryHelpers';
import { getStateCommonTelemetryProperties } from '../SharedComponents.selectors';
import { stockImage } from '../../../Helpers/stockImage';
import { flattenObject, isMobileResolution, validateCondition } from '../../../Helpers/sharedHelpers';
import { PeoplePicker } from '../../Shared/Components/PeoplePicker';
import { CloseButton } from './DetailsButtons/CloseButton';
import { RefreshButton } from './DetailsButtons/RefreshButton';
import { MaximizeButton } from './DetailsButtons/MaximizeButton';
import { BackButton } from './DetailsButtons/BackButton';
import { Checkbox, ICheckboxProps } from '@fluentui/react/lib/Checkbox';
import DetailsFooter from './DetailsFooter';
import { DetailsWrapper } from './DetailsWrapper';
import { getDetailsCommonPropertiesSelector } from './Details.selectors';
import { ContinueMessage } from '../Components/ContinueMessage';
import { CONTINUE_TIMEOUT } from '../SharedConstants';
import { Modal } from '@fluentui/react/lib/Modal';
import { ContextualMenu, IconButton } from '@fluentui/react';

type DetailsAdaptiveState = {
    actionCompleted: boolean;
    code: string;
    reasonCode: string;
    reasonText: string;
    displayNotesInput: boolean;
    justificationsList: null | object[];
    comments: string;
    isCommentMandatory: boolean;
    notesErrorMessage: string;
    charLimit: number | null;
    attachmentsArray: object[] | null;
    selectedAttachmentID: string | null;
    selectedAttachmentName: string | null;
    actionDetailsComments: null | object[];
    isScrollPositionReset: boolean;
    isCloseFocusSet: boolean;
    showPreviousErrors: boolean;
    receiptsCheck: boolean;
    corruptionCheck: boolean;
    displayReceiptError: boolean;
    displayCorruptionError: boolean;
    displayReceiptCheckbox: boolean;
    displayCorruptionCheckbox: boolean;
    scrollHeight: number;
    showContinue: boolean;
    isModalExpanded: boolean;
};

const WIP_MESSAGE = 'The details page for this application is currently in development.';
const ADAPTIVE_ERROR_NOT_IN_DICTIONARY = 'The given key was not present in the dictionary.';
const FLIGHTING_ERROR = 'Tenant/ User not flighted to the modern UI experience';

interface IDetailsAdaptiveDispatch {
    dispatchRequestUserImage(userAlias: string): void;
    dispatchRequestHeader(
        tenantId: string,
        documentNumber: string,
        userAlias: string,
        isPullModelEnabled: boolean,
        summaryJSON?: object,
        summaryDataMapping?: string
    ): void;
    dispatchRequestMyDetails(
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        userAlias: string,
        requiresTemplate: boolean,
        isPullModelEnabled: boolean
    ): void;
    dispatchRequestCallbackDetails(callBackURLs: string[], userAlias: string): void;
    dispatchUpdateMyRequest(
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        userAlias: string
    ): void;
    dispatchPostAction(submission: object, userAlias: string, isBulkAction?: boolean): void;
    dispatchRequestMySummary(userAlias: string): void;
    dispatchUpdatePanelState(isOpen: boolean): void;
    dispatchRequestDocument(
        url: string,
        actionId: string,
        attachmentName: string,
        userAlias: string,
        tenantId?: string,
        documentNumber?: string,
        displayDocumentNumber?: string,
        attachmentId?: string
    ): void;
    dispatchRequestDocumentPreview(
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        attachmentId: string,
        userAlias: string,
        isModal: boolean
    ): void;
    dispatchRequestAllDocuments(
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        attachmentArray: string,
        userAlias: string
    ): void;
    dispatchClearDocumentPreview(): void;
    dispatchCloseDocumentPreview(): void;
    dispatchReinitializeDetails(): void;
    dispatchOpenMicrofrontend(): void;
    dispatchCloseMicrofrontend(): void;
    dispatchUpdateAdditionalData(newAdditionalData: any): void;
    dispatchRequestFullyScrolled(toogle: boolean, shouldDetailReRender: boolean): void;
    disptachToggleDetailScreen(): void;
    dispatchSetFooterHeight(height: number): void;
    dispatchRequestFullyRendered(isRequestFullyRendered: boolean): void;
    dispatchUpdatePeoplePickerSelections(peoplePickerSelections: object[]): void;
    dispatchUpdatePeoplePickerHasError(peoplePickerHasError: boolean): void;
    dispatchRequestPullTenantSummary(tenantId: number, userAlias: string, filterCriteria: object): void;
}

interface IDetailsAdaptiveProps extends IDetailsState, IDetailsAdaptiveDispatch {
    componentContext: IComponentContext;
    reduxContext: IReduxContext;
    hideHeaderActionBar?: boolean;
    viewType: string;
    templateType?: string;
    windowWidth?: any;
    windowHeight: any;
    shouldDetailReRender: boolean;
    handleContainerScrolling(e: any): void;
    historyRef: any;
    locationRef: any;
}
class RequestView extends React.Component<IDetailsAdaptiveProps, DetailsAdaptiveState> {
    footerRef: any;
    detailFooterComponentRef: any;
    successTimeout?: any;
    primaryActionRef: any;
    justificationRef: any;

    constructor(props: IDetailsAdaptiveProps) {
        super(props);
        this.state = {
            actionCompleted: false,
            code: '',
            reasonCode: '',
            reasonText: '',
            displayNotesInput: false,
            justificationsList: null,
            comments: '',
            isCommentMandatory: false,
            notesErrorMessage: '',
            charLimit: null,
            attachmentsArray: null,
            selectedAttachmentID: null,
            selectedAttachmentName: null,
            actionDetailsComments: null,
            isScrollPositionReset: false,
            isCloseFocusSet: false,
            showPreviousErrors: true,
            receiptsCheck: false,
            corruptionCheck: false,
            displayCorruptionError: false,
            displayReceiptError: false,
            displayReceiptCheckbox: false,
            displayCorruptionCheckbox: false,
            scrollHeight: 0,
            showContinue: false,
            isModalExpanded: true,
        };
        this.primaryActionRef = null;
        this.justificationRef = null;

        this.footerRef = React.createRef();
        this.detailFooterComponentRef = React.createRef();
    }

    executeMicrofrontendActionRef: any = {};

    componentDidMount(): void {
        const { reduxContext, componentContext } = this.props;
        const { reducerRegistry, runSaga } = reduxContext;
        const { authClient, telemetryClient } = componentContext;

        //connecting details reducer and sagas to redux store
        if (!reducerRegistry.exists(detailsReducerName)) {
            reducerRegistry.registerDynamic(detailsReducerName, detailsReducer, false, false);
            runSaga(detailsSagas);
        }

        const { dispatchRequestMyDetails, dispatchRequestHeader, selectedPage } = this.props;
        const {
            tenantId,
            displayDocumentNumber,
            documentNumber,
            stateCommonProperties,
            detailsComponentType,
            isPullModelEnabled,
            summaryJSON,
            summaryDataMapping,
        } = this.props;

        if (tenantId && displayDocumentNumber) {
            dispatchRequestHeader(
                tenantId,
                displayDocumentNumber,
                selectedPage === 'history' ? '' : this.props.userAlias,
                isPullModelEnabled,
                summaryJSON,
                summaryDataMapping
            );
        }
        if (tenantId && displayDocumentNumber && this.props.templateType.toLowerCase() == 'all') {
            dispatchRequestMyDetails(
                tenantId,
                documentNumber,
                displayDocumentNumber,
                this.props.userAlias,
                detailsComponentType === DetailsType.AdaptiveCard,
                isPullModelEnabled
            );
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Open Details',
                'MSApprovals.GetDetails',
                TrackingEventId.DetailsLoadInitiate,
                stateCommonProperties
            );
        }
    }

    private afterActionCompletion(): void {
        this.setState({
            actionCompleted: true,
        });

        //closes panel after 3 seconds automatically after a successful action and refreshes summary data
        if (!this.props.postActionHasError) {
            const { historyRef, locationRef } = this.props;
            if (historyRef && locationRef) {
                if (locationRef.pathname.length > 1) {
                    historyRef.push('/');
                }
            }
            this.successTimeout = setTimeout(() => {
                this.setState({
                    actionCompleted: false,
                });
                this.props.dispatchUpdatePanelState(false);
                if (!this.props.isPullModelEnabled) {
                    this.props.dispatchRequestMySummary(this.props.userAlias);
                }
            }, 3000);
        }
    }

    componentDidUpdate(prevProps: IDetailsAdaptiveProps, prevState: DetailsAdaptiveState): void {
        if (this.footerRef && this.footerRef.clientHeight) {
            if (this.props.footerHeight != this.footerRef.clientHeight) {
                this.props.dispatchSetFooterHeight(this.footerRef.clientHeight);
            }
        }

        if (prevProps.isProcessingAction && !this.props.isProcessingAction) {
            this.afterActionCompletion();
        }
        if (!prevProps.isProcessingAction && this.props.isProcessingAction) {
            this.setState({
                showContinue: true,
            });
            this.props.dispatchSetFooterHeight(detailsInitialState.footerHeight);
        }
        if (
            this.props.documentNumber != '' &&
            this.props.tenantId != '' &&
            prevProps.displayDocumentNumber != this.props.displayDocumentNumber
        ) {
            if (this.successTimeout && this.props.isPullModelEnabled) {
                clearTimeout(this.successTimeout);
                this.successTimeout = null;
            }
            this.props.dispatchRequestHeader(
                this.props.tenantId,
                this.props.displayDocumentNumber,
                this.props.selectedPage === 'history' ? '' : this.props.userAlias,
                this.props.isPullModelEnabled,
                this.props.summaryJSON,
                this.props.summaryDataMapping
            );
            //clearing local state when user selects a new request
            this.setState({
                actionCompleted: false,
                showPreviousErrors: true,
                scrollHeight: 0,
                showContinue: false,
            });
            if (this.detailFooterComponentRef && this.detailFooterComponentRef.current) {
                this.detailFooterComponentRef.current.clearNotesPopupData();
            }
            //clearing microfrontend ref when a new request is selected
            this.executeMicrofrontendActionRef = {};
            //clearing peoplePicker data
            this.props.dispatchUpdatePeoplePickerSelections([]);
            this.props.dispatchUpdatePeoplePickerHasError(false);
        }
        if (
            this.props.documentNumber != '' &&
            this.props.tenantId != '' &&
            prevProps.displayDocumentNumber != this.props.displayDocumentNumber &&
            this.props.templateType.toLowerCase() == 'all'
        ) {
            this.props.dispatchRequestMyDetails(
                this.props.tenantId,
                this.props.documentNumber,
                this.props.displayDocumentNumber,
                this.props.userAlias,
                this.props.detailsComponentType === DetailsType.AdaptiveCard,
                this.props.isPullModelEnabled
            );
        }
        if (prevState.displayNotesInput != this.state.displayNotesInput) {
            if (this.state.displayNotesInput && this.justificationRef) {
                this.justificationRef.focus();
            } else if (!this.state.displayNotesInput && this.primaryActionRef) {
                this.primaryActionRef.focus();
            }
        }
        const { detailsJSON, callbackJSONs, headerDetailsJSON, isRequestFullyScrolled } = this.props;

        if (
            this.props.selectedPage === 'summary' &&
            !isRequestFullyScrolled &&
            this.props.isControlsAndComplianceRequired &&
            detailsJSON &&
            detailsJSON !== null &&
            headerDetailsJSON
        ) {
            const callBackURLs = detailsJSON.CallBackURLCollection;

            if (callBackURLs !== undefined && callBackURLs.length > 0) {
                if (callbackJSONs !== undefined && callbackJSONs !== null) {
                    setTimeout(() => {
                        this.enableButtonsOnScroll(prevState.scrollHeight);
                    }, 500);
                }
            } else {
                setTimeout(() => {
                    this.enableButtonsOnScroll(prevState.scrollHeight);
                }, 500);
            }
        }

        if (this.props.isMicrofrontendOpen && !this.state.isScrollPositionReset) {
            // reset scroll to top
            let bodyelements = document.getElementsByClassName('custom-details-container');

            if (!bodyelements || bodyelements.length === 0) {
                bodyelements = document.getElementsByClassName('ms-Panel-scrollableContent');
            }

            if (bodyelements[0]) {
                const el = bodyelements[0];

                if (el.scrollTop > 0) {
                    el.scrollTop = 0;

                    this.setState({ isScrollPositionReset: true });
                }
            }
        }

        if (!this.props.isMicrofrontendOpen && this.state.isScrollPositionReset) {
            this.setState({ isScrollPositionReset: false });
        }
    }

    componentWillUnmount(): void {
        this.props.dispatchReinitializeDetails();
        if (this.successTimeout && this.props.isPullModelEnabled) {
            clearTimeout(this.successTimeout);
            this.successTimeout = null;
        }
    }
    enableButtonsOnScroll(prevScrollHeight: number): void {
        let bodyelements = document.getElementsByClassName('custom-details-container');

        if (!bodyelements || bodyelements.length === 0) {
            bodyelements = document.getElementsByClassName('ms-Panel-scrollableContent');
        }

        if (bodyelements[0]) {
            const el = bodyelements[0];
            if (Math.round(el.scrollHeight) != prevScrollHeight) {
                this.setState({ scrollHeight: Math.round(el.scrollHeight) });
            } else if (
                !this.props.isRequestFullyRendered &&
                !this.props.isRequestFullyScrolled &&
                prevScrollHeight == el.scrollHeight
            ) {
                this.props.dispatchRequestFullyRendered(true);
                this.props.handleContainerScrolling({ target: el });
                if (el.scrollHeight === el.clientHeight) {
                    // in case no scrollbar appears
                    this.props.dispatchRequestFullyScrolled(true, false);
                }
            }
        }
    }
    replaceUserImage(): void {
        const headerTemplate = this.props.headerTemplateJSON;
        let jsonString = JSON.stringify(headerTemplate);

        if (!this.props.userImageHasError && this.props.userImage) {
            // replace with user image
            jsonString = jsonString.replace('#UserImage#', `data:image/png;base64,${this.props.userImage}`);
        }
        // image wasn't found
        else {
            // replace with stock image
            jsonString = jsonString.replace('#UserImage#', `data:image/png;base64,${stockImage}`);
        }

        return JSON.parse(jsonString);
    }
    combineDataPayload(): any {
        const { detailsJSON, callbackJSONs, callbackHasError } = this.props;

        const callBackURLs = detailsJSON.CallBackURLCollection;

        // if the callback payload is there, merge with the data payload
        if (callbackJSONs !== undefined && callbackJSONs !== null) {
            // merge the callback JSONs with the data payload
            let completeData = detailsJSON;
            for (let i = 0; i < this.props.callbackJSONs.length; i++) {
                completeData = {
                    ...completeData,
                    ...callbackJSONs[i],
                };
            }
            return completeData;
        }
        // if no callback call has been made yet, but needs to be
        else if (callBackURLs !== undefined && callBackURLs.length > 0 && !callbackHasError) {
            this.props.dispatchRequestCallbackDetails(callBackURLs, this.props.userAlias);
        }
        // if no need for callback call, set the complete payload to the details json
        else if (callBackURLs === undefined || callBackURLs.length === 0) {
            return detailsJSON;
        }
    }

    handleAdaptiveCardSubmitAction = (id: string, data: any): void => {
        const {
            dispatchRequestDocumentPreview,
            dispatchRequestAllDocuments,
            dispatchOpenMicrofrontend,
            tenantId,
            documentNumber,
            displayDocumentNumber,
            componentContext,
            stateCommonProperties,
            viewType,
            windowWidth,
        } = this.props;
        const { authClient, telemetryClient } = componentContext;
        const isModal = viewType === 'Docked' && !isMobileResolution(windowWidth);
        if (id.includes('preview')) {
            const attachmentID = data[0].ID;
            const attachmentName = data[0].Name;
            this.setState({
                attachmentsArray: data,
                selectedAttachmentID: attachmentID,
                selectedAttachmentName: attachmentName,
            });
            dispatchRequestDocumentPreview(
                tenantId,
                documentNumber,
                displayDocumentNumber,
                attachmentID,
                this.props.userAlias,
                isModal
            );
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'View file preview',
                'MSApprovals.GetFilePreview',
                TrackingEventId.PreviewAttachments,
                stateCommonProperties
            );
        } else if (id.includes('downloadAll')) {
            dispatchRequestAllDocuments(tenantId, documentNumber, displayDocumentNumber, data, this.props.userAlias);
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Download all attachments',
                'MSApprovals.GetAllAttachments',
                TrackingEventId.DownloadAllAttachments,
                stateCommonProperties
            );
        } else if (id.includes('viewMoreDetails')) {
            dispatchOpenMicrofrontend();
        }
    };

    handleErrorMessage = (errorMessage: string): JSX.Element => {
        const isWIP = errorMessage === ADAPTIVE_ERROR_NOT_IN_DICTIONARY || errorMessage === FLIGHTING_ERROR;
        return isWIP ? (
            <Stack.Item>
                <InfoView
                    infoTitle={'This page is a work in progress.'}
                    infoMessage={WIP_MESSAGE}
                    linkHref={this.getDeepLinkUrl()}
                    linkText={'View details in our classic website'}
                />
            </Stack.Item>
        ) : (
            <Stack.Item>
                <ErrorView errorMessage={errorMessage} failureType={'Loading details'} />
            </Stack.Item>
        );
    };

    handleAttachmentSelect = (e: object, selection: IDropdownOption): void => {
        const {
            tenantId,
            documentNumber,
            displayDocumentNumber,
            dispatchRequestDocumentPreview,
            viewType,
            windowWidth,
        } = this.props;
        const selectedID = selection.key as string;
        const selectedName = selection.text as string;
        this.setState({
            selectedAttachmentID: selectedID,
            selectedAttachmentName: selectedName,
        });
        const isModal = viewType === 'Docked' && !isMobileResolution(windowWidth);
        dispatchRequestDocumentPreview(
            tenantId,
            documentNumber,
            displayDocumentNumber,
            selectedID,
            this.props.userAlias,
            isModal
        );
    };

    handleActionResponse = (): JSX.Element => {
        const { postActionHasError, postActionErrorMessage, componentContext, stateCommonProperties } = this.props;
        const { telemetryClient, authClient } = componentContext;
        if (postActionHasError) {
            let errorMessage;
            try {
                const errorResponseObject = JSON.parse(postActionErrorMessage);
                errorMessage = errorResponseObject.ErrorMessage ?? postActionErrorMessage;
            } catch (ex: any) {
                errorMessage = postActionErrorMessage;
                const exception = ex?.message ? new Error(ex.message) : ex;
                trackException(
                    authClient,
                    telemetryClient,
                    'Parse action error response - Failure',
                    'MSApprovals.ParseActionErrorResponse.Failure',
                    TrackingEventId.ParseActionErrorResponseFailure,
                    stateCommonProperties,
                    exception
                );
            }
            return <ErrorView errorMessage={errorMessage} failureType={'Submit'} />;
        } else {
            return <SuccessView />;
        }
    };

    getDeepLinkUrl = (): string | null => {
        const templateUrl = this.props.tenantDetailUrl;
        const templateName = this.props.templateName;
        const { tenantId, displayDocumentNumber } = this.props;
        let deepUrl = '';
        try {
            deepUrl = templateUrl.replace('{0}', tenantId);
            deepUrl = deepUrl.replace('{1}', displayDocumentNumber);
            deepUrl = deepUrl.replace('{2}', '');
            deepUrl = deepUrl.replace('{3}', templateName);
            return __CLASSIC_WEB_URL__ + deepUrl;
        } catch (ex) {
            return null;
        }
    };

    renderBackButton = () => {
        return (
            <BackButton
                callbackOnBackButton={(): void => {
                    this.detailFooterComponentRef.current.clearNotesPopupData();
                    this.props.dispatchUpdatePeoplePickerSelections([]);
                    this.props.dispatchUpdatePeoplePickerHasError(false);
                }}
            />
        );
    };

    handleDownload = (): void => {
        const { dispatchRequestDocument, tenantId, documentNumber, displayDocumentNumber, userAlias } = this.props;
        const { selectedAttachmentID, selectedAttachmentName } = this.state;
        dispatchRequestDocument(
            '',
            'download',
            selectedAttachmentName,
            userAlias,
            tenantId,
            documentNumber,
            displayDocumentNumber,
            selectedAttachmentID
        );
    };

    renderFilePreview = (
        isModal?: boolean,
        modalWidth?: number,
        modalHeight?: number,
        isModalExpanded?: boolean
    ): JSX.Element => {
        const {
            documentPreview,
            panelWidth,
            documentPreviewHasError,
            documentPreviewErrorMessage,
            toggleDetailsScreen,
        } = this.props;
        const { attachmentsArray, selectedAttachmentID } = this.state;
        const attachmentOptions: IDropdownOption[] = attachmentsArray.map((attachment: any) => {
            return { key: attachment.ID, text: attachment.Name };
        });
        return (
            <Stack tokens={Styled.DetailsFilePreviewStackTokens}>
                <Stack.Item>
                    <DocumentPreview
                        documentPreview={documentPreview}
                        documentPreviewHasError={documentPreviewHasError}
                        dropdownOnChange={this.handleAttachmentSelect}
                        dropdownSelectedKey={selectedAttachmentID}
                        dropdownOptions={attachmentOptions}
                        previewContainerInitialWidth={isModal ? modalWidth : panelWidth} //provide height minus footer height
                        footerHeight={this.props.footerHeight}
                        handleDownloadClick={this.handleDownload}
                        toggleDetailsScreen={toggleDetailsScreen}
                        isModal={isModal}
                        isModalExpanded={isModalExpanded}
                        previewContainerInitialHeight={isModal ? modalHeight : null}
                    />
                </Stack.Item>
                {documentPreviewHasError && documentPreviewErrorMessage && (
                    <Stack.Item>
                        <ErrorView errorMessage={documentPreviewErrorMessage} failureType={'Document preview'} />
                    </Stack.Item>
                )}
            </Stack>
        );
    };

    detailCardHeaderActionBar = () => {
        return (
            <Stack.Item
                styles={{
                    ...Styled.DetailsDocPreviewHeaderBarStyles(this.props.viewType, this.props.selectedPage),
                    root: {
                        ...(Styled.DetailsDocPreviewHeaderBarStyles(this.props.viewType, this.props.selectedPage)
                            .root as any),
                        ...(Styled.StickyDetailsHeder.root as any),
                    },
                }}
            >
                {(this.props.isPreviewOpen || this.props.isMicrofrontendOpen) && this.renderBackButton()}
                {!(this.props.isPreviewOpen || this.props.isMicrofrontendOpen) && <div></div>}
                <Stack style={{ flexFlow: 'row' }}>
                    {this.props.documentDownload && this.props.detailsJSON && (
                        <Stack.Item>
                            <Spinner
                                label={`Downloading ${this.props.docName}`}
                                labelPosition="left"
                                style={{ margin: '5px' }}
                            />
                        </Stack.Item>
                    )}
                    {this.props.documentDownloadHasError &&
                        this.props.documentDownloadErrorMessage &&
                        this.props.detailsJSON && (
                            <Stack.Item styles={Styled.HeaderActionBarMessageStyle}>
                                <SharedStyled.ErrorText>Document download failed.</SharedStyled.ErrorText>
                            </Stack.Item>
                        )}
                    {this.props.allDocumentsDownloadHasError &&
                        this.props.allDocumentsDownloadErrorMessage &&
                        this.props.detailsJSON && (
                            <Stack.Item styles={Styled.HeaderActionBarMessageStyle}>
                                <SharedStyled.ErrorText>Download all failed.</SharedStyled.ErrorText>
                            </Stack.Item>
                        )}
                    <Stack.Item>
                        <MaximizeButton />
                    </Stack.Item>
                    {!(this.props.isMicrofrontendOpen || this.props.isPreviewOpen) && (
                        <Stack.Item>
                            <RefreshButton />
                        </Stack.Item>
                    )}
                    <Stack.Item>
                        <CloseButton />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
        );
    };

    public render(): React.ReactElement {
        const {
            isLoadingDetails,
            isLoadingHeader,
            isLoadingCallback,
            isLoadingCallbackHeader,
            isLoadingUserImage,
            headerDetailsJSON,
            headerTemplateJSON,
            detailsJSON,
            detailsTemplateJSON,
            tenantId,
            documentNumber,
            isProcessingAction,
            selectedPage,
            detailsHasError,
            detailsErrorMessage,
            headerErrorMessage,
            callbackHasError,
            callbackErrorMessage,
            isPreviewOpen,
            postActionHasError,
            isLoadingPreview,
            headerHasError,
            showingDetails,
            documentDownload,
            documentDownloadHasError,
            documentDownloadErrorMessage,
            userName,
            userAlias,
            allDocumentsDownloadHasError,
            allDocumentsDownloadErrorMessage,
            isMicrofrontendOpen,
            detailsComponentType,
            isModalPreviewOpen,
            windowWidth,
            windowHeight,
        } = this.props;

        const stackTokens: IStackTokens = { childrenGap: isPreviewOpen ? 5 : 12 };

        const { showPreviousErrors, isModalExpanded } = this.state;

        const actionDetails = detailsJSON?.Actions;

        const modalDimensions = Styled.getModalDimensions(isModalExpanded, windowWidth, windowHeight);

        return (
            <div>
                <Modal
                    titleAriaId={'documentPreviewModal'}
                    isOpen={isModalPreviewOpen}
                    isBlocking={false}
                    dragOptions={{ moveMenuItemText: 'Move', closeMenuItemText: 'Close', menu: ContextualMenu }}
                    onDismiss={(): void => {
                        this.props.dispatchCloseDocumentPreview();
                    }}
                    styles={{ main: { minWidth: modalDimensions.width, minHeight: modalDimensions.height } }}
                >
                    <Stack>
                        <Stack.Item align={'end'}>
                            <Stack horizontal>
                                <IconButton
                                    iconProps={
                                        isModalExpanded ? { iconName: 'BackToWindow' } : { iconName: 'FullScreen' }
                                    }
                                    ariaLabel={isModalExpanded ? 'Restore modal size' : 'Maximize modal'}
                                    title={isModalExpanded ? 'Resize' : 'Maximize'}
                                    onClick={(): void => {
                                        this.setState({ isModalExpanded: !isModalExpanded });
                                    }}
                                    styles={{ icon: { fontSize: 18 } }}
                                />
                                <IconButton
                                    iconProps={{ iconName: 'Cancel' }}
                                    ariaLabel="Close preview modal"
                                    title="Close"
                                    onClick={(): void => {
                                        this.props.dispatchCloseDocumentPreview();
                                    }}
                                    styles={{ icon: { fontSize: 18 } }}
                                />
                            </Stack>
                        </Stack.Item>
                        {isModalPreviewOpen && isLoadingPreview && (
                            <Stack.Item verticalFill={true}>
                                <Spinner label="Loading preview..." />
                            </Stack.Item>
                        )}
                        {isModalPreviewOpen &&
                            !isLoadingPreview &&
                            this.renderFilePreview(
                                true,
                                modalDimensions.width,
                                modalDimensions.height,
                                isModalExpanded
                            )}
                    </Stack>
                </Modal>
                <Stack tokens={stackTokens}>
                    {!this.props.hideHeaderActionBar && this.detailCardHeaderActionBar()}
                    {(!tenantId || !documentNumber) && (
                        <Stack.Item>
                            <h2>No request selected</h2>
                        </Stack.Item>
                    )}
                    {showPreviousErrors &&
                        detailsJSON &&
                        detailsJSON.LastFailedExceptionMessage &&
                        headerDetailsJSON &&
                        headerDetailsJSON.LastFailed &&
                        !isProcessingAction &&
                        (postActionHasError || !this.state.actionCompleted) &&
                        !isMicrofrontendOpen &&
                        !isPreviewOpen && (
                            <Stack.Item>
                                <WarningView
                                    warningTitle={'Reasons for previous action failures'}
                                    warningMessages={detailsJSON.LastFailedExceptionMessage}
                                    onDismiss={(): void => this.setState({ showPreviousErrors: false })}
                                    isCollapsible={true}
                                />
                            </Stack.Item>
                        )}
                    {this.state.actionCompleted &&
                        !isProcessingAction &&
                        !isLoadingDetails &&
                        this.handleActionResponse()}
                    {!headerHasError && isLoadingHeader && (
                        <Stack.Item>
                            <Spinner label="Loading summary..." />
                        </Stack.Item>
                    )}
                    {headerHasError &&
                        headerErrorMessage &&
                        headerErrorMessage !== FLIGHTING_ERROR && ( //prevents flighting message from being rendered twice
                            <Stack.Item>
                                <ErrorView errorMessage={headerErrorMessage} failureType={'Loading summary'} />
                            </Stack.Item>
                        )}
                    {!headerHasError &&
                        !isLoadingHeader &&
                        !isLoadingUserImage &&
                        headerDetailsJSON &&
                        (postActionHasError || !this.state.actionCompleted) &&
                        !isMicrofrontendOpen && (
                            <>
                                {!isPreviewOpen && (
                                    <>
                                        <Stack.Item
                                            styles={
                                                !this.props.hideHeaderActionBar
                                                    ? Styled.DetailStackStyles
                                                    : Styled.DetailCardBackgroundColor
                                            }
                                        >
                                            <Stack>
                                                <Stack.Item>
                                                    <Adaptive
                                                        template={this.replaceUserImage()}
                                                        dataPayload={headerDetailsJSON}
                                                        onOpenURLActionExecuted={this.props.dispatchRequestDocument}
                                                        onSubmitActionExecuted={this.handleAdaptiveCardSubmitAction}
                                                        userAlias={this.props.userAlias}
                                                        shouldDetailReRender={this.props.shouldDetailReRender}
                                                    />
                                                </Stack.Item>
                                            </Stack>
                                        </Stack.Item>
                                    </>
                                )}
                            </>
                        )}
                    {((detailsHasError && detailsErrorMessage) || (callbackHasError && callbackErrorMessage)) &&
                        (postActionHasError || !this.state.actionCompleted) &&
                        this.handleErrorMessage(callbackHasError ? callbackErrorMessage : detailsErrorMessage)}
                    {!detailsHasError &&
                        !callbackHasError &&
                        !isLoadingDetails &&
                        !isLoadingUserImage &&
                        !isLoadingCallbackHeader &&
                        headerTemplateJSON &&
                        headerDetailsJSON &&
                        (postActionHasError || !this.state.actionCompleted) &&
                        this.props.templateType == 'Summary' &&
                        !showingDetails && (
                            <Stack.Item align="center">
                                <BasicButton
                                    primary={true}
                                    text="Load More Details"
                                    title="Load More Details"
                                    onClick={() =>
                                        this.props.dispatchRequestMyDetails(
                                            this.props.tenantId,
                                            this.props.documentNumber,
                                            this.props.displayDocumentNumber,
                                            '',
                                            this.props.detailsComponentType === DetailsType.AdaptiveCard,
                                            this.props.isPullModelEnabled
                                        )
                                    }
                                />
                            </Stack.Item>
                        )}
                    {!detailsHasError &&
                        !callbackHasError &&
                        showingDetails &&
                        !isLoadingHeader &&
                        (isLoadingDetails || isLoadingCallback) && (
                            <div
                                ref={(input) => input && input.focus()}
                                role="loading details"
                                aria-label="loading details"
                                title="loading details"
                                tabIndex={0}
                                style={{ outline: 'none' }}
                            >
                                <Stack.Item>
                                    <Spinner label="Loading details..." />
                                </Stack.Item>
                            </div>
                        )}
                    {!detailsHasError &&
                        !callbackHasError &&
                        !headerHasError &&
                        !isLoadingHeader &&
                        !isLoadingDetails &&
                        !isLoadingCallback &&
                        !isLoadingUserImage &&
                        !isProcessingAction &&
                        (detailsTemplateJSON || detailsComponentType === DetailsType.Microfrontend) &&
                        detailsJSON &&
                        !isMicrofrontendOpen &&
                        (postActionHasError || !this.state.actionCompleted) && (
                            <>
                                {!isPreviewOpen && (
                                    <>
                                        <Stack.Item
                                            styles={
                                                !this.props.hideHeaderActionBar
                                                    ? {
                                                          ...Styled.DetailStackStyles,
                                                          root: {
                                                              ...(Styled.DetailStackStyles.root as any),
                                                              ...(Styled.StackAdditionDetails(this.props.footerHeight)
                                                                  .root as any),
                                                          },
                                                      }
                                                    : Styled.StackAdditionDetails(this.props.footerHeight)
                                            }
                                        >
                                            <DetailsWrapper
                                                detailsComponentType={this.props.detailsComponentType}
                                                cdnURL={this.props.cdnURL}
                                                detailsTemplateJSON={detailsTemplateJSON}
                                                combinedDetailsJSON={this.combineDataPayload()}
                                                onOpenURLActionExecuted={this.props.dispatchRequestDocument}
                                                onSubmitActionExecuted={this.handleAdaptiveCardSubmitAction}
                                                userAlias={this.props.userAlias}
                                                shouldDetailReRender={this.props.shouldDetailReRender}
                                                tenantId={tenantId}
                                                executeMicrofrontendActionRef={this.executeMicrofrontendActionRef}
                                                dispatchUpdateAdditionalData={this.props.dispatchUpdateAdditionalData}
                                                selectedPage={this.props.selectedPage}
                                            />
                                        </Stack.Item>
                                    </>
                                )}
                                {isPreviewOpen && isLoadingPreview && (
                                    <Stack.Item>
                                        <Spinner label="Loading preview..." />
                                    </Stack.Item>
                                )}
                                {isPreviewOpen && !isLoadingPreview && this.renderFilePreview()}
                            </>
                        )}
                    {isMicrofrontendOpen && (postActionHasError || !this.state.actionCompleted) && (
                        <>
                            {!isProcessingAction && (
                                <Stack.Item
                                    styles={{
                                        ...Styled.DetailStackStyles,
                                        root: {
                                            ...(Styled.DetailStackStyles.root as any),
                                            ...(Styled.StackAdditionDetails(this.props.footerHeight).root as any),
                                        },
                                    }}
                                >
                                    <Microfrontend
                                        selectedPage={this.props.selectedPage}
                                        tenantId={Number(tenantId)}
                                        detailsJSON={detailsJSON}
                                        executeMicrofrontendActionRef={this.executeMicrofrontendActionRef}
                                        dispatchUpdateAdditionalData={this.props.dispatchUpdateAdditionalData}
                                    />
                                </Stack.Item>
                            )}
                        </>
                    )}
                    {!isLoadingDetails && (isProcessingAction || this.state.actionCompleted) && (
                        <Stack.Item>
                            {isProcessingAction && (
                                <Spinner componentRef={(input: any) => input && input.focus()} label="Processing..." />
                            )}
                            {/* show continue message after successful completion to allow user to proceed */}
                            {!this.props.isPullModelEnabled && this.state.showContinue && !postActionHasError && (
                                <ContinueMessage
                                    isBulkAction={false}
                                    customLabel={
                                        this.state.actionCompleted ? 'Click continue to work on other requests' : null
                                    }
                                />
                            )}
                            <Styled.ExtraLargeSpace />
                        </Stack.Item>
                    )}
                </Stack>
                {(detailsHasError || callbackHasError) && !headerHasError && <Styled.LargeSpace />}
                {!isLoadingDetails &&
                    !isLoadingCallback &&
                    !isLoadingUserImage &&
                    !isLoadingHeader &&
                    !isLoadingCallbackHeader &&
                    !detailsHasError &&
                    !callbackHasError &&
                    !headerHasError &&
                    showingDetails &&
                    !(selectedPage === 'history') &&
                    !isProcessingAction &&
                    //actions should still display when the previous submit failed
                    (postActionHasError || !this.state.actionCompleted) &&
                    !isProcessingAction && (
                        <DetailsFooter
                            ref={this.detailFooterComponentRef}
                            windowWidth={this.props.windowWidth}
                            windowHeight={this.props.windowHeight}
                            actionDetails={actionDetails}
                            detailsJSON={detailsJSON}
                            headerDetailsJSON={headerDetailsJSON}
                            isMicrofrontendOpen={isMicrofrontendOpen}
                            isBulkApproval={false}
                            isControlsAndComplianceRequired={this.props.isControlsAndComplianceRequired}
                            isRequestFullyScrolled={this.props.isRequestFullyScrolled}
                            executeMicrofrontendActionRef={this.executeMicrofrontendActionRef}
                            setFooterRef={(element: any) => {
                                this.footerRef = element;

                                if (this.footerRef && this.footerRef.clientHeight) {
                                    if (this.props.footerHeight != this.footerRef.clientHeight) {
                                        this.props.dispatchSetFooterHeight(this.footerRef.clientHeight);
                                    }
                                }
                            }}
                            setActionComplete={(v: any) => this.setState({ actionCompleted: v })}
                            componentContext={this.props.componentContext}
                            stateCommonProperties={this.props.stateCommonProperties}
                            tenantId={this.props.tenantId}
                            documentNumber={this.props.documentNumber}
                            displayDocumentNumber={this.props.displayDocumentNumber}
                            fiscalYear={this.props.fiscalYear}
                            dispatchPostAction={this.props.dispatchPostAction}
                            documentTypeId={this.props.documentTypeId}
                            businessProcessName={this.props.businessProcessName}
                            userAlias={this.props.userAlias}
                            detailsComponentType={this.props.detailsComponentType}
                            isExternalTenantActionDetails={this.props.isExternalTenantActionDetails}
                        ></DetailsFooter>
                    )}
            </div>
        );
    }
}

const mapStateToProps = (state: IDetailsAppState): IDetailsState => {
    if (state.dynamic[detailsReducerName] && state.SharedComponentsPersistentReducer) {
        const dynamicState = state.dynamic;
        const { tenantId } = dynamicState[detailsReducerName];
        const { tenantInfo, selectedPage, toggleDetailsScreen, pullTenantSearchCriteria, pullTenantSearchSelection } =
            dynamicState.SharedComponentsReducer;
        const { userName, userAlias } = state.SharedComponentsPersistentReducer;
        const updatedState: any = { ...state.dynamic[detailsReducerName] };
        if (!tenantId || !tenantInfo) {
            return detailsInitialState;
        }
        const {
            businessProcessName,
            docTypeId,
            isControlsAndComplianceRequired,
            tenantDetailUrl,
            templateName,
            detailsComponentInfo,
            isPullModelEnabled,
            summaryDataMapping,
            isExternalTenantActionDetails,
        } = tenantInfo.filter((item: object) => (item as any).tenantId === Number(tenantId))[0];
        updatedState.businessProcessName = businessProcessName;
        updatedState.documentTypeId = docTypeId;
        updatedState.selectedPage = selectedPage;
        updatedState.toggleDetailsScreen = toggleDetailsScreen;
        updatedState.pullTenantSearchCriteria = pullTenantSearchCriteria;
        updatedState.pullTenantSearchSelection = pullTenantSearchSelection;
        updatedState.userName = userName;
        updatedState.userAlias = userAlias;
        updatedState.tenantDetailUrl = tenantDetailUrl;
        updatedState.templateName = templateName;
        updatedState.isControlsAndComplianceRequired = isControlsAndComplianceRequired;
        updatedState.detailsComponentType = detailsComponentInfo.detailsComponentType;
        updatedState.cdnURL = detailsComponentInfo.cdnUrl;
        updatedState.isPullModelEnabled = isPullModelEnabled;
        updatedState.summaryDataMapping = summaryDataMapping;
        updatedState.isExternalTenantActionDetails = isExternalTenantActionDetails;
        const stateCommonProperties = getDetailsCommonPropertiesSelector(state);
        updatedState.stateCommonProperties = stateCommonProperties;
        return updatedState;
    } else {
        return detailsInitialState;
    }
};

const mapDispatchToProps = (dispatch: Dispatch): IDetailsAdaptiveDispatch => ({
    dispatchRequestUserImage: (userAlias: string): void => {
        dispatch(requestUserImage(userAlias));
    },
    dispatchRequestHeader: (
        tenantId: string,
        documentNumber: string,
        userAlias: string,
        isPullModelEnabled: boolean,
        summaryJSON?: object,
        summaryDataMapping?: string
    ): void => {
        dispatch(
            requestHeader(tenantId, documentNumber, userAlias, isPullModelEnabled, summaryJSON, summaryDataMapping)
        );
    },
    dispatchRequestMyDetails: (
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        userAlias: string,
        requiresTemplate: boolean,
        isPullModelEnabled: boolean
    ): void => {
        dispatch(
            requestMyDetails(
                tenantId,
                documentNumber,
                displayDocumentNumber,
                userAlias,
                requiresTemplate,
                isPullModelEnabled
            )
        );
    },
    dispatchRequestCallbackDetails: (callBackURLs: string[], userAlias: string): void => {
        dispatch(requestCallbackDetails(callBackURLs, userAlias));
    },
    dispatchUpdateMyRequest: (tenantId: string, documentNumber: string, displayDocumentNumber: string): void => {
        dispatch(updateMyRequest(Number(tenantId), documentNumber, displayDocumentNumber));
    },
    dispatchPostAction: (
        submission: object,
        userAlias: string,
        isBulkAction?: boolean,
        isPullModelEnabled?: boolean
    ): void => {
        dispatch(postAction(submission, userAlias, isBulkAction, isPullModelEnabled));
    },
    dispatchRequestMySummary: (userAlias: string): void => {
        dispatch(requestMySummary(userAlias));
    },
    dispatchUpdatePanelState: (isOpen: boolean): void => {
        dispatch(updatePanelState(isOpen));
    },
    dispatchRequestDocument: (
        url: string,
        actionId: string,
        attachmentName: string,
        userAlias: string,
        tenantId?: string,
        documentNumber?: string,
        displayDocumentNumber?: string,
        attachmentId?: string
    ): void => {
        dispatch(requestDocumentStart(attachmentName.split(' ')[1] ?? attachmentName));
        dispatch(
            requestDocument(
                url,
                actionId,
                attachmentName,
                userAlias,
                tenantId,
                documentNumber,
                displayDocumentNumber,
                attachmentId
            )
        );
    },
    dispatchRequestDocumentPreview: (
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        attachmentId: string,
        userAlias: string,
        isModal: boolean
    ): void => {
        dispatch(
            requestDocumentPreview(tenantId, documentNumber, displayDocumentNumber, attachmentId, userAlias, isModal)
        );
    },
    dispatchRequestAllDocuments: (
        tenantId: string,
        documentNumber: string,
        displayDocumentNumber: string,
        attachmentArray: string,
        userAlias: string
    ): void => {
        dispatch(requestDocumentStart('All Attachments'));
        dispatch(requestAllDocuments(tenantId, documentNumber, displayDocumentNumber, attachmentArray, userAlias));
    },
    dispatchClearDocumentPreview: (): void => {
        dispatch(clearDocumentPreview());
    },
    dispatchCloseDocumentPreview: (): void => {
        dispatch(closeDocumentPreview());
    },
    dispatchReinitializeDetails: (): void => {
        dispatch(reinitializeDetails());
    },
    dispatchOpenMicrofrontend: (): void => {
        dispatch(openMicrofrontend());
    },
    dispatchCloseMicrofrontend: (): void => {
        dispatch(closeMicrofrontend());
    },
    dispatchUpdateAdditionalData: (newAdditionalData: any): void => {
        dispatch(updateAdditionalData(newAdditionalData));
    },
    dispatchRequestFullyScrolled: (toggle: boolean, shouldDetailReRender: boolean): void => {
        dispatch(requestFullyScrolled(toggle, shouldDetailReRender));
    },
    disptachToggleDetailScreen: (): void => {
        dispatch(toggleDetailsScreen());
    },
    dispatchSetFooterHeight: (height: number): void => {
        dispatch(setFooterHeight(height));
    },
    dispatchRequestFullyRendered: (isRequestFullyRendered: boolean): void => {
        dispatch(requestFullyRendered(isRequestFullyRendered));
    },
    dispatchUpdatePeoplePickerSelections: (peoplePickerSelections: object[]): void => {
        dispatch(updatePeoplePickerSelection(peoplePickerSelections));
    },
    dispatchUpdatePeoplePickerHasError: (peoplePickerHasError: boolean): void => {
        dispatch(updatePeoplePickerHasError(peoplePickerHasError));
    },
    dispatchRequestPullTenantSummary: (tenantId: number, userAlias: string, filterCriteria: object): void => {
        dispatch(requestPullTenantSummary(tenantId, userAlias, filterCriteria));
    },
});

const connected = withContext(connect(mapStateToProps, mapDispatchToProps)(RequestView));
export { connected as RequestView };
