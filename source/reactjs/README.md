# Microsoft Assent
## Assent <sub>**A***pproval* **S***olution* **S***implified for* **ENT***erprise*<sub>
Microsoft Assent (*a.k.a Approvals*) as a platform provides the "one stop shop" solution for approvers via a model that brings together disparate different approval requests in a consistent and ultra-modern model. Approvals delivers a unified approvals experience for any approval on multiple form factors - Website, Outlook Actionable email, Teams. It consolidates approvals across organization's line of business applications, building on modern technology and powered by Microsoft Azure. It serves as a showcase for solving modern IT scenarios using the latest technologies.
- Payload Receiver Service API - Accepts payload from tenant system.
- Audit Processor - Azure Function that logs the payload data into Azure Cosmos DB.
- Primary Processor - Azure Function that processes the payload pushed by payload receiver service API to service bus.
- Notification Processor - Azure Function that sends email notifications to Approvers/ Submitters as per configurations.
- WatchdogProcessor - as per configurations from tenant sends reminder email notifications to Approvers for pending approvals as per configurations from tenant.
- Core Services API - Set of Web APIs to support the Approvals UI.

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

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.