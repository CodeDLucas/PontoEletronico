export interface UserUpdate {
  fullName: string;
  email: string;
}

export interface ChangePassword {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface UserList {
  id: string;
  fullName: string;
  email: string;
  employeeCode: string;
  isActive: boolean;
  createdAt: string;
}