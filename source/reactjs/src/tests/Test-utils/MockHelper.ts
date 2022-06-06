import { IAuthClient } from '@micro-frontend-react/employee-experience/lib/IAuthClient';
import { ITelemetryClient } from '@micro-frontend-react/employee-experience/lib/ITelemetryClient';

export const telemetryClientMock : ITelemetryClient = {
    trackDependencyData: jest.fn(),
    getChildInstance: jest.fn(),
    setContext: jest.fn(),
    getCorrelationId: jest.fn(),
    setAuthenticatedUserContext: jest.fn(),
    trackPageView: jest.fn(),
    stopTrackEvent: jest.fn(),
    startTrackEvent: jest.fn(),
    trackCustomEvent: jest.fn(),
    trackEvent: jest.fn(),
    addTelemetryInitializer: jest.fn(),
    trackException: jest.fn(),
    _onerror: jest.fn(),
    trackTrace: jest.fn(),
    trackMetric: jest.fn(),
    startTrackPage: jest.fn(),
    stopTrackPage: jest.fn(),
    trackPageViewPerformance: jest.fn()
};

export const authClientMock : IAuthClient = {
    authContext: jest.fn(),
    login: jest.fn(),
    logOut: jest.fn(),
    getUser: jest.fn(),
    getUserId: jest.fn(),
    isLoggedIn: jest.fn(),
    acquireToken: jest.fn()
};

export const errorMock = {
    name: 'errorName',
    message: 'errorMessage',
    stack: 'errorStack:'
};