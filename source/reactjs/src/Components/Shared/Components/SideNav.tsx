import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { DirectionalHint, IButtonStyles, ICalloutProps, Icon, IconButton, TooltipHost } from '@fluentui/react';
import { CommandBarButton } from '@fluentui/react/lib/Button';
import { INavLink } from '../../../navConfig';
import { isMediumResolution, isMobileResolution } from '../../../Helpers/sharedHelpers';
import FlightingHandler from './FlightingHandler';
import { CSSProperties } from 'styled-components';
import { useHistory, useLocation } from 'react-router-dom';

export function SideNav(props: { links: INavLink[]; userRoles?: string[] }): React.ReactElement {
    const { links, userRoles } = props;
    const browserPath = useLocation().pathname;
    const [isNavExpanded, setIsNavExpanded] = React.useState(false);
    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

    const history = useHistory();
    React.useEffect(() => {
        function handleResize(): void {
            setDimensions({
                height: window.innerHeight,
                width: window.innerWidth,
            });
        }
        window.addEventListener('resize', handleResize);

        const urlSearch = new URLSearchParams(history.location.search);
        const isDelegatedRequestUrl = urlSearch.has('alias');
        const hasDelegatedRequestAlias = isDelegatedRequestUrl && urlSearch.get('alias');

        if (hasDelegatedRequestAlias) {
            document.querySelector('[aria-label="Pending Approvals"]')?.setAttribute('href', '/?alias=' + urlSearch.get('alias'));
        }

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };

    }, []);

    const isMedium = isMediumResolution(dimensions.width);
    const isMobile = isMobileResolution(dimensions.width);

    const navIconStyles: IButtonStyles = {
        root: { height: '42px', width: '48px', marginBottom: isNavExpanded ? '8px' : '6px', borderRadius: '0' },
        icon: { color: 'black' },
    };

    const navButtonStyles: IButtonStyles = {
        root: { height: '40px', marginLeft: '5px', background: '#e5e5e5' },
        icon: { color: 'black' },
        label: { textAlign: 'left', marginLeft: '6px' },
    };

    const textStyle: CSSProperties = {
        fontSize: '10px',
        color: 'black',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
        maxWidth: '40px',
    };

    const toolTipStyles: ICalloutProps = {
        isBeakVisible: false,
        directionalHint: DirectionalHint.bottomCenter,
        styles: {
            root: {
                padding: '2px',
                outline: '1px solid black',
            },
        },
    };

    const toggleNav = () => {
        setIsNavExpanded(!isNavExpanded);
    };

    const renderNavLinks = () => {
        const barElement = (linkItem: INavLink) => {
            return (
                <CommandBarButton
                    iconProps={{ iconName: linkItem.icon }}
                    text={linkItem.text}
                    href={linkItem.href}
                    styles={navButtonStyles}
                    ariaLabel={linkItem.ariaLabel}
                />
            );
        };
        const buttonElement = (linkItem: INavLink) => {
            return (
                <IconButton
                    styles={navIconStyles}
                    ariaLabel={linkItem.ariaLabel}
                    href={linkItem.href}
                    style={{
                        boxShadow: browserPath === linkItem.href ? 'inset 4px 0 0 0 #0078d4' : 'none',
                    }}
                >
                    <Stack horizontalAlign="center">
                        <Icon iconName={linkItem.icon} styles={{ root: { color: 'black', marginBottom: '5px' } }} />
                        <div style={textStyle}>{linkItem.secondaryText}</div>
                    </Stack>
                </IconButton>
            );
        };
        return links
            .filter((linkItem: INavLink) => {
                if (!linkItem.requiresRole) return true;
                return userRoles?.some((role) => linkItem.requiresRole.includes(role));
            })
            .map((linkItem: INavLink) => {
            const navElement = (
                <TooltipHost content={linkItem.ariaLabel} calloutProps={toolTipStyles}>
                    {isNavExpanded ? barElement(linkItem) : buttonElement(linkItem)}
                </TooltipHost>
            );
            const flightingCheckedComponent = linkItem.flightingName ? (
                <FlightingHandler featureName={linkItem.flightingName}>{navElement}</FlightingHandler>
            ) : (
                navElement
            );
            return flightingCheckedComponent;
        });
    };

    // 1026 x 868
    return (
        <Stack
            className="navContainer"
            role="navigation"
            styles={{
                root: {
                    width: isNavExpanded ? '176px' : '48px',
                    zIndex: isNavExpanded ? '1' : 'auto',
                    height: isMedium ? (isNavExpanded ? '100vh' : '48px') : '100vh',
                    selectors: {
                        '@media only screen and (min-device-width: 1023px) and (max-width: 639px)': {
                            height: '24px !important',
                        },
                    },
                },
            }}
        >
            <TooltipHost content={(isNavExpanded ? 'Collapse' : 'Expand') + ' Navigation'} calloutProps={toolTipStyles}>
                <IconButton
                    iconProps={{ iconName: 'GlobalNavButton' }}
                    styles={navIconStyles}
                    onClick={toggleNav}
                    ariaLabel={(isNavExpanded ? 'Collapse' : 'Expand') + ' Navigation'}
                />
            </TooltipHost>

            {(!isMedium || isNavExpanded) && <Stack>{renderNavLinks()}</Stack>}
        </Stack>
    );
}
