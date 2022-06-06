import * as React from 'react';
import { usePageTracking } from '@micro-frontend-react/employee-experience/lib/usePageTracking';
import { usePageTitle } from '@micro-frontend-react/employee-experience/lib/usePageTitle';
import { Stack } from '@fluentui/react/lib/Stack';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { updateSelectedPage } from '../Shared/SharedComponents.actions';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getFeature, getPageLoadFeature } from '@micro-frontend-react/employee-experience/lib/UsageTelemetryHelper';
import * as Styled from './FAQStyling';
import { FAQList, ITemplate } from './FAQData';
import { Link } from '../Shared/Styles/Link';
import { format } from 'react-string-format';
import { CollapsibleSection } from '../Shared/Components/CollapsibleSection';

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
    template?: ITemplate;
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
            href={`mailto:${email}?${subject !== '' ? `subject=${encodeURIComponent(subject) || ' '}` : ''}${
                body !== '' ? `&body=${encodeURIComponent(body) || ' '}` : ''
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

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

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

    return (
        <Styled.FAQContainer windowHeight={dimensions.height} windowWidth={dimensions.width}>
            <Stack className="scroll-hidden v-scroll-auto custom-scrollbar">
                <Styled.FAQTitle>FAQ and Videos</Styled.FAQTitle>
                <br />
                {FAQList.map((item: IFAQList, index: number) => (
                    <Stack.Item styles={{ root: { marginBottom: '5px', width: '90%' } }}>
                        <CollapsibleSection
                            defaultIsExpanded={item.isExpanded}
                            titleText={item.title}
                            renderHeaderAs={item.textAsHeader}
                            styles={{ root: { flexGrow: 0, width: '100%' } }}
                        >
                            {item.email === true ? (
                                <p style={{ textAlign: 'justify' }}>
                                    {format(
                                        item.text,
                                        <Mailto
                                            thisRef={(input: { focus: () => any }) =>
                                                input && index == 0 && input.focus()
                                            }
                                            alternateText={item.template.alternateText}
                                            email={item.template.emailAddress}
                                            subject={item.template.subject}
                                            body={item.template.body}
                                        >
                                            {item.template.text}
                                        </Mailto>
                                    )}
                                </p>
                            ) : (
                                <p style={{ textAlign: 'justify' }}>{item.text}</p>
                            )}
                            <br />
                            <div
                                style={{ position: 'relative', paddingBottom: '56.25%', height: 0, overflow: 'hidden' }}
                            >
                                <iframe
                                    width={item.videoWidth}
                                    height={item.videoHeight}
                                    src={item.videoUrl}
                                    allowFullScreen={item.fullScreen}
                                    title={item.title}
                                    style={{
                                        border: 'none',
                                        position: 'absolute',
                                        top: 0,
                                        left: 0,
                                        right: 0,
                                        bottom: 0,
                                        height: '100%',
                                        maxWidth: '100%',
                                    }}
                                ></iframe>
                            </div>
                        </CollapsibleSection>
                    </Stack.Item>
                ))}
            </Stack>
        </Styled.FAQContainer>
    );
}

const connected = withContext(FAQPage);
export { connected as FAQPage };
