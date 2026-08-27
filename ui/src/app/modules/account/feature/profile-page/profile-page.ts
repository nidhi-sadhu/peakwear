import { Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { AccountStore } from '@modules/account/data-access/account.store';
import { Address } from '@modules/account/interfaces/account.interfaces';

@Component({
  selector: 'app-profile-page',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
  ],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePage {
  private fb = inject(FormBuilder);
  readonly store = inject(AccountStore);

  editingAddressId = signal<string | null>(null);
  showAddressForm = signal(false);

  readonly shoppingForOptions = ['Men', 'Women', 'Both'];
  readonly sizes = ['XS', 'S', 'M', 'L', 'XL'];

  profileForm = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: ['', Validators.pattern(/^[\d\s()+-]{7,20}$/)],
  });

  preferenceForm = this.fb.nonNullable.group({
    shoppingFor: ['Both', Validators.required],
    sizeTop: [''],
    sizeBottom: [''],
    marketingOptIn: [false],
  });

  addressForm = this.fb.nonNullable.group({
    line1: ['', Validators.required],
    line2: [''],
    city: ['', Validators.required],
    state: ['', Validators.required],
    postalCode: ['', Validators.required],
    countryCode: ['US', Validators.required],
    isDefault: [false],
  });

  passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor() {
    this.store.load();

    // Fill the forms once the profile arrives
    effect(() => {
      const profile = this.store.profile();
      if (!profile) return;

      this.profileForm.patchValue({
        firstName: profile.firstName,
        lastName: profile.lastName,
        phoneNumber: profile.phoneNumber ?? '',
      });

      if (profile.preference) {
        this.preferenceForm.patchValue({
          shoppingFor: profile.preference.shoppingFor,
          sizeTop: profile.preference.sizeTop ?? '',
          sizeBottom: profile.preference.sizeBottom ?? '',
          marketingOptIn: profile.preference.marketingOptIn,
        });
      }
    });

    effect(() => {
      if (this.store.message() === 'Password updated.') {
        this.passwordForm.reset();
        this.passwordForm.markAsUntouched();
        this.passwordForm.markAsPristine();
      }
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    const value = this.profileForm.getRawValue();
    this.store.updateProfile({
      firstName: value.firstName,
      lastName: value.lastName,
      phoneNumber: value.phoneNumber || null,
    });
  }

  savePreferences(): void {
    const value = this.preferenceForm.getRawValue();
    this.store.updatePreferences({
      shoppingFor: value.shoppingFor,
      sizeTop: value.sizeTop || null,
      sizeBottom: value.sizeBottom || null,
      marketingOptIn: value.marketingOptIn,
    });
  }

  newAddress(): void {
    this.editingAddressId.set(null);
    this.addressForm.reset({ countryCode: 'US', isDefault: false });
    this.showAddressForm.set(true);
  }

  editAddress(address: Address): void {
    this.editingAddressId.set(address.id);
    this.addressForm.patchValue({
      line1: address.line1,
      line2: address.line2 ?? '',
      city: address.city,
      state: address.state,
      postalCode: address.postalCode,
      countryCode: address.countryCode,
      isDefault: address.isDefault,
    });
    this.showAddressForm.set(true);
  }

  saveAddress(): void {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }

    const value = this.addressForm.getRawValue();
    const request = { ...value, line2: value.line2 || null };
    const id = this.editingAddressId();

    if (id) {
      this.store.updateAddress({ id, request });
    } else {
      this.store.addAddress(request);
    }

    this.showAddressForm.set(false);
  }

  cancelAddress(): void {
    this.showAddressForm.set(false);
    this.editingAddressId.set(null);
  }

  changePassword(formDirective: FormGroupDirective): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    this.store.changePassword(this.passwordForm.getRawValue());
    formDirective.resetForm();
  }
}
