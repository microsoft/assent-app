import { notificationsPanelInitialState, notificationsPanelReducerName } from './NotificationsPanel.reducer';
import { INotificationsPanelState } from './NotificationsPanel.types';

export const getIsNotficationsPanelOpen = (state: INotificationsPanelState): boolean => {
    return (
        state.dynamic?.[notificationsPanelReducerName]?.isNotificationPanelOpen ||
        notificationsPanelInitialState.isNotificationPanelOpen
    );
};
