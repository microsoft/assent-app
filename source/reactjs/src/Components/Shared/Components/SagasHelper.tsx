export const setHeader = (userAlias: string, tcv?: string, xcv?: string) => {
    return {
        ClientDevice: 'React',
        ...(userAlias && { UserAlias: `${userAlias ? userAlias : undefined}` }),
        ...(tcv && { Tcv: tcv }),
        ...(xcv && { Xcv: xcv })
    };
};
