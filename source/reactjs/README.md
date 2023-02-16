# MSApprovals
MSApprovals is an important service and platform that supports 16000 users in approving ~750K approvals each month with the total amount ranging anywhere from ~3B$ to ~5B$ and this is across ~21 different Line of Business applications.


## Run the template

1. Update below properties in the globals section of config/local.js
    1. __CLIENT_ID__: process.env.clientId || '########-####-####-####-############',
    2. __INSTRUMENTATION_KEY__: process.env.instrumentationKey || '########-####-####-####-############'
};

2. Run `npm install`

3. Run `npm start` to run the app

## User feedback setup

1. Update the __FEEDBACK_CONFIGURATION_URL__ value in the config files to your feedback endpoint, the feedback icon in top header will only appear if this value is set
2. Implement the IFeedback interface provided in Feedback.ts, a sample Feedback class is provided in the same file
3. If using a custom component for the Feedback UI, it can be linked with the top header using the launchfeedback event handler of the Feedback class and any initialization required for the user feedback flow can be done within the constructor
4. Create an instance of your feedback class of type IFeedback and pass it to the TopHeader component, an example is provided below where feedback is initialized as a local state object
    
    ```
    setFeedback(new Feedback() as IFeedback);
    <TopHeader upn={user?.email} displayName={user?.name} feedback={feedback} />
    ```

# Contributors
    MSApprovals Team (MSApprovalsCore@microsoft.com)