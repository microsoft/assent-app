import * as React from 'react';
import { render, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { RenderListItem } from '../../../Components/NotificationsPanel/RenderNotificationsListItem';

describe('RenderNotificationsListItem', () => {
    const mockItem = {
        itemKey: '1',
        displayStatus: 'new',
        status: 'unread',
        messageBodyText: 'This is test text',
        subjectIcon: 'ReportWarning',
        subjectHeader: 'Testing Notification One'
    };

    it('should expand/collapse when clicked', () => {
        const { getByText, queryByText } = render(<RenderListItem item={mockItem} status="unread" />);
        
        // The notification should initially be expanded (for unread status)
        expect(queryByText('This is test text')).toBeInTheDocument();
        
        // Click to collapse
        fireEvent.click(getByText('Testing Notification One'));
        expect(queryByText('This is test text')).not.toBeInTheDocument();
        
        // Click to expand again
        fireEvent.click(getByText('Testing Notification One'));
        expect(queryByText('This is test text')).toBeInTheDocument();
    });

    it('should expand/collapse when pressing Enter key', () => {
        const { getByRole, queryByText } = render(<RenderListItem item={mockItem} status="unread" />);
        const notificationItem = getByRole('button');

        // The notification should initially be expanded (for unread status)
        expect(queryByText('This is test text')).toBeInTheDocument();
        
        // Press Enter to collapse
        fireEvent.keyDown(notificationItem, { key: 'Enter', code: 'Enter' });
        expect(queryByText('This is test text')).not.toBeInTheDocument();
        
        // Press Enter to expand again
        fireEvent.keyDown(notificationItem, { key: 'Enter', code: 'Enter' });
        expect(queryByText('This is test text')).toBeInTheDocument();
    });

    it('should expand/collapse when pressing Space key', () => {
        const { getByRole, queryByText } = render(<RenderListItem item={mockItem} status="unread" />);
        const notificationItem = getByRole('button');

        // The notification should initially be expanded (for unread status)
        expect(queryByText('This is test text')).toBeInTheDocument();
        
        // Press Space to collapse
        fireEvent.keyDown(notificationItem, { key: ' ', code: 'Space' });
        expect(queryByText('This is test text')).not.toBeInTheDocument();
        
        // Press Space to expand again
        fireEvent.keyDown(notificationItem, { key: ' ', code: 'Space' });
        expect(queryByText('This is test text')).toBeInTheDocument();
    });

    it('should have proper ARIA attributes', () => {
        const { getByRole } = render(<RenderListItem item={mockItem} status="unread" />);
        const notificationItem = getByRole('button');

        // Check for ARIA attributes
        expect(notificationItem).toHaveAttribute('aria-expanded', 'true');
        expect(notificationItem).toHaveAttribute('aria-label', expect.stringContaining('Testing Notification One notification'));
        expect(notificationItem).toHaveAttribute('aria-label', expect.stringContaining('Collapse notification details'));
        
        // Collapse the notification
        fireEvent.click(notificationItem);
        
        // Check that aria-expanded is now false
        expect(notificationItem).toHaveAttribute('aria-expanded', 'false');
        expect(notificationItem).toHaveAttribute('aria-label', expect.stringContaining('Expand notification details'));
    });

    it('should handle read notifications properly', () => {
        const { getByRole, queryByText } = render(<RenderListItem item={mockItem} status="read" />);
        const notificationItem = getByRole('button');

        // The notification should initially be collapsed for read status
        expect(queryByText('This is test text')).not.toBeInTheDocument();
        
        // Press Space to expand
        fireEvent.keyDown(notificationItem, { key: ' ', code: 'Space' });
        expect(queryByText('This is test text')).toBeInTheDocument();
    });
});
