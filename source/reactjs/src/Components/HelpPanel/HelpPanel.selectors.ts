import { helpPanelInitialState, helpPanelReducerName } from './HelpPanel.reducer';
import { IHelpPanelState } from './HelpPanel.types';

export const getSupportEmailId = (state: IHelpPanelState) => {
    return state.dynamic?.[helpPanelReducerName]?.supportEmailId || helpPanelInitialState.supportEmailId;
};

export const getIsHelpPanelOpen = (state: IHelpPanelState) => {
    return state.dynamic?.[helpPanelReducerName]?.isHelpPanelOpen || helpPanelInitialState.isHelpPanelOpen;
};
