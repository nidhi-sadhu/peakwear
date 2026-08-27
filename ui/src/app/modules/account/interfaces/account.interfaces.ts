export interface Preference {
  shoppingFor: string;
  sizeTop: string | null;
  sizeBottom: string | null;
  marketingOptIn: boolean;
}

export interface Address {
  id: string;
  line1: string;
  line2: string | null;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
  isDefault: boolean;
}

export interface Profile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  createdAtUtc: string;
  preference: Preference | null;
  addresses: Address[];
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
}

export interface AddressRequest {
  line1: string;
  line2: string | null;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
  isDefault: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface AccountState {
  profile: Profile | null;
  addresses: Address[];
  isLoading: boolean;
  error: string | null;
  message: string | null;
}
