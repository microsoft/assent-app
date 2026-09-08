import * as React from 'react';
import { Modal, Stack, IconButton, Spinner, IIconProps, IModalStyles } from '@fluentui/react';
import { ContextualMenu } from '@fluentui/react/lib/ContextualMenu';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { trackBusinessProcessEvent, trackException, TrackingEventId } from '../../../../Helpers/telemetryHelpers';
import { setHeader } from '../../Components/SagasHelper';
import { guid } from '../../../../Helpers/Guid';
import { getModalDimensions } from '../DetailsStyling';
import {
    DocumentPreviewMode,
    ISelfFetchProps,
    IPreFetchedProps,
    IDocumentPreviewModalProps,
} from './DocumentPreviewModal.types';

import {
    arrayBufferToBase64,
    getImageMimeType,
    getModalSize as getModalSizeBase,
    detectPreviewContentType,
    buildPreviewUrl,
} from './DocumentPreviewModal.helpers';

export { DocumentPreviewMode } from './DocumentPreviewModal.types';

const cancelIcon: IIconProps = { iconName: 'Cancel' };
const fullScreenIcon: IIconProps = { iconName: 'FullScreen' };
const restoreIcon: IIconProps = { iconName: 'BackToWindow' };
const downloadIcon: IIconProps = { iconName: 'Download' };

const getModalSize = (isExpanded: boolean) =>
    getModalSizeBase(isExpanded, window.innerWidth, window.innerHeight);

