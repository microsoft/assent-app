// Any changes in this file requires rebuild

const staticComponents = {
    /* =========================
     * Add host app entry point here
     * ====================== */
    app: './src/App.tsx'
};

const dynamicComponents = {
    /* =========================
     * Add micro-frontend app entry points here
     * ====================== */
};

const globals = {
    /* =========================
     * Add build time variables here
     * These variables also need to added in ./global.d.ts file to be available in Typescript
     * ====================== */

    __APP_NAME__: 'Approvals',
    __CLIENT_ID__: process.env.clientId || '',
    __BASE_URL__: process.env.baseUrl || 'https://localhost:9000/',
    __INSTRUMENTATION_KEY__: process.env.instrumentationKey || '',
    __ENV_NAME__: 'Development',

    __AUTHORITY__: process.env.authority || 'https://login.microsoftonline.com/{AADTenantID}',
    __API_BASE_URL__: process.env.apiBaseUrl || 'https://api.approvals.contoso.com',
    __API_URL_ROOT__: process.env.apiBaseUrl || '/api/v1',
    __GRAPH_BASE_URL__: process.env.graphBaseUrl || 'https://graph.microsoft.com/v1.0/',
    __GRAPH_RESOURCE_URL__: process.env.graphResourceUrl || 'https://graph.microsoft.com',
    __RESOURCE_URL__: process.env.resourceUrl || 'https://api.approvals.contoso.com/',
    __MICROFRONTEND_CDN_URL__: process.env.microfrontendURL || '',
    __UPN_SUFFIX__: process.env.upnSuffix || '@contoso.com',
    //OCV App Id and telemetry group
    __OCV_APP_ID__: process.env.ocvAppId || 0000,
    __OCV_ENVIRONMENT_NAME__: 'Int',
    __OCV_TELEMETRY_GROUP__: process.env.telemetryGroup || {
        featureArea: 'Modern Website'
    },

    __API_BASE_MATTER_URL__: process.env.apiBaseUrl || '',
    __API_BASE_MATTER_RESOURCE_URL__: process.env.apiBaseUrl || '',
    __CLASSIC_WEB_URL__: process.env.classicWebUrl || 'https://approvals.contoso.com/'
};

module.exports = {
    staticComponents,
    dynamicComponents,
    globals
};