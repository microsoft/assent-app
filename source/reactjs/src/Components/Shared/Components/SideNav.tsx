import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { DirectionalHint, IButtonStyles, ICalloutProps, IconButton, TooltipHost } from '@fluentui/react';
import { CommandBarButton } from '@fluentui/react/lib/Button';
import { INavLink } from '../../../navConfig';
import { isMediumResolution, isMobileResolution } from '../../../Helpers/sharedHelpers';

export function SideNav(props: { links: INavLink[] }): React.ReactElement {
    const { links } = props;
    const [isNavExpanded, setIsNavExpanded] = React.useState(false);

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

    React.useEffect(() => {
        function handleResize(): void {
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

    const isMedium = isMediumResolution(dimensions.width);
    const isMobile = isMobileResolution(dimensions.width);

    const navIconStyles: IButtonStyles = {
        root: { height: '40px', marginLeft: '5px', marginBottom: isNavExpanded ? '8px' : '0px' },
        icon: { color: 'black' },
    };

    const navButtonStyles: IButtonStyles = {
        root: { height: '40px', marginLeft: '5px', background: '#e5e5e5' },
        icon: { color: 'black' },
        label: { textAlign: 'left', marginLeft: '6px' },
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
    }

    const toggleNav = () => {
        setIsNavExpanded(!isNavExpanded);
    };

    const renderNavLinks = () => {
        return isNavExpanded
            ? links.map((linkItem: INavLink) => (
                <TooltipHost
                    content={linkItem.ariaLabel}
                    calloutProps={toolTipStyles}
                >
                    <CommandBarButton
                        iconProps={{ iconName: linkItem.icon }}
                        text={linkItem.text}
                        href={linkItem.href}
                        styles={navButtonStyles}
                    />
                </TooltipHost>
            ))
            : links.map((linkItem: INavLink) => (
                <TooltipHost
                    content={linkItem.ariaLabel}
                    calloutProps={toolTipStyles}
                >
                    <IconButton
                        iconProps={{ iconName: linkItem.icon }}
                        styles={navIconStyles}
                        href={linkItem.href}
                    />
                </TooltipHost>
            ));
    };

    // 1026 x 868
    return (
        <Stack
            className="navContainer"
            styles={{
                root: {
                    width: isNavExpanded ? '176px' : '48px',
                    zIndex: isNavExpanded ? '1' : 'auto',
                    height: isMedium ? (isNavExpanded ? '100vh' : isMobile ? '24px' : '48px') : '100vh',
                },
            }}
        >
            <TooltipHost
                content={(isNavExpanded ? 'Collapse' : 'Expand') + ' Navigation'}
                calloutProps={toolTipStyles}
            >
                <IconButton
                    iconProps={{ iconName: 'GlobalNavButton' }}
                    styles={navIconStyles}
                    onClick={toggleNav}
                />
            </TooltipHost>

            {(!isMedium || isNavExpanded) && <Stack>{renderNavLinks()}</Stack>}
        </Stack>
    );
}
