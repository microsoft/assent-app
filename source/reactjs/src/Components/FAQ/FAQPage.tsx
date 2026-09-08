import * as React from 'react';
import { usePageTracking } from '@micro-frontend-react/employee-experience/lib/usePageTracking';
import { usePageTitle } from '@micro-frontend-react/employee-experience/lib/usePageTitle';
import { IStackTokens, Stack } from '@fluentui/react/lib/Stack';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { setQuickTourData, toggleQuickTour, updateSelectedPage } from '../Shared/SharedComponents.actions';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getFeature, getPageLoadFeature } from '@micro-frontend-react/employee-experience/lib/UsageTelemetryHelper';
import * as Styled from './FAQStyling';
import { FAQList, ITemplate } from './FAQData';
import { Link } from '../Shared/Styles/Link';
import { format } from 'react-string-format';
import { CollapsibleSection } from '../Shared/Components/CollapsibleSection';
import { QuickTour } from '../Shared/Components/QuickTour/QuickTour';
import { DefaultButton, IconButton, PrimaryButton } from '@fluentui/react';
import { getItemsRead, getItemsUnread } from '../Shared/SharedComponents.selectors';
import { CoherenceColors } from '../Shared/SharedColors';

export interface IFAQList {
    title: string;
    text: string;
    videoUrl: string;
    videoWidth: string;
    videoHeight: string;
    textAsHeader: any;
    isExpanded: boolean;
    fullScreen: boolean;
    email?: boolean;
    quickTour?: boolean;
    template?: ITemplate;
    target: string;
    screenshot: Array<string>;
}

function Mailto({
    thisRef,
    alternateText,
    email,
    subject,
    body,
    ...props
}: {
    [x: string]: any;
    thisRef: React.Ref<HTMLAnchorElement>;
    alternateText: string;
    email: string;
    subject: string;
    body: string;
}): JSX.Element {
    return (
        <Link
            ref={thisRef}
            title={alternateText ? alternateText : null}
            href={`mailto:${email}?${subject !== '' ? `subject=${encodeURIComponent(subject) || ' '}` : ''}${body !== '' ? `&body=${encodeURIComponent(body) || ' '}` : ''
                }`}
        >
            {props.children}
        </Link>
    );
}

function FAQPage(): React.ReactElement {
    usePageTitle(`FAQ - ${__APP_NAME__}`);
    const feature = getFeature('MSApprovalsWeb', 'FAQPage');
    usePageTracking(getPageLoadFeature(feature));
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const stackTokens: IStackTokens = { childrenGap: 5 };

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

    const itemsRead = useSelector(getItemsRead)
    const itemsUnread = useSelector(getItemsUnread)
    const quickTours = itemsRead.concat(itemsUnread).filter((item) => item.slides && item.slides.length > 0)
    let textArray;

    React.useEffect(() => {
        dispatch(updateSelectedPage('faq'));

        function handleResize() {
            setDimensions({
                height: window.innerHeight,
                width: window.innerWidth,
            });
        }
        window.addEventListener('resize', handleResize);

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    function handleQuickTourOpenAndClose(index: number): void {
        if (index === -1) { //for multiple quick tours 
            dispatch(setQuickTourData(quickTours))
        }
        else {
            dispatch(setQuickTourData([quickTours[index]]))
        }
        dispatch(toggleQuickTour())
    }

    const handleClick = (e: React.MouseEvent) => {
            e.preventDefault();
        };

    return (
        <Styled.FAQContainer windowHeight={dimensions.height} windowWidth={dimensions.width}>
            <Stack className="scroll-hidden v-scroll-auto custom-scrollbar">
                <Styled.FAQTitle>FAQ</Styled.FAQTitle>
                <br />
                {FAQList.map((item: IFAQList, index: number) => (
                    <Stack.Item styles={{ root: { marginBottom: '5px', width: '90%' } }}>
                        <CollapsibleSection
                            defaultIsExpanded={item.isExpanded}
                            titleText={item.title}
                            renderHeaderAs={item.textAsHeader}
                            styles={{ root: { flexGrow: 0, width: '100%' } }}
                        >
                            <Stack.Item styles={{ root: { marginLeft: '40px' } }}>
                                {item.email === true ? (
                                    <p style={{ textAlign: 'justify' }}>
                                        {format(
                                            item.text,
                                            <Link
                                                title={item.template.alternateText ? item.template.alternateText : null}
                                                onClick={handleClick}
                                                target={item.target}
                                            >
                                                {item.template.text}
                                            </Link>
                                        )}
                                    </p>
                                ) : 
                                  item.screenshot ? (
                                    textArray = item.text.split('^'),
                                    textArray.map((obj, index: number) =>
                                        <div style={{ position: 'relative', height: 'auto', overflow: 'hidden' }}>
                                            <p style={{ textAlign: 'justify' }}>{obj}</p>
                                            <span>
                                                <img src={"./images/"+item.screenshot.at(index)} width="80%" alt={item.screenshot.at(index)} title={item.screenshot.at(index)} style={{padding:'2%'}} />
                                            </span>
                                        </div>
                                    )
                                ) : (
                                    <p style={{ textAlign: 'justify' }}>{item.text}</p>
                                )}

                                {item.quickTour === true && (
                                    <>
                                        <div>
                                            <br />
                                            <Stack tokens={stackTokens} horizontalAlign='start' wrap>
                                                <Stack.Item>
                                                    <PrimaryButton
                                                        styles={Styled.QuickTourButtons}
                                                        secondaryText="View All"
                                                        onClick={() => handleQuickTourOpenAndClose(-1)}
                                                        text="View All"
                                                        disabled={quickTours.length < 1} />
                                                </Stack.Item>

                                                {quickTours.map((quickTour: { name: string; summaryImage: string; }, index: number) => (

                                                    <div>
                                                        <Stack title={quickTour.name} styles={Styled.interactiveStackStyles} tokens={stackTokens} horizontal horizontalAlign='start' verticalAlign='center' wrap onClick={() => handleQuickTourOpenAndClose(index)}>
                                                            <Stack.Item >
                                                                <IconButton
                                                                    title={quickTour.name}
                                                                    ariaLabel={quickTour.name}
                                                                    styles={Styled.interactiveStyles}
                                                                >
                                                                    <img src={quickTour.summaryImage} width="32" height="32" alt={quickTour.name} />
                                                                </IconButton>
                                                            </Stack.Item>
                                                            <Stack.Item>
                                                                <p> {quickTour.name}</p>
                                                            </Stack.Item>
                                                        </Stack>
                                                    </div>

                                                ))}

                                            </Stack>
                                            <QuickTour
                                                hidden={true}
                                                unreadOnly={false}
                                            />
                                        </div>
                                    </>
                                )}
                            </Stack.Item>
                        </CollapsibleSection>
                    </Stack.Item>
                ))}
            </Stack>
        </Styled.FAQContainer>
    );
}

const connected = withContext(FAQPage);
export { connected as FAQPage };
