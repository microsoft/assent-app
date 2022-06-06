import { IFAQList } from "./FAQPage";

export interface IEmailTemplate {
    supportTemplate: ITemplate
}

export interface ITemplate {
    text: string;
    emailAddress: string;
    body: string,
    subject: string,
    alternateText: string
}

export const emailTemplate: IEmailTemplate = {
    supportTemplate: {
        text: "MSApprovals Support",
        emailAddress: "msapprovalssup@microsoft.com",
        body: "",
        subject: "",
        alternateText: "MSApprovals Support link"
    }
}


export const FAQList: Array<IFAQList> = [
    {
        title: "General navigation",
        text: `The MSApprovals website can be navigated using the top header and left navigation controls.
                The top header allows you to view notifications, change user settings, view the help page, provide
                feedback, and log out. You can also toggle between card view and table view using the ‘view type’
                toggle, sort requests based on application, submitter, and date, and filter requests as well.
                There is also a checkbox to enable bulk selection and a button to refresh your queue. You can
                use the left navigation to switch to the history page and view your past actions.`,
        videoUrl: "https://msit.microsoftstream.com/embed/video/7aba0840-98dc-b561-8dbb-f1ebf3c77450?showinfo=false",
        videoWidth: "100%",
        videoHeight: "360",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: true
    },
    {
        title: "How do I take an action on a request?",
        text: `To take an action on a single request, click on the request you would like to view.
                Some applications require you to view the entire request, and actions are enabled once you 
                scroll to the bottom. Click on the action you would like to take, and enter any additional 
                information. Click on submit in order to complete the action.`,
        videoUrl: "https://msit.microsoftstream.com/embed/video/7aba0840-98dc-b561-6860-f1ebee652b6e?showinfo=false",
        videoWidth: "100%",
        videoHeight: "360",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: true
    },
    {
        title: "How do I perform bulk approvals?",
        text: `The bulk approval feature enables you to take action on multiple requests at the same time,
                rather than having to approve/reject each request individually. To enable bulk approval, check the
                "select multiple" checkbox on the home page. Bulk approvals are performed on a single application
                at a time. You can use the dropdown to select your desired application. Next you can select each
                request you would like to take an action on, or use the select all checkbox to select the maximum
                amount for the chosen application. Click on the action you would like to take and click submit to
                complete the action.`,
        videoUrl: "https://msit.microsoftstream.com/embed/video/cca00840-98dc-b561-ea7e-f1ebf3c6c18a?showinfo=false",
        videoWidth: "100%",
        videoHeight: "360",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: true
    },
    {
        title: "How do I set user preferences?",
        text: `To personalize your experience by setting user preferences, click on the 'settings'
                icon in the top header. Here you can set default settings for viewing request details,
                grouping your requests, and bulk selection. These settings will be remembered every time
                you come to MSApprovals.`,
        videoUrl: "https://msit.microsoftstream.com/embed/video/a7ae0840-98dc-b561-fa1f-f1ebf3c85a0c?showinfo=false",
        videoWidth: "100%",
        videoHeight: "360",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: true
    },
    {
        title: "If my question is not listed here.",
        text: `If any of your Approvals related question is not listed here, please send an email to  
                {0} and we will be happy to help!`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        email: true,
        template: emailTemplate.supportTemplate
    }
]