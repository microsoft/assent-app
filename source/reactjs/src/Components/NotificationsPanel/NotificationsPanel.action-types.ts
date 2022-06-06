import { INotificationPanelListItem } from './NotificationsPanel.types';

export enum NotificationsPanelActionType {
    REQUEST_ALERTS_INFO = 'REQUEST_ALERTS_INFO',
    RECEIVE_ALERTS_INFO = 'RECEIVE_ALERTS_INFO',
    RESET_ALERTS_INFO = 'RESET_ALERTS_INFO',
    POST_ALERTS_INFO = 'POST_ALERTS_INFO',
    TOGGLE_NOTIFICATION_PANEL = 'TOGGLE_NOTIFICATION_PANEL',
    SET_UNREAD_NOTIFICATIONS = 'SET_UNREAD_NOTIFICATIONS',
}

export type NotificationsPanelAction =
    | IRequestAlertsInfo
    | IReceiveAlertsInfo
    | IResetAlertsInfo
    | IPostAlertsInfo
    | IToggleNotificationPanel
    | IClearUnreadNotifications;

export interface IRequestAlertsInfo {
    type: NotificationsPanelActionType.REQUEST_ALERTS_INFO;
}

export interface IReceiveAlertsInfo {
    type: NotificationsPanelActionType.RECEIVE_ALERTS_INFO;
    itemsUnread: INotificationPanelListItem[];
    itemsRead: INotificationPanelListItem[];
    unReadNotifications: Array<string>;
}

export interface IResetAlertsInfo {
    type: NotificationsPanelActionType.RESET_ALERTS_INFO;
}

export interface IPostAlertsInfo {
    type: NotificationsPanelActionType.POST_ALERTS_INFO;
    unReadNotifications: Array<string>;
}

export interface IToggleNotificationPanel {
    type: NotificationsPanelActionType.TOGGLE_NOTIFICATION_PANEL
}

export interface IClearUnreadNotifications {
    type: NotificationsPanelActionType.SET_UNREAD_NOTIFICATIONS
}