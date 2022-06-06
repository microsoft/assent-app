import { FAQListItemsInitialState, FAQReducerName } from "./FAQ.reducer"
import { IFAQListItemsState } from "./FAQ.types"


export const getFAQListItems = (state: IFAQListItemsState): object => {
    const faqStateData = {
        text: state.dynamic?.[FAQReducerName]?.text || FAQListItemsInitialState.text,
        title: state.dynamic?.[FAQReducerName]?.title || FAQListItemsInitialState.title,
        videoUrl: state.dynamic?.[FAQReducerName]?.videoUrl || FAQListItemsInitialState.videoUrl,
        videoWidth: state.dynamic?.[FAQReducerName]?.videoWidth || FAQListItemsInitialState.videoWidth,
        videoHeight: state.dynamic?.[FAQReducerName]?.videoHeight || FAQListItemsInitialState.videoHeight,
        textAsHeader: state.dynamic?.[FAQReducerName]?.textAsHeader || FAQListItemsInitialState.textAsHeader,
        isExpanded: state.dynamic?.[FAQReducerName]?.isExpanded || FAQListItemsInitialState.isExpanded,
        fullScreen: state.dynamic?.[FAQReducerName]?.fullScreen || FAQListItemsInitialState.fullScreen,
    }
    return faqStateData;
}