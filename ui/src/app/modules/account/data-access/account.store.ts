import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from './account.service';
import {
  AccountState,
  Address,
  AddressRequest,
  ChangePasswordRequest,
  Preference,
  Profile,
  UpdateProfileRequest,
} from '../interfaces/account.interfaces';

const initialState: AccountState = {
  profile: null,
  addresses: [],
  isLoading: false,
  error: null,
  message: null,
};

export const AccountStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withComputed((store) => ({
    fullName: computed(() => {
      const p = store.profile();
      return p ? `${p.firstName} ${p.lastName}` : '';
    }),
    defaultAddress: computed(() => store.addresses().find((a) => a.isDefault) ?? null),
  })),

  withMethods((store, accountService = inject(AccountService)) => {
    const fail = (fallback: string) => (error: HttpErrorResponse) =>
      patchState(store, {
        error: error.error?.message ?? fallback,
        isLoading: false,
        message: null,
      });

    const addressesOk = (message: string) => (addresses: Address[]) =>
      patchState(store, { addresses, isLoading: false, error: null, message });

    return {
      load: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(() =>
            accountService.getProfile().pipe(
              tapResponse({
                next: (profile: Profile) =>
                  patchState(store, {
                    profile,
                    addresses: profile.addresses,
                    isLoading: false,
                  }),
                error: fail('Could not load your profile.'),
              }),
            ),
          ),
        ),
      ),

      updateProfile: rxMethod<UpdateProfileRequest>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null, message: null })),
          switchMap((request) =>
            accountService.updateProfile(request).pipe(
              tapResponse({
                next: (profile: Profile) =>
                  patchState(store, {
                    profile,
                    addresses: profile.addresses,
                    isLoading: false,
                    message: 'Profile updated.',
                  }),
                error: fail('Could not update your profile.'),
              }),
            ),
          ),
        ),
      ),

      updatePreferences: rxMethod<Preference>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null, message: null })),
          switchMap((request) =>
            accountService.updatePreferences(request).pipe(
              tapResponse({
                next: (preference: Preference) =>
                  patchState(store, {
                    profile: store.profile() ? { ...store.profile()!, preference } : null,
                    isLoading: false,
                    message: 'Preferences saved.',
                  }),
                error: fail('Could not save your preferences.'),
              }),
            ),
          ),
        ),
      ),

      addAddress: rxMethod<AddressRequest>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null, message: null })),
          switchMap((request) =>
            accountService.addAddress(request).pipe(
              tapResponse({
                next: addressesOk('Address added.'),
                error: fail('Could not add that address.'),
              }),
            ),
          ),
        ),
      ),

      updateAddress: rxMethod<{ id: string; request: AddressRequest }>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null, message: null })),
          switchMap(({ id, request }) =>
            accountService.updateAddress(id, request).pipe(
              tapResponse({
                next: addressesOk('Address updated.'),
                error: fail('Could not update that address.'),
              }),
            ),
          ),
        ),
      ),

      deleteAddress: rxMethod<string>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null, message: null })),
          switchMap((id) =>
            accountService.deleteAddress(id).pipe(
              tapResponse({
                next: addressesOk('Address removed.'),
                error: fail('Could not remove that address.'),
              }),
            ),
          ),
        ),
      ),

      changePassword: rxMethod<ChangePasswordRequest>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null, message: null })),
          switchMap((request) =>
            accountService.changePassword(request).pipe(
              tapResponse({
                next: () => patchState(store, { isLoading: false, message: 'Password updated.' }),
                error: fail('Could not change your password.'),
              }),
            ),
          ),
        ),
      ),

      clearMessages: () => patchState(store, { error: null, message: null }),
    };
  }),
);
