import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { FAQReducerName } from './FAQ.reducer';

export interface IFAQListItemsState extends IDefaultState {
    dynamic?: {
        [FAQReducerName]: IFAQListItemsState;
    };
}

export interface IFAQListItemsState {
    title: string;
    text: string;
    videoUrl: string;
    videoWidth: string;
    videoHeight: string;
    textAsHeader: string;
    isExpanded: boolean;
    fullScreen: boolean;
    isLoading: boolean,
    hasError: boolean
}