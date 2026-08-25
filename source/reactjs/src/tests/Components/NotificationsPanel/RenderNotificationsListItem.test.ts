import { sanitizeNotificationMessage } from '../../../Components/NotificationsPanel/RenderNotificationsListItem';

describe('sanitizeNotificationMessage', () => {
    it.each([
        '<xmp><script>alert(1)</script></xmp>',
        '<a href="javascript:alert(1)">link</a>',
        '<a href="jav&#x61;script:alert(1)">link</a>',
        '<a href="javascript:alert(1)"><strong>safe',
    ])('removes unsafe notification markup from %s', (message) => {
        const sanitized = sanitizeNotificationMessage(message);

        expect(sanitized).not.toMatch(/<xmp|<script|javascript:/i);
    });

    it('preserves permitted notification markup', () => {
        const message = '<p><strong>Approved</strong> <a href="https://example.com" target="_blank">details</a></p>';

        expect(sanitizeNotificationMessage(message)).toBe(message);
    });
});
