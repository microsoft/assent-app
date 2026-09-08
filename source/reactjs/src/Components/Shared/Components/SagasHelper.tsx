export const setHeader = (userAlias: string, tcv?: string, xcv?: string, onBehalfUserUpn?: string, onBehalfUserId?: string) => {
    return {
        ClientDevice: 'React',
        ...(userAlias && { UserAlias: `${userAlias ? userAlias : undefined}` }),
        ...(tcv && { Tcv: tcv }),
        ...(xcv && { Xcv: xcv }),
        ...(onBehalfUserUpn && { OnBehalfUserUpn: onBehalfUserUpn }),
        ...(onBehalfUserId && { OnBehalfUserId: onBehalfUserId }),
    };
};
