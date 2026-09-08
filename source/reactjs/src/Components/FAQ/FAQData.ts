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
        text: 'Approvals Assistant',
        emailAddress: '',
        body: '',
        subject: '',
        alternateText: 'Open Approvals Assistant',
    }
}


export const FAQList: Array<IFAQList> = [
    {
        title: "General navigation",
        text: `The MSApprovals website can be navigated using the top header and left navigation controls.
                The top header allows you to view notifications, change user settings, view the help page, provide
                feedback, and log out.^ You can also toggle between card view and table view using the ‘view type’
                toggle, sort requests based on application, submitter, and date, and filter requests as well.^
                There is also a checkbox to enable bulk selection and a button to refresh your queue. You can
                use the left navigation to switch to the history page and view your past actions.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        target: '_blank',
        screenshot: ['blueheadernav.png','toggleview.png','refreshsummary.png'],
    },
    {
        title: "How do I take an action on a request?",
        text: `To take an action on a single request, click on the request you would like to view.
                Some applications require you to view the entire request, and actions are enabled once you 
                scroll to the bottom.^ Click on the action you would like to take, and enter any additional 
                information. Click on submit in order to complete the action.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        target: '_blank',
        screenshot:['approveaction.png','actionslist.png'],
    },
    {
        title: "How do I perform bulk approvals?",
        text: `The bulk approval feature enables you to take action on multiple requests at the same time,
                rather than having to approve/reject each request individually. To enable bulk approval, check the
                "select multiple" checkbox on the home page.^ Bulk approvals are performed on a single application
                at a time. You can use the dropdown to select your desired application.^ Next you can select each
                request you would like to take an action on, or use the select all checkbox to select the maximum
                amount for the chosen application. Click on the action you would like to take and click submit to
                complete the action.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        target: '_blank',
        screenshot:['selectmultiple.png','selectmultiplechecked.png','allrowsselected.png'],
    },
    {
        title: "How do I set user preferences?",
        text: `To personalize your experience by setting user preferences, click on the 'settings'
                icon in the top header. Here you can set default settings for viewing request details,
                grouping your requests, and bulk selection. These settings will be remembered every time
                you come to MSApprovals.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        target: '_blank',
        screenshot: ['settings.png'],
    },
    {
        title: "How can I reassign a request?",
        text: `To reassign a request, open the request details by clicking on it. 
                Scroll to the bottom and locate the 'Other Actions' option, found 
                in the actions menu. Click on it and select 'Reassign - Assign to another user.' 
                ^ Choose the new assignee from the dropdown list, add any relevant 
                notes for the reassignment, and click 'Submit' to confirm.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        target: '_blank',
        screenshot: ['reassignDropdown.png','reassignAliasNotes.png'],
    },
    {
        title: "How do I view a quick tour again?",
        text: `All the quick tours are available in this section. Press "view all" to see all the quick tours at once, or click on an individual quicktour you may be interested in. You can view them again at anytime by clicking on the corresponding quick tour you wish to see.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        quickTour: true,
        target: '_blank',
        screenshot: null,
    },
    {
        title: "How do I use the Additional Information filter?",
        text: `The 'Additional Information' filter lets you refine requests by metadata supplied with the request. Type or pick a value from the dropdown and the table will immediately refresh to only show matching rows. You can combine this with other filters (Unit Value, Company Code, Application) to narrow results further. Clear the chip or select 'Clear' beside the filter to remove it and return to the broader list. Once you have filtered your requests, you can select multiple requests by enabling the 'select multiple' checkbox and then take bulk actions on the selected requests for efficient processing.`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        target: '_blank',
        screenshot: ['additionalInfoFilter.png'],
    },
    {
        title: "If my question is not listed here.",
        text: `If any of your Approvals related question is not listed here, please use  
                {0} to reach out to us and we will be happy to help!`,
        videoUrl: "",
        videoWidth: "",
        videoHeight: "",
        textAsHeader: "h4",
        isExpanded: false,
        fullScreen: false,
        email: true,
        template: emailTemplate.supportTemplate,
        target: '_blank',
        screenshot: null,
    },
]