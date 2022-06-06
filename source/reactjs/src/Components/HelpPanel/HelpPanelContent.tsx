import * as React from 'react';
import * as Styled from './HelpPanel.styled';
import * as HelpPageData from './HelpPanelData';
import { Link } from '../Shared/Styles/Link';
import { SearchBox } from '@fluentui/react/lib/SearchBox';
import { helpPanelReducerName, helpPanelReducer, helpPanelInitialState } from './HelpPanel.reducer';
import { helpPanelSagas } from './HelpPanel.sagas';
import { Reducer } from 'redux';
import { IHelpPanelState } from './HelpPanel.types';
import { requestAboutInfo } from './HelpPanel.actions';
import { useDispatch, useSelector } from 'react-redux';
import { usePersistentReducer } from '../Shared/Components/PersistentReducer';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import {
    sharedComponentsPersistentReducerName,
    sharedComponentsPersistentReducer
} from '../Shared/SharedComponents.persistent-reducer';
import { getSupportEmailId } from './HelpPanel.selectors';

function HelpPanelContent(): React.ReactElement {
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);

    const [onMainPage, setOnMainPage] = React.useState(true);
    const [showSearch, setShowSearch] = React.useState(false);

    const supportEmailId = useSelector(getSupportEmailId);

    const dispatch = useDispatch();

    React.useEffect(() => {
        dispatch(requestAboutInfo());
    }, [dispatch]);

    function Mailto({
        thisRef,
        alternateText,
        getSupportEmail,
        email,
        subject,
        body,
        ...props
    }: {
        [x: string]: any;
        thisRef: React.Ref<HTMLAnchorElement>;
        alternateText: string;
        getSupportEmail: boolean;
        email: string;
        subject: string;
        body: string;
    }): JSX.Element {
        if (getSupportEmail) {
            email = supportEmailId;
        }
        return (
            <Link
                ref={thisRef}
                title={alternateText ? alternateText : null}
                href={`mailto:${email}?subject=${encodeURIComponent(subject) || ' '}&body=${encodeURIComponent(body) ||
                    ' '}`}
            >
                {props.children}
            </Link>
        );
    }

    function getMainContent(): JSX.Element {
        return (
            <div>
                <Styled.Subheader>Overview</Styled.Subheader>
                <Styled.Body>{HelpPageData.HelpMainPageContent}</Styled.Body>
                <Styled.Divider />
                <Styled.BaseCTitle>Quick Links</Styled.BaseCTitle>
                <ul style={{ listStyle: 'none', padding: '0px' }}>
                    {HelpPageData.QuickLinks.map((link, index) =>
                        link.email === true ? (
                            <li>
                                <Mailto
                                    thisRef={(input: { focus: () => any }) => input && index == 0 && input.focus()}
                                    alternateText={link.alternateText}
                                    getSupportEmail={link.getSupportEmail}
                                    email={link.emailAddress}
                                    subject={link.subject}
                                    body={link.body}
                                >
                                    <Styled.QuickLink>{link.text}</Styled.QuickLink>
                                </Mailto>
                            </li>
                        ) : (
                            <li>
                                <Link
                                    title={link.alternateText ? link.alternateText : null}
                                    ref={input => input && index == 0 && input.focus()}
                                    href={link.link}
                                    target={link.target ? link.target: null}
                                >
                                    <Styled.QuickLink>{link.text}</Styled.QuickLink>
                                </Link>
                            </li>
                        )
                    )}
                </ul>
                {/* <Styled.BaseCTitle>
                    New to MSApprovals?
                </Styled.BaseCTitle> */}
                {/* take quick tour -> for later */}
                {/* <Link onClick={() => dispatch(toggleTeachingBubbleVisibility())}> Take a Quick Tour </Link> */}
            </div>
        );
    }

    return (
        <div>
            {showSearch ? (
                <SearchBox
                    placeholder="Search help"
                    onSearch={newValue => console.log('value is ' + newValue)}
                    onFocus={() => console.log('onFocus called')}
                    onBlur={() => console.log('onBlur called')}
                    onChange={() => console.log('onChange called')}
                />
            ) : (
                <div></div>
            )}
            {onMainPage ? getMainContent() : <div>Other Page</div>}
        </div>
    );
}

export default HelpPanelContent;
