import * as React from 'react';
import { Persona as FabricPersona } from '@fluentui/react/lib/Persona';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { getImageURLForAlias, getIsLoadingSubmitterImages, isAliasInSubmitters } from '../SharedComponents.selectors';
import { Persona } from '../Components/Persona';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

export const SubmitterPersona = (props: { emailAlias: string; size: number, imageAlt?: string }): JSX.Element => {
    const { useSelector } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { emailAlias } = props;
    const isLoadingSubmitterImages = useSelector(getIsLoadingSubmitterImages);
    const savedURL = useSelector((state: any) => getImageURLForAlias(state, emailAlias));
    const isAliasInSubmittersState = useSelector((state: any) => isAliasInSubmitters(state, emailAlias));
    if (isLoadingSubmitterImages) {
        return React.createElement(FabricPersona, Object.assign({}, props));
    } else {
        if (isAliasInSubmittersState) {
            return React.createElement(FabricPersona, Object.assign({}, props, { imageUrl: savedURL }));
        } else {
            return <Persona emailAlias={props.emailAlias + `${__UPN_SUFFIX__}`} size={props.size} />;
        }
    }
};
