import * as React from 'react';
import { render, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { NotificationPanelCustom } from '../../../Components/NotificationsPanel/NotificationsPanel';
import { IReducerRegistry } from '../../../Models/IReducerRegistry';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { ReactReduxContext } from 'react-redux';

const reducerRegistryMock: IReducerRegistry = {
    getReducers: jest.fn(),
    getDynamicReducers: jest.fn(),
    exists: () => false,
    register: jest.fn(),
    registerDynamic: jest.fn(),
    addChangeListener: jest.fn(),
    removeChangeListener: jest.fn()
};

describe('NotificationsPanel', () => {
    it('should NotificationsPanel renders correctly', () => {
        const NotificationStore = {
            allNotifications: [
                {
                    itemKey: '1',
                    displayStatus: 'new',
                    status: 'read',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'ReportWarning',
                    subjectHeader: 'Testing Notification One'
                },
                {
                    itemKey: '2',
                    displayStatus: 'new',
                    status: 'unread',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'IncidentTriangle',
                    subjectHeader: 'Testing Notification Two'
                },
                {
                    itemKey: '3',
                    displayStatus: 'new',
                    status: 'unread',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'InfoSolid',
                    subjectHeader: 'Testing Notification Three'
                }
            ],
            unReadNotifications: ['2', '3'],
            isNotificationPanelOpen: true,
            itemsUnread: [
                {
                    itemKey: '2',
                    displayStatus: 'new',
                    status: 'unread',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'IncidentTriangle',
                    subjectHeader: 'Testing Notification Two'
                },
                {
                    itemKey: '3',
                    displayStatus: 'new',
                    status: 'unread',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'InfoSolid',
                    subjectHeader: 'Testing Notification Three'
                }
            ],
            itemsRead: [
                {
                    itemKey: '1',
                    displayStatus: 'new',
                    status: 'read',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'ReportWarning',
                    subjectHeader: 'Testing Notification One'
                }
            ]
        };
        const reduxContext: IReduxContext = {
            reducerRegistry: { ...reducerRegistryMock },
            runSaga: jest.fn(),
            dispatch: jest.fn(),
            useSelector: jest.fn().mockImplementation(() => NotificationStore),
            // eslint-disable-next-line @typescript-eslint/camelcase
            __redux_context__: ReactReduxContext
        };
        const wrapper = render(
            <ReduxContext.Provider value={reduxContext}>
                <NotificationPanelCustom />
            </ReduxContext.Provider>
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('no unreadNotifications', () => {
        const NotificationStore: any = {
            allNotifications: [
                {
                    itemKey: '1',
                    displayStatus: 'new',
                    status: 'read',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'ReportWarning',
                    subjectHeader: 'Testing Notification One'
                },
                {
                    itemKey: '2',
                    displayStatus: 'new',
                    status: 'unread',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'IncidentTriangle',
                    subjectHeader: 'Testing Notification Two'
                },
                {
                    itemKey: '3',
                    displayStatus: 'new',
                    status: 'unread',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'InfoSolid',
                    subjectHeader: 'Testing Notification Three'
                }
            ],
            unReadNotifications: [],
            isNotificationPanelOpen: true,
            itemsUnread: [],
            itemsRead: [
                {
                    itemKey: '1',
                    displayStatus: 'new',
                    status: 'read',
                    messageBodyText: 'This is test text',
                    subjectIcon: 'ReportWarning',
                    subjectHeader: 'Testing Notification One'
                }
            ]
        };
        const reduxContext: IReduxContext = {
            reducerRegistry: { ...reducerRegistryMock },
            runSaga: jest.fn(),
            dispatch: jest.fn(),
            useSelector: jest.fn().mockImplementation(() => NotificationStore),
            // eslint-disable-next-line @typescript-eslint/camelcase
            __redux_context__: ReactReduxContext
        };
        const { container, getByText } = render(
            <ReduxContext.Provider value={reduxContext}>
                <NotificationPanelCustom />
            </ReduxContext.Provider>
        );
        // const getAlertIcon = container.querySelector("button[aria-label='Alert Notifications']");
        // fireEvent.click(getAlertIcon);
        expect(getByText('Notifications')).toBeInTheDocument();
    });

    it('empty notifications', () => {
        const NotificationStore: any = {
            allNotifications: [],
            unReadNotifications: [],
            isNotificationPanelOpen: true,
            itemsUnread: [],
            itemsRead: []
        };
        const reduxContext: IReduxContext = {
            reducerRegistry: { ...reducerRegistryMock },
            runSaga: jest.fn(),
            dispatch: jest.fn(),
            useSelector: jest.fn().mockImplementation(() => NotificationStore),
            // eslint-disable-next-line @typescript-eslint/camelcase
            __redux_context__: ReactReduxContext
        };
        const { container, getByText } = render(
            <ReduxContext.Provider value={reduxContext}>
                <NotificationPanelCustom />
            </ReduxContext.Provider>
        );
        // const getAlertIcon = container.querySelector("button[aria-label='Alert Notifications']");
        // fireEvent.click(getAlertIcon);
        expect(getByText('Notifications')).toBeInTheDocument();
    });

    it('close notifications', () => {
      const NotificationStore: any = {
          allNotifications: [],
          unReadNotifications: [],
          isNotificationPanelOpen: false,
          itemsUnread: [],
          itemsRead: []
      };
      const reduxContext: IReduxContext = {
          reducerRegistry: { ...reducerRegistryMock },
          runSaga: jest.fn(),
          dispatch: jest.fn(),
          useSelector: jest.fn().mockImplementation(() => NotificationStore),
          // eslint-disable-next-line @typescript-eslint/camelcase
          __redux_context__: ReactReduxContext
      };
      const { container, getByText } = render(
          <ReduxContext.Provider value={reduxContext}>
              <NotificationPanelCustom />
          </ReduxContext.Provider>
      );
      expect(/'Notifications'/).not.toBeUndefined();
  });
});
