import * as React from 'react';
import { render, fireEvent, cleanup } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { IReducerRegistry } from '../Models/IReducerRegistry';
import { ReactReduxContext } from 'react-redux';
import { App } from '../App';
import {} from "../ShellWithStore"

const reducerRegistryMock: IReducerRegistry = {
    getReducers: jest.fn(),
    getDynamicReducers: jest.fn(),
    exists: () => false,
    register: jest.fn(),
    registerDynamic: jest.fn(),
    addChangeListener: jest.fn(),
    removeChangeListener: jest.fn()
};

afterEach(cleanup);
describe.skip('App testing', () => {
    it('render app properly', () => {
        const AppStore: any = {
            isNotificationPanelOpen: true,
            unReadNotificationsCount: 2
        };

        const reduxContext: IEmployeeExperienceContext = {
            reducerRegistry: { ...reducerRegistryMock },
            runSaga: jest.fn(),
            dispatch: jest.fn(),
            useSelector: jest.fn().mockImplementation(() => AppStore),
            // eslint-disable-next-line @typescript-eslint/camelcase
            __redux_context__: ReactReduxContext
        };
        const wrapper = render(
            <Context.Provider value={reduxContext}>
                <App />
            </Context.Provider>
        );
        expect(wrapper).toMatchSnapshot();
    });
    it.skip('click and open notification', () => {});
});
