import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { helpPanelReducerName } from './HelpPanel.reducer';

export interface IHelpPanelState extends IDefaultState {
    dynamic?: {
        [helpPanelReducerName]: IHelpPanelState;
    };
}

export interface IHelpPanelState {
    supportEmailId: string | null;
    isLoading: boolean;
    hasError: boolean;
    isHelpPanelOpen: boolean;
}
