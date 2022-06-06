export enum FAQActionType {
    REQUEST_FAQ_INFO = 'REQUEST_FAQ_INFO',
    RECEIVE_FAQ_INFO = 'RECEIVE_FAQ_INFO',
}

export type FAQAction =
    | IRequestFAQInfo
    | IReceiveFAQInfo;

export interface IRequestFAQInfo {
    type: FAQActionType.REQUEST_FAQ_INFO;
}

export interface IReceiveFAQInfo {
    type: FAQActionType.RECEIVE_FAQ_INFO;
    title: string;
    text: string;
    videoUrl: string;
    videoWidth: string;
    videoHeight: string;
    textAsHeader: string;
    isExpanded: boolean;
    fullScreen: boolean;
}