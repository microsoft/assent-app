import { IDefaultState } from '@micro-frontend-react/employee-experience/lib/IDefaultState';
import { notificationsPanelReducerName } from './NotificationsPanel.reducer';

export interface INotificationsPanelState extends IDefaultState {
    dynamic?: {
        [notificationsPanelReducerName]: INotificationsPanelState;
    };
}

export interface INotificationsPanelState {
    itemsUnread: INotificationPanelListItem[];
    itemsRead: INotificationPanelListItem[];
    allNotifications: INotificationPanelListItem[];
    unReadNotifications: Array<string>;
    unReadNotificationsCount: number;
    isNotificationPanelOpen: boolean;
    isLoading: boolean;
    hasError: boolean;
}

export interface INotificationPanelListItem {
    itemKey: string,
    displayStatus: string,
    status: string,
    messageBodyText: string,
    subjectIcon: string,
    subjectHeader: string,
}

export interface NProps {
    item: INotificationPanelListItem,
    status: string
}
