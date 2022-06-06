import * as telemetryHelpers from '../../../../Helpers/telemetryHelpers';
import { telemetryClientMock, authClientMock, errorMock } from '../../../Test-utils/MockHelper';

describe('telemetryHelpers', () => {
    let appAction: string = 'testAction';
    let eventName: string = 'testEvent';
    let eventId: number = 1234;
    let businessProcessName: string = 'testBusinessProcess';
    let additionalProperties: { messageID: number } = { messageID: 123456 };

    it('should get Common Telemetry Properties', () => {
        const properties = telemetryHelpers.getContextCommonTelemetryProperties(
            authClientMock,
            telemetryClientMock,
            appAction,
            eventName,
            eventId
        );
        expect(properties).toBeTruthy;
        expect(properties.ComponentType).toEqual('Web');
        expect(properties.ErrorView).not.toBeNull;
    });

    it('should track Business Process Event', () => {
        var properties = telemetryHelpers.trackBusinessProcessEvent(
            authClientMock,
            telemetryClientMock,
            businessProcessName,
            appAction,
            eventId,
            '',
            additionalProperties
        );
        expect(properties).toBeTruthy;

        var properties = telemetryHelpers.trackBusinessProcessEvent(
            authClientMock,
            telemetryClientMock,
            businessProcessName,
            appAction,
            eventId,
            ''
        );
        expect(properties).toBeTruthy;
    });

    it('should track Feature Usage Event', () => {
        var properties = telemetryHelpers.trackFeatureUsageEvent(
            authClientMock,
            telemetryClientMock,
            businessProcessName,
            appAction,
            eventId,
            '',
            additionalProperties
        );
        expect(properties).toBeTruthy;
        var properties = telemetryHelpers.trackFeatureUsageEvent(
            authClientMock,
            telemetryClientMock,
            businessProcessName,
            appAction,
            eventId,
            ''
        );
        expect(properties).toBeTruthy;
    });

    it('should track Exception', () => {
        var properties = telemetryHelpers.trackException(
            authClientMock,
            telemetryClientMock,
            businessProcessName,
            appAction,
            eventId,
            '',
            errorMock,
            additionalProperties
        );
        expect(properties).toBeTruthy;
        var properties = telemetryHelpers.trackException(
            authClientMock,
            telemetryClientMock,
            businessProcessName,
            appAction,
            eventId,
            '',
            errorMock
        );
        expect(properties).toBeTruthy;
    });
});