function DocumentPreviewModalBase(props: IDocumentPreviewModalProps): React.ReactElement {
    const { isOpen, onDismiss, attachmentName } = props;
    const isProvidedMode = props.mode === DocumentPreviewMode.Provided;

    const { httpClient, telemetryClient, authClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );

    // Self-fetch state (only used when mode !== 'provided')
    const [fetchedPreview, setFetchedPreview] = React.useState<string | null>(null);
    const [fetchedContentType, setFetchedContentType] = React.useState<string | null>(null);
    const [isFetchLoading, setIsFetchLoading] = React.useState(false);
    const [fetchHasError, setFetchHasError] = React.useState(false);
    const [fetchErrorMessage, setFetchErrorMessage] = React.useState<string | null>(null);
    const [isExpanded, setIsExpanded] = React.useState(isProvidedMode);

    // Reset expanded state when modal reopens in provided mode
    React.useEffect(() => {
        if (isOpen && isProvidedMode) {
            setIsExpanded(true);
        }
    }, [isOpen, isProvidedMode]);

    const [modalDims, setModalDims] = React.useState(() => getModalSize(isExpanded));

    React.useEffect(() => {
        if (isProvidedMode) return;
        setModalDims(getModalSize(isExpanded));
        let resizeTimer: number | null = null;
        const handleResize = () => {
            if (resizeTimer != null) window.clearTimeout(resizeTimer);
            resizeTimer = window.setTimeout(() => {
                setModalDims(getModalSize(isExpanded));
                resizeTimer = null;
            }, 100);
        };
        window.addEventListener('resize', handleResize);
        return () => {
            window.removeEventListener('resize', handleResize);
            if (resizeTimer != null) window.clearTimeout(resizeTimer);
        };
    }, [isExpanded, isProvidedMode]);

    const fetchProps = !isProvidedMode ? (props as ISelfFetchProps) : null;
    const providedProps = isProvidedMode ? (props as IPreFetchedProps) : null;

    React.useEffect(() => {
        if (isProvidedMode || !isOpen) return;
        if (!fetchProps?.attachmentId || !fetchProps?.tenantId || !fetchProps?.documentNumber) return;

        let cancelled = false;
        setIsFetchLoading(true);
        setFetchHasError(false);
        setFetchedPreview(null);

        const fetchPreview = async () => {
            const tcv = guid();
            const xcv = fetchProps.displayDocumentNumber || fetchProps.documentNumber || tcv;
            try {
                const url = buildPreviewUrl(
                    __API_BASE_URL__,
                    __API_URL_ROOT__,
                    fetchProps.tenantId,
                    fetchProps.documentNumber,
                    fetchProps.attachmentId,
                    fetchProps.displayDocumentNumber
                );
                const { data, headers } = await httpClient.request({
                    url,
                    resource: __RESOURCE_URL__,
                    responseType: 'arraybuffer',
                    headers: setHeader(fetchProps.userAlias || '', tcv, xcv),
                });
                if (!cancelled) {
                    setFetchedPreview(arrayBufferToBase64(data as ArrayBuffer));
                    const ct = headers?.['content-type'] ?? headers?.['Content-Type'] ?? null;
                    setFetchedContentType(typeof ct === 'string' ? ct.split(';')[0].trim() : null);
                    setIsFetchLoading(false);
                    trackBusinessProcessEvent(
                        authClient,
                        telemetryClient,
                        'DeepSearch - Citation preview opened',
                        'MSApprovals.DeepSearch.CitationPreview.Open',
                        TrackingEventId.DeepSearchPreviewOpen,
                        {},
                        {
                            AttachmentId: fetchProps.attachmentId,
                            AttachmentName: attachmentName,
                            DocumentNumber: fetchProps.documentNumber,
                        }
                    );
                }
            } catch (err: any) {
                if (!cancelled) {
                    setFetchHasError(true);
                    setFetchErrorMessage(err?.message || 'Unable to load attachment preview');
                    setIsFetchLoading(false);
                    trackException(
                        authClient,
                        telemetryClient,
                        'DeepSearch - Citation preview error',
                        'MSApprovals.DeepSearch.CitationPreview.Error',
                        TrackingEventId.DeepSearchPreviewError,
                        {},
                        err instanceof Error ? err : new Error(err?.message || 'Preview load failed')
                    );
                }
            }
        };

        fetchPreview();
        return () => {
            cancelled = true;
        };
    }, [
        isOpen,
        isProvidedMode,
        fetchProps?.attachmentId,
        fetchProps?.tenantId,
        fetchProps?.documentNumber,
        fetchProps?.displayDocumentNumber,
        fetchProps?.userAlias,
    ]);

    const handleDismiss = () => {
        if (!isProvidedMode) {
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'DeepSearch - Citation preview closed',
                'MSApprovals.DeepSearch.CitationPreview.Close',
                TrackingEventId.DeepSearchPreviewClose,
                {},
                { AttachmentName: attachmentName }
            );
        }
        setFetchedPreview(null);
        setIsExpanded(false);
        onDismiss();
    };

    const handleDownload = () => {
        if (!fetchedPreview || !attachmentName) return;
        const byteChars = atob(fetchedPreview);
        const byteArray = new Uint8Array(byteChars.length);
        for (let i = 0; i < byteChars.length; i++) {
            byteArray[i] = byteChars.charCodeAt(i);
        }
        const blob = new Blob([byteArray], { type: fetchedContentType || 'application/octet-stream' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = attachmentName;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }, 100);
    };

    const { width: modalWidth, height: modalHeight } = providedProps
        ? getModalDimensions(
              isExpanded,
              providedProps.windowWidth || window.innerWidth,
              providedProps.windowHeight || window.innerHeight
          )
        : modalDims;

    const modalStyles: Partial<IModalStyles> = isProvidedMode
        ? { main: { minWidth: modalWidth, minHeight: modalHeight } }
        : {
              main: { minWidth: modalWidth, minHeight: modalHeight, width: modalWidth, height: modalHeight },
              scrollableContent: { height: '100%', overflow: 'hidden' },
          };

    const isLoading = providedProps ? providedProps.isLoading : isFetchLoading;
    const hasError = providedProps ? providedProps.hasError : fetchHasError;
    const errorMsg = providedProps ? providedProps.errorMessage : fetchErrorMessage;

    const renderFetchContent = (): React.ReactNode => {
        if (!fetchedPreview) return null;

        const previewType = detectPreviewContentType(fetchedContentType, fetchedPreview[0]);

        if (previewType === 'image') {
            const mimeType = fetchedContentType?.toLowerCase() || getImageMimeType(fetchedPreview[0]);
            return (
                <img
                    src={`data:${mimeType};base64,${fetchedPreview}`}
                    alt={attachmentName || 'Attachment preview'}
                    style={{ maxWidth: '100%' }}
                />
            );
        }
        if (previewType === 'pdf') {
            return (
                <iframe
                    src={`data:application/pdf;base64,${fetchedPreview}`}
                    title="PDF preview"
                    style={{ width: '100%', height: '100%', border: 'none' }}
                />
            );
        }
        return (
            <Stack horizontalAlign="center" verticalAlign="center" styles={{ root: { padding: 40, height: '100%' } }}>
                <span style={{ fontSize: 14, color: '#605e5c' }}>
                    There was an issue previewing this file, please download the file for viewing.
                </span>
            </Stack>
        );
    };

    return (
        <Modal
            isOpen={isOpen}
            isBlocking={false}
            dragOptions={{ moveMenuItemText: 'Move', closeMenuItemText: 'Close', menu: ContextualMenu }}
            onDismiss={handleDismiss}
            styles={modalStyles}
            aria-label={`Document preview: ${attachmentName || 'attachment'}`}
        >
            <Stack verticalFill styles={{ root: { height: '100%' } }}>
                <Stack.Item align="end">
                    <Stack horizontal verticalAlign="center">
                        {!isProvidedMode && attachmentName && (
                            <Stack.Item
                                grow
                                styles={{
                                    root: {
                                        padding: '8px 12px 0 12px',
                                        fontSize: 14,
                                        fontWeight: 600,
                                        display: 'flex',
                                        alignItems: 'center',
                                    },
                                }}
                            >
                                {attachmentName}
                            </Stack.Item>
                        )}
                        {!isProvidedMode && fetchedPreview && (
                            <IconButton
                                iconProps={downloadIcon}
                                ariaLabel="Download file"
                                title="Download file"
                                onClick={handleDownload}
                                styles={{ icon: { fontSize: 18 } }}
                            />
                        )}
                        <IconButton
                            iconProps={isExpanded ? restoreIcon : fullScreenIcon}
                            ariaLabel={isExpanded ? 'Restore modal size' : 'Maximize modal'}
                            title={isExpanded ? 'Resize' : 'Maximize'}
                            onClick={() => setIsExpanded(!isExpanded)}
                            styles={{ icon: { fontSize: 18 } }}
                        />
                        <IconButton
                            iconProps={cancelIcon}
                            ariaLabel="Close preview"
                            title="Close"
                            onClick={handleDismiss}
                            styles={{ icon: { fontSize: 18 } }}
                        />
                    </Stack>
                </Stack.Item>
                <Stack.Item grow styles={{ root: { overflow: 'auto', position: 'relative' } }}>
                    {isLoading && <Spinner label="Loading preview..." styles={{ root: { padding: 40 } }} />}
                    {hasError && (
                        <div style={{ padding: 20 }}>
                            <span style={{ color: '#a80000' }}>{errorMsg || 'Unable to load preview'}</span>
                        </div>
                    )}
                    {!isLoading &&
                        !hasError &&
                        providedProps &&
                        isOpen &&
                        providedProps.renderContent(modalWidth, modalHeight, isExpanded)}
                    {!isLoading && !hasError && !isProvidedMode && renderFetchContent()}
                </Stack.Item>
            </Stack>
        </Modal>
    );
}

export const DocumentPreviewModal = React.memo(DocumentPreviewModalBase);
