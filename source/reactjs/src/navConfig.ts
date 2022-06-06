export interface INavLink {
    key: string;
    text: string;
    icon: string;
    href: string;
    ariaLabel: string;
}

export const navConfig: INavLink[] = [
    {
        key: 'pendingApprovals',
        text: 'Pending Approvals',
        icon: 'View',
        href: '/',
        ariaLabel: 'Pending Approvals',
    },
    {
        key: 'history',
        text: 'History',
        icon: 'History',
        href: '/history',
        ariaLabel: 'History',
    },
];
