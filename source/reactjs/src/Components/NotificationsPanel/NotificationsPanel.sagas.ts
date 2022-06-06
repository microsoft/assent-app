import { SimpleEffect, getContext, put, all, call, takeLatest, select, delay } from 'redux-saga/effects';
import { IHttpClient } from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { IAuthClient } from '@micro-frontend-react/employee-experience/lib/IAuthClient';
import { ITelemetryClient } from '@micro-frontend-react/employee-experience/lib/ITelemetryClient';
import { NotificationsPanelActionType, IPostAlertsInfo } from './NotificationsPanel.action-types';
import { receiveAlertsInfo, resetAlertsInfo, toggleNotificationPanel } from './NotificationsPanel.actions';
import { setHeader } from '../Shared/Components/SagasHelper';
import { trackBusinessProcessEvent, trackException, TrackingEventId } from '../../Helpers/telemetryHelpers';
import { getStateCommonTelemetryProperties } from '../Shared/SharedComponents.selectors';
import { INotificationPanelListItem } from './NotificationsPanel.types';

function* fetchAlertsInfo(): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        const alertResponse: any = yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/alerts`,
            resource: __RESOURCE_URL__,
            headers: setHeader(null)
        });
        let tempItemsUnread: INotificationPanelListItem[] = [];
        let tempItemsRead: INotificationPanelListItem[] = [];
        const newNotifications: Array<string> = []
        
        const getIconType = (type: string): string => {
            if(type === 'danger') return 'ReportWarning'
            if(type === 'warning') return 'IncidentTriangle'
            if(type === 'info') return 'InfoSolid'
        }

        alertResponse.data.alerts.forEach((ele: any) => {
            ele.items.map((item: any) => {
                newNotifications.push(`${item.id}`);
                if (item.isRead === false) {
                    tempItemsUnread.push({
                        itemKey: item.id,
                        displayStatus: 'new',
                        status: item.isRead ? 'read' : 'unread',
                        messageBodyText: item.message,
                        subjectIcon: getIconType(ele.severity),
                        subjectHeader: item.title,
                    })
                } else {
                    tempItemsRead.push({
                        itemKey: item.id,
                        displayStatus: 'new',
                        status: item.isRead ? 'read' : 'unread',
                        messageBodyText: item.message,
                        subjectIcon: getIconType(ele.severity),
                        subjectHeader: item.title,
                    })
                }
            })
        });
        yield put(receiveAlertsInfo(tempItemsUnread, tempItemsRead, newNotifications));
        yield delay(1000) // if not delayed , badge number is not showing
        if(tempItemsUnread?.length > 0) {
            yield put(toggleNotificationPanel());
        }
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Notifications - Success',
            'Notifications.Success',
            TrackingEventId.NotificationsSuccess,
            stateCommonProperties,
        );
    } catch (error: any) {
        const exception = error.message ? new Error(error.message) : error;
        yield put(resetAlertsInfo());
        trackException(
            authClient,
            telemetryClient,
            'Notifications - Failure',
            'Notifications.Failure',
            TrackingEventId.NotificationsFailure,
            stateCommonProperties,
            exception
        );
    }
}

// have to pass array of ids of read notifications
function* updateAlertsInfo(action: IPostAlertsInfo): IterableIterator<SimpleEffect<{}, {}>> {
    const telemetryClient: ITelemetryClient = yield getContext('telemetryClient');
    const authClient: IAuthClient = yield getContext('authClient');
    const stateCommonProperties = yield select(getStateCommonTelemetryProperties);
    try {
        const httpClient: IHttpClient = yield getContext('httpClient');
        yield call([httpClient, httpClient.request], {
            url: `${__API_BASE_URL__}${__API_URL_ROOT__}/userpreference?SessionId=${telemetryClient.getCorrelationId()}`,
            method: 'POST',
            resource: __RESOURCE_URL__,
            headers: setHeader(null),
            data: { FeaturePreferenceJson: null, ReadNotificationsList: `${JSON.stringify(action.unReadNotifications)}` }
        });
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'Update Notifications - Success',
            'Notifications.Success',
            TrackingEventId.UpdateNotificationsSuccess,
            stateCommonProperties,
        );
    } catch (error: any) {
        const exception = error.message ? new Error(error.message) : error;
        trackException(
            authClient,
            telemetryClient,
            'Update Notifications - Failure',
            'Notifications.Failure',
            TrackingEventId.UpdateNotificationsFailure,
            stateCommonProperties,
            exception
        );
    }
}

export function* notificationsPanelSagas(): IterableIterator<{}> {
    yield all([
        takeLatest(NotificationsPanelActionType.REQUEST_ALERTS_INFO, fetchAlertsInfo),
        takeLatest(NotificationsPanelActionType.POST_ALERTS_INFO, updateAlertsInfo)
    ]);
}
