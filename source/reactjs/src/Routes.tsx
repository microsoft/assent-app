import * as React from 'react';
import { Switch, Route } from 'react-router-dom';
import { RouteComponentProvider } from '@micro-frontend-react/employee-experience/lib/RouteComponentProvider';
import { OutOfSyncPage } from './Components/OutOfSync/OutOfSyncApprovalsPage';
import { PendingApprovalsPage } from './Components/PendingApprovals/PendingApprovalsPage';
import { HistoryPage } from './Components/History/HistoryPage';
import { MicrofrontendPageClass } from './Components/MicrofrontendPage/MicrofrontendPageClass';
import { UrlWithQueryParams } from './Helpers/sharedHelpers';
import { FAQPage } from './Components/FAQ/FAQPage';

export function Routes(): React.ReactElement {
    UrlWithQueryParams();
    return (
        <Switch>
            <Route path="/history" component={HistoryPage} exact={true} />
            <Route path="/outofsync/:documentNumber" component={OutOfSyncPage} exact={true} />
            <Route path="/outofsync" component={OutOfSyncPage} exact={true}/>
            <Route path="/:tenantId/:documentNumber" component={PendingApprovalsPage} exact={true} />
            <Route path="/" component={PendingApprovalsPage} exact={true} />
            <Route path="/microfrontend-inputs-class" component={MicrofrontendPageClass} exact={true} />
            <Route path="/faq" component={FAQPage} exact={true} />
            <RouteComponentProvider
                path="/dynamic-redux-hooks"
                config={{
                    script: '/bundles/dynamic-redux-hooks.js',
                    name: 'DynamicReduxHooks'
                }}
            />
            <RouteComponentProvider
                path="/dynamic-redux-class"
                config={{
                    script: '/bundles/dynamic-redux-class.js',
                    name: 'DynamicReduxClass'
                }}
            />
            <RouteComponentProvider
                path="/dynamic-sub-routes"
                config={{
                    script: '/bundles/dynamic-sub-routes.js',
                    name: 'DynamicSubRoutes'
                }}
            />
            <RouteComponentProvider
                path="/dynamic-custom-props"
                config={{
                    script: '/bundles/dynamic-custom-props.js',
                    name: 'DynamicCustomProps'
                }}
            />
        </Switch>
    );
}
