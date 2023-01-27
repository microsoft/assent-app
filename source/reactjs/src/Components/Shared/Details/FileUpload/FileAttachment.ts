/**
 * File attachment.
 * This models attachment objects in the details Attachments array.
 */
export class FileAttachment {
    public name: string;
    public id: string;
    public url: string;
    public isPreAttached: boolean;

    /**
     * Creates a FileAttachment.
     * This constructor will work with PascalCased or camelCased json.
     * @param json Loose untyped json data.
     */
    constructor(json: any) {
        this.name = json.name || json.Name;
        this.id = json.id || json.ID; // Current objects in Attachments use ID (both letters capitalized)
        this.url = json.url || json.Url;
        this.isPreAttached = json.isPreAttached || json.IsPreAttached;
    }
}
