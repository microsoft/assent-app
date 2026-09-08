export interface IBot {
    launchBot(): void;

    handleBotException(ex?: any): void;
}

/* SAMPLE IMPLEMENTION OF IFEDABOT INTERFACE
class SampleBot implements IBot { 
    
    Use the constructor to initialize a Bot connection or make any initial api calls required to set up Bot
    
    constructor() {
    }

    
    This launches a Bot window - this function will get called when the user clicks on the help -> bot link in the top header.
    If you're using a custom react component for the Bot UI, this function can be used to update the display flag for the component.
    If the display flag is stored in the redux state, this fuction can be used to dispatch an action to update the display value to true.
    
    public launchBot = () => {};
}
*/

// add a registry of type IFeedback
export class BotRegistry {
    implementations: IBot[] = [];

    getImplementation(): IBot {
        return this.implementations?.[0];
    }

    register<T extends IBot>(newClass: T) {
        this.implementations.push(newClass);
    }
}
