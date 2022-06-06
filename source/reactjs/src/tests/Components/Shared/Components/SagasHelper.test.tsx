import { setHeader } from '../../../../Components/Shared/Components/SagasHelper';

describe('SagasHelper', () => {
    it('should Set header for user alias', () => {
        let userAlias: string = 'test';
        const wrapper = setHeader('test');
        expect(wrapper.UserAlias).toEqual(userAlias);
        expect(wrapper.ClientDevice).toEqual('React');
    });

    it('should Set header without user alias', () => {
        let userAlias: undefined;
        const wrapper = setHeader(userAlias);
        expect(wrapper.UserAlias).toHaveProperty;
        expect(wrapper.ClientDevice).toEqual('React');
    });
});
