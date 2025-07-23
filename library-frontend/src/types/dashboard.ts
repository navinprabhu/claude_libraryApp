export interface DashboardStats {
  totalBooks: number;
  availableBooks: number;
  booksBorrowed: number;
  totalMembers: number;
  activeMembers: number;
  overdueBooks: number;
}

export interface Transaction {
  id: number;
  memberId: number;
  memberName: string;
  bookId: number;
  bookTitle: string;
  action: string;
  timestamp: string;
}

export interface BookCategory {
  category: string;
  totalBooks: number;
  availableBooks: number;
  borrowedBooks: number;
}

export interface TopMember {
  memberId: number;
  memberName: string;
  email: string;
  totalBorrowings: number;
  currentBorrowings: number;
}

export interface Alert {
  id: number;
  type: string;
  message: string;
  severity: 'info' | 'warning' | 'error' | 'success';
  timestamp: string;
}

export interface StatCard {
  title: string;
  value: number;
  icon: string;
  color: string;
  trend?: {
    value: number;
    direction: 'up' | 'down';
  };
}