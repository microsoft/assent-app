import {
    IRequestAboutInfo,
    IReceiveAboutInfo,
    HelpPanelActionType,
    IUpdateHelpPanelState,
} from './HelpPanel.action-types';

export function requestAboutInfo(): IRequestAboutInfo {
    return {
        type: HelpPanelActionType.REQUEST_ABOUT_INFO,
    };
}

export function receiveAboutInfo(supportEmailId: string): IReceiveAboutInfo {
    return {
        type: HelpPanelActionType.RECEIVE_ABOUT_INFO,
        supportEmailId,
    };
}

export function updateHelpPanelState(isHelpPanelOpen: boolean): IUpdateHelpPanelState {
    return {
        type: HelpPanelActionType.UPDATE_HELP_PANEL_STATE,
        isHelpPanelOpen,
    };
}
