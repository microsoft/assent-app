import { IHelpPanelState } from './HelpPanel.types';
import { HelpPanelAction, HelpPanelActionType } from './HelpPanel.action-types';

export const helpPanelReducerName = 'HelpPanelReducer';
export const helpPanelInitialState: IHelpPanelState = {
    supportEmailId: '',
    isLoading: false,
    hasError: false,
    isHelpPanelOpen: false,
};

export function helpPanelReducer(
    prev: IHelpPanelState = helpPanelInitialState,
    action: HelpPanelAction
): IHelpPanelState {
    switch (action.type) {
        case HelpPanelActionType.REQUEST_ABOUT_INFO:
            return {
                ...prev,
                isLoading: true,
                hasError: false,
            };
        case HelpPanelActionType.RECEIVE_ABOUT_INFO:
            return {
                ...prev,
                isLoading: false,
                hasError: false,
                supportEmailId: action.supportEmailId,
            };
        case HelpPanelActionType.UPDATE_HELP_PANEL_STATE:
            return {
                ...prev,
                isHelpPanelOpen: action.isHelpPanelOpen,
            };
        default:
            return prev;
    }
}
