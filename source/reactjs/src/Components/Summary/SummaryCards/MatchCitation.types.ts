import { IAttachmentMatch } from '../../Shared/SharedComponents.types';
export { IAttachmentMatch };

export interface IMatchCitationProps {
    attachmentMatches?: IAttachmentMatch[];
    hasFieldMatch?: boolean;
    onPreviewClick: (attachmentId: string | null, attachmentName: string) => void;
    onOpenRequest?: () => void;
    /** Document number of the request this citation belongs to. Used for telemetry. */
    documentNumber?: string;
}
