/**
 * File attachment options.
 * This is the tenant specific options that is returned as part of the details api for the approval request.
 */
export class FileAttachmentOptions {
    public allowedFileTypes: string;
    public maxFileSizeInBytes: number;
    public maxAttachments: number;

    /**
     * Creates a FileAttachmentOptions.
     * This constructor will work with PascalCased or camelCased json.
     * @param json Loose untyped json data.
     */
    constructor(json: any) {
        this.allowedFileTypes = json.allowedFileTypes || json.AllowedFileTypes;
        this.maxFileSizeInBytes = json.maxFileSizeInBytes || json.MaxFileSizeInBytes;
        this.maxAttachments = json.maxAttachments || json.MaxAttachments;
    }
}
