import { IFeaturesIntroductionStep } from "../SharedComponents.types";

export const teachingSteps: IFeaturesIntroductionStep[] = [
    {
        // This step happens when we decline step 1
        step: 0,
        headline: "You can always take the tour later by clicking on help!",
        target: "#help",
        successButtonLabel: "Great!",
        declineButtonLabel: null,
        successNextStep: -1,
        declineNextStep: -1
    },
    {
        step: 1,
        headline: "Welcome to MSApprovals! Want to learn our new features?",
        target: "#MyView_id0",
        successButtonLabel: "Take Tour",
        declineButtonLabel: "Maybe Later",
        successNextStep: 2,
        declineNextStep: 0
    },
    {
        step: 2,
        headline: "Click here to group items!",
        target: "#groupBy",
        successButtonLabel: "Next",
        declineButtonLabel: null,
        successNextStep: 3,
        declineNextStep: -1
    },
    {
        step: 3,
        headline: "Filter your items here!",
        target: "#filter",
        successButtonLabel: "Next",
        declineButtonLabel: null,
        successNextStep: 4,
        declineNextStep: -1
    },
    {
        step: 4,
        headline: "Click here to view your history!",
        target: "#history",
        successButtonLabel: "Next",
        declineButtonLabel: null,
        successNextStep: 5,
        declineNextStep: -1
    },
    {
        step: 5,
        headline: "Thanks for taking the tour!",
        target: "#MyView_id0",
        successButtonLabel: "Done",
        declineButtonLabel: null,
        successNextStep: -1,
        declineNextStep: -1
    }
]

export const initialTeachingStep: number = 1;