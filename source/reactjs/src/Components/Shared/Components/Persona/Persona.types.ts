import { IPersonaProps as IFabricPersonaProps, PersonaInitialsColor, PersonaSize } from '@fluentui/react/lib/Persona';
import { IGraphClient } from '@micro-frontend-react/employee-experience/lib/IGraphClient';

export interface IPersonaProps extends IFabricPersonaProps {
    emailAlias?: string;
    graphClient?: IGraphClient;
}

export { PersonaInitialsColor, PersonaSize };
