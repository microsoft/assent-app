import * as React from 'react';
import {
    notificationsPanelReducerName,
    notificationsPanelReducer,
    notificationsPanelInitialState,
} from './NotificationsPanel.reducer';
import { INotificationsPanelState } from './NotificationsPanel.types';
import { notificationsPanelSagas } from './NotificationsPanel.sagas';
import { Reducer } from 'redux';
import { requestAlertsInfo } from './NotificationsPanel.actions';
import { useDispatch } from 'react-redux';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Panel } from '@fluentui/react/lib/Panel';
import { postAlertsInfo, toggleNotificationPanel, clearUnreadNotifications } from './NotificationsPanel.actions';
import { FocusZone, FocusZoneDirection } from '@fluentui/react/lib/FocusZone';
import { List } from '@fluentui/react/lib/List';
import { RenderListItem } from './RenderNotificationsListItem';
import * as notificationStyled from './NotificationsPanelStyling';
import { EmptyResults } from '../Shared/Components/EmptyResults';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

export function NotificationPanelCustom(): React.ReactElement {
    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const { allNotifications, unReadNotifications, isNotificationPanelOpen, itemsUnread, itemsRead } = useSelector(
        (state: INotificationsPanelState) =>
            state.dynamic?.[notificationsPanelReducerName] || notificationsPanelInitialState
    );

    return (
        <Panel
            headerText={'Notifications'}
            isOpen={isNotificationPanelOpen}
            onOpened={() => {
                if (unReadNotifications?.length > 0) {
                    dispatch(postAlertsInfo(unReadNotifications));
                }
            }}
            onDismiss={() => {
                dispatch(toggleNotificationPanel());
                dispatch(clearUnreadNotifications());
            }}
            isLightDismiss
            closeButtonAriaLabel="Close notifications panel"
        >
            {unReadNotifications?.length > 0 && itemsUnread?.length > 0 && (
                <FocusZone direction={FocusZoneDirection.vertical}>
                    <h4 className={notificationStyled.italicStyle}>{itemsUnread?.length} new alert(s)</h4>
                    <div className={notificationStyled.container} data-is-scrollable>
                        <List
                            items={itemsUnread}
                            onRenderCell={(item) => <RenderListItem item={item} status="unread" />}
                        />
                    </div>
                </FocusZone>
            )}

            {unReadNotifications?.length > 0 && itemsRead?.length > 0 && (
                <FocusZone direction={FocusZoneDirection.vertical}>
                    <h4 className={notificationStyled.italicStyle}>Older alert(s)</h4>
                    <div className={notificationStyled.container} data-is-scrollable>
                        <List items={itemsRead} onRenderCell={(item) => <RenderListItem item={item} status="read" />} />
                    </div>
                </FocusZone>
            )}

            {/* When all notifications are read */}
            {unReadNotifications?.length === 0 && allNotifications?.length > 0 && (
                <FocusZone direction={FocusZoneDirection.vertical}>
                    <h4 className={notificationStyled.italicStyle}>Older alert(s)</h4>
                    <div className={notificationStyled.container} data-is-scrollable>
                        <List
                            items={allNotifications}
                            onRenderCell={(item) => <RenderListItem item={item} status="read" />}
                        />
                    </div>
                </FocusZone>
            )}

            {/* When there are no notifications at all */}
            {allNotifications?.length === 0 && (
                <div className={notificationStyled.emptyNotifications}>
                    <EmptyResults message="No new notifications" />
                </div>
            )}
        </Panel>
    );
}
// to get notifications
export function getNotifications() {
    useDynamicReducer(
        notificationsPanelReducerName,
        notificationsPanelReducer as Reducer,
        [notificationsPanelSagas],
        false
    );

    const dispatch = useDispatch();

    React.useEffect(() => {
        dispatch(requestAlertsInfo());
    }, [dispatch]);
}
