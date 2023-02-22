import * as styledUMD from 'styled-components';

declare global {
    const styled: typeof styledUMD.default;
    const __APP_NAME__: string;
    const __CLIENT_ID__: string;
    const __BASE_URL__: string;
    const __INSTRUMENTATION_KEY__: string;
    const __ENV_NAME__: string;

    const __API_BASE_URL__: string;
    const __API_URL_ROOT__: string;
    const __RESOURCE_URL__: string;
    const __GRAPH_BASE_URL__: string;
    const __UPN_SUFFIX__: string;
    const __GRAPH_RESOURCE_URL__: string;
    const __AUTHORITY__: string;
    const __API_BASE_MATTER_URL__: string;
    const __API_BASE_MATTER_RESOURCE_URL__: string;
    const __FEEDBACK_ENVIRONMENT_NAME__: string;
    const __FEEDBACK_CONFIGURATION_URL__: string;
    const __MICROFRONTEND_CDN_URL__: string;
    const __CLASSIC_WEB_URL__: string;
}
