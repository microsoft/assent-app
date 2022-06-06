import { ComponentProvider } from '@micro-frontend-react/employee-experience/lib/ComponentProvider';
import * as React from 'react';
const Microfrontend = (props: {
    tenantId: number;
    executeMicrofrontendActionRef: any;
    dispatchUpdateAdditionalData: any;
    detailsJSON?: any;
    cdnURL?: string;
    selectedPage: string;
}): JSX.Element => {
    const Tenants = {
    };

    if (props.cdnURL && props.cdnURL !== '') {
        return (
            <ComponentProvider
                config={{
                    script: props.cdnURL,
                    name: 'MicrofrontendInputs'
                }}
                data={{ executeMicrofrontendActionRef: props.executeMicrofrontendActionRef }}
            />
        );
    }

    return <h2>Microfrontend view</h2>;
};

export default Microfrontend;