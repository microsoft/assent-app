import { stockImage } from '../../../../Helpers/stockImage';

describe('stockImage', () => {
    it('should stockImage have value', () => {
        const properties = stockImage;
        expect(properties).toBeTruthy;
    });
});
