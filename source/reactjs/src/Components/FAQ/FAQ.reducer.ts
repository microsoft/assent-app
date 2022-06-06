import { FAQAction, FAQActionType } from "./FAQ.action-types";
import { IFAQListItemsState } from "./FAQ.types";


export const FAQReducerName = 'FAQReducer';

export const FAQListItemsInitialState: IFAQListItemsState = {
    fullScreen: false,
    isExpanded: false,
    text: "",
    textAsHeader: "",
    title: "",
    videoHeight: "",
    videoUrl: "",
    videoWidth: "",
    isLoading: false,
    hasError: false
}

export const faqReducer = (prev: IFAQListItemsState, action: FAQAction): IFAQListItemsState => {
    switch (action.type) {
        case FAQActionType.REQUEST_FAQ_INFO:
            return {
                ...prev,
                isLoading: true,
                hasError: false
            };
        case FAQActionType.RECEIVE_FAQ_INFO:
            return {
                ...prev,
                isLoading: false,
                hasError: false,
                fullScreen: action.fullScreen,
                isExpanded: action.isExpanded,
                text: action.text,
                textAsHeader: action.textAsHeader,
                title: action.title,
                videoHeight: action.videoHeight,
                videoUrl: action.videoUrl,
                videoWidth: action.videoWidth
            };
        default:
            return prev;
    }

}