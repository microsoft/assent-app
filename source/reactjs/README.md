## Run the template

1. Update all properties in the globals section of config/local.js
    1. __CLIENT_ID__: process.env.clientId || '########-####-####-####-############',
    2. __INSTRUMENTATION_KEY__: process.env.instrumentationKey || '########-####-####-####-############'
};

2. Run `npm install`

3. Run `npm start` to run the app