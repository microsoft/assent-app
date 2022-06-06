export enum HelpPanelActionType {
    REQUEST_ABOUT_INFO = 'REQUEST_ABOUT_INFO',
    RECEIVE_ABOUT_INFO = 'RECEIVE_ABOUT_INFO',
    UPDATE_HELP_PANEL_STATE = 'UPDATE_HELP_PANEL_STATE',
}

export type HelpPanelAction = IRequestAboutInfo | IReceiveAboutInfo | IUpdateHelpPanelState;

export interface IRequestAboutInfo {
    type: HelpPanelActionType.REQUEST_ABOUT_INFO;
}

export interface IReceiveAboutInfo {
    type: HelpPanelActionType.RECEIVE_ABOUT_INFO;
    supportEmailId: string;
}

export interface IUpdateHelpPanelState {
    type: HelpPanelActionType.UPDATE_HELP_PANEL_STATE;
    isHelpPanelOpen: boolean;
}
