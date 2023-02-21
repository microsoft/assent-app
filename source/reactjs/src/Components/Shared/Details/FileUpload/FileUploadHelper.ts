import { FileAttachment } from './FileAttachment';
import { FileAttachmentOptions } from './FileAttachmentOptions';
import { maxFilesToUploadAtOnceLimit } from './FileUpload';
import { IFileUploadOptions } from './FileUpload';

/**
 * Converts from attachments from the details api in the Attachments property to array of FileAttachment objects.
 * @param attachmentsArray Array of attachments.
 * @returns Array of FileAttachment objects.
 */
export const createFileAttachmentArray = (attachmentsArray: any[]): FileAttachment[] | undefined => {
    if (!attachmentsArray) {
        return undefined;
    }
    const fileAttachments: FileAttachment[] = [];
    attachmentsArray.forEach((x) => {
        fileAttachments.push(new FileAttachment(x));
    });
    return fileAttachments;
};

/**
 * Prepares props for use with FileUpload.
 * @param fileAttachmentOptions File attachment options.
 * @param fileAttachments File attachments.
 * @returns Props to be used with FileUpload.
 */
export const propsForFileUpload = (
    fileAttachmentOptions: FileAttachmentOptions | undefined,
    fileAttachments: FileAttachment[] | undefined
): IFileUploadOptions => {
    // Init empty IFileUploadOptions.
    // If there are no fileAttachmentOptions then the adaptive card should not even display the upload button.
    const fileUploadOptions: IFileUploadOptions = {
        allowedFileTypes: '',
        maxFileSizeInBytes: 0,
        maxAttachments: 0,
        maxFilesToUploadAtOnce: 0,
        currentFileAttachments: [],
    };

    if (fileAttachmentOptions) {
        fileUploadOptions.allowedFileTypes = fileAttachmentOptions.allowedFileTypes;
        fileUploadOptions.maxFileSizeInBytes = fileAttachmentOptions.maxFileSizeInBytes;
        fileUploadOptions.maxAttachments = fileAttachmentOptions.maxAttachments;
        fileUploadOptions.currentFileAttachments = fileAttachments;

        if (fileAttachmentOptions.maxAttachments === undefined || fileAttachmentOptions.maxAttachments === null) {
            fileUploadOptions.maxFilesToUploadAtOnce = maxFilesToUploadAtOnceLimit; // See comments in IFileUploadOptions as to why maxFilesToUploadAtOnceLimit.
        } else {
            // Set maxFilesToUploadAtOnce to be fileAttachmentOptions.maxAttachments or a lesser value based on current attachment count.
            const currentUserAttachmentCount: number = fileAttachments
                ? fileAttachments.filter((x) => x.isPreAttached === false).length
                : 0;
            const attachmentDelta: number = fileAttachmentOptions.maxAttachments - currentUserAttachmentCount;
            if (attachmentDelta <= 0) {
                // Do not allow more attachments. The upload button in the card should be disabled in this case.
                fileUploadOptions.maxFilesToUploadAtOnce = 0;
            } else {
                if (attachmentDelta > maxFilesToUploadAtOnceLimit) {
                    // Even though more could be allowed, based on the max attachments.
                    // Set the limit to maxFilesToUploadAtOnceLimit just to prevent a massive number of files being uploaded
                    // at once.
                    fileUploadOptions.maxFilesToUploadAtOnce = maxFilesToUploadAtOnceLimit;
                } else {
                    fileUploadOptions.maxFilesToUploadAtOnce = attachmentDelta;
                }
            }
        }
    }

    return fileUploadOptions;
};
