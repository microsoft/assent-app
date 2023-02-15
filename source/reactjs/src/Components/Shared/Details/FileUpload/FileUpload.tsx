import * as React from 'react';
import { ChangeEvent, useEffect, useRef, useState } from 'react';
import {
    IColumn,
    Stack,
    DefaultButton,
    DetailsList,
    SelectionMode,
    DetailsListLayoutMode,
    ConstrainMode,
    Text,
    IconButton,
    FontIcon,
    DetailsRow,
    IDetailsRowStyles,
    IDetailsRowProps,
    IRenderFunction,
    MessageBar,
    MessageBarType,
} from '@fluentui/react';
import { getFileTypeIconProps } from '@fluentui/react-file-type-icons';
import { controlStyles, deleteIcon, mainStack, buttonStack } from './FileUpload.styles';
import { FileAttachment } from './FileAttachment';

/**
 * Represents a file to be uploaded.
 */
export interface IFileUpload {
    clientRowKey: string;
    name: string;
    fileSize: number;
    base64Content: string;
    attachmentAlreadyExists: boolean;
}

/**
 * Max files to upload at once limit.
 */
export const maxFilesToUploadAtOnceLimit = 10;

/**
 * File upload options.
 */
export interface IFileUploadOptions {
    /**
     * Allowed file types.
     * Comma separated, ex: ".pdf,.pptx,.doc,.docx,.xls,.xlsx,.csv,.txt,.jpg,.jpeg,.png,.msg,.json,.zip"
     */
    allowedFileTypes: string;

    /**
     * Max file size in bytes for individual files.
     */
    maxFileSizeInBytes: number;

    /**
     * Max user attachments to allow. This does not include pre-attachments in the approval request.
     * If undefined or null - then there is no max.
     */
    maxAttachments: number | undefined | null;

    /**
     * Indicates max files that can be uploaded at once.
     * This is the current attachment count subtracted from the max attachments.
     * If that value is > maxFilesToUploadAtOnceLimit or if maxAttachments is undefined, then this will
     * be set to maxFilesToUploadAtOnceLimit (an arbitrary number, just to limit users from uploading a massive
     * number of files in one go).
     */
    maxFilesToUploadAtOnce: number;

    /**
     * Current file attachments.
     */
    currentFileAttachments: FileAttachment[];
}

/**
 * Properties passed to the FileUpload component.
 */
export interface IFileUploadProps {
    /**
     * File upload options.
     */
    fileUploadOptions: IFileUploadOptions;

    /**
     * Submit button clicked event callback.
     */
    submitButtonClicked: (files: IFileUpload[]) => void;
}

/**
 * Common column props used in the DetailsList.
 */
const commonColumnProps: IColumn = {
    key: '',
    name: '',
    minWidth: 0,
    isRowHeader: false,
    isResizable: true,
    isPadded: true,
    isCollapsible: false,
    isMultiline: false,
};

/**
 * File upload component.
 * @param props File upload component properties.
 * @returns JSX element.
 */
