import * as React from 'react';
import * as Styled from './DocumentPreviewStyling';
import * as SharedStyled from '../../SharedLayout';
import { Stack } from '@fluentui/react/lib/Stack';
import BasicDropdown from '../../../../Controls/BasicDropdown';
import { IconButton } from '@fluentui/react/lib/Button';
import ErrorResult from '../../Components/ErrorResult';

const DocumentPreview = (props: any): JSX.Element => {
    const {
        documentPreview,
        documentPreviewHasError,
        dropdownOnChange,
        dropdownSelectedKey,
        dropdownOptions,
        previewContainerInitialWidth,
        footerHeight,
        handleDownloadClick,
        toggleDetailsScreen,
        isModal,
        isModalExpanded,
        previewContainerInitialHeight
    } = props;
    const type = documentPreview?.[0];
    const isImage = type === '/' || type === 'i' || type === 'R';
    const [rotation, setRotation] = React.useState(0);
    const [width, setWidth] = React.useState(0);
    const [height, setHeight] = React.useState(0);
    const [isFitToWindow, setIsFitToWidow] = React.useState(false);
    const [containerWidth, setContainerWidth] = React.useState(previewContainerInitialWidth - 50);
    const [containerHeight, setContainerHeight] = React.useState(window.innerHeight * 0.65);

    const rotationAlt = rotation != 0 ? ` rotated ${rotation} degrees` : '';

    function handleGetDimensions(e: object): void {
        const { naturalWidth, naturalHeight } = (e as any).target;
        setWidth(naturalWidth);
        setHeight(naturalHeight);
    }

    function handleRotate(): void {
        if (rotation === 270) {
            setRotation(0);
        } else {
            const newRotation = rotation + 90;
            setRotation(newRotation);
        }
    }

    React.useEffect(() => {
        let newWidth;
        let newHeight;
        if (!isModal) {
            const offset = toggleDetailsScreen ? 0.8 : 0.82;
            let bodyelements = document.getElementsByClassName('custom-details-container');

            if (!bodyelements || bodyelements.length === 0) {
                bodyelements = document.getElementsByClassName('ms-Panel-scrollableContent');
            }

            if (bodyelements[0]) {
                const el = bodyelements[0];
                newWidth = el.clientWidth - 50;
                newHeight = el.clientHeight * offset;
                setContainerWidth(newWidth);
                setContainerHeight(newHeight);
            }
        } else {
            const offset = 0.85;
            newWidth = previewContainerInitialWidth;
            newHeight = previewContainerInitialHeight * offset;
            setContainerWidth(newWidth);
            setContainerHeight(newHeight);
        }
    }, [
        toggleDetailsScreen,
        footerHeight,
        isModal,
        isModalExpanded,
        previewContainerInitialWidth,
        previewContainerInitialHeight
    ]);

    function calcContainerStyle(): React.CSSProperties {
        let previewMaxSize = containerWidth;
        let previewMaxHeight = containerHeight;

        if (!isModal) {
            const offset = toggleDetailsScreen ? 0.8 : 0.82;
            let bodyelements = document.getElementsByClassName('custom-details-container');

            if (!bodyelements || bodyelements.length === 0) {
                bodyelements = document.getElementsByClassName('ms-Panel-scrollableContent');
            }

            if (bodyelements[0]) {
                const el = bodyelements[0];
                previewMaxSize = el.clientWidth - 50;
                previewMaxHeight = el.clientHeight * offset;
            }
        }

        const style: React.CSSProperties = {
            overflow: `auto`,
            height: `${previewMaxHeight}px`,
            maxWidth: `${previewMaxSize}px`,
            margin: 'auto'
        };
        switch (rotation) {
            case 90:
                if (width > previewMaxHeight) {
                    style.height = `${previewMaxHeight}px`;
                }
                return style;
            case 270:
                if (width > previewMaxSize) {
                    style.height = `${previewMaxSize}px`;
                }

                if (height > previewMaxHeight) {
                    style.width = `${previewMaxHeight}px`;
                }
                return style;
            default:
                return style;
        }
    }

    function calcImgStyle(): React.CSSProperties {
        const style: React.CSSProperties = {
            display: `block`,
            transformOrigin: `top left`
        };
        if (isFitToWindow) {
            style.height = 'auto';
            style.width = 'auto';
            style.maxHeight = '100%';
            style.maxWidth = '100%';
        }
        switch (rotation) {
            case 90:
                style.transform = `rotate(${rotation}deg) translateY(-100%)`;
                return style;
            case 180:
                style.transform = `rotate(${rotation}deg) translate(-100%, -100%)`;
                return style;
            case 270:
                style.transform = `rotate(${rotation}deg) translateX(-100%)`;
                return style;
            default:
                return style;
        }
    }

    const renderPreviewElement = (documentPreview: string): JSX.Element => {
        const getExtensionName = (typeChar: string): string => {
            switch (typeChar) {
                case '/':
                    return 'jpg';
                case 'i':
                    return 'png';
                case 'R':
                    return 'gif';
                case 'V':
                    return 'txt';
                case 'J':
                    return 'pdf';
            }
        };
        const extensionName = getExtensionName(type);
        if (isImage) {
            return (
                <div className="custom-scrollbar" style={calcContainerStyle()}>
                    <img
                        style={calcImgStyle()}
                        src={`data:image/${extensionName};base64,${documentPreview}`}
                        alt={'Image preview' + rotationAlt}
                        onLoad={handleGetDimensions}
                    />
                </div>
            );
        } else if (extensionName === 'txt') {
            return (
                <iframe
                    src={`data:text/plain;base64,${documentPreview}`}
                    name="Text file preview"
                    title="Text file preview"
                />
            );
        } else if (extensionName === 'pdf') {
            return (
                <iframe
                    src={`data:application/${extensionName};base64,${documentPreview}`}
                    name="PDF file preview"
                    title="PDF file preview"
                    height={containerHeight}
                />
            );
        } else {
            return (
                <div style={{ padding: '5%' }}>
                    <ErrorResult message="This file type is not supported for preview, please download the file for viewing." />
                </div>
            );
        }
    };

    return (
        <Stack tokens={{ childrenGap: 3 }}>
            <Stack horizontal tokens={Styled.DocumentPreviewStackTokens}>
                <Stack.Item>
                    <BasicDropdown
                        options={dropdownOptions}
                        selectedKey={dropdownSelectedKey}
                        onChange={dropdownOnChange}
                        styles={SharedStyled.SmallDropdownStyles}
                        label="Select file to preview"
                        componentRef={(input: { focus: () => any }) => {
                            input && input.focus();
                        }}
                    />
                </Stack.Item>
                <Stack.Item align="auto" tokens={{ padding: '30px 0px 0px' }}>
                    <IconButton
                        iconProps={{ iconName: 'Download' }}
                        title="Download file"
                        ariaLabel="Download file"
                        onClick={handleDownloadClick}
                    />
                </Stack.Item>
                <Stack.Item align="auto" tokens={{ padding: '30px 0px 0px' }}>
                    {isImage && (
                        <IconButton
                            iconProps={{ iconName: 'Rotate' }}
                            title="Rotate 90 degrees"
                            ariaLabel="Rotate 90 degrees"
                            onClick={handleRotate}
                        />
                    )}
                </Stack.Item>
                <Stack.Item align="auto" tokens={{ padding: '30px 0px 0px' }}>
                    {isImage && (
                        <IconButton
                            iconProps={isFitToWindow ? { iconName: 'FitWidth' } : { iconName: 'FitPage' }}
                            title={isFitToWindow ? 'Original size' : 'Fit to window'}
                            ariaLabel={isFitToWindow ? 'Show full size image' : 'Fit image to window'}
                            onClick={(): void => {
                                setIsFitToWidow(!isFitToWindow);
                            }}
                        />
                    )}
                </Stack.Item>
            </Stack>
            {!documentPreviewHasError && renderPreviewElement(documentPreview)}
        </Stack>
    );
};

export default DocumentPreview;
