import { INotificationsPanelState } from './NotificationsPanel.types';
import { NotificationsPanelAction, NotificationsPanelActionType } from './NotificationsPanel.action-types';

export const notificationsPanelReducerName = 'NotificationsPanelReducer';
export const notificationsPanelInitialState: INotificationsPanelState = {
    itemsUnread: [],
    itemsRead: [],
    allNotifications: [],
    unReadNotifications: [],
    unReadNotificationsCount: 0,
    isNotificationPanelOpen: false,
    isLoading: false,
    hasError: false,
};

export function notificationsPanelReducer(
    prev: INotificationsPanelState = notificationsPanelInitialState,
    action: NotificationsPanelAction
): INotificationsPanelState {
    switch (action.type) {
        case NotificationsPanelActionType.REQUEST_ALERTS_INFO:
            return {
                ...prev,
                isLoading: true,
                hasError: false,
            };
        case NotificationsPanelActionType.RECEIVE_ALERTS_INFO:
            return {
                ...prev,
                isLoading: false,
                hasError: false,
                itemsUnread: action?.itemsUnread,
                itemsRead: action?.itemsRead,
                allNotifications: action?.itemsUnread.concat(action?.itemsRead),
                unReadNotifications: action?.unReadNotifications,
                unReadNotificationsCount: action?.itemsUnread?.length,
            };
        case NotificationsPanelActionType.POST_ALERTS_INFO:
            return {
                ...prev,
                isLoading: false,
                hasError: false,
                unReadNotificationsCount: 0,
            };
        case NotificationsPanelActionType.TOGGLE_NOTIFICATION_PANEL:
            return {
                ...prev,
                isNotificationPanelOpen: !prev.isNotificationPanelOpen,
                isLoading: false,
                hasError: false,
            };
        case NotificationsPanelActionType.SET_UNREAD_NOTIFICATIONS:
            return {
                ...prev,
                hasError: false,
                isLoading: false,
                unReadNotifications: [],
            };
        case NotificationsPanelActionType.RESET_ALERTS_INFO:
            return {
                ...prev,
                isLoading: false,
                hasError: true,
                itemsUnread: [],
                itemsRead: [],
                allNotifications: [],
                unReadNotifications: [],
                unReadNotificationsCount: 0,
            };
        default:
            return prev;
    }
}
