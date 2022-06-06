import { mapDate } from '../../../Components/Shared/Components/DateFormatting';

describe('Date Formatting', function() {
    it('should render format date', function() {
      let date: string = "2021-01-11";
      expect(mapDate(date)).toBe('Jan 11, 2021');   
  });
});