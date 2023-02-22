export interface IFeedback {
    launchFeedback(): void;
}

/* SAMPLE IMPLEMENTION OF IFEEDBACK INTERFACE
class SampleFeedback implements IFeedback { 
    
    Use the constructor to initialize a feedback connection or make any initial api calls required to set up feedback
    
    constructor() {
    }

    
    This launches a feedback window - this function will get called when the user clicks on the feedback button in the top header.
    If you're using a custom react component for the feedback UI, this function can be used to update the display flag for the component.
    If the display flag is stored in the redux state, this fuction can be used to dispatch an action to update the display value to true.
    
    public launchFeedback = () => {};
}
*/

// add a registry of type IFeedback
export class FeedbackRegistry {
    implementations: IFeedback[] = [];

    getImplementation(): IFeedback {
        return this.implementations?.[0];
    }

    register<T extends IFeedback>(newClass: T) {
        this.implementations.push(newClass);
    }
}
