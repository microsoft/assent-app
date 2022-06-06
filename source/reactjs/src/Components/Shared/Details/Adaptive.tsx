import * as React from 'react';
import * as AdaptiveCards from 'adaptivecards';
import * as MarkdownIt from 'markdown-it';
import * as adaptivecardsTemplating from 'adaptivecards-templating';
const { hostConfig } = require('./AdaptiveHostConfig');

interface IAdaptiveProps {
    template: any;
    dataPayload: any;
    style?: any;
    onOpenURLActionExecuted: any;
    onSubmitActionExecuted: any;
    userAlias: string;
    shouldDetailReRender: boolean;
}

export class Adaptive extends React.Component<IAdaptiveProps> {
    private adaptiveCard = new AdaptiveCards.AdaptiveCard();

    shouldComponentUpdate(nextProps: IAdaptiveProps): boolean {
        return (
            nextProps.dataPayload.toString() !== this.props.dataPayload.toString() &&
            this.props.shouldDetailReRender &&
            nextProps.shouldDetailReRender
        );
    }

    public render(): React.ReactElement {
        const openURLclickHandler = this.props.onOpenURLActionExecuted;
        const onSubmitActionExecuted = this.props.onSubmitActionExecuted;
        try {
            const userAlias = this.props.userAlias;
            AdaptiveCards.AdaptiveCard.onProcessMarkdown = function (text, result) {
                result.outputHtml = MarkdownIt().render(text);
                result.didProcess = true;
            };

            AdaptiveCards.AdaptiveCard.onExecuteAction = function (action: any) {
                const type = action.getJsonTypeName();
                if (type === 'Action.OpenUrl') {
                    openURLclickHandler(action.getHref(), action.id, action.title, userAlias);
                } else if (type === 'Action.Submit') {
                    onSubmitActionExecuted(action.id, action.data);
                }
            };

            var templatePayload = new adaptivecardsTemplating.Template(this.props.template);

            // Expand the template with your `$root` data object.
            // This binds it to the data and produces the final Adaptive Card payload
            var cardPayload = templatePayload.expand({
                $root: this.props.dataPayload,
            });

            this.adaptiveCard.hostConfig = new AdaptiveCards.HostConfig(hostConfig);
            this.adaptiveCard.parse(cardPayload);
            const result = this.adaptiveCard.render();
            result.style.outline = 'none';
            result.setAttribute('tabIndex','-1');
           
            const acSelectableElements = result.getElementsByClassName('ac-selectable');
            for (let i = 0; i < acSelectableElements.length; i++) {
                (acSelectableElements[i] as HTMLElement).style.cursor = 'Pointer';
            }
            const anchorElements = result.getElementsByClassName('ac-anchor');
            //added aria label to links in adaptive card
            for (let i = 0; i < anchorElements.length; i++) {
                const anchorTitle = anchorElements[i].getAttribute('title');
                anchorElements[i].setAttribute('aria-label', anchorTitle); 
                anchorElements[i].setAttribute('tabIndex', 0);   
            }
            const expandElements = result.getElementsByClassName('ac-selectable');
            //sets aria-expanded state for expand/collapse buttons
            for (let i = 0; i < expandElements.length; i++) {
                const elementTitle = expandElements[i].getAttribute('title') ?? expandElements[i].getAttribute('alt');
                if (elementTitle?.toLowerCase().includes('expand')) {
                    expandElements[i].setAttribute('aria-expanded', 'false');
                } else if (elementTitle?.toLowerCase().includes('collapse')) {
                    expandElements[i].setAttribute('aria-expanded', 'true');
                }
            }
            const imageElements = result.getElementsByClassName('ac-image');
            //setting alt to empty string for decorative images
            for (let i = 0; i < imageElements.length; i++) {
                const imageAlt = imageElements[i].getAttribute('alt');
                if (!imageAlt) {
                    imageElements[i].setAttribute('alt', '');
                }
                //set title property to alt
                imageElements[i].setAttribute('title', imageAlt ?? '');
                //addressing expand/collapse buttons that are images instead of selectables
                if (imageAlt?.toLowerCase().includes('expand')) {
                    imageElements[i].setAttribute('aria-expanded', 'false');
                } else if (imageAlt?.toLowerCase().includes('collapse')) {
                    imageElements[i].setAttribute('aria-expanded', 'true');
                }
            }
            return (
                <div
                    style={this.props.style}
                    ref={(n) => {
                        n != null && n.firstChild && n.removeChild(n.firstChild);
                        n != null && n.appendChild(result);                      
                    }}
                />
            );
        } catch (err: any) {
            console.error(err);
            return <div style={{ color: 'red' }}>{err.message}</div>;
        }
    }
}
