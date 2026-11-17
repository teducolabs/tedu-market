import { Banner } from '@tedu-market/ui';
import { Component, type ReactNode } from 'react';

interface ErrorBoundaryState {
  hasError: boolean;
}

/** Error boundary ở root App quản trị (conventions.md mục 4): chỉ hiển thị lỗi cho người dùng. */
export class ErrorBoundary extends Component<{ children: ReactNode }, ErrorBoundaryState> {
  override state: ErrorBoundaryState = { hasError: false };

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  override render(): ReactNode {
    if (this.state.hasError) {
      return (
        <div className="p-6">
          <Banner tone="error">Có lỗi xảy ra, vui lòng thử lại.</Banner>
        </div>
      );
    }
    return this.props.children;
  }
}