export const FileUpload: React.FunctionComponent<IFileUploadProps> = (props: IFileUploadProps): JSX.Element => {
    const [filesToUpload, setFilesToUpload] = useState<IFileUpload[]>([]);
    const [allowedFileTypes, setAllowedFileTypes] = useState<string[]>([]);
    const [warnEmptyFilesSelected, setWarnEmptyFilesSelected] = useState<string[]>([]);
    const [warnLargeFilesSelected, setWarnLargeFilesSelected] = useState<string[]>([]);
    const [warnUnsupportedFilesSelected, setWarnUnsupportedFilesSelected] = useState<string[]>([]);
    const [warnMaxFilesToUploadAtOnceReached, setWarnMaxFilesToUploadAtOnceReached] = useState<boolean>();
    const fileInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        const arr: string[] = [];
        let allowedFileTypes: string = props.fileUploadOptions.allowedFileTypes; // ex: .pdf,.pptx,.doc,.docx
        if (allowedFileTypes) {
            allowedFileTypes = allowedFileTypes.replace(/\./g, ''); // Remove all . from file types (.docx becomes docx).
            arr.push(...allowedFileTypes.split(','));
            for (let i = 0; i < arr.length; i++) {
                arr[i] = arr[i].trim();
            }
        }
        setAllowedFileTypes(arr);
    }, [props.fileUploadOptions.allowedFileTypes]);

    /**
     * Choose file button clicked event handler.
     */
    const chooseFileButtonClicked = (): void => {
        // Invoke click event on hidden input type file control.
        fileInputRef.current?.click();
    };

    /**
     * Generate a random 4 character id, like 06d7.
     * @returns Random id.
     */
    const generateRandomId = (): string => {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    };

    /**
     * Convert file to base64.
     * @param file File object.
     * @returns Base64 string content for the file.
     */
    const toBase64 = (file: File) => {
        return new Promise<string>((resolve, reject) => {
            const reader: FileReader = new FileReader();
            reader.readAsDataURL(file);
            // The split is to remove the Data-URL declaration portion. ex: data:image/png;base64,iVBORw...
            reader.onload = () => resolve((reader.result as string).split(',')[1]);
            reader.onerror = (error) => reject(error);
        });
    };

    /**
     * File input change event handler.
     * @param event Change event.
     */
    const fileInputChangeHandler = async (event: ChangeEvent<HTMLInputElement>): Promise<void> => {
        const emptyFilesSelected: string[] = [];
        setWarnEmptyFilesSelected(emptyFilesSelected);
        const largeFilesSelected: string[] = [];
        setWarnLargeFilesSelected(largeFilesSelected);
        const unsupportedFilesSelected: string[] = [];
        setWarnUnsupportedFilesSelected(unsupportedFilesSelected);
        setWarnMaxFilesToUploadAtOnceReached(false);

        const newFilesToUpload: IFileUpload[] = [...filesToUpload];
        const files: FileList | null = event.target.files;
        if (files) {
            for (let i = 0; i < files.length; i++) {
                const file: File | null = files.item(i);
                if (
                    file /* File is not undefined. */ &&
                    newFilesToUpload.filter((f) => f.name === file.name).length === 0 /* Not already in the list. */
                ) {
                    if (file.size === 0) {
                        // Check for empty files.
                        emptyFilesSelected.push(file.name);
                        setWarnEmptyFilesSelected(emptyFilesSelected);
                    } else if (file.size > props.fileUploadOptions.maxFileSizeInBytes) {
                        // Check for files too large.
                        largeFilesSelected.push(file.name);
                        setWarnLargeFilesSelected(largeFilesSelected);
                    } else if (newFilesToUpload.length + 1 > props.fileUploadOptions.maxFilesToUploadAtOnce) {
                        // Check if max files to upload at once reached.
                        setWarnMaxFilesToUploadAtOnceReached(true);
                    } else {
                        // Get the extension from the file.
                        const nameParts: string[] = file.name.split('.');
                        const ext: string = nameParts[nameParts.length - 1]; // Extension is what comes after the final . (some files may have more than one . in the name).

                        // Check if it is a supported extension.
                        if (allowedFileTypes.indexOf(ext) > -1) {
                            const fileUpload: IFileUpload = {
                                clientRowKey: generateRandomId(),
                                name: file.name,
                                fileSize: file.size,
                                base64Content: await toBase64(file),
                                attachmentAlreadyExists:
                                    props.fileUploadOptions.currentFileAttachments.find(
                                        (x) => x.name === file.name && x.isPreAttached === true
                                    ) !== undefined,
                            };
                            newFilesToUpload.push(fileUpload);
                        } else {
                            unsupportedFilesSelected.push(file.name);
                            setWarnUnsupportedFilesSelected(unsupportedFilesSelected);
                        }
                    }
                }
            }
        }
        setFilesToUpload(newFilesToUpload);

        // Set the value of the input element to blank. This fixes an issue where if a user selects a file, then
        // removes that file, then selects that file again... the value of the control does not change so this
        // fileInputChangeHandler event does not fire.
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
    };

    /**
     * Remove file button clicked event handler.
     * @param fileUpload File upload.
     */
    const removeFileButtonClicked = (fileUpload: IFileUpload): void => {
        const index: number = filesToUpload.indexOf(fileUpload);
        if (index > -1) {
            const newFilesToUpload: IFileUpload[] = [...filesToUpload];
            newFilesToUpload.splice(index, 1);
            setFilesToUpload(newFilesToUpload);
        }
    };

    /**
     * Submit button clicked event handler.
     */
    const submitButtonClicked = (): void => {
        // Raise submitButtonClicked event so caller can handle this event.
        props.submitButtonClicked(filesToUpload);
    };

    /**
     * Returns file extension for the file name.
     * @param fileName File name.
     * @returns File extension.
     */
    const getFileExt = (fileName: string): string => {
        const parts: string[] = fileName.split('.');
        if (parts.length > 1) {
            return parts[1];
        }
        return '';
    };

    /**
     * Convert number to KB, with 2 digit floating precision.
     * @param n Number to convert.
     * @returns Number in KB.
     */
    const convertToKb = (n: number): number => {
        return Number((n / Math.pow(1024, 1)).toFixed(2));
    };

    /**
     * Columns to be displayed in the DetailsList.
     */
    const fileUploadColumns: IColumn[] = [
        {
            ...commonColumnProps,
            isRowHeader: true,
            key: 'column1',
            name: 'File',
            fieldName: 'name',
            minWidth: 80,
            onRender: (item: IFileUpload): JSX.Element => {
                return (
                    <>
                        <FontIcon
                            {...getFileTypeIconProps({
                                extension: getFileExt(item.name),
                                size: 24,
                                imageFileType: 'svg',
                            })}
                            className={controlStyles.fileIcon}
                        />
                        <Text className={controlStyles.fileListText}>{item.name}</Text>
                        {item.attachmentAlreadyExists && (
                            <Text className={controlStyles.fileListTextAlreadyExists}>
                                File already attached and will be overwritten
                            </Text>
                        )}
                    </>
                );
            },
        },
        {
            ...commonColumnProps,
            key: 'column2',
            name: 'Size (KB)',
            fieldName: 'size',
            minWidth: 50,
            onRender: (item: IFileUpload): JSX.Element => {
                return (
                    <div className={controlStyles.fileListTextContainer}>
                        <Text className={controlStyles.fileListText}>
                            {convertToKb(item.fileSize).toLocaleString()}
                        </Text>
                    </div>
                );
            },
        },
        {
            ...commonColumnProps,
            key: 'column3',
            minWidth: 26,
            onRender: (item: IFileUpload): JSX.Element => {
                return (
                    <IconButton
                        className={controlStyles.fileListText}
                        iconProps={deleteIcon}
                        ariaLabel={`Remove ${item.name}`}
                        onClick={(): void => removeFileButtonClicked(item)}
                    />
                );
            },
        },
    ];

    /**
     * Custom row rendering for details list. Disables the hover effect on each row which is not
     * desirable especially when SelectionMode is none.
     * See: https://github.com/microsoft/fluentui/issues/8783
     * @param detailsRowProps Details row props. Allows undefined because IRenderFunction requires it (but this will return empty element if this is the case).
     * @returns JSX for details row.
     */
    const onRenderRow: IRenderFunction<IDetailsRowProps> = (
        detailsRowProps: IDetailsRowProps | undefined
    ): JSX.Element => {
        if (detailsRowProps) {
            const rowStyles: Partial<IDetailsRowStyles> = {
                root: {
                    selectors: {
                        ':hover': {
                            background: 'transparent',
                        },
                    },
                },
            };

            return <DetailsRow {...detailsRowProps} styles={rowStyles} />;
        }
        return <></>;
    };

    return (
        <Stack horizontalAlign="start" verticalFill={true} tokens={buttonStack} className={controlStyles.mainStack}>
            <Stack.Item>
                <Text>Select files to upload as attachments to this approval request.</Text>
                <br />
                <ul className={controlStyles.ul}>
                    <li>
                        <Text>Supported file types: {allowedFileTypes.join(', ')}</Text>
                        <br />
                    </li>
                    <li>
                        <Text>
                            Max file size: {convertToKb(props.fileUploadOptions.maxFileSizeInBytes).toLocaleString()} KB
                        </Text>
                    </li>
                    {props.fileUploadOptions.maxAttachments && (
                        <li>
                            <Text>
                                Max attachments to this approval request: {props.fileUploadOptions.maxAttachments}
                            </Text>
                        </li>
                    )}
                    <li>
                        <Text>
                            Max files allowed to upload at once: {props.fileUploadOptions.maxFilesToUploadAtOnce}
                        </Text>
                    </li>
                </ul>
            </Stack.Item>
            <Stack.Item>
                <DefaultButton
                    text="Choose file(s)"
                    disabled={filesToUpload.length >= props.fileUploadOptions.maxFilesToUploadAtOnce}
                    onClick={chooseFileButtonClicked}
                    aria-label="Choose one or more files to upload"
                />
                <input
                    style={{ display: 'none' }}
                    ref={fileInputRef}
                    type="file"
                    name="file"
                    onChange={fileInputChangeHandler}
                    multiple
                    accept={props.fileUploadOptions.allowedFileTypes}
                />
            </Stack.Item>
            <Stack.Item verticalFill={true} align="stretch">
                <DetailsList
                    className={controlStyles.fileUploadDetailsListStyle}
                    ariaLabelForGrid="Files to upload"
                    items={filesToUpload}
                    compact={true}
                    columns={fileUploadColumns}
                    selectionMode={SelectionMode.none}
                    onRenderRow={onRenderRow}
                    getKey={(item: IFileUpload): string => item.clientRowKey}
                    layoutMode={DetailsListLayoutMode.justified}
                    isHeaderVisible={true}
                    constrainMode={ConstrainMode.horizontalConstrained}
                />
            </Stack.Item>
            {warnEmptyFilesSelected.length > 0 && (
                <Stack.Item>
                    <MessageBar messageBarType={MessageBarType.warning} isMultiline={false}>
                        <Text>
                            You selected files that are empty, which is not allowed: {warnEmptyFilesSelected.join(', ')}
                        </Text>
                    </MessageBar>
                </Stack.Item>
            )}
            {warnLargeFilesSelected.length > 0 && (
                <Stack.Item>
                    <MessageBar messageBarType={MessageBarType.warning} isMultiline={false}>
                        <Text>You selected files that are too large: {warnLargeFilesSelected.join(', ')}</Text>
                    </MessageBar>
                </Stack.Item>
            )}
            {warnUnsupportedFilesSelected.length > 0 && (
                <Stack.Item>
                    <MessageBar messageBarType={MessageBarType.warning} isMultiline={false}>
                        <Text>You selected files that are unsupported: {warnUnsupportedFilesSelected.join(', ')}</Text>
                    </MessageBar>
                </Stack.Item>
            )}
            {warnMaxFilesToUploadAtOnceReached && (
                <Stack.Item>
                    <MessageBar messageBarType={MessageBarType.warning} isMultiline={false}>
                        <Text>Max files to upload at once is reached.</Text>
                    </MessageBar>
                </Stack.Item>
            )}
            <Stack.Item>
                <Stack horizontal tokens={mainStack}>
                    <DefaultButton
                        text={
                            'Submit' +
                            (filesToUpload.length > 0
                                ? ` ${filesToUpload.length} file${filesToUpload.length > 1 ? 's' : ''}`
                                : '')
                        }
                        disabled={filesToUpload.length === 0}
                        onClick={submitButtonClicked}
                        aria-label="Submit selected files"
                    />
                    <Text className={controlStyles.cannotDeleteNote}>
                        Once files are uploaded, they can not be deleted.
                    </Text>
                </Stack>
            </Stack.Item>
        </Stack>
    );
};
