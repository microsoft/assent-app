//import { MSALV2Client } from '@employee-experience/core/lib/MSALV2Client';
import { AuthClient } from '@micro-frontend-react/employee-experience/lib/AuthClient';
import { TelemetryClient } from '@micro-frontend-react/employee-experience/lib/TelemetryClient';
import { HttpClient } from '@micro-frontend-react/employee-experience/lib/HttpClient';
import { GraphClient } from '@micro-frontend-react/employee-experience/lib/GraphClient';
import { Shell } from '@micro-frontend-react/employee-experience/lib/Shell';
import { withStore } from '@micro-frontend-react/employee-experience/lib/withStore';
import { StoreBuilder } from '@micro-frontend-react/employee-experience/lib/StoreBuilder';
import { ReducerRegistry } from '@micro-frontend-react/employee-experience/lib/ReducerRegistry';
import sessionStorage from 'redux-persist/lib/storage/session';
import { guid } from './Helpers/Guid';

let correlationId = window.sessionStorage.getItem('correlationId');
if (correlationId === 'value' || !correlationId) {
    correlationId = guid();
    window.sessionStorage.setItem('correlationId', correlationId);
}

const telemetryClient = new TelemetryClient(
    {
        instrumentationKey: __INSTRUMENTATION_KEY__,
        UTPConfig: {
            EnvironmentName: __ENV_NAME__,
            ServiceOffering: 'Finance',
            ServiceLine: 'Procure To Pay',
            Service: 'MSApprovals',
            ComponentName: 'MSApprovalsReact',
            ComponentId: 'b375ec23-b2b4-49f6-98ac-3118fb312f25',
        },
        defaultProperties: {
            appName: __APP_NAME__,
        },
    },
    correlationId
);

const authClient = new AuthClient(
    {
        auth: {
            clientId: __CLIENT_ID__,
            redirectUri: window.location.origin,
            authority: __AUTHORITY__,
        },
    },
    telemetryClient
);

const httpClient = new HttpClient(telemetryClient, authClient);
const graphClient = new GraphClient(httpClient);
//const componentLoader = new ComponentLoader(telemetryClient, httpClient);

const persistStateConfig = {
    storage: sessionStorage,
    blacklist: ['dynamic'],
};

const reducerRegistry = new ReducerRegistry();
const appName = 'MSApprovalsWeb';
const storeResult = new StoreBuilder(reducerRegistry, {})
    .configureSaga({ telemetryClient, authClient, httpClient, graphClient, appName })
    .configurePersistor(persistStateConfig)
    .configureLogger(false)
    .build();

export const ShellWithStore = withStore(storeResult)(Shell);
