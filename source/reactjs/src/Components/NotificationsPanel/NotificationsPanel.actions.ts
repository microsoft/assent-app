import {
    IRequestAlertsInfo,
    IReceiveAlertsInfo,
    IResetAlertsInfo,
    IPostAlertsInfo,
    IToggleNotificationPanel,
    IClearUnreadNotifications,
    NotificationsPanelActionType
} from './NotificationsPanel.action-types';
import { INotificationPanelListItem } from './NotificationsPanel.types';

export function requestAlertsInfo(): IRequestAlertsInfo {
    return {
        type: NotificationsPanelActionType.REQUEST_ALERTS_INFO
    };
}

export function receiveAlertsInfo(itemsUnread: INotificationPanelListItem[], itemsRead: INotificationPanelListItem[], unReadNotifications: Array<string>): IReceiveAlertsInfo {
    return {
        type: NotificationsPanelActionType.RECEIVE_ALERTS_INFO,
        itemsUnread,
        itemsRead,
        unReadNotifications
    };
}

export function resetAlertsInfo(): IResetAlertsInfo {
    return {
        type: NotificationsPanelActionType.RESET_ALERTS_INFO
    }
}

export function postAlertsInfo(unReadNotifications: Array<string>): IPostAlertsInfo {
    return {
        type: NotificationsPanelActionType.POST_ALERTS_INFO,
        unReadNotifications
    }
}

export function toggleNotificationPanel(): IToggleNotificationPanel {
    return {
        type: NotificationsPanelActionType.TOGGLE_NOTIFICATION_PANEL,

    }
}

export function clearUnreadNotifications(): IClearUnreadNotifications {
    return {
        type: NotificationsPanelActionType.SET_UNREAD_NOTIFICATIONS,
    }
}