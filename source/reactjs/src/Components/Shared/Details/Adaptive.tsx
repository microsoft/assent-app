import * as React from 'react';
import * as AdaptiveCards from 'adaptivecards';
import * as MarkdownIt from 'markdown-it';
import * as adaptivecardsTemplating from 'adaptivecards-templating';
import { highlightTermsInContainer } from '../../../Helpers/searchHighlightUtils';
const { hostConfig } = require('./AdaptiveHostConfig');

interface IAdaptiveProps {
    template: any;
    dataPayload: any;
    style?: any;
    onOpenURLActionExecuted: any;
    onSubmitActionExecuted: any;
    onToggleVisibilityActionExecuted: any;
    userAlias: string;
    shouldDetailReRender: boolean;
    highlightTerms?: string[];
}

export class Adaptive extends React.Component<IAdaptiveProps> {
    private adaptiveCard = new AdaptiveCards.AdaptiveCard();
    private hasScrolledToHighlight = false;
    private highlightTimerId: number | null = null;

    public componentWillUnmount(): void {
        if (this.highlightTimerId != null) window.clearTimeout(this.highlightTimerId);
    }

    private static arraysShallowEqual(a?: string[], b?: string[]): boolean {
        if (a === b) return true;
        if (!a || !b) return false;
        if (a.length !== b.length) return false;
        for (let i = 0; i < a.length; i++) {
            if (a[i] !== b[i]) return false;
        }
        return true;
    }

    shouldComponentUpdate(nextProps: IAdaptiveProps): boolean {
        const highlightChanged = !Adaptive.arraysShallowEqual(nextProps.highlightTerms, this.props.highlightTerms);
        const payloadChanged = nextProps.dataPayload.toString() !== this.props.dataPayload.toString();
        if (highlightChanged || payloadChanged) this.hasScrolledToHighlight = false;
        const safeToRerender = this.props.shouldDetailReRender && nextProps.shouldDetailReRender;
        return (highlightChanged || payloadChanged) && safeToRerender;
    }

    public render(): React.ReactElement {
        const openURLclickHandler = this.props.onOpenURLActionExecuted;
        const onSubmitActionExecuted = this.props.onSubmitActionExecuted;
        const onToggleVisibilityActionExecuted = this.props.onToggleVisibilityActionExecuted;
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

            AdaptiveCards.AdaptiveCard.onElementVisibilityChanged = function (element: any) {
                if (
                    (element.id.includes('uploadPOEContainer') ||
                        element.id.includes('exemptPOEContainer')) &&
                    element.isVisible
                ) {
                    onToggleVisibilityActionExecuted(element);
                }
                if (element.id === 'thumbsUpSelected' || element.id === 'thumbsDownSelected') {
                    onSubmitActionExecuted('riskLevelAgreement', {
                        value: element.isVisible ? (element.id === 'thumbsUpSelected' ? 'Like' : 'Dislike') : null,
                        type: 'Thumbs',
                    });
                }
            };

            AdaptiveCards.AdaptiveCard.onInputValueChanged = function (input) {
                // Call onSubmitActionExecuted for correctRiskLevel
                if (input.id === 'correctRiskLevel') {
                    onSubmitActionExecuted(input.id, {
                        value: input.value,
                        type: 'ChoiceSet',
                    });
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
            result.setAttribute('tabIndex', '-1');

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
                    style={{ marginTop: '-10px', ...this.props.style }}
                    ref={(n) => {
                        n != null && n.firstChild && n.removeChild(n.firstChild);
                        if (n != null) {
                            n.appendChild(result);
                            if (this.highlightTimerId != null) window.clearTimeout(this.highlightTimerId);
                            const timerId = window.setTimeout(() => {
                                this.highlightTimerId = null;
                                highlightTermsInContainer(n, this.props.highlightTerms);
                                if (this.props.highlightTerms?.length > 0 && !this.hasScrolledToHighlight) {
                                    const firstMark = n.querySelector('mark');
                                    if (firstMark) {
                                        firstMark.scrollIntoView({ behavior: 'smooth', block: 'center' });
                                        this.hasScrolledToHighlight = true;
                                    }
                                }
                            }, 0);
                            this.highlightTimerId = timerId;
                        }
                    }}
                />
            );
        } catch (err: any) {
            console.error(err);
            return <div style={{ color: 'red' }}>{err.message}</div>;
        }
    }
}
